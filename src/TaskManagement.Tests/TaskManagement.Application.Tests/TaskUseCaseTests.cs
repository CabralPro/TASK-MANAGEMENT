using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using TaskManagement.Application.DTOs.Requests;
using TaskManagement.Application.Mapping;
using TaskManagement.Application.UseCases.Tasks;
using TaskManagement.Domain.Abstractions;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Exceptions;
using TaskManagement.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using TaskStatusEnum = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Application.Tests;

public class TaskUseCaseTests
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TaskUseCase _sut;
    private readonly Guid _userId = Guid.NewGuid();
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public TaskUseCaseTests()
    {
        _taskRepository = Substitute.For<ITaskRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _taskRepository.UnitOfWork.Returns(_unitOfWork);
        _unitOfWork.Commit(Arg.Any<CancellationToken>()).Returns(true);

        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<DomainToDtoMappingProfile>();
            cfg.AddProfile<RequestToDomainMappingProfile>();
        }, NullLoggerFactory.Instance).CreateMapper();

        _sut = new TaskUseCase(_taskRepository, mapper, NullLogger<TaskUseCase>.Instance);
    }

    [Fact]
    public async Task GetAll_ReturnsMappedTasksForUser()
    {
        var tasks = new List<TaskItem>
        {
            new("A", "d", TaskStatusEnum.Pending, DateTime.UtcNow, _userId)
        };
        _taskRepository.GetAllByUserId(_userId, Arg.Any<CancellationToken>()).Returns(tasks);

        var result = (await _sut.GetAll(_userId, Ct)).ToList();

        Assert.Single(result);
        Assert.Equal("A", result[0].Title);
    }

    [Fact]
    public async Task GetById_WhenMissing_ThrowsNotFound()
    {
        _taskRepository.GetByIdForUser(Arg.Any<Guid>(), _userId, Arg.Any<CancellationToken>())
            .Returns((TaskItem)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.GetById(Guid.NewGuid(), _userId, Ct));
    }

    [Fact]
    public async Task Create_AddsTaskForUser()
    {
        var request = new CreateTaskRequest
        {
            Title = "New task",
            Description = "Desc",
            Status = TaskStatusEnum.Pending,
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        var result = await _sut.Create(request, _userId, Ct);

        Assert.Equal("New task", result.Title);
        Assert.Equal(_userId, result.UserId);
        _taskRepository.Received(1).Add(Arg.Is<TaskItem>(t => t.UserId == _userId && t.Title == "New task"));
    }

    [Fact]
    public async Task Update_WhenExists_UpdatesFields()
    {
        var existing = new TaskItem("Old", "Old", TaskStatusEnum.Pending, DateTime.UtcNow, _userId);
        _taskRepository.GetByIdForUser(existing.Id, _userId, Arg.Any<CancellationToken>())
            .Returns(existing);

        var result = await _sut.Update(existing.Id, new UpdateTaskRequest
        {
            Title = "Updated",
            Description = "New desc",
            Status = TaskStatusEnum.Completed,
            DueDate = DateTime.UtcNow.AddDays(2)
        }, _userId, Ct);

        Assert.Equal("Updated", result.Title);
        Assert.Equal(TaskStatusEnum.Completed, result.Status);
        _taskRepository.Received(1).Update(existing);
    }

    [Fact]
    public async Task Delete_WhenExists_RemovesTask()
    {
        var existing = new TaskItem("Old", "Old", TaskStatusEnum.Pending, DateTime.UtcNow, _userId);
        _taskRepository.GetByIdForUser(existing.Id, _userId, Arg.Any<CancellationToken>())
            .Returns(existing);

        await _sut.Delete(existing.Id, _userId, Ct);

        _taskRepository.Received(1).Delete(existing);
        await _unitOfWork.Received(1).Commit(Arg.Any<CancellationToken>());
    }
}
