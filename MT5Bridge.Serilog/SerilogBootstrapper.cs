using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Sinks.AwsCloudWatch;
using Serilog.Sinks.AwsCloudWatch.LogStreamNameProvider;
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
    /// CloudWatch client (if enabled). Keep reference for proper disposal.
    /// </summary>
    private static AmazonCloudWatchLogsClient? _cloudWatchClient;

    /// <summary>
    /// Lock object for thread-safe access to CloudWatch client.
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

        // Validate at least one sink is enabled
        if (!config.Console.Enabled && !config.File.Enabled && !config.CloudWatch.Enabled)
        {
            throw new InvalidOperationException(
                "At least one logging sink must be enabled (Console, File, or CloudWatch)");
        }

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Is(config.MinimumLevel)
            .Enrich.FromLogContext();

        // 1. Console Sink
        if (config.Console.Enabled)
        {
            if (config.Console.UseJson)
            {
                loggerConfiguration.WriteTo.Console(new CompactJsonFormatter());
            }
            else
            {
                loggerConfiguration.WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");
            }
        }

        // 2. File Sink
        if (config.File.Enabled)
        {
            ValidateAndConfigureFileSink(loggerConfiguration, config.File);
        }

        // 3. CloudWatch Sink
        if (config.CloudWatch.Enabled)
        {
            ConfigureCloudWatchSink(loggerConfiguration, config.CloudWatch);
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
        }
    }

    /// <summary>
    /// Asynchronously flushes and closes the Serilog logger and disposes internal resources.
    /// This is the recommended way to shut down the logging system to prevent log loss.
    /// </summary>
    public static async Task FlushAndCloseAsync()
    {
        Dispose();
        await Log.CloseAndFlushAsync();
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
        SerilogConfig.FileConfig fileConfig)
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

        if (fileConfig.UseJson)
        {
            loggerConfiguration.WriteTo.File(
                new CompactJsonFormatter(),
                fileConfig.Path,
                rollingInterval: interval,
                fileSizeLimitBytes: fileConfig.FileSizeLimitBytes,
                retainedFileCountLimit: fileConfig.RetainedFileCountLimit);
        }
        else
        {
            loggerConfiguration.WriteTo.File(
                fileConfig.Path,
                rollingInterval: interval,
                fileSizeLimitBytes: fileConfig.FileSizeLimitBytes,
                retainedFileCountLimit: fileConfig.RetainedFileCountLimit,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}");
        }
    }

    private static void ConfigureCloudWatchSink(
        LoggerConfiguration loggerConfiguration,
        SerilogConfig.CloudWatchConfig cwConfig)
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
            }
            else
            {
                // Use default AWS credential chain (profile, environment, IAM role)
                client = new AmazonCloudWatchLogsClient(region);
            }
        }
        catch (Exception ex)
        {
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
            LogStreamNameProvider = new ConfigurableLogStreamNameProvider(cwConfig.LogStreamPrefix),
            BatchSizeLimit = cwConfig.BatchSizeLimit,
            Period = TimeSpan.FromSeconds(cwConfig.PeriodSeconds)
        };

        // Apply JSON formatter based on UseJson configuration
        if (cwConfig.UseJson)
        {
            options.TextFormatter = new CompactJsonFormatter();
        }

        loggerConfiguration.WriteTo.AmazonCloudWatch(options, client);
    }

    private static void ValidateCloudWatchConfig(SerilogConfig.CloudWatchConfig cwConfig)
    {
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

        // Validate LogStreamPrefix
        if (string.IsNullOrWhiteSpace(cwConfig.LogStreamPrefix))
        {
            throw new InvalidOperationException("CloudWatch.LogStreamPrefix cannot be empty or whitespace");
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
}
