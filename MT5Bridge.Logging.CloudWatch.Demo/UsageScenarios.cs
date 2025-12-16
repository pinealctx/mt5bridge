using MT5Bridge.Logging.CloudWatch;
using NLog;

namespace MT5Bridge.Logging.CloudWatch.Demo;

/// <summary>
/// Demonstrates all 4 usage scenarios
/// </summary>
public class UsageScenarios
{
    /// <summary>
    /// Scenario 1: File logging only (no CloudWatch)
    /// </summary>
    public static void Scenario1_FileOnly()
    {
        Console.WriteLine("\n=== Scenario 1: File Logging Only ===\n");

        var logger = new LoggingFactory()
            .AddFile("logs/scenario1-${shortdate}.log")
            .AddConsole(LogLevel.Info)
            .Build();

        logger.Debug("Debug message - only in file");
        logger.Info("Info message - in file and console");
        logger.Warn("Warning message - in file and console");
        logger.Error("Error message - in file and console");

        Console.WriteLine("✓ Logs written to: logs/scenario1-{date}.log");
        Console.WriteLine("✓ No CloudWatch involved\n");
    }

    /// <summary>
    /// Scenario 2: CloudWatch only (no local files)
    /// </summary>
    public static void Scenario2_CloudWatchOnly()
    {
        Console.WriteLine("\n=== Scenario 2: CloudWatch Only ===\n");

        try
        {
            // Only configure CloudWatch
            var (success, message) = CloudWatchNLogConfiguration.ConfigureNLog("awslog_config.json");

            if (!success)
            {
                Console.WriteLine($"✗ CloudWatch not configured: {message}");
                Console.WriteLine("  To test this scenario, create awslog_config.json with your AWS credentials");
                return;
            }

            var logger = LogManager.GetCurrentClassLogger();

            logger.Info("Info message - sent to CloudWatch only");
            logger.Warn("Warning message - sent to CloudWatch only");
            logger.Error("Error message - sent to CloudWatch only");

            Console.WriteLine("✓ Logs sent to CloudWatch");
            Console.WriteLine("✓ No local files written\n");

            // Wait a bit for async CloudWatch sending
            System.Threading.Thread.Sleep(2000);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ CloudWatch configuration failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Scenario 3: Multiple targets (File + Console + CloudWatch)
    /// </summary>
    public static void Scenario3_MultipleTargets()
    {
        Console.WriteLine("\n=== Scenario 3: Multiple Targets (File + Console + CloudWatch) ===\n");

        try
        {
            var logger = new LoggingFactory()
                .AddFile("logs/scenario3-${shortdate}.log", minLevel: LogLevel.Debug)
                .AddConsole(minLevel: LogLevel.Info)
                .AddCloudWatch("awslog_config.json", minLevel: LogLevel.Warn)
                .Build();

            logger.Debug("Debug - only in file");
            logger.Info("Info - in file and console");
            logger.Warn("Warning - in file, console, and CloudWatch");
            logger.Error("Error - in file, console, and CloudWatch");

            Console.WriteLine("\n✓ Log routing:");
            Console.WriteLine("  Debug → File only");
            Console.WriteLine("  Info  → File + Console");
            Console.WriteLine("  Warn  → File + Console + CloudWatch");
            Console.WriteLine("  Error → File + Console + CloudWatch\n");

            // Wait for CloudWatch async sending
            System.Threading.Thread.Sleep(2000);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Note: CloudWatch not available ({ex.Message})");
            Console.WriteLine("File and console logging still work\n");
        }
    }

    /// <summary>
    /// Scenario 4: Environment-based configuration (Factory pattern)
    /// </summary>
    public static void Scenario4_FactoryPattern()
    {
        Console.WriteLine("\n=== Scenario 4: Factory Pattern (Environment-based) ===\n");

        // Simulate different environments
        string[] environments = { "Development", "Staging", "Production" };

        foreach (var env in environments)
        {
            Console.WriteLine($"\nEnvironment: {env}");
            var logger = ConfigureForEnvironment(env);

            logger.Debug($"[{env}] Debug message");
            logger.Info($"[{env}] Info message");
            logger.Warn($"[{env}] Warning message");
            logger.Error($"[{env}] Error message");

            Console.WriteLine($"✓ {env} configuration applied\n");

            // Clean up
            LogManager.Shutdown();
            System.Threading.Thread.Sleep(500);
        }
    }

    private static Logger ConfigureForEnvironment(string environment)
    {
        var factory = new LoggingFactory();

        switch (environment.ToLower())
        {
            case "development":
                Console.WriteLine("  Targets: Console + File (Debug level)");
                return factory
                    .AddConsole(LogLevel.Debug)
                    .AddFile($"logs/dev-${{shortdate}}.log", LogLevel.Debug)
                    .Build();

            case "staging":
                Console.WriteLine("  Targets: Console + File + CloudWatch (Info level)");
                try
                {
                    return factory
                        .AddConsole(LogLevel.Info)
                        .AddFile($"logs/staging-${{shortdate}}.log", LogLevel.Debug)
                        .AddCloudWatch("awslog_config.json", minLevel: LogLevel.Info)
                        .Build();
                }
                catch
                {
                    Console.WriteLine("  (CloudWatch unavailable, using Console + File only)");
                    return factory
                        .AddConsole(LogLevel.Info)
                        .AddFile($"logs/staging-${{shortdate}}.log", LogLevel.Debug)
                        .Build();
                }

            case "production":
                Console.WriteLine("  Targets: CloudWatch (Info) + File (Error)");
                try
                {
                    return factory
                        .AddFile($"logs/production-${{shortdate}}.log", LogLevel.Error)
                        .AddCloudWatch("awslog_config.json", minLevel: LogLevel.Info)
                        .Build();
                }
                catch
                {
                    Console.WriteLine("  (CloudWatch unavailable, using File only)");
                    return factory
                        .AddFile($"logs/production-${{shortdate}}.log", LogLevel.Info)
                        .Build();
                }

            default:
                Console.WriteLine("  Targets: Console (default)");
                return factory.AddConsole().Build();
        }
    }

    /// <summary>
    /// Demonstrate quick start methods
    /// </summary>
    public static void Scenario5_QuickStart()
    {
        Console.WriteLine("\n=== Scenario 5: Quick Start Methods ===\n");

        // Method 1: Simple console logger
        Console.WriteLine("1. Quick console logger:");
        var consoleLogger = LoggingFactory.CreateConsoleLogger();
        consoleLogger.Info("Simple console logging");
        LogManager.Shutdown();
        System.Threading.Thread.Sleep(100);

        // Method 2: Simple file logger
        Console.WriteLine("\n2. Quick file logger:");
        var fileLogger = LoggingFactory.CreateFileLogger("logs/quickstart.log");
        fileLogger.Info("Simple file logging");
        LogManager.Shutdown();
        System.Threading.Thread.Sleep(100);

        // Method 3: Custom combination
        Console.WriteLine("\n3. Custom combination:");
        var customLogger = new LoggingFactory()
            .AddFile("logs/custom-${shortdate}.log")
            .AddConsole()
            .Build();
        customLogger.Info("Custom multi-target logging");

        Console.WriteLine("\n✓ Three quick start methods demonstrated\n");
    }

    /// <summary>
    /// Run all scenarios
    /// </summary>
    public static void RunAllScenarios()
    {
        Console.WriteLine("╔════════════════════════════════════════════════════╗");
        Console.WriteLine("║  MT5Bridge Logging Usage Scenarios Demo           ║");
        Console.WriteLine("╚════════════════════════════════════════════════════╝");

        try
        {
            Scenario1_FileOnly();
            Scenario5_QuickStart();
            Scenario3_MultipleTargets();
            Scenario2_CloudWatchOnly();
            Scenario4_FactoryPattern();

            Console.WriteLine("\n╔════════════════════════════════════════════════════╗");
            Console.WriteLine("║  All scenarios completed successfully!             ║");
            Console.WriteLine("╚════════════════════════════════════════════════════╝\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Error: {ex.Message}");
        }
        finally
        {
            LogManager.Shutdown();
        }
    }
}
