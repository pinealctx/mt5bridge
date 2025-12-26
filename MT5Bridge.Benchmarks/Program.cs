using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Text.Json;
using MT5Bridge.Manager.Models;

namespace MT5Bridge.Benchmarks;

/// <summary>
/// Benchmark program to compare different serialization approaches
/// </summary>
public static class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<SerializationBenchmarks>();
    }
}

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 5)]
public class SerializationBenchmarks
{
    private DealModel _testDeal = null!;

    [GlobalSetup]
    public void Setup()
    {
        _testDeal = new DealModel
        {
            Deal = 12345678,
            Login = 888888,
            Symbol = "EURUSD",
            Action = DealAction.Buy,
            Entry = EntryFlag.In,
            Reason = DealReason.Expert,
            Price = 1.08505,
            Volume = 10000,
            Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Commission = -2.5,
            Storage = 0,
            Profit = 125.50
        };
    }

    [Benchmark(Baseline = true)]
    public void ToString_Serialization()
    {
        var message = _testDeal.ToString();
    }

    [Benchmark]
    public void FastJson_Serialization()
    {
        var json = MT5JsonSerializer.ToFastJson(_testDeal);
    }

    [Benchmark]
    public void ReadableJson_Serialization()
    {
        var json = MT5JsonSerializer.ToReadableJson(_testDeal);
    }
}
