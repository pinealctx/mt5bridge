using MetaQuotes.MT5CommonAPI;

namespace MT5Bridge.Manager.Models.Proto;

/// <summary>
/// Extension methods for converting MT5 SDK Position to Protobuf PositionModel
/// </summary>
public static class PositionProtoExtensions
{
    /// <summary>
    /// Convert MT5 SDK Position to Protobuf PositionModel
    /// </summary>
    public static PositionModel ToProto(this CIMTPosition mtPos)
    {
        ArgumentNullException.ThrowIfNull(mtPos);

        var model = new PositionModel
        {
            Position = mtPos.Position(),
            ExternalId = mtPos.ExternalID(),
            Login = mtPos.Login(),
            Dealer = mtPos.Dealer(),
            Symbol = mtPos.Symbol(),
            Action = (PositionAction)mtPos.Action(),
            Reason = (PositionReason)mtPos.Reason(),
            Digits = mtPos.Digits(),
            DigitsCurrency = mtPos.DigitsCurrency(),
            ContractSize = mtPos.ContractSize(),
            TimeCreate = mtPos.TimeCreate(),
            TimeUpdate = mtPos.TimeUpdate(),
            TimeCreateMsc = mtPos.TimeCreateMsc(),
            TimeUpdateMsc = mtPos.TimeUpdateMsc(),
            PriceOpen = mtPos.PriceOpen(),
            PriceCurrent = mtPos.PriceCurrent(),
            PriceSl = mtPos.PriceSL(),
            PriceTp = mtPos.PriceTP(),
            Volume = mtPos.Volume(),
            VolumeExt = mtPos.VolumeExt(),
            Profit = mtPos.Profit(),
            Storage = mtPos.Storage(),
            ObsoleteValue = mtPos.ObsoleteValue(),
            RateProfit = mtPos.RateProfit(),
            RateMargin = mtPos.RateMargin(),
            ExpertId = mtPos.ExpertID(),
            ExpertPositionId = mtPos.ExpertPositionID(),
            Comment = mtPos.Comment(),
            ActivationMode = (PositionActivation)mtPos.ActivationMode(),
            ActivationTime = mtPos.ActivationTime(),
            ActivationPrice = mtPos.ActivationPrice(),
            ActivationFlags = (TradeActivationFlags)mtPos.ActivationFlags(),
            ModificationFlags = (TradeModifyFlags)mtPos.ModificationFlags()
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
