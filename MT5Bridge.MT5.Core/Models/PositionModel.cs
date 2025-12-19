using System;
using System.Collections.Generic;
using MetaQuotes.MT5CommonAPI;

namespace MT5Bridge.MT5.Core.Models;

#region Enums

public enum PositionAction : uint
{
    /// <summary>A Buy position.</summary>
    Buy = 0,
    /// <summary>A Sell position.</summary>
    Sell = 1
}

public enum PositionActivation : uint
{
    /// <summary>No activation.</summary>
    None = 0,
    /// <summary>Activation of the Stop Loss level.</summary>
    SL = 1,
    /// <summary>Activation of the Take Profit level.</summary>
    TP = 2,
    /// <summary>Position cancellation upon Stop Out.</summary>
    StopOut = 3
}

public enum PositionReason : uint
{
    /// <summary>The position is opened by a client manually through the client terminal.</summary>
    Client = 0,
    /// <summary>The position is opened by a client using an Expert Advisor.</summary>
    Expert = 1,
    /// <summary>The position is opened by a dealer through the manager terminal.</summary>
    Dealer = 2,
    /// <summary>The position is opened as a result of Stop Loss activation.</summary>
    SL = 3,
    /// <summary>The position is opened as a result of Take Profit activation.</summary>
    TP = 4,
    /// <summary>The position is opened when the client reached the Stop-Out level.</summary>
    SO = 5,
    /// <summary>The position is opened when reopening a position for charging swaps.</summary>
    Rollover = 6,
    /// <summary>The position is opened from an external trading system. As opposed to POSITION_REASON_EXTERNAL_SERVICE, commission is charged in this type of position.</summary>
    ExternalClient = 7,
    /// <summary>The position is opened to charge the variation margin.</summary>
    VMargin = 8,
    /// <summary>The position is opened by a MetaTrader 5 gateway connected to the platform.</summary>
    Gateway = 9,
    /// <summary>The position is opened as a result of copying a trading signal according to the subscription in the client terminal.</summary>
    Signal = 10,
    /// <summary>Forced closing of a position due to a futures/option delivery date coming into effect.</summary>
    Settlement = 11,
    /// <summary>The position is opened as a result of relocating a position with a calculated price to a new symbol with the same underlying asset.</summary>
    Transfer = 12,
    /// <summary>The position is opened while synchronizing a trading account with an external system.</summary>
    Sync = 13,
    /// <summary>Position opened from an external trading system for technical reasons (for example, to correct the trade state of a client). Commission is not charged on this type of positions.</summary>
    ExternalService = 14,
    /// <summary>The position is created while importing clients' trading operations from the MetaTrader 4 server.</summary>
    Migration = 15,
    /// <summary>The position is opened via the MetaTrader 5 mobile terminal for Android or iPhone.</summary>
    Mobile = 16,
    /// <summary>The position is opened via the web terminal.</summary>
    Web = 17,
    /// <summary>The position is opened as a result of a symbol split.</summary>
    Split = 18,
    /// <summary>The position is created as a result of a corporate action, such as consolidating or renaming securities, transferring a client to a different account, etc.</summary>
    CorporateAction = 19
}

#endregion

/// <summary>
/// POCO model for MT5 Position, suitable for serialization and remote storage.
/// </summary>
public class PositionModel
{
    // Identification
    /// <summary>Ticket of a position.</summary>
    public ulong Position { get; set; }
    /// <summary>Position ID in external trading systems.</summary>
    public string ExternalID { get; set; } = string.Empty;
    /// <summary>Login of the client, to whom the position belongs.</summary>
    public ulong Login { get; set; }
    /// <summary>Login of a dealer, who has processed a position.</summary>
    public ulong Dealer { get; set; }
    /// <summary>Symbol, for which a position is opened.</summary>
    public string Symbol { get; set; } = string.Empty;

    // Action and State
    /// <summary>Type of action performed with a position.</summary>
    public PositionAction Action { get; set; }
    /// <summary>Reason for position opening.</summary>
    public PositionReason Reason { get; set; }

    // Digits and Precision
    /// <summary>Number of decimal places in the price of a position.</summary>
    public uint Digits { get; set; }
    /// <summary>Number of decimal places the deposit currency of a client who has opened the position.</summary>
    public uint DigitsCurrency { get; set; }
    /// <summary>Contract size of the symbol, for which a position is opened.</summary>
    public double ContractSize { get; set; }

    // Times
    /// <summary>Time of a position opening.</summary>
    public long TimeCreate { get; set; }
    /// <summary>Time of a position change.</summary>
    public long TimeUpdate { get; set; }
    /// <summary>Time of a position opening in milliseconds.</summary>
    public long TimeCreateMsc { get; set; }
    /// <summary>Time of a position change in milliseconds.</summary>
    public long TimeUpdateMsc { get; set; }

