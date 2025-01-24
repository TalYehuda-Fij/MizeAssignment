namespace MizeAssignment.Services;

public interface IRateLimiter<TArgument>
{
    public Task PerformAsync(TArgument argument);
}
