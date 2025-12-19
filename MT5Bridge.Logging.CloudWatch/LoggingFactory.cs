namespace MT5Bridge.Logging.CloudWatch;

using System;
using global::NLog;
using global::NLog.Config;
using global::NLog.Targets;

/// <summary>
/// Logging factory for configuring NLog with pluggable targets
/// Similar to Log4j's factory pattern
/// </summary>
public class LoggingFactory
{
    private LoggingConfiguration _config;
    private bool _configured;

    public LoggingFactory()
    {
        _config = new LoggingConfiguration();
        _configured = false;
    }

    /// <summary>
    /// Add Console target
    /// </summary>
    /// <param name="minLevel">Minimum log level (default: Debug)</param>
    /// <param name="layout">Optional layout pattern</param>
    public LoggingFactory AddConsole(LogLevel? minLevel = null, string? layout = null)
    {
        var target = new ConsoleTarget("console");

        if (!string.IsNullOrEmpty(layout))
        {
            target.Layout = layout;
        }

        _config.AddRule(minLevel ?? LogLevel.Debug, LogLevel.Fatal, target);
        return this;
    }

    /// <summary>
    /// Add File target with optional rolling configuration
    /// </summary>
    /// <param name="fileName">Log file path (supports ${date} placeholders)</param>
    /// <param name="minLevel">Minimum log level (default: Debug)</param>
    /// <param name="archiveAboveSize">Archive file when it exceeds this size (default: 10MB)</param>
    /// <param name="maxArchiveFiles">Maximum number of archive files (default: 10)</param>
    /// <param name="layout">Optional layout pattern</param>
    public LoggingFactory AddFile(
        string fileName,
        LogLevel? minLevel = null,
        long archiveAboveSize = 10 * 1024 * 1024,
        int maxArchiveFiles = 10,
        string? layout = null)
    {
        var target = new FileTarget("file")
        {
            FileName = fileName,
            ArchiveAboveSize = archiveAboveSize,
            MaxArchiveFiles = maxArchiveFiles
        };

        if (!string.IsNullOrEmpty(layout))
        {
            target.Layout = layout;
        }

        _config.AddRule(minLevel ?? LogLevel.Debug, LogLevel.Fatal, target);
        return this;
    }

    /// <summary>
    /// Add AWS CloudWatch target from configuration file
    /// </summary>
    /// <param name="configFilePath">Path to awslog_config.json</param>
    /// <param name="streamNameSuffix">Optional suffix for log stream (e.g., machine name)</param>
    /// <param name="minLevel">Minimum log level (default: Debug)</param>
    public LoggingFactory AddCloudWatch(
        string configFilePath = "awslog_config.json",
        string? streamNameSuffix = null,
        LogLevel? minLevel = null)
    {
        // First apply current configuration
        if (!_configured)
        {
            LogManager.Configuration = _config;
            _configured = true;
        }

        // Add CloudWatch target (it will merge with existing config)
        var (success, message) = CloudWatchNLogConfiguration.ConfigureNLog(configFilePath, streamNameSuffix);

        if (!success)
        {
            throw new InvalidOperationException($"Failed to configure CloudWatch: {message}");
        }

        // Update reference to merged configuration
        _config = LogManager.Configuration ?? _config;
        return this;
    }

    /// <summary>
    /// Add AWS CloudWatch target from configuration object
    /// </summary>
    /// <param name="config">CloudWatch configuration object</param>
    /// <param name="minLevel">Minimum log level (default: Debug)</param>
    public LoggingFactory AddCloudWatch(CloudWatchConfig config, LogLevel? minLevel = null)
    {
        // First apply current configuration
        if (!_configured)
        {
            LogManager.Configuration = _config;
            _configured = true;
        }

        // Add CloudWatch target (it will merge with existing config)
        var (success, message) = CloudWatchNLogConfiguration.ConfigureNLog(config);

        if (!success)
        {
            throw new InvalidOperationException($"Failed to configure CloudWatch: {message}");
        }

        // Update reference to merged configuration
        _config = LogManager.Configuration ?? _config;
        return this;
    }

