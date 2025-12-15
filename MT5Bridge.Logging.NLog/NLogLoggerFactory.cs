using NLog;
using CoreLogger = MT5Bridge.Core.Logging.ILogger;
using CoreLoggerFactory = MT5Bridge.Core.Logging.ILoggerFactory;

namespace MT5Bridge.Logging.NLog;

/// <summary>
/// Factory for creating NLog-based loggers.
/// </summary>
public class NLogLoggerFactory : CoreLoggerFactory
{
    /// <summary>
    /// Creates a logger with the specified name using NLog.
    /// </summary>
    /// <param name="name">The name of the logger (typically a class or category name).</param>
    /// <returns>An ILogger instance backed by NLog.</returns>
    public CoreLogger CreateLogger(string name)
    {
        var nlogger = LogManager.GetLogger(name);
        return new NLogAdapter(nlogger);
    }

    /// <summary>
    /// Creates a logger for a specific type.
    /// </summary>
    public CoreLogger CreateLogger<T>()
    {
        return CreateLogger(typeof(T).FullName ?? typeof(T).Name);
    }
}
