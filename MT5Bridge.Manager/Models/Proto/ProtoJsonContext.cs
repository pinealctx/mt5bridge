using System.Text.Json;
using System.Text.Json.Serialization;

namespace MT5Bridge.Manager.Models.Proto;

/// <summary>
/// JSON serialization context for Protobuf models (readable format for logging)
/// </summary>
[JsonSerializable(typeof(DealModel))]
[JsonSerializable(typeof(OrderModel))]
[JsonSerializable(typeof(PositionModel))]
[JsonSerializable(typeof(UserModel))]
[JsonSerializable(typeof(AccountModel))]
[JsonSerializable(typeof(GroupModel))]
[JsonSerializable(typeof(ApiDataModel))]
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    Converters = new[] {
        typeof(JsonStringEnumConverter<TradeActivationFlags>),
        typeof(JsonStringEnumConverter<TradeModifyFlags>),
        typeof(JsonStringEnumConverter<DealAction>),
        typeof(JsonStringEnumConverter<EntryFlag>),
        typeof(JsonStringEnumConverter<DealReason>),
        typeof(JsonStringEnumConverter<OrderType>),
        typeof(JsonStringEnumConverter<OrderFilling>),
        typeof(JsonStringEnumConverter<OrderTime>),
        typeof(JsonStringEnumConverter<OrderState>),
        typeof(JsonStringEnumConverter<OrderActivation>),
        typeof(JsonStringEnumConverter<OrderReason>),
        typeof(JsonStringEnumConverter<PositionAction>),
        typeof(JsonStringEnumConverter<PositionActivation>),
        typeof(JsonStringEnumConverter<PositionReason>)
    })]
public partial class ProtoJsonContext : JsonSerializerContext
{
}

/// <summary>
/// Extension methods for Protobuf model JSON serialization
/// </summary>
public static class ProtoJsonExtensions
{
    /// <summary>
    /// Convert Protobuf model to human-readable JSON (for logging and debugging)
    /// </summary>
    public static string ToReadableJson(this DealModel deal)
    {
        return JsonSerializer.Serialize(deal, ProtoJsonContext.Default.DealModel);
    }

    /// <summary>
    /// Convert Protobuf model to human-readable JSON (for logging and debugging)
    /// </summary>
    public static string ToReadableJson(this OrderModel order)
    {
        return JsonSerializer.Serialize(order, ProtoJsonContext.Default.OrderModel);
    }

    /// <summary>
    /// Convert Protobuf model to human-readable JSON (for logging and debugging)
    /// </summary>
    public static string ToReadableJson(this PositionModel position)
    {
        return JsonSerializer.Serialize(position, ProtoJsonContext.Default.PositionModel);
    }

    /// <summary>
    /// Convert Protobuf model to human-readable JSON (for logging and debugging)
    /// </summary>
    public static string ToReadableJson(this UserModel user)
    {
        return JsonSerializer.Serialize(user, ProtoJsonContext.Default.UserModel);
    }

    /// <summary>
    /// Convert Protobuf model to human-readable JSON (for logging and debugging)
    /// </summary>
    public static string ToReadableJson(this AccountModel account)
    {
        return JsonSerializer.Serialize(account, ProtoJsonContext.Default.AccountModel);
    }

    /// <summary>
    /// Convert Protobuf model to human-readable JSON (for logging and debugging)
    /// </summary>
    public static string ToReadableJson(this GroupModel group)
    {
        return JsonSerializer.Serialize(group, ProtoJsonContext.Default.GroupModel);
    }
}
