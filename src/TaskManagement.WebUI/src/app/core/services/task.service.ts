import { Injectable, inject, signal } from '@angular/core';
import { ITaskService } from '../interfaces/task.service.interface';
import { TaskItem, TaskPayload } from '../models';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class TaskService implements ITaskService {
  private readonly api = inject(ApiService);

  private readonly tasksSignal = signal<TaskItem[]>([]);
  private readonly loadingSignal = signal(false);
  private readonly errorSignal = signal<string | null>(null);

  readonly tasks = this.tasksSignal.asReadonly();
  readonly loading = this.loadingSignal.asReadonly();
  readonly error = this.errorSignal.asReadonly();

  async loadTasks(): Promise<TaskItem[]> {
    this.loadingSignal.set(true);
    this.errorSignal.set(null);

    try {
      const response = await this.api.get<TaskItem[]>('/api/v1/tasks');
      this.ensureSuccess(response);
      const tasks = response.data ?? [];
      this.tasksSignal.set(tasks);
      return tasks;
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Request failed';
      this.errorSignal.set(message);
      throw error;
    } finally {
      this.loadingSignal.set(false);
    }
  }

  async createTask(payload: TaskPayload): Promise<TaskItem> {
    const response = await this.api.post<TaskItem>('/api/v1/tasks', payload);
    this.ensureSuccess(response);
    this.tasksSignal.update((current) => [...current, response.data]);
    return response.data;
  }

  async updateTask(id: string, payload: TaskPayload): Promise<TaskItem> {
    const response = await this.api.put<TaskItem>(`/api/v1/tasks/${id}`, { ...payload, id });
    this.ensureSuccess(response);
    this.tasksSignal.update((current) =>
      current.map((task) => (task.id === id ? response.data : task))
    );
    return response.data;
  }

  async deleteTask(id: string): Promise<boolean> {
    const response = await this.api.delete<boolean>(`/api/v1/tasks/${id}`);
    this.ensureSuccess(response);
    this.tasksSignal.update((current) => current.filter((task) => task.id !== id));
    return response.data;
  }

  clearError(): void {
    this.errorSignal.set(null);
  }

  private ensureSuccess<T>(response: { success: boolean; message?: string | null }): void {
    if (!response.success) {
      throw new Error(response.message ?? 'Request failed');
    }
  }
}
