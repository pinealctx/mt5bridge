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

    /// <summary>
    /// Enable auto-reconnect
    /// </summary>
    public bool AutoReconnect { get; set; } = true;

    /// <summary>
    /// Reconnect interval in milliseconds
    /// </summary>
    public int ReconnectIntervalMs { get; set; } = 5000;

    /// <summary>
    /// Maximum reconnect attempts (0 = infinite)
    /// </summary>
    public int MaxReconnectAttempts { get; set; } = 0;
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
