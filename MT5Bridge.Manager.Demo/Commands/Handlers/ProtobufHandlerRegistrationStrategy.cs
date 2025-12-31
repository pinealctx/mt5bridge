using Serilog;
using MT5Bridge.Manager.Managers;
using ProtoDeal = MT5Bridge.Manager.Models.Proto.DealModel;
using ProtoOrder = MT5Bridge.Manager.Models.Proto.OrderModel;
using ProtoPosition = MT5Bridge.Manager.Models.Proto.PositionModel;
using ProtoAccount = MT5Bridge.Manager.Models.Proto.AccountModel;
using ProtoUser = MT5Bridge.Manager.Models.Proto.UserModel;

namespace MT5Bridge.Manager.Demo.Commands.Handlers;

/// <summary>
/// Protobuf model handler registration strategy
/// Responsible for:
/// - Registering event handlers for Protobuf models
/// - Logging event information to the logger
/// </summary>
public class ProtobufHandlerRegistrationStrategy : IHandlerRegistrationStrategy
{
    private readonly ILogger _logger;

    public ProtobufHandlerRegistrationStrategy(ILogger logger)
    {
        _logger = logger;
    }

    public MT5Result RegisterHandlers(MT5Manager manager, string? types)
    {
        var typeList = (types ?? EventTypes.Deal)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        bool slim = typeList.Contains(EventTypes.Slim, StringComparer.OrdinalIgnoreCase);

        var result = RegisterManagerHandler(manager);
        if (!result.IsSuccess)
        {
            _logger.Error("Failed to register manager proto handler: {Code}, {Message}", result.RetCode, result.Message);
            return result;
        }

        if (slim)
        {
            result = RegisterOrderSlimHandler(manager);
            if (!result.IsSuccess)
            {
                _logger.Error("Failed to register order slim proto handler: {Code}, {Message}", result.RetCode, result.Message);
                return result;
            }
            result = RegisterPositionSlimHandler(manager);
            if (!result.IsSuccess)
            {
                _logger.Error("Failed to register position slim proto handler: {Code}, {Message}", result.RetCode, result.Message);
                return result;
            }
            result = RegisterDealHandler(manager);
            if (!result.IsSuccess)
            {
                _logger.Error("Failed to register deal proto handler: {Code}, {Message}", result.RetCode, result.Message);
                return result;
            }
            return result;
        }
        else
        {
            bool all = typeList.Contains(EventTypes.All, StringComparer.OrdinalIgnoreCase);
            // Deal handler (using Proto version)
            if (all || typeList.Contains(EventTypes.Deal, StringComparer.OrdinalIgnoreCase))
            {
                result = RegisterDealHandler(manager);
                if (!result.IsSuccess)
                {
                    _logger.Error("Failed to register deal proto handler: {Code}, {Message}", result.RetCode, result.Message);
                    return result;
                }
            }

            // Order handler
            if (all || typeList.Contains(EventTypes.Order, StringComparer.OrdinalIgnoreCase))
            {
                result = RegisterOrderHandler(manager);
                if (!result.IsSuccess)
                {
                    _logger.Error("Failed to register order proto handler: {Code}, {Message}", result.RetCode, result.Message);
                    return result;
                }
            }

            // Position handler
            if (all || typeList.Contains(EventTypes.Position, StringComparer.OrdinalIgnoreCase))
            {
                result = RegisterPositionHandler(manager);
                if (!result.IsSuccess)
                {
                    _logger.Error("Failed to register position proto handler: {Code}, {Message}", result.RetCode, result.Message);
                    return result;
                }
            }
        }
        return result;
    }

    private MT5Result RegisterDealHandler(MT5Manager manager)
    {
        return manager.RegisterDealProtoHandler(
            onAdd: protoDeal => { LogDealProto(_logger, "PROTO DEAL ADD", protoDeal); },
            onUpdate: protoDeal => { LogDealProto(_logger, "PROTO DEAL UPDATE", protoDeal); },
            onDelete: protoDeal => { LogDealProto(_logger, "PROTO DEAL DELETE", protoDeal); },
            onClean: login => { _logger.Information("PROTO DEAL CLEAN: login={Login}", login); },
            onSync: () => { _logger.Information("PROTO DEAL SYNC completed"); },
            onPerform: (deal, account, position) =>
            {
                _logger.Information("=== PROTO DEAL PERFORM START ===");
                LogDealProto(_logger, "PROTO DEAL (in PERFORM)", deal);
                LogAccountProto(_logger, "PROTO ACCOUNT (in PERFORM)", account);
                LogPositionProto(_logger, "PROTO POSITION (in PERFORM)", position);
                _logger.Information("=== PROTO DEAL PERFORM END ===");
            });
    }

