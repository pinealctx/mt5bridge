namespace MT5Bridge.Core.Logging;

/// <summary>
/// Represents a logger that can write log messages at different severity levels.
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Writes a debug-level log message.
    /// </summary>
    void Debug(string message);

    /// <summary>
    /// Writes an informational log message.
    /// </summary>
    void Info(string message);

    /// <summary>
    /// Writes a warning log message.
    /// </summary>
    void Warn(string message);

    /// <summary>
    /// Writes an error log message.
    /// </summary>
    void Error(string message);

    /// <summary>
    /// Writes an error log message with an exception.
    /// </summary>
    void Error(string message, Exception exception);
}
