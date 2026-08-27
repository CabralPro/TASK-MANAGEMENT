using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Persistence;
using TaskManagement.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using TaskStatusEnum = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Infrastructure.Tests;

public class TaskRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly TaskManagementDbContext _context;
    private readonly TaskRepository _sut;
    private readonly Guid _userId = Guid.NewGuid();
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public TaskRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<TaskManagementDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new TaskManagementDbContext(options);
        _context.Database.EnsureCreated();

        _context.Users.Add(new User(_userId, "owner", "owner@example.com", "hash"));
        _context.SaveChanges();

        _sut = new TaskRepository(_context);
    }

    [Fact]
    public async Task GetAllByUserId_ReturnsOnlyUserTasks()
    {
        var otherUserId = Guid.NewGuid();
        _context.Users.Add(new User(otherUserId, "other", "other@example.com", "hash"));
        _context.Tasks.Add(new TaskItem("Mine", "d", TaskStatusEnum.Pending, DateTime.UtcNow, _userId));
        _context.Tasks.Add(new TaskItem("Theirs", "d", TaskStatusEnum.Pending, DateTime.UtcNow, otherUserId));
        await _context.SaveChangesAsync(Ct);

        var result = (await _sut.GetAllByUserId(_userId, Ct)).ToList();

        Assert.Single(result);
        Assert.Equal("Mine", result[0].Title);
    }

    [Fact]
    public async Task GetByIdForUser_WhenOtherUser_ReturnsNull()
    {
        var otherUserId = Guid.NewGuid();
        _context.Users.Add(new User(otherUserId, "other", "other@example.com", "hash"));
        var task = new TaskItem("Mine", "d", TaskStatusEnum.Pending, DateTime.UtcNow, _userId);
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync(Ct);

        var result = await _sut.GetByIdForUser(task.Id, otherUserId, Ct);

        Assert.Null(result);
    }

    [Fact]
    public async Task Delete_RemovesTask()
    {
        var task = new TaskItem("Delete me", "d", TaskStatusEnum.Pending, DateTime.UtcNow, _userId);
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync(Ct);

        _sut.Delete(task);
        await _context.SaveChangesAsync(Ct);

        Assert.Null(await _sut.GetByIdForUser(task.Id, _userId, Ct));
    }

    public void Dispose()
    {
        _sut.Dispose();
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
