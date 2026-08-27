using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TaskManagement.Domain.Abstractions;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Repositories;
using TaskManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace TaskManagement.Infrastructure.Repositories;

public class TaskRepository(TaskManagementDbContext context) : ITaskRepository
{
    public IUnitOfWork UnitOfWork => context;

    public async Task<IEnumerable<TaskItem>> GetAllByUserId(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await context.Tasks.AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderBy(t => t.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<TaskItem> GetByIdForUser(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await context.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, cancellationToken);
    }

    public void Add(TaskItem taskItem)
    {
        context.Tasks.Add(taskItem);
    }

    public void Update(TaskItem taskItem)
    {
        context.Tasks.Update(taskItem);
    }

    public void Delete(TaskItem taskItem)
    {
        context.Tasks.Remove(taskItem);
    }

    public void Dispose()
    {
        context?.Dispose();
        GC.SuppressFinalize(this);
    }
}
