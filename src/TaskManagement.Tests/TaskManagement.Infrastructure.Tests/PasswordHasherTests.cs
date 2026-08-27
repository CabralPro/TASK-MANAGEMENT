using TaskManagement.Infrastructure.Identity;
using Xunit;

namespace TaskManagement.Infrastructure.Tests;

public class PasswordHasherTests
{
    [Fact]
    public void HashAndVerify_RoundTrips()
    {
        var sut = new PasswordHasher();
        var hash = sut.Hash("@Demo123");

        Assert.True(sut.Verify("@Demo123", hash));
        Assert.False(sut.Verify("wrong", hash));
    }
}
