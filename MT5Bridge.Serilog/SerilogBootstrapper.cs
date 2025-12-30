using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Json;
using Serilog.Sinks.AwsCloudWatch;
using Serilog.Sinks.AwsCloudWatch.LogStreamNameProvider;
using Serilog.Sinks.SystemConsole.Themes;
using Amazon;
using Amazon.CloudWatchLogs;
using Amazon.Runtime;
using Microsoft.Extensions.Configuration;

namespace MT5Bridge.Serilog;

/// <summary>
/// Bootstrapper for configuring and creating Serilog instances with support for
/// Console, File (with rolling), and AWS CloudWatch sinks.
/// </summary>
public static class SerilogBootstrapper
{
    /// <summary>
    /// Default output template for plain text logging (used by Console and File sinks).
    /// Format: [Date Time Level] Message (with timestamp precision and exception details)
    /// </summary>
    private const string DefaultOutputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}";

    /// <summary>
    /// CloudWatch client (if enabled). Keep reference for proper disposal.
    /// </summary>
    private static AmazonCloudWatchLogsClient? _cloudWatchClient;

    /// <summary>
    /// Internal logger for diagnostics (if enabled).
    /// </summary>
    private static global::Serilog.ILogger? _internalLogger;

    /// <summary>
    /// Lock object for thread-safe access to static resources.
    /// </summary>
    private static readonly object _lockObject = new();



    /// <summary>
    /// Creates a configured Serilog logger based on the provided configuration.
    /// </summary>
    /// <param name="config">Logger configuration</param>
    /// <returns>Configured ILogger instance</returns>
    /// <exception cref="ArgumentNullException">Thrown if config or sub-configs are null</exception>
    /// <exception cref="InvalidOperationException">Thrown if configuration is invalid</exception>
    public static global::Serilog.ILogger CreateLogger(SerilogConfig config)
    {
        ArgumentNullException.ThrowIfNull(config, nameof(config));

        // Validate sub-configuration objects
        ArgumentNullException.ThrowIfNull(config.Console, nameof(config.Console));
        ArgumentNullException.ThrowIfNull(config.File, nameof(config.File));
        ArgumentNullException.ThrowIfNull(config.CloudWatch, nameof(config.CloudWatch));
        ArgumentNullException.ThrowIfNull(config.Diagnostics, nameof(config.Diagnostics));

        // Validate at least one sink is enabled
        if (!config.Console.Enabled && !config.File.Enabled && !config.CloudWatch.Enabled)
        {
            throw new InvalidOperationException(
                "At least one logging sink must be enabled (Console, File, or CloudWatch)");
        }

        // Configure internal diagnostics
        ConfigureDiagnostics(config.Diagnostics);

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Is(config.MinimumLevel)
            .Enrich.FromLogContext();

        // Add LogLevelEnricher if any sink uses compact or rendered-compact format
        if (IsCompactFormatterUsed(config))
        {
            loggerConfiguration.Enrich.With<LogLevelEnricher>();
        }

        // 1. Console Sink
        if (config.Console.Enabled)
        {
            var consoleMinLevel = config.Console.MinimumLevel ?? config.MinimumLevel;
            var formatter = CreateTextFormatter(config.Console.TextFormatter);
            var consoleTheme = config.Console.UseAnsiColors
                ? SystemConsoleTheme.Literate
                : SystemConsoleTheme.None;

            if (formatter != null)
            {
                // JSON format - formatter doesn't support theme parameter
                loggerConfiguration.WriteTo.Console(
                    formatter: formatter,
                    restrictedToMinimumLevel: consoleMinLevel);
            }
            else
            {
                // Plain text format - supports theme
                loggerConfiguration.WriteTo.Console(
                    restrictedToMinimumLevel: consoleMinLevel,
                    outputTemplate: DefaultOutputTemplate,
                    theme: consoleTheme);
            }
        }

        // 2. File Sink
        if (config.File.Enabled)
        {
            ValidateAndConfigureFileSink(loggerConfiguration, config.File, config.MinimumLevel);
        }

        // 3. CloudWatch Sink
        if (config.CloudWatch.Enabled)
        {
            ConfigureCloudWatchSink(loggerConfiguration, config.CloudWatch, config.MinimumLevel);
        }

        return loggerConfiguration.CreateLogger();
    }

    /// <summary>
    /// Disposes the CloudWatch client if it was created.
    /// Call this when shutting down the application. Thread-safe.
    /// </summary>
    public static void Dispose()
    {
        lock (_lockObject)
        {
            _cloudWatchClient?.Dispose();
            _cloudWatchClient = null;

            if (_internalLogger is IDisposable disposable)
            {
                disposable.Dispose();
            }
            _internalLogger = null;

            global::Serilog.Debugging.SelfLog.Disable();
        }
    }

    /// <summary>
    /// Asynchronously flushes and closes the Serilog logger and disposes internal resources.
    /// This is the recommended way to shut down the logging system to prevent log loss.
    /// </summary>
    public static async Task FlushAndCloseAsync()
    {
        InternalLog("Flushing and closing logger...");
        try
        {
            await Log.CloseAndFlushAsync();
            InternalLog("Logger flushed successfully");
        }
        catch (Exception ex)
        {
            InternalLog($"Flush failed: {ex.Message}", isError: true);
        }

        Dispose();
        // Note: InternalLog cannot be called here as _internalLogger is disposed in Dispose()
    }

    /// <summary>
    /// Gets the CloudWatch client reference (useful for advanced scenarios).
    /// Returns null if CloudWatch sink was not enabled. Thread-safe.
    /// </summary>
    public static AmazonCloudWatchLogsClient? GetCloudWatchClient()
    {
        lock (_lockObject)
        {
            return _cloudWatchClient;
        }
    }

    private static void ValidateAndConfigureFileSink(
        LoggerConfiguration loggerConfiguration,
        SerilogConfig.FileConfig fileConfig,
        LogEventLevel globalMinLevel)
    {
        // Validate path
        ValidateFilePath(fileConfig.Path);

        // Validate RollingInterval
        if (!Enum.TryParse<RollingInterval>(fileConfig.RollingInterval, ignoreCase: true, out var interval))
        {
            var validValues = string.Join(", ", Enum.GetNames(typeof(RollingInterval)));
            throw new InvalidOperationException(
                $"Invalid RollingInterval '{fileConfig.RollingInterval}'. " +
                $"Valid values: {validValues}");
        }

        // Validate file size limit
        if (fileConfig.FileSizeLimitBytes.HasValue && fileConfig.FileSizeLimitBytes.Value <= 0)
        {
            throw new InvalidOperationException(
                $"FileSizeLimitBytes must be positive, got {fileConfig.FileSizeLimitBytes}");
        }

        // Validate retained file count
        if (fileConfig.RetainedFileCountLimit.HasValue && fileConfig.RetainedFileCountLimit.Value < 0)
        {
            throw new InvalidOperationException(
                $"RetainedFileCountLimit must be non-negative, got {fileConfig.RetainedFileCountLimit}");
        }

        // Get effective minimum level for File sink
        var fileMinLevel = fileConfig.MinimumLevel ?? globalMinLevel;
        var formatter = CreateTextFormatter(fileConfig.TextFormatter);

        if (formatter != null)
        {
            // JSON format
            loggerConfiguration.WriteTo.File(
                formatter,
                fileConfig.Path,
                restrictedToMinimumLevel: fileMinLevel,
                fileSizeLimitBytes: fileConfig.FileSizeLimitBytes,
                rollingInterval: interval,
                retainedFileCountLimit: fileConfig.RetainedFileCountLimit);
        }
        else
        {
            // Plain text format
            loggerConfiguration.WriteTo.File(
                fileConfig.Path,
                restrictedToMinimumLevel: fileMinLevel,
                outputTemplate: DefaultOutputTemplate,
                fileSizeLimitBytes: fileConfig.FileSizeLimitBytes,
                rollingInterval: interval,
                retainedFileCountLimit: fileConfig.RetainedFileCountLimit);
        }
    }

    private static void ConfigureCloudWatchSink(
        LoggerConfiguration loggerConfiguration,
        SerilogConfig.CloudWatchConfig cwConfig,
        LogEventLevel globalMinLevel)
    {
        // Validate CloudWatch configuration
        ValidateCloudWatchConfig(cwConfig);

        // Validate and create region
        RegionEndpoint region;
        try
        {
            region = RegionEndpoint.GetBySystemName(cwConfig.Region);
            if (region == null)
            {
                throw new InvalidOperationException(
                    $"Invalid AWS region: '{cwConfig.Region}'. " +
                    $"Check AWS.RegionEndpoint for valid values.");
            }
        }
        catch (Exception ex) when (!(ex is InvalidOperationException))
        {
            throw new InvalidOperationException(
                $"Failed to resolve AWS region '{cwConfig.Region}': {ex.Message}", ex);
        }

        // Create and validate CloudWatch client (thread-safe)
        AmazonCloudWatchLogsClient client;
        try
        {
            // Check if explicit credentials are provided
            bool hasAccessKey = !string.IsNullOrWhiteSpace(cwConfig.AccessKeyId);
            bool hasSecretKey = !string.IsNullOrWhiteSpace(cwConfig.SecretKey);

            // Validate credential configuration
            if (hasAccessKey != hasSecretKey)
            {
                throw new InvalidOperationException(
                    "Both AccessKeyId and SecretKey must be specified together, or both left empty to use default credential chain");
            }

            if (hasAccessKey && hasSecretKey)
            {
                // Use explicit credentials
                var credentials = new BasicAWSCredentials(cwConfig.AccessKeyId, cwConfig.SecretKey);
                client = new AmazonCloudWatchLogsClient(credentials, region);
                InternalLog($"[CloudWatch] Using explicit credentials, Region: {region.SystemName}");
            }
            else
            {
                // Use default AWS credential chain (profile, environment, IAM role)
                client = new AmazonCloudWatchLogsClient(region);
                InternalLog($"[CloudWatch] Using default credential chain, Region: {region.SystemName}");
            }

            InternalLog($"[CloudWatch] LogGroup: {cwConfig.LogGroup}, BatchSize: {cwConfig.BatchSizeLimit}, Period: {cwConfig.PeriodSeconds}s");
        }
        catch (Exception ex)
        {
            InternalLog($"[CloudWatch ERROR] Failed to create client: {ex.Message}", isError: true);
            throw new InvalidOperationException(
                $"Failed to create CloudWatch client for region '{cwConfig.Region}': {ex.Message}", ex);
        }

        // Store client with lock
        lock (_lockObject)
        {
            _cloudWatchClient = client;
        }

        var options = new CloudWatchSinkOptions
        {
            LogGroupName = cwConfig.LogGroup,
            CreateLogGroup = cwConfig.CreateLogGroup,
            LogStreamNameProvider = CreateLogStreamNameProvider(cwConfig),
            TextFormatter = CreateTextFormatter(cwConfig.TextFormatter),
            BatchSizeLimit = cwConfig.BatchSizeLimit,
            Period = TimeSpan.FromSeconds(cwConfig.PeriodSeconds),
            QueueSizeLimit = cwConfig.QueueSizeLimit,
            RetryAttempts = cwConfig.RetryAttempts,
            MinimumLogEventLevel = cwConfig.MinimumLevel ?? globalMinLevel,
        };

        try
        {
            loggerConfiguration.WriteTo.AmazonCloudWatch(options, client);
            InternalLog($"[CloudWatch] Sink configured successfully");
        }
        catch (Exception ex)
        {
            InternalLog($"[CloudWatch ERROR] Failed to configure sink: {ex.Message}", isError: true);
            throw;
        }
    }

    private static ILogStreamNameProvider CreateLogStreamNameProvider(SerilogConfig.CloudWatchConfig cwConfig)
    {
        return cwConfig.LogStreamNamingStrategy.ToLowerInvariant() switch
        {
            "default" => new DefaultLogStreamProvider(),
            "constant" => new ConstantLogStreamNameProvider(cwConfig.LogStreamPrefix),
            "configurable" => new ConfigurableLogStreamNameProvider(
                cwConfig.LogStreamPrefix,
                cwConfig.LogStreamIncludeHostname,
                cwConfig.LogStreamIncludeGuid),
            _ => throw new InvalidOperationException(
                $"Invalid LogStreamNamingStrategy '{cwConfig.LogStreamNamingStrategy}'. " +
                "Valid values: default, constant, configurable")
        };
    }

    /// <summary>
    /// Checks if any sink is configured to use compact or rendered-compact formatter.
    /// </summary>
    private static bool IsCompactFormatterUsed(SerilogConfig config)
    {
        return IsCompactFormat(config.Console.TextFormatter) ||
               IsCompactFormat(config.File.TextFormatter) ||
               IsCompactFormat(config.CloudWatch.TextFormatter);
    }

    /// <summary>
    /// Checks if the formatter name is "compact" or "rendered-compact".
    /// </summary>
    private static bool IsCompactFormat(string? formatterName)
    {
        if (string.IsNullOrEmpty(formatterName))
            return false;

        var name = formatterName.ToLowerInvariant();
        return name == "compact" || name == "rendered-compact";
    }

    /// <summary>
    /// Creates a text formatter for Console/File sinks based on configuration.
    /// Returns null for plain text format, ITextFormatter for JSON formats.
    /// Supports: "plain" (or empty), "json", "compact", "rendered-compact"
    /// </summary>
    private static global::Serilog.Formatting.ITextFormatter? CreateTextFormatter(string formatterName)
    {
        // Empty string or "plain" means no formatter (plain text with template)
        if (string.IsNullOrEmpty(formatterName) || formatterName.Equals("plain", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return formatterName.ToLowerInvariant() switch
        {
            "json" => new JsonFormatter(),
            "compact" => new CompactJsonFormatter(),
            "rendered-compact" => new RenderedCompactJsonFormatter(),
            _ => throw new InvalidOperationException(
                $"Invalid TextFormatter '{formatterName}'. " +
                "Valid values: plain (or empty), json, compact, rendered-compact")
        };
    }

    private static void ValidateCloudWatchConfig(SerilogConfig.CloudWatchConfig cwConfig)
    {
        // Validate Region
        if (string.IsNullOrWhiteSpace(cwConfig.Region))
        {
            throw new InvalidOperationException(
                "CloudWatch.Region cannot be empty or whitespace. Valid examples: us-east-1, eu-west-1, ap-southeast-1");
        }

        // Validate LogGroup
        if (string.IsNullOrWhiteSpace(cwConfig.LogGroup))
        {
            throw new InvalidOperationException("CloudWatch.LogGroup cannot be empty or whitespace");
        }

        if (cwConfig.LogGroup.Length > 256)
        {
            throw new InvalidOperationException(
                $"CloudWatch.LogGroup cannot exceed 256 characters, got {cwConfig.LogGroup.Length}");
        }

        // Validate LogStreamPrefix (may be empty for some naming strategies)
        if (string.IsNullOrWhiteSpace(cwConfig.LogStreamPrefix))
        {
            InternalLog("[CloudWatch WARN] LogStreamPrefix is empty - log stream naming will depend on strategy", isError: false);
        }

        // Validate BatchSizeLimit (AWS API limit: 1-1000)
        if (cwConfig.BatchSizeLimit < 1 || cwConfig.BatchSizeLimit > 1000)
        {
            throw new InvalidOperationException(
                $"CloudWatch.BatchSizeLimit must be between 1 and 1000, got {cwConfig.BatchSizeLimit}");
        }

        // Validate PeriodSeconds (reasonable range: 1-300)
        if (cwConfig.PeriodSeconds < 1 || cwConfig.PeriodSeconds > 300)
        {
            throw new InvalidOperationException(
                $"CloudWatch.PeriodSeconds must be between 1 and 300, got {cwConfig.PeriodSeconds}");
        }

        // Validate LogStreamNamingStrategy
        var validStrategies = new[] { "default", "constant", "configurable" };
        if (!validStrategies.Contains(cwConfig.LogStreamNamingStrategy.ToLowerInvariant()))
        {
            throw new InvalidOperationException(
                $"CloudWatch.LogStreamNamingStrategy must be one of: {string.Join(", ", validStrategies)}, " +
                $"got '{cwConfig.LogStreamNamingStrategy}'");
        }

        // Validate QueueSizeLimit (reasonable range: 100-100000)
        if (cwConfig.QueueSizeLimit < 100 || cwConfig.QueueSizeLimit > 100_000)
        {
            throw new InvalidOperationException(
                $"CloudWatch.QueueSizeLimit must be between 100 and 100000, got {cwConfig.QueueSizeLimit}");
        }
    }

    private static void ValidateFilePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new InvalidOperationException("File.Path cannot be empty or whitespace");
        }

        try
        {
            // Check for invalid characters
            var invalidChars = Path.GetInvalidPathChars();
            if (path.IndexOfAny(invalidChars) >= 0)
            {
                throw new InvalidOperationException(
                    $"File.Path contains invalid characters: '{string.Join("", invalidChars)}'");
            }

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException(
                $"Invalid file path '{path}': {ex.Message}", ex);
        }
        catch (Exception ex) when (!(ex is InvalidOperationException))
        {
            throw new InvalidOperationException(
                $"Cannot create directory for path '{path}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Sliding window for tracking Serilog internal error message timestamps.
    /// Used for throttling to prevent overwhelming diagnostics output.
    /// </summary>
    private static readonly Queue<DateTime> _internalLogTimestamps = new();
    private static int _throttleWindowSeconds = 300;  // Default: 5 minutes
    private static int _throttleLimit = 100;          // Default: 100 messages per window

    private static void ConfigureDiagnostics(SerilogConfig.DiagnosticsConfig config)
    {
        lock (_lockObject)
        {
            // Disable existing SelfLog and dispose previous logger
            global::Serilog.Debugging.SelfLog.Disable();
            if (_internalLogger is IDisposable disposable)
            {
                disposable.Dispose();
            }
            _internalLogger = null;

            if (!config.Console.Enabled && !config.File.Enabled)
            {
                return;
            }

            var internalConfig = new LoggerConfiguration()
                .MinimumLevel.Verbose();

            if (config.Console.Enabled)
            {
                internalConfig.WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} DIAG] {Message:lj}{NewLine}{Exception}");
            }

            if (config.File.Enabled)
            {
                // Ensure directory exists
                var dir = Path.GetDirectoryName(config.File.Path);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                else if (Path.IsPathRooted(config.File.Path))
                {
                    // Path is rooted but has no directory (e.g., "C:\file.log")
                    // Use current directory
                }

                internalConfig.WriteTo.File(
                    config.File.Path,
                    rollingInterval: RollingInterval.Day,
                    fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB
                    retainedFileCountLimit: 7,            // Keep 1 week
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} DIAG] {Message:lj}{NewLine}{Exception}");
            }

            _internalLogger = internalConfig.CreateLogger();

            // Update throttle settings from config
            _throttleWindowSeconds = config.ThrottleWindowSeconds;
            _throttleLimit = config.ThrottleLimit;

            // Pipe Serilog's internal SelfLog to our diagnostic logger
            global::Serilog.Debugging.SelfLog.Enable(msg =>
            {
                // Sliding window throttling: allow N messages per time window
                lock (_lockObject)
                {
                    var now = DateTime.UtcNow;
                    var windowStart = now.AddSeconds(-_throttleWindowSeconds);

                    // Remove timestamps outside the window
                    while (_internalLogTimestamps.Count > 0 && _internalLogTimestamps.Peek() < windowStart)
                    {
                        _internalLogTimestamps.Dequeue();
                    }

                    // Check if we've exceeded the limit
                    if (_internalLogTimestamps.Count >= _throttleLimit)
                    {
                        return; // Throttled
                    }

                    // Record this message
                    _internalLogTimestamps.Enqueue(now);
                }

                _internalLogger?.Warning("[Serilog Internal] {Message}", msg);
            });
        }
    }

    /// <summary>
    /// Logs an internal diagnostic message.
    /// Only logs if internal logger is configured.
    /// Note: This method does NOT apply throttling (only SelfLog messages are throttled).
    /// </summary>
    private static void InternalLog(string message, bool isError = false)
    {
        // If internal logger is not configured, skip logging
        if (_internalLogger == null)
        {
            return;
        }

        if (isError)
        {
            _internalLogger.Error(message);
        }
        else
        {
            _internalLogger.Information(message);
        }
    }
}
