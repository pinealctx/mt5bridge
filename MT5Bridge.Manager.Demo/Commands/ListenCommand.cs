using Microsoft.Extensions.Configuration;
using Serilog;
using MT5Bridge.Manager.Managers;
using MT5Bridge.Manager.Reconnection;
using MT5Bridge.Manager.Demo.Commands.Handlers;

namespace MT5Bridge.Manager.Demo.Commands;

/// <summary>
/// Listen command: subscribe to real-time events using traditional or generic handlers
/// 
/// Refactoring notes (2025-12-30):
/// - Optimized from 715 lines to ~120 lines (83% reduction)
/// - Uses strategy pattern to eliminate ~400 lines of duplicate code in PocoAsync/ProtobufAsync
/// - ListenAsync handles connection and reconnection logic uniformly
/// - Handler registration strategies are responsible for creating appropriate handlers
/// </summary>
public class ListenCommand : BaseCommand
{
    public ListenCommand(ILogger logger, IConfiguration configuration)
        : base(logger, configuration)
    {
    }

    public async Task<int> RunAsync(string? server, ulong? login, string? password, string? types, string mode)
    {
        // Select appropriate strategy based on mode
        var strategy = mode.ToLowerInvariant() switch
        {
            "protobuf" => (IHandlerRegistrationStrategy)
                new ProtobufHandlerRegistrationStrategy(Logger),
            _ => new PocoHandlerRegistrationStrategy(Logger)
        };

        return await ListenAsync(server, login, password, types, strategy);
    }

    /// <summary>
    /// Unified listening flow - handles both POCO and Protobuf modes
    /// 
    /// This method eliminates the code duplication from previous PocoAsync/ProtobufAsync
    /// implementations. Only one implementation is needed and supports both model types via strategy pattern
    /// </summary>
    private async Task<int> ListenAsync(
        string? server, ulong? login, string? password, string? types,
        IHandlerRegistrationStrategy strategy)
    {
        var settings = CreateConnectionSettings(server, login, password);

        Logger.Information("=== Listening to Trade Events ===");
        Logger.Information($"Subscribed to: {types ?? EventTypes.Deal}");
        Logger.Information("Press any key to stop...\n");

        // Create manager
        using var manager = new MT5Manager(Logger);

        // Register handlers using strategy
        var registrationResult = strategy.RegisterHandlers(manager, types);
        if (!registrationResult.IsSuccess)
        {
            Logger.Error("Failed to register handlers: {Message}", registrationResult.Message);
            return 1;
        }

        // Subscribe to connection state changes
        manager.ConnectionStateChanged += (sender, e) =>
        {
            Logger.Information("Connection state changed: {OldState} -> {NewState} ({Message})",
                e.OldState, e.NewState, e.Message);

            if (e.NewState == ConnectionState.Disconnected &&
                e.OldState == ConnectionState.Connected)
            {
                Logger.Warning("Server disconnected, attempting auto-reconnect...");
                // Fire and forget async operation - intentionally not awaited to avoid blocking event handler
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await manager.StartAutoReconnectAsync().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Auto-reconnect failed");
                    }
                });
            }
        };

        // Connect
        var connectResult = await manager.ConnectAsync(settings);
        if (!connectResult.IsSuccess)
        {
            Logger.Error("Initial connection failed: {Message}", connectResult.Message);

            var retCode = connectResult.RetCode;

            if (ErrorAnalyzer.IsTransientError(retCode))
            {
                Logger.Information("Error is transient, starting auto-reconnect...");
                await manager.StartAutoReconnectAsync();
            }
            else
            {
                Logger.Fatal("Error : {Error} is permanent (or unknown), exiting...", retCode);
                return 1;
            }
        }

        Logger.Information("Connected successfully!");

        // Wait for user to stop
        Console.ReadKey(true);
        Logger.Information("Stopped listening.");

        return 0;
    }
}
