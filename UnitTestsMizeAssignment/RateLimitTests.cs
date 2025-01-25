using MizeAssignment.Services;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Xunit;

public class RateLimitTests
{
    [Fact]
    public void ShouldWait_WhenUnderLimit_ReturnsFalse()
    {
        // Arrange
        var rateLimit = new RateLimit(5, TimeSpan.FromMinutes(1));

        // Act
        var shouldWait = rateLimit.ShouldWait();

        // Assert
        Assert.False(shouldWait);
    }

    [Fact]
    public async Task EnforceLimitAsync_WaitsWhenAtLimit()
    {
        // Arrange
        var rateLimit = new RateLimit(1, TimeSpan.FromMilliseconds(500));
        await rateLimit.EnforceLimitAsync();

        var startTime = DateTime.UtcNow;

        // Act
        await rateLimit.EnforceLimitAsync();
        var elapsed = DateTime.UtcNow - startTime;

        // Assert
        Assert.True(elapsed >= TimeSpan.FromMilliseconds(500), "EnforceLimitAsync should wait for at least the time window.");
    }
}
