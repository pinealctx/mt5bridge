using MT5Bridge.Core.Atomic;

namespace MT5Bridge.Core.Tests.Atomic;

public class AtomicRefTests
{
    private class TestObject
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void Constructor_ShouldInitializeWithNull()
    {
        var atomic = new AtomicRef<TestObject>(null);

        Assert.Null(atomic.Load());
    }

    [Fact]
    public void Constructor_ShouldInitializeWithValue()
    {
        var obj = new TestObject { Value = 42, Name = "Test" };
        var atomic = new AtomicRef<TestObject>(obj);

        var loaded = atomic.Load();
        Assert.NotNull(loaded);
        Assert.Equal(42, loaded.Value);
        Assert.Equal("Test", loaded.Name);
    }

    [Fact]
    public void Store_ShouldUpdateReference()
    {
        var obj1 = new TestObject { Value = 1, Name = "First" };
        var obj2 = new TestObject { Value = 2, Name = "Second" };
        var atomic = new AtomicRef<TestObject>(obj1);

        atomic.Store(obj2);
        var loaded = atomic.Load();

        Assert.NotNull(loaded);
        Assert.Equal(2, loaded.Value);
        Assert.Equal("Second", loaded.Name);
    }

    [Fact]
    public void Store_CanStoreNull()
    {
        var obj = new TestObject { Value = 99 };
        var atomic = new AtomicRef<TestObject>(obj);

        atomic.Store(null);
        Assert.Null(atomic.Load());
    }

    [Fact]
    public void Load_ShouldReturnSameReference()
    {
        var obj = new TestObject { Value = 123 };
        var atomic = new AtomicRef<TestObject>(obj);

        var loaded1 = atomic.Load();
        var loaded2 = atomic.Load();

        Assert.Same(obj, loaded1);
        Assert.Same(obj, loaded2);
    }

    [Fact]
    public void CompareExchange_ShouldUpdateWhenMatch()
    {
        var obj1 = new TestObject { Value = 1 };
        var obj2 = new TestObject { Value = 2 };
        var atomic = new AtomicRef<TestObject>(obj1);

        var original = atomic.CompareExchange(obj2, obj1);

        Assert.Same(obj1, original);
        Assert.Same(obj2, atomic.Load());
    }

    [Fact]
    public void CompareExchange_ShouldNotUpdateWhenNoMatch()
    {
        var obj1 = new TestObject { Value = 1 };
        var obj2 = new TestObject { Value = 2 };
        var obj3 = new TestObject { Value = 3 };
        var atomic = new AtomicRef<TestObject>(obj1);

        var original = atomic.CompareExchange(obj2, obj3);

        Assert.Same(obj1, original);
        Assert.Same(obj1, atomic.Load());
    }

    [Fact]
    public async Task ThreadSafety_ConcurrentReferenceSwaps()
    {
        var objects = Enumerable.Range(0, 10)
            .Select(i => new TestObject { Value = i, Name = $"Object{i}" })
            .ToArray();

        var atomic = new AtomicRef<TestObject>(objects[0]);
        var iterations = 1000;

        var tasks = new List<Task>();
        for (int i = 0; i < 5; i++)
        {
            int index = i;
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < iterations; j++)
                {
                    atomic.Store(objects[index]);
                    var loaded = atomic.Load();

                    // Loaded object should be one of our objects
                    Assert.Contains(loaded, objects);
                }
            }));
        }

        await Task.WhenAll(tasks);
    }

    [Fact]
    public void WithStrings_ShouldWorkCorrectly()
    {
        var atomic = new AtomicRef<string>("initial");

        Assert.Equal("initial", atomic.Load());

        atomic.Store("updated");
        Assert.Equal("updated", atomic.Load());

        atomic.Store(null);
        Assert.Null(atomic.Load());
    }
}
