using MT5Bridge.Core.Atomic;

namespace MT5Bridge.Core.Tests.Atomic;

public class AtomicBoolTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithValue()
    {
        var atomicTrue = new AtomicBool(true);
        var atomicFalse = new AtomicBool(false);

        Assert.True(atomicTrue.Load());
        Assert.False(atomicFalse.Load());
    }

    [Fact]
    public void Store_ShouldUpdateValue()
    {
        var atomic = new AtomicBool(false);

        atomic.Store(true);
        Assert.True(atomic.Load());

        atomic.Store(false);
        Assert.False(atomic.Load());
    }

    [Fact]
    public void Load_ShouldReturnCurrentValue()
    {
        var atomic = new AtomicBool(true);

        Assert.True(atomic.Load());
        Assert.True(atomic.Load()); // Multiple reads should work
    }

    [Fact]
    public void ThreadSafety_MultipleThreadsCanAccessSafely()
    {
        var atomic = new AtomicBool(false);
        var iterations = 1000;
        var toggleCount = 0;

        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < iterations; j++)
                {
                    bool current = atomic.Load();
                    atomic.Store(!current);
                    Interlocked.Increment(ref toggleCount);
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());

        // Should have toggled 10 * 1000 times
        Assert.Equal(10 * iterations, toggleCount);
    }
}
