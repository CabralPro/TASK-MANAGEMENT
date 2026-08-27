import { TestBed } from '@angular/core/testing';
import { ApiService } from './api.service';
import { TaskService } from './task.service';
import { TaskItem } from '../models';

describe('TaskService', () => {
  let service: TaskService;
  let api: {
    get: ReturnType<typeof vi.fn>;
    post: ReturnType<typeof vi.fn>;
    put: ReturnType<typeof vi.fn>;
    delete: ReturnType<typeof vi.fn>;
  };

  const sample: TaskItem = {
    id: 't1',
    title: 'Prep interview',
    description: 'Notes',
    status: 'Pending',
    dueDate: '2026-09-01T12:00:00Z',
    userId: 'u1'
  };

  beforeEach(() => {
    api = {
      get: vi.fn(),
      post: vi.fn(),
      put: vi.fn(),
      delete: vi.fn()
    };

    TestBed.configureTestingModule({
      providers: [TaskService, { provide: ApiService, useValue: api }]
    });

    service = TestBed.inject(TaskService);
  });

  it('loadTasks stores list on success', async () => {
    api.get.mockResolvedValue({ success: true, data: [sample], message: null, errors: [] });

    await service.loadTasks();

    expect(service.tasks()).toEqual([sample]);
    expect(service.loading()).toBe(false);
    expect(service.error()).toBeNull();
  });

  it('loadTasks treats null data as empty list', async () => {
    api.get.mockResolvedValue({ success: true, data: null, message: null, errors: [] });

    await service.loadTasks();

    expect(service.tasks()).toEqual([]);
  });

  it('createTask appends to local list', async () => {
    api.post.mockResolvedValue({ success: true, data: sample, message: null, errors: [] });

    await service.createTask({
      title: sample.title,
      description: sample.description,
      status: sample.status,
      dueDate: sample.dueDate
    });

    expect(service.tasks()).toEqual([sample]);
  });

  it('updateTask replaces matching item and leaves others untouched', async () => {
    const other = { ...sample, id: 't2', title: 'Other' };
    api.get.mockResolvedValue({ success: true, data: [sample, other], message: null, errors: [] });
    await service.loadTasks();

    const updated = { ...sample, title: 'Updated', status: 'Completed' as const };
    api.put.mockResolvedValue({ success: true, data: updated, message: null, errors: [] });

    await service.updateTask('t1', {
      title: updated.title,
      description: updated.description,
      status: updated.status,
      dueDate: updated.dueDate
    });

    expect(service.tasks()).toEqual([updated, other]);
  });

  it('deleteTask removes item from local list', async () => {
    api.get.mockResolvedValue({ success: true, data: [sample], message: null, errors: [] });
    await service.loadTasks();

    api.delete.mockResolvedValue({ success: true, data: true, message: null, errors: [] });
    await service.deleteTask('t1');

    expect(service.tasks()).toEqual([]);
  });

  it('loadTasks sets error when API fails', async () => {
    api.get.mockResolvedValue({
      success: false,
      data: null,
      message: 'Unauthorized',
      errors: []
    });

    await expect(service.loadTasks()).rejects.toThrow('Unauthorized');
    expect(service.error()).toBe('Unauthorized');
    expect(service.loading()).toBe(false);
  });

  it('loadTasks uses fallback message for non-Error failures', async () => {
    api.get.mockRejectedValue('boom');

    await expect(service.loadTasks()).rejects.toBe('boom');
    expect(service.error()).toBe('Request failed');
  });

  it('clearError resets error signal', async () => {
    api.get.mockResolvedValue({
      success: false,
      data: null,
      message: 'Unauthorized',
      errors: []
    });

    await expect(service.loadTasks()).rejects.toThrow();
    service.clearError();
    expect(service.error()).toBeNull();
  });

  it('createTask throws Request failed when message is missing', async () => {
    api.post.mockResolvedValue({
      success: false,
      data: null,
      message: null,
      errors: []
    });

    await expect(
      service.createTask({
        title: sample.title,
        description: sample.description,
        status: sample.status,
        dueDate: sample.dueDate
      })
    ).rejects.toThrow('Request failed');
  });
});
