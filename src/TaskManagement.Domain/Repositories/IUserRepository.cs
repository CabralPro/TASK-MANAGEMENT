using System;
using System.Threading;
using System.Threading.Tasks;
using TaskManagement.Domain.Abstractions;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Domain.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User> GetById(Guid id, CancellationToken cancellationToken = default);
    Task<User> GetByUsername(string username, CancellationToken cancellationToken = default);
    Task<User> GetByEmail(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUsernameOrEmail(string username, string email, CancellationToken cancellationToken = default);
    void Add(User user);
}
