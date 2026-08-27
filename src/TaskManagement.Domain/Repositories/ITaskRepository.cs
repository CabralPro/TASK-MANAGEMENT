using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaskManagement.Domain.Abstractions;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Domain.Repositories;

public interface ITaskRepository : IRepository<TaskItem>
{
    Task<IEnumerable<TaskItem>> GetAllByUserId(Guid userId, CancellationToken cancellationToken = default);
    Task<TaskItem> GetByIdForUser(Guid id, Guid userId, CancellationToken cancellationToken = default);
    void Add(TaskItem taskItem);
    void Update(TaskItem taskItem);
    void Delete(TaskItem taskItem);
}
