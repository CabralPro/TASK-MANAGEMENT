using System;
using TaskManagement.WebAPI.Setup.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace TaskManagement.WebAPI;

/// <summary>
/// Application entry point that bootstraps Serilog and the ASP.NET Core host.
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.With<ClassNameEnricher>()
            .WriteStyledConsole()
            .CreateBootstrapLogger();

        try
        {
            CreateHostBuilder(args).Build().Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.With<ClassNameEnricher>()
                .WriteStyledConsole())
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
}
