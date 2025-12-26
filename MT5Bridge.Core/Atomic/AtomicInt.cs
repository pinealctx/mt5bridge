namespace MT5Bridge.Core.Atomic;

public class AtomicInt
{
    private int _value;

    public AtomicInt(int initialValue)
    {
        Store(initialValue);
    }

    public int Load()
    {
        return Volatile.Read(ref _value);
    }

    public int Increment()
    {
        return Interlocked.Increment(ref _value);
    }

    public int Decrement()
    {
        return Interlocked.Decrement(ref _value);
    }

    public int Add(int value)
    {
        return Interlocked.Add(ref _value, value);
    }

    public int CompareExchange(int value, int comparand)
    {
        return Interlocked.CompareExchange(ref _value, value, comparand);
    }

    public void Store(int value)
    {
        Interlocked.Exchange(ref _value, value);
    }
}
