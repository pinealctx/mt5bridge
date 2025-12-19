using System;
using System.Collections.Generic;
using MetaQuotes.MT5CommonAPI;

namespace MT5Bridge.MT5.Core.Models;

#region Enums

public enum OrderType : uint
{
    /// <summary>A Buy order.</summary>
    Buy = 0,
    /// <summary>A Sell order.</summary>
    Sell = 1,
    /// <summary>A Buy Limit order.</summary>
    BuyLimit = 2,
    /// <summary>A Sell Limit order.</summary>
    SellLimit = 3,
    /// <summary>A Buy Stop order.</summary>
    BuyStop = 4,
    /// <summary>A Sell Stop order.</summary>
    SellStop = 5,
    /// <summary>A Buy Stop Limit order.</summary>
    BuyStopLimit = 6,
    /// <summary>A Sell Stop Limit order.</summary>
    SellStopLimit = 7,
    /// <summary>A Close By order — a simultaneous closure of two opposite positions of the same financial instrument. This operation type is only used in the hedging mode.</summary>
    CloseBy = 8
}

public enum OrderFilling : uint
{
    /// <summary>The FOK (Fill or Kill) policy indicates that an order can only be executed in the specified volume. If the necessary amount of a financial instrument is not currently available in the market, the order will not be executed. The required volume can be filled using several offers available at the moment.</summary>
    FOK = 0,
    /// <summary>The IOC (Immediate or Cancel) policy indicates that a deal can be executed in the maximum volume available in the market within the volume specified in the order. If the order cannot be filled completely, the available volume will be filled, and the remaining volume will be canceled.</summary>
    IOC = 1,
    /// <summary>The Return policy is only used for market, Limit and Stop Limit orders. In case of partial filling, an order with the remaining volume is not canceled, but is processed further. This policy is not used for Stop and Stop Limit orders when they are activated.</summary>
    Return = 2,
    /// <summary>The BOC (Book or Cancel) policy indicates that an order can only be placed in the Depth of Market. If the order can be executed immediately when placed, it is canceled. This policy is used for Limit orders.</summary>
    BOC = 3
}

public enum OrderTime : uint
{
    /// <summary>The order will be in the queue until it is canceled.</summary>
    GTC = 0,
    /// <summary>The order will be in the queue only during the current trading day.</summary>
    Day = 1,
    /// <summary>The order will be in the queue until the time specified in the order.</summary>
    Specified = 2,
    /// <summary>The order will be in the queue until the end of the day specified in the order.</summary>
    SpecifiedDay = 3
}

public enum OrderState : uint
{
    /// <summary>The order correctness has been checked, but it has not yet been accepted by the broker.</summary>
    Started = 0,
    /// <summary>The order has been accepted.</summary>
    Placed = 1,
    /// <summary>The order has been canceled by a client.</summary>
    Canceled = 2,
    /// <summary>The order has been partially filled.</summary>
    Partial = 3,
    /// <summary>The order has been filled.</summary>
    Filled = 4,
    /// <summary>The order has been rejected.</summary>
    Rejected = 5,
    /// <summary>The order has been canceled upon expiration.</summary>
    Expired = 6,
    /// <summary>The order is being registered (placed in the system).</summary>
    RequestAdd = 7,
    /// <summary>The order is being modified (changing the order parameters).</summary>
    RequestModify = 8,
    /// <summary>The order is being deleted (removing the order from the system).</summary>
    RequestCancel = 9
}

public enum OrderActivation : uint
{
    /// <summary>No activation.</summary>
    None = 0,
    /// <summary>Pending order activation.</summary>
    Pending = 1,
    /// <summary>Stop Limit order activation.</summary>
    StopLimit = 2,
    /// <summary>Order cancellation upon expiration.</summary>
    Expiration = 3,
    /// <summary>Order cancellation upon Stop Out.</summary>
    StopOut = 4
}

