using MT5Bridge.Manager.Models;
using ProtoUser = MT5Bridge.Manager.Models.Proto.UserModel;
using ProtoAccount = MT5Bridge.Manager.Models.Proto.AccountModel;
using ProtoGroup = MT5Bridge.Manager.Models.Proto.GroupModel;

namespace MT5Bridge.Manager.Managers;

/// <summary>
/// MT5 Manager API wrapper interface
/// </summary>
public interface IMT5Manager : IDisposable
{
    /// <summary>
    /// Is connected to MT5 server
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Current connection state
    /// </summary>
    ConnectionState State { get; }

    /// <summary>
    /// Connection settings
    /// </summary>
    MT5ConnectionSettings? Settings { get; }

    /// <summary>
    /// Connection state changed event
    /// </summary>
    event EventHandler<ConnectionStateChangedEventArgs>? ConnectionStateChanged;

    #region Typed Event Handlers

    /// <summary>
    /// Register a deal event handler (POCO)
    /// </summary>
    void RegisterDealHandler(
        Action<DealModel>? onAdd = null,
        Action<DealModel>? onUpdate = null,
        Action<DealModel>? onDelete = null);

    /// <summary>
    /// Register a deal event handler (Protobuf)
    /// </summary>
    void RegisterDealProtoHandler(
        Action<MT5Bridge.Manager.Models.Proto.DealModel>? onAdd = null,
        Action<MT5Bridge.Manager.Models.Proto.DealModel>? onUpdate = null,
        Action<MT5Bridge.Manager.Models.Proto.DealModel>? onDelete = null);

    /// <summary>
    /// Register an order event handler (POCO)
    /// </summary>
    void RegisterOrderHandler(
        Action<OrderModel>? onAdd = null,
        Action<OrderModel>? onUpdate = null,
        Action<OrderModel>? onDelete = null);

    /// <summary>
    /// Register an order event handler (Protobuf)
    /// </summary>
    void RegisterOrderProtoHandler(
        Action<MT5Bridge.Manager.Models.Proto.OrderModel>? onAdd = null,
        Action<MT5Bridge.Manager.Models.Proto.OrderModel>? onUpdate = null,
        Action<MT5Bridge.Manager.Models.Proto.OrderModel>? onDelete = null);

    /// <summary>
    /// Register a position event handler (POCO)
    /// </summary>
    void RegisterPositionHandler(
        Action<PositionModel>? onAdd = null,
        Action<PositionModel>? onUpdate = null,
        Action<PositionModel>? onDelete = null);

    /// <summary>
    /// Register a position event handler (Protobuf)
    /// </summary>
    void RegisterPositionProtoHandler(
        Action<MT5Bridge.Manager.Models.Proto.PositionModel>? onAdd = null,
        Action<MT5Bridge.Manager.Models.Proto.PositionModel>? onUpdate = null,
        Action<MT5Bridge.Manager.Models.Proto.PositionModel>? onDelete = null);

    #endregion

    #region Connection Management

