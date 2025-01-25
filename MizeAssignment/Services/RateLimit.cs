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
            CleanupOldTimestamps(now);
            var count = _requestTimestamps.Count;

            return count >= _maxRequests;
    }

    public async Task EnforceLimitAsync()
    {
        {
            await _lock.WaitAsync();
            try
            {
                DateTime now = DateTime.UtcNow;
                Console.WriteLine($"Enforcing limit at {now:HH:mm:ss.fff}");

                CleanupOldTimestamps(now);
                var currentCount = _requestTimestamps.Count;

                if (currentCount >= _maxRequests)
                {
                    if (_requestTimestamps.TryPeek(out var oldest))
                    {
                        var waitUntil = oldest.Add(_timeWindow);
                        var delay = waitUntil - now;

                        if (delay > TimeSpan.Zero)
                        {
                            Console.WriteLine($"Rate limit reached, Maximum of {_maxRequests} requests allowed per {_timeWindow.TotalMinutes} minute(s).");
                            await Task.Delay(delay);

                            now = DateTime.UtcNow;
                            CleanupOldTimestamps(now);
                            currentCount = _requestTimestamps.Count;

                            if (currentCount >= _maxRequests)
                            {
                                throw new InvalidOperationException($"Rate limit still exceeded after waiting. Try again later.");
                            }
                        }
                    }
                }

                _requestTimestamps.Enqueue(now);
            }
            finally
            {
                _lock.Release();
            }
        }
    }

    private void CleanupOldTimestamps(DateTime now)
    {
        int removedCount = 0;
        while (_requestTimestamps.TryPeek(out var oldest) &&
              (now - oldest) > _timeWindow)
        {
            _requestTimestamps.TryDequeue(out _);
            removedCount++;
        }
        if (removedCount > 0)
        {
            Console.WriteLine($"Cleaned up {removedCount} old timestamps");
        }
    }
}