public enum OrderReason : uint
{
    /// <summary>The order is placed by a client manually through the client terminal.</summary>
    Client = 0,
    /// <summary>The order is placed by a client using an Expert Advisor.</summary>
    Expert = 1,
    /// <summary>The order is placed by a dealer through the manager terminal.</summary>
    Dealer = 2,
    /// <summary>The order is placed as a result of Stop Loss activation.</summary>
    SL = 3,
    /// <summary>The order is placed as a result of Take Profit activation.</summary>
    TP = 4,
    /// <summary>The order is placed when the client reached the Stop-Out level.</summary>
    SO = 5,
    /// <summary>The order is placed when reopening a position for charging swaps.</summary>
    Rollover = 6,
    /// <summary>The order is placed from an external trading system. As opposed to ORDER_REASON_EXTERNAL_SERVICE, commission is charged in this type of order.</summary>
    ExternalClient = 7,
    /// <summary>The order is placed to charge the variation margin.</summary>
    VMargin = 8,
    /// <summary>The order is placed by a MetaTrader 5 gateway connected to the platform.</summary>
    Gateway = 9,
    /// <summary>The order is placed as a result of copying a trading signal according to the subscription in the client terminal.</summary>
    Signal = 10,
    /// <summary>Forced closing of a position due to a futures/option delivery date coming into effect.</summary>
    Settlement = 11,
    /// <summary>The order is placed as a result of relocating a position with a calculated price to a new symbol with the same underlying asset.</summary>
    Transfer = 12,
    /// <summary>The order is placed while synchronizing a trading account with an external system.</summary>
    Sync = 13,
    /// <summary>Order placed from an external trading system for technical reasons (for example, to correct the trade state of a client). Commission is not charged on this type of orders.</summary>
    ExternalService = 14,
    /// <summary>The order is created while importing clients' trading operations from the MetaTrader 4 server.</summary>
    Migration = 15,
    /// <summary>The order is placed via the MetaTrader 5 mobile terminal for Android or iPhone.</summary>
    Mobile = 16,
    /// <summary>The order is placed via the web terminal.</summary>
    Web = 17,
    /// <summary>The order is placed as a result of a symbol split.</summary>
    Split = 18,
    /// <summary>The order is created as a result of a corporate action, such as consolidating or renaming securities, transferring a client to a different account, etc.</summary>
    CorporateAction = 19
}

#endregion

/// <summary>
/// POCO model for MT5 Order, suitable for serialization and remote storage.
/// </summary>
public class OrderModel
{
    // Identification
    /// <summary>Ticket of an order.</summary>
    public ulong Order { get; set; }
    /// <summary>Order ID in external trading systems.</summary>
    public string ExternalID { get; set; } = string.Empty;
    /// <summary>Login of the client, to whom the order belongs.</summary>
    public ulong Login { get; set; }
    /// <summary>Login of a dealer, who has processed an order.</summary>
    public ulong Dealer { get; set; }
    /// <summary>Symbol, for which an order is placed.</summary>
    public string Symbol { get; set; } = string.Empty;

    // Digits and Precision
    /// <summary>Number of decimal places in the price of an order.</summary>
    public uint Digits { get; set; }
    /// <summary>Number of decimal places the deposit currency of a client who has placed the order.</summary>
    public uint DigitsCurrency { get; set; }
    /// <summary>Contract size of the symbol, for which an order is placed.</summary>
    public double ContractSize { get; set; }

    // State and Reason
    /// <summary>Order state.</summary>
    public OrderState State { get; set; }
    /// <summary>Reason for order placement.</summary>
    public OrderReason Reason { get; set; }

    // Times (Unix timestamps in seconds)
    /// <summary>Time of an order placement.</summary>
    public long TimeSetup { get; set; }
    /// <summary>Order expiration time.</summary>
    public long TimeExpiration { get; set; }
    /// <summary>Time of an order execution or cancellation.</summary>
    public long TimeDone { get; set; }

    // Times (Unix timestamps in milliseconds)
    /// <summary>Time of an order placement in milliseconds.</summary>
    public long TimeSetupMsc { get; set; }
    /// <summary>Time of an order execution or cancellation in milliseconds.</summary>
    public long TimeDoneMsc { get; set; }

    // Order Details
    /// <summary>Order type.</summary>
    public OrderType Type { get; set; }
    /// <summary>Order filling policy.</summary>
    public OrderFilling TypeFill { get; set; }
    /// <summary>Order lifetime.</summary>
    public OrderTime TypeTime { get; set; }

    // Prices
    /// <summary>Price specified in an order.</summary>
    public double PriceOrder { get; set; }
    /// <summary>Stop-Limit order trigger price.</summary>
    public double PriceTrigger { get; set; }
    /// <summary>Current price of the symbol, for which an order is placed.</summary>
    public double PriceCurrent { get; set; }
    /// <summary>Stop Loss level of an order.</summary>
    public double PriceSL { get; set; }
    /// <summary>Take Profit level of an order.</summary>
    public double PriceTP { get; set; }

