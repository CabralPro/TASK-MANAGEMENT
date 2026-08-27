using System;
using Xunit;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Exceptions;
using TaskStatusEnum = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Domain.Tests;

public class TaskItemTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var userId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddDays(1);

        var task = new TaskItem("Title", "Description", TaskStatusEnum.Pending, dueDate, userId);

        Assert.Equal("Title", task.Title);
        Assert.Equal("Description", task.Description);
        Assert.Equal(TaskStatusEnum.Pending, task.Status);
        Assert.Equal(dueDate, task.DueDate);
        Assert.Equal(userId, task.UserId);
        Assert.NotEqual(Guid.Empty, task.Id);
    }

    [Fact]
    public void Update_ChangesMutableFields()
    {
        var task = new TaskItem("Old", "Old desc", TaskStatusEnum.Pending, DateTime.UtcNow, Guid.NewGuid());
        var newDue = DateTime.UtcNow.AddDays(5);

        task.Update("New", "New desc", TaskStatusEnum.Completed, newDue);

        Assert.Equal("New", task.Title);
        Assert.Equal("New desc", task.Description);
        Assert.Equal(TaskStatusEnum.Completed, task.Status);
        Assert.Equal(newDue, task.DueDate);
    }

    [Fact]
    public void Constructor_NullDescription_BecomesEmpty()
    {
        var task = new TaskItem("Title", null, TaskStatusEnum.InProgress, DateTime.UtcNow, Guid.NewGuid());

        Assert.Equal(string.Empty, task.Description);
    }

    [Fact]
    public void Constructor_EmptyTitle_ThrowsDomainException()
    {
        var ex = Assert.Throws<DomainException>(() =>
            new TaskItem("  ", "desc", TaskStatusEnum.Pending, DateTime.UtcNow, Guid.NewGuid()));

        Assert.Equal("Title is required", ex.Message);
    }

    [Fact]
    public void Constructor_TitleOver100_ThrowsDomainException()
    {
        var ex = Assert.Throws<DomainException>(() =>
            new TaskItem(new string('a', 101), "desc", TaskStatusEnum.Pending, DateTime.UtcNow, Guid.NewGuid()));

        Assert.Equal("Title cannot exceed 100 characters", ex.Message);
    }

    [Fact]
    public void Constructor_InvalidStatus_ThrowsDomainException()
    {
        var ex = Assert.Throws<DomainException>(() =>
            new TaskItem("Title", "desc", (TaskStatusEnum)99, DateTime.UtcNow, Guid.NewGuid()));

        Assert.Equal("Status must be a valid task status", ex.Message);
    }

    [Fact]
    public void Update_EmptyTitle_ThrowsDomainException()
    {
        var task = new TaskItem("Old", "Old desc", TaskStatusEnum.Pending, DateTime.UtcNow, Guid.NewGuid());

        var ex = Assert.Throws<DomainException>(() =>
            task.Update("", "New desc", TaskStatusEnum.Completed, DateTime.UtcNow));

        Assert.Equal("Title is required", ex.Message);
        Assert.Equal("Old", task.Title);
    }

    [Fact]
    public void Constructor_WithExplicitId_UsesProvidedId()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddDays(1);

        var task = new TaskItem(id, "Title", "Description", TaskStatusEnum.Pending, dueDate, userId);

        Assert.Equal(id, task.Id);
        Assert.Equal(userId, task.UserId);
        Assert.Equal("Title", task.Title);
    }

    [Fact]
    public void Constructor_NullTitle_ThrowsDomainException()
    {
        var ex = Assert.Throws<DomainException>(() =>
            new TaskItem(null, "desc", TaskStatusEnum.Pending, DateTime.UtcNow, Guid.NewGuid()));

        Assert.Equal("Title is required", ex.Message);
    }

    [Fact]
    public void Constructor_TrimsTitleAndDescription()
    {
        var task = new TaskItem("  Title  ", "  Desc  ", TaskStatusEnum.Pending, DateTime.UtcNow, Guid.NewGuid());

        Assert.Equal("Title", task.Title);
        Assert.Equal("Desc", task.Description);
    }

    [Fact]
    public void Update_InvalidStatus_ThrowsDomainException()
    {
        var task = new TaskItem("Old", "Old desc", TaskStatusEnum.Pending, DateTime.UtcNow, Guid.NewGuid());

        var ex = Assert.Throws<DomainException>(() =>
            task.Update("Old", "Old desc", (TaskStatusEnum)99, DateTime.UtcNow));

        Assert.Equal("Status must be a valid task status", ex.Message);
        Assert.Equal(TaskStatusEnum.Pending, task.Status);
    }
}
