using MetaQuotes.MT5CommonAPI;
using MetaQuotes.MT5ManagerAPI;

namespace MT5Bridge.Manager.Sinks;

/// <summary>
/// Generic sink for position events that supports custom model transformations
/// </summary>
/// <typeparam name="T">Target model type (e.g., PositionModel or Proto.PositionModel)</typeparam>
internal class MT5GenericPositionSink<T> : CIMTPositionSink where T : class
{
    private readonly Func<CIMTPosition, T> _converter;
    private readonly Action<T>? _onAdd;
    private readonly Action<T>? _onUpdate;
    private readonly Action<T>? _onDelete;
    private readonly Action<ulong>? _onClean;
    private readonly Action? _onSync;

    public MT5GenericPositionSink(
        Func<CIMTPosition, T> converter,
        Action<T>? onAdd,
        Action<T>? onUpdate,
        Action<T>? onDelete,
        Action<ulong>? onClean = null,
        Action? onSync = null)
    {
        _converter = converter ?? throw new ArgumentNullException(nameof(converter));
        _onAdd = onAdd;
        _onUpdate = onUpdate;
        _onDelete = onDelete;
        _onClean = onClean;
        _onSync = onSync;
    }

    public override void OnPositionAdd(CIMTPosition position)
    {
        if (_onAdd != null)
        {
            var model = _converter(position);
            _onAdd(model);
        }
    }

    public override void OnPositionUpdate(CIMTPosition position)
    {
        if (_onUpdate != null)
        {
            var model = _converter(position);
            _onUpdate(model);
        }
    }

    public override void OnPositionDelete(CIMTPosition position)
    {
        if (_onDelete != null)
        {
            var model = _converter(position);
            _onDelete(model);
        }
    }

    public override void OnPositionSync()
    {
        _onSync?.Invoke();
    }

    public override void OnPositionClean(ulong login)
    {
        _onClean?.Invoke(login);
    }
}
