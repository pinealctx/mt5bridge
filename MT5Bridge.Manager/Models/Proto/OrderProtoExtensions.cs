using MetaQuotes.MT5CommonAPI;
using MetaQuotes.MT5ManagerAPI;

namespace MT5Bridge.Manager.Models.Proto;

/// <summary>
/// Extension methods for converting MT5 SDK Order to Protobuf OrderModel
/// </summary>
public static class OrderProtoExtensions
{
    /// <summary>
    /// Convert MT5 SDK Order to Protobuf OrderModel
    /// </summary>
    public static OrderModel ToProto(this CIMTOrder mtOrder)
    {
        ArgumentNullException.ThrowIfNull(mtOrder);

        var model = new OrderModel
        {
            Order = mtOrder.Order(),
            ExternalId = mtOrder.ExternalID(),
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
            TimeSetupMsc = mtOrder.TimeSetupMsc(),
            TimeDoneMsc = mtOrder.TimeDoneMsc(),
            Type = (OrderType)mtOrder.Type(),
            TypeFill = (OrderFilling)mtOrder.TypeFill(),
            TypeTime = (OrderTime)mtOrder.TypeTime(),
            PriceOrder = mtOrder.PriceOrder(),
            PriceTrigger = mtOrder.PriceTrigger(),
            PriceCurrent = mtOrder.PriceCurrent(),
            PriceSl = mtOrder.PriceSL(),
            PriceTp = mtOrder.PriceTP(),
            VolumeInitial = mtOrder.VolumeInitial(),
            VolumeCurrent = mtOrder.VolumeCurrent(),
            VolumeInitialExt = mtOrder.VolumeInitialExt(),
            VolumeCurrentExt = mtOrder.VolumeCurrentExt(),
            ExpertId = mtOrder.ExpertID(),
            PositionId = mtOrder.PositionID(),
            PositionById = mtOrder.PositionByID(),
            ActivationMode = (OrderActivation)mtOrder.ActivationMode(),
            ActivationTime = mtOrder.ActivationTime(),
            ActivationPrice = mtOrder.ActivationPrice(),
            ActivationFlags = (TradeActivationFlags)mtOrder.ActivationFlags(),
            Comment = mtOrder.Comment(),
            RateMargin = mtOrder.RateMargin(),
            ModificationFlags = (TradeModifyFlags)mtOrder.ModificationFlags()
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
