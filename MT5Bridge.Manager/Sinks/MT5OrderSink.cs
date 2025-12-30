using MetaQuotes.MT5CommonAPI;
using MetaQuotes.MT5ManagerAPI;

namespace MT5Bridge.Manager.Sinks;

/// <summary>
/// Sink for order events
/// </summary>
internal class MT5OrderSink : CIMTOrderSink
{
    private readonly Action<CIMTOrder> _onAdd;
    private readonly Action<CIMTOrder> _onUpdate;
    private readonly Action<CIMTOrder> _onDelete;
    private readonly Action<ulong>? _onClean;
    private readonly Action? _onSync;

    public MT5OrderSink(
        Action<CIMTOrder> onAdd,
        Action<CIMTOrder> onUpdate,
        Action<CIMTOrder> onDelete,
        Action<ulong>? onClean = null,
        Action? onSync = null)
    {
        _onAdd = onAdd;
        _onUpdate = onUpdate;
        _onDelete = onDelete;
        _onClean = onClean;
        _onSync = onSync;
    }

    public override void OnOrderAdd(CIMTOrder order)
    {
        _onAdd?.Invoke(order);
    }

    public override void OnOrderUpdate(CIMTOrder order)
    {
        _onUpdate?.Invoke(order);
    }

    public override void OnOrderDelete(CIMTOrder order)
    {
        _onDelete?.Invoke(order);
    }

    public override void OnOrderClean(ulong login)
    {
        _onClean?.Invoke(login);
    }

    public override void OnOrderSync()
    {
        _onSync?.Invoke();
    }
}
