using MT5Bridge.Core.Logging;

namespace MT5Bridge.Core.Tests.Logging;

public class LoggerExtensionsTests
{
    private class TestLogger : ILogger
    {
        public List<string> Messages { get; } = new();

        public void Debug(string message) => Messages.Add($"DEBUG: {message}");
        public void Info(string message) => Messages.Add($"INFO: {message}");
        public void Warn(string message) => Messages.Add($"WARN: {message}");
        public void Error(string message) => Messages.Add($"ERROR: {message}");
        public void Error(string message, Exception exception) => Messages.Add($"ERROR: {message} - {exception.Message}");
    }

    [Fact]
    public void Debug_WithFormatting_ShouldFormatCorrectly()
    {
        var logger = new TestLogger();

        logger.Debug("User {0} logged in at {1}", "admin", "10:30");

        Assert.Single(logger.Messages);
        Assert.Equal("DEBUG: User admin logged in at 10:30", logger.Messages[0]);
    }

    [Fact]
    public void Info_WithFormatting_ShouldFormatCorrectly()
    {
        var logger = new TestLogger();

        logger.Info("Processing {0} items", 42);

        Assert.Single(logger.Messages);
        Assert.Equal("INFO: Processing 42 items", logger.Messages[0]);
    }

    [Fact]
    public void Warn_WithFormatting_ShouldFormatCorrectly()
    {
        var logger = new TestLogger();

        logger.Warn("Retry attempt {0} of {1}", 3, 5);

        Assert.Single(logger.Messages);
        Assert.Equal("WARN: Retry attempt 3 of 5", logger.Messages[0]);
    }

    [Fact]
    public void Error_WithFormatting_ShouldFormatCorrectly()
    {
        var logger = new TestLogger();

        logger.Error("Failed to connect to {0}:{1}", "localhost", 8080);

        Assert.Single(logger.Messages);
        Assert.Equal("ERROR: Failed to connect to localhost:8080", logger.Messages[0]);
    }

    [Fact]
    public void Debug_WithMultipleArgs_ShouldFormatCorrectly()
    {
        var logger = new TestLogger();

        logger.Debug("Values: {0}, {1}, {2}, {3}", 1, 2.5, "test", true);

        Assert.Contains("1", logger.Messages[0]);
        Assert.Contains("2.5", logger.Messages[0]);
        Assert.Contains("test", logger.Messages[0]);
        Assert.Contains("True", logger.Messages[0]);
    }

    [Fact]
    public void Info_WithNoArgs_ShouldHandleCorrectly()
    {
        var logger = new TestLogger();

        logger.Info("Simple message without formatting");

        Assert.Single(logger.Messages);
        Assert.Equal("INFO: Simple message without formatting", logger.Messages[0]);
    }

    [Fact]
    public void ExtensionMethods_ShouldNotInterfereWithBaseImplementation()
    {
        var logger = new TestLogger();

        // Call base method
        logger.Info("Base method");

        // Call extension method
        logger.Info("Extension {0}", "method");

        Assert.Equal(2, logger.Messages.Count);
        Assert.Equal("INFO: Base method", logger.Messages[0]);
        Assert.Equal("INFO: Extension method", logger.Messages[1]);
    }
}