    /// <summary>
    /// Add Database target (example for extensibility)
    /// Note: Requires NLog.Database package to be installed
    /// </summary>
    /// <param name="connectionString">Database connection string</param>
    /// <param name="tableName">Log table name</param>
    /// <param name="minLevel">Minimum log level (default: Debug)</param>
    public LoggingFactory AddDatabase(
        string connectionString,
        string tableName = "Logs",
        LogLevel? minLevel = null)
    {
        // Note: DatabaseTarget requires NLog.Database package
        // This is a placeholder method to show extensibility
        // To use: dotnet add package NLog.Database

        throw new NotSupportedException(
            "DatabaseTarget requires NLog.Database package. " +
            "Install with: dotnet add package NLog.Database\n" +
            "Then uncomment the implementation in LoggingFactory.cs");

        // Uncomment when NLog.Database is installed:
        // var target = new DatabaseTarget("database")
        // {
        //     ConnectionString = connectionString,
        //     CommandText = $"INSERT INTO {tableName} (Timestamp, Level, Message) VALUES (@timestamp, @level, @message)"
        // };
        // target.Parameters.Add(new DatabaseParameterInfo("@timestamp", "${longdate}"));
        // target.Parameters.Add(new DatabaseParameterInfo("@level", "${level}"));
        // target.Parameters.Add(new DatabaseParameterInfo("@message", "${message}"));
        // _config.AddRule(minLevel ?? LogLevel.Debug, LogLevel.Fatal, target);
        // return this;
    }

    /// <summary>
    /// Add custom NLog target
    /// </summary>
    /// <param name="targetName">Target name</param>
    /// <param name="target">Custom target instance</param>
    /// <param name="minLevel">Minimum log level (default: Debug)</param>
    public LoggingFactory AddTarget(string targetName, Target target, LogLevel? minLevel = null)
    {
        target.Name = targetName;
        _config.AddRule(minLevel ?? LogLevel.Debug, LogLevel.Fatal, target);
        return this;
    }

    /// <summary>
    /// Build and apply the configuration
    /// </summary>
    /// <returns>Configured ILogger instance</returns>
    public Logger Build(string? loggerName = null)
    {
        if (!_configured)
        {
            LogManager.Configuration = _config;
            _configured = true;
        }

        return string.IsNullOrEmpty(loggerName)
            ? LogManager.GetCurrentClassLogger()
            : LogManager.GetLogger(loggerName);
    }

    /// <summary>
    /// Build and get MT5Bridge.Core.Logging ILogger
    /// Note: Requires MT5Bridge.Logging.NLog package reference
    /// </summary>
    /// <param name="categoryName">Logger category name</param>
    /// <returns>Core ILogger instance</returns>
    public MT5Bridge.Core.Logging.ILogger BuildCoreLogger(string categoryName)
    {
        if (!_configured)
        {
            LogManager.Configuration = _config;
            _configured = true;
        }

        // Requires project reference to MT5Bridge.Logging.NLog
        // Use reflection to avoid hard dependency
        var factoryType = Type.GetType("MT5Bridge.Logging.NLog.NLogLoggerFactory, MT5Bridge.Logging.NLog");
        if (factoryType == null)
        {
            throw new InvalidOperationException(
                "MT5Bridge.Logging.NLog package not found. " +
                "Add project reference to use BuildCoreLogger.");
        }

        var factory = (MT5Bridge.Core.Logging.ILoggerFactory)Activator.CreateInstance(factoryType)!;
        return factory.CreateLogger(categoryName);
    }

    /// <summary>
    /// Load configuration from NLog.config file
    /// </summary>
    /// <param name="configFilePath">Path to NLog.config</param>
    public static Logger FromConfigFile(string configFilePath = "NLog.config")
    {
        LogManager.Configuration = new XmlLoggingConfiguration(configFilePath);
        return LogManager.GetCurrentClassLogger();
    }

    /// <summary>
    /// Create a simple console logger (quick start)
    /// </summary>
    public static Logger CreateConsoleLogger(LogLevel? minLevel = null)
    {
        return new LoggingFactory()
            .AddConsole(minLevel)
            .Build();
    }

    /// <summary>
    /// Create a simple file logger (quick start)
    /// </summary>
    public static Logger CreateFileLogger(string fileName, LogLevel? minLevel = null)
    {
        return new LoggingFactory()
            .AddFile(fileName, minLevel)
            .Build();
    }
}
