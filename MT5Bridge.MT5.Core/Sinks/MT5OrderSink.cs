using MetaQuotes.MT5CommonAPI;
using MetaQuotes.MT5ManagerAPI;

namespace MT5Bridge.MT5.Core.Sinks;

/// <summary>
/// Sink for order events
/// </summary>
internal class MT5OrderSink : CIMTOrderSink
{
    private readonly Action<CIMTOrder> _onAdd;
    private readonly Action<CIMTOrder> _onUpdate;
    private readonly Action<CIMTOrder> _onDelete;

    public MT5OrderSink(Action<CIMTOrder> onAdd, Action<CIMTOrder> onUpdate, Action<CIMTOrder> onDelete)
    {
        _onAdd = onAdd;
        _onUpdate = onUpdate;
        _onDelete = onDelete;
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
}
