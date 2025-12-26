using Microsoft.Extensions.Configuration;
using Serilog;
using MT5Bridge.Manager;
using MT5Bridge.Manager.Managers;

namespace MT5Bridge.Manager.Demo.Commands;

/// <summary>
/// Base class for all commands with common functionality
/// </summary>
public abstract class BaseCommand
{
    protected ILogger Logger { get; }
    protected IConfiguration Configuration { get; }

    protected BaseCommand(ILogger logger, IConfiguration configuration)
    {
        Logger = logger;
        Configuration = configuration;
    }

    /// <summary>
    /// Create connection settings from configuration and optional overrides
    /// </summary>
    protected MT5ConnectionSettings CreateConnectionSettings(
        string? server = null,
        ulong? login = null,
        string? password = null)
    {
        var finalPassword = password ?? Configuration["MT5Connection:Password"] ?? throw new InvalidOperationException("Password not configured");

        return new MT5ConnectionSettings
        {
            Server = (server ?? Configuration["MT5Connection:Server"] ?? throw new InvalidOperationException("Server not configured")).Trim(),
            Login = login ?? ulong.Parse(Configuration["MT5Connection:Login"] ?? "0"),
            Password = finalPassword.Trim(),
            TimeoutMs = uint.Parse(Configuration["MT5Connection:TimeoutMs"] ?? "30000"),
            PumpMode = PumpMode.Full
        };
    }

    /// <summary>
    /// Connect to MT5 server with progress feedback
    /// </summary>
    protected async Task<IMT5Manager> ConnectAsync(MT5ConnectionSettings settings)
    {
        var manager = new MT5Manager(Logger);

        // Subscribe to connection state changes
        manager.ConnectionStateChanged += (sender, e) =>
        {
            Console.WriteLine($"Connection: {e.OldState} -> {e.NewState}");
            if (e.Message != null)
            {
                Console.WriteLine($"  Message: {e.Message}");
            }
        };

        Console.WriteLine($"Connecting to {settings.Server}...");
        var result = await manager.ConnectAsync(settings);

        if (!result.IsSuccess)
        {
            manager.Dispose();
            throw new Exception($"Connection failed: {result.Message}");
        }

        Console.WriteLine("Connected successfully\n");
        return manager;
    }
}
