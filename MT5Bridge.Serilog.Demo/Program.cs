using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.Configuration;
using MT5Bridge.Serilog;
using MT5Bridge.Manager.Models;
using Serilog;
using Serilog.Events;

namespace MT5Bridge.Serilog.Demo;

/// <summary>
/// Comprehensive demo showcasing MT5Bridge Serilog logging capabilities
/// </summary>
public static class Program
{
    public class Options
    {
        [Option('o', "output-file", HelpText = "Path to the log file")]
        public string? LogFile { get; set; }

        [Option('a', "aws-config", HelpText = "Path to AWS configuration JSON file")]
        public string? AwsConfigPath { get; set; }

        [Option('m', "message", Default = "Hello from MT5Bridge Serilog!", HelpText = "Message to log")]
        public string Message { get; set; } = "Hello from MT5Bridge Serilog!";

        [Option('j', "json-mode", Default = "readable", HelpText = "JSON context: 'fast' or 'readable'")]
        public string JsonMode { get; set; } = "readable";

        [Option('s', "skip-cloudwatch", Default = false, HelpText = "Skip CloudWatch demo")]
        public bool SkipCloudWatch { get; set; }
    }

    public static async Task Main(string[] args)
    {
        await Parser.Default.ParseArguments<Options>(args)
            .WithParsedAsync(RunDemoAsync);
    }

    private static async Task RunDemoAsync(Options opts)
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   MT5Bridge Serilog Logging - Comprehensive Demo         ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝\n");

