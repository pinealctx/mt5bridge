using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.Configuration;
using MT5Bridge.Serilog;
using MT5Bridge.Manager.Models;
using MT5Bridge.Manager.Models.Proto;
using Serilog;
using Serilog.Events;

namespace MT5Bridge.Serilog.Demo;

/// <summary>
/// Performance testing demo for MT5Bridge.Serilog with continuous logging and statistics
/// </summary>
public static class Program
{
    public class Options
    {
        [Option('c', "config", Required = true, HelpText = "Path to appsettings configuration file")]
        public string ConfigFile { get; set; } = "";

        [Option('d', "duration", Default = 10, HelpText = "Duration to run test in seconds")]
        public int DurationSeconds { get; set; } = 10;

        [Option("min-interval", Default = 1, HelpText = "Minimum interval between logs in milliseconds")]
        public int MinIntervalMs { get; set; } = 1;

        [Option("max-interval", Default = 100, HelpText = "Maximum interval between logs in milliseconds")]
        public int MaxIntervalMs { get; set; } = 100;

        [Option('m', "model-type", Default = "poco", HelpText = "Model type to test: 'poco' or 'protobuf'")]
        public string ModelType { get; set; } = "poco";
    }

    public static async Task Main(string[] args)
    {
        await Parser.Default.ParseArguments<Options>(args)
            .WithParsedAsync(RunDemoAsync);
    }

