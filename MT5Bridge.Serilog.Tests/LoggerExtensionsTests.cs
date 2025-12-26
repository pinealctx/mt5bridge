using Serilog;
using Serilog.Core;
using Serilog.Events;
using Xunit;
using MT5Bridge.Serilog;
using MT5Bridge.Manager.Models;

namespace MT5Bridge.Serilog.Tests;

/// <summary>
/// Comprehensive tests for LoggerExtensions and ModelLogger
/// </summary>
public class LoggerExtensionsTests
{
    /// <summary>
    /// Simple in-memory sink for capturing log events
    /// </summary>
    private class TestSink : ILogEventSink
    {
        public List<LogEvent> Events { get; } = new();
        public void Emit(LogEvent logEvent) => Events.Add(logEvent);
    }

    // ===== BASIC LOGGER EXTENSION TESTS =====

    [Fact]
    public void WithModel_ShouldAttachModelToContext()
    {
        var sink = new TestSink();
        var logger = new LoggerConfiguration()
            .WriteTo.Sink(sink)
            .CreateLogger();

        var model = new DealModel
        {
            Deal = 1,
            Symbol = "EURUSD",
            Price = 1.0850,
            Volume = 100,
            Time = (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Action = DealAction.Buy,
            Comment = "Test"
        };

        logger.WithModel("DealData", model, FastJsonContext.Default.DealModel)
              .Information("Log with model");

        Assert.Single(sink.Events);
        var @event = sink.Events[0];
        Assert.Contains("DealData", @event.Properties.Keys);
    }

    [Fact]
    public void WithModelLazy_ShouldSkipSerializationWhenLevelDisabled()
    {
        var sink = new TestSink();
        var logger = new LoggerConfiguration()
            .MinimumLevel.Is(LogEventLevel.Information)
            .WriteTo.Sink(sink)
            .CreateLogger();

        var model = new DealModel
        {
            Deal = 2,
            Symbol = "GBPUSD",
            Price = 1.2750,
            Volume = 200,
            Time = (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Action = DealAction.Sell,
            Comment = "Debug Model"
        };

        // Try to attach model at Debug level (which is disabled), then log at Information level
        logger.WithModelLazy("DebugData", model, FastJsonContext.Default, LogEventLevel.Debug)
              .Information("This message should not include the model");

        Assert.Single(sink.Events);
        // DebugData should not be present since Debug level is disabled (lazy evaluation skipped serialization)
        Assert.DoesNotContain("DebugData", sink.Events[0].Properties.Keys);
    }

    [Fact]
    public void WithModelLazy_ShouldIncludeModelWhenLevelEnabled()
    {
        var sink = new TestSink();
        var logger = new LoggerConfiguration()
            .MinimumLevel.Is(LogEventLevel.Information)
            .WriteTo.Sink(sink)
            .CreateLogger();

        var model = new DealModel
        {
            Deal = 3,
            Symbol = "XAUUSD",
            Price = 2050.00,
            Volume = 100,
            Time = (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Action = DealAction.Buy,
            Comment = "Info Model"
        };

        logger.WithModelLazy("InfoData", model, FastJsonContext.Default, LogEventLevel.Information)
              .Information("Log with lazy model at enabled level");

        Assert.Single(sink.Events);
        Assert.Contains("InfoData", sink.Events[0].Properties.Keys);
    }

    [Fact]
    public void WithModelDirect_ShouldStoreAsJsonString()
    {
        var sink = new TestSink();
        var logger = new LoggerConfiguration()
            .WriteTo.Sink(sink)
            .CreateLogger();

        var model = new DealModel
        {
            Deal = 4,
            Symbol = "USDJPY",
            Price = 149.50,
            Volume = 50,
            Time = (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Action = DealAction.Buy,
            Comment = "Direct"
        };

        logger.WithModelDirect("DirectJson", model, FastJsonContext.Default.DealModel)
              .Information("Direct JSON log");

        Assert.Single(sink.Events);
        Assert.Contains("DirectJson", sink.Events[0].Properties.Keys);
    }

    [Fact]
    public void WithField_ShouldAttachSimpleField()
    {
        var sink = new TestSink();
        var logger = new LoggerConfiguration()
            .WriteTo.Sink(sink)
            .CreateLogger();

        logger.WithField("UserId", 42)
              .WithField("Action", "Login")
              .WithField("Success", true)
              .Information("User action");

        Assert.Single(sink.Events);
        var props = sink.Events[0].Properties;
        Assert.Contains("UserId", props.Keys);
        Assert.Contains("Action", props.Keys);
        Assert.Contains("Success", props.Keys);
    }

    // ===== CONVENIENCE METHODS =====

    [Fact]
    public void LogModelInfo_ShouldLogAtInformationLevel()
    {
        var sink = new TestSink();
        var logger = new LoggerConfiguration()
            .MinimumLevel.Is(LogEventLevel.Information)
            .WriteTo.Sink(sink)
            .CreateLogger();

        var model = new DealModel
        {
            Deal = 6,
            Symbol = "NZDUSD",
            Price = 0.6050,
            Volume = 250,
            Time = (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Action = DealAction.Sell,
            Comment = "Info"
        };

        logger.LogModelInfo("Info log", model, FastJsonContext.Default.DealModel);

        Assert.Single(sink.Events);
        Assert.Equal(LogEventLevel.Information, sink.Events[0].Level);
    }

    [Fact]
    public void LogModelDebug_ShouldSkipWhenDebugDisabled()
    {
        var sink = new TestSink();
        var logger = new LoggerConfiguration()
            .MinimumLevel.Is(LogEventLevel.Information)
            .WriteTo.Sink(sink)
            .CreateLogger();

        var model = new DealModel
        {
            Deal = 7,
            Symbol = "EURCAD",
            Price = 1.4500,
            Volume = 50,
            Time = (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Action = DealAction.Buy,
            Comment = "Debug"
        };

        logger.LogModelDebug("Debug log", model, FastJsonContext.Default.DealModel);

        // No events should be logged since Debug is disabled
        Assert.Empty(sink.Events);
    }

    [Fact]
    public void LogModelError_ShouldIncludeException()
    {
        var sink = new TestSink();
        var logger = new LoggerConfiguration()
            .WriteTo.Sink(sink)
            .CreateLogger();

        var ex = new InvalidOperationException("Test error");
        var model = new DealModel
        {
            Deal = 8,
            Symbol = "GBPJPY",
            Price = 184.50,
            Volume = 20,
            Time = (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Action = DealAction.Buy,
            Comment = "Error"
        };

        logger.LogModelError("Error occurred", model, FastJsonContext.Default.DealModel, ex);

        Assert.Single(sink.Events);
        Assert.NotNull(sink.Events[0].Exception);
        Assert.IsType<InvalidOperationException>(sink.Events[0].Exception);
    }

    // ===== MODEL LOGGER WRAPPER TESTS =====

    [Fact]
    public void ModelLogger_ShouldMaintainContext()
    {
        var sink = new TestSink();
        var baseLogger = new LoggerConfiguration()
            .WriteTo.Sink(sink)
            .CreateLogger();

        var modelLogger = new ModelLogger(baseLogger, LogEventLevel.Information);

        var model1 = new DealModel
        {
            Deal = 10,
            Symbol = "EURUSD",
            Price = 1.0850,
            Volume = 150,
            Time = (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Action = DealAction.Buy,
            Comment = "First"
        };

        var model2 = new DealModel
        {
            Deal = 11,
            Symbol = "GBPUSD",
            Price = 1.2750,
            Volume = 250,
            Time = (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Action = DealAction.Sell,
            Comment = "Second"
        };

        modelLogger.WithModel("Model1", model1, FastJsonContext.Default.DealModel)
                   .WithModel("Model2", model2, FastJsonContext.Default.DealModel)
                   .Write("Multiple models");

        Assert.Single(sink.Events);
        var props = sink.Events[0].Properties;
        Assert.Contains("Model1", props.Keys);
        Assert.Contains("Model2", props.Keys);
    }

    [Fact]
    public void ModelLogger_WithField_ShouldChain()
    {
        var sink = new TestSink();
        var baseLogger = new LoggerConfiguration()
            .WriteTo.Sink(sink)
            .CreateLogger();

        var modelLogger = new ModelLogger(baseLogger, LogEventLevel.Information);

        var model = new DealModel
        {
            Deal = 13,
            Symbol = "USDJPY",
            Price = 149.50,
            Volume = 100,
            Time = (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Action = DealAction.Buy,
            Comment = "Field Test"
        };

        modelLogger.WithModel("TestData", model, FastJsonContext.Default.DealModel)
                   .WithField("Status", "Complete")
                   .WithField("Count", 5)
                   .Write("Message with fields");

        Assert.Single(sink.Events);
        var props = sink.Events[0].Properties;
        Assert.Contains("TestData", props.Keys);
        Assert.Contains("Status", props.Keys);
        Assert.Contains("Count", props.Keys);
    }

    // ===== GLOBAL CONTEXT TESTS =====

    [Fact]
    public void DefaultContext_ShouldBeUsable()
    {
        LoggerExtensions.DefaultContext = FastJsonContext.Default;

        var sink = new TestSink();
        var logger = new LoggerConfiguration()
            .WriteTo.Sink(sink)
            .CreateLogger();

        var model = new DealModel
        {
            Deal = 20,
            Symbol = "EURUSD",
            Price = 1.0850,
            Volume = 100,
            Time = (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Action = DealAction.Buy,
            Comment = "Default Context"
        };

        logger.WithModel("DefaultTest", model, FastJsonContext.Default.DealModel)
              .Information("Using default context");

        Assert.Single(sink.Events);
        Assert.Contains("DefaultTest", sink.Events[0].Properties.Keys);

        // Cleanup
        LoggerExtensions.DefaultContext = null;
    }

    // ===== ERROR HANDLING TESTS =====

    [Fact]
    public void WithModel_NullModel_ShouldReturnUnchangedLogger()
    {
        var sink = new TestSink();
        var logger = new LoggerConfiguration()
            .WriteTo.Sink(sink)
            .CreateLogger();

        DealModel? nullModel = null;

        var result = logger.WithModel<DealModel?>("NullTest", nullModel, FastJsonContext.Default.DealModel!);
        Assert.NotNull(result);

        result.Information("Null model handled");

        Assert.Single(sink.Events);
        Assert.DoesNotContain("NullTest", sink.Events[0].Properties.Keys);
    }

    // ===== PERFORMANCE / INTEGRATION TESTS =====

    [Fact]
    public void HighFrequencyLogging_WithLazyEvaluation()
    {
        var sink = new TestSink();
        var logger = new LoggerConfiguration()
            .MinimumLevel.Is(LogEventLevel.Warning)  // Debug is disabled
            .WriteTo.Sink(sink)
            .CreateLogger();

        var model = new DealModel
        {
            Deal = 30,
            Symbol = "GBPUSD",
            Price = 1.2750,
            Volume = 200,
            Time = (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Action = DealAction.Sell,
            Comment = "Perf Test"
        };

        // Log many times at Debug level (should be skipped)
        for (int i = 0; i < 1000; i++)
        {
            logger.WithModelLazy("PerfData", model, FastJsonContext.Default, LogEventLevel.Debug)
                  .Debug("This should not be serialized");
        }

        // No events should be logged
        Assert.Empty(sink.Events);
    }

    [Fact]
    public void MultipleModelsInChain()
    {
        var sink = new TestSink();
        var logger = new LoggerConfiguration()
            .WriteTo.Sink(sink)
            .CreateLogger();

        var user = new DealModel
        {
            Deal = 1,
            Symbol = "USER",
            Price = 0,
            Volume = 1,
            Time = (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Action = DealAction.Buy,
            Comment = "User1"
        };

        var account = new DealModel
        {
            Deal = 2,
            Symbol = "ACCOUNT",
            Price = 5000,
            Volume = 1,
            Time = (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Action = DealAction.Buy,
            Comment = "Account1"
        };

        var transaction = new DealModel
        {
            Deal = 3,
            Symbol = "EURUSD",
            Price = 1.0850,
            Volume = 50,
            Time = (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Action = DealAction.Buy,
            Comment = "Trans1"
        };

        logger.WithModel("User", user, FastJsonContext.Default.DealModel)
              .WithModel("Account", account, FastJsonContext.Default.DealModel)
              .WithModel("Transaction", transaction, FastJsonContext.Default.DealModel)
              .WithField("Action", "Transfer")
              .WithField("Status", "Success")
              .Information("Complex transaction");

        Assert.Single(sink.Events);
        var props = sink.Events[0].Properties;
        Assert.Contains("User", props.Keys);
        Assert.Contains("Account", props.Keys);
        Assert.Contains("Transaction", props.Keys);
        Assert.Contains("Action", props.Keys);
        Assert.Contains("Status", props.Keys);
    }
}
