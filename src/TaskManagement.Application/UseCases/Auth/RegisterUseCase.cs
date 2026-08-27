using System.Threading;
using System.Threading.Tasks;
using TaskManagement.Application.Auth;
using TaskManagement.Application.DTOs.Requests;
using TaskManagement.Application.DTOs.Responses;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Repositories;
using TaskManagement.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace TaskManagement.Application.UseCases.Auth;

public class RegisterUseCase(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ILogger<RegisterUseCase> logger) : IRegisterUseCase
{
    public async Task<AuthenticatedUser> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var username = request.UserName.Trim();
        var email = request.Email.Trim();

        if (await userRepository.ExistsByUsernameOrEmail(username, email, cancellationToken))
        {
            throw new DomainException("Username or email is already registered.");
        }

        var user = new User(username, email, passwordHasher.Hash(request.Password));
        userRepository.Add(user);
        await userRepository.UnitOfWork.Commit(cancellationToken);

        logger.LogInformation("User registered {UserId} {UserName}", user.Id, user.Username);

        return new AuthenticatedUser
        {
            Id = user.Id,
            UserName = user.Username,
            Email = user.Email,
            Role = "user"
        };
    }
}
