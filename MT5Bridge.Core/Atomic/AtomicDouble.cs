namespace MT5Bridge.Core.Atomic;

public class AtomicDouble
{
    private double _value;

    public AtomicDouble(double initialValue)
    {
        Store(initialValue);
    }

    public double Load()
    {
        return Volatile.Read(ref _value);
    }

    public void Store(double value)
    {
        Interlocked.Exchange(ref _value, value);
    }
}
