using MT5Bridge.Core.Atomic;

namespace MT5Bridge.Core.Tests.Atomic;

public class AtomicDoubleTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithValue()
    {
        var atomic = new AtomicDouble(3.14159);

        Assert.Equal(3.14159, atomic.Load(), precision: 5);
    }

    [Fact]
    public void Store_ShouldUpdateValue()
    {
        var atomic = new AtomicDouble(0.0);

        atomic.Store(2.71828);
        Assert.Equal(2.71828, atomic.Load(), precision: 5);

        atomic.Store(-999.999);
        Assert.Equal(-999.999, atomic.Load(), precision: 3);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(1.5)]
    [InlineData(-100.25)]
    [InlineData(double.MaxValue)]
    [InlineData(double.MinValue)]
    [InlineData(double.Epsilon)]
    public void StoreAndLoad_ShouldHandleVariousValues(double value)
    {
        var atomic = new AtomicDouble(0.0);
        atomic.Store(value);

        Assert.Equal(value, atomic.Load());
    }

    [Fact]
    public void StoreAndLoad_ShouldHandleSpecialValues()
    {
        var atomic = new AtomicDouble(0.0);

        atomic.Store(double.NaN);
        Assert.True(double.IsNaN(atomic.Load()));

        atomic.Store(double.PositiveInfinity);
        Assert.True(double.IsPositiveInfinity(atomic.Load()));

        atomic.Store(double.NegativeInfinity);
        Assert.True(double.IsNegativeInfinity(atomic.Load()));
    }

    [Fact]
    public void CompareExchange_ShouldUpdateWhenMatch()
    {
        var atomic = new AtomicDouble(10.5);
        var original = atomic.CompareExchange(20.5, 10.5);

        Assert.Equal(10.5, original);
        Assert.Equal(20.5, atomic.Load());
    }

    [Fact]
    public void CompareExchange_ShouldNotUpdateWhenNoMatch()
    {
        var atomic = new AtomicDouble(10.5);
        var original = atomic.CompareExchange(20.5, 5.5);

        Assert.Equal(10.5, original);
        Assert.Equal(10.5, atomic.Load());
    }

    [Fact]
    public async Task ThreadSafety_ConcurrentAccess()
    {
        var atomic = new AtomicDouble(0.0);
        var iterations = 1000;

        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            double threadValue = i * 1.5;
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < iterations; j++)
                {
                    atomic.Store(threadValue);
                    double loaded = atomic.Load();
                    // Should be one of the valid thread values
                    Assert.True(loaded >= 0.0 && loaded <= 13.5);
                }
            }));
        }

        await Task.WhenAll(tasks);
    }
}
