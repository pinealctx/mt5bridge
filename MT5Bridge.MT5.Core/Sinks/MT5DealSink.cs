using MetaQuotes.MT5CommonAPI;
using MetaQuotes.MT5ManagerAPI;

namespace MT5Bridge.MT5.Core.Sinks;

/// <summary>
/// Sink for deal events
/// </summary>
internal class MT5DealSink : CIMTDealSink
{
    private readonly Action<CIMTDeal> _onAdd;
    private readonly Action<CIMTDeal> _onUpdate;
    private readonly Action<CIMTDeal> _onDelete;

    public MT5DealSink(Action<CIMTDeal> onAdd, Action<CIMTDeal> onUpdate, Action<CIMTDeal> onDelete)
    {
        _onAdd = onAdd;
        _onUpdate = onUpdate;
        _onDelete = onDelete;
    }

    public override void OnDealAdd(CIMTDeal deal)
    {
        _onAdd?.Invoke(deal);
    }

    public override void OnDealUpdate(CIMTDeal deal)
    {
        _onUpdate?.Invoke(deal);
    }

    public override void OnDealDelete(CIMTDeal deal)
    {
        _onDelete?.Invoke(deal);
    }
}
