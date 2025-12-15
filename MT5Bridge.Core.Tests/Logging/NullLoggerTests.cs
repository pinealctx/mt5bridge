using MT5Bridge.Core.Logging;

namespace MT5Bridge.Core.Tests.Logging;

public class NullLoggerTests
{
    [Fact]
    public void Instance_ShouldReturnSameInstance()
    {
        var logger1 = NullLogger.Instance;
        var logger2 = NullLogger.Instance;

        Assert.Same(logger1, logger2);
    }

    [Fact]
    public void Debug_ShouldNotThrow()
    {
        var logger = NullLogger.Instance;

        var exception = Record.Exception(() => logger.Debug("Test message"));

        Assert.Null(exception);
    }

    [Fact]
    public void Info_ShouldNotThrow()
    {
        var logger = NullLogger.Instance;

        var exception = Record.Exception(() => logger.Info("Test message"));

        Assert.Null(exception);
    }

    [Fact]
    public void Warn_ShouldNotThrow()
    {
        var logger = NullLogger.Instance;

        var exception = Record.Exception(() => logger.Warn("Test message"));

        Assert.Null(exception);
    }

    [Fact]
    public void Error_WithMessage_ShouldNotThrow()
    {
        var logger = NullLogger.Instance;

        var exception = Record.Exception(() => logger.Error("Test error"));

        Assert.Null(exception);
    }

    [Fact]
    public void Error_WithMessageAndException_ShouldNotThrow()
    {
        var logger = NullLogger.Instance;
        var testException = new Exception("Test exception");

        var exception = Record.Exception(() => logger.Error("Test error", testException));

        Assert.Null(exception);
    }

    [Fact]
    public void NullLoggerFactory_Instance_ShouldReturnSameInstance()
    {
        var factory1 = NullLoggerFactory.Instance;
        var factory2 = NullLoggerFactory.Instance;

        Assert.Same(factory1, factory2);
    }

    [Fact]
    public void NullLoggerFactory_CreateLogger_ShouldReturnNullLogger()
    {
        var factory = NullLoggerFactory.Instance;

        var logger = factory.CreateLogger("TestLogger");

        Assert.Same(NullLogger.Instance, logger);
    }

    [Fact]
    public void NullLoggerFactory_CreateLogger_WithDifferentNames_ShouldReturnSameInstance()
    {
        var factory = NullLoggerFactory.Instance;

        var logger1 = factory.CreateLogger("Logger1");
        var logger2 = factory.CreateLogger("Logger2");

        Assert.Same(logger1, logger2);
    }
}
