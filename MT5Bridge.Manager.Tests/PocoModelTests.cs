using MT5Bridge.Manager.Models;
using Xunit;

namespace MT5Bridge.Manager.Tests;

/// <summary>
/// Tests for POCO model properties and basic functionality
/// </summary>
public class PocoModelTests
{
    [Fact]
    public void DealModel_AllProperties_ShouldWork()
    {
        // Arrange & Act
        var deal = new DealModel
        {
            Deal = 12345,
            Login = 67890,
            Symbol = "EURUSD",
            Action = DealAction.Buy,
            Entry = EntryFlag.In,
            Reason = DealReason.Expert,
            Price = 1.08505,
            PriceSL = 1.08000,
            PriceTP = 1.09000,
            Volume = 10000,
            Profit = 150.50,
            Commission = -5.00,
            Storage = -2.00,
            Comment = "Test Deal"
        };

        // Assert
        Assert.Equal(12345UL, deal.Deal);
        Assert.Equal(67890UL, deal.Login);
        Assert.Equal("EURUSD", deal.Symbol);
        Assert.Equal(DealAction.Buy, deal.Action);
        Assert.Equal(EntryFlag.In, deal.Entry);
        Assert.Equal(DealReason.Expert, deal.Reason);
        Assert.Equal(1.08505, deal.Price);
        Assert.Equal(1.08000, deal.PriceSL);
        Assert.Equal(1.09000, deal.PriceTP);
        Assert.Equal(10000UL, deal.Volume);
        Assert.Equal(150.50, deal.Profit);
        Assert.Equal(-5.00, deal.Commission);
        Assert.Equal(-2.00, deal.Storage);
        Assert.Equal("Test Deal", deal.Comment);
    }

    [Fact]
    public void DealModel_ApiData_ShouldSupportMultipleItems()
    {
        // Arrange
        var deal = new DealModel { Deal = 123 };

        // Act
        deal.ApiData.Add(new ApiDataModel { AppId = 1, Id = 10, Value = 100 });
        deal.ApiData.Add(new ApiDataModel { AppId = 2, Id = 20, Value = 200 });

        // Assert
        Assert.Equal(2, deal.ApiData.Count);
        Assert.Equal(100UL, deal.ApiData[0].Value);
        Assert.Equal(200UL, deal.ApiData[1].Value);
    }

    [Fact]
    public void OrderModel_AllProperties_ShouldWork()
    {
        // Arrange & Act
        var order = new OrderModel
        {
            Order = 98765,
            Symbol = "GBPUSD",
            Type = OrderType.BuyLimit,
            State = OrderState.Placed,
            PriceOrder = 1.2500,
            PriceCurrent = 1.2480,
            PriceSL = 1.2400,
            PriceTP = 1.2600,
            VolumeInitial = 20000,
            VolumeCurrent = 20000,
            TimeSetup = 1702982400,
            Comment = "Test Order"
        };

        // Assert
        Assert.Equal(98765UL, order.Order);
        Assert.Equal("GBPUSD", order.Symbol);
        Assert.Equal(OrderType.BuyLimit, order.Type);
        Assert.Equal(OrderState.Placed, order.State);
        Assert.Equal(1.2500, order.PriceOrder);
        Assert.Equal(1.2480, order.PriceCurrent);
        Assert.Equal(1.2400, order.PriceSL);
        Assert.Equal(1.2600, order.PriceTP);
        Assert.Equal(20000UL, order.VolumeInitial);
    }

    [Fact]
    public void PositionModel_AllProperties_ShouldWork()
    {
        // Arrange & Act
        var position = new PositionModel
        {
            Position = 55566,
            Symbol = "XAUUSD",
            Action = PositionAction.Sell,
            PriceOpen = 2030.50,
            PriceCurrent = 2025.00,
            PriceSL = 2040.00,
            PriceTP = 2010.00,
            Volume = 5000,
            Profit = 275.00,
            Storage = -1.50,
            ActivationFlags = TradeActivationFlags.NoSL | TradeActivationFlags.NoTP
        };

        // Assert
        Assert.Equal(55566UL, position.Position);
        Assert.Equal("XAUUSD", position.Symbol);
        Assert.Equal(PositionAction.Sell, position.Action);
        Assert.Equal(2030.50, position.PriceOpen);
        Assert.Equal(2025.00, position.PriceCurrent);
        Assert.Equal(5000UL, position.Volume);
        Assert.Equal(275.00, position.Profit);
        Assert.True(position.ActivationFlags.HasFlag(TradeActivationFlags.NoSL));
        Assert.True(position.ActivationFlags.HasFlag(TradeActivationFlags.NoTP));
    }

    [Fact]
    public void AccountModel_AllProperties_ShouldWork()
    {
        // Arrange & Act
        var account = new AccountModel
        {
            Login = 12345,
            Balance = 10000.50,
            Credit = 500.00,
            Equity = 10500.50,
            Margin = 2000.00,
            MarginFree = 8500.50,
            MarginLevel = 525.025,
            Profit = 500.00,
            Storage = -10.00,
            Floating = 490.00,
            MarginLeverage = 100
        };

        // Assert
        Assert.Equal(12345UL, account.Login);
        Assert.Equal(10000.50, account.Balance);
        Assert.Equal(500.00, account.Credit);
        Assert.Equal(10500.50, account.Equity);
        Assert.Equal(2000.00, account.Margin);
        Assert.Equal(8500.50, account.MarginFree);
        Assert.Equal(525.025, account.MarginLevel);
        Assert.Equal(500.00, account.Profit);
        Assert.Equal(490.00, account.Floating);
        Assert.Equal(100.0, account.MarginLeverage);
    }

    [Fact]
    public void GroupModel_AllProperties_ShouldWork()
    {
        // Arrange & Act
        var group = new GroupModel
        {
            Group = "demo\\main",
            Server = 1,
            Currency = "USD",
            CurrencyDigits = 2,
            Company = "MetaQuotes",
            CompanyPage = "https://www.metaquotes.net"
        };

        // Assert
        Assert.Equal("demo\\main", group.Group);
        Assert.Equal(1U, group.Server);
        Assert.Equal("USD", group.Currency);
        Assert.Equal(2U, group.CurrencyDigits);
        Assert.Equal("MetaQuotes", group.Company);
        Assert.Equal("https://www.metaquotes.net", group.CompanyPage);
    }

    [Fact]
    public void ApiDataModel_AllProperties_ShouldWork()
    {
        // Arrange & Act
        var apiData = new ApiDataModel
        {
            AppId = 1,
            Id = 10,
            Value = 999
        };

        // Assert
        Assert.Equal((ushort)1, apiData.AppId);
        Assert.Equal((byte)10, apiData.Id);
        Assert.Equal(999UL, apiData.Value);
    }

    [Fact]
    public void TradeFlags_ShouldWork()
    {
        // Arrange
        var activationFlags = TradeActivationFlags.NoSL | TradeActivationFlags.NoTP;
        var modifyFlags = TradeModifyFlags.Admin | TradeModifyFlags.Manager;

        // Assert
        Assert.True(activationFlags.HasFlag(TradeActivationFlags.NoSL));
        Assert.True(activationFlags.HasFlag(TradeActivationFlags.NoTP));
        Assert.False(activationFlags.HasFlag(TradeActivationFlags.NoLimit));

        Assert.True(modifyFlags.HasFlag(TradeModifyFlags.Admin));
        Assert.True(modifyFlags.HasFlag(TradeModifyFlags.Manager));
        Assert.False(modifyFlags.HasFlag(TradeModifyFlags.Restore));
    }
}
