using MT5Bridge.Core.Atomic;

namespace MT5Bridge.Core.Tests.Atomic;

public class AtomicIntTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithValue()
    {
        var atomic = new AtomicInt(42);

        Assert.Equal(42, atomic.Load());
    }

    [Fact]
    public void Store_ShouldUpdateValue()
    {
        var atomic = new AtomicInt(0);

        atomic.Store(100);
        Assert.Equal(100, atomic.Load());

        atomic.Store(-50);
        Assert.Equal(-50, atomic.Load());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(42)]
    [InlineData(-100)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void StoreAndLoad_ShouldHandleVariousValues(int value)
    {
        var atomic = new AtomicInt(0);
        atomic.Store(value);

        Assert.Equal(value, atomic.Load());
    }

    [Fact]
    public void Increment_ShouldIncreaseValue()
    {
        var atomic = new AtomicInt(10);
        var result = atomic.Increment();

        Assert.Equal(11, result);
        Assert.Equal(11, atomic.Load());
    }

    [Fact]
    public void Decrement_ShouldDecreaseValue()
    {
        var atomic = new AtomicInt(10);
        var result = atomic.Decrement();

        Assert.Equal(9, result);
        Assert.Equal(9, atomic.Load());
    }

    [Fact]
    public void Add_ShouldAddValue()
    {
        var atomic = new AtomicInt(10);
        var result = atomic.Add(5);

        Assert.Equal(15, result);
        Assert.Equal(15, atomic.Load());
    }

    [Fact]
    public void CompareExchange_ShouldUpdateWhenMatch()
    {
        var atomic = new AtomicInt(10);
        var original = atomic.CompareExchange(20, 10);

        Assert.Equal(10, original);
        Assert.Equal(20, atomic.Load());
    }

    [Fact]
    public void CompareExchange_ShouldNotUpdateWhenNoMatch()
    {
        var atomic = new AtomicInt(10);
        var original = atomic.CompareExchange(20, 5);

        Assert.Equal(10, original);
        Assert.Equal(10, atomic.Load());
    }

    [Fact]
    public async Task ThreadSafety_ConcurrentStoresAndLoads()
    {
        var atomic = new AtomicInt(0);
        var iterations = 10000;

        var tasks = new List<Task>();
        for (int i = 0; i < 5; i++)
        {
            int threadValue = i;
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < iterations; j++)
                {
                    atomic.Store(threadValue);
                    int loaded = atomic.Load();
                    // Should be a valid value from one of the threads
                    Assert.InRange(loaded, 0, 4);
                }
            }));
        }

        await Task.WhenAll(tasks);
    }
}