    /// <summary>
    /// Connect to MT5 server
    /// </summary>
    Task<MT5Result> ConnectAsync(MT5ConnectionSettings settings, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnect from MT5 server
    /// </summary>
    Task DisconnectAsync();
    /// <summary>
    /// Disconnect from MT5 server (sync)
    /// </summary>
    void Disconnect();
    #endregion

    #region Group Operations

    /// <summary>
    /// Get user groups (POCO)
    /// </summary>
    Task<MT5Result<GroupModel[]>> GetGroupsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user groups (Protobuf)
    /// </summary>
    Task<MT5Result<ProtoGroup[]>> GetGroupsProtoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get specific group by name (POCO)
    /// </summary>
    Task<MT5Result<GroupModel>> GetGroupAsync(string groupName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get specific group by name (Protobuf)
    /// </summary>
    Task<MT5Result<ProtoGroup>> GetGroupProtoAsync(string groupName, CancellationToken cancellationToken = default);

    #endregion

    #region User Operations

    /// <summary>
    /// Get users by group with pagination (POCO)
    /// </summary>
    /// <param name="groupMask">Group mask (e.g., "demo*", "*" for all). If null, gets all users.</param>
    /// <param name="offset">Number of users to skip</param>
    /// <param name="limit">Maximum number of users to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Array of users matching the group mask</returns>
    Task<MT5Result<UserModel[]>> GetUsersAsync(string? groupMask = null, int offset = 0, int limit = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get users by group with pagination (Protobuf)
    /// </summary>
    /// <param name="groupMask">Group mask (e.g., "demo*", "*" for all). If null, gets all users.</param>
    /// <param name="offset">Number of users to skip</param>
    /// <param name="limit">Maximum number of users to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Array of users matching the group mask</returns>
    Task<MT5Result<ProtoUser[]>> GetUsersProtoAsync(string? groupMask = null, int offset = 0, int limit = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user information (POCO)
    /// </summary>
    Task<MT5Result<UserModel>> GetUserAsync(ulong login, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user information (Protobuf)
    /// </summary>
    Task<MT5Result<ProtoUser>> GetUserProtoAsync(ulong login, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create new user (POCO)
    /// </summary>
    Task<MT5Result<UserModel>> CreateUserAsync(UserModel user, string masterPassword, string investorPassword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create new user (Protobuf)
    /// </summary>
    Task<MT5Result<ProtoUser>> CreateUserProtoAsync(ProtoUser user, string masterPassword, string investorPassword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update user information (POCO)
    /// </summary>
    Task<MT5Result<UserModel>> UpdateUserAsync(UserModel user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update user information (Protobuf)
    /// </summary>
    Task<MT5Result<ProtoUser>> UpdateUserProtoAsync(ProtoUser user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete user
    /// </summary>
    Task<MT5Result> DeleteUserAsync(ulong login, CancellationToken cancellationToken = default);

    #endregion

    #region Account Operations

    /// <summary>
    /// Get user account information (POCO)
    /// </summary>
    Task<MT5Result<AccountModel>> GetAccountAsync(ulong login, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user account information (Protobuf)
    /// </summary>
    Task<MT5Result<ProtoAccount>> GetAccountProtoAsync(ulong login, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deposit to user account
    /// </summary>
    Task<MT5Result> DepositAsync(ulong login, decimal amount, string comment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Withdraw from user account
    /// </summary>
    Task<MT5Result> WithdrawAsync(ulong login, decimal amount, string comment, CancellationToken cancellationToken = default);

    #endregion

    #region Deal Operations

    /// <summary>
    /// Get user deals (POCO)
    /// </summary>
    Task<MT5Result<DealModel[]>> GetDealsAsync(ulong login, DateTime from, DateTime to, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user deals (POCO) - Unix timestamp
    /// </summary>
    Task<MT5Result<DealModel[]>> GetDealsAsync(ulong login, long from, long to, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user deals (Protobuf)
    /// </summary>
    Task<MT5Result<MT5Bridge.Manager.Models.Proto.DealModel[]>> GetDealsProtoAsync(ulong login, DateTime from, DateTime to, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user deals (Protobuf) - Unix timestamp
    /// </summary>
    Task<MT5Result<MT5Bridge.Manager.Models.Proto.DealModel[]>> GetDealsProtoAsync(ulong login, long from, long to, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get deals by time range and group mask (POCO)
    /// </summary>
    Task<MT5Result<DealModel[]>> GetDealsAsync(DateTime from, DateTime to, string groupMask = "*", CancellationToken cancellationToken = default);

    /// <summary>
    /// Get deals by time range and group mask (POCO) - Unix timestamp
    /// </summary>
    Task<MT5Result<DealModel[]>> GetDealsAsync(long from, long to, string groupMask = "*", CancellationToken cancellationToken = default);

    /// <summary>
    /// Get deals by time range and group mask (Protobuf)
    /// </summary>
    Task<MT5Result<MT5Bridge.Manager.Models.Proto.DealModel[]>> GetDealsProtoAsync(DateTime from, DateTime to, string groupMask = "*", CancellationToken cancellationToken = default);

    /// <summary>
    /// Get deals by time range and group mask (Protobuf) - Unix timestamp
    /// </summary>
    Task<MT5Result<MT5Bridge.Manager.Models.Proto.DealModel[]>> GetDealsProtoAsync(long from, long to, string groupMask = "*", CancellationToken cancellationToken = default);

    /// <summary>
    /// Get specific deal by ticket (POCO)
    /// </summary>
    Task<MT5Result<DealModel>> GetDealAsync(ulong ticket, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get specific deal by ticket (Protobuf)
    /// </summary>
    Task<MT5Result<MT5Bridge.Manager.Models.Proto.DealModel>> GetDealProtoAsync(ulong ticket, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get balance operations (deposits/withdrawals) (POCO)
    /// </summary>
    Task<MT5Result<DealModel[]>> GetBalanceHistoryAsync(ulong login, DateTime from, DateTime to, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get balance operations (deposits/withdrawals) (POCO) - Unix timestamp
    /// </summary>
    Task<MT5Result<DealModel[]>> GetBalanceHistoryAsync(ulong login, long from, long to, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get balance operations (deposits/withdrawals) (Protobuf)
    /// </summary>
    Task<MT5Result<MT5Bridge.Manager.Models.Proto.DealModel[]>> GetBalanceHistoryProtoAsync(ulong login, DateTime from, DateTime to, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get balance operations (deposits/withdrawals) (Protobuf) - Unix timestamp
    /// </summary>
    Task<MT5Result<MT5Bridge.Manager.Models.Proto.DealModel[]>> GetBalanceHistoryProtoAsync(ulong login, long from, long to, CancellationToken cancellationToken = default);

    #endregion

    #region Order & Position Operations

    /// <summary>
    /// Get active orders for a user (POCO)
    /// </summary>
    Task<MT5Result<OrderModel[]>> GetOrdersAsync(ulong login, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get active orders for a user (Protobuf)
    /// </summary>
    Task<MT5Result<MT5Bridge.Manager.Models.Proto.OrderModel[]>> GetOrdersProtoAsync(ulong login, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get history orders for a user (POCO)
    /// </summary>
    Task<MT5Result<OrderModel[]>> GetHistoryOrdersAsync(ulong login, DateTime from, DateTime to, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get history orders for a user (POCO) - Unix timestamp
    /// </summary>
    Task<MT5Result<OrderModel[]>> GetHistoryOrdersAsync(ulong login, long from, long to, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get history orders for a user (Protobuf)
    /// </summary>
    Task<MT5Result<MT5Bridge.Manager.Models.Proto.OrderModel[]>> GetHistoryOrdersProtoAsync(ulong login, DateTime from, DateTime to, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get history orders for a user (Protobuf) - Unix timestamp
    /// </summary>
    Task<MT5Result<MT5Bridge.Manager.Models.Proto.OrderModel[]>> GetHistoryOrdersProtoAsync(ulong login, long from, long to, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get history orders by time range and group mask (POCO)
    /// </summary>
    Task<MT5Result<OrderModel[]>> GetHistoryOrdersAsync(DateTime from, DateTime to, string groupMask = "*", CancellationToken cancellationToken = default);

    /// <summary>
    /// Get history orders by time range and group mask (POCO) - Unix timestamp
    /// </summary>
    Task<MT5Result<OrderModel[]>> GetHistoryOrdersAsync(long from, long to, string groupMask = "*", CancellationToken cancellationToken = default);

    /// <summary>
    /// Get history orders by time range and group mask (Protobuf)
    /// </summary>
    Task<MT5Result<MT5Bridge.Manager.Models.Proto.OrderModel[]>> GetHistoryOrdersProtoAsync(DateTime from, DateTime to, string groupMask = "*", CancellationToken cancellationToken = default);

    /// <summary>
    /// Get history orders by time range and group mask (Protobuf) - Unix timestamp
    /// </summary>
    Task<MT5Result<MT5Bridge.Manager.Models.Proto.OrderModel[]>> GetHistoryOrdersProtoAsync(long from, long to, string groupMask = "*", CancellationToken cancellationToken = default);

    /// <summary>
    /// Get active positions for a user (POCO)
    /// </summary>
    Task<MT5Result<PositionModel[]>> GetPositionsAsync(ulong login, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get active positions for a user (Protobuf)
    /// </summary>
    Task<MT5Result<MT5Bridge.Manager.Models.Proto.PositionModel[]>> GetPositionsProtoAsync(ulong login, CancellationToken cancellationToken = default);

    #endregion
}
