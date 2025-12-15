namespace MT5Bridge.Core.Logging;

/// <summary>
/// Extension methods for ILogger to provide formatted logging.
/// </summary>
public static class LoggerExtensions
{
    /// <summary>
    /// Writes a formatted debug message.
    /// </summary>
    public static void Debug(this ILogger logger, string format, params object[] args)
    {
        logger.Debug(string.Format(format, args));
    }

    /// <summary>
    /// Writes a formatted info message.
    /// </summary>
    public static void Info(this ILogger logger, string format, params object[] args)
    {
        logger.Info(string.Format(format, args));
    }

    /// <summary>
    /// Writes a formatted warning message.
    /// </summary>
    public static void Warn(this ILogger logger, string format, params object[] args)
    {
        logger.Warn(string.Format(format, args));
    }

    /// <summary>
    /// Writes a formatted error message.
    /// </summary>
    public static void Error(this ILogger logger, string format, params object[] args)
    {
        logger.Error(string.Format(format, args));
    }
}
