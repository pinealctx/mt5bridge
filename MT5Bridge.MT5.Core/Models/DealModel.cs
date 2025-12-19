using System;
using System.Collections.Generic;
using MetaQuotes.MT5CommonAPI;

namespace MT5Bridge.MT5.Core.Models;

#region Enums

public enum DealAction : uint
{
    /// <summary>A Buy deal.</summary>
    Buy = 0,
    /// <summary>A Sell deal.</summary>
    Sell = 1,
    /// <summary>A balance operation.</summary>
    Balance = 2,
    /// <summary>Credit operation.</summary>
    Credit = 3,
    /// <summary>Additional charges/withdrawals.</summary>
    Charge = 4,
    /// <summary>Correcting operations.</summary>
    Correction = 5,
    /// <summary>Bonuses. Operations of this type affect the credit assets of a client.</summary>
    Bonus = 6,
    /// <summary>Commission.</summary>
    Commission = 7,
    /// <summary>Daily commission.</summary>
    CommissionDaily = 8,
    /// <summary>Monthly commission.</summary>
    CommissionMonthly = 9,
    /// <summary>Daily agent commission.</summary>
    AgentDaily = 10,
    /// <summary>Monthly agent commission.</summary>
    AgentMonthly = 11,
    /// <summary>Accrual of annual interest.</summary>
    InterestRate = 12,
    /// <summary>A canceled Buy deal.</summary>
    BuyCanceled = 13,
    /// <summary>A canceled Sell deal.</summary>
    SellCanceled = 14,
    /// <summary>Dividend operations.</summary>
    Dividend = 15,
    /// <summary>Franked (non-taxable) dividend operations (tax is paid by a company, not a client).</summary>
    DividendFranked = 16,
    /// <summary>Charging a tax.</summary>
    Tax = 17,
    /// <summary>Charging an agent commission. Used during an instant commission charge to an agent (each time the agent's client performs a deal).</summary>
    Agent = 18,
    /// <summary>An operation connected with the compensation of a negative account after the Stop Out event.</summary>
    SoCompensation = 19,
    /// <summary>Withdrawing credit funds after a negative balance compensation operation.</summary>
    SoCompensationCredit = 20
}

public enum EntryFlag : uint
{
    /// <summary>Entering the market or adding the volume.</summary>
    In = 0,
    /// <summary>Exit from the market or partial closure.</summary>
    Out = 1,
    /// <summary>The deal that closed an existing position and opened a new one in the opposite direction. It is only used with the netting position accounting system.</summary>
    InOut = 2,
    /// <summary>Close by — a simultaneous closure of two opposite positions of the same financial instrument. This operation type is only used in the hedging mode.</summary>
    OutBy = 3
}

public enum DealReason : uint
{
    /// <summary>The deal is conducted by a client manually through the client terminal.</summary>
    Client = 0,
    /// <summary>The deal is conducted by a client using an Expert Advisor.</summary>
    Expert = 1,
    /// <summary>The deal is conducted by a dealer through the manager terminal.</summary>
    Dealer = 2,
    /// <summary>The deal is conducted as a result of Stop Loss activation.</summary>
    SL = 3,
    /// <summary>The deal is conducted as a result of Take Profit activation.</summary>
    TP = 4,
    /// <summary>The deal is conducted when the client reached the Stop-Out level.</summary>
    SO = 5,
    /// <summary>The deal is conducted when reopening a position for charging swaps.</summary>
    Rollover = 6,
    /// <summary>The deal is conducted from an external trading system. As opposed to DEAL_REASON_EXTERNAL_SERVICE, commission is charged in this type of deal.</summary>
    ExternalClient = 7,
    /// <summary>The deal is conducted to charge the variation margin.</summary>
    VMargin = 8,
    /// <summary>The deal is conducted by a MetaTrader 5 gateway connected to the platform.</summary>
    Gateway = 9,
    /// <summary>The deal is conducted as a result of copying a trading signal according to the subscription in the client terminal.</summary>
    Signal = 10,
    /// <summary>Forced closing of a position due to a futures/option delivery date coming into effect.</summary>
    Settlement = 11,
    /// <summary>The deal is conducted as a result of relocating a position with a calculated price to a new symbol with the same underlying asset.</summary>
    Transfer = 12,
    /// <summary>The deal is conducted while synchronizing a trading account with an external system.</summary>
    Sync = 13,
    /// <summary>Deal performed from an external trading system for technical reasons (for example, to correct the trade state of a client). Commission is not charged on this type of deals.</summary>
    ExternalService = 14,
    /// <summary>The deal is created while importing clients' trading operations from the MetaTrader 4 server.</summary>
    Migration = 15,
    /// <summary>The deal is conducted via the MetaTrader 5 mobile terminal for Android or iPhone.</summary>
    Mobile = 16,
    /// <summary>The deal is conducted via the web terminal.</summary>
    Web = 17,
    /// <summary>The deal is conducted as a result of a symbol split.</summary>
    Split = 18,
    /// <summary>The deal is created as a result of a corporate action, such as consolidating or renaming securities, transferring a client to a different account, etc.</summary>
    CorporateAction = 19
}

