namespace MizeAssignment.Services;

public interface IRateLimit
{
    public bool ShouldWait();
    public Task EnforceLimitAsync();
}
