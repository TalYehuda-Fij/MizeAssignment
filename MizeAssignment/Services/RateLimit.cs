using System.Collections.Concurrent;

namespace MizeAssignment.Services;

public class RateLimit : IRateLimit
{
    private readonly int _maxRequests;
    private readonly TimeSpan _timeWindow;
    private readonly ConcurrentQueue<DateTime> _requestTimestamps = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public RateLimit(int maxRequests, TimeSpan timeWindow)
    {
        _maxRequests = maxRequests;
        _timeWindow = timeWindow;
    }

    public bool ShouldWait()
    {
        DateTime now = DateTime.UtcNow;

        // Remove timestamps outside of the time window
        while (_requestTimestamps.TryPeek(out var oldest) && (now - oldest) > _timeWindow)
        {
            _requestTimestamps.TryDequeue(out _);
        }

        // Check if current requests exceed the limit
        return _requestTimestamps.Count >= _maxRequests;
    }

    public async Task EnforceLimitAsync()
    {
        await _lock.WaitAsync();
        try
        {
            DateTime now = DateTime.UtcNow;

            // Remove timestamps outside of the time window
            while (_requestTimestamps.TryPeek(out var oldest) && (now - oldest) > _timeWindow)
            {
                _requestTimestamps.TryDequeue(out _);
            }

            // If limit exceeded, calculate the delay based on the sliding window
            if (_requestTimestamps.Count >= _maxRequests && _requestTimestamps.TryPeek(out var nextAllowed))
            {
                TimeSpan delay = _timeWindow - (now - nextAllowed);
                await Task.Delay(delay);
            }

            // Record the current timestamp
            _requestTimestamps.Enqueue(now);
        }
        finally
        {
            _lock.Release();
        }
    }
}
