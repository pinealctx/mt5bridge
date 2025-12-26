using MT5Bridge.Manager.Managers;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace MT5Bridge.Manager.Tests;

/// <summary>
/// Tests for generic event handler registration (RegisterDealHandler<T>, etc.).
/// 测试泛型事件处理器注册（RegisterDealHandler<T> 等）。
/// 
/// Note: These tests only verify handler registration doesn't throw.
/// Real event firing and MT5 SDK integration requires a live MT5 Server connection.
/// 
/// 注意：这些测试仅验证处理器注册不会抛出异常。
/// 真实的事件触发和 MT5 SDK 集成需要实时的 MT5 Server 连接。
/// </summary>
public class GenericEventHandlerRegistrationTests
{
    private readonly ITestOutputHelper _output;
    private readonly ILogger _logger;

    public GenericEventHandlerRegistrationTests(ITestOutputHelper output)
    {
        _output = output;
        _logger = new LoggerConfiguration()
            .WriteTo.TestOutput(output)
            .CreateLogger();
    }

    [Fact]
    public void CreateManager_ShouldSucceed()
    {
        // Arrange & Act
        using var manager = new MT5Manager(_logger);

        // Assert
        Assert.NotNull(manager);
        _output.WriteLine("Manager created successfully");
    }

    [Fact]
    public void DisposeManager_ShouldNotThrow()
    {
        // Arrange
        var manager = new MT5Manager(_logger);

        // Act & Assert - Should not throw
        manager.Dispose();

        _output.WriteLine("Manager disposed successfully");
    }

    [Fact]
    public void RegisterDealHandler_ShouldNotThrow()
    {
        // Arrange
        using var manager = new MT5Manager(_logger);

        // Act
        manager.RegisterDealHandler(
            onAdd: deal => _output.WriteLine($"Deal: {deal.Deal} {deal.Symbol}")
        );

        // Assert
        _output.WriteLine("POCO deal handler registered successfully");
    }

    [Fact]
    public void RegisterDealProtoHandler_ShouldNotThrow()
    {
        // Arrange
        using var manager = new MT5Manager(_logger);

        // Act
        manager.RegisterDealProtoHandler(
            onAdd: deal => _output.WriteLine($"Proto Deal Add: {deal.Deal} {deal.Symbol}"),
            onUpdate: deal => _output.WriteLine($"Proto Deal Update: {deal.Deal}"),
            onDelete: deal => _output.WriteLine($"Proto Deal Delete: {deal.Deal}")
        );

        // Assert
        _output.WriteLine("Proto deal handler registered successfully");
    }

    [Fact]
    public void RegisterOrderHandler_ShouldNotThrow()
    {
        // Arrange
        using var manager = new MT5Manager(_logger);

        // Act
        manager.RegisterOrderHandler(
            onAdd: order => _output.WriteLine($"Order: {order.Order}")
        );

        // Assert
        _output.WriteLine("POCO order handler registered successfully");
    }

    [Fact]
    public void RegisterOrderProtoHandler_ShouldNotThrow()
    {
        // Arrange
        using var manager = new MT5Manager(_logger);

        // Act
        manager.RegisterOrderProtoHandler(
            onAdd: order => _output.WriteLine($"Proto Order: {order.Order}")
        );

        // Assert
        _output.WriteLine("Proto order handler registered successfully");
    }

    [Fact]
    public void RegisterPositionHandler_ShouldNotThrow()
    {
        // Arrange
        using var manager = new MT5Manager(_logger);

        // Act
        manager.RegisterPositionHandler(
            onAdd: pos => _output.WriteLine($"Position: {pos.Position}")
        );

        // Assert
        _output.WriteLine("POCO position handler registered successfully");
    }

    [Fact]
    public void RegisterPositionProtoHandler_ShouldNotThrow()
    {
        // Arrange
        using var manager = new MT5Manager(_logger);

        // Act
        manager.RegisterPositionProtoHandler(
            onAdd: pos => _output.WriteLine($"Proto Position: {pos.Position}")
        );

        // Assert
        _output.WriteLine("Proto position handler registered successfully");
    }

    [Fact]
    public void RegisterMultipleHandlers_ShouldNotThrow()
    {
        // Arrange
        using var manager = new MT5Manager(_logger);

        // Act - Register multiple handlers
        manager.RegisterDealHandler(onAdd: _ => { });
        manager.RegisterDealProtoHandler(onAdd: _ => { });
        manager.RegisterOrderHandler(onAdd: _ => { });
        manager.RegisterOrderProtoHandler(onAdd: _ => { });
        manager.RegisterPositionHandler(onAdd: _ => { });
        manager.RegisterPositionProtoHandler(onAdd: _ => { });

        // Assert
        _output.WriteLine("Multiple handlers registered successfully");
    }

    [Fact]
    public void RegisterHandlerWithAllCallbacks_ShouldNotThrow()
    {
        // Arrange
        using var manager = new MT5Manager(_logger);

        // Act
        manager.RegisterDealHandler(
            onAdd: deal => _output.WriteLine($"Add: {deal.Deal}"),
            onUpdate: deal => _output.WriteLine($"Update: {deal.Deal}"),
            onDelete: deal => _output.WriteLine($"Delete: {deal.Deal}")
        );

        // Assert
        _output.WriteLine("Handler with all callbacks registered successfully");
    }

    [Fact]
    public void RegisterHandlerWithNullCallbacks_ShouldNotThrow()
    {
        // Arrange
        using var manager = new MT5Manager(_logger);

        // Act - Null callbacks are valid
        manager.RegisterDealHandler(
            onAdd: null,
            onUpdate: null,
            onDelete: null
        );

        // Assert
        _output.WriteLine("Handler with null callbacks registered successfully");
    }
}