#endregion

/// <summary>
/// POCO model for MT5 Deal, suitable for serialization and remote storage.
/// </summary>
public class DealModel
{
    // Identification
    /// <summary>Ticket of a deal.</summary>
    public ulong Deal { get; set; }
    /// <summary>Deal ID in external trading systems.</summary>
    public string ExternalID { get; set; } = string.Empty;
    /// <summary>Login of the client, to whom the deal belongs.</summary>
    public ulong Login { get; set; }
    /// <summary>Login of a dealer, who has processed a deal.</summary>
    public ulong Dealer { get; set; }
    /// <summary>Ticket of the order, as a result of which a deal was executed.</summary>
    public ulong Order { get; set; }
    /// <summary>Symbol, for which a deal is executed.</summary>
    public string Symbol { get; set; } = string.Empty;

    // Action and Entry
    /// <summary>Type of action performed with a deal.</summary>
    public DealAction Action { get; set; }
    /// <summary>Deal direction.</summary>
    public EntryFlag Entry { get; set; }
    /// <summary>Reason for deal execution.</summary>
    public DealReason Reason { get; set; }

    // Digits and Precision
    /// <summary>Number of decimal places in the price of a deal.</summary>
    public uint Digits { get; set; }
    /// <summary>Number of decimal places the deposit currency of a client who has executed the deal.</summary>
    public uint DigitsCurrency { get; set; }
    /// <summary>Contract size of the symbol, for which a deal is executed.</summary>
    public double ContractSize { get; set; }

    // Times
    /// <summary>Time of a deal.</summary>
    public long Time { get; set; }
    /// <summary>Time of a deal execution in milliseconds.</summary>
    public long TimeMsc { get; set; }

    // Prices
    /// <summary>Price of a deal.</summary>
    public double Price { get; set; }
    /// <summary>Price of the position closed by the deal.</summary>
    public double PricePosition { get; set; }
    /// <summary>Stop Loss level of a deal.</summary>
    public double PriceSL { get; set; }
    /// <summary>Take Profit level of a deal.</summary>
    public double PriceTP { get; set; }
    /// <summary>Actual price of a deal executed via a gateway in an external trading system, not taking into account the gateway price transformation settings.</summary>
    public double PriceGateway { get; set; }

    // Volumes
    /// <summary>Deal volume.</summary>
    public ulong Volume { get; set; }
    /// <summary>Position volume that was closed by the deal.</summary>
    public ulong VolumeClosed { get; set; }
    /// <summary>Deal volume with an extended accuracy.</summary>
    public ulong VolumeExt { get; set; }
    /// <summary>Extended accuracy volume of a position that was closed by this deal.</summary>
    public ulong VolumeClosedExt { get; set; }

    // Financials
    /// <summary>Value of the profit from the deal execution.</summary>
    public double Profit { get; set; }
    /// <summary>Value of profit/loss resulting from the deal execution. The profit/loss is expressed in the profit currency of the symbol, for which a deal is executed.</summary>
    public double ProfitRaw { get; set; }
    /// <summary>Swap size for a deal.</summary>
    public double Storage { get; set; }
    /// <summary>Amount of commission charged for a deal.</summary>
    public double Commission { get; set; }
    /// <summary>Fee amount per deal.</summary>
    public double Fee { get; set; }
    /// <summary>Deal value in client deposit currency.</summary>
    public double Value { get; set; }
    /// <summary>Obsolete value.</summary>
    public double ObsoleteValue { get; set; }
    /// <summary>Exchange rate of the profit currency of a deal to the deposit currency of a client group.</summary>
    public double RateProfit { get; set; }
    /// <summary>Exchange rate of the margin currency of a deal to the client's deposit currency.</summary>
    public double RateMargin { get; set; }
    /// <summary>Tick price for a deal.</summary>
    public double TickValue { get; set; }
    /// <summary>Tick size for a deal.</summary>
    public double TickSize { get; set; }

