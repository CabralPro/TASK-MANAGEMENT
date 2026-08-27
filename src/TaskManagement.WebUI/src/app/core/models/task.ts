export type TaskStatus = 'Pending' | 'InProgress' | 'Completed';

export interface TaskItem {
  id: string;
  title: string;
  description: string;
  status: TaskStatus;
  dueDate: string;
  userId: string;
}

export interface TaskPayload {
  title: string;
  description: string;
  status: TaskStatus;
  dueDate: string;
}

export const TASK_STATUSES: TaskStatus[] = ['Pending', 'InProgress', 'Completed'];
