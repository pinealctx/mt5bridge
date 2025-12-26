using Xunit;
using MT5Bridge.Manager.Sinks;

namespace MT5Bridge.Manager.Tests;

public class MT5ManagerSinkTests
{
    [Fact]
    public void OnDisconnect_ShouldInvokeAction()
    {
        // Arrange
        bool invoked = false;
        using var sink = new MT5ManagerSink(() => invoked = true);

        // Act
        sink.OnDisconnect();

        // Assert
        Assert.True(invoked);
    }
}
