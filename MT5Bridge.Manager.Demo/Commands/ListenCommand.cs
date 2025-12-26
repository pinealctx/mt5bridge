using MetaQuotes.MT5CommonAPI;
using Microsoft.Extensions.Configuration;
using Serilog;
using MT5Bridge.Manager.Models;
using MT5Bridge.Manager.Models.Proto;
using Google.Protobuf;
using ProtoDeal = MT5Bridge.Manager.Models.Proto.DealModel;

namespace MT5Bridge.Manager.Demo.Commands;

/// <summary>
/// Listen command: subscribe to real-time events using traditional or generic handlers
/// </summary>
public class ListenCommand : BaseCommand
{
    public static class EventTypes
    {
        public const string Deal = "deal";
        public const string Order = "order";
        public const string Position = "position";
        public const string All = "all";
    }

    public ListenCommand(ILogger logger, IConfiguration configuration)
        : base(logger, configuration)
    {
    }

    public async Task<int> RunAsync(string? server, ulong? login, string? password, string? types, string mode)
    {
        return mode.ToLowerInvariant() switch
        {
            "poco" => await PocoAsync(server, login, password, types),
            "protobuf" => await ProtobufAsync(server, login, password, types),
            "mixed" => await MixedAsync(server, login, password, types),
            _ => await PocoAsync(server, login, password, types)
        };
    }

    public async Task<int> PocoAsync(string? server, ulong? login, string? password, string? types)
    {
        var settings = CreateConnectionSettings(server, login, password);
        using var manager = await ConnectAsync(settings);

        Logger.Information("=== Listening to Trade Events (POCO Models) ===");
        Logger.Information($"Subscribed to: {types ?? EventTypes.Deal}");
        Logger.Information("Press any key to stop...\n");

        var startTime = DateTime.Now;
        var typeList = (types ?? EventTypes.Deal).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        bool all = typeList.Contains(EventTypes.All, StringComparer.OrdinalIgnoreCase);

        // Register POCO model handlers
        if (all || typeList.Contains(EventTypes.Deal, StringComparer.OrdinalIgnoreCase))
        {
            manager.RegisterDealHandler(
                onAdd: dealModel =>
                {
                    var elapsed = (DateTime.Now - startTime).TotalMilliseconds;

                    Logger.Information($"[DEAL ADD] #{dealModel.Deal}: {dealModel.Symbol}, " +
                               $"Action: {dealModel.Action}, Price: {dealModel.Price:F5}, " +
                               $"Volume: {dealModel.VolumeExt / 10000.0:F2}, Profit: {dealModel.Profit:F2}, " +
                               $"Login: {dealModel.Login}, Time: {DateTimeOffset.FromUnixTimeSeconds(dealModel.Time):yyyy-MM-dd HH:mm:ss} " +
                               $"(Elapsed: {elapsed:F0}ms)");

                    // Show readable JSON for first 3 deals
                    if (dealModel.Deal % 100 == 0)
                    {
                        Logger.Debug($"Deal JSON:\n{dealModel.ToString()}");
                    }
                },
                onUpdate: dealModel =>
                {
                    Logger.Information($"[DEAL UPDATE] #{dealModel.Deal}: {dealModel.Symbol}, State changed");
                },
                onDelete: dealModel =>
                {
                    Logger.Information($"[DEAL DELETE] #{dealModel.Deal}: {dealModel.Symbol}");
                }
            );
        }

        if (all || typeList.Contains(EventTypes.Order, StringComparer.OrdinalIgnoreCase))
        {
            manager.RegisterOrderHandler(
                onAdd: orderModel =>
                {
                    var elapsed = (DateTime.Now - startTime).TotalMilliseconds;

                    var volumeInitial = orderModel.VolumeInitialExt > 0
                        ? orderModel.VolumeInitialExt / 10000.0
                        : orderModel.VolumeInitial / 100.0;
                    var volumeCurrent = orderModel.VolumeCurrentExt > 0
                        ? orderModel.VolumeCurrentExt / 10000.0
                        : orderModel.VolumeCurrent / 100.0;

                    Logger.Information($"[ORDER ADD] #{orderModel.Order}: {orderModel.Symbol}, " +
                               $"Type: {orderModel.Type}, State: {orderModel.State}, " +
                               $"Price: {orderModel.PriceOrder:F5}, SL: {orderModel.PriceSL:F5}, TP: {orderModel.PriceTP:F5}, " +
                               $"Volume: {volumeInitial:F2}/{volumeCurrent:F2}, " +
                               $"Login: {orderModel.Login}, Reason: {orderModel.Reason}, " +
                               $"Time: {DateTimeOffset.FromUnixTimeSeconds(orderModel.TimeSetup):yyyy-MM-dd HH:mm:ss} " +
                               $"(Elapsed: {elapsed:F0}ms)");
                },
                onUpdate: orderModel =>
                {
                    var volumeCurrent = orderModel.VolumeCurrentExt > 0
                        ? orderModel.VolumeCurrentExt / 10000.0
                        : orderModel.VolumeCurrent / 100.0;

                    Logger.Information($"[ORDER UPDATE] #{orderModel.Order}: {orderModel.Symbol}, " +
                               $"State: {orderModel.State}, Volume: {volumeCurrent:F2}, Price: {orderModel.PriceCurrent:F5}");
                },
                onDelete: orderModel =>
                {
                    Logger.Information($"[ORDER DELETE] #{orderModel.Order}: {orderModel.Symbol}, " +
                               $"Final State: {orderModel.State}");
                }
            );
        }

        if (all || typeList.Contains(EventTypes.Position, StringComparer.OrdinalIgnoreCase))
        {
            manager.RegisterPositionHandler(
                onAdd: positionModel =>
                {
                    var elapsed = (DateTime.Now - startTime).TotalMilliseconds;

                    var volume = positionModel.VolumeExt > 0
                        ? positionModel.VolumeExt / 10000.0
                        : positionModel.Volume / 100.0;

                    Logger.Information($"[POSITION ADD] #{positionModel.Position}: {positionModel.Symbol}, " +
                               $"Action: {positionModel.Action}, Price: {positionModel.PriceOpen:F5}, " +
                               $"Volume: {volume:F2}, Profit: {positionModel.Profit:F2}, " +
                               $"SL: {positionModel.PriceSL:F5}, TP: {positionModel.PriceTP:F5}, " +
                               $"Login: {positionModel.Login}, Time: {DateTimeOffset.FromUnixTimeSeconds(positionModel.TimeCreate):yyyy-MM-dd HH:mm:ss} " +
                               $"(Elapsed: {elapsed:F0}ms)");
                },
                onUpdate: positionModel =>
                {
                    var volume = positionModel.VolumeExt > 0
                        ? positionModel.VolumeExt / 10000.0
                        : positionModel.Volume / 100.0;

                    Logger.Information($"[POSITION UPDATE] #{positionModel.Position}: {positionModel.Symbol}, " +
                               $"Volume: {volume:F2}, Profit: {positionModel.Profit:F2}, Price: {positionModel.PriceCurrent:F5}");
                },
                onDelete: positionModel =>
                {
                    Logger.Information($"[POSITION DELETE] #{positionModel.Position}: {positionModel.Symbol}, " +
                               $"Final Profit: {positionModel.Profit:F2}");
                }
            );
        }

        // Wait for key press
        Console.ReadKey(true);

        Logger.Information("Stopped listening.");
        return 0;
    }

