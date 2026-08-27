using System;
using System.Linq;
using TaskManagement.WebAPI.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Events;

namespace TaskManagement.WebAPI.Setup;

internal static class SerilogRequestLoggingExtensions
{
    public static IApplicationBuilder UseStructuredRequestLogging(this IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.GetLevel = static (httpContext, elapsed, ex) => GetRequestLevel(httpContext, elapsed, ex);

            options.EnrichDiagnosticContext = static (diagnosticContext, httpContext) =>
            {
                EnrichFromRequest(diagnosticContext, httpContext);
            };
        });

        return app;
    }

    private static void EnrichFromRequest(IDiagnosticContext diagnosticContext, HttpContext httpContext)
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);

        if (httpContext.Request.QueryString.HasValue)
        {
            diagnosticContext.Set("QueryString", httpContext.Request.QueryString.Value);
        }

        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            diagnosticContext.Set("UserName", httpContext.User.Identity.Name);
        }

        if (httpContext.Response.Headers.TryGetValue(CorrelationIdMiddleware.HeaderName, out var correlationId))
        {
            diagnosticContext.Set("CorrelationId", correlationId.FirstOrDefault());
        }
    }

    private static LogEventLevel GetRequestLevel(HttpContext httpContext, double elapsedMs, Exception ex)
    {
        if (ex is not null || httpContext.Response.StatusCode >= 500)
        {
            return LogEventLevel.Error;
        }

        if (httpContext.Response.StatusCode >= 400)
        {
            return LogEventLevel.Warning;
        }

        if (elapsedMs > 1000)
        {
            return LogEventLevel.Warning;
        }

        return LogEventLevel.Information;
    }
}
