using System.Threading;
using System.Threading.Tasks;
using TaskManagement.Application.Auth;
using TaskManagement.Application.DTOs.Requests;
using TaskManagement.Application.DTOs.Responses;
using TaskManagement.Domain.Exceptions;
using TaskManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace TaskManagement.Application.UseCases.Auth;

public class SignInUseCase(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator tokenGenerator,
    ILogger<SignInUseCase> logger) : ISignInUseCase
{
    public async Task<SignInResponse> SignInAsync(
        SignInRequest request,
        CancellationToken cancellationToken = default)
    {
        var userName = request.UserName.Trim();
        var user = await userRepository.GetByUsername(userName, cancellationToken);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            logger.LogWarning("Sign-in failed for user {UserName}", userName);
            throw new UnauthorizedException("Incorrect username or password");
        }

        var authenticated = new AuthenticatedUser
        {
            Id = user.Id,
            UserName = user.Username,
            Email = user.Email,
            Role = "user"
        };

        logger.LogInformation("Sign-in succeeded for user {UserId} {UserName}", user.Id, user.Username);

        return new SignInResponse
        {
            User = authenticated,
            Token = tokenGenerator.GenerateToken(authenticated)
        };
    }
}
