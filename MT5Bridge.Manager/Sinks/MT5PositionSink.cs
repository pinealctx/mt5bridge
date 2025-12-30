using MetaQuotes.MT5CommonAPI;
using MetaQuotes.MT5ManagerAPI;

namespace MT5Bridge.Manager.Sinks;

/// <summary>
/// Sink for position events
/// </summary>
internal class MT5PositionSink : CIMTPositionSink
{
    private readonly Action<CIMTPosition> _onAdd;
    private readonly Action<CIMTPosition> _onUpdate;
    private readonly Action<CIMTPosition> _onDelete;
    private readonly Action<ulong>? _onClean;
    private readonly Action? _onSync;

    public MT5PositionSink(
        Action<CIMTPosition> onAdd,
        Action<CIMTPosition> onUpdate,
        Action<CIMTPosition> onDelete,
        Action<ulong>? onClean = null,
        Action? onSync = null)
    {
        _onAdd = onAdd;
        _onUpdate = onUpdate;
        _onDelete = onDelete;
        _onClean = onClean;
        _onSync = onSync;
    }

    public override void OnPositionAdd(CIMTPosition position)
    {
        _onAdd?.Invoke(position);
    }

    public override void OnPositionUpdate(CIMTPosition position)
    {
        _onUpdate?.Invoke(position);
    }

    public override void OnPositionDelete(CIMTPosition position)
    {
        _onDelete?.Invoke(position);
    }

    public override void OnPositionClean(ulong login)
    {
        _onClean?.Invoke(login);
    }

    public override void OnPositionSync()
    {
        _onSync?.Invoke();
    }
}