    private static async Task RunDemoAsync(Options opts)
    {
        Console.WriteLine("MT5Bridge Serilog Performance Test");
        Console.WriteLine("===================================\n");

        try
        {
            if (!File.Exists(opts.ConfigFile))
            {
                Console.Error.WriteLine($"ERROR: Configuration file not found: {opts.ConfigFile}");
                return;
            }

            // Validate options
            if (opts.MinIntervalMs < 0 || opts.MaxIntervalMs < 0 || opts.MinIntervalMs > opts.MaxIntervalMs)
            {
                Console.Error.WriteLine("ERROR: Invalid interval range. Min must be <= Max and both >= 0");
                return;
            }

            if (opts.DurationSeconds <= 0)
            {
                Console.Error.WriteLine("ERROR: Duration must be positive");
                return;
            }

            // Validate model type
            var modelType = opts.ModelType.ToLowerInvariant();
            if (modelType != "poco" && modelType != "protobuf")
            {
                Console.Error.WriteLine("ERROR: Model type must be 'poco' or 'protobuf'");
                return;
            }

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(opts.ConfigFile, optional: false)
                .Build();

            var serilogConfig = new SerilogConfig();
            configuration.Bind(serilogConfig);

            var logger = SerilogBootstrapper.CreateLogger(serilogConfig);

            // Display test configuration
            Console.WriteLine($"Configuration:");
            Console.WriteLine($"  Model Type: {modelType.ToUpper()}");
            Console.WriteLine($"  Duration: {opts.DurationSeconds} seconds");
            Console.WriteLine($"  Interval: {opts.MinIntervalMs}-{opts.MaxIntervalMs} ms");
            Console.WriteLine($"  Min Log Level: {serilogConfig.MinimumLevel}");
            Console.WriteLine($"  Sinks: Console={serilogConfig.Console.Enabled}, File={serilogConfig.File.Enabled}, CloudWatch={serilogConfig.CloudWatch.Enabled}");
            Console.WriteLine();

            // Run performance test
            var stats = modelType == "poco"
                ? await RunPerformanceTestPocoAsync(logger, opts)
                : await RunPerformanceTestProtobufAsync(logger, opts);

            // Display statistics
            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("Performance Statistics");
            Console.WriteLine(new string('=', 60));
            Console.WriteLine($"Total Logs:        {stats.TotalLogs:N0}");
            Console.WriteLine($"Total Duration:    {stats.TotalDuration.TotalSeconds:F2} seconds");
            Console.WriteLine($"Logs/Second:       {stats.LogsPerSecond:F2}");
            Console.WriteLine($"Avg Log Time:      {stats.AverageLogTimeMs:F4} ms");
            Console.WriteLine($"Min Log Time:      {stats.MinLogTimeMs:F4} ms");
            Console.WriteLine($"Max Log Time:      {stats.MaxLogTimeMs:F4} ms");
            Console.WriteLine($"95th Percentile:   {stats.Percentile95Ms:F4} ms");
            Console.WriteLine($"99th Percentile:   {stats.Percentile99Ms:F4} ms");
            Console.WriteLine(new string('=', 60));

            await SerilogBootstrapper.FlushAndCloseAsync();
            Console.WriteLine("\nTest completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nTest failed: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    private static async Task<PerformanceStats> RunPerformanceTestPocoAsync(ILogger logger, Options opts)
    {
        var random = new Random();
        var stopwatch = Stopwatch.StartNew();
        var logTimings = new List<double>();
        var endTime = DateTime.UtcNow.AddSeconds(opts.DurationSeconds);
        var symbols = new[] { "EURUSD", "GBPUSD", "USDJPY", "AUDUSD", "USDCAD", "NZDUSD", "EURJPY", "GBPJPY" };
        var actions = new[] { Manager.Models.DealAction.Buy, Manager.Models.DealAction.Sell };
        var reasons = new[] { Manager.Models.DealReason.Client, Manager.Models.DealReason.Expert, Manager.Models.DealReason.Dealer, Manager.Models.DealReason.SL, Manager.Models.DealReason.TP };
        var entries = new[] { Manager.Models.EntryFlag.In, Manager.Models.EntryFlag.Out, Manager.Models.EntryFlag.InOut };

        long totalLogs = 0;
        Console.WriteLine("Starting performance test (POCO Model)...\n");

        while (DateTime.UtcNow < endTime)
        {
            // Generate comprehensive random deal data
            var now = DateTimeOffset.UtcNow;
            var deal = new Manager.Models.DealModel
            {
                Deal = (ulong)random.Next(100000000, 999999999),
                ExternalID = $"EXT-{random.Next(10000, 99999)}",
                Login = (ulong)random.Next(1000, 9999),
                Dealer = (ulong)random.Next(100, 999),
                Order = (ulong)random.Next(100000000, 999999999),
                Symbol = symbols[random.Next(symbols.Length)],
                Action = actions[random.Next(actions.Length)],
                Entry = entries[random.Next(entries.Length)],
                Reason = reasons[random.Next(reasons.Length)],
                Digits = (uint)random.Next(2, 6),
                DigitsCurrency = 2,
                ContractSize = 100000.0,
                Time = now.ToUnixTimeSeconds(),
                TimeMsc = now.ToUnixTimeMilliseconds(),
                Price = 1.0 + random.NextDouble() * 0.5,
                PricePosition = 1.0 + random.NextDouble() * 0.5,
                PriceSL = random.Next(0, 2) == 1 ? 1.0 + random.NextDouble() * 0.4 : 0,
                PriceTP = random.Next(0, 2) == 1 ? 1.0 + random.NextDouble() * 0.6 : 0,
                Volume = (ulong)random.Next(1, 1000),
                VolumeClosed = (ulong)random.Next(0, 500),
                Profit = (random.NextDouble() - 0.5) * 1000,
                ProfitRaw = (random.NextDouble() - 0.5) * 1000,
                Storage = random.NextDouble() * 10,
                Commission = random.NextDouble() * 50,
                Fee = random.NextDouble() * 5,
                RateProfit = 1.0 + random.NextDouble() * 0.1,
                RateMargin = 1.0 + random.NextDouble() * 0.1,
                TickValue = random.NextDouble() * 10,
                TickSize = 0.00001,
                MarketBid = 1.0 + random.NextDouble() * 0.5,
                MarketAsk = 1.0 + random.NextDouble() * 0.5,
                MarketLast = 1.0 + random.NextDouble() * 0.5,
                ExpertID = (ulong)random.Next(0, 100000),
                PositionID = (ulong)random.Next(100000000, 999999999),
                Comment = random.Next(0, 3) == 1 ? "Test comment" : "",
                Gateway = ""
            };

            // Measure only logging time - log all fields except ApiData
            var logStopwatch = Stopwatch.StartNew();
            logger.Information(
                "Deal: {Deal} ExtID:{ExternalID} Login:{Login} Dealer:{Dealer} Order:{Order} " +
                "Symbol:{Symbol} Action:{Action} Entry:{Entry} Reason:{Reason} " +
                "Price:{Price} Vol:{Volume} VolClosed:{VolumeClosed} " +
                "Profit:{Profit} ProfitRaw:{ProfitRaw} Storage:{Storage} Comm:{Commission} Fee:{Fee} " +
                "SL:{PriceSL} TP:{PriceTP} PricePos:{PricePosition} " +
                "Time:{Time} TimeMsc:{TimeMsc} " +
                "RateProfit:{RateProfit} RateMargin:{RateMargin} " +
                "TickVal:{TickValue} TickSize:{TickSize} " +
                "Bid:{MarketBid} Ask:{MarketAsk} Last:{MarketLast} " +
                "ExpertID:{ExpertID} PosID:{PositionID} Comment:{Comment}",
                deal.Deal, deal.ExternalID, deal.Login, deal.Dealer, deal.Order,
                deal.Symbol, deal.Action, deal.Entry, deal.Reason,
                deal.Price, deal.Volume, deal.VolumeClosed,
                deal.Profit, deal.ProfitRaw, deal.Storage, deal.Commission, deal.Fee,
                deal.PriceSL, deal.PriceTP, deal.PricePosition,
                deal.Time, deal.TimeMsc,
                deal.RateProfit, deal.RateMargin,
                deal.TickValue, deal.TickSize,
                deal.MarketBid, deal.MarketAsk, deal.MarketLast,
                deal.ExpertID, deal.PositionID, deal.Comment);
            logStopwatch.Stop();

            logTimings.Add(logStopwatch.Elapsed.TotalMilliseconds);
            totalLogs++;

            // Display progress every 1000 logs
            if (totalLogs % 1000 == 0)
            {
                var elapsed = stopwatch.Elapsed.TotalSeconds;
                var rate = totalLogs / elapsed;
                Console.WriteLine($"Progress: {totalLogs:N0} logs, {elapsed:F1}s elapsed, {rate:F2} logs/sec");
            }

            // Random delay
            if (opts.MaxIntervalMs > 0)
            {
                var delayMs = random.Next(opts.MinIntervalMs, opts.MaxIntervalMs + 1);
                if (delayMs > 0)
                {
                    await Task.Delay(delayMs);
                }
            }
        }

        stopwatch.Stop();

        // Calculate statistics
        logTimings.Sort();
        var stats = new PerformanceStats
        {
            TotalLogs = totalLogs,
            TotalDuration = stopwatch.Elapsed,
            LogsPerSecond = totalLogs / stopwatch.Elapsed.TotalSeconds,
            AverageLogTimeMs = logTimings.Average(),
            MinLogTimeMs = logTimings.Min(),
            MaxLogTimeMs = logTimings.Max(),
            Percentile95Ms = GetPercentile(logTimings, 0.95),
            Percentile99Ms = GetPercentile(logTimings, 0.99)
        };

        return stats;
    }

    private static async Task<PerformanceStats> RunPerformanceTestProtobufAsync(ILogger logger, Options opts)
    {
        var random = new Random();
        var stopwatch = Stopwatch.StartNew();
        var logTimings = new List<double>();
        var endTime = DateTime.UtcNow.AddSeconds(opts.DurationSeconds);
        var symbols = new[] { "EURUSD", "GBPUSD", "USDJPY", "AUDUSD", "USDCAD", "NZDUSD", "EURJPY", "GBPJPY" };
        var actions = new[] { Manager.Models.Proto.DealAction.Buy, Manager.Models.Proto.DealAction.Sell };
        var reasons = new[] { Manager.Models.Proto.DealReason.Client, Manager.Models.Proto.DealReason.Expert, Manager.Models.Proto.DealReason.Dealer, Manager.Models.Proto.DealReason.Sl, Manager.Models.Proto.DealReason.Tp };
        var entries = new[] { Manager.Models.Proto.EntryFlag.In, Manager.Models.Proto.EntryFlag.Out, Manager.Models.Proto.EntryFlag.Inout };

        long totalLogs = 0;
        Console.WriteLine("Starting performance test (Protobuf Model)...\n");

        while (DateTime.UtcNow < endTime)
        {
            // Generate comprehensive random deal data
            var now = DateTimeOffset.UtcNow;
            var deal = new Manager.Models.Proto.DealModel
            {
                Deal = (ulong)random.Next(100000000, 999999999),
                ExternalId = $"EXT-{random.Next(10000, 99999)}",
                Login = (ulong)random.Next(1000, 9999),
                Dealer = (ulong)random.Next(100, 999),
                Order = (ulong)random.Next(100000000, 999999999),
                Symbol = symbols[random.Next(symbols.Length)],
                Action = actions[random.Next(actions.Length)],
                Entry = entries[random.Next(entries.Length)],
                Reason = reasons[random.Next(reasons.Length)],
                Digits = (uint)random.Next(2, 6),
                DigitsCurrency = 2,
                ContractSize = 100000.0,
                Time = now.ToUnixTimeSeconds(),
                TimeMsc = now.ToUnixTimeMilliseconds(),
                Price = 1.0 + random.NextDouble() * 0.5,
                PricePosition = 1.0 + random.NextDouble() * 0.5,
                PriceSl = random.Next(0, 2) == 1 ? 1.0 + random.NextDouble() * 0.4 : 0,
                PriceTp = random.Next(0, 2) == 1 ? 1.0 + random.NextDouble() * 0.6 : 0,
                Volume = (ulong)random.Next(1, 1000),
                VolumeClosed = (ulong)random.Next(0, 500),
                Profit = (random.NextDouble() - 0.5) * 1000,
                ProfitRaw = (random.NextDouble() - 0.5) * 1000,
                Storage = random.NextDouble() * 10,
                Commission = random.NextDouble() * 50,
                Fee = random.NextDouble() * 5,
                RateProfit = 1.0 + random.NextDouble() * 0.1,
                RateMargin = 1.0 + random.NextDouble() * 0.1,
                TickValue = random.NextDouble() * 10,
                TickSize = 0.00001,
                MarketBid = 1.0 + random.NextDouble() * 0.5,
                MarketAsk = 1.0 + random.NextDouble() * 0.5,
                MarketLast = 1.0 + random.NextDouble() * 0.5,
                ExpertId = (ulong)random.Next(0, 100000),
                PositionId = (ulong)random.Next(100000000, 999999999),
                Comment = random.Next(0, 3) == 1 ? "Test comment" : "",
                Gateway = ""
            };

            // Measure only logging time - log all fields except ApiData
            var logStopwatch = Stopwatch.StartNew();
            logger.Information(
                "Deal: {Deal} ExtID:{ExternalID} Login:{Login} Dealer:{Dealer} Order:{Order} " +
                "Symbol:{Symbol} Action:{Action} Entry:{Entry} Reason:{Reason} " +
                "Price:{Price} Vol:{Volume} VolClosed:{VolumeClosed} " +
                "Profit:{Profit} ProfitRaw:{ProfitRaw} Storage:{Storage} Comm:{Commission} Fee:{Fee} " +
                "SL:{PriceSL} TP:{PriceTP} PricePos:{PricePosition} " +
                "Time:{Time} TimeMsc:{TimeMsc} " +
                "RateProfit:{RateProfit} RateMargin:{RateMargin} " +
                "TickVal:{TickValue} TickSize:{TickSize} " +
                "Bid:{MarketBid} Ask:{MarketAsk} Last:{MarketLast} " +
                "ExpertID:{ExpertID} PosID:{PositionID} Comment:{Comment}",
                deal.Deal, deal.ExternalId, deal.Login, deal.Dealer, deal.Order,
                deal.Symbol, deal.Action, deal.Entry, deal.Reason,
                deal.Price, deal.Volume, deal.VolumeClosed,
                deal.Profit, deal.ProfitRaw, deal.Storage, deal.Commission, deal.Fee,
                deal.PriceSl, deal.PriceTp, deal.PricePosition,
                deal.Time, deal.TimeMsc,
                deal.RateProfit, deal.RateMargin,
                deal.TickValue, deal.TickSize,
                deal.MarketBid, deal.MarketAsk, deal.MarketLast,
                deal.ExpertId, deal.PositionId, deal.Comment);
            logStopwatch.Stop();

            logTimings.Add(logStopwatch.Elapsed.TotalMilliseconds);
            totalLogs++;

            // Display progress every 1000 logs
            if (totalLogs % 1000 == 0)
            {
                var elapsed = stopwatch.Elapsed.TotalSeconds;
                var rate = totalLogs / elapsed;
                Console.WriteLine($"Progress: {totalLogs:N0} logs, {elapsed:F1}s elapsed, {rate:F2} logs/sec");
            }

            // Random delay
            if (opts.MaxIntervalMs > 0)
            {
                var delayMs = random.Next(opts.MinIntervalMs, opts.MaxIntervalMs + 1);
                if (delayMs > 0)
                {
                    await Task.Delay(delayMs);
                }
            }
        }

        stopwatch.Stop();

        // Calculate statistics
        logTimings.Sort();
        var stats = new PerformanceStats
        {
            TotalLogs = totalLogs,
            TotalDuration = stopwatch.Elapsed,
            LogsPerSecond = totalLogs / stopwatch.Elapsed.TotalSeconds,
            AverageLogTimeMs = logTimings.Average(),
            MinLogTimeMs = logTimings.Min(),
            MaxLogTimeMs = logTimings.Max(),
            Percentile95Ms = GetPercentile(logTimings, 0.95),
            Percentile99Ms = GetPercentile(logTimings, 0.99)
        };

        return stats;
    }

    private static double GetPercentile(List<double> sortedValues, double percentile)
    {
        if (sortedValues.Count == 0) return 0;

        var index = (int)Math.Ceiling(percentile * sortedValues.Count) - 1;
        index = Math.Max(0, Math.Min(sortedValues.Count - 1, index));
        return sortedValues[index];
    }

    private class PerformanceStats
    {
        public long TotalLogs { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public double LogsPerSecond { get; set; }
        public double AverageLogTimeMs { get; set; }
        public double MinLogTimeMs { get; set; }
        public double MaxLogTimeMs { get; set; }
        public double Percentile95Ms { get; set; }
        public double Percentile99Ms { get; set; }
    }
}
