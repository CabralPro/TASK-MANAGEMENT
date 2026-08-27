using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaskManagement.Domain.Abstractions;

/// <summary>
/// Defines the persistence boundary for atomic save and transactional workflows.
/// Implemented by the EF Core context in Infrastructure.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists pending changes tracked by the current context.
    /// </summary>
    /// <returns><c>true</c> when at least one row was written; otherwise <c>false</c>.</returns>
    Task<bool> Commit(CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs <paramref name="action"/> inside a database transaction, committing on success
    /// and rolling back on failure.
    /// </summary>
    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default);
}
