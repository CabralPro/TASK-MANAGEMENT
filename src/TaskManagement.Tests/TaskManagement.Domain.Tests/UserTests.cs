using System;
using Xunit;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Domain.Tests;

public class UserTests
{
    [Fact]
    public void Constructor_SetsIdentityFields()
    {
        var user = new User("demo", "demo@example.com", "hash");

        Assert.Equal("demo", user.Username);
        Assert.Equal("demo@example.com", user.Email);
        Assert.Equal("hash", user.PasswordHash);
        Assert.NotEqual(Guid.Empty, user.Id);
    }

    [Fact]
    public void Constructor_WithId_PreservesId()
    {
        var id = Guid.NewGuid();
        var user = new User(id, "demo", "demo@example.com", "hash");

        Assert.Equal(id, user.Id);
    }
}
