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

    public bool CompareExchange(bool value, bool comparand)
    {
        int val = value ? 1 : 0;
        int comp = comparand ? 1 : 0;
        return Interlocked.CompareExchange(ref _value, val, comp) != 0;
    }

    public void Store(bool value)
    {
        Interlocked.Exchange(ref _value, value ? 1 : 0);
    }
}
