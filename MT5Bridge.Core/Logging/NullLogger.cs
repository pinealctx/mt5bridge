namespace MT5Bridge.Core.Logging;

/// <summary>
/// A logger implementation that does nothing. Useful for testing or when logging is disabled.
/// </summary>
public class NullLogger : ILogger
{
    public static readonly NullLogger Instance = new();

    private NullLogger() { }

    public void Debug(string message) { }

    public void Info(string message) { }

    public void Warn(string message) { }

    public void Error(string message) { }

    public void Error(string message, Exception exception) { }
}

/// <summary>
/// Factory that creates NullLogger instances.
/// </summary>
public class NullLoggerFactory : ILoggerFactory
{
    public static readonly NullLoggerFactory Instance = new();

    private NullLoggerFactory() { }

    public ILogger CreateLogger(string name) => NullLogger.Instance;
}
