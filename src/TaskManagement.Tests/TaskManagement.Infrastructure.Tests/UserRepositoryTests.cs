using System;
using System.Threading;
using System.Threading.Tasks;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Persistence;
using TaskManagement.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace TaskManagement.Infrastructure.Tests;

public class UserRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly TaskManagementDbContext _context;
    private readonly UserRepository _sut;
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public UserRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<TaskManagementDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new TaskManagementDbContext(options);
        _context.Database.EnsureCreated();
        _sut = new UserRepository(_context);
    }

    [Fact]
    public async Task GetByUsername_ReturnsMatchingUser()
    {
        _context.Users.Add(new User("demo", "demo@example.com", "hash"));
        await _context.SaveChangesAsync(Ct);

        var user = await _sut.GetByUsername("demo", Ct);

        Assert.NotNull(user);
        Assert.Equal("demo@example.com", user.Email);
    }

    [Fact]
    public async Task ExistsByUsernameOrEmail_WhenEmailTaken_ReturnsTrue()
    {
        _context.Users.Add(new User("demo", "demo@example.com", "hash"));
        await _context.SaveChangesAsync(Ct);

        Assert.True(await _sut.ExistsByUsernameOrEmail("other", "demo@example.com", Ct));
    }

    [Fact]
    public async Task Add_PersistsUser()
    {
        _sut.Add(new User("alice", "alice@example.com", "hash"));
        await _context.Commit(Ct);

        Assert.NotNull(await _sut.GetByUsername("alice", Ct));
    }

    public void Dispose()
    {
        _sut.Dispose();
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
