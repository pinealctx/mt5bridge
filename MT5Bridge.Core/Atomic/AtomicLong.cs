namespace MT5Bridge.Core.Atomic;

public class AtomicLong
{
    private long _value;

    public AtomicLong(long initialValue)
    {
        Store(initialValue);
    }

    public long Load()
    {
        return Volatile.Read(ref _value);
    }

    public long Increment()
    {
        return Interlocked.Increment(ref _value);
    }

    public long Decrement()
    {
        return Interlocked.Decrement(ref _value);
    }

    public long Add(long value)
    {
        return Interlocked.Add(ref _value, value);
    }

    public long CompareExchange(long value, long comparand)
    {
        return Interlocked.CompareExchange(ref _value, value, comparand);
    }

    public void Store(long value)
    {
        Interlocked.Exchange(ref _value, value);
    }
}
