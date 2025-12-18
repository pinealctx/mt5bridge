using MetaQuotes.MT5CommonAPI;
using MetaQuotes.MT5ManagerAPI;

namespace MT5Bridge.MT5.Core.Sinks;

/// <summary>
/// Sink for position events
/// </summary>
internal class MT5PositionSink : CIMTPositionSink
{
    private readonly Action<CIMTPosition> _onAdd;
    private readonly Action<CIMTPosition> _onUpdate;
    private readonly Action<CIMTPosition> _onDelete;

    public MT5PositionSink(Action<CIMTPosition> onAdd, Action<CIMTPosition> onUpdate, Action<CIMTPosition> onDelete)
    {
        _onAdd = onAdd;
        _onUpdate = onUpdate;
        _onDelete = onDelete;
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
}
