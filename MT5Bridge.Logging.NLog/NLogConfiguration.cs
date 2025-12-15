using NLog;
using NLog.Config;
using NLog.Targets;

namespace MT5Bridge.Logging.NLog;

/// <summary>
/// Provides convenient configuration methods for NLog.
/// </summary>
public static class NLogConfiguration
{
    /// <summary>
    /// Configures NLog with console output using a standard format.
    /// </summary>
    public static void ConfigureConsole()
    {
        var config = new LoggingConfiguration();

        var consoleTarget = new ConsoleTarget("console")
        {
            Layout = "${longdate} ${level:uppercase=true} ${logger} - ${message} ${exception:format=tostring}"
        };

        config.AddTarget(consoleTarget);
        config.AddRule(LogLevel.Debug, LogLevel.Fatal, consoleTarget);

        LogManager.Configuration = config;
    }

    /// <summary>
    /// Configures NLog with file output.
    /// </summary>
    /// <param name="filePath">Path to the log file.</param>
    public static void ConfigureFile(string filePath)
    {
        var config = new LoggingConfiguration();

        var fileTarget = new FileTarget("file")
        {
            FileName = filePath,
            Layout = "${longdate} ${level:uppercase=true} ${logger} - ${message} ${exception:format=tostring}"
        };

        config.AddTarget(fileTarget);
        config.AddRule(LogLevel.Debug, LogLevel.Fatal, fileTarget);

        LogManager.Configuration = config;
    }

    /// <summary>
    /// Configures NLog with both console and file output.
    /// </summary>
    public static void ConfigureConsoleAndFile(string filePath)
    {
        var config = new LoggingConfiguration();

        var consoleTarget = new ConsoleTarget("console")
        {
            Layout = "${longdate} ${level:uppercase=true} ${logger} - ${message} ${exception:format=tostring}"
        };

        var fileTarget = new FileTarget("file")
        {
            FileName = filePath,
            Layout = "${longdate} ${level:uppercase=true} ${logger} - ${message} ${exception:format=tostring}"
        };

        config.AddTarget(consoleTarget);
        config.AddTarget(fileTarget);
        config.AddRule(LogLevel.Debug, LogLevel.Fatal, consoleTarget);
        config.AddRule(LogLevel.Debug, LogLevel.Fatal, fileTarget);

        LogManager.Configuration = config;
    }
}
