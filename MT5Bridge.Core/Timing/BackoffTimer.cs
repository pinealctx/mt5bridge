namespace MT5Bridge.Core.Timing;

public class BackoffTimer
{
    private readonly object _lock = new();
    private readonly double _initialIntervalMs;
    private readonly double _maxIntervalMs;
    private readonly double _multiplier;

    private double _currentIntervalMs;
    private long _lastTimeMs;

    public BackoffTimer(double initialIntervalMs, double maxIntervalMs, double multiplier = 2.0)
    {
        if (initialIntervalMs <= 0)
            throw new ArgumentException("Initial interval must be positive", nameof(initialIntervalMs));
        if (maxIntervalMs <= 0)
            throw new ArgumentException("Max interval must be positive", nameof(maxIntervalMs));
        if (multiplier <= 1.0)
            throw new ArgumentException("Multiplier must be greater than 1.0", nameof(multiplier));

        _initialIntervalMs = initialIntervalMs;
        _maxIntervalMs = maxIntervalMs;
        _multiplier = multiplier;
        Reset();
    }

    public bool CanExecute()
    {
        lock (_lock)
        {
            long currentTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (currentTimeMs - _lastTimeMs >= _currentIntervalMs)
            {
                _lastTimeMs = currentTimeMs;
                _currentIntervalMs = Math.Min(_currentIntervalMs * _multiplier, _maxIntervalMs);
                return true;
            }

            return false;
        }
    }

    public long GetRemainingWaitTimeMs()
    {
        lock (_lock)
        {
            long currentTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long elapsed = currentTimeMs - _lastTimeMs;
            long remaining = (long)_currentIntervalMs - elapsed;
            return Math.Max(0, remaining);
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            _currentIntervalMs = _initialIntervalMs;
            _lastTimeMs = 0;
        }
    }

    public double CurrentIntervalMs
    {
        get
        {
            lock (_lock)
            {
                return _currentIntervalMs;
            }
        }
    }
}
