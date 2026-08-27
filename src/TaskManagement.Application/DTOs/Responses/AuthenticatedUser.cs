using System;

namespace TaskManagement.Application.DTOs.Responses;

public class AuthenticatedUser
{
    public Guid Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
}
