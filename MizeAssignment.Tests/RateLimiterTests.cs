using System;
using System.Threading.Tasks;
using MizeAssignment.Services;
using Moq;
using Xunit;

public class RateLimiterTests
{
    [Fact]
    public async Task PerformAsync_EnforcesAllRateLimits()
    {
        // Arrange
        var rateLimitMock1 = new Mock<IRateLimit>();
        var rateLimitMock2 = new Mock<IRateLimit>();

        rateLimitMock1.Setup(rl => rl.EnforceLimitAsync()).Returns(Task.CompletedTask);
        rateLimitMock2.Setup(rl => rl.EnforceLimitAsync()).Returns(Task.CompletedTask);

        var rateLimiter = new RateLimiter<string>(arg => Task.CompletedTask, rateLimitMock1.Object, rateLimitMock2.Object);

        // Act
        await rateLimiter.PerformAsync("Test");

        // Assert
        rateLimitMock1.Verify(rl => rl.EnforceLimitAsync(), Times.Once);
        rateLimitMock2.Verify(rl => rl.EnforceLimitAsync(), Times.Once);
    }

    [Fact]
    public async Task PerformAsync_ExecutesActionIfLimitsAllow()
    {
        // Arrange
        var rateLimitMock = new Mock<IRateLimit>();
        rateLimitMock.Setup(rl => rl.EnforceLimitAsync()).Returns(Task.CompletedTask);

        var actionExecuted = false;
        var rateLimiter = new RateLimiter<string>(arg =>
        {
            actionExecuted = true;
            return Task.CompletedTask;
        }, rateLimitMock.Object);

        // Act
        await rateLimiter.PerformAsync("Test");

        // Assert
        Assert.True(actionExecuted, "Action should have been executed.");
    }

    [Fact]
    public async Task PerformAsync_EnforcesRateLimitsInOrder()
    {
        // Arrange
        var rateLimitMock1 = new Mock<IRateLimit>();
        var rateLimitMock2 = new Mock<IRateLimit>();

        var executionOrder = new List<string>();

        rateLimitMock1
            .Setup(rl => rl.EnforceLimitAsync())
            .Callback(() => executionOrder.Add("Limit1"))
            .Returns(Task.CompletedTask);

        rateLimitMock2
            .Setup(rl => rl.EnforceLimitAsync())
            .Callback(() => executionOrder.Add("Limit2"))
            .Returns(Task.CompletedTask);

        var rateLimiter = new RateLimiter<string>(
            arg =>
            {
                executionOrder.Add("Action");
                return Task.CompletedTask;
            },
            rateLimitMock1.Object,
            rateLimitMock2.Object);

        // Act
        await rateLimiter.PerformAsync("Test");

        // Assert
        Assert.Equal(new[] { "Limit1", "Limit2", "Action" }, executionOrder);
    }

    [Fact]
    public async Task PerformAsync_DoesNotExecuteActionIfRateLimitFails()
    {
        // Arrange
        var rateLimitMock = new Mock<IRateLimit>();
        rateLimitMock
            .Setup(rl => rl.EnforceLimitAsync())
            .ThrowsAsync(new InvalidOperationException("Rate limit exceeded"));

        var actionExecuted = false;
        var rateLimiter = new RateLimiter<string>(
            arg =>
            {
                actionExecuted = true;
                return Task.CompletedTask;
            },
            rateLimitMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => rateLimiter.PerformAsync("Test"));
        Assert.False(actionExecuted, "Action should not be executed if rate limit fails.");
    }


    [Fact]
    public async Task PerformAsync_WithNoRateLimits_ExecutesAction()
    {
        // Arrange
        var actionExecuted = false;
        var rateLimiter = new RateLimiter<string>(
            arg =>
            {
                actionExecuted = true;
                return Task.CompletedTask;
            });

        // Act
        await rateLimiter.PerformAsync("Test");

        // Assert
        Assert.True(actionExecuted, "Action should be executed when no rate limits are provided.");
    }
}