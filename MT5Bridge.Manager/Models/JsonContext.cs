using System.Text.Json;
using System.Text.Json.Serialization;

namespace MT5Bridge.Manager.Models;

/// <summary>
/// Source-generated JSON context for high-performance serialization.
/// </summary>
[JsonSerializable(typeof(DealModel))]
[JsonSerializable(typeof(OrderModel))]
[JsonSerializable(typeof(PositionModel))]
[JsonSerializable(typeof(UserModel))]
[JsonSerializable(typeof(AccountModel))]
[JsonSerializable(typeof(GroupModel))]
[JsonSerializable(typeof(ApiDataModel))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    GenerationMode = JsonSourceGenerationMode.Default)]
public partial class FastJsonContext : JsonSerializerContext
{
}

/// <summary>
/// Source-generated JSON context for human-readable logging (Enums as strings).
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
public partial class ReadableJsonContext : JsonSerializerContext
{
}

/// <summary>
/// Helper class to provide easy access to different serialization profiles.
/// </summary>
public static class MT5JsonSerializer
{
    /// <summary>
    /// Fast serialization for network or database (Enums as numbers, no indentation).
    /// </summary>
    public static string ToFastJson<T>(T value)
    {
        return JsonSerializer.Serialize(value, typeof(T), FastJsonContext.Default);
    }

    /// <summary>
    /// Readable serialization for logging (Enums as strings, indented).
    /// </summary>
    public static string ToReadableJson<T>(T value)
    {
        return JsonSerializer.Serialize(value, typeof(T), ReadableJsonContext.Default);
    }

    /// <summary>
    /// Deserializes JSON using the fast context.
    /// </summary>
    public static T? FromJson<T>(string json)
    {
        return (T?)JsonSerializer.Deserialize(json, typeof(T), FastJsonContext.Default);
    }
}
