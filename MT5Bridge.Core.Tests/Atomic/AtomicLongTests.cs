using MT5Bridge.Core.Atomic;

namespace MT5Bridge.Core.Tests.Atomic;

public class AtomicLongTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithValue()
    {
        var atomic = new AtomicLong(12345678901234L);

        Assert.Equal(12345678901234L, atomic.Load());
    }

    [Fact]
    public void Store_ShouldUpdateValue()
    {
        var atomic = new AtomicLong(0);

        atomic.Store(999999999999L);
        Assert.Equal(999999999999L, atomic.Load());
    }

    [Fact]
    public void Increment_ShouldIncreaseValueByOne()
    {
        var atomic = new AtomicLong(0);

        long result1 = atomic.Increment();
        Assert.Equal(1L, result1);
        Assert.Equal(1L, atomic.Load());

        long result2 = atomic.Increment();
        Assert.Equal(2L, result2);
        Assert.Equal(2L, atomic.Load());
    }

    [Fact]
    public void Increment_FromNegativeValue()
    {
        var atomic = new AtomicLong(-5);

        atomic.Increment();
        Assert.Equal(-4L, atomic.Load());

        atomic.Increment();
        Assert.Equal(-3L, atomic.Load());
    }

    [Fact]
    public void ThreadSafety_ConcurrentIncrements()
    {
        var atomic = new AtomicLong(0);
        var threadsCount = 10;
        var incrementsPerThread = 10000;

        var tasks = new List<Task>();
        for (int i = 0; i < threadsCount; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < incrementsPerThread; j++)
                {
                    atomic.Increment();
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());

        // Should have incremented exactly threadsCount * incrementsPerThread times
        Assert.Equal(threadsCount * incrementsPerThread, atomic.Load());
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(long.MaxValue)]
    [InlineData(long.MinValue)]
    [InlineData(-1L)]
    public void StoreAndLoad_ShouldHandleEdgeCases(long value)
    {
        var atomic = new AtomicLong(0);
        atomic.Store(value);

        Assert.Equal(value, atomic.Load());
    }
}
