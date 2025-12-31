using MetaQuotes.MT5CommonAPI;
using Serilog;

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
    private readonly ILogger? _logger;

    public MT5GenericPositionSink(
        Func<CIMTPosition, T> converter,
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

    public override void OnPositionAdd(CIMTPosition position)
    {
        if (_onAdd != null)
        {
            try
            {
                var model = _converter(position);
                _onAdd(model);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Error in OnPositionAdd handler");
            }
        }
    }

    public override void OnPositionUpdate(CIMTPosition position)
    {
        if (_onUpdate != null)
        {
            try
            {
                var model = _converter(position);
                _onUpdate(model);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Error in OnPositionUpdate handler");
            }
        }
    }

    public override void OnPositionDelete(CIMTPosition position)
    {
        if (_onDelete != null)
        {
            try
            {
                var model = _converter(position);
                _onDelete(model);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Error in OnPositionDelete handler");
            }
        }
    }

    public override void OnPositionSync()
    {
        try
        {
            _onSync?.Invoke();
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Error in OnPositionSync handler");
        }
    }

    public override void OnPositionClean(ulong login)
    {
        try
        {
            _onClean?.Invoke(login);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Error in OnPositionClean handler for login {Login}", login);
        }
    }
}
