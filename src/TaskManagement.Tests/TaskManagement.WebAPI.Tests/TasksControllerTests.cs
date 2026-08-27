using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using TaskManagement.Application.DTOs.Requests;
using TaskManagement.Application.DTOs.Responses;
using TaskManagement.Application.UseCases.Tasks;
using TaskManagement.Domain.Exceptions;
using TaskManagement.WebAPI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using TaskStatusEnum = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.WebAPI.Tests;

public class TasksControllerTests
{
    [Fact]
    public async Task GetAll_WithoutNameIdentifierClaim_ThrowsUnauthorized()
    {
        var controller = CreateController(new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, "demo")],
            authenticationType: "Test")));

        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            controller.GetAll(CancellationToken.None));

        Assert.Equal("User identity is missing from the token.", ex.Message);
    }

    [Fact]
    public async Task GetById_WithNonGuidNameIdentifier_ThrowsUnauthorized()
    {
        var controller = CreateController(new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "not-a-guid")],
            authenticationType: "Test")));

        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            controller.GetById(Guid.NewGuid(), CancellationToken.None));

        Assert.Equal("User identity is missing from the token.", ex.Message);
    }

    [Fact]
    public async Task GetAll_WithValidUser_ReturnsTasks()
    {
        var userId = Guid.NewGuid();
        var expected = new TaskDto
        {
            Id = Guid.NewGuid(),
            Title = "Covered",
            Description = "via unit test",
            Status = TaskStatusEnum.Pending,
            DueDate = DateTime.UtcNow.AddDays(1),
            UserId = userId
        };
        var useCase = new StubTaskUseCase { Tasks = [expected] };
        var controller = CreateController(
            new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
                authenticationType: "Test")),
            useCase);

        var result = await controller.GetAll(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(userId, useCase.LastUserId);
        Assert.NotNull(ok.Value);
    }

    private static TasksController CreateController(
        ClaimsPrincipal user,
        ITaskUseCase useCase = null)
    {
        var controller = new TasksController(useCase ?? new StubTaskUseCase());
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
        return controller;
    }

    private sealed class StubTaskUseCase : ITaskUseCase
    {
        public List<TaskDto> Tasks { get; init; } = [];
        public Guid LastUserId { get; private set; }

        public Task<IEnumerable<TaskDto>> GetAll(Guid userId, CancellationToken cancellationToken = default)
        {
            LastUserId = userId;
            return Task.FromResult<IEnumerable<TaskDto>>(Tasks);
        }

        public Task<TaskDto> GetById(Guid id, Guid userId, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<TaskDto> Create(CreateTaskRequest request, Guid userId, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<TaskDto> Update(Guid id, UpdateTaskRequest request, Guid userId, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task Delete(Guid id, Guid userId, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }
}
