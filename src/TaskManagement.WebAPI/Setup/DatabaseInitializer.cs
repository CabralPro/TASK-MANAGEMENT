using System.IO;
using System.Linq;
using TaskManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace TaskManagement.WebAPI.Setup;

/// <summary>
/// Ensures the SQLite database exists, applies migrations, and seeds initial data.
/// </summary>
public static class DatabaseInitializer
{
    public static void Initialize(
        IWebHostEnvironment env,
        TaskManagementDbContext dbContext,
        ILogger logger)
    {
        if (string.Equals(env.EnvironmentName, "Testing", System.StringComparison.OrdinalIgnoreCase))
        {
            dbContext.Database.EnsureCreated();
            TaskManagementDataSeeder.SeedIfEmpty(dbContext);
            return;
        }

        var databaseDirectory = Path.Combine(env.ContentRootPath, "Database");
        Directory.CreateDirectory(databaseDirectory);

        logger.LogInformation("Applying database migrations to {DatabasePath}", dbContext.Database.GetDbConnection().DataSource);
        dbContext.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully");

        var userCountBeforeSeed = dbContext.Users.Count();
        TaskManagementDataSeeder.SeedIfEmpty(dbContext);

        if (dbContext.Users.Count() > userCountBeforeSeed)
        {
            logger.LogInformation(
                "Seeded database with demo user and {TaskCount} tasks",
                dbContext.Tasks.Count());
        }
        else
        {
            logger.LogDebug("Database already contains data; seed skipped");
        }
    }
}
