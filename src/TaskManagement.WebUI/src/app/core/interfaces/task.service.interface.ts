import { InjectionToken, Signal } from '@angular/core';
import { TaskItem, TaskPayload } from '../models';

export interface ITaskService {
  readonly tasks: Signal<TaskItem[]>;
  readonly loading: Signal<boolean>;
  readonly error: Signal<string | null>;
  loadTasks(): Promise<TaskItem[]>;
  createTask(payload: TaskPayload): Promise<TaskItem>;
  updateTask(id: string, payload: TaskPayload): Promise<TaskItem>;
  deleteTask(id: string): Promise<boolean>;
  clearError(): void;
}

export const TASK_SERVICE = new InjectionToken<ITaskService>('TASK_SERVICE');
