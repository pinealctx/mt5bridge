using MetaQuotes.MT5CommonAPI;
using MetaQuotes.MT5ManagerAPI;

namespace MT5Bridge.Manager.Sinks;

/// <summary>
/// Sink for manager system events (connection, etc.)
/// </summary>
internal class MT5ManagerSink : CIMTManagerSink
{
    private readonly Action? _onConnect;
    private readonly Action? _onDisconnect;
    private readonly Action<
        MTRetCode,
        long,
        CIMTUser,
        CIMTAccount,
        CIMTOrderArray,
        CIMTPositionArray>? _onTradeAccountSet;

    public MT5ManagerSink(
        Action? onConnect = null,
        Action? onDisconnect = null,
        Action<
            MTRetCode,
            long,
            CIMTUser,
            CIMTAccount,
            CIMTOrderArray,
            CIMTPositionArray>? onTradeAccountSet = null)
    {
        _onConnect = onConnect;
        _onDisconnect = onDisconnect;
        _onTradeAccountSet = onTradeAccountSet;
    }

    public override void OnConnect()
    {
        _onConnect?.Invoke();
    }

    public override void OnDisconnect()
    {
        _onDisconnect?.Invoke();
    }

    public override void OnTradeAccountSet(
        MTRetCode retcode,
        long request_id,
        CIMTUser user,
        CIMTAccount account,
        CIMTOrderArray orders,
        CIMTPositionArray positions)
    {
        _onTradeAccountSet?.Invoke(retcode, request_id, user, account, orders, positions);
    }
}
