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

    public double CompareExchange(double value, double comparand)
    {
        return Interlocked.CompareExchange(ref _value, value, comparand);
    }

    public void Store(double value)
    {
        Interlocked.Exchange(ref _value, value);
    }
}