    public async Task<int> ProtobufAsync(string? server, ulong? login, string? password, string? types)
    {
        var settings = CreateConnectionSettings(server, login, password);
        using var manager = await ConnectAsync(settings);

        Logger.Information("=== Listening to Trade Events (Protobuf Models) ===");
        Logger.Information($"Subscribed to: {types ?? EventTypes.Deal}");
        Logger.Information("Press any key to stop...\n");

        long totalBytes = 0;
        int totalDeals = 0;

        // Register Protobuf model handlers
        manager.RegisterDealProtoHandler(
            onAdd: protoDeal =>
            {
                totalDeals++;
                byte[] bytes = protoDeal.ToByteArray();
                totalBytes += bytes.Length;

                Logger.Information($"[PROTO DEAL] #{protoDeal.Deal}: {protoDeal.Symbol}, " +
                           $"Binary size: {bytes.Length} bytes");

                // Show stats every 10 deals
                if (totalDeals % 10 == 0)
                {
                    Logger.Information($"Stats: {totalDeals} deals, avg size: {totalBytes / totalDeals} bytes");
                }
            }
        );

        // Wait for key press
        Console.ReadKey(true);

        Logger.Information($"Stopped listening. Total: {totalDeals} deals, {totalBytes:N0} bytes");
        return 0;
    }

    public async Task<int> MixedAsync(string? server, ulong? login, string? password, string? types)
    {
        var settings = CreateConnectionSettings(server, login, password);
        using var manager = await ConnectAsync(settings);

        Logger.Information("=== Listening with Mixed Handlers ===");
        Logger.Information($"- Subscribed to: {types ?? EventTypes.Deal}");
        Logger.Information("- POCO models for console display");
        Logger.Information("- Protobuf models for binary serialization");
        Logger.Information("Press any key to stop...\n");

        // Handler 1: POCO for display
        manager.RegisterDealHandler(
            onAdd: dealModel =>
            {
                Logger.Information($"[DISPLAY] Deal #{dealModel.Deal}: {dealModel.Symbol}");
            }
        );

        // Handler 2: Protobuf for serialization
        manager.RegisterDealProtoHandler(
            onAdd: protoDeal =>
            {
                // Simulate sending to Kafka/Redis
                byte[] data = protoDeal.ToByteArray();
                Logger.Debug($"[SERIALIZED] Deal #{protoDeal.Deal} -> {data.Length} bytes");
            }
        );

        // Wait for key press
        Console.ReadKey(true);

        Logger.Information("Stopped listening.");
        return 0;
    }

    #region Event Handlers

    // No longer using CIMT... objects in public handlers

    #endregion
}
