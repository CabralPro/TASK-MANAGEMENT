using System;
using System.Threading;
using System.Threading.Tasks;
using TaskManagement.Application.Auth;
using TaskManagement.Application.DTOs.Requests;
using TaskManagement.Application.DTOs.Responses;
using TaskManagement.Application.UseCases.Auth;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Exceptions;
using TaskManagement.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace TaskManagement.Application.Tests;

public class SignInUseCaseTests
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly SignInUseCase _sut;

    public SignInUseCaseTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _tokenGenerator = Substitute.For<IJwtTokenGenerator>();
        _sut = new SignInUseCase(
            _userRepository,
            _passwordHasher,
            _tokenGenerator,
            NullLogger<SignInUseCase>.Instance);
    }

    [Fact]
    public async Task SignIn_WithValidCredentials_ReturnsToken()
    {
        var ct = TestContext.Current.CancellationToken;
        var user = new User(Guid.NewGuid(), "demo", "demo@example.com", "hash");
        _userRepository.GetByUsername("demo", Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("@Demo123", "hash").Returns(true);
        _tokenGenerator.GenerateToken(Arg.Any<AuthenticatedUser>()).Returns("jwt-token");

        var result = await _sut.SignInAsync(new SignInRequest
        {
            UserName = "demo",
            Password = "@Demo123"
        }, ct);

        Assert.Equal("jwt-token", result.Token);
        Assert.Equal("demo", result.User.UserName);
        Assert.Equal("demo@example.com", result.User.Email);
    }

    [Fact]
    public async Task SignIn_WithInvalidCredentials_ThrowsUnauthorizedException()
    {
        var ct = TestContext.Current.CancellationToken;
        _userRepository.GetByUsername("demo", Arg.Any<CancellationToken>()).Returns((User)null);

        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.SignInAsync(new SignInRequest
        {
            UserName = "demo",
            Password = "wrong"
        }, ct));

        Assert.Equal("Incorrect username or password", ex.Message);
    }
}
