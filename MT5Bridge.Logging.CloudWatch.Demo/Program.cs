using CommandLine;
using MT5Bridge.Core.Logging;
using MT5Bridge.Logging.NLog;
using MT5Bridge.Logging.CloudWatch;
using NLog;
using NLog.Config;
using NLog.Targets;
using CoreLogger = MT5Bridge.Core.Logging.ILogger;

namespace MT5Bridge.Logging.CloudWatch.Demo;

/// <summary>
/// AWS CloudWatch NLog Target demo program
/// </summary>
public class Program
{
    public class Options
    {
        [Option("scenario", HelpText = "Run specific scenario (1-5) or 'all' for all scenarios")]
        public string? Scenario { get; set; }

        [Option('c', "cloudwatch", Default = false, HelpText = "Enable CloudWatch logging")]
        public bool EnableCloudWatch { get; set; }

        [Option('f', "file", Default = false, HelpText = "Enable local file logging")]
        public bool EnableFileLogging { get; set; }

        [Option("config", Default = "awslog_config.json", HelpText = "CloudWatch config file path")]
        public string ConfigFile { get; set; } = null!;

        [Option('s', "suffix", HelpText = "Log stream suffix (e.g., machine name)")]
        public string? StreamSuffix { get; set; }

        [Option('n', "count", Default = 50, HelpText = "Number of test logs to generate")]
        public int LogCount { get; set; }

        [Option('i', "interval", Default = 100, HelpText = "Log output interval in milliseconds")]
        public int IntervalMs { get; set; }

        [Option('p', "path", Default = "logs", HelpText = "Local log directory path")]
        public string LogPath { get; set; } = null!;

        [Option("core-api", Default = false, HelpText = "Use MT5Bridge.Core.Logging API instead of NLog directly")]
        public bool UseCoreApi { get; set; }
    }

    public static async Task<int> Main(string[] args)
    {
        Console.WriteLine("=== MT5Bridge AWS CloudWatch NLog Demo ===\n");

        return await Parser.Default.ParseArguments<Options>(args)
            .MapResult(
                async (Options opts) =>
                {
                    // Check if scenario mode is requested
                    if (!string.IsNullOrEmpty(opts.Scenario))
                    {
                        return RunScenarios(opts.Scenario);
                    }

                    return await RunDemo(opts);
                },
                errs => Task.FromResult(1));
    }

    private static int RunScenarios(string scenario)
    {
        try
        {
            switch (scenario.ToLower())
            {
                case "1":
                    UsageScenarios.Scenario1_FileOnly();
                    break;
                case "2":
                    UsageScenarios.Scenario2_CloudWatchOnly();
                    break;
                case "3":
                    UsageScenarios.Scenario3_MultipleTargets();
                    break;
                case "4":
                    UsageScenarios.Scenario4_FactoryPattern();
                    break;
                case "5":
                    UsageScenarios.Scenario5_QuickStart();
                    break;
                case "all":
                    UsageScenarios.RunAllScenarios();
                    break;
                default:
                    Console.WriteLine($"Unknown scenario: {scenario}");
                    Console.WriteLine("Valid options: 1, 2, 3, 4, 5, or 'all'");
                    Console.WriteLine("\nScenarios:");
                    Console.WriteLine("  1 - File logging only");
                    Console.WriteLine("  2 - CloudWatch only");
                    Console.WriteLine("  3 - Multiple targets (File + Console + CloudWatch)");
                    Console.WriteLine("  4 - Factory pattern (Environment-based)");
                    Console.WriteLine("  5 - Quick start methods");
                    Console.WriteLine("  all - Run all scenarios");
                    return 1;
            }
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running scenario: {ex.Message}");
            return 1;
        }
    }

    private static async Task<int> RunDemo(Options options)
    {
        try
        {
            // Display configuration
            Console.WriteLine("Configuration:");
            Console.WriteLine($"  CloudWatch:       {options.EnableCloudWatch}");
            Console.WriteLine($"  File logging:     {options.EnableFileLogging}");
            Console.WriteLine($"  Config file:      {options.ConfigFile}");
            Console.WriteLine($"  Stream suffix:    {options.StreamSuffix ?? "(none)"}");
            Console.WriteLine($"  Log count:        {options.LogCount}");
            Console.WriteLine($"  Interval:         {options.IntervalMs}ms");
            Console.WriteLine($"  Log path:         {Path.GetFullPath(options.LogPath)}");
            Console.WriteLine($"  Use Core API:     {options.UseCoreApi}");
            Console.WriteLine();

            if (!options.EnableCloudWatch && !options.EnableFileLogging)
            {
                Console.WriteLine("Error: At least one output target must be enabled (--cloudwatch or --file)");
                Console.WriteLine("Run with --help for usage information");
                return 1;
            }

            // 1. Configure local file logging (optional)
            if (options.EnableFileLogging)
            {
                ConfigureFileLogging(options.LogPath);
                Console.WriteLine($"✓ File logging configured: {Path.GetFullPath(options.LogPath)}");
            }

            // 2. Configure AWS CloudWatch
            if (options.EnableCloudWatch)
            {
                var (success, message) = CloudWatchNLogConfiguration.ConfigureNLog(
                    configFilePath: options.ConfigFile,
                    streamNameSuffix: options.StreamSuffix
                );

                if (success)
                {
                    Console.WriteLine($"✓ CloudWatch configured: {message}");
                }
                else
                {
                    Console.WriteLine($"✗ CloudWatch configuration failed: {message}");
                    if (!options.EnableFileLogging)
                    {
                        Console.WriteLine("Hint: Run with --file to enable file logging as fallback");
                        return 1;
                    }
                }
            }

            Console.WriteLine();

            // 3. Generate test logs
            if (options.UseCoreApi)
            {
                Console.WriteLine($"Generating {options.LogCount} test logs using MT5Bridge.Core.Logging API...\n");
                await GenerateTestLogsWithCoreApi(options.LogCount, options.IntervalMs);
            }
            else
            {
                Console.WriteLine($"Generating {options.LogCount} test logs using NLog API...\n");
                await GenerateTestLogsWithNLog(options.LogCount, options.IntervalMs);
            }

            Console.WriteLine("\n✓ Demo completed successfully");
            Console.WriteLine("Press any key to exit (logs may still be sending in background)...");
            Console.ReadKey();

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
            Console.WriteLine($"Stack: {ex.StackTrace}");
            return 1;
        }
        finally
        {
            Console.WriteLine("\nShutting down NLog...");
            LogManager.Shutdown();
        }
    }

