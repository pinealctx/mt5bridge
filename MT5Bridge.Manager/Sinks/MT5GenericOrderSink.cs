using MetaQuotes.MT5CommonAPI;
using Serilog;

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
    private readonly ILogger? _logger;

    public MT5GenericOrderSink(
        Func<CIMTOrder, T> converter,
        Action<T>? onAdd,
        Action<T>? onUpdate,
        Action<T>? onDelete,
        Action<ulong>? onClean = null,
        Action? onSync = null,
        ILogger? logger = null)
    {
        _converter = converter ?? throw new ArgumentNullException(nameof(converter));
        _onAdd = onAdd;
        _onUpdate = onUpdate;
        _onDelete = onDelete;
        _onClean = onClean;
        _onSync = onSync;
        _logger = logger;
    }

    public override void OnOrderAdd(CIMTOrder order)
    {
        if (_onAdd != null)
        {
            try
            {
                var model = _converter(order);
                _onAdd(model);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Error in OnOrderAdd handler");
            }
        }
    }

    public override void OnOrderUpdate(CIMTOrder order)
    {
        if (_onUpdate != null)
        {
            try
            {
                var model = _converter(order);
                _onUpdate(model);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Error in OnOrderUpdate handler");
            }
        }
    }

    public override void OnOrderDelete(CIMTOrder order)
    {
        if (_onDelete != null)
        {
            try
            {
                var model = _converter(order);
                _onDelete(model);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Error in OnOrderDelete handler");
            }
        }
    }

    public override void OnOrderSync()
    {
        try
        {
            _onSync?.Invoke();
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Error in OnOrderSync handler");
        }
    }

    public override void OnOrderClean(ulong login)
    {
        try
        {
            _onClean?.Invoke(login);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Error in OnOrderClean handler for login {Login}", login);
        }
    }
}