        try
        {
            // 1. Load Configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .Build();

            // 2. Setup Serilog Config (either from config or programmatically)
            var serilogConfig = BuildSerilogConfig(opts, configuration);

            // 3. Create Logger
            Console.WriteLine("Creating Serilog logger...");
            var logger = SerilogBootstrapper.CreateLogger(serilogConfig);
            Console.WriteLine("Logger created\n");

            // 4. Standard Logging
            await DemoStandardLogging(logger, opts);

            // 5. High-Performance Model Logging with Lazy Evaluation
            await DemoLazyModelLogging(logger, opts);

            // 6. Direct JSON Model Logging
            await DemoDirectJsonLogging(logger, opts);

            // 7. ModelLogger Wrapper
            await DemoModelLoggerWrapper(logger, opts);

            // 8. Global Context Usage
            await DemoGlobalContext(logger, opts);

            // 9. Nested Models
            await DemoNestedModels(logger, opts);

            // 10. Error Handling & Exceptions
            await DemoErrorHandling(logger, opts);

            // 11. Field Enrichment
            await DemoFieldEnrichment(logger, opts);

            Console.WriteLine("\nFlushing logs to all targets...");
            await Task.Delay(2000);
            await SerilogBootstrapper.FlushAndCloseAsync();
            Console.WriteLine("Logs flushed successfully\n");

            Console.WriteLine("═════════════════════════════════════════════════════════");
            Console.WriteLine("Demo completed successfully!");
            Console.WriteLine("   Check logs/ folder for file outputs");
            Console.WriteLine("   Check CloudWatch for cloud outputs (if configured)");
            Console.WriteLine("═════════════════════════════════════════════════════════");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nDemo failed with error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            await SerilogBootstrapper.FlushAndCloseAsync();
        }
    }

    private static SerilogConfig BuildSerilogConfig(Options opts, IConfiguration config)
    {
        var serilogConfig = new SerilogConfig
        {
            MinimumLevel = LogEventLevel.Debug,
            Console = new SerilogConfig.ConsoleConfig
            {
                Enabled = true,
                UseJson = false,
                UseAnsiColors = true
            },
            File = new SerilogConfig.FileConfig
            {
                Enabled = true,
                Path = opts.LogFile ?? "logs/mt5bridge-.txt",
                RollingInterval = "Day",
                FileSizeLimitBytes = 104857600, // 100MB
                RetainedFileCountLimit = 30,
                UseJson = true
            }
        };

        if (!opts.SkipCloudWatch && !string.IsNullOrEmpty(opts.AwsConfigPath))
        {
            serilogConfig.CloudWatch.Enabled = true;
            // Note: CloudWatch.UseJson defaults to true (set in SerilogConfig.CloudWatchConfig)
            // You can override it if needed:
            // serilogConfig.CloudWatch.UseJson = false;
        }

        return serilogConfig;
    }

    private static async Task DemoStandardLogging(ILogger logger, Options opts)
    {
        Console.WriteLine("DEMO 1: Standard Logging");
        logger.Information(opts.Message);
        logger.Warning("This is a warning message");
        logger.Debug("Debug-level information");
        Console.WriteLine("Standard logs created\n");
        await Task.Delay(500);
    }

    private static async Task DemoLazyModelLogging(ILogger logger, Options opts)
    {
        Console.WriteLine("DEMO 2: Lazy Model Logging (High Performance)");
        var deal = new DealModel
        {
            Deal = 987654321,
            Symbol = "EURUSD",
            Action = DealAction.Buy,
            Price = 1.0850,
            Volume = 100,
            Time = (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Comment = "Lazy evaluation - skips serialization when level disabled"
        };

        var context = (JsonSerializerContext)(string.Equals(opts.JsonMode, "fast", StringComparison.OrdinalIgnoreCase)
            ? FastJsonContext.Default
            : ReadableJsonContext.Default);

        // This will serialize only if Information level is enabled
        logger.WithModelLazy("DealData", deal, context, LogEventLevel.Information)
              .Information("Trade executed with lazy model");

        // This will be skipped since Debug is disabled
        logger.WithModelLazy("DealDebug", deal, context, LogEventLevel.Debug)
              .Debug("This debug won't be serialized if Debug is disabled");

        Console.WriteLine("Lazy model logs created (efficient - only enabled levels serialized)\n");
        await Task.Delay(500);
    }

    private static async Task DemoDirectJsonLogging(ILogger logger, Options opts)
    {
        Console.WriteLine("DEMO 3: Direct JSON Model Logging");
        var deal = new DealModel
        {
            Deal = 123456789,
            Symbol = "GBPUSD",
            Action = DealAction.Sell,
            Price = 1.2750,
            Volume = 100,
            Time = (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Comment = "Direct JSON logging for raw model storage"
        };

        var context = (JsonSerializerContext)(string.Equals(opts.JsonMode, "fast", StringComparison.OrdinalIgnoreCase)
            ? FastJsonContext.Default
            : ReadableJsonContext.Default);

        logger.WithModelDirect("RawDeal", deal, context)
              .Information("Trade with raw JSON model");

        Console.WriteLine("Direct JSON logs created\n");
        await Task.Delay(500);
    }

    private static async Task DemoModelLoggerWrapper(ILogger logger, Options opts)
    {
        Console.WriteLine("DEMO 4: ModelLogger Wrapper (Chainable Context)");

        // Create a ModelLogger for fluent, chainable logging
        var modelLogger = new ModelLogger(logger, LogEventLevel.Information);

        var account = new DealModel
        {
            Deal = 111111111,
            Symbol = "Account",
            Action = DealAction.Buy,
            Price = 5000,
            Volume = 100,
            Time = (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Comment = "Account state"
        };

        var trade = new DealModel
        {
            Deal = 222222222,
            Symbol = "Trade",
            Action = DealAction.Sell,
            Price = 1.0800,
            Volume = 100,
            Time = (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Comment = "Trade details"
        };

        var context = ReadableJsonContext.Default;
        var typeInfo = (System.Text.Json.Serialization.Metadata.JsonTypeInfo<DealModel>?)context.GetTypeInfo(typeof(DealModel))!;

        modelLogger.WithModel("Account", account, typeInfo)
                   .WithModel("Trade", trade, typeInfo)
                   .WithField("Status", "Processing")
                   .WithField("Priority", 1)
                   .Write("Multi-model transaction processed");

        Console.WriteLine("ModelLogger chainable context created\n");
        await Task.Delay(500);
    }

    private static async Task DemoGlobalContext(ILogger logger, Options opts)
    {
        Console.WriteLine("DEMO 5: Global JSON Context");

        // Set global context to avoid passing it repeatedly
        LoggerExtensions.DefaultContext = ReadableJsonContext.Default;

        var deal = new DealModel
        {
            Deal = 333333333,
            Symbol = "USDJPY",
            Action = DealAction.Buy,
            Price = 149.50,
            Volume = 100,
            Time = (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Comment = "Using global context"
        };

        // No need to pass context each time
        logger.WithModel("GlobalDeal", deal, ReadableJsonContext.Default)
              .Information("Trade with global context set");

        Console.WriteLine("Global context demo completed\n");
        await Task.Delay(500);
    }

    private static async Task DemoNestedModels(ILogger logger, Options opts)
    {
        Console.WriteLine("DEMO 6: Nested Model Structures");

        var primaryDeal = new DealModel
        {
            Deal = 444444444,
            Symbol = "XAUUSD",
            Action = DealAction.Buy,
            Price = 2050.00,
            Volume = 100,
            Time = (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Comment = "Primary transaction"
        };

        var context = ReadableJsonContext.Default;

        logger.WithModel("Primary", primaryDeal, context)
              .WithField("Nested", true)
              .WithField("Level", 1)
              .Information("Nested model structure");

        Console.WriteLine("Nested model logs created\n");
        await Task.Delay(500);
    }

    private static async Task DemoErrorHandling(ILogger logger, Options opts)
    {
        Console.WriteLine("DEMO 7: Error Handling & Exceptions");

        try
        {
            // Simulate error
            throw new InvalidOperationException("Simulated trading error - insufficient balance");
        }
        catch (Exception ex)
        {
            var failedDeal = new DealModel
            {
                Deal = 555555555,
                Symbol = "AUDCAD",
                Action = DealAction.Buy,
                Price = 0.9200,
                Volume = 100,
                Time = (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Comment = "Failed trade"
            };

            var errorContext = ReadableJsonContext.Default;
            var errorTypeInfo = (System.Text.Json.Serialization.Metadata.JsonTypeInfo<DealModel>?)errorContext.GetTypeInfo(typeof(DealModel))!;
            logger.LogModelError("Trade execution failed", failedDeal, errorTypeInfo, ex);
        }

        Console.WriteLine("Error handling demo completed\n");
        await Task.Delay(500);
    }

    private static async Task DemoFieldEnrichment(ILogger logger, Options opts)
    {
        Console.WriteLine("DEMO 8: Field Enrichment & Complex Logging");

        var deal = new DealModel
        {
            Deal = 666666666,
            Symbol = "NZDUSD",
            Action = DealAction.Sell,
            Price = 0.6050,
            Volume = 100,
            Time = (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Comment = "Enriched logging example"
        };

        logger.WithModel("Trade", deal, ReadableJsonContext.Default)
              .WithField("TradeId", 666666666)
              .WithField("UserId", "trader@example.com")
              .WithField("Profitability", "Profitable")
              .WithField("RiskLevel", "Medium")
              .WithField("ExecutionTime", 250)
              .WithField("Timestamp", DateTime.UtcNow)
              .Information("Trade enriched with comprehensive context");

        Console.WriteLine("Field enrichment demo completed\n");
        await Task.Delay(500);
    }
}

