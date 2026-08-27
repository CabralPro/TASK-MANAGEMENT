using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Exceptions;
using TaskManagement.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using TaskStatusEnum = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Infrastructure.Tests;

public class TaskManagementDbContextTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly TaskManagementDbContext _context;
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public TaskManagementDbContextTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<TaskManagementDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new TaskManagementDbContext(options);
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task Commit_WhenAddingUser_PersistsChanges()
    {
        var user = new User("bob", "bob@example.com", "hash");
        _context.Users.Add(user);

        var committed = await _context.Commit(Ct);

        Assert.True(committed);
        Assert.Single(_context.Users);
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_WhenActionFails_RollsBack()
    {
        var user = new User("carol", "carol@example.com", "hash");
        _context.Users.Add(user);
        await _context.SaveChangesAsync(Ct);

        await Assert.ThrowsAsync<DomainException>(() =>
            _context.ExecuteInTransactionAsync(async ct =>
            {
                _context.Tasks.Add(new TaskItem("T", "d", TaskStatusEnum.Pending, DateTime.UtcNow, user.Id));
                await _context.SaveChangesAsync(ct);
                throw new DomainException("Rollback test");
            }, Ct));

        Assert.Empty(_context.Tasks.ToList());
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
