using System;
using TaskManagement.Domain.Exceptions;

namespace TaskManagement.Domain.Entities;

public class TaskItem : Entity, IAggregateRoot
{
    public const int TitleMaxLength = 100;

    public string Title { get; private set; }
    public string Description { get; private set; }
    public Enums.TaskStatus Status { get; private set; }
    public DateTime DueDate { get; private set; }
    public Guid UserId { get; private set; }

    protected TaskItem() { }

    public TaskItem(
        string title,
        string description,
        Enums.TaskStatus status,
        DateTime dueDate,
        Guid userId)
    {
        UserId = userId;
        Apply(title, description, status, dueDate);
    }

    public TaskItem(
        Guid id,
        string title,
        string description,
        Enums.TaskStatus status,
        DateTime dueDate,
        Guid userId) : base(id)
    {
        UserId = userId;
        Apply(title, description, status, dueDate);
    }

    public void Update(string title, string description, Enums.TaskStatus status, DateTime dueDate)
    {
        Apply(title, description, status, dueDate);
    }

    private void Apply(string title, string description, Enums.TaskStatus status, DateTime dueDate)
    {
        var normalizedTitle = (title ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalizedTitle))
        {
            throw new DomainException("Title is required");
        }

        if (normalizedTitle.Length > TitleMaxLength)
        {
            throw new DomainException("Title cannot exceed 100 characters");
        }

        if (!Enum.IsDefined(typeof(Enums.TaskStatus), status))
        {
            throw new DomainException("Status must be a valid task status");
        }

        Title = normalizedTitle;
        Description = description?.Trim() ?? string.Empty;
        Status = status;
        DueDate = dueDate;
    }
}
