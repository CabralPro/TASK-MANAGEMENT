using System;
using System.Threading;
using System.Threading.Tasks;
using TaskManagement.Domain.Abstractions;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Repositories;
using TaskManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace TaskManagement.Infrastructure.Repositories;

public class UserRepository(TaskManagementDbContext context) : IUserRepository
{
    public IUnitOfWork UnitOfWork => context;

    public async Task<User> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User> GetByUsername(string username, CancellationToken cancellationToken = default)
    {
        return await context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<User> GetByEmail(string email, CancellationToken cancellationToken = default)
    {
        return await context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> ExistsByUsernameOrEmail(
        string username,
        string email,
        CancellationToken cancellationToken = default)
    {
        return await context.Users.AsNoTracking()
            .AnyAsync(u => u.Username == username || u.Email == email, cancellationToken);
    }

    public void Add(User user)
    {
        context.Users.Add(user);
    }

    public void Dispose()
    {
        context?.Dispose();
        GC.SuppressFinalize(this);
    }
}
