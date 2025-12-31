using MT5Bridge.Manager.Managers;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace MT5Bridge.Manager.Tests;

/// <summary>
/// Tests for generic event handler registration (RegisterDealHandler<T>, etc.).
/// 
/// Note: These tests only verify handler registration doesn't throw.
/// Real event firing and MT5 SDK integration requires a live MT5 Server connection.
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
        var result = manager.RegisterDealHandler(
            onAdd: deal => _output.WriteLine($"Deal: {deal.Deal} {deal.Symbol}")
        );

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess, result.Message);
        _output.WriteLine("POCO deal handler registered successfully");
    }

    [Fact]
    public void RegisterDealProtoHandler_ShouldNotThrow()
    {
        // Arrange
        using var manager = new MT5Manager(_logger);

        // Act
        var result = manager.RegisterDealProtoHandler(
            onAdd: deal => _output.WriteLine($"Proto Deal Add: {deal.Deal} {deal.Symbol}"),
            onUpdate: deal => _output.WriteLine($"Proto Deal Update: {deal.Deal}"),
            onDelete: deal => _output.WriteLine($"Proto Deal Delete: {deal.Deal}")
        );

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess, result.Message);
        _output.WriteLine("Proto deal handler registered successfully");
    }

    [Fact]
    public void RegisterOrderHandler_ShouldNotThrow()
    {
        // Arrange
        using var manager = new MT5Manager(_logger);

        // Act
        var result = manager.RegisterOrderHandler(
            onAdd: order => _output.WriteLine($"Order: {order.Order}")
        );

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess, result.Message);
        _output.WriteLine("POCO order handler registered successfully");
    }

    [Fact]
    public void RegisterOrderProtoHandler_ShouldNotThrow()
    {
        // Arrange
        using var manager = new MT5Manager(_logger);

        // Act
        var result = manager.RegisterOrderProtoHandler(
            onAdd: order => _output.WriteLine($"Proto Order: {order.Order}")
        );

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess, result.Message);
        _output.WriteLine("Proto order handler registered successfully");
    }

    [Fact]
    public void RegisterPositionHandler_ShouldNotThrow()
    {
        // Arrange
        using var manager = new MT5Manager(_logger);

        // Act
        var result = manager.RegisterPositionHandler(
            onAdd: pos => _output.WriteLine($"Position: {pos.Position}")
        );

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess, result.Message);
        _output.WriteLine("POCO position handler registered successfully");
    }

    [Fact]
    public void RegisterPositionProtoHandler_ShouldNotThrow()
    {
        // Arrange
        using var manager = new MT5Manager(_logger);

        // Act
        var result = manager.RegisterPositionProtoHandler(
            onAdd: pos => _output.WriteLine($"Proto Position: {pos.Position}")
        );

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess, result.Message);
        _output.WriteLine("Proto position handler registered successfully");
    }

    [Fact]
    public void RegisterMultipleHandlers_ShouldNotThrow()
    {
        // Arrange
        using var manager = new MT5Manager(_logger);

        // Act - Register multiple handlers
        var dealResult = manager.RegisterDealHandler(onAdd: _ => { });
        var dealProtoResult = manager.RegisterDealProtoHandler(onAdd: _ => { });
        var orderResult = manager.RegisterOrderHandler(onAdd: _ => { });
        var orderProtoResult = manager.RegisterOrderProtoHandler(onAdd: _ => { });
        var posResult = manager.RegisterPositionHandler(onAdd: _ => { });
        var posProtoResult = manager.RegisterPositionProtoHandler(onAdd: _ => { });

        // Assert
        Assert.True(dealResult.IsSuccess, dealResult.Message);
        Assert.True(dealProtoResult.IsSuccess, dealProtoResult.Message);
        Assert.True(orderResult.IsSuccess, orderResult.Message);
        Assert.True(orderProtoResult.IsSuccess, orderProtoResult.Message);
        Assert.True(posResult.IsSuccess, posResult.Message);
        Assert.True(posProtoResult.IsSuccess, posProtoResult.Message);
        _output.WriteLine("Multiple handlers registered successfully");
    }

    [Fact]
    public void RegisterHandlerWithAllCallbacks_ShouldNotThrow()
    {
        // Arrange
        using var manager = new MT5Manager(_logger);

        // Act
        var result = manager.RegisterDealHandler(
            onAdd: deal => _output.WriteLine($"Add: {deal.Deal}"),
            onUpdate: deal => _output.WriteLine($"Update: {deal.Deal}"),
            onDelete: deal => _output.WriteLine($"Delete: {deal.Deal}")
        );

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess, result.Message);
        _output.WriteLine("Handler with all callbacks registered successfully");
    }

    [Fact]
    public void RegisterHandlerWithNullCallbacks_ShouldNotThrow()
    {
        // Arrange
        using var manager = new MT5Manager(_logger);

        // Act - Null callbacks are valid
        var result = manager.RegisterDealHandler(
            onAdd: null,
            onUpdate: null,
            onDelete: null
        );

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess, result.Message);
        _output.WriteLine("Handler with null callbacks registered successfully");
    }
}
