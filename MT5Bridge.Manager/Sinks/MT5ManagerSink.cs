using MetaQuotes.MT5CommonAPI;
using MetaQuotes.MT5ManagerAPI;

namespace MT5Bridge.Manager.Sinks;

/// <summary>
/// Sink for manager system events (connection, etc.)
/// </summary>
internal class MT5ManagerSink : CIMTManagerSink
{
    private readonly Action? _onDisconnect;

    public MT5ManagerSink(Action? onDisconnect = null)
    {
        _onDisconnect = onDisconnect;
    }

    public override void OnDisconnect()
    {
        _onDisconnect?.Invoke();
    }
}
