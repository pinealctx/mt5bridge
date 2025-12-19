using MT5Bridge.MT5.Core.Models;
using Xunit;
using Xunit.Abstractions;

namespace MT5Bridge.MT5.Core.Tests;

public class JsonSerializationTests
{
    private readonly ITestOutputHelper _output;

    public JsonSerializationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void CompareSerializationModes()
    {
        // 1. Prepare a sample DealModel
        var deal = new DealModel
        {
            Deal = 12345678,
            Login = 888888,
            Symbol = "EURUSD",
            Action = DealAction.Buy,
            Entry = EntryFlag.In,
            Reason = DealReason.Expert,
            Price = 1.08505,
            Volume = 10000, // 0.1 lot
            Time = 1702982400,
            Comment = "Test Deal",
            ApiData = new List<ApiDataModel>
            {
                new() { AppId = 1, Id = 10, Value = 999 }
            }
        };

        // 2. Fast JSON (For Network/DB)
        string fastJson = MT5JsonSerializer.ToFastJson(deal);
        _output.WriteLine("=== Fast JSON (Network/DB) ===");
        _output.WriteLine(fastJson);

        // 3. Readable JSON (For Logging)
        string readableJson = MT5JsonSerializer.ToReadableJson(deal);
        _output.WriteLine("\n=== Readable JSON (Logging) ===");
        _output.WriteLine(readableJson);

        // 4. Assertions
        Assert.Contains("\"action\":0", fastJson); // Enum as number in fast mode
        Assert.Contains("\"action\": \"Buy\"", readableJson); // Enum as string in readable mode
        Assert.Contains("\n", readableJson); // Indented in readable mode
        Assert.DoesNotContain("\n", fastJson); // Compact in fast mode

        // 5. Round-trip test
        var deserialized = MT5JsonSerializer.FromJson<DealModel>(fastJson);
        Assert.NotNull(deserialized);
        Assert.Equal(deal.Deal, deserialized.Deal);
        Assert.Equal(deal.Action, deserialized.Action);
        Assert.Equal(deal.Symbol, deserialized.Symbol);
        Assert.Single(deserialized.ApiData);
        Assert.Equal(deal.ApiData[0].Value, deserialized.ApiData[0].Value);
    }

    [Fact]
    public void OrderModelSerializationTest()
    {
        var order = new OrderModel
        {
            Order = 987654,
            Symbol = "GBPUSD",
            Type = OrderType.BuyLimit,
            State = OrderState.Placed,
            PriceOrder = 1.2500,
            VolumeInitial = 20000
        };

        string fastJson = MT5JsonSerializer.ToFastJson(order);
        string readableJson = MT5JsonSerializer.ToReadableJson(order);

        _output.WriteLine("=== Order Fast JSON ===");
        _output.WriteLine(fastJson);
        _output.WriteLine("\n=== Order Readable JSON ===");
        _output.WriteLine(readableJson);

        Assert.Contains("\"type\":2", fastJson);
        Assert.Contains("\"type\": \"BuyLimit\"", readableJson);
    }

    [Fact]
    public void PositionModelSerializationTest()
    {
        var position = new PositionModel
        {
            Position = 555666,
            Symbol = "XAUUSD",
            Action = PositionAction.Sell,
            PriceOpen = 2030.50,
            Volume = 5000,
            ActivationFlags = TradeActivationFlags.NoSL | TradeActivationFlags.NoTP
        };

        string fastJson = MT5JsonSerializer.ToFastJson(position);
        string readableJson = MT5JsonSerializer.ToReadableJson(position);

        _output.WriteLine("=== Position Fast JSON ===");
        _output.WriteLine(fastJson);
        _output.WriteLine("\n=== Position Readable JSON ===");
        _output.WriteLine(readableJson);

        // Flags test
        Assert.Contains("\"activationFlags\":24", fastJson); // 8 | 16 = 24
        Assert.Contains("\"activationFlags\": \"NoSL, NoTP\"", readableJson);
    }
}
