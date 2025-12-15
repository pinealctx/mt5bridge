namespace MT5Bridge.Core.Atomic;

public class AtomicBool
{
    private int _value;

    public AtomicBool(bool initialValue)
    {
        Store(initialValue);
    }

    public bool Load()
    {
        return Volatile.Read(ref _value) != 0;
    }

    public void Store(bool value)
    {
        Interlocked.Exchange(ref _value, value ? 1 : 0);
    }
}
