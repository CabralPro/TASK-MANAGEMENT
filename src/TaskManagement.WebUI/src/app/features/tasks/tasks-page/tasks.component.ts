import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  inject,
  signal
} from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { firstValueFrom } from 'rxjs';
import { TASK_SERVICE } from '../../../core/interfaces/task.service.interface';
import { TaskItem, TaskStatus } from '../../../core/models';
import { ConfirmDialogComponent } from '../../../shared/ui/confirm-dialog/confirm-dialog.component';
import { TaskFormDialogComponent, TaskFormDialogData } from '../task-form-dialog/task-form-dialog.component';

@Component({
  selector: 'app-tasks',
  standalone: true,
  imports: [
    DatePipe,
    FormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatSnackBarModule,
    MatTableModule
  ],
  templateUrl: './tasks.component.html',
  styleUrl: './tasks.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TasksComponent implements OnInit {
  private readonly taskService = inject(TASK_SERVICE);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly tasks = this.taskService.tasks;
  protected readonly loading = this.taskService.loading;
  protected readonly error = this.taskService.error;
  protected readonly statusFilter = signal<TaskStatus | 'All'>('All');
  protected readonly deletingId = signal<string | null>(null);
  protected readonly displayedColumns = ['title', 'status', 'dueDate', 'actions'];

  ngOnInit(): void {
    void this.refresh();
  }

  protected async refresh(): Promise<void> {
    if (this.loading() || this.deletingId()) {
      return;
    }

    try {
      await this.taskService.loadTasks();
    } catch {
      // TaskService already records the error signal for the template.
    }
  }

  protected filteredTasks(): TaskItem[] {
    const filter = this.statusFilter();
    const items = this.tasks();
    if (filter === 'All') {
      return items;
    }
    return items.filter((task) => task.status === filter);
  }

  protected openCreate(): void {
    if (this.loading() || this.deletingId()) {
      return;
    }
    void this.openDialog({ mode: 'create' });
  }

  protected openEdit(task: TaskItem): void {
    if (this.loading() || this.deletingId()) {
      return;
    }

    void this.openDialog({
      mode: 'edit',
      task: {
        title: task.title,
        description: task.description,
        status: task.status,
        dueDate: task.dueDate.slice(0, 10)
      },
      taskId: task.id
    });
  }

  protected async deleteTask(task: TaskItem): Promise<void> {
    if (this.loading() || this.deletingId()) {
      return;
    }

    const confirmed = await firstValueFrom(
      this.dialog
        .open(ConfirmDialogComponent, {
          width: '400px',
          data: {
            title: 'Delete task',
            message: `Delete "${task.title}"? This cannot be undone.`,
            confirmText: 'Delete'
          }
        })
        .afterClosed()
    );

    if (!confirmed) {
      return;
    }

    this.deletingId.set(task.id);

    try {
      await this.taskService.deleteTask(task.id);
      this.snackBar.open('Task deleted', 'Dismiss', { duration: 3000 });
    } catch (error) {
      this.showError(error instanceof Error ? error.message : 'Delete failed');
    } finally {
      this.deletingId.set(null);
    }
  }

  protected statusClass(status: TaskStatus): string {
    return `status-${status.toLowerCase()}`;
  }

  private async openDialog(data: TaskFormDialogData): Promise<void> {
    const saved = await firstValueFrom(
      this.dialog
        .open(TaskFormDialogComponent, {
          width: '480px',
          data,
          disableClose: false
        })
        .afterClosed()
    );

    if (!saved) {
      return;
    }

    this.snackBar.open(
      data.mode === 'create' ? 'Task created' : 'Task updated',
      'Dismiss',
      { duration: 3000 }
    );
  }

  private showError(message: string): void {
    this.snackBar.open(message, 'Dismiss', { duration: 5000 });
  }
}
