namespace TaskManagement.Application.DTOs.Responses;

public class SignInResponse
{
    public AuthenticatedUser User { get; init; }
    public string Token { get; init; }
}
