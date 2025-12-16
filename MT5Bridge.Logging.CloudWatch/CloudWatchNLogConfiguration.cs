namespace MT5Bridge.Logging.CloudWatch;

using System;
using System.IO;
using System.Text.Json;
using global::NLog;
using global::NLog.Common;
using global::NLog.Config;

/// <summary>
/// AWS CloudWatch NLog configuration helper
/// </summary>
public static class CloudWatchNLogConfiguration
{
    private const string DefaultConfigFileName = "awslog_config.json";

    /// <summary>
    /// Configure NLog CloudWatch Target from configuration file
    /// </summary>
    /// <param name="configFilePath">Configuration file path, defaults to awslog_config.json</param>
    /// <param name="streamNameSuffix">Optional log stream name suffix to distinguish different instances</param>
    /// <returns>Configuration result and error message</returns>
    public static (bool Success, string Message) ConfigureNLog(
        string? configFilePath = null,
        string? streamNameSuffix = null)
    {
        var configFile = configFilePath ?? DefaultConfigFileName;

        try
        {
            if (!File.Exists(configFile))
            {
                return (false, $"Configuration file not found: {configFile}");
            }

            var json = File.ReadAllText(configFile);
            var cloudWatchConfig = JsonSerializer.Deserialize<CloudWatchConfig>(json);

            if (cloudWatchConfig == null)
            {
                return (false, "Failed to deserialize CloudWatch configuration");
            }

            if (!cloudWatchConfig.Enabled)
            {
                return (false, "AWS CloudWatch logging is disabled");
            }

            var (isValid, errorMessage) = cloudWatchConfig.Validate();
            if (!isValid)
            {
                return (false, $"Invalid CloudWatch configuration: {errorMessage}");
            }

            // Apply custom stream name suffix
            if (!string.IsNullOrEmpty(streamNameSuffix))
            {
                cloudWatchConfig.LogStream = $"{cloudWatchConfig.LogStream}-{streamNameSuffix}";
            }

            ConfigureCloudWatchTarget(cloudWatchConfig);
            return (true, $"AWS CloudWatch logging configured: {cloudWatchConfig.LogGroup}/{cloudWatchConfig.LogStream}");
        }
        catch (Exception ex)
        {
            InternalLogger.Error(ex, "Failed to load CloudWatch configuration");
            return (false, $"Failed to load CloudWatch configuration: {ex.Message}");
        }
    }

    /// <summary>
    /// Configure NLog CloudWatch Target directly using configuration object
    /// </summary>
    /// <param name="config">CloudWatch configuration object</param>
    /// <returns>Configuration result and error message</returns>
    public static (bool Success, string Message) ConfigureNLog(CloudWatchConfig config)
    {
        try
        {
            if (!config.Enabled)
            {
                return (false, "AWS CloudWatch logging is disabled");
            }

            var (isValid, errorMessage) = config.Validate();
            if (!isValid)
            {
                return (false, $"Invalid CloudWatch configuration: {errorMessage}");
            }

            ConfigureCloudWatchTarget(config);
            return (true, $"AWS CloudWatch logging configured: {config.LogGroup}/{config.LogStream}");
        }
        catch (Exception ex)
        {
            InternalLogger.Error(ex, "Failed to configure CloudWatch");
            return (false, $"Failed to configure CloudWatch: {ex.Message}");
        }
    }

    private static void ConfigureCloudWatchTarget(CloudWatchConfig config)
    {
        // Use UTC time for date suffix
        var dateStr = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var currentLogStream = $"{config.LogStream}-{dateStr}";

        var layout = "${message} ${exception:format=tostring}";

        var cloudWatchTarget = new AwsCloudWatchTarget
        {
            Name = "awsCloudWatch",
            LogGroup = config.LogGroup,
            LogStream = currentLogStream,
            LogBatchSize = config.BatchSize,
            BatchTimeoutMs = config.BatchTimeoutMs,
            Layout = layout,
            Region = config.Region,
            AccessKeyId = config.AccessKeyId,
            SecretKey = config.SecretKey
        };

        // Ensure LogManager has configuration
        if (LogManager.Configuration == null)
        {
            LogManager.Configuration = new LoggingConfiguration();
        }

        // Add Target and rules
        LogManager.Configuration.AddTarget("awsCloudWatch", cloudWatchTarget);
        LogManager.Configuration.AddRule(LogLevel.Debug, LogLevel.Fatal, cloudWatchTarget);

        // Apply configuration
        LogManager.ReconfigExistingLoggers();

        InternalLogger.Info($"AWS CloudWatch Target configured: {config.LogGroup}/{currentLogStream}");
    }
}
