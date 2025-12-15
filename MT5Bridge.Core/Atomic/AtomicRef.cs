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

    public void Store(T? value)
    {
        Interlocked.Exchange(ref _value, value);
    }
}
