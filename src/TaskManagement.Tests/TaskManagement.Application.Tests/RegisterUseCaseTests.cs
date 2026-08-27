using System;
using System.Threading;
using System.Threading.Tasks;
using TaskManagement.Application.Auth;
using TaskManagement.Application.DTOs.Requests;
using TaskManagement.Application.UseCases.Auth;
using TaskManagement.Domain.Abstractions;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Exceptions;
using TaskManagement.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace TaskManagement.Application.Tests;

public class RegisterUseCaseTests
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly RegisterUseCase _sut;

    public RegisterUseCaseTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _userRepository.UnitOfWork.Returns(_unitOfWork);
        _sut = new RegisterUseCase(
            _userRepository,
            _passwordHasher,
            NullLogger<RegisterUseCase>.Instance);
    }

    [Fact]
    public async Task Register_WithNewUser_PersistsAndReturnsUser()
    {
        var ct = TestContext.Current.CancellationToken;
        _userRepository.ExistsByUsernameOrEmail("alice", "alice@example.com", Arg.Any<CancellationToken>())
            .Returns(false);
        _passwordHasher.Hash("Secret1!").Returns("hashed");
        _unitOfWork.Commit(Arg.Any<CancellationToken>()).Returns(true);

        var result = await _sut.RegisterAsync(new RegisterRequest
        {
            UserName = "alice",
            Email = "alice@example.com",
            Password = "Secret1!"
        }, ct);

        Assert.Equal("alice", result.UserName);
        Assert.Equal("alice@example.com", result.Email);
        _userRepository.Received(1).Add(Arg.Is<User>(u =>
            u.Username == "alice" && u.Email == "alice@example.com" && u.PasswordHash == "hashed"));
        await _unitOfWork.Received(1).Commit(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Register_WhenUsernameOrEmailExists_ThrowsDomainException()
    {
        var ct = TestContext.Current.CancellationToken;
        _userRepository.ExistsByUsernameOrEmail("alice", "alice@example.com", Arg.Any<CancellationToken>())
            .Returns(true);

        await Assert.ThrowsAsync<DomainException>(() => _sut.RegisterAsync(new RegisterRequest
        {
            UserName = "alice",
            Email = "alice@example.com",
            Password = "Secret1!"
        }, ct));
    }
}
