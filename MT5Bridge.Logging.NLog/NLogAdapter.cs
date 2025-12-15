using NLog;
using CoreLogger = MT5Bridge.Core.Logging.ILogger;

namespace MT5Bridge.Logging.NLog;

/// <summary>
/// NLog implementation of MT5Bridge.Core.Logging.ILogger.
/// </summary>
public class NLogAdapter : CoreLogger
{
    private readonly Logger _logger;

    public NLogAdapter(Logger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Debug(string message)
    {
        _logger.Debug(message);
    }

    public void Info(string message)
    {
        _logger.Info(message);
    }

    public void Warn(string message)
    {
        _logger.Warn(message);
    }

    public void Error(string message)
    {
        _logger.Error(message);
    }

    public void Error(string message, Exception exception)
    {
        _logger.Error(exception, message);
    }
}
