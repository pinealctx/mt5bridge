using MetaQuotes.MT5CommonAPI;

namespace MT5Bridge.Manager.Sinks;

/// <summary>
/// Sink for deal events
/// </summary>
internal class MT5DealSink : CIMTDealSink
{
    private readonly Action<CIMTDeal> _onAdd;
    private readonly Action<CIMTDeal> _onUpdate;
    private readonly Action<CIMTDeal> _onDelete;
    private readonly Action<ulong>? _onClean;
    private readonly Action? _onSync;
    private readonly Action<CIMTDeal, CIMTAccount, CIMTPosition>? _onPerform;

    public MT5DealSink(
        Action<CIMTDeal> onAdd,
        Action<CIMTDeal> onUpdate,
        Action<CIMTDeal> onDelete,
        Action<ulong>? onClean = null,
        Action? onSync = null,
        Action<CIMTDeal, CIMTAccount, CIMTPosition>? onPerform = null)
    {
        _onAdd = onAdd;
        _onUpdate = onUpdate;
        _onDelete = onDelete;
        _onClean = onClean;
        _onSync = onSync;
        _onPerform = onPerform;
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

    public override void OnDealClean(ulong login)
    {
        _onClean?.Invoke(login);
    }

    public override void OnDealSync()
    {
        _onSync?.Invoke();
    }

    public override void OnDealPerform(
        CIMTDeal deal,
        CIMTAccount account,
        CIMTPosition position)
    {
        _onPerform?.Invoke(deal, account, position);
    }
}
