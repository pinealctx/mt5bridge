using MetaQuotes.MT5CommonAPI;
using MetaQuotes.MT5ManagerAPI;

namespace MT5Bridge.MT5.Core.Managers;

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

    /// <summary>
    /// Deal added event
    /// </summary>
    event EventHandler<CIMTDeal>? DealAdded;

    /// <summary>
    /// Deal updated event
    /// </summary>
    event EventHandler<CIMTDeal>? DealUpdated;

    /// <summary>
    /// Deal deleted event
    /// </summary>
    event EventHandler<CIMTDeal>? DealDeleted;

    /// <summary>
    /// Order added event
    /// </summary>
    event EventHandler<CIMTOrder>? OrderAdded;

    /// <summary>
    /// Order updated event
    /// </summary>
    event EventHandler<CIMTOrder>? OrderUpdated;

    /// <summary>
    /// Order deleted event
    /// </summary>
    event EventHandler<CIMTOrder>? OrderDeleted;

    /// <summary>
    /// Position added event
    /// </summary>
    event EventHandler<CIMTPosition>? PositionAdded;

    /// <summary>
    /// Position updated event
    /// </summary>
    event EventHandler<CIMTPosition>? PositionUpdated;

    /// <summary>
    /// Position deleted event
    /// </summary>
    event EventHandler<CIMTPosition>? PositionDeleted;

    /// <summary>
    /// Connect to MT5 server
    /// </summary>
    Task<MT5Result> ConnectAsync(MT5ConnectionSettings settings, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnect from MT5 server
    /// </summary>
    Task DisconnectAsync();

    /// <summary>
    /// Get user groups
    /// </summary>
    Task<MT5Result<CIMTConGroupArray>> GetGroupsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get specific group by name
    /// </summary>
    Task<MT5Result<CIMTConGroup>> GetGroupAsync(string groupName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user information
    /// </summary>
    Task<MT5Result<CIMTUser>> GetUserAsync(ulong login, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create new user
    /// </summary>
    Task<MT5Result<CIMTUser>> CreateUserAsync(CIMTUser user, string masterPassword, string investorPassword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update user information
    /// </summary>
    Task<MT5Result<CIMTUser>> UpdateUserAsync(CIMTUser user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete user
    /// </summary>
    Task<MT5Result> DeleteUserAsync(ulong login, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user account information
    /// </summary>
    Task<MT5Result<CIMTAccount>> GetAccountAsync(ulong login, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deposit to user account
    /// </summary>
    Task<MT5Result> DepositAsync(ulong login, decimal amount, string comment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Withdraw from user account
    /// </summary>
    Task<MT5Result> WithdrawAsync(ulong login, decimal amount, string comment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user deals
    /// </summary>
    Task<MT5Result<CIMTDealArray>> GetDealsAsync(ulong login, DateTime from, DateTime to, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user deal by ticket
    /// </summary>
    Task<MT5Result<CIMTDeal>> GetDealAsync(ulong ticket, CancellationToken cancellationToken = default);
}
