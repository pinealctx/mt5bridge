using MetaQuotes.MT5CommonAPI;
using MetaQuotes.MT5ManagerAPI;

namespace MT5Bridge.Manager.Sinks;

/// <summary>
/// Generic sink for order events that supports custom model transformations
/// </summary>
/// <typeparam name="T">Target model type (e.g., OrderModel or Proto.OrderModel)</typeparam>
internal class MT5GenericOrderSink<T> : CIMTOrderSink where T : class
{
    private readonly Func<CIMTOrder, T> _converter;
    private readonly Action<T>? _onAdd;
    private readonly Action<T>? _onUpdate;
    private readonly Action<T>? _onDelete;
    private readonly Action<ulong>? _onClean;
    private readonly Action? _onSync;

    public MT5GenericOrderSink(
        Func<CIMTOrder, T> converter,
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

    public override void OnOrderAdd(CIMTOrder order)
    {
        if (_onAdd != null)
        {
            var model = _converter(order);
            _onAdd(model);
        }
    }

    public override void OnOrderUpdate(CIMTOrder order)
    {
        if (_onUpdate != null)
        {
            var model = _converter(order);
            _onUpdate(model);
        }
    }

    public override void OnOrderDelete(CIMTOrder order)
    {
        if (_onDelete != null)
        {
            var model = _converter(order);
            _onDelete(model);
        }
    }

    public override void OnOrderSync()
    {
        _onSync?.Invoke();
    }

    public override void OnOrderClean(ulong login)
    {
        _onClean?.Invoke(login);
    }
}
