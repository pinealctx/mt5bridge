using MetaQuotes.MT5CommonAPI;
using Microsoft.Extensions.Configuration;
using Serilog;
using MT5Bridge.Manager;
using MT5Bridge.Manager.Managers;
using MT5Bridge.Manager.Models;
using MT5Bridge.Manager.Models.Proto;
using Google.Protobuf;
using ProtoDeal = MT5Bridge.Manager.Models.Proto.DealModel;
using ProtoOrder = MT5Bridge.Manager.Models.Proto.OrderModel;
using ProtoPosition = MT5Bridge.Manager.Models.Proto.PositionModel;
using ProtoAccount = MT5Bridge.Manager.Models.Proto.AccountModel;
using ProtoUser = MT5Bridge.Manager.Models.Proto.UserModel;

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
            _ => await PocoAsync(server, login, password, types)
        };
    }

    public async Task<int> PocoAsync(string? server, ulong? login, string? password, string? types)
    {
        var settings = CreateConnectionSettings(server, login, password);

        Logger.Information("=== Listening to Trade Events (POCO Models) ===");
        Logger.Information($"Subscribed to: {types ?? EventTypes.Deal}");
        Logger.Information("Press any key to stop...\n");

        var startTime = DateTime.Now;
        var typeList = (types ?? EventTypes.Deal).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        bool all = typeList.Contains(EventTypes.All, StringComparer.OrdinalIgnoreCase);

        // Create manager first (subscribe BEFORE connecting)
        var manager = new MT5Manager(Logger);

        // Register POCO model handlers BEFORE connecting
        if (all || typeList.Contains(EventTypes.Deal, StringComparer.OrdinalIgnoreCase))
        {
            manager.RegisterDealHandler(
                onAdd: dealModel =>
                {
                    LogDeal(Logger, "DEAL ADD", dealModel);
                },
                onUpdate: dealModel =>
                {
                    LogDeal(Logger, "DEAL UPDATE", dealModel);
                },
                onDelete: dealModel =>
                {
                    LogDeal(Logger, "DEAL DELETE", dealModel);
                },
                onClean: login =>
                {
                    Logger.Information("DEAL CLEAN: login={Login}", login);
                },
                onSync: () =>
                {
                    Logger.Information("DEAL SYNC completed");
                },
                onPerform: (deal, account, position) =>
                {
                    Logger.Information("=== DEAL PERFORM START ===");
                    LogDeal(Logger, "DEAL (in PERFORM)", deal);
                    LogAccount(Logger, "ACCOUNT (in PERFORM)", account);
                    LogPosition(Logger, "POSITION (in PERFORM)", position);
                    Logger.Information("=== DEAL PERFORM END ===");
                }
            );
        }

        if (all || typeList.Contains(EventTypes.Order, StringComparer.OrdinalIgnoreCase))
        {
            manager.RegisterOrderHandler(
                onAdd: orderModel =>
                {
                    LogOrder(Logger, "ORDER ADD", orderModel);
                },
                onUpdate: orderModel =>
                {
                    LogOrder(Logger, "ORDER UPDATE", orderModel);
                },
                onDelete: orderModel =>
                {
                    LogOrder(Logger, "ORDER DELETE", orderModel);
                },
                onClean: login =>
                {
                    Logger.Information("ORDER CLEAN: login={Login}", login);
                },
                onSync: () =>
                {
                    Logger.Information("ORDER SYNC completed");
                }
            );
        }

        if (all || typeList.Contains(EventTypes.Position, StringComparer.OrdinalIgnoreCase))
        {
            manager.RegisterPositionHandler(
                onAdd: positionModel =>
                {
                    LogPosition(Logger, "POSITION ADD", positionModel);
                },
                onUpdate: positionModel =>
                {
                    LogPosition(Logger, "POSITION UPDATE", positionModel);
                },
                onDelete: positionModel =>
                {
                    LogPosition(Logger, "POSITION DELETE", positionModel);
                },
                onClean: login =>
                {
                    Logger.Information("POSITION CLEAN: login={Login}", login);
                },
                onSync: () =>
                {
                    Logger.Information("POSITION SYNC completed");
                }
            );
        }

        // Register Manager handler BEFORE connecting
        manager.RegisterManagerHandler(
            onConnect: () =>
            {
                Logger.Information("MANAGER: Connected to MT5 server");
            },
            onDisconnect: () =>
            {
                Logger.Information("MANAGER: Disconnected from MT5 server");
            },
            onTradeAccountSet: (retCode, login, user, account, orders, positions) =>
            {
                Logger.Information(
                    "MANAGER TRADE_ACCOUNT_SET: RetCode={RetCode} Login={Login} User[Name={Name} Group={Group}] Account[Balance={Balance} Equity={Equity}] Orders={OrderCount} Positions={PositionCount}",
                    retCode, login, user.FirstName, user.Group, account.Balance, account.Equity, orders.Count, positions.Count);
                foreach (var order in orders.Take(3))
                {
                    Logger.Information("  Order: {Order} {Symbol} {Volume}@{Price} State={State}",
                        order.Order, order.Symbol, order.VolumeInitial, order.PriceOrder, order.State);
                }
                if (orders.Count > 3)
                {
                    Logger.Information("  ... and {MoreCount} more orders", orders.Count - 3);
                }
                foreach (var pos in positions.Take(3))
                {
                    Logger.Information("  Position: {PosID} {Symbol} {Volume}@{Price}",
                        pos.Position, pos.Symbol, pos.Volume, pos.PriceOpen);
                }
                if (positions.Count > 3)
                {
                    Logger.Information("  ... and {MoreCount} more positions", positions.Count - 3);
                }
            }
        );

        // Now connect AFTER registering handlers
        using (manager)
        {
            // Subscribe to connection state changes
            manager.ConnectionStateChanged += (sender, e) =>
            {
                Logger.Information("Connection state changed: {OldState} -> {NewState} ({Message})", e.OldState, e.NewState, e.Message);
            };

            // Connect
            var connectResult = await manager.ConnectAsync(settings);
            if (!connectResult.IsSuccess)
            {
                Logger.Error("Failed to connect: {Message}", connectResult.Message);
                return 1;
            }

            Logger.Information("Connected successfully!");

            // Wait for key press
            Console.ReadKey(true);

            Logger.Information("Stopped listening.");
        }

        return 0;
    }

    public async Task<int> ProtobufAsync(string? server, ulong? login, string? password, string? types)
    {
        var settings = CreateConnectionSettings(server, login, password);

        Logger.Information("=== Listening to Trade Events (Protobuf Models) ===");
        Logger.Information($"Subscribed to: {types ?? EventTypes.Deal}");
        Logger.Information("Press any key to stop...\n");

        var typeList = (types ?? EventTypes.Deal).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        bool all = typeList.Contains(EventTypes.All, StringComparer.OrdinalIgnoreCase);

        // Create manager first (subscribe BEFORE connecting)
        var manager = new MT5Manager(Logger);

        // Register Protobuf model handlers BEFORE connecting
        if (all || typeList.Contains(EventTypes.Deal, StringComparer.OrdinalIgnoreCase))
        {
            manager.RegisterDealProtoHandler(
                onAdd: protoDeal =>
                {
                    LogDealProto(Logger, "PROTO DEAL ADD", protoDeal);
                },
                onUpdate: protoDeal =>
                {
                    LogDealProto(Logger, "PROTO DEAL UPDATE", protoDeal);
                },
                onDelete: protoDeal =>
                {
                    LogDealProto(Logger, "PROTO DEAL DELETE", protoDeal);
                },
                onClean: login =>
                {
                    Logger.Information("PROTO DEAL CLEAN: login={Login}", login);
                },
                onSync: () =>
                {
                    Logger.Information("PROTO DEAL SYNC completed");
                },
                onPerform: (deal, account, position) =>
                {
                    Logger.Information("=== PROTO DEAL PERFORM START ===");
                    LogDealProto(Logger, "PROTO DEAL (in PERFORM)", deal);
                    LogAccountProto(Logger, "PROTO ACCOUNT (in PERFORM)", account);
                    LogPositionProto(Logger, "PROTO POSITION (in PERFORM)", position);
                    Logger.Information("=== PROTO DEAL PERFORM END ===");
                }
            );
        }

        if (all || typeList.Contains(EventTypes.Order, StringComparer.OrdinalIgnoreCase))
        {
            manager.RegisterOrderProtoHandler(
                onAdd: protoOrder =>
                {
                    LogOrderProto(Logger, "PROTO ORDER ADD", protoOrder);
                },
                onUpdate: protoOrder =>
                {
                    LogOrderProto(Logger, "PROTO ORDER UPDATE", protoOrder);
                },
                onDelete: protoOrder =>
                {
                    LogOrderProto(Logger, "PROTO ORDER DELETE", protoOrder);
                },
                onClean: login =>
                {
                    Logger.Information("PROTO ORDER CLEAN: login={Login}", login);
                },
                onSync: () =>
                {
                    Logger.Information("PROTO ORDER SYNC completed");
                }
            );
        }

        if (all || typeList.Contains(EventTypes.Position, StringComparer.OrdinalIgnoreCase))
        {
            manager.RegisterPositionProtoHandler(
                onAdd: protoPosition =>
                {
                    LogPositionProto(Logger, "PROTO POSITION ADD", protoPosition);
                },
                onUpdate: protoPosition =>
                {
                    LogPositionProto(Logger, "PROTO POSITION UPDATE", protoPosition);
                },
                onDelete: protoPosition =>
                {
                    LogPositionProto(Logger, "PROTO POSITION DELETE", protoPosition);
                },
                onClean: login =>
                {
                    Logger.Information("PROTO POSITION CLEAN: login={Login}", login);
                },
                onSync: () =>
                {
                    Logger.Information("PROTO POSITION SYNC completed");
                }
            );
        }

        // Register Manager handler BEFORE connecting
        manager.RegisterManagerProtoHandler(
            onConnect: () =>
            {
                Logger.Information("MANAGER: Connected to MT5 server");
            },
            onDisconnect: () =>
            {
                Logger.Information("MANAGER: Disconnected from MT5 server");
            },
            onTradeAccountSet: (retCode, login, user, account, orders, positions) =>
            {
                Logger.Information(
                    "MANAGER TRADE_ACCOUNT_SET: RetCode={RetCode} Login={Login} User[Name={Name} Group={Group}] Account[Balance={Balance} Equity={Equity}] Orders={OrderCount} Positions={PositionCount}",
                    retCode, login, user.FirstName, user.Group, account.Balance, account.Equity, orders.Count, positions.Count);
                foreach (var order in orders.Take(3))
                {
                    Logger.Information("  Order: {Order} {Symbol} {Volume}@{Price} State={State}",
                        order.Order, order.Symbol, order.VolumeInitial, order.PriceOrder, order.State);
                }
                if (orders.Count > 3)
                {
                    Logger.Information("  ... and {MoreCount} more orders", orders.Count - 3);
                }
                foreach (var pos in positions.Take(3))
                {
                    Logger.Information("  Position: {PosID} {Symbol} {Volume}@{Price}",
                        pos.Position, pos.Symbol, pos.Volume, pos.PriceOpen);
                }
                if (positions.Count > 3)
                {
                    Logger.Information("  ... and {MoreCount} more positions", positions.Count - 3);
                }
            }
        );

        // Now connect AFTER registering handlers
        using (manager)
        {
            // Subscribe to connection state changes
            manager.ConnectionStateChanged += (sender, e) =>
            {
                Logger.Information("Connection state changed: {OldState} -> {NewState} ({Message})", e.OldState, e.NewState, e.Message);
            };

            // Connect
            var connectResult = await manager.ConnectAsync(settings);
            if (!connectResult.IsSuccess)
            {
                Logger.Error("Failed to connect: {Message}", connectResult.Message);
                return 1;
            }

            Logger.Information("Connected successfully!");

            // Wait for key press
            Console.ReadKey(true);

            Logger.Information("Stopped listening.");
        }

        return 0;
    }

    #region Logging Helpers

    /// <summary>
    /// Logs all properties of a DealModel using structured logging (excludes ApiData)
    /// </summary>
    private static void LogDeal(ILogger logger, string eventType, Manager.Models.DealModel deal)
    {
        logger.Information(
            "[{EventType}] Deal:{Deal} ExtID:{ExternalID} Login:{Login} Dealer:{Dealer} Order:{Order} " +
            "Symbol:{Symbol} Action:{Action} Entry:{Entry} Reason:{Reason} " +
            "Digits:{Digits} DigitsCurr:{DigitsCurrency} ContractSize:{ContractSize} " +
            "Time:{Time} TimeMsc:{TimeMsc} " +
            "Price:{Price} PricePos:{PricePosition} PriceSL:{PriceSL} PriceTP:{PriceTP} PriceGw:{PriceGateway} " +
            "Vol:{Volume} VolClosed:{VolumeClosed} VolExt:{VolumeExt} VolClosedExt:{VolumeClosedExt} " +
            "Profit:{Profit} ProfitRaw:{ProfitRaw} Storage:{Storage} Comm:{Commission} Fee:{Fee} " +
            "Value:{Value} ObsValue:{ObsoleteValue} " +
            "RateProfit:{RateProfit} RateMargin:{RateMargin} " +
            "TickVal:{TickValue} TickSize:{TickSize} " +
            "Bid:{MarketBid} Ask:{MarketAsk} Last:{MarketLast} " +
            "ExpertID:{ExpertID} PosID:{PositionID} " +
            "Gateway:{Gateway} Comment:{Comment} " +
            "Flags:{Flags} ModFlags:{ModificationFlags}",
            eventType, deal.Deal, deal.ExternalID, deal.Login, deal.Dealer, deal.Order,
            deal.Symbol, deal.Action, deal.Entry, deal.Reason,
            deal.Digits, deal.DigitsCurrency, deal.ContractSize,
            deal.Time, deal.TimeMsc,
            deal.Price, deal.PricePosition, deal.PriceSL, deal.PriceTP, deal.PriceGateway,
            deal.Volume, deal.VolumeClosed, deal.VolumeExt, deal.VolumeClosedExt,
            deal.Profit, deal.ProfitRaw, deal.Storage, deal.Commission, deal.Fee,
            deal.Value, deal.ObsoleteValue,
            deal.RateProfit, deal.RateMargin,
            deal.TickValue, deal.TickSize,
            deal.MarketBid, deal.MarketAsk, deal.MarketLast,
            deal.ExpertID, deal.PositionID,
            deal.Gateway, deal.Comment,
            deal.Flags, deal.ModificationFlags);
    }

    /// <summary>
    /// Logs all properties of a Protobuf DealModel using structured logging
    /// </summary>
    private static void LogDealProto(ILogger logger, string eventType, ProtoDeal deal)
    {
        logger.Information(
            "[{EventType}] Deal:{Deal} ExtID:{ExternalId} Login:{Login} Dealer:{Dealer} Order:{Order} " +
            "Symbol:{Symbol} Action:{Action} Entry:{Entry} Reason:{Reason} " +
            "Digits:{Digits} DigitsCurr:{DigitsCurrency} ContractSize:{ContractSize} " +
            "Time:{Time} TimeMsc:{TimeMsc} " +
            "Price:{Price} PricePos:{PricePosition} PriceSL:{PriceSl} PriceTP:{PriceTp} PriceGw:{PriceGateway} " +
            "Vol:{Volume} VolClosed:{VolumeClosed} VolExt:{VolumeExt} VolClosedExt:{VolumeClosedExt} " +
            "Profit:{Profit} ProfitRaw:{ProfitRaw} Storage:{Storage} Comm:{Commission} Fee:{Fee} " +
            "Value:{Value} ObsValue:{ObsoleteValue} " +
            "RateProfit:{RateProfit} RateMargin:{RateMargin} " +
            "TickVal:{TickValue} TickSize:{TickSize} " +
            "Bid:{MarketBid} Ask:{MarketAsk} Last:{MarketLast} " +
            "ExpertID:{ExpertId} PosID:{PositionId} " +
            "Gateway:{Gateway} Comment:{Comment} " +
            "Flags:{Flags} ModFlags:{ModificationFlags}",
            eventType, deal.Deal, deal.ExternalId, deal.Login, deal.Dealer, deal.Order,
            deal.Symbol, deal.Action, deal.Entry, deal.Reason,
            deal.Digits, deal.DigitsCurrency, deal.ContractSize,
            deal.Time, deal.TimeMsc,
            deal.Price, deal.PricePosition, deal.PriceSl, deal.PriceTp, deal.PriceGateway,
            deal.Volume, deal.VolumeClosed, deal.VolumeExt, deal.VolumeClosedExt,
            deal.Profit, deal.ProfitRaw, deal.Storage, deal.Commission, deal.Fee,
            deal.Value, deal.ObsoleteValue,
            deal.RateProfit, deal.RateMargin,
            deal.TickValue, deal.TickSize,
            deal.MarketBid, deal.MarketAsk, deal.MarketLast,
            deal.ExpertId, deal.PositionId,
            deal.Gateway, deal.Comment,
            deal.Flags, deal.ModificationFlags);
    }

    /// <summary>
    /// Logs all properties of an AccountModel using structured logging
    /// </summary>
    private static void LogAccount(ILogger logger, string eventType, Manager.Models.AccountModel account)
    {
#pragma warning disable CS0618 // Commission is obsolete but we want to log it
        logger.Information(
            "[{EventType}] Login:{Login} CurrencyDigits:{CurrencyDigits} " +
            "Balance:{Balance} Credit:{Credit} Equity:{Equity} Profit:{Profit} " +
            "Margin:{Margin} MarginFree:{MarginFree} MarginLevel:{MarginLevel} MarginLeverage:{MarginLeverage} " +
            "MarginInitial:{MarginInitial} MarginMaintenance:{MarginMaintenance} " +
            "Storage:{Storage} Floating:{Floating} Commission:{Commission} " +
            "SOActivation:{SOActivation} SOTime:{SOTime} SOLevel:{SOLevel} SOEquity:{SOEquity} SOMargin:{SOMargin} " +
            "BlockedCommission:{BlockedCommission} BlockedProfit:{BlockedProfit} " +
            "Assets:{Assets} Liabilities:{Liabilities}",
            eventType, account.Login, account.CurrencyDigits,
            account.Balance, account.Credit, account.Equity, account.Profit,
            account.Margin, account.MarginFree, account.MarginLevel, account.MarginLeverage,
            account.MarginInitial, account.MarginMaintenance,
            account.Storage, account.Floating, account.Commission,
            account.SOActivation, account.SOTime, account.SOLevel, account.SOEquity, account.SOMargin,
            account.BlockedCommission, account.BlockedProfit,
            account.Assets, account.Liabilities);
#pragma warning restore CS0618
    }

    /// <summary>
    /// Logs all properties of a Protobuf AccountModel using structured logging
    /// </summary>
    private static void LogAccountProto(ILogger logger, string eventType, ProtoAccount account)
    {
#pragma warning disable CS0618 // Commission is obsolete but we want to log it
        logger.Information(
            "[{EventType}] Login:{Login} CurrencyDigits:{CurrencyDigits} " +
            "Balance:{Balance} Credit:{Credit} Equity:{Equity} Profit:{Profit} " +
            "Margin:{Margin} MarginFree:{MarginFree} MarginLevel:{MarginLevel} MarginLeverage:{MarginLeverage} " +
            "MarginInitial:{MarginInitial} MarginMaintenance:{MarginMaintenance} " +
            "Storage:{Storage} Floating:{Floating} Commission:{Commission} " +
            "BlockedCommission:{BlockedCommission} BlockedProfit:{BlockedProfit} " +
            "Assets:{Assets} Liabilities:{Liabilities}",
            eventType, account.Login, account.CurrencyDigits,
            account.Balance, account.Credit, account.Equity, account.Profit,
            account.Margin, account.MarginFree, account.MarginLevel, account.MarginLeverage,
            account.MarginInitial, account.MarginMaintenance,
            account.Storage, account.Floating, account.Commission,
            account.BlockedCommission, account.BlockedProfit,
            account.Assets, account.Liabilities);
#pragma warning restore CS0618
    }

    /// <summary>
    /// Logs all properties of an OrderModel using structured logging (excludes ApiData)
    /// </summary>
    private static void LogOrder(ILogger logger, string eventType, Manager.Models.OrderModel order)
    {
        logger.Information(
            "[{EventType}] Order:{Order} ExtID:{ExternalID} Login:{Login} Dealer:{Dealer} Symbol:{Symbol} " +
            "Digits:{Digits} DigitsCurr:{DigitsCurrency} ContractSize:{ContractSize} " +
            "State:{State} Reason:{Reason} " +
            "TimeSetup:{TimeSetup} TimeExp:{TimeExpiration} TimeDone:{TimeDone} " +
            "TimeSetupMsc:{TimeSetupMsc} TimeDoneMsc:{TimeDoneMsc} " +
            "Type:{Type} TypeFill:{TypeFill} TypeTime:{TypeTime} " +
            "PriceOrder:{PriceOrder} PriceTrigger:{PriceTrigger} PriceCurrent:{PriceCurrent} PriceSL:{PriceSL} PriceTP:{PriceTP} " +
            "VolInit:{VolumeInitial} VolCurr:{VolumeCurrent} VolInitExt:{VolumeInitialExt} VolCurrExt:{VolumeCurrentExt} " +
            "ExpertID:{ExpertID} PosID:{PositionID} PosByID:{PositionByID} " +
            "ActMode:{ActivationMode} ActTime:{ActivationTime} ActPrice:{ActivationPrice} ActFlags:{ActivationFlags} " +
            "Comment:{Comment} RateMargin:{RateMargin} ModFlags:{ModificationFlags}",
            eventType, order.Order, order.ExternalID, order.Login, order.Dealer, order.Symbol,
            order.Digits, order.DigitsCurrency, order.ContractSize,
            order.State, order.Reason,
            order.TimeSetup, order.TimeExpiration, order.TimeDone,
            order.TimeSetupMsc, order.TimeDoneMsc,
            order.Type, order.TypeFill, order.TypeTime,
            order.PriceOrder, order.PriceTrigger, order.PriceCurrent, order.PriceSL, order.PriceTP,
            order.VolumeInitial, order.VolumeCurrent, order.VolumeInitialExt, order.VolumeCurrentExt,
            order.ExpertID, order.PositionID, order.PositionByID,
            order.ActivationMode, order.ActivationTime, order.ActivationPrice, order.ActivationFlags,
            order.Comment, order.RateMargin, order.ModificationFlags);
    }

    /// <summary>
    /// Logs all properties of a Protobuf OrderModel using structured logging
    /// </summary>
    private static void LogOrderProto(ILogger logger, string eventType, ProtoOrder order)
    {
        logger.Information(
            "[{EventType}] Order:{Order} ExtID:{ExternalId} Login:{Login} Dealer:{Dealer} Symbol:{Symbol} " +
            "Digits:{Digits} DigitsCurr:{DigitsCurrency} ContractSize:{ContractSize} " +
            "State:{State} Reason:{Reason} " +
            "TimeSetup:{TimeSetup} TimeExp:{TimeExpiration} TimeDone:{TimeDone} " +
            "TimeSetupMsc:{TimeSetupMsc} TimeDoneMsc:{TimeDoneMsc} " +
            "Type:{Type} TypeFill:{TypeFill} TypeTime:{TypeTime} " +
            "PriceOrder:{PriceOrder} PriceTrigger:{PriceTrigger} PriceCurrent:{PriceCurrent} PriceSL:{PriceSl} PriceTP:{PriceTp} " +
            "VolInit:{VolumeInitial} VolCurr:{VolumeCurrent} VolInitExt:{VolumeInitialExt} VolCurrExt:{VolumeCurrentExt} " +
            "ExpertID:{ExpertId} PosID:{PositionId} PosByID:{PositionById} " +
            "ActMode:{ActivationMode} ActTime:{ActivationTime} ActPrice:{ActivationPrice} ActFlags:{ActivationFlags} " +
            "Comment:{Comment} RateMargin:{RateMargin} ModFlags:{ModificationFlags}",
            eventType, order.Order, order.ExternalId, order.Login, order.Dealer, order.Symbol,
            order.Digits, order.DigitsCurrency, order.ContractSize,
            order.State, order.Reason,
            order.TimeSetup, order.TimeExpiration, order.TimeDone,
            order.TimeSetupMsc, order.TimeDoneMsc,
            order.Type, order.TypeFill, order.TypeTime,
            order.PriceOrder, order.PriceTrigger, order.PriceCurrent, order.PriceSl, order.PriceTp,
            order.VolumeInitial, order.VolumeCurrent, order.VolumeInitialExt, order.VolumeCurrentExt,
            order.ExpertId, order.PositionId, order.PositionById,
            order.ActivationMode, order.ActivationTime, order.ActivationPrice, order.ActivationFlags,
            order.Comment, order.RateMargin, order.ModificationFlags);
    }

    /// <summary>
    /// Logs all properties of a PositionModel using structured logging (excludes ApiData)
    /// </summary>
    private static void LogPosition(ILogger logger, string eventType, Manager.Models.PositionModel position)
    {
        logger.Information(
            "[{EventType}] Pos:{Position} ExtID:{ExternalID} Login:{Login} Dealer:{Dealer} Symbol:{Symbol} " +
            "Action:{Action} Reason:{Reason} " +
            "Digits:{Digits} DigitsCurr:{DigitsCurrency} ContractSize:{ContractSize} " +
            "TimeCreate:{TimeCreate} TimeUpdate:{TimeUpdate} TimeCreateMsc:{TimeCreateMsc} TimeUpdateMsc:{TimeUpdateMsc} " +
            "PriceOpen:{PriceOpen} PriceCurrent:{PriceCurrent} PriceSL:{PriceSL} PriceTP:{PriceTP} " +
            "Vol:{Volume} VolExt:{VolumeExt} " +
            "Profit:{Profit} Storage:{Storage} ObsValue:{ObsoleteValue} " +
            "RateProfit:{RateProfit} RateMargin:{RateMargin} " +
            "ExpertID:{ExpertID} ExpertPosID:{ExpertPositionID} " +
            "Comment:{Comment} " +
            "ActMode:{ActivationMode} ActTime:{ActivationTime} ActPrice:{ActivationPrice} ActFlags:{ActivationFlags} " +
            "ModFlags:{ModificationFlags}",
            eventType, position.Position, position.ExternalID, position.Login, position.Dealer, position.Symbol,
            position.Action, position.Reason,
            position.Digits, position.DigitsCurrency, position.ContractSize,
            position.TimeCreate, position.TimeUpdate, position.TimeCreateMsc, position.TimeUpdateMsc,
            position.PriceOpen, position.PriceCurrent, position.PriceSL, position.PriceTP,
            position.Volume, position.VolumeExt,
            position.Profit, position.Storage, position.ObsoleteValue,
            position.RateProfit, position.RateMargin,
            position.ExpertID, position.ExpertPositionID,
            position.Comment,
            position.ActivationMode, position.ActivationTime, position.ActivationPrice, position.ActivationFlags,
            position.ModificationFlags);
    }

    /// <summary>
    /// Logs all properties of a Protobuf PositionModel using structured logging
    /// </summary>
    private static void LogPositionProto(ILogger logger, string eventType, ProtoPosition position)
    {
        logger.Information(
            "[{EventType}] Pos:{Position} ExtID:{ExternalId} Login:{Login} Dealer:{Dealer} Symbol:{Symbol} " +
            "Action:{Action} Reason:{Reason} " +
            "Digits:{Digits} DigitsCurr:{DigitsCurrency} ContractSize:{ContractSize} " +
            "TimeCreate:{TimeCreate} TimeUpdate:{TimeUpdate} TimeCreateMsc:{TimeCreateMsc} TimeUpdateMsc:{TimeUpdateMsc} " +
            "PriceOpen:{PriceOpen} PriceCurrent:{PriceCurrent} PriceSL:{PriceSl} PriceTP:{PriceTp} " +
            "Vol:{Volume} VolExt:{VolumeExt} " +
            "Profit:{Profit} Storage:{Storage} ObsValue:{ObsoleteValue} " +
            "RateProfit:{RateProfit} RateMargin:{RateMargin} " +
            "ExpertID:{ExpertId} ExpertPosID:{ExpertPositionId} " +
            "Comment:{Comment} " +
            "ActMode:{ActivationMode} ActTime:{ActivationTime} ActPrice:{ActivationPrice} ActFlags:{ActivationFlags} " +
            "ModFlags:{ModificationFlags}",
            eventType, position.Position, position.ExternalId, position.Login, position.Dealer, position.Symbol,
            position.Action, position.Reason,
            position.Digits, position.DigitsCurrency, position.ContractSize,
            position.TimeCreate, position.TimeUpdate, position.TimeCreateMsc, position.TimeUpdateMsc,
            position.PriceOpen, position.PriceCurrent, position.PriceSl, position.PriceTp,
            position.Volume, position.VolumeExt,
            position.Profit, position.Storage, position.ObsoleteValue,
            position.RateProfit, position.RateMargin,
            position.ExpertId, position.ExpertPositionId,
            position.Comment,
            position.ActivationMode, position.ActivationTime, position.ActivationPrice, position.ActivationFlags,
            position.ModificationFlags);
    }

    #endregion
}
