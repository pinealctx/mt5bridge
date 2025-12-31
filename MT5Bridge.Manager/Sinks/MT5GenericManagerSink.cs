using MetaQuotes.MT5CommonAPI;
using MetaQuotes.MT5ManagerAPI;
using Serilog;

namespace MT5Bridge.Manager.Sinks;

/// <summary>
/// Generic sink for manager system events that supports custom model transformations
/// </summary>
/// <typeparam name="TUser">User model type (e.g., UserModel or Proto.UserModel)</typeparam>
/// <typeparam name="TAccount">Account model type (e.g., AccountModel or Proto.AccountModel)</typeparam>
/// <typeparam name="TOrder">Order model type (e.g., OrderModel or Proto.OrderModel)</typeparam>
/// <typeparam name="TPosition">Position model type (e.g., PositionModel or Proto.PositionModel)</typeparam>
internal class MT5GenericManagerSink<TUser, TAccount, TOrder, TPosition> : CIMTManagerSink
    where TUser : class
    where TAccount : class
    where TOrder : class
    where TPosition : class
{
    private readonly Func<CIMTUser, TUser>? _userConverter;
    private readonly Func<CIMTAccount, TAccount>? _accountConverter;
    private readonly Func<CIMTOrder, TOrder>? _orderConverter;
    private readonly Func<CIMTPosition, TPosition>? _positionConverter;
    private readonly Action? _onConnect;
    private readonly Action? _onDisconnect;
    private readonly Action<MTRetCode, long, TUser, TAccount, List<TOrder>, List<TPosition>>? _onTradeAccountSet;
    private readonly ILogger? _logger;

    public MT5GenericManagerSink(
        Action? onConnect = null,
        Action? onDisconnect = null,
        Func<CIMTUser, TUser>? userConverter = null,
        Func<CIMTAccount, TAccount>? accountConverter = null,
        Func<CIMTOrder, TOrder>? orderConverter = null,
        Func<CIMTPosition, TPosition>? positionConverter = null,
        Action<MTRetCode, long, TUser, TAccount, List<TOrder>, List<TPosition>>? onTradeAccountSet = null,
        ILogger? logger = null)
    {
        _onConnect = onConnect;
        _onDisconnect = onDisconnect;
        _userConverter = userConverter;
        _accountConverter = accountConverter;
        _orderConverter = orderConverter;
        _positionConverter = positionConverter;
        _onTradeAccountSet = onTradeAccountSet;
        _logger = logger;
    }

    public override void OnConnect()
    {
        try
        {
            _onConnect?.Invoke();
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Error in OnConnect handler");
        }
    }

    public override void OnDisconnect()
    {
        try
        {
            _onDisconnect?.Invoke();
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Error in OnDisconnect handler");
        }
    }

    public override void OnTradeAccountSet(
        MTRetCode retcode,
        long request_id,
        CIMTUser user,
        CIMTAccount account,
        CIMTOrderArray orders,
        CIMTPositionArray positions)
    {
        if (_onTradeAccountSet != null && _userConverter != null && _accountConverter != null &&
            _orderConverter != null && _positionConverter != null)
        {
            try
            {
                var userModel = _userConverter(user);
                var accountModel = _accountConverter(account);

                var orderList = new List<TOrder>();
                if (orders != null)
                {
                    var totalOrders = orders.Total();
                    for (uint i = 0; i < totalOrders; i++)
                    {
                        var order = orders.Next(i);
                        if (order != null)
                        {
                            orderList.Add(_orderConverter(order));
                        }
                    }
                }

                var positionList = new List<TPosition>();
                if (positions != null)
                {
                    var totalPos = positions.Total();
                    for (uint i = 0; i < totalPos; i++)
                    {
                        var position = positions.Next(i);
                        if (position != null)
                        {
                            positionList.Add(_positionConverter(position));
                        }
                    }
                }

                _onTradeAccountSet(retcode, request_id, userModel, accountModel, orderList, positionList);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Error in OnTradeAccountSet handler for request {RequestId}", request_id);
            }
        }
    }
}
