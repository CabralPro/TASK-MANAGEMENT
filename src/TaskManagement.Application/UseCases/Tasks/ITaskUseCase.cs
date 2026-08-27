using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaskManagement.Application.DTOs.Requests;
using TaskManagement.Application.DTOs.Responses;

namespace TaskManagement.Application.UseCases.Tasks;

public interface ITaskUseCase
{
    Task<TaskDto> GetById(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskDto>> GetAll(Guid userId, CancellationToken cancellationToken = default);
    Task<TaskDto> Create(CreateTaskRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<TaskDto> Update(Guid id, UpdateTaskRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task Delete(Guid id, Guid userId, CancellationToken cancellationToken = default);
}