    // Market Prices
    /// <summary>Market Bid price as at the time of deal execution by the server.</summary>
    public double MarketBid { get; set; }
    /// <summary>Market Ask price as at the time of deal execution by the server.</summary>
    public double MarketAsk { get; set; }
    /// <summary>Market Last price as at the time of deal execution by the server.</summary>
    public double MarketLast { get; set; }

    // IDs
    /// <summary>ID of the Expert Advisor that has executed a deal.</summary>
    public ulong ExpertID { get; set; }
    /// <summary>Position ID (ticket) for a deal.</summary>
    public ulong PositionID { get; set; }
    /// <summary>Comment to a deal.</summary>
    public string Comment { get; set; } = string.Empty;
    /// <summary>ID of a trade gateway, using which the deal was executed.</summary>
    public string Gateway { get; set; } = string.Empty;

    // Flags
    /// <summary>Common flags of a deal.</summary>
    public ulong Flags { get; set; }
    /// <summary>Deal modification flags.</summary>
    public TradeModifyFlags ModificationFlags { get; set; }

    // Custom API Data
    /// <summary>Custom API data.</summary>
    public List<ApiDataModel> ApiData { get; set; } = new();
}

public static class DealMapper
{
    public static DealModel ToModel(this CIMTDeal mtDeal)
    {
        ArgumentNullException.ThrowIfNull(mtDeal);

        var model = new DealModel
        {
            Deal = mtDeal.Deal(),
            ExternalID = mtDeal.ExternalID(),
            Login = mtDeal.Login(),
            Dealer = mtDeal.Dealer(),
            Order = mtDeal.Order(),
            Action = (DealAction)mtDeal.Action(),
            Entry = (EntryFlag)mtDeal.Entry(),
            Digits = mtDeal.Digits(),
            DigitsCurrency = mtDeal.DigitsCurrency(),
            ContractSize = mtDeal.ContractSize(),
            Time = mtDeal.Time(),

            Symbol = mtDeal.Symbol(),
            Price = mtDeal.Price(),
            Volume = mtDeal.Volume(),
            Profit = mtDeal.Profit(),
            Storage = mtDeal.Storage(),
            Commission = mtDeal.Commission(),
            ObsoleteValue = mtDeal.ObsoleteValue(),
            RateProfit = mtDeal.RateProfit(),
            RateMargin = mtDeal.RateMargin(),
            ExpertID = mtDeal.ExpertID(),
            PositionID = mtDeal.PositionID(),
            Comment = mtDeal.Comment(),
            ProfitRaw = mtDeal.ProfitRaw(),
            PricePosition = mtDeal.PricePosition(),
            VolumeClosed = mtDeal.VolumeClosed(),
            TickValue = mtDeal.TickValue(),
            TickSize = mtDeal.TickSize(),
            Flags = mtDeal.Flags(),
            TimeMsc = mtDeal.TimeMsc(),
            Reason = (DealReason)mtDeal.Reason(),
            Gateway = mtDeal.Gateway(),
            PriceGateway = mtDeal.PriceGateway(),
            ModificationFlags = (TradeModifyFlags)mtDeal.ModificationFlags(),
            PriceSL = mtDeal.PriceSL(),
            PriceTP = mtDeal.PriceTP(),
            VolumeExt = mtDeal.VolumeExt(),
            VolumeClosedExt = mtDeal.VolumeClosedExt(),
            Fee = mtDeal.Fee(),
            Value = mtDeal.Value(),
            MarketBid = mtDeal.MarketBid(),
            MarketAsk = mtDeal.MarketAsk(),
            MarketLast = mtDeal.MarketLast()
        };

        // Export ApiData
        uint pos = 0;
        while (true)
        {
            MTRetCode rc = mtDeal.ApiDataNext(pos, out ushort appId, out byte id, out ulong val);
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
