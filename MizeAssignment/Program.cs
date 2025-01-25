using MizeAssignment.Services;

namespace MizeAssignment;

class Program
{
    static async Task Main(string[] args)
    {
        var dailyLimit = new RateLimit(10, TimeSpan.FromHours(24));
        var minuteLimit = new RateLimit(2, TimeSpan.FromMinutes(1));

        var rateLimiter = new RateLimiter<int>(
            async (number) =>
            {
                Console.WriteLine($"Processing request #{number}");
                await Task.Delay(100);
                Console.WriteLine($"Completed request #{number}");
            },
            dailyLimit,
            minuteLimit
        );

        Console.WriteLine("Testing rate limiter with multiple limits:");
        Console.WriteLine("- 10 requests per 24 hours");
        Console.WriteLine("- 2 requests per minute\n");

        for (int i = 1; i <= 4; i++)
        {
            try
            {
                Console.WriteLine($"\nAttempting request #{i} at {DateTime.Now:HH:mm:ss}");
                await rateLimiter.PerformAsync(i);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Request failed: {ex.Message}");
            }
        }
    }
}