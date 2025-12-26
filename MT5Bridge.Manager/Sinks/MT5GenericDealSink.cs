using MetaQuotes.MT5CommonAPI;
using MetaQuotes.MT5ManagerAPI;

namespace MT5Bridge.Manager.Sinks;

/// <summary>
/// Generic sink for deal events that supports custom model transformations
/// </summary>
/// <typeparam name="T">Target model type (e.g., DealModel or Proto.DealModel)</typeparam>
internal class MT5GenericDealSink<T> : CIMTDealSink where T : class
{
    private readonly Func<CIMTDeal, T> _converter;
    private readonly Action<T>? _onAdd;
    private readonly Action<T>? _onUpdate;
    private readonly Action<T>? _onDelete;

    public MT5GenericDealSink(
        Func<CIMTDeal, T> converter,
        Action<T>? onAdd,
        Action<T>? onUpdate,
        Action<T>? onDelete)
    {
        _converter = converter ?? throw new ArgumentNullException(nameof(converter));
        _onAdd = onAdd;
        _onUpdate = onUpdate;
        _onDelete = onDelete;
    }

    public override void OnDealAdd(CIMTDeal deal)
    {
        if (_onAdd != null)
        {
            var model = _converter(deal);
            _onAdd(model);
        }
    }

    public override void OnDealUpdate(CIMTDeal deal)
    {
        if (_onUpdate != null)
        {
            var model = _converter(deal);
            _onUpdate(model);
        }
    }

    public override void OnDealDelete(CIMTDeal deal)
    {
        if (_onDelete != null)
        {
            var model = _converter(deal);
            _onDelete(model);
        }
    }
}
