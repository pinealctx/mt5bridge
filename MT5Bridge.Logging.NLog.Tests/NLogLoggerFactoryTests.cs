using Xunit;
using CoreLogger = MT5Bridge.Core.Logging.ILogger;

namespace MT5Bridge.Logging.NLog.Tests;

public class NLogLoggerFactoryTests
{
    [Fact]
    public void CreateLogger_WithName_ShouldReturnNLogAdapter()
    {
        // Arrange
        var factory = new NLogLoggerFactory();
        var loggerName = "TestLogger";

        // Act
        var logger = factory.CreateLogger(loggerName);

        // Assert
        Assert.NotNull(logger);
        Assert.IsAssignableFrom<CoreLogger>(logger);
        Assert.IsType<NLogAdapter>(logger);
    }

    [Fact]
    public void CreateLogger_WithEmptyName_ShouldStillWork()
    {
        // Arrange
        var factory = new NLogLoggerFactory();

        // Act
        var logger = factory.CreateLogger("");

        // Assert
        Assert.NotNull(logger);
        Assert.IsType<NLogAdapter>(logger);
    }

    [Fact]
    public void CreateLogger_WithDifferentNames_ShouldReturnDifferentInstances()
    {
        // Arrange
        var factory = new NLogLoggerFactory();

        // Act
        var logger1 = factory.CreateLogger("Logger1");
        var logger2 = factory.CreateLogger("Logger2");

        // Assert
        Assert.NotSame(logger1, logger2);
    }

    [Fact]
    public void CreateLogger_WithGenericType_ShouldReturnNLogAdapter()
    {
        // Arrange
        var factory = new NLogLoggerFactory();

        // Act
        var logger = factory.CreateLogger<NLogLoggerFactoryTests>();

        // Assert
        Assert.NotNull(logger);
        Assert.IsAssignableFrom<CoreLogger>(logger);
        Assert.IsType<NLogAdapter>(logger);
    }

    [Fact]
    public void CreateLogger_WithSameName_ShouldReturnNewInstanceEachTime()
    {
        // Arrange
        var factory = new NLogLoggerFactory();
        var loggerName = "TestLogger";

        // Act
        var logger1 = factory.CreateLogger(loggerName);
        var logger2 = factory.CreateLogger(loggerName);

        // Assert
        Assert.NotSame(logger1, logger2);
    }

    [Fact]
    public void CreateLogger_CalledMultipleTimes_ShouldNotThrow()
    {
        // Arrange
        var factory = new NLogLoggerFactory();

        // Act & Assert - should not throw
        for (int i = 0; i < 100; i++)
        {
            var logger = factory.CreateLogger($"Logger{i}");
            Assert.NotNull(logger);
        }
    }
}
