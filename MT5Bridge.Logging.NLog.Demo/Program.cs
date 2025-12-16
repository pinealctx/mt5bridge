using CommandLine;
using MT5Bridge.Logging.NLog;
using NLog;
using NLog.Config;
using NLog.Targets;
using CoreLogger = MT5Bridge.Core.Logging.ILogger;

namespace MT5Bridge.Logging.NLog.Demo;

internal class Program
{
    public class Options
    {
        [Option('p', "path", Default = "./logs", HelpText = "Log directory path")]
        public string LogPath { get; set; } = null!;

        [Option('r', "rolling", Default = "day", HelpText = "Log rolling period: minute, hour, day")]
        public string Rolling { get; set; } = null!;

        [Option('i', "interval", Default = 1, HelpText = "Log output interval in seconds")]
        public int Interval { get; set; }

        [Option('m', "max-files", Default = 7, HelpText = "Maximum number of archive files to keep (0 = unlimited)")]
        public int MaxArchiveFiles { get; set; }

        [Option('d', "max-days", Default = 0, HelpText = "Maximum number of days to keep archive files (0 = unlimited)")]
        public int MaxArchiveDays { get; set; }

        [Option('s', "archive-size", Default = 0, HelpText = "Archive when file exceeds size in MB (0 = disabled)")]
        public long ArchiveAboveSizeMB { get; set; }

        [Option('z', "compress", Default = false, HelpText = "Compress archive files (.zip)")]
        public bool CompressArchive { get; set; }

        [Option('f', "date-format", Default = "yyyy-MM-dd-HHmm", HelpText = "Archive file date format")]
        public string ArchiveDateFormat { get; set; } = null!;

        [Option('c', "console", Default = true, HelpText = "Also output to console")]
        public bool EnableConsole { get; set; }
    }

    static async Task<int> Main(string[] args)
    {
        Console.WriteLine("MT5Bridge NLog Demo - Log Rolling Test Tool");
        Console.WriteLine("===========================================");
        Console.WriteLine();

        return await Parser.Default.ParseArguments<Options>(args)
            .MapResult(
                async (Options opts) => await RunDemo(opts),
                errs => Task.FromResult(1));
    }

