using System;
using System.Linq;
using TaskManagement.Domain.Entities;
using TaskStatusEnum = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Infrastructure.Persistence;

public static class TaskManagementDataSeeder
{
    public static readonly Guid DemoUserId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

    public static void SeedIfEmpty(TaskManagementDbContext context)
    {
        if (context.Users.Any())
        {
            return;
        }

        var demoUser = new User(
            DemoUserId,
            "demo",
            "demo@example.com",
            BCrypt.Net.BCrypt.HashPassword("@Demo123"));

        context.Users.Add(demoUser);

        var now = DateTime.UtcNow;
        context.Tasks.AddRange(
            new TaskItem(
                "Welcome task",
                "Explore the task management API.",
                TaskStatusEnum.Pending,
                now.AddDays(3),
                DemoUserId),
            new TaskItem(
                "Complete profile",
                "Update your account details when ready.",
                TaskStatusEnum.InProgress,
                now.AddDays(7),
                DemoUserId),
            new TaskItem(
                "Ship first feature",
                "Deliver a working CRUD flow end to end.",
                TaskStatusEnum.Completed,
                now.AddDays(-1),
                DemoUserId));

        context.SaveChanges();
    }
}
