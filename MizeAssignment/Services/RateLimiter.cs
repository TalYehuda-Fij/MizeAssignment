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
        foreach (var rateLimit in _rateLimits)
        {
            await rateLimit.EnforceLimitAsync();
        }

        await _actionToPerform(argument);
    }
}
