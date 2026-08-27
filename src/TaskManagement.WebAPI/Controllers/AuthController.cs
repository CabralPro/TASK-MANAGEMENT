using System.Threading;
using System.Threading.Tasks;
using Asp.Versioning;
using TaskManagement.Application.DTOs.Requests;
using TaskManagement.Application.DTOs.Responses;
using TaskManagement.Application.UseCases.Auth;
using TaskManagement.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace TaskManagement.WebAPI.Controllers;

/// <summary>
/// Authentication endpoints for registration and sign-in.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController(
    IRegisterUseCase registerUseCase,
    ISignInUseCase signInUseCase) : ApiControllerBase
{
    /// <summary>
    /// Registers a new user account.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-sign-in")]
    [EndpointSummary("Register")]
    [EndpointDescription("Creates a new user account.")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var user = await registerUseCase.RegisterAsync(request, cancellationToken);

        return OkResponse(new AuthResponse
        {
            UserId = user.Id,
            Name = user.UserName,
            Email = user.Email,
            Role = user.Role
        }, "User registered successfully.");
    }

    /// <summary>
    /// Signs in a user and returns a JWT.
    /// </summary>
    [HttpPost("sign-in")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-sign-in")]
    [EndpointSummary("Sign-in")]
    [EndpointDescription("Authenticates a user and returns a JWT bearer token.")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> SignIn(
        [FromBody] SignInRequest request,
        CancellationToken cancellationToken)
    {
        var result = await signInUseCase.SignInAsync(request, cancellationToken);

        return OkResponse(new AuthResponse
        {
            Token = result.Token,
            UserId = result.User.Id,
            Name = result.User.UserName,
            Email = result.User.Email,
            Role = result.User.Role
        });
    }
}
