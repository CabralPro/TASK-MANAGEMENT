using System;
using System.Linq;
using TaskManagement.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace TaskManagement.Infrastructure.Tests;

public class TaskManagementDataSeederTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly TaskManagementDbContext _context;

    public TaskManagementDataSeederTests()
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
    public void SeedIfEmpty_WhenDatabaseEmpty_AddsDemoUserAndTasks()
    {
        TaskManagementDataSeeder.SeedIfEmpty(_context);

        Assert.Single(_context.Users);
        Assert.Equal(3, _context.Tasks.Count());
        Assert.Equal("demo", _context.Users.Single().Username);
    }

    [Fact]
    public void SeedIfEmpty_WhenUsersExist_DoesNotDuplicate()
    {
        _context.Users.Add(new Domain.Entities.User("existing", "e@example.com", "hash"));
        _context.SaveChanges();

        TaskManagementDataSeeder.SeedIfEmpty(_context);

        Assert.Single(_context.Users);
        Assert.Empty(_context.Tasks);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
