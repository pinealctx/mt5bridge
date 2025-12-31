using MT5Bridge.Manager.Managers;

namespace MT5Bridge.Manager.Demo.Commands.Handlers;

/// <summary>
/// Handler registration strategy - supports different model types (POCO/Protobuf)
/// 
/// Design rationale:
/// Previously PocoAsync() and ProtobufAsync() had a lot of duplicate code.
/// Using the strategy pattern extracts the handler registration logic into specialized strategy classes.
/// This allows ListenCommand to have a single unified ListenAsync() method.
/// </summary>
public interface IHandlerRegistrationStrategy
{
    /// <summary>
    /// Register all subscribed handlers
    /// </summary>
    /// <param name="manager">MT5 Manager instance</param>
    /// <param name="types">Event types to subscribe to (comma-separated), null means default deal events</param>
    /// <returns>Registration result</returns>
    MT5Result RegisterHandlers(MT5Manager manager, string? types);
}
