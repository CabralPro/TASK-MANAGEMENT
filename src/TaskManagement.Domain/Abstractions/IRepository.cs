using TaskManagement.Domain.Entities;
using System;

namespace TaskManagement.Domain.Abstractions;

public interface IRepository<T> : IDisposable where T : IAggregateRoot
{
    IUnitOfWork UnitOfWork { get; }
}
