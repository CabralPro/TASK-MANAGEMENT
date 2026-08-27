namespace TaskManagement.Domain.Entities;

/// <summary>
/// Marker for aggregate root entities — the entry point for a consistency boundary
/// and the type persisted through <see cref="Abstractions.IRepository{T}"/>.
/// </summary>
public interface IAggregateRoot { }