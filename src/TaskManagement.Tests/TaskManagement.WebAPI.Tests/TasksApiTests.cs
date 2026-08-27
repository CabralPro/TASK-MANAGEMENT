using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using TaskManagement.Application.DTOs.Requests;
using TaskManagement.Application.DTOs.Responses;
using TaskManagement.Infrastructure.Persistence;
using TaskManagement.WebAPI.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using TaskStatusEnum = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.WebAPI.Tests;

public class TasksApiTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _client = factory.CreateClient();
    private readonly CustomWebApplicationFactory _factory = factory;
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task GetTasks_WithoutToken_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/v1/tasks", Ct);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_ThenSignIn_ReturnsToken()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var username = $"user_{Guid.NewGuid():N}".Substring(0, 20);
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            userName = username,
            email = $"{username}@example.com",
            password = "Secret1!"
        }, Ct);

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var signInResponse = await _client.PostAsJsonAsync("/api/v1/auth/sign-in", new
        {
            userName = username,
            password = "Secret1!"
        }, Ct);

        Assert.Equal(HttpStatusCode.OK, signInResponse.StatusCode);
        var envelope = await signInResponse.Content.ReadFromJsonAsync<ApiResponse<AuthData>>(JsonOptions, Ct);
        Assert.True(envelope?.Success);
        Assert.False(string.IsNullOrWhiteSpace(envelope.Data?.Token));
    }

    [Fact]
    public async Task SignIn_DemoUser_ThenCreateListUpdateDeleteTask_Succeeds()
    {
        await EnsureDemoUserAsync();
        var token = await SignInAsync("demo", "@Demo123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await _client.PostAsJsonAsync("/api/v1/tasks", NewTask("API task"), JsonOptions, Ct);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<TaskDto>>(JsonOptions, Ct))!.Data;
        Assert.Equal("API task", created.Title);
        Assert.Equal($"/api/v1/tasks/{created.Id}", createResponse.Headers.Location?.OriginalString);

        var listResponse = await _client.GetAsync("/api/v1/tasks", Ct);
        var list = (await listResponse.Content.ReadFromJsonAsync<ApiResponse<TaskDto[]>>(JsonOptions, Ct))!.Data;
        Assert.Contains(list, t => t.Id == created.Id);

        var getResponse = await _client.GetAsync($"/api/v1/tasks/{created.Id}", Ct);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        created.Title = "Updated task";
        created.Status = TaskStatusEnum.Completed;
        var updateResponse = await _client.PutAsJsonAsync($"/api/v1/tasks/{created.Id}", created, JsonOptions, Ct);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = (await updateResponse.Content.ReadFromJsonAsync<ApiResponse<TaskDto>>(JsonOptions, Ct))!.Data;
        Assert.Equal("Updated task", updated.Title);
        Assert.Equal(TaskStatusEnum.Completed, updated.Status);

        var deleteResponse = await _client.DeleteAsync($"/api/v1/tasks/{created.Id}", Ct);
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        var missing = await _client.GetAsync($"/api/v1/tasks/{created.Id}", Ct);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [Fact]
    public async Task CannotAccessOtherUsersTask()
    {
        await EnsureDemoUserAsync();
        var demoToken = await SignInAsync("demo", "@Demo123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", demoToken);

        var createResponse = await _client.PostAsJsonAsync("/api/v1/tasks", NewTask("Private"), JsonOptions, Ct);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<TaskDto>>(JsonOptions, Ct))!.Data;
        Assert.NotNull(created);

        var otherUsername = $"other_{Guid.NewGuid():N}".Substring(0, 20);
        _client.DefaultRequestHeaders.Authorization = null;
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            userName = otherUsername,
            email = $"{otherUsername}@example.com",
            password = "Secret1!"
        }, Ct);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var otherToken = await SignInAsync(otherUsername, "Secret1!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherToken);

        var response = await _client.GetAsync($"/api/v1/tasks/{created.Id}", Ct);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateTask_WithInvalidTitle_ReturnsValidationError()
    {
        await EnsureDemoUserAsync();
        var token = await SignInAsync("demo", "@Demo123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/v1/tasks", new
        {
            title = "",
            description = "x",
            status = "Pending",
            dueDate = DateTime.UtcNow
        }, Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions, Ct);
        Assert.False(envelope!.Success);
        Assert.Equal("Validation failed", envelope.Message);
    }

    [Fact]
    public async Task SignIn_WithMissingCredentials_ReturnsValidationError()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.PostAsJsonAsync("/api/v1/auth/sign-in", new
        {
            userName = "",
            password = ""
        }, Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health", Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private Task EnsureDemoUserAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TaskManagementDbContext>();
        TaskManagementDataSeeder.SeedIfEmpty(db);
        return Task.CompletedTask;
    }

    private async Task<string> SignInAsync(string userName, string password)
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.PostAsJsonAsync("/api/v1/auth/sign-in", new
        {
            userName,
            password
        }, Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<AuthData>>(JsonOptions, Ct);
        Assert.True(envelope?.Success);
        Assert.False(string.IsNullOrWhiteSpace(envelope.Data?.Token));
        return envelope.Data.Token;
    }

    private static CreateTaskRequest NewTask(string title) => new()
    {
        Title = title,
        Description = "Test description",
        Status = TaskStatusEnum.Pending,
        DueDate = DateTime.UtcNow.AddDays(2)
    };

    private sealed class AuthData
    {
        public string Token { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
