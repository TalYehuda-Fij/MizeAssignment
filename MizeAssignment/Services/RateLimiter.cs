namespace MizeAssignment.Services;

public class RateLimiter<T> : IRateLimiter<T>
{
    private readonly Func<T, Task> _actionToPerform;
    private readonly List<IRateLimit> _rateLimits;

    public RateLimiter(Func<T, Task> actionToPerform, params IRateLimit[] rateLimits)
    {
        _actionToPerform = actionToPerform;
        _rateLimits = new List<IRateLimit>(rateLimits);
    }

    public async Task PerformAsync(T argument)
    {
        IEnumerable<IRateLimit> limitsToWait = _rateLimits.Where(rl => rl.ShouldWait());
        // Check if any rate limit requires waiting before proceeding
        if (limitsToWait.Any())
        {
            var waitingTasks = limitsToWait.Select(rl => rl.EnforceLimitAsync());
            await Task.WhenAll(waitingTasks);
        }

        // Perform the main action after all rate limits are satisfied
        await _actionToPerform(argument);
    }
}