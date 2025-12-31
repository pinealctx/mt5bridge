using MetaQuotes.MT5CommonAPI;

namespace MT5Bridge.Manager.Models.Proto;

/// <summary>
/// Extension methods for converting MT5 SDK Deal to Protobuf DealModel
/// </summary>
public static class DealProtoExtensions
{
    /// <summary>
    /// Convert MT5 SDK Deal to Protobuf DealModel
    /// </summary>
    public static DealModel ToProto(this CIMTDeal mtDeal)
    {
        ArgumentNullException.ThrowIfNull(mtDeal);

        var model = new DealModel
        {
            Deal = mtDeal.Deal(),
            ExternalId = mtDeal.ExternalID(),
            Login = mtDeal.Login(),
            Dealer = mtDeal.Dealer(),
            Order = mtDeal.Order(),
            Symbol = mtDeal.Symbol(),
            Action = (DealAction)mtDeal.Action(),
            Entry = (EntryFlag)mtDeal.Entry(),
            Reason = (DealReason)mtDeal.Reason(),
            Digits = mtDeal.Digits(),
            DigitsCurrency = mtDeal.DigitsCurrency(),
            ContractSize = mtDeal.ContractSize(),
            Time = mtDeal.Time(),
            TimeMsc = mtDeal.TimeMsc(),
            Price = mtDeal.Price(),
            PricePosition = mtDeal.PricePosition(),
            PriceSl = mtDeal.PriceSL(),
            PriceTp = mtDeal.PriceTP(),
            PriceGateway = mtDeal.PriceGateway(),
            Volume = mtDeal.Volume(),
            VolumeClosed = mtDeal.VolumeClosed(),
            VolumeExt = mtDeal.VolumeExt(),
            VolumeClosedExt = mtDeal.VolumeClosedExt(),
            Profit = mtDeal.Profit(),
            ProfitRaw = mtDeal.ProfitRaw(),
            Storage = mtDeal.Storage(),
            Commission = mtDeal.Commission(),
            Fee = mtDeal.Fee(),
            Value = mtDeal.Value(),
            ObsoleteValue = mtDeal.ObsoleteValue(),
            RateProfit = mtDeal.RateProfit(),
            RateMargin = mtDeal.RateMargin(),
            TickValue = mtDeal.TickValue(),
            TickSize = mtDeal.TickSize(),
            MarketBid = mtDeal.MarketBid(),
            MarketAsk = mtDeal.MarketAsk(),
            MarketLast = mtDeal.MarketLast(),
            ExpertId = mtDeal.ExpertID(),
            PositionId = mtDeal.PositionID(),
            Comment = mtDeal.Comment(),
            Gateway = mtDeal.Gateway(),
            Flags = mtDeal.Flags(),
            ModificationFlags = (TradeModifyFlags)mtDeal.ModificationFlags()
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
