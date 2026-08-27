using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Asp.Versioning;
using TaskManagement.Application.DTOs.Requests;
using TaskManagement.Application.DTOs.Responses;
using TaskManagement.Application.UseCases.Tasks;
using TaskManagement.Domain.Exceptions;
using TaskManagement.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TaskManagement.WebAPI.Controllers;

/// <summary>
/// Task CRUD endpoints for the authenticated user.
/// </summary>
[Authorize]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/tasks")]
public class TasksController(ITaskUseCase taskUseCase) : ApiControllerBase
{
    /// <summary>
    /// Lists all tasks for the current user.
    /// </summary>
    [HttpGet]
    [EndpointSummary("List tasks")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TaskDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<TaskDto>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var tasks = await taskUseCase.GetAll(GetUserId(), cancellationToken);
        return OkResponse(tasks);
    }

    /// <summary>
    /// Gets a task by id for the current user.
    /// </summary>
    [HttpGet("{id:guid}")]
    [EndpointSummary("Get task")]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TaskDto>>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var task = await taskUseCase.GetById(id, GetUserId(), cancellationToken);
        return OkResponse(task);
    }

    /// <summary>
    /// Creates a task for the current user.
    /// </summary>
    [HttpPost]
    [EndpointSummary("Create task")]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<TaskDto>>> Create(
        [FromBody] CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var task = await taskUseCase.Create(request, GetUserId(), cancellationToken);
        return CreatedResponse($"/api/v1/tasks/{task.Id}", task);
    }

    /// <summary>
    /// Updates a task for the current user.
    /// </summary>
    [HttpPut("{id:guid}")]
    [EndpointSummary("Update task")]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TaskDto>>> Update(
        Guid id,
        [FromBody] UpdateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var task = await taskUseCase.Update(id, request, GetUserId(), cancellationToken);
        return OkResponse(task);
    }

    /// <summary>
    /// Deletes a task for the current user.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [EndpointSummary("Delete task")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await taskUseCase.Delete(id, GetUserId(), cancellationToken);
        return OkResponse(true);
    }

    private Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(value) || !Guid.TryParse(value, out var userId))
        {
            throw new UnauthorizedException("User identity is missing from the token.");
        }

        return userId;
    }
}
