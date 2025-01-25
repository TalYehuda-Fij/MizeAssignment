using MizeAssignment.Services;

namespace MizeAssignment;

class Program
{
    static async Task Main(string[] args)
    {
        // Test 1: Process strings with different lengths
        var stringRateLimit = new RateLimit(2, TimeSpan.FromSeconds(5));
        var stringLimiter = new RateLimiter<string>(
            ProcessString,
            stringRateLimit
        );

        Console.WriteLine("\n=== Test 1: Processing Strings ===");
        await TestStrings(stringLimiter);

        // Test 2: Process custom object
        var objectRateLimit = new RateLimit(3, TimeSpan.FromSeconds(10));
        var objectLimiter = new RateLimiter<TestObject>(
            ProcessObject,
            objectRateLimit
        );

        Console.WriteLine("\n=== Test 2: Processing Objects ===");
        await TestObjects(objectLimiter);

        Console.WriteLine("\nAll tests completed. Press any key to exit.");
        Console.ReadKey();
    }

    private static async Task TestStrings(RateLimiter<string> limiter)
    {
        var strings = new[] { "Short", "Medium length string", "This is a very long string to process" };

        foreach (var str in strings)
        {
            try
            {
                Console.WriteLine($"\nAttempting to process: '{str}'");
                await limiter.PerformAsync(str);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing string: {ex.Message}");
            }
            await Task.Delay(1000); // Wait 1 second between attempts
        }
    }

    private static async Task TestObjects(RateLimiter<TestObject> limiter)
    {
        var objects = new[]
        {
            new TestObject { Id = 1, Name = "First" },
            new TestObject { Id = 2, Name = "Second" },
            new TestObject { Id = 3, Name = "Third" },
            new TestObject { Id = 4, Name = "Fourth" }
        };

        foreach (var obj in objects)
        {
            try
            {
                Console.WriteLine($"\nAttempting to process object: Id={obj.Id}, Name='{obj.Name}'");
                await limiter.PerformAsync(obj);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing object: {ex.Message}");
            }
            await Task.Delay(2000); // Wait 2 seconds between attempts
        }
    }

    // Processing functions
    private static async Task ProcessString(string text)
    {
        Console.WriteLine($"Processing string of length {text.Length}...");
        await Task.Delay(500); // Simulate processing time
        Console.WriteLine($"Completed processing string: '{text}'");
    }

    private static async Task ProcessObject(TestObject obj)
    {
        Console.WriteLine($"Processing object {obj.Id}...");
        await Task.Delay(1000); // Simulate processing time
        Console.WriteLine($"Completed processing object: Id={obj.Id}, Name='{obj.Name}'");
    }
}

class TestObject
{
    public int Id { get; set; }
    public string Name { get; set; }
}