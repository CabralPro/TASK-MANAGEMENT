using TaskManagement.Application.DTOs.Responses;

namespace TaskManagement.Application.Auth;

public interface IJwtTokenGenerator
{
    string GenerateToken(AuthenticatedUser user);
}
