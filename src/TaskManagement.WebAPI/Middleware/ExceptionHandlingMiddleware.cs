using System;
using System.Threading.Tasks;
using TaskManagement.Domain.Exceptions;
using TaskManagement.WebAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TaskManagement.WebAPI.Middleware;

/// <summary>
/// Catches unhandled exceptions and maps domain errors to HTTP status codes
/// using the standard <see cref="Models.ApiResponse{T}"/> envelope.
/// </summary>
public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment environment)
{
    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (UnauthorizedException ex)
        {
            logger.LogWarning(
                ex,
                "Authentication failed {Method} {Path} -> {StatusCode}",
                context.Request.Method,
                context.Request.Path,
                StatusCodes.Status401Unauthorized);
            await WriteAsync(context, StatusCodes.Status401Unauthorized, ApiResponse<object>.Fail(ex.Message));
        }
        catch (NotFoundException ex)
        {
            logger.LogWarning(
                ex,
                "Resource not found {Method} {Path} -> {StatusCode}",
                context.Request.Method,
                context.Request.Path,
                StatusCodes.Status404NotFound);
            await WriteAsync(context, StatusCodes.Status404NotFound, ApiResponse<object>.Fail(ex.Message));
        }
        catch (DomainException ex)
        {
            logger.LogWarning(
                ex,
                "Domain rule failed {Method} {Path} -> {StatusCode}",
                context.Request.Method,
                context.Request.Path,
                StatusCodes.Status400BadRequest);
            await WriteAsync(context, StatusCodes.Status400BadRequest, ApiResponse<object>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unhandled exception {Method} {Path}",
                context.Request.Method,
                context.Request.Path);
            var message = environment.IsDevelopment()
                ? ex.Message
                : "An unexpected error occurred";
            await WriteAsync(context, StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail(message));
        }
    }

    private static async Task WriteAsync(HttpContext context, int statusCode, ApiResponse<object> payload)
    {
        if (context.Response.HasStarted)
        {
            throw new InvalidOperationException("The response has already started; cannot write error envelope.");
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonConvert.SerializeObject(payload, JsonSettings));
    }
}
