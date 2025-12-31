using Serilog;
using MT5Bridge.Manager.Managers;
using MT5Bridge.Manager.Models;

namespace MT5Bridge.Manager.Demo.Commands.Handlers;

/// <summary>
/// POCO model handler registration strategy
/// Responsible for:
/// - Registering event handlers for POCO models
/// - Logging event information to the logger
/// </summary>
public class PocoHandlerRegistrationStrategy : IHandlerRegistrationStrategy
{
    private readonly ILogger _logger;

    public PocoHandlerRegistrationStrategy(ILogger logger)
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
            _logger.Error("Failed to register manager handler: {Code}, {Message}", result.RetCode, result.Message);
            return result;
        }

        if (slim)
        {
            result = RegisterOrderSlimHandler(manager);
            if (!result.IsSuccess)
            {
                _logger.Error("Failed to register order slim handler: {Code}, {Message}", result.RetCode, result.Message);
                return result;
            }
            result = RegisterPositionSlimHandler(manager);
            if (!result.IsSuccess)
            {
                _logger.Error("Failed to register position slim handler: {Code}, {Message}", result.RetCode, result.Message);
                return result;
            }
            result = RegisterDealHandler(manager);
            if (!result.IsSuccess)
            {
                _logger.Error("Failed to register deal handler: {Code}, {Message}", result.RetCode, result.Message);
                return result;
            }
            return result;
        }
        else
        {
            bool all = typeList.Contains(EventTypes.All, StringComparer.OrdinalIgnoreCase);

            // Deal handler
            if (all || typeList.Contains(EventTypes.Deal, StringComparer.OrdinalIgnoreCase))
            {
                result = RegisterDealHandler(manager);
                if (!result.IsSuccess)
                {
                    _logger.Error("Failed to register deal handler: {Code}, {Message}", result.RetCode, result.Message);
                    return result;
                }
            }

            // Order handler
            if (all || typeList.Contains(EventTypes.Order, StringComparer.OrdinalIgnoreCase))
            {
                result = RegisterOrderHandler(manager);
                if (!result.IsSuccess)
                {
                    _logger.Error("Failed to register order handler: {Code}, {Message}", result.RetCode, result.Message);
                    return result;
                }
            }

            // Position handler
            if (all || typeList.Contains(EventTypes.Position, StringComparer.OrdinalIgnoreCase))
            {
                result = RegisterPositionHandler(manager);
                if (!result.IsSuccess)
                {
                    _logger.Error("Failed to register position handler: {Code}, {Message}", result.RetCode, result.Message);
                    return result;
                }
            }
        }

        return result;
    }

    private MT5Result RegisterDealHandler(MT5Manager manager)
    {
        return manager.RegisterDealHandler(
            onAdd: dealModel => { LogDeal(_logger, "DEAL ADD", dealModel); },
            onUpdate: dealModel => { LogDeal(_logger, "DEAL UPDATE", dealModel); },
            onDelete: dealModel => { LogDeal(_logger, "DEAL DELETE", dealModel); },
            onClean: login => { _logger.Information("DEAL CLEAN: login={Login}", login); },
            onSync: () => { _logger.Information("DEAL SYNC completed"); },
            onPerform: (deal, account, position) =>
            {
                _logger.Information("=== DEAL PERFORM START ===");
                LogDeal(_logger, "DEAL (in PERFORM)", deal);
                LogAccount(_logger, "ACCOUNT (in PERFORM)", account);
                LogPosition(_logger, "POSITION (in PERFORM)", position);
                _logger.Information("=== DEAL PERFORM END ===");
            }
        );
    }

    private MT5Result RegisterOrderHandler(MT5Manager manager)
    {
        return manager.RegisterOrderHandler(
            onAdd: orderModel => { LogOrder(_logger, "ORDER ADD", orderModel); },
            onUpdate: orderModel => { LogOrder(_logger, "ORDER UPDATE", orderModel); },
            onDelete: orderModel => { LogOrder(_logger, "ORDER DELETE", orderModel); },
            onClean: login => { _logger.Information("ORDER CLEAN: login={Login}", login); },
            onSync: () => { _logger.Information("ORDER SYNC completed"); }
        );
    }

    private MT5Result RegisterPositionHandler(MT5Manager manager)
    {
        return manager.RegisterPositionHandler(
            onAdd: positionModel => { LogPosition(_logger, "POSITION ADD", positionModel); },
            onUpdate: positionModel => { LogPosition(_logger, "POSITION UPDATE", positionModel); },
            onDelete: positionModel => { LogPosition(_logger, "POSITION DELETE", positionModel); },
            onClean: login => { _logger.Information("POSITION CLEAN: login={Login}", login); },
            onSync: () => { _logger.Information("POSITION SYNC completed"); }
        );
    }

    private MT5Result RegisterManagerHandler(MT5Manager manager)
    {
        return manager.RegisterManagerHandler(
            onConnect: () => { _logger.Information("MANAGER: Connected to MT5 server"); },
            onDisconnect: () => { _logger.Information("MANAGER: Disconnected from MT5 server"); },
            onTradeAccountSet: (retCode, login, user, account, orders, positions) =>
            {
                _logger.Information("MANAGER: Trade account set for login={Login}, RetCode={RetCode}", login, retCode);
                if (user != null) LogUser(_logger, "USER", user);
                if (account != null) LogAccount(_logger, "ACCOUNT", account);
                if (orders != null) foreach (var o in orders) LogOrder(_logger, "ORDER", o);
                if (positions != null) foreach (var p in positions) LogPosition(_logger, "POSITION", p);
            }
        );
    }

    private MT5Result RegisterOrderSlimHandler(MT5Manager manager)
    {
        return manager.RegisterOrderHandler(
            onAdd: null,
            onUpdate: null,
            onDelete: null,
            onClean: login => { _logger.Information("ORDER CLEAN: login={Login}", login); },
            onSync: () => { _logger.Information("ORDER SYNC completed"); }
        );
    }

    private MT5Result RegisterPositionSlimHandler(MT5Manager manager)
    {
        return manager.RegisterPositionHandler(
            onAdd: null,
            onUpdate: null,
            onDelete: null,
            onClean: login => { _logger.Information("POSITION CLEAN: login={Login}", login); },
            onSync: () => { _logger.Information("POSITION SYNC completed"); }
        );
    }

    private static void LogDeal(ILogger logger, string prefix, DealModel deal)
    {
        logger.Information(
            "[{Prefix}] Deal:{Deal} ExtID:{ExternalID} Login:{Login} Dealer:{Dealer} Order:{Order} " +
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
            prefix, deal.Deal, deal.ExternalID, deal.Login, deal.Dealer, deal.Order,
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

    private static void LogOrder(ILogger logger, string prefix, OrderModel order)
    {
        logger.Information(
            "[{Prefix}] Order:{Order} ExtID:{ExternalID} Login:{Login} Dealer:{Dealer} Symbol:{Symbol} " +
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
            prefix, order.Order, order.ExternalID, order.Login, order.Dealer, order.Symbol,
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

    private static void LogPosition(ILogger logger, string prefix, PositionModel position)
    {
        logger.Information(
            "[{Prefix}] Pos:{Position} ExtID:{ExternalID} Login:{Login} Dealer:{Dealer} Symbol:{Symbol} " +
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
            prefix, position.Position, position.ExternalID, position.Login, position.Dealer, position.Symbol,
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

    private static void LogAccount(ILogger logger, string prefix, AccountModel account)
    {
#pragma warning disable CS0618 // Commission is obsolete but we want to log it        
        logger.Information(
            "[{Prefix}] Login:{Login} CurrencyDigits:{CurrencyDigits} " +
            "Balance:{Balance} Credit:{Credit} Equity:{Equity} Profit:{Profit} " +
            "Margin:{Margin} MarginFree:{MarginFree} MarginLevel:{MarginLevel} MarginLeverage:{MarginLeverage} " +
            "MarginInitial:{MarginInitial} MarginMaintenance:{MarginMaintenance} " +
            "Storage:{Storage} Floating:{Floating} Commission:{Commission} " +
            "SOActivation:{SOActivation} SOTime:{SOTime} SOLevel:{SOLevel} SOEquity:{SOEquity} SOMargin:{SOMargin} " +
            "BlockedCommission:{BlockedCommission} BlockedProfit:{BlockedProfit} " +
            "Assets:{Assets} Liabilities:{Liabilities}",
            prefix, account.Login, account.CurrencyDigits,
            account.Balance, account.Credit, account.Equity, account.Profit,
            account.Margin, account.MarginFree, account.MarginLevel, account.MarginLeverage,
            account.MarginInitial, account.MarginMaintenance,
            account.Storage, account.Floating, account.Commission,
            account.SOActivation, account.SOTime, account.SOLevel, account.SOEquity, account.SOMargin,
            account.BlockedCommission, account.BlockedProfit,
            account.Assets, account.Liabilities);
#pragma warning restore CS0618            
    }

    private static void LogUser(ILogger logger, string prefix, UserModel user)
    {
#pragma warning disable CS0618 // Name is obsolete but we want to log it
        logger.Information(
            "[{Prefix}] Login:{Login} Group:{Group} CertSerial:{CertSerialNumber} Rights:{Rights} " +
            "Registration:{Registration} LastAccess:{LastAccess} LastIP:{LastIP} " +
            "Name:{Name} FirstName:{FirstName} LastName:{LastName} MiddleName:{MiddleName} Company:{Company} Account:{Account} " +
            "Country:{Country} Language:{Language} City:{City} State:{State} ZIP:{ZIPCode} Address:{Address} " +
            "Phone:{Phone} Email:{Email} ID:{ID} " +
            "MQID:{MQID} ClientID:{ClientID} Status:{Status} Comment:{Comment} Color:{Color} " +
            "PhonePassword:{PhonePassword} LastPassChange:{LastPassChange} PasswordHash:{PasswordHash} OTPSecret:{OTPSecret} " +
            "Leverage:{Leverage} Agent:{Agent} LimitOrders:{LimitOrders} LimitPositionsValue:{LimitPositionsValue} " +
            "LeadSource:{LeadSource} LeadCampaign:{LeadCampaign} " +
            "Balance:{Balance} BalancePrevDay:{BalancePrevDay} BalancePrevMonth:{BalancePrevMonth} " +
            "EquityPrevDay:{EquityPrevDay} EquityPrevMonth:{EquityPrevMonth} Credit:{Credit} " +
            "InterestRate:{InterestRate} CommissionDaily:{CommissionDaily} CommissionMonthly:{CommissionMonthly} " +
            "CommissionAgentDaily:{CommissionAgentDaily} CommissionAgentMonthly:{CommissionAgentMonthly}",
            prefix, user.Login, user.Group, user.CertSerialNumber, user.Rights,
            user.Registration, user.LastAccess, user.LastIP,
            user.Name, user.FirstName, user.LastName, user.MiddleName, user.Company, user.Account,
            user.Country, user.Language, user.City, user.State, user.ZIPCode, user.Address,
            user.Phone, user.Email, user.ID,
            user.MQID, user.ClientID, user.Status, user.Comment, user.Color,
            user.PhonePassword, user.LastPassChange, user.PasswordHash, user.OTPSecret,
            user.Leverage, user.Agent, user.LimitOrders, user.LimitPositionsValue,
            user.LeadSource, user.LeadCampaign,
            user.Balance, user.BalancePrevDay, user.BalancePrevMonth,
            user.EquityPrevDay, user.EquityPrevMonth, user.Credit,
            user.InterestRate, user.CommissionDaily, user.CommissionMonthly,
            user.CommissionAgentDaily, user.CommissionAgentMonthly);
#pragma warning restore CS0618
    }
}
