namespace MT5Bridge.Manager;

/// <summary>
/// MT5 connection state
/// </summary>
public enum ConnectionState
{
    /// <summary>
    /// Disconnected
    /// </summary>
    Disconnected,

    /// <summary>
    /// Connecting
    /// </summary>
    Connecting,

    /// <summary>
    /// Connected
    /// </summary>
    Connected,

    /// <summary>
    /// Reconnecting
    /// </summary>
    Reconnecting,

    /// <summary>
    /// Connection failed
    /// </summary>
    Failed
}

/// <summary>
/// Connection state changed event arguments
/// </summary>
public class ConnectionStateChangedEventArgs : EventArgs
{
    public ConnectionState OldState { get; }
    public ConnectionState NewState { get; }
    public string? Message { get; }
    public Exception? Exception { get; }

    public ConnectionStateChangedEventArgs(
        ConnectionState oldState,
        ConnectionState newState,
        string? message = null,
        Exception? exception = null)
    {
        OldState = oldState;
        NewState = newState;
        Message = message;
        Exception = exception;
    }
}
