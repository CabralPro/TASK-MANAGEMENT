using System;
using System.Threading.RateLimiting;
using TaskManagement.Infrastructure;
using TaskManagement.Infrastructure.Persistence;
using TaskManagement.WebAPI.Setup.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TaskManagement.WebAPI.Setup;

/// <summary>
/// Registers host infrastructure: persistence adapters, health checks, CORS, and rate limiting.
/// </summary>
public static class InfrastructureExtensions
{
    public static IServiceCollection AddApplicationInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);

        services.AddHealthChecks()
            .AddDbContextCheck<TaskManagementDbContext>("database");

        var cors = configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>() ?? new CorsOptions();
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
                policy.WithOrigins(cors.AllowedOrigins.Count > 0
                        ? cors.AllowedOrigins.ToArray()
                        : ["http://localhost:4200"])
                    .AllowAnyHeader()
                    .AllowAnyMethod());
        });

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("auth-sign-in", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));
        });

        return services;
    }
}
