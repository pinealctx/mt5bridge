using MetaQuotes.MT5CommonAPI;

namespace MT5Bridge.Manager.Reconnection;

/// <summary>
/// Reconnect strategy interface
/// </summary>
public interface IReconnectStrategy
{
    /// <summary>
    /// Calculate next reconnect delay in milliseconds
    /// </summary>
    /// <param name="attemptNumber">Attempt number (starting from 1)</param>
    /// <param name="lastError">Last error code from connection attempt</param>
    /// <returns>Delay in milliseconds, or -1 to give up</returns>
    int GetNextDelayMs(int attemptNumber, MTRetCode lastError);

    /// <summary>
    /// Check if this error should trigger reconnection or give up
    /// </summary>
    /// <param name="lastError">Error code to check</param>
    /// <returns>true to retry, false to give up permanently</returns>
    bool ShouldRetry(MTRetCode lastError);

    /// <summary>
    /// Reset strategy state after successful connection
    /// </summary>
    void Reset();
}

/// <summary>
/// Fixed interval reconnect strategy
/// </summary>
public class FixedIntervalReconnectStrategy : IReconnectStrategy
{
    private readonly int _intervalMs;

    /// <summary>
    /// Create fixed interval strategy
    /// </summary>
    /// <param name="intervalMs">Fixed interval in milliseconds (default: 5000)</param>
    public FixedIntervalReconnectStrategy(int intervalMs = 5000)
    {
        if (intervalMs < 0)
            throw new ArgumentException("Interval must be >= 0", nameof(intervalMs));
        _intervalMs = intervalMs;
    }

    public int GetNextDelayMs(int attemptNumber, MTRetCode lastError) => _intervalMs;

    public bool ShouldRetry(MTRetCode lastError)
    {
        // Whitelist Strategy: Only retry for explicitly transient errors
        return ErrorAnalyzer.IsTransientError(lastError);
    }

    public void Reset() { }
}

/// <summary>
/// Exponential backoff reconnect strategy (recommended)
/// 2x increase per attempt until max, then stay at max
/// </summary>
public class ExponentialBackoffReconnectStrategy : IReconnectStrategy
{
    private readonly int _initialDelayMs;
    private readonly int _maxDelayMs;

    /// <summary>
    /// Create exponential backoff strategy
    /// </summary>
    /// <param name="initialDelayMs">Initial delay in milliseconds (default: 100)</param>
    /// <param name="maxDelayMs">Maximum delay in milliseconds (default: 30000)</param>
    public ExponentialBackoffReconnectStrategy(
        int initialDelayMs = 100,
        int maxDelayMs = 30000)
    {
        if (initialDelayMs < 0)
            throw new ArgumentException("Initial delay must be >= 0", nameof(initialDelayMs));
        if (maxDelayMs < 0)
            throw new ArgumentException("Max delay must be >= 0", nameof(maxDelayMs));
        if (maxDelayMs < initialDelayMs)
            throw new ArgumentException("Max delay must be >= initial delay", nameof(maxDelayMs));

        _initialDelayMs = initialDelayMs;
        _maxDelayMs = maxDelayMs;
    }

    public int GetNextDelayMs(int attemptNumber, MTRetCode lastError)
    {
        // 2x exponential backoff: 100 -> 200 -> 400 -> 800 ...
        // Cap at maxDelayMs
        double delay = _initialDelayMs * Math.Pow(2, attemptNumber - 1);
        return (int)Math.Min(delay, _maxDelayMs);
    }

    public bool ShouldRetry(MTRetCode lastError)
    {
        // Whitelist Strategy: Only retry for explicitly transient errors
        return ErrorAnalyzer.IsTransientError(lastError);
    }

    public void Reset() { }
}

/// <summary>
/// Reconnection decision (for logging/monitoring)
/// </summary>
public enum ReconnectDecision
{
    /// <summary>
    /// Retry with backoff delay
    /// </summary>
    RetryWithBackoff,

    /// <summary>
    /// Don't retry (permanent error)
    /// </summary>
    GiveUp
}
