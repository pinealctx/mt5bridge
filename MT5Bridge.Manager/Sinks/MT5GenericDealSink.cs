using MetaQuotes.MT5CommonAPI;
using Serilog;

namespace MT5Bridge.Manager.Sinks;

/// <summary>
/// Generic sink for deal events that supports custom model transformations
/// </summary>
/// <typeparam name="TDeal">Deal model type (e.g., DealModel or Proto.DealModel)</typeparam>
/// <typeparam name="TAccount">Account model type (e.g., AccountModel or Proto.AccountModel)</typeparam>
/// <typeparam name="TPosition">Position model type (e.g., PositionModel or Proto.PositionModel)</typeparam>
internal class MT5GenericDealSink<TDeal, TAccount, TPosition> : CIMTDealSink
    where TDeal : class
    where TAccount : class
    where TPosition : class
{
    private readonly ILogger? _logger;
    private readonly Func<CIMTDeal, TDeal> _converter;
    private readonly Func<CIMTAccount, TAccount>? _accountConverter;
    private readonly Func<CIMTPosition, TPosition>? _positionConverter;
    private readonly Action<TDeal>? _onAdd;
    private readonly Action<TDeal>? _onUpdate;
    private readonly Action<TDeal>? _onDelete;
    private readonly Action<ulong>? _onClean;
    private readonly Action? _onSync;
    private readonly Action<TDeal, TAccount, TPosition>? _onPerform;

    public MT5GenericDealSink(
        Func<CIMTDeal, TDeal> converter,
        Action<TDeal>? onAdd,
        Action<TDeal>? onUpdate,
        Action<TDeal>? onDelete,
        Action<ulong>? onClean = null,
        Action? onSync = null,
        Func<CIMTAccount, TAccount>? accountConverter = null,
        Func<CIMTPosition, TPosition>? positionConverter = null,
        Action<TDeal, TAccount, TPosition>? onPerform = null,
        ILogger? logger = null)
    {
        _converter = converter ?? throw new ArgumentNullException(nameof(converter));
        _accountConverter = accountConverter;
        _positionConverter = positionConverter;
        _onAdd = onAdd;
        _onUpdate = onUpdate;
        _onDelete = onDelete;
        _onClean = onClean;
        _onSync = onSync;
        _onPerform = onPerform;
        _logger = logger;
    }

    public override void OnDealAdd(CIMTDeal deal)
    {
        if (_onAdd != null)
        {
            // FIXED M2: Protect event handlers from exceptions
            try
            {
                var model = _converter(deal);
                _onAdd(model);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Error in OnDealAdd handler for deal {Deal}", deal.Deal());
            }
        }
    }

    public override void OnDealUpdate(CIMTDeal deal)
    {
        if (_onUpdate != null)
        {
            try
            {
                var model = _converter(deal);
                _onUpdate(model);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Error in OnDealUpdate handler for deal {Deal}", deal.Deal());
            }
        }
    }

    public override void OnDealDelete(CIMTDeal deal)
    {
        if (_onDelete != null)
        {
            try
            {
                var model = _converter(deal);
                _onDelete(model);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Error in OnDealDelete handler for deal {Deal}", deal.Deal());
            }
        }
    }

    public override void OnDealSync()
    {
        if (_onSync != null)
        {
            try
            {
                _onSync();
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Error in OnDealSync handler");
            }
        }
    }

    public override void OnDealClean(ulong login)
    {
        if (_onClean != null)
        {
            try
            {
                _onClean(login);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Error in OnDealClean handler for login {Login}", login);
            }
        }
    }

    public override void OnDealPerform(CIMTDeal deal, CIMTAccount account, CIMTPosition position)
    {
        if (_onPerform != null && _accountConverter != null && _positionConverter != null)
        {
            try
            {
                var dealModel = _converter(deal);
                var accountModel = _accountConverter(account);
                var positionModel = _positionConverter(position);
                _onPerform(dealModel, accountModel, positionModel);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Error in OnDealPerform handler for deal {Deal}", deal.Deal());
            }
        }
    }
}
