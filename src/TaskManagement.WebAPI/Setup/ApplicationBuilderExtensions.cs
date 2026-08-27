using TaskManagement.WebAPI.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;
using Serilog;

namespace TaskManagement.WebAPI.Setup;

/// <summary>
/// Configures the HTTP middleware pipeline and endpoint routing.
/// </summary>
public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseApplicationPipeline(
        this IApplicationBuilder app,
        IWebHostEnvironment env)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseStructuredRequestLogging();
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        if (!env.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseRouting();
        app.UseRateLimiter();
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }

    public static IEndpointRouteBuilder MapApplicationEndpoints(
        this IEndpointRouteBuilder endpoints,
        IWebHostEnvironment env)
    {
        endpoints.MapControllers();
        endpoints.MapHealthChecks("/health");
        endpoints.MapHealthChecks("/health/ready");

        if (env.IsDevelopment())
        {
            endpoints.MapOpenApi().WithDocumentPerVersion();
            endpoints.MapScalarApiReference(options =>
            {
                options
                    .WithTitle("TaskManagement API")
                    .WithOpenApiRoutePattern("/openapi/{documentName}.json")
                    .AddPreferredSecuritySchemes(["Bearer"]);
            });
        }

        return endpoints;
    }
}