    // Volumes
    /// <summary>Initial volume of an order.</summary>
    public ulong VolumeInitial { get; set; }
    /// <summary>Unfilled volume of an order.</summary>
    public ulong VolumeCurrent { get; set; }
    /// <summary>Initial volume of an order with an extended accuracy.</summary>
    public ulong VolumeInitialExt { get; set; }
    /// <summary>Unfilled volume of an order with an extended accuracy.</summary>
    public ulong VolumeCurrentExt { get; set; }

    // IDs
    /// <summary>ID of the Expert Advisor that has placed an order.</summary>
    public ulong ExpertID { get; set; }
    /// <summary>Position ID (ticket) for an order.</summary>
    public ulong PositionID { get; set; }
    /// <summary>Opposite position ID (ticket) for an order.</summary>
    public ulong PositionByID { get; set; }

    // Activation
    /// <summary>Order activation mode.</summary>
    public OrderActivation ActivationMode { get; set; }
    /// <summary>Order activation time.</summary>
    public long ActivationTime { get; set; }
    /// <summary>Order activation price.</summary>
    public double ActivationPrice { get; set; }
    /// <summary>Order activation flags.</summary>
    public TradeActivationFlags ActivationFlags { get; set; }

    // Others
    /// <summary>Comment to an order.</summary>
    public string Comment { get; set; } = string.Empty;
    /// <summary>Exchange rate of the margin currency of a deal to the client's deposit currency.</summary>
    public double RateMargin { get; set; }
    /// <summary>Order modification flags.</summary>
    public TradeModifyFlags ModificationFlags { get; set; }

    // Custom API Data
    /// <summary>Custom API data.</summary>
    public List<ApiDataModel> ApiData { get; set; } = new();

    public override string ToString() => MT5JsonSerializer.ToReadableJson(this);
}

public static class OrderMapper
{
    public static OrderModel ToModel(this CIMTOrder mtOrder)
    {
        ArgumentNullException.ThrowIfNull(mtOrder);

        var model = new OrderModel
        {
            Order = mtOrder.Order(),
            ExternalID = mtOrder.ExternalID(),
            Login = mtOrder.Login(),
            Dealer = mtOrder.Dealer(),
            Symbol = mtOrder.Symbol(),
            Digits = mtOrder.Digits(),
            DigitsCurrency = mtOrder.DigitsCurrency(),
            ContractSize = mtOrder.ContractSize(),
            State = (OrderState)mtOrder.State(),
            Reason = (OrderReason)mtOrder.Reason(),
            TimeSetup = mtOrder.TimeSetup(),
            TimeExpiration = mtOrder.TimeExpiration(),
            TimeDone = mtOrder.TimeDone(),
            Type = (OrderType)mtOrder.Type(),
            TypeFill = (OrderFilling)mtOrder.TypeFill(),
            TypeTime = (OrderTime)mtOrder.TypeTime(),
            PriceOrder = mtOrder.PriceOrder(),
            PriceTrigger = mtOrder.PriceTrigger(),
            PriceCurrent = mtOrder.PriceCurrent(),
            PriceSL = mtOrder.PriceSL(),
            PriceTP = mtOrder.PriceTP(),
            VolumeInitial = mtOrder.VolumeInitial(),
            VolumeCurrent = mtOrder.VolumeCurrent(),
            ExpertID = mtOrder.ExpertID(),
            PositionID = mtOrder.PositionID(),
            Comment = mtOrder.Comment(),
            ActivationMode = (OrderActivation)mtOrder.ActivationMode(),
            ActivationTime = mtOrder.ActivationTime(),
            ActivationPrice = mtOrder.ActivationPrice(),
            ActivationFlags = (TradeActivationFlags)mtOrder.ActivationFlags(),
            TimeSetupMsc = mtOrder.TimeSetupMsc(),
            TimeDoneMsc = mtOrder.TimeDoneMsc(),
            RateMargin = mtOrder.RateMargin(),
            PositionByID = mtOrder.PositionByID(),
            ModificationFlags = (TradeModifyFlags)mtOrder.ModificationFlags(),
            VolumeInitialExt = mtOrder.VolumeInitialExt(),
            VolumeCurrentExt = mtOrder.VolumeCurrentExt()
        };

        // Export ApiData
        uint pos = 0;
        while (true)
        {
            MTRetCode rc = mtOrder.ApiDataNext(pos, out ushort appId, out byte id, out ulong val);
            if (rc != MTRetCode.MT_RET_OK)
            {
                break;
            }

            model.ApiData.Add(new ApiDataModel
            {
                AppId = appId,
                Id = id,
                Value = val
            });
            pos++;
        }

        return model;
    }
}

