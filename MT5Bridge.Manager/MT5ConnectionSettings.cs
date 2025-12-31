using MT5Bridge.Manager.Reconnection;

namespace MT5Bridge.Manager;

/// <summary>
/// MT5 connection settings
/// </summary>
public class MT5ConnectionSettings
{
    /// <summary>
    /// Server address (e.g., "localhost:443" or "192.168.1.100:443")
    /// </summary>
    public required string Server { get; set; }

    /// <summary>
    /// Manager login (account number)
    /// </summary>
    public required ulong Login { get; set; }

    /// <summary>
    /// Manager password
    /// </summary>
    public required string Password { get; set; }

    /// <summary>
    /// Connection timeout in milliseconds (default: 30000)
    /// </summary>
    public uint TimeoutMs { get; set; } = 30000;

    /// <summary>
    /// Pump mode for real-time updates
    /// </summary>
    public PumpMode PumpMode { get; set; } = PumpMode.Full;

    // ===== Reconnection Configuration =====

    /// <summary>
    /// Reconnect delay in milliseconds
    /// null or 0 = no reconnect (disabled)
    /// with ReconnectMaxDelayMs = null or 0: fixed interval reconnect
    /// with ReconnectMaxDelayMs > 0: exponential backoff reconnect
    /// (default: 100)
    /// </summary>
    public int? ReconnectDelayMs { get; set; } = 100;

    /// <summary>
    /// Maximum reconnect delay in milliseconds (for exponential backoff only)
    /// null or 0 = fixed interval strategy using ReconnectDelayMs
    /// with ReconnectDelayMs > 0 and ReconnectMaxDelayMs > 0: exponential backoff from ReconnectDelayMs to ReconnectMaxDelayMs
    /// (default: 30000)
    /// </summary>
    public int? ReconnectMaxDelayMs { get; set; } = 30000;

    /// <summary>
    /// Get or create reconnection strategy based on configuration
    /// </summary>
    /// <returns>Reconnect strategy instance, or null if reconnection disabled</returns>
    public IReconnectStrategy? GetReconnectStrategy()
    {
        // No reconnect if ReconnectDelayMs is null or 0
        if (ReconnectDelayMs == null || ReconnectDelayMs == 0)
            return null;

        // Fixed interval: ReconnectMaxDelayMs is null or 0
        if (ReconnectMaxDelayMs == null || ReconnectMaxDelayMs == 0)
            return new FixedIntervalReconnectStrategy(ReconnectDelayMs.Value);

        // Exponential backoff: both delay values > 0
        return new ExponentialBackoffReconnectStrategy(
            initialDelayMs: ReconnectDelayMs.Value,
            maxDelayMs: ReconnectMaxDelayMs.Value);
    }

}

/// <summary>
/// Pump mode for MT5 connection
/// </summary>
public enum PumpMode
{
    /// <summary>
    /// No real-time updates
    /// </summary>
    None = 0,

    /// <summary>
    /// Full real-time updates
    /// </summary>
    Full = 1,

    /// <summary>
    /// Symbols only
    /// </summary>
    Symbols = 2
}