    static async Task<int> RunDemo(Options options)
    {
        try
        {
            // Validate rolling period
            var archivePeriod = ParseArchivePeriod(options.Rolling);

            // Validate interval
            if (options.Interval <= 0)
            {
                Console.WriteLine($"Error: Interval must be greater than 0, current: {options.Interval}");
                return 1;
            }

            // Display configuration
            Console.WriteLine("Configuration:");
            Console.WriteLine($"  Log directory:    {Path.GetFullPath(options.LogPath)}");
            Console.WriteLine($"  Rolling period:   {options.Rolling}");
            Console.WriteLine($"  Output interval:  {options.Interval} seconds");
            Console.WriteLine($"  Max archive files: {(options.MaxArchiveFiles > 0 ? options.MaxArchiveFiles.ToString() : "Unlimited")}");
            Console.WriteLine($"  Max archive days:  {(options.MaxArchiveDays > 0 ? options.MaxArchiveDays.ToString() : "Unlimited")}");
            Console.WriteLine($"  Archive size limit: {(options.ArchiveAboveSizeMB > 0 ? $"{options.ArchiveAboveSizeMB} MB" : "Disabled")}");
            Console.WriteLine($"  Compress archives: {options.CompressArchive}");
            Console.WriteLine($"  Archive date format: {options.ArchiveDateFormat}");
            Console.WriteLine($"  Console output:   {options.EnableConsole}");
            Console.WriteLine();

            // Configure NLog
            ConfigureNLog(options.LogPath, archivePeriod, options.MaxArchiveFiles, options.MaxArchiveDays,
                         options.ArchiveAboveSizeMB, options.CompressArchive, options.ArchiveDateFormat, options.EnableConsole);

            // Create logger using MT5Bridge abstraction
            var factory = new NLogLoggerFactory();
            var logger = factory.CreateLogger("DemoApp");

            Console.WriteLine("Application started. Press Ctrl+C or any key to exit.");
            Console.WriteLine(new string('=', 60));
            Console.WriteLine();

            // Create cancellation token
            using var cts = new CancellationTokenSource();

            // Start periodic logging task
            var loggingTask = StartPeriodicLogging(logger, options.Interval, cts.Token);

            // Start keyboard listener task
            var inputTask = WaitForKeyPress(cts);

            // Wait for any task to complete
            await Task.WhenAny(loggingTask, inputTask);

            // Cancel all tasks
            cts.Cancel();

            try
            {
                await loggingTask;
            }
            catch (OperationCanceledException)
            {
                // Expected when cancelled
            }

            Console.WriteLine();
            Console.WriteLine("Application exited.");
            Console.WriteLine($"Log files are in: {Path.GetFullPath(options.LogPath)}");

            // Shutdown NLog
            LogManager.Shutdown();

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private static FileArchivePeriod ParseArchivePeriod(string rolling)
    {
        return rolling.ToLower() switch
        {
            "minute" => FileArchivePeriod.Minute,
            "hour" => FileArchivePeriod.Hour,
            "day" => FileArchivePeriod.Day,
            _ => throw new ArgumentException($"Invalid rolling period: {rolling}. Valid values: minute, hour, day")
        };
    }

    private static void ConfigureNLog(string logPath, FileArchivePeriod archivePeriod, int maxArchiveFiles,
                                      int maxArchiveDays, long archiveAboveSizeMB, bool compressArchive,
                                      string archiveDateFormat, bool enableConsole)
    {
        var config = new LoggingConfiguration();

        // File target with rolling
        var fileTarget = new FileTarget("file")
        {
            FileName = $"{logPath}/mt5bridge-${{shortdate}}.log",
            Layout = "${longdate} | ${level:uppercase=true:padding=-5} | ${logger:shortName=true} | ${message} ${exception:format=tostring}",
            ArchiveEvery = archivePeriod,
            ArchiveFileName = $"{logPath}/archive/mt5bridge-${{#}}.log",
            MaxArchiveFiles = maxArchiveFiles > 0 ? maxArchiveFiles : 0,
            MaxArchiveDays = maxArchiveDays > 0 ? maxArchiveDays : 0,
            ArchiveAboveSize = archiveAboveSizeMB > 0 ? archiveAboveSizeMB * 1024 * 1024 : 0,
            KeepFileOpen = false,
            Encoding = System.Text.Encoding.UTF8
        };

        // Note: Archive compression was removed in NLog 6.0
        // If compression is needed, consider using external tools or NLog.Targets.Zip package

        config.AddTarget(fileTarget);
        config.AddRule(LogLevel.Debug, LogLevel.Fatal, fileTarget);

        // Console target (optional)
        if (enableConsole)
        {
            var consoleTarget = new ConsoleTarget("console")
            {
                Layout = "${time} | ${level:uppercase=true:padding=-5} | ${message}"
            };
            config.AddTarget(consoleTarget);
            config.AddRule(LogLevel.Info, LogLevel.Fatal, consoleTarget);
        }

        LogManager.Configuration = config;
    }

    private static async Task StartPeriodicLogging(CoreLogger logger, int intervalSeconds, CancellationToken cancellationToken)
    {
        int messageCount = 0;
        var startTime = DateTime.Now;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                messageCount++;
                var elapsed = DateTime.Now - startTime;
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                // Log at different levels
                if (messageCount % 10 == 0)
                {
                    logger.Warn($"[{messageCount}] Warning message at {timestamp} (elapsed: {elapsed:hh\\:mm\\:ss})");
                }
                else if (messageCount % 5 == 0)
                {
                    logger.Info($"[{messageCount}] Info message at {timestamp} (elapsed: {elapsed:hh\\:mm\\:ss})");
                }
                else
                {
                    logger.Debug($"[{messageCount}] Debug message at {timestamp} (elapsed: {elapsed:hh\\:mm\\:ss})");
                }

                // Occasionally log errors
                if (messageCount % 20 == 0)
                {
                    try
                    {
                        throw new InvalidOperationException($"Simulated error #{messageCount / 20}");
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"[{messageCount}] Error occurred at {timestamp}", ex);
                    }
                }

                Console.WriteLine($"[{timestamp}] Message #{messageCount} logged (elapsed: {elapsed:hh\\:mm\\:ss})");

                await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in logging loop: {ex.Message}");
            }
        }

        Console.WriteLine($"Total messages logged: {messageCount}");
    }

    private static async Task WaitForKeyPress(CancellationTokenSource cts)
    {
        await Task.Run(() =>
        {
            Console.ReadKey(true);
            cts.Cancel();
        });
    }
}
