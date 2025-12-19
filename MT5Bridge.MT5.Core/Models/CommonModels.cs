using System;

namespace MT5Bridge.MT5.Core.Models;

/// <summary>
/// Order/Position activation flags.
/// </summary>
[Flags]
public enum TradeActivationFlags : uint
{
    /// <summary>No flags.</summary>
    None = 0,
    /// <summary>Do not handle reaching of the Limit level. Only used for orders.</summary>
    NoLimit = 1,
    /// <summary>Do not handle the reaching of the stop level. Only used for orders.</summary>
    NoStop = 2,
    /// <summary>Do not handle reaching of the Stop-Limit level. Only used for orders.</summary>
    NoSLimit = 4,
    /// <summary>Do not handle activation upon Stop Loss. Only used for positions.</summary>
    NoSL = 8,
    /// <summary>Do not handle activation upon Take Profit. Only used for positions.</summary>
    NoTP = 16,
    /// <summary>Do not handle activation upon Stop-Out. Used for positions and orders.</summary>
    NoSO = 32,
    /// <summary>Do not handle order cancellation upon expiration. Only used for orders.</summary>
    NoExpiration = 64,
    /// <summary>All flags are set.</summary>
    All = 127
}

/// <summary>
/// Deal/Order/Position modification flags.
/// </summary>
[Flags]
public enum TradeModifyFlags : uint
{
    /// <summary>No flags.</summary>
    None = 0,
    /// <summary>Modified by an administrator.</summary>
    Admin = 1,
    /// <summary>Open price has been modified by a manager.</summary>
    Manager = 2,
    /// <summary>Modification flags have been inherited from the position.</summary>
    Position = 4,
    /// <summary>Restored.</summary>
    Restore = 8,
    /// <summary>Modified using the administrator interface of the Manager API.</summary>
    ApiAdmin = 16,
    /// <summary>Modified using the manager interface of the Manager API.</summary>
    ApiManager = 32,
    /// <summary>Modified using the Server API.</summary>
    ApiServer = 64,
    /// <summary>Modified using the Gateway API.</summary>
    ApiGateway = 128,
    /// <summary>All flags are set.</summary>
    All = 255
}

/// <summary>
/// Represents a custom data entry attached to an order, deal or position (ApiData)
/// </summary>
public class ApiDataModel
{
    /// <summary>Application ID</summary>
    public ushort AppId { get; set; }
    /// <summary>Parameter ID</summary>
    public byte Id { get; set; }
    /// <summary>Value of the custom parameter</summary>
    public ulong Value { get; set; }
}
