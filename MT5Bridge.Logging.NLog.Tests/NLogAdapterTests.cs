using NLog;
using NSubstitute;
using Xunit;
using CoreLogger = MT5Bridge.Core.Logging.ILogger;

namespace MT5Bridge.Logging.NLog.Tests;

public class NLogAdapterTests
{
    private readonly Logger _mockLogger;
    private readonly NLogAdapter _adapter;

    public NLogAdapterTests()
    {
        // Create a substitute for NLog.Logger
        _mockLogger = Substitute.For<Logger>();
        _adapter = new NLogAdapter(_mockLogger);
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new NLogAdapter(null!));
    }

    [Fact]
    public void Debug_ShouldCallNLogDebug()
    {
        // Arrange
        var message = "Debug message";

        // Act
        _adapter.Debug(message);

        // Assert
        _mockLogger.Received(1).Debug(message);
    }

    [Fact]
    public void Info_ShouldCallNLogInfo()
    {
        // Arrange
        var message = "Info message";

        // Act
        _adapter.Info(message);

        // Assert
        _mockLogger.Received(1).Info(message);
    }

    [Fact]
    public void Warn_ShouldCallNLogWarn()
    {
        // Arrange
        var message = "Warning message";

        // Act
        _adapter.Warn(message);

        // Assert
        _mockLogger.Received(1).Warn(message);
    }

    [Fact]
    public void Error_WithMessage_ShouldCallNLogError()
    {
        // Arrange
        var message = "Error message";

        // Act
        _adapter.Error(message);

        // Assert
        _mockLogger.Received(1).Error(message);
    }

    [Fact]
    public void Error_WithMessageAndException_ShouldCallNLogErrorWithException()
    {
        // Arrange
        var message = "Error with exception";
        var exception = new InvalidOperationException("Test exception");

        // Act
        _adapter.Error(message, exception);

        // Assert
        _mockLogger.Received(1).Error(exception, message);
    }

    [Fact]
    public void MultipleCallsToSameMethod_ShouldAllBeCalled()
    {
        // Arrange & Act
        _adapter.Debug("Message 1");
        _adapter.Debug("Message 2");
        _adapter.Debug("Message 3");

        // Assert
        _mockLogger.Received(1).Debug("Message 1");
        _mockLogger.Received(1).Debug("Message 2");
        _mockLogger.Received(1).Debug("Message 3");
    }

    [Fact]
    public void DifferentLogLevels_ShouldCallCorrectNLogMethods()
    {
        // Act
        _adapter.Debug("Debug");
        _adapter.Info("Info");
        _adapter.Warn("Warn");
        _adapter.Error("Error");

        // Assert - verify each was called exactly once
        _mockLogger.Received(1).Debug("Debug");
        _mockLogger.Received(1).Info("Info");
        _mockLogger.Received(1).Warn("Warn");
        _mockLogger.Received(1).Error("Error");

        // Verify no other log methods were called
        _mockLogger.DidNotReceive().Fatal(Arg.Any<string>());
        _mockLogger.DidNotReceive().Trace(Arg.Any<string>());
    }
}
