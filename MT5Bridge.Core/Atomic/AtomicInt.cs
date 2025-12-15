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

    public void Store(int value)
    {
        Interlocked.Exchange(ref _value, value);
    }
}
