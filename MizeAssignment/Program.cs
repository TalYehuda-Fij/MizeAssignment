using MizeAssignment.Services;

namespace MizeAssignment;

public class Program
{
    public static void Main(string[] args)
    {
        var rateLimiter = new RateLimiter<string>(
            async argument =>
            {
                Console.WriteLine($"Executing: {argument} at {DateTime.UtcNow}");
                await Task.Delay(50); // Simulated work
            },
        new RateLimit(10, TimeSpan.FromSeconds(1)),
        new RateLimit(100, TimeSpan.FromMinutes(1))
    );

        var tasks = Enumerable.Range(1, 50).Select(i => rateLimiter.PerformAsync($"Task {i}"));
        Task.WhenAll(tasks).GetAwaiter().GetResult();

    }
}
