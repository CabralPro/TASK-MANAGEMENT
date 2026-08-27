using TaskManagement.Application;
using TaskManagement.Infrastructure.Persistence;
using TaskManagement.WebAPI.Setup;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TaskManagement.WebAPI;

/// <summary>
/// Configures services and the HTTP pipeline for the TaskManagement API host.
/// </summary>
public class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddApplication()
            .AddApplicationOptions(Configuration)
            .AddApplicationApi()
            .AddApplicationAuthentication(Configuration)
            .AddApplicationInfrastructure(Configuration);
    }

    public void Configure(
        IApplicationBuilder app,
        IWebHostEnvironment env,
        TaskManagementDbContext dbContext,
        ILogger<Startup> logger)
    {
        app.UseApplicationPipeline(env);
        DatabaseInitializer.Initialize(env, dbContext, logger);

        app.UseEndpoints(endpoints => endpoints.MapApplicationEndpoints(env));
    }
}
