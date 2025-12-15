namespace MT5Bridge.Core.Logging;

/// <summary>
/// Factory for creating logger instances.
/// </summary>
public interface ILoggerFactory
{
    /// <summary>
    /// Creates a logger with the specified name.
    /// </summary>
    /// <param name="name">The name of the logger (typically class or category name).</param>
    ILogger CreateLogger(string name);
}