    private static void ConfigureFileLogging(string logPath)
    {
        var fileTarget = new FileTarget("logfile")
        {
            FileName = $"{logPath}/demo.${{shortdate}}.log",
            Layout = "${longdate} | ${level:uppercase=true} | ${logger} | ${message} ${exception:format=tostring}",
            KeepFileOpen = true,
            Encoding = System.Text.Encoding.UTF8,
            MaxArchiveFiles = 10,
            ArchiveAboveSize = 10485760, // 10MB
            CreateDirs = true
        };

        var config = LogManager.Configuration ?? new LoggingConfiguration();
        config.AddTarget("file", fileTarget);
        config.AddRule(global::NLog.LogLevel.Debug, global::NLog.LogLevel.Fatal, fileTarget);
        LogManager.Configuration = config;
    }

    /// <summary>
    /// Generate test logs using NLog API directly
    /// </summary>
    private static async Task GenerateTestLogsWithNLog(int count, int intervalMs)
    {
        var logger = LogManager.GetLogger("DemoApp");

        for (int i = 1; i <= count; i++)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");

            switch (i % 5)
            {
                case 0:
                    logger.Debug($"[{i}/{count}] Debug message at {timestamp}: checking variable state");
                    break;
                case 1:
                    logger.Info($"[{i}/{count}] Info at {timestamp}: processing item #{i}");
                    break;
                case 2:
                    logger.Warn($"[{i}/{count}] Warning at {timestamp}: resource usage at 75%");
                    break;
                case 3:
                    logger.Error($"[{i}/{count}] Error at {timestamp}: failed to connect to service");
                    break;
                case 4:
                    try
                    {
                        throw new InvalidOperationException($"Simulated exception #{i}");
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"[{i}/{count}] Exception caught at {timestamp}");
                    }
                    break;
            }

            // Progress indicator
            if (i % 10 == 0)
            {
                Console.Write(".");
            }

            if (intervalMs > 0)
            {
                await Task.Delay(intervalMs);
            }
        }

        Console.WriteLine($"\n{count} logs generated");
    }

    /// <summary>
    /// Generate test logs using MT5Bridge.Core.Logging API
    /// This demonstrates integration with MT5Bridge.Core abstractions
    /// </summary>
    private static async Task GenerateTestLogsWithCoreApi(int count, int intervalMs)
    {
        // Create logger using MT5Bridge.Core abstraction
        ILoggerFactory factory = new NLogLoggerFactory();
        CoreLogger logger = factory.CreateLogger("DemoApp");

        Console.WriteLine("Using MT5Bridge.Core.Logging API (logs still go to configured targets)\n");

        for (int i = 1; i <= count; i++)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");

            switch (i % 5)
            {
                case 0:
                    logger.Debug($"[{i}/{count}] Debug via Core API at {timestamp}: checking state");
                    break;
                case 1:
                    logger.Info($"[{i}/{count}] Info via Core API at {timestamp}: processing item #{i}");
                    break;
                case 2:
                    logger.Warn($"[{i}/{count}] Warning via Core API at {timestamp}: high resource usage");
                    break;
                case 3:
                    logger.Error($"[{i}/{count}] Error via Core API at {timestamp}: connection failed");
                    break;
                case 4:
                    try
                    {
                        throw new InvalidOperationException($"Simulated exception #{i}");
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"[{i}/{count}] Exception via Core API at {timestamp}", ex);
                    }
                    break;
            }

            // Progress indicator
            if (i % 10 == 0)
            {
                Console.Write(".");
            }

            if (intervalMs > 0)
            {
                await Task.Delay(intervalMs);
            }
        }

        Console.WriteLine($"\n{count} logs generated using MT5Bridge.Core.Logging API");
    }
}
