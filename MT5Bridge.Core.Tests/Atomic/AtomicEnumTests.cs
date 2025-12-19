using MT5Bridge.Core.Atomic;

namespace MT5Bridge.Core.Tests.Atomic;

public class AtomicEnumTests
{
    public enum TestStatus
    {
        Idle = 0,
        Running = 1,
        Paused = 2,
        Stopped = 3,
        Failed = 4
    }

    [Fact]
    public void Constructor_ShouldInitializeWithValue()
    {
        var atomic = new AtomicEnum<TestStatus>(TestStatus.Running);

        Assert.Equal(TestStatus.Running, atomic.Load());
    }

    [Fact]
    public void Store_ShouldUpdateValue()
    {
        var atomic = new AtomicEnum<TestStatus>(TestStatus.Idle);

        atomic.Store(TestStatus.Running);
        Assert.Equal(TestStatus.Running, atomic.Load());

        atomic.Store(TestStatus.Stopped);
        Assert.Equal(TestStatus.Stopped, atomic.Load());
    }

    [Theory]
    [InlineData(TestStatus.Idle)]
    [InlineData(TestStatus.Running)]
    [InlineData(TestStatus.Paused)]
    [InlineData(TestStatus.Stopped)]
    [InlineData(TestStatus.Failed)]
    public void StoreAndLoad_ShouldHandleAllEnumValues(TestStatus status)
    {
        var atomic = new AtomicEnum<TestStatus>(TestStatus.Idle);
        atomic.Store(status);

        Assert.Equal(status, atomic.Load());
    }

    [Fact]
    public async Task ThreadSafety_ConcurrentEnumChanges()
    {
        var atomic = new AtomicEnum<TestStatus>(TestStatus.Idle);
        var iterations = 1000;

        var tasks = new List<Task>
        {
            Task.Run(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    atomic.Store(TestStatus.Running);
                }
            }),
            Task.Run(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    atomic.Store(TestStatus.Paused);
                }
            }),
            Task.Run(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    TestStatus current = atomic.Load();
                    Assert.True(Enum.IsDefined(typeof(TestStatus), current));
                }
            })
        };

        await Task.WhenAll(tasks);
    }
}
