namespace MT5Bridge.Core.Atomic;

public class AtomicRef<T> where T : class
{
    private T? _value;

    public AtomicRef(T? initialValue)
    {
        Store(initialValue);
    }

    public T? Load()
    {
        return Volatile.Read(ref _value);
    }

    public T? CompareExchange(T? value, T? comparand)
    {
        return Interlocked.CompareExchange(ref _value, value, comparand);
    }

    public void Store(T? value)
    {
        Interlocked.Exchange(ref _value, value);
    }
}
