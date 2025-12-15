using System.Runtime.CompilerServices;

namespace MT5Bridge.Core.Atomic;

public class AtomicEnum<T> where T : unmanaged, Enum
{
    private T _value;

    public AtomicEnum(T initialValue)
    {
        Store(initialValue);
    }

    public T Load()
    {
        ref var intRef = ref Unsafe.As<T, int>(ref _value);
        int value = Volatile.Read(ref intRef);
        return Unsafe.As<int, T>(ref value);
    }

    public void Store(T value)
    {
        ref var intRef = ref Unsafe.As<T, int>(ref _value);
        var intValue = Unsafe.As<T, int>(ref value);
        Interlocked.Exchange(ref intRef, intValue);
    }
}
