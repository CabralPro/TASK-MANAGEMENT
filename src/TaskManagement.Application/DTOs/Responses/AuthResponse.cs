using System;

namespace TaskManagement.Application.DTOs.Responses;

public class AuthResponse
{
    public string Token { get; init; }
    public Guid UserId { get; init; }
    public string Name { get; init; }
    public string Email { get; init; }
    public string Role { get; init; }
}