    private MT5Result RegisterOrderHandler(MT5Manager manager)
    {
        return manager.RegisterOrderProtoHandler(
            onAdd: protoOrder => { LogOrderProto(_logger, "PROTO ORDER ADD", protoOrder); },
            onUpdate: protoOrder => { LogOrderProto(_logger, "PROTO ORDER UPDATE", protoOrder); },
            onDelete: protoOrder => { LogOrderProto(_logger, "PROTO ORDER DELETE", protoOrder); },
            onClean: login => { _logger.Information("PROTO ORDER CLEAN: login={Login}", login); },
            onSync: () => { _logger.Information("PROTO ORDER SYNC completed"); }
        );
    }

    private MT5Result RegisterPositionHandler(MT5Manager manager)
    {
        return manager.RegisterPositionProtoHandler(
            onAdd: protoPosition => { LogPositionProto(_logger, "PROTO POSITION ADD", protoPosition); },
            onUpdate: protoPosition => { LogPositionProto(_logger, "PROTO POSITION UPDATE", protoPosition); },
            onDelete: protoPosition => { LogPositionProto(_logger, "PROTO POSITION DELETE", protoPosition); },
            onClean: login => { _logger.Information("PROTO POSITION CLEAN: login={Login}", login); },
            onSync: () => { _logger.Information("PROTO POSITION SYNC completed"); }
        );
    }

    private MT5Result RegisterManagerHandler(MT5Manager manager)
    {
        return manager.RegisterManagerProtoHandler(
            onConnect: () => { _logger.Information("PROTO MANAGER: Connected to MT5 server"); },
            onDisconnect: () => { _logger.Information("PROTO MANAGER: Disconnected from MT5 server"); },
            onTradeAccountSet: (retCode, login, user, account, orders, positions) =>
            {
                _logger.Information("PROTO MANAGER: Trade account set for login={Login}, RetCode={RetCode}", login, retCode);
                if (user != null) LogUserProto(_logger, "PROTO USER", user);
                if (account != null) LogAccountProto(_logger, "PROTO ACCOUNT", account);
                if (orders != null) foreach (var o in orders) LogOrderProto(_logger, "PROTO ORDER", o);
                if (positions != null) foreach (var p in positions) LogPositionProto(_logger, "PROTO POSITION", p);
            }
        );
    }

    private MT5Result RegisterOrderSlimHandler(MT5Manager manager)
    {
        return manager.RegisterOrderProtoHandler(
            onAdd: null,
            onUpdate: null,
            onDelete: null,
            onClean: login => { _logger.Information("PROTO ORDER CLEAN: login={Login}", login); },
            onSync: () => { _logger.Information("PROTO ORDER SYNC completed"); }
        );
    }

    private MT5Result RegisterPositionSlimHandler(MT5Manager manager)
    {
        return manager.RegisterPositionProtoHandler(
            onAdd: null,
            onUpdate: null,
            onDelete: null,
            onClean: login => { _logger.Information("PROTO POSITION CLEAN: login={Login}", login); },
            onSync: () => { _logger.Information("PROTO POSITION SYNC completed"); }
        );
    }

