using System;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.DTOs.Responses;

public class TaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public TaskStatus Status { get; set; }
    public DateTime DueDate { get; set; }
    public Guid UserId { get; set; }
}
