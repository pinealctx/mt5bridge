using NLog;
using NLog.Config;
using NLog.Targets;
using Xunit;

namespace MT5Bridge.Logging.NLog.Tests;

public class NLogConfigurationTests
{
    [Fact]
    public void CreateBasicConfiguration_ShouldCreateValidConfig()
    {
        // Arrange & Act
        var config = new LoggingConfiguration();
        var consoleTarget = new ConsoleTarget("console");
        config.AddTarget(consoleTarget);
        config.AddRule(LogLevel.Debug, LogLevel.Fatal, consoleTarget);

        // Assert
        Assert.NotNull(config);
        Assert.Single(config.AllTargets);
        Assert.Single(config.LoggingRules);
    }

    [Fact]
    public void FileTarget_WithRolling_ShouldBeConfigurable()
    {
        // Arrange
        var config = new LoggingConfiguration();
        var fileTarget = new FileTarget("file")
        {
            FileName = "${basedir}/logs/test-${shortdate}.log",
            ArchiveEvery = FileArchivePeriod.Day,
            MaxArchiveFiles = 7
        };

        // Act
        config.AddTarget(fileTarget);
        config.AddRule(LogLevel.Debug, LogLevel.Fatal, fileTarget);

        // Assert
        Assert.NotNull(config);
        Assert.Single(config.AllTargets);
        Assert.Equal(FileArchivePeriod.Day, fileTarget.ArchiveEvery);
        Assert.Equal(7, fileTarget.MaxArchiveFiles);
    }

    [Fact]
    public void MultipleTargets_ShouldBeSupported()
    {
        // Arrange
        var config = new LoggingConfiguration();

        var consoleTarget = new ConsoleTarget("console");
        var fileTarget = new FileTarget("file")
        {
            FileName = "${basedir}/logs/test.log"
        };

        // Act
        config.AddTarget(consoleTarget);
        config.AddTarget(fileTarget);
        config.AddRule(LogLevel.Debug, LogLevel.Fatal, consoleTarget);
        config.AddRule(LogLevel.Info, LogLevel.Fatal, fileTarget);

        // Assert
        Assert.Equal(2, config.AllTargets.Count);
        Assert.Equal(2, config.LoggingRules.Count);
    }

    [Fact]
    public void DifferentArchivePeriods_ShouldBeConfigurable()
    {
        // Test Minute
        var minuteTarget = new FileTarget("minute")
        {
            ArchiveEvery = FileArchivePeriod.Minute
        };
        Assert.Equal(FileArchivePeriod.Minute, minuteTarget.ArchiveEvery);

        // Test Hour
        var hourTarget = new FileTarget("hour")
        {
            ArchiveEvery = FileArchivePeriod.Hour
        };
        Assert.Equal(FileArchivePeriod.Hour, hourTarget.ArchiveEvery);

        // Test Day
        var dayTarget = new FileTarget("day")
        {
            ArchiveEvery = FileArchivePeriod.Day
        };
        Assert.Equal(FileArchivePeriod.Day, dayTarget.ArchiveEvery);
    }

    [Fact]
    public void LoggerFactory_WithConfiguration_ShouldCreateWorkingLoggers()
    {
        // Arrange
        var config = new LoggingConfiguration();
        var consoleTarget = new ConsoleTarget("console");
        config.AddTarget(consoleTarget);
        config.AddRule(LogLevel.Debug, LogLevel.Fatal, consoleTarget);
        LogManager.Configuration = config;

        var factory = new NLogLoggerFactory();

        // Act
        var logger = factory.CreateLogger("TestLogger");

        // Assert
        Assert.NotNull(logger);

        // Should not throw
        logger.Debug("Test message");
        logger.Info("Test message");
        logger.Warn("Test message");
        logger.Error("Test message");
    }
}