    private static void LogDealProto(ILogger logger, string prefix, ProtoDeal deal)
    {
        logger.Information(
            "[{Prefix}] Deal:{Deal} ExtID:{ExternalId} Login:{Login} Dealer:{Dealer} Order:{Order} " +
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
            prefix, deal.Deal, deal.ExternalId, deal.Login, deal.Dealer, deal.Order,
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

    private static void LogOrderProto(ILogger logger, string prefix, ProtoOrder order)
    {
        logger.Information(
            "[{Prefix}] Order:{Order} ExtID:{ExternalId} Login:{Login} Dealer:{Dealer} Symbol:{Symbol} " +
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
            prefix, order.Order, order.ExternalId, order.Login, order.Dealer, order.Symbol,
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

    private static void LogPositionProto(ILogger logger, string prefix, ProtoPosition position)
    {
        logger.Information(
            "[{Prefix}] Pos:{Position} ExtID:{ExternalId} Login:{Login} Dealer:{Dealer} Symbol:{Symbol} " +
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
            prefix, position.Position, position.ExternalId, position.Login, position.Dealer, position.Symbol,
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

    private static void LogAccountProto(ILogger logger, string prefix, ProtoAccount account)
    {
#pragma warning disable CS0618 // Commission is obsolete but we want to log it
        logger.Information(
            "[{Prefix}] Login:{Login} CurrencyDigits:{CurrencyDigits} " +
            "Balance:{Balance} Credit:{Credit} Equity:{Equity} Profit:{Profit} " +
            "Margin:{Margin} MarginFree:{MarginFree} MarginLevel:{MarginLevel} MarginLeverage:{MarginLeverage} " +
            "MarginInitial:{MarginInitial} MarginMaintenance:{MarginMaintenance} " +
            "Storage:{Storage} Floating:{Floating} Commission:{Commission} " +
            "BlockedCommission:{BlockedCommission} BlockedProfit:{BlockedProfit} " +
            "Assets:{Assets} Liabilities:{Liabilities}",
            prefix, account.Login, account.CurrencyDigits,
            account.Balance, account.Credit, account.Equity, account.Profit,
            account.Margin, account.MarginFree, account.MarginLevel, account.MarginLeverage,
            account.MarginInitial, account.MarginMaintenance,
            account.Storage, account.Floating, account.Commission,
            account.BlockedCommission, account.BlockedProfit,
            account.Assets, account.Liabilities);
#pragma warning restore CS0618
    }

    private static void LogUserProto(ILogger logger, string prefix, ProtoUser user)
    {
        logger.Information(
            "[{Prefix}] Login:{Login} Group:{Group} CertSerial:{CertSerialNumber} Rights:{Rights} " +
            "Registration:{Registration} LastAccess:{LastAccess} LastIP:{LastIp} " +
            "Name:{Name} FirstName:{FirstName} LastName:{LastName} MiddleName:{MiddleName} Company:{Company} Account:{Account} " +
            "Country:{Country} Language:{Language} City:{City} State:{State} ZIP:{ZipCode} Address:{Address} " +
            "Phone:{Phone} Email:{Email} ID:{Id} " +
            "MQID:{Mqid} ClientID:{ClientId} Status:{Status} Comment:{Comment} Color:{Color} " +
            "PhonePassword:{PhonePassword} LastPassChange:{LastPassChange} PasswordHash:{PasswordHash} OTPSecret:{OtpSecret} " +
            "Leverage:{Leverage} Agent:{Agent} LimitOrders:{LimitOrders} LimitPositionsValue:{LimitPositionsValue} " +
            "LeadSource:{LeadSource} LeadCampaign:{LeadCampaign} " +
            "Balance:{Balance} BalancePrevDay:{BalancePrevDay} BalancePrevMonth:{BalancePrevMonth} " +
            "EquityPrevDay:{EquityPrevDay} EquityPrevMonth:{EquityPrevMonth} Credit:{Credit} " +
            "InterestRate:{InterestRate} CommissionDaily:{CommissionDaily} CommissionMonthly:{CommissionMonthly} " +
            "CommissionAgentDaily:{CommissionAgentDaily} CommissionAgentMonthly:{CommissionAgentMonthly}",
            prefix, user.Login, user.Group, user.CertSerialNumber, user.Rights,
            user.Registration, user.LastAccess, user.LastIp,
            user.Name, user.FirstName, user.LastName, user.MiddleName, user.Company, user.Account,
            user.Country, user.Language, user.City, user.State, user.ZipCode, user.Address,
            user.Phone, user.Email, user.Id,
            user.Mqid, user.ClientId, user.Status, user.Comment, user.Color,
            user.PhonePassword, user.LastPassChange, user.PasswordHash, user.OtpSecret,
            user.Leverage, user.Agent, user.LimitOrders, user.LimitPositionsValue,
            user.LeadSource, user.LeadCampaign,
            user.Balance, user.BalancePrevDay, user.BalancePrevMonth,
            user.EquityPrevDay, user.EquityPrevMonth, user.Credit,
            user.InterestRate, user.CommissionDaily, user.CommissionMonthly,
            user.CommissionAgentDaily, user.CommissionAgentMonthly);
    }
}
