import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { TASK_SERVICE } from '../../../core/interfaces/task.service.interface';
import { TASK_STATUSES, TaskPayload, TaskStatus } from '../../../core/models';

export interface TaskFormDialogData {
  mode: 'create' | 'edit';
  task?: TaskPayload;
  taskId?: string;
}

@Component({
  selector: 'app-task-form-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule
  ],
  templateUrl: './task-form-dialog.component.html',
  styleUrl: './task-form-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TaskFormDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<TaskFormDialogComponent, boolean>);
  private readonly taskService = inject(TASK_SERVICE);
  private readonly fb = inject(FormBuilder);
  protected readonly data = inject<TaskFormDialogData>(MAT_DIALOG_DATA);
  protected readonly statuses = TASK_STATUSES;

  protected readonly form = this.fb.nonNullable.group({
    title: [
      this.data.task?.title ?? '',
      [Validators.required, Validators.maxLength(100)]
    ],
    description: [this.data.task?.description ?? ''],
    status: this.fb.nonNullable.control<TaskStatus>(this.data.task?.status ?? 'Pending', {
      validators: [Validators.required]
    }),
    dueDate: [
      this.data.task?.dueDate ?? this.defaultDueDate(),
      [Validators.required]
    ]
  });
  protected readonly isSaving = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected async save(): Promise<void> {
    this.form.markAllAsTouched();

    if (this.form.invalid || this.isSaving()) {
      return;
    }

    const value = this.form.getRawValue();
    const payload: TaskPayload = {
      title: value.title.trim(),
      description: value.description.trim(),
      status: value.status,
      dueDate: new Date(`${value.dueDate}T12:00:00`).toISOString()
    };

    if (!payload.title) {
      this.form.controls.title.setErrors({ required: true });
      return;
    }

    this.errorMessage.set(null);
    this.isSaving.set(true);
    this.dialogRef.disableClose = true;
    this.form.disable();

    try {
      if (this.data.mode === 'create') {
        await this.taskService.createTask(payload);
      } else {
        await this.taskService.updateTask(this.data.taskId!, payload);
      }
      this.dialogRef.close(true);
    } catch (error) {
      this.errorMessage.set(error instanceof Error ? error.message : 'Request failed');
      this.form.enable();
    } finally {
      this.isSaving.set(false);
      this.dialogRef.disableClose = false;
    }
  }

  protected cancel(): void {
    if (this.isSaving()) {
      return;
    }
    this.dialogRef.close(false);
  }

  private defaultDueDate(): string {
    return new Date().toISOString().slice(0, 10);
  }
}