    // Prices
    /// <summary>Price of a position opening.</summary>
    public double PriceOpen { get; set; }
    /// <summary>Current price of the symbol, for which a position is opened.</summary>
    public double PriceCurrent { get; set; }
    /// <summary>Stop Loss level of a position.</summary>
    public double PriceSL { get; set; }
    /// <summary>Take Profit level of a position.</summary>
    public double PriceTP { get; set; }

    // Volumes
    /// <summary>Position volume.</summary>
    public ulong Volume { get; set; }
    /// <summary>Position volume with an extended accuracy.</summary>
    public ulong VolumeExt { get; set; }

    // Financials
    /// <summary>Value of the profit from the position.</summary>
    public double Profit { get; set; }
    /// <summary>Swap size for a position.</summary>
    public double Storage { get; set; }
    /// <summary>Obsolete value.</summary>
    public double ObsoleteValue { get; set; }
    /// <summary>Exchange rate of the profit currency of a position to the deposit currency of a client group.</summary>
    public double RateProfit { get; set; }
    /// <summary>Exchange rate of the margin currency of a position to the client's deposit currency.</summary>
    public double RateMargin { get; set; }

    // IDs
    /// <summary>ID of the Expert Advisor that has opened a position.</summary>
    public ulong ExpertID { get; set; }
    /// <summary>Position ID in the Expert Advisor.</summary>
    public ulong ExpertPositionID { get; set; }
    /// <summary>Comment to a position.</summary>
    public string Comment { get; set; } = string.Empty;

    // Activation
    /// <summary>Position activation mode.</summary>
    public PositionActivation ActivationMode { get; set; }
    /// <summary>Position activation time.</summary>
    public long ActivationTime { get; set; }
    /// <summary>Position activation price.</summary>
    public double ActivationPrice { get; set; }
    /// <summary>Position activation flags.</summary>
    public TradeActivationFlags ActivationFlags { get; set; }

    // Flags
    /// <summary>Position modification flags.</summary>
    public TradeModifyFlags ModificationFlags { get; set; }

    // Custom API Data
    /// <summary>Custom API data.</summary>
    public List<ApiDataModel> ApiData { get; set; } = new();

    public override string ToString() => MT5JsonSerializer.ToReadableJson(this);
}

public static class PositionMapper
{
    public static PositionModel ToModel(this CIMTPosition mtPos)
    {
        ArgumentNullException.ThrowIfNull(mtPos);

        var model = new PositionModel
        {
            Login = mtPos.Login(),
            Symbol = mtPos.Symbol(),
            Action = (PositionAction)mtPos.Action(),
            Digits = mtPos.Digits(),
            DigitsCurrency = mtPos.DigitsCurrency(),
            ContractSize = mtPos.ContractSize(),
            TimeCreate = mtPos.TimeCreate(),
            TimeUpdate = mtPos.TimeUpdate(),
            PriceOpen = mtPos.PriceOpen(),
            PriceCurrent = mtPos.PriceCurrent(),
            PriceSL = mtPos.PriceSL(),
            PriceTP = mtPos.PriceTP(),
            Volume = mtPos.Volume(),
            Profit = mtPos.Profit(),
            Storage = mtPos.Storage(),
            ObsoleteValue = mtPos.ObsoleteValue(),
            RateProfit = mtPos.RateProfit(),
            RateMargin = mtPos.RateMargin(),
            ExpertID = mtPos.ExpertID(),
            ExpertPositionID = mtPos.ExpertPositionID(),
            Comment = mtPos.Comment(),
            ActivationMode = (PositionActivation)mtPos.ActivationMode(),
            ActivationTime = mtPos.ActivationTime(),
            ActivationPrice = mtPos.ActivationPrice(),
            ActivationFlags = (TradeActivationFlags)mtPos.ActivationFlags(),

            TimeCreateMsc = mtPos.TimeCreateMsc(),
            TimeUpdateMsc = mtPos.TimeUpdateMsc(),
            Dealer = mtPos.Dealer(),
            Position = mtPos.Position(),
            ExternalID = mtPos.ExternalID(),
            ModificationFlags = (TradeModifyFlags)mtPos.ModificationFlags(),
            Reason = (PositionReason)mtPos.Reason(),
            VolumeExt = mtPos.VolumeExt()
        };

        // Export ApiData
        uint pos = 0;
        while (true)
        {
            MTRetCode rc = mtPos.ApiDataNext(pos, out ushort appId, out byte id, out ulong val);
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
