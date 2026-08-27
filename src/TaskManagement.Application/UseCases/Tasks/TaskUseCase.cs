using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using TaskManagement.Application.DTOs.Requests;
using TaskManagement.Application.DTOs.Responses;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Exceptions;
using TaskManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace TaskManagement.Application.UseCases.Tasks;

public class TaskUseCase(
    ITaskRepository taskRepository,
    IMapper mapper,
    ILogger<TaskUseCase> logger) : ITaskUseCase
{
    public async Task<TaskDto> GetById(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var task = await taskRepository.GetByIdForUser(id, userId, cancellationToken)
            ?? throw new NotFoundException($"Task '{id}' was not found.");

        return mapper.Map<TaskDto>(task);
    }

    public async Task<IEnumerable<TaskDto>> GetAll(Guid userId, CancellationToken cancellationToken = default)
    {
        var tasks = await taskRepository.GetAllByUserId(userId, cancellationToken);
        return mapper.Map<IEnumerable<TaskDto>>(tasks);
    }

    public async Task<TaskDto> Create(
        CreateTaskRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var task = new TaskItem(
            request.Title.Trim(),
            request.Description?.Trim() ?? string.Empty,
            request.Status,
            request.DueDate,
            userId);

        taskRepository.Add(task);
        await taskRepository.UnitOfWork.Commit(cancellationToken);

        logger.LogInformation("Task created {TaskId} for user {UserId}", task.Id, userId);
        return mapper.Map<TaskDto>(task);
    }

    public async Task<TaskDto> Update(
        Guid id,
        UpdateTaskRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var task = await taskRepository.GetByIdForUser(id, userId, cancellationToken)
            ?? throw new NotFoundException($"Task '{id}' was not found.");

        task.Update(
            request.Title.Trim(),
            request.Description?.Trim() ?? string.Empty,
            request.Status,
            request.DueDate);

        taskRepository.Update(task);
        await taskRepository.UnitOfWork.Commit(cancellationToken);

        logger.LogInformation("Task updated {TaskId} for user {UserId}", task.Id, userId);
        return mapper.Map<TaskDto>(task);
    }

    public async Task Delete(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var task = await taskRepository.GetByIdForUser(id, userId, cancellationToken)
            ?? throw new NotFoundException($"Task '{id}' was not found.");

        taskRepository.Delete(task);
        await taskRepository.UnitOfWork.Commit(cancellationToken);

        logger.LogInformation("Task deleted {TaskId} for user {UserId}", id, userId);
    }
}
