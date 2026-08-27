using System;
using TaskManagement.Application.DTOs.Requests;
using TaskManagement.Application.Validators;
using Xunit;
using TaskStatusEnum = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Application.Tests;

public class ValidatorTests
{
    [Fact]
    public void CreateTaskRequestValidator_RejectsEmptyTitle()
    {
        var validator = new CreateTaskRequestValidator();
        var result = validator.Validate(new CreateTaskRequest
        {
            Title = "",
            Status = TaskStatusEnum.Pending,
            DueDate = DateTime.UtcNow
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Title");
    }

    [Fact]
    public void UpdateTaskRequestValidator_RejectsTitleOver100()
    {
        var validator = new UpdateTaskRequestValidator();
        var result = validator.Validate(new UpdateTaskRequest
        {
            Title = new string('a', 101),
            Status = TaskStatusEnum.Pending,
            DueDate = DateTime.UtcNow
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("100"));
    }

    [Fact]
    public void CreateTaskRequestValidator_RejectsInvalidStatus()
    {
        var validator = new CreateTaskRequestValidator();
        var result = validator.Validate(new CreateTaskRequest
        {
            Title = "Valid",
            Status = (TaskStatusEnum)99,
            DueDate = DateTime.UtcNow
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Status");
    }

    [Fact]
    public void RegisterRequestValidator_RequiresFields()
    {
        var validator = new RegisterRequestValidator();
        var result = validator.Validate(new RegisterRequest());

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "UserName");
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
        Assert.Contains(result.Errors, e => e.PropertyName == "Password");
    }

    [Fact]
    public void SignInRequestValidator_RequiresCredentials()
    {
        var validator = new SignInRequestValidator();
        var result = validator.Validate(new SignInRequest { UserName = "", Password = "" });

        Assert.False(result.IsValid);
    }
}
