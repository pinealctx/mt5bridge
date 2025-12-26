using MT5Bridge.Manager.Models;
using Proto = MT5Bridge.Manager.Models.Proto;
using MT5Bridge.Manager.Models.Proto;
using Xunit;
using Xunit.Abstractions;

namespace MT5Bridge.Manager.Tests;

/// <summary>
/// Tests for Protobuf model serialization and extensions
/// Protobuf models are designed for efficient binary serialization
/// </summary>
public class ProtobufSerializationTests
{
    private readonly ITestOutputHelper _output;

    public ProtobufSerializationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ProtoDealModel_Creation_ShouldWork()
    {
        // Arrange & Act
        var protoDeal = new Proto.DealModel
        {
            Deal = 12345678,
            Login = 888888,
            Symbol = "EURUSD",
            Action = Proto.DealAction.Buy,
            Entry = Proto.EntryFlag.In,
            Reason = Proto.DealReason.Expert,
            Price = 1.08505,
            Volume = 10000,
            Time = 1702982400,
            Comment = "Test Deal"
        };
        protoDeal.ApiData.Add(new Proto.ApiDataModel { AppId = 1, Id = 10, Value = 999 });

        // Assert
        Assert.Equal(12345678UL, protoDeal.Deal);
        Assert.Equal(888888UL, protoDeal.Login);
        Assert.Equal("EURUSD", protoDeal.Symbol);
        Assert.Equal(Proto.DealAction.Buy, protoDeal.Action);
        Assert.Equal(Proto.EntryFlag.In, protoDeal.Entry);
        Assert.Single(protoDeal.ApiData);

        _output.WriteLine($"Created Proto Deal: {protoDeal.Deal} {protoDeal.Symbol}");
    }

    [Fact]
    public void ProtoDealModel_JsonSerialization_ShouldWork()
    {
        // Arrange
        var protoDeal = new Proto.DealModel
        {
            Deal = 12345678,
            Symbol = "EURUSD",
            Action = Proto.DealAction.Buy,
            Price = 1.08505
        };

        // Act
        string json = protoDeal.ToReadableJson();

        // Assert
        Assert.NotEmpty(json);
        Assert.Contains("deal", json.ToLower());
        Assert.Contains("eurusd", json.ToLower());

        _output.WriteLine("=== Protobuf Readable JSON ===");
        _output.WriteLine(json);
    }

    [Fact]
    public void ProtoOrderModel_Creation_ShouldWork()
    {
        // Arrange & Act
        var protoOrder = new Proto.OrderModel
        {
            Order = 987654,
            Symbol = "GBPUSD",
            Type = Proto.OrderType.BuyLimit,
            State = Proto.OrderState.Placed,
            PriceOrder = 1.2500,
            VolumeInitial = 20000
        };

        // Assert
        Assert.Equal(987654UL, protoOrder.Order);
        Assert.Equal("GBPUSD", protoOrder.Symbol);
        Assert.Equal(Proto.OrderType.BuyLimit, protoOrder.Type);
        Assert.Equal(Proto.OrderState.Placed, protoOrder.State);

        _output.WriteLine($"Created Proto Order: {protoOrder.Order}");
    }

    [Fact]
    public void ProtoPositionModel_Creation_ShouldWork()
    {
        // Arrange & Act
        var protoPosition = new Proto.PositionModel
        {
            Position = 555666,
            Symbol = "XAUUSD",
            Action = Proto.PositionAction.Sell,
            PriceOpen = 2030.50,
            Volume = 5000,
            ActivationFlags = Proto.TradeActivationFlags.TradeActivationNoSl | Proto.TradeActivationFlags.TradeActivationNoTp
        };

        // Assert
        Assert.Equal(555666UL, protoPosition.Position);
        Assert.Equal("XAUUSD", protoPosition.Symbol);
        Assert.Equal(Proto.PositionAction.Sell, protoPosition.Action);
        Assert.True(protoPosition.ActivationFlags.HasFlag(Proto.TradeActivationFlags.TradeActivationNoSl));

        _output.WriteLine($"Created Proto Position: {protoPosition.Position}");
    }

    [Fact]
    public void ProtoUserModel_Creation_ShouldWork()
    {
        // Arrange & Act
        var protoUser = new Proto.UserModel
        {
            Login = 12345,
            FirstName = "John",
            LastName = "Doe",
            Group = "demo\\main",
            Email = "john@example.com",
            Country = "USA",
            City = "New York"
        };

        // Assert
        Assert.Equal(12345UL, protoUser.Login);
        Assert.Equal("John", protoUser.FirstName);
        Assert.Equal("Doe", protoUser.LastName);
        Assert.Equal("demo\\main", protoUser.Group);

        _output.WriteLine($"Created Proto User: {protoUser.Login}");
    }

    [Fact]
    public void ProtoAccountModel_Creation_ShouldWork()
    {
        // Arrange & Act
        var protoAccount = new Proto.AccountModel
        {
            Login = 12345,
            Balance = 10000.50,
            Credit = 500.00,
            Equity = 10500.50,
            Margin = 2000.00,
            MarginFree = 8500.50
        };

        // Assert
        Assert.Equal(12345UL, protoAccount.Login);
        Assert.Equal(10000.50, protoAccount.Balance);
        Assert.Equal(10500.50, protoAccount.Equity);

        _output.WriteLine($"Created Proto Account: {protoAccount.Login}");
    }

    [Fact]
    public void ProtoGroupModel_Creation_ShouldWork()
    {
        // Arrange & Act
        var protoGroup = new Proto.GroupModel
        {
            Group = "demo\\main",
            Server = 1,
            Currency = "USD",
            CurrencyDigits = 2
        };

        // Assert
        Assert.Equal("demo\\main", protoGroup.Group);
        Assert.Equal(1UL, protoGroup.Server);
        Assert.Equal("USD", protoGroup.Currency);

        _output.WriteLine($"Created Proto Group: {protoGroup.Group}");
    }

    [Fact]
    public void ProtobufModels_JsonSerialization_ShouldWork()
    {
        // Arrange
        var protoDeal = new Proto.DealModel
        {
            Deal = 12345678,
            Login = 888888,
            Symbol = "EURUSD",
            Action = Proto.DealAction.Buy,
            Price = 1.08505,
            Volume = 10000
        };

        var protoOrder = new Proto.OrderModel
        {
            Order = 987654,
            Symbol = "GBPUSD",
            Type = Proto.OrderType.BuyLimit
        };

        // Act
        string dealJson = protoDeal.ToReadableJson();
        string orderJson = protoOrder.ToReadableJson();

        // Assert
        Assert.NotEmpty(dealJson);
        Assert.NotEmpty(orderJson);
        Assert.Contains("eurusd", dealJson.ToLower());
        Assert.Contains("gbpusd", orderJson.ToLower());

        _output.WriteLine("=== Proto Deal JSON ===");
        _output.WriteLine(dealJson);
        _output.WriteLine("\n=== Proto Order JSON ===");
        _output.WriteLine(orderJson);
    }
}
