using MetaQuotes.MT5CommonAPI;
using MetaQuotes.MT5ManagerAPI;
using Serilog;
using MT5Bridge.Manager.Sinks;
using MT5Bridge.Manager.Models;
using System.Diagnostics.CodeAnalysis;

// POCO models - use aliases to avoid conflicts
using UserModel = MT5Bridge.Manager.Models.UserModel;
using AccountModel = MT5Bridge.Manager.Models.AccountModel;
using GroupModel = MT5Bridge.Manager.Models.GroupModel;
using DealModel = MT5Bridge.Manager.Models.DealModel;
using OrderModel = MT5Bridge.Manager.Models.OrderModel;
using PositionModel = MT5Bridge.Manager.Models.PositionModel;

// Protobuf models - use aliases
using ProtoUser = MT5Bridge.Manager.Models.Proto.UserModel;
using ProtoAccount = MT5Bridge.Manager.Models.Proto.AccountModel;
using ProtoGroup = MT5Bridge.Manager.Models.Proto.GroupModel;
using ProtoDeal = MT5Bridge.Manager.Models.Proto.DealModel;
using ProtoOrder = MT5Bridge.Manager.Models.Proto.OrderModel;
using ProtoPosition = MT5Bridge.Manager.Models.Proto.PositionModel;

// Proto extensions
using MT5Bridge.Manager.Models.Proto;

namespace MT5Bridge.Manager.Managers;

/// <summary>
/// MT5 Manager implementation
/// Thread-safe wrapper around CIMTManagerAPI
/// </summary>
public class MT5Manager : IMT5Manager
{
    private static int _instanceCount = 0;
    private static readonly object _factoryLock = new();

    private readonly ILogger _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP006:Implement IDisposable")]
    [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")]
    private CIMTManagerAPI? _manager;

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP006:Implement IDisposable")]
    [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")]
    private CIMTDealArray? _dealArray;

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP006:Implement IDisposable")]
    [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")]
    private CIMTUser? _user;

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP006:Implement IDisposable")]
    [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")]
    private CIMTUserArray? _userArray;

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP006:Implement IDisposable")]
    [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")]
    private CIMTAccount? _account;

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP006:Implement IDisposable")]
    [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")]
    private CIMTConGroup? _group;

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP006:Implement IDisposable")]
    [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")]
    private CIMTConGroupArray? _groupArray;

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP006:Implement IDisposable")]
    [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")]
    private CIMTOrderArray? _orderArray;

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP006:Implement IDisposable")]
    [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")]
    private CIMTPositionArray? _positionArray;

    [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")]
    private MT5ManagerSink? _managerSink;

    // Generic sinks (supporting custom model types)
    private readonly List<object> _genericSinks = new();

    private ConnectionState _state = ConnectionState.Disconnected;
    private MT5ConnectionSettings? _settings;
    private bool _disposed;

    public bool IsConnected => _state == ConnectionState.Connected;
    public ConnectionState State => _state;
    public MT5ConnectionSettings? Settings => _settings;

    public event EventHandler<ConnectionStateChangedEventArgs>? ConnectionStateChanged;

    public MT5Manager(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        lock (_factoryLock)
        {
            _instanceCount++;
        }
    }

    #region Typed Event Handlers

    /// <summary>
    /// Register a deal event handler (POCO)
    /// </summary>
    public void RegisterDealHandler(
        Action<DealModel>? onAdd = null,
        Action<DealModel>? onUpdate = null,
        Action<DealModel>? onDelete = null)
    {
        RegisterDealHandlerInternal<DealModel>(m => m.ToModel(), onAdd, onUpdate, onDelete);
    }

    /// <summary>
    /// Register a deal event handler (Protobuf)
    /// </summary>
    public void RegisterDealProtoHandler(
        Action<ProtoDeal>? onAdd = null,
        Action<ProtoDeal>? onUpdate = null,
        Action<ProtoDeal>? onDelete = null)
    {
        RegisterDealHandlerInternal<ProtoDeal>(m => m.ToProto(), onAdd, onUpdate, onDelete);
    }

    /// <summary>
    /// Register an order event handler (POCO)
    /// </summary>
    public void RegisterOrderHandler(
        Action<OrderModel>? onAdd = null,
        Action<OrderModel>? onUpdate = null,
        Action<OrderModel>? onDelete = null)
    {
        RegisterOrderHandlerInternal<OrderModel>(m => m.ToModel(), onAdd, onUpdate, onDelete);
    }

    /// <summary>
    /// Register an order event handler (Protobuf)
    /// </summary>
    public void RegisterOrderProtoHandler(
        Action<ProtoOrder>? onAdd = null,
        Action<ProtoOrder>? onUpdate = null,
        Action<ProtoOrder>? onDelete = null)
    {
        RegisterOrderHandlerInternal<ProtoOrder>(m => m.ToProto(), onAdd, onUpdate, onDelete);
    }

    /// <summary>
    /// Register a position event handler (POCO)
    /// </summary>
    public void RegisterPositionHandler(
        Action<PositionModel>? onAdd = null,
        Action<PositionModel>? onUpdate = null,
        Action<PositionModel>? onDelete = null)
    {
        RegisterPositionHandlerInternal<PositionModel>(m => m.ToModel(), onAdd, onUpdate, onDelete);
    }

    /// <summary>
    /// Register a position event handler (Protobuf)
    /// </summary>
    public void RegisterPositionProtoHandler(
        Action<ProtoPosition>? onAdd = null,
        Action<ProtoPosition>? onUpdate = null,
        Action<ProtoPosition>? onDelete = null)
    {
        RegisterPositionHandlerInternal<ProtoPosition>(m => m.ToProto(), onAdd, onUpdate, onDelete);
    }

    /// <summary>
    /// Register a typed deal event handler
    /// </summary>
    private void RegisterDealHandlerInternal<T>(
        Func<CIMTDeal, T> converter,
        Action<T>? onAdd = null,
        Action<T>? onUpdate = null,
        Action<T>? onDelete = null) where T : class
    {
        if (converter == null) throw new ArgumentNullException(nameof(converter));

        var sink = new MT5GenericDealSink<T>(converter, onAdd, onUpdate, onDelete);

        if (sink.RegisterSink() != MTRetCode.MT_RET_OK)
        {
            throw new InvalidOperationException("Failed to register generic deal sink");
        }

        _genericSinks.Add(sink);

        // If already connected, subscribe immediately
        if (_manager != null && IsConnected)
        {
            _manager.DealSubscribe(sink);
        }

        _logger.Debug($"Registered generic deal handler for type {typeof(T).Name}");
    }

    /// <summary>
    /// Register a typed order event handler
    /// </summary>
    private void RegisterOrderHandlerInternal<T>(
        Func<CIMTOrder, T> converter,
        Action<T>? onAdd = null,
        Action<T>? onUpdate = null,
        Action<T>? onDelete = null) where T : class
    {
        if (converter == null) throw new ArgumentNullException(nameof(converter));

        var sink = new MT5GenericOrderSink<T>(converter, onAdd, onUpdate, onDelete);

        if (sink.RegisterSink() != MTRetCode.MT_RET_OK)
        {
            throw new InvalidOperationException("Failed to register generic order sink");
        }

        _genericSinks.Add(sink);

        // If already connected, subscribe immediately
        if (_manager != null && IsConnected)
        {
            _manager.OrderSubscribe(sink);
        }

        _logger.Debug($"Registered generic order handler for type {typeof(T).Name}");
    }

    /// <summary>
    /// Register a typed position event handler
    /// </summary>
    private void RegisterPositionHandlerInternal<T>(
        Func<CIMTPosition, T> converter,
        Action<T>? onAdd = null,
        Action<T>? onUpdate = null,
        Action<T>? onDelete = null) where T : class
    {
        if (converter == null) throw new ArgumentNullException(nameof(converter));

        var sink = new MT5GenericPositionSink<T>(converter, onAdd, onUpdate, onDelete);

        if (sink.RegisterSink() != MTRetCode.MT_RET_OK)
        {
            throw new InvalidOperationException("Failed to register generic position sink");
        }

        _genericSinks.Add(sink);

        // If already connected, subscribe immediately
        if (_manager != null && IsConnected)
        {
            _manager.PositionSubscribe(sink);
        }

        _logger.Debug($"Registered generic position handler for type {typeof(T).Name}");
    }

    #endregion

    public async Task<MT5Result> ConnectAsync(MT5ConnectionSettings settings, CancellationToken cancellationToken = default)
    {
        if (settings == null) throw new ArgumentNullException(nameof(settings));

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_state == ConnectionState.Connected)
            {
                return MT5Result.Success("Already connected");
            }

            SetState(ConnectionState.Connecting);
            _settings = settings;

            // Initialize manager if needed
            if (_manager == null)
            {
                var initResult = await Task.Run(() => InitializeManager(), cancellationToken);
                if (!initResult.IsSuccess)
                {
                    SetState(ConnectionState.Failed, initResult.Message);
                    return initResult;
                }
            }

            // Convert pump mode
            var pumpMode = settings.PumpMode switch
            {
                PumpMode.None => CIMTManagerAPI.EnPumpModes.PUMP_MODE_NONE,
                PumpMode.Full => CIMTManagerAPI.EnPumpModes.PUMP_MODE_FULL,
                PumpMode.Symbols => CIMTManagerAPI.EnPumpModes.PUMP_MODE_SYMBOLS,
                _ => CIMTManagerAPI.EnPumpModes.PUMP_MODE_FULL
            };

            // Connect
            var result = await Task.Run(() =>
            {
                // Subscribe to manager sink
                if (_managerSink != null)
                {
                    _manager!.Subscribe(_managerSink);
                }

                // Subscribe to generic sinks
                foreach (var sink in _genericSinks)
                {
                    if (sink is CIMTDealSink dealSink)
                    {
                        _manager!.DealSubscribe(dealSink);
                    }
                    else if (sink is CIMTOrderSink orderSink)
                    {
                        _manager!.OrderSubscribe(orderSink);
                    }
                    else if (sink is CIMTPositionSink positionSink)
                    {
                        _manager!.PositionSubscribe(positionSink);
                    }
                }

                var res = _manager!.Connect(
                    settings.Server,
                    settings.Login,
                    settings.Password,
                    null,
                    pumpMode,
                    settings.TimeoutMs);

                if (res != MTRetCode.MT_RET_OK)
                {
                    _logger.Error($"MT5 connection failed: {res}");
                    return MT5Result.Failure(res, $"Connection failed: {res}");
                }

                _logger.Information($"MT5 connected: {settings.Server}, login={settings.Login}");
                return MT5Result.Success("Connected successfully");
            }, cancellationToken);

            if (result.IsSuccess)
            {
                SetState(ConnectionState.Connected);
            }
            else
            {
                SetState(ConnectionState.Failed, result.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Connect error");
            SetState(ConnectionState.Failed, ex.Message);
            return MT5Result.Failure($"Connect error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Disconnect()
    {
        _lock.Wait();
        try
        {
            DisconnectInternal();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task DisconnectAsync()
    {
        await _lock.WaitAsync();
        try
        {
            await Task.Run(DisconnectInternal);
        }
        finally
        {
            _lock.Release();
        }
    }

    private void DisconnectInternal()
    {
        if (_manager != null && _state == ConnectionState.Connected)
        {
            try
            {
                // Unsubscribe manager sink
                if (_managerSink != null)
                {
                    _manager.Unsubscribe(_managerSink);
                }

                // Unsubscribe generic sinks
                foreach (var sink in _genericSinks)
                {
                    if (sink is CIMTDealSink dealSink)
                    {
                        _manager.DealUnsubscribe(dealSink);
                    }
                    else if (sink is CIMTOrderSink orderSink)
                    {
                        _manager.OrderUnsubscribe(orderSink);
                    }
                    else if (sink is CIMTPositionSink positionSink)
                    {
                        _manager.PositionUnsubscribe(positionSink);
                    }
                }

                _manager.Disconnect();
                _logger.Information("MT5 disconnected");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error during disconnect operation: {ex.Message}");
                // Continue to set state even if disconnect failed
            }
        }
        SetState(ConnectionState.Disconnected);
    }

    #region Group Operations

    public async Task<MT5Result<GroupModel[]>> GetGroupsAsync(CancellationToken cancellationToken = default)
    {
        var comResult = await GetGroupsComAsync(cancellationToken);
        if (!comResult.IsSuccess || comResult.Data == null)
            return MT5Result<GroupModel[]>.Failure(comResult.RetCode, comResult.Message);

        try
        {
            var groups = new List<GroupModel>();
            var groupArray = comResult.Data;

            for (uint i = 0; i < groupArray.Total(); i++)
            {
                var group = groupArray.Next(i);
                if (group != null)
                {
                    groups.Add(group.ToModel());
                }
            }

            return MT5Result<GroupModel[]>.Success(groups.ToArray());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "GetGroupsAsync conversion error");
            return MT5Result<GroupModel[]>.Failure($"Conversion error: {ex.Message}");
        }
    }

    public async Task<MT5Result<ProtoGroup[]>> GetGroupsProtoAsync(CancellationToken cancellationToken = default)
    {
        var comResult = await GetGroupsComAsync(cancellationToken);
        if (!comResult.IsSuccess || comResult.Data == null)
            return MT5Result<ProtoGroup[]>.Failure(comResult.RetCode, comResult.Message);

        try
        {
            var groups = new List<ProtoGroup>();
            var groupArray = comResult.Data;

            for (uint i = 0; i < groupArray.Total(); i++)
            {
                var group = groupArray.Next(i);
                if (group != null)
                {
                    groups.Add(MT5Bridge.Manager.Models.Proto.GroupProtoExtensions.ToProto(group));
                }
            }

            return MT5Result<ProtoGroup[]>.Success(groups.ToArray());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "GetGroupsProtoAsync conversion error");
            return MT5Result<ProtoGroup[]>.Failure($"Conversion error: {ex.Message}");
        }
    }

    public async Task<MT5Result<GroupModel>> GetGroupAsync(string groupName, CancellationToken cancellationToken = default)
    {
        var comResult = await GetGroupComAsync(groupName, cancellationToken);
        if (!comResult.IsSuccess || comResult.Data == null)
            return MT5Result<GroupModel>.Failure(comResult.RetCode, comResult.Message);

        try
        {
            return MT5Result<GroupModel>.Success(comResult.Data.ToModel());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"GetGroupAsync({groupName}) conversion error");
            return MT5Result<GroupModel>.Failure($"Conversion error: {ex.Message}");
        }
    }

    public async Task<MT5Result<ProtoGroup>> GetGroupProtoAsync(string groupName, CancellationToken cancellationToken = default)
    {
        var comResult = await GetGroupComAsync(groupName, cancellationToken);
        if (!comResult.IsSuccess || comResult.Data == null)
            return MT5Result<ProtoGroup>.Failure(comResult.RetCode, comResult.Message);

        try
        {
            return MT5Result<ProtoGroup>.Success(MT5Bridge.Manager.Models.Proto.GroupProtoExtensions.ToProto(comResult.Data));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"GetGroupProtoAsync({groupName}) conversion error");
            return MT5Result<ProtoGroup>.Failure($"Conversion error: {ex.Message}");
        }
    }

    private async Task<MT5Result<CIMTConGroupArray>> GetGroupsComAsync(CancellationToken cancellationToken = default)
    {
        if (!IsConnected) return MT5Result<CIMTConGroupArray>.Failure("Not connected");

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_groupArray == null)
            {
                return MT5Result<CIMTConGroupArray>.Failure("GroupArray not initialized");
            }

            var result = await Task.Run(() =>
            {
                _groupArray.Clear();
                var res = _manager!.GroupRequestArray("*", _groupArray);

                if (res != MTRetCode.MT_RET_OK)
                {
                    _logger.Error($"GroupRequestArray failed: {res}");
                    return MT5Result<CIMTConGroupArray>.Failure(res, $"GroupRequestArray failed: {res}");
                }

                _logger.Debug($"Retrieved {_groupArray.Total()} groups");
                return MT5Result<CIMTConGroupArray>.Success(_groupArray);
            }, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "GetGroups error");
            return MT5Result<CIMTConGroupArray>.Failure($"GetGroups error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<MT5Result<CIMTConGroup>> GetGroupComAsync(string groupName, CancellationToken cancellationToken = default)
    {
        if (!IsConnected) return MT5Result<CIMTConGroup>.Failure("Not connected");
        if (string.IsNullOrEmpty(groupName)) throw new ArgumentNullException(nameof(groupName));

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_group == null)
            {
                return MT5Result<CIMTConGroup>.Failure("Group object not initialized");
            }

            var result = await Task.Run(() =>
            {
                _group.Clear();
                var res = _manager!.GroupRequest(groupName, _group);

                if (res != MTRetCode.MT_RET_OK)
                {
                    _logger.Error($"GroupRequest({groupName}) failed: {res}");
                    return MT5Result<CIMTConGroup>.Failure(res, $"GroupRequest failed: {res}");
                }

                _logger.Debug($"Retrieved group: {groupName}");
                return MT5Result<CIMTConGroup>.Success(_group);
            }, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"GetGroup({groupName}) error");
            return MT5Result<CIMTConGroup>.Failure($"GetGroup error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    #endregion

    #region User Operations

    public async Task<MT5Result<UserModel[]>> GetUsersAsync(string? groupMask = null, int offset = 0, int limit = 100, CancellationToken cancellationToken = default)
    {
        var comResult = await GetUsersComAsync(groupMask, offset, limit, cancellationToken);
        if (!comResult.IsSuccess || comResult.Data == null)
            return MT5Result<UserModel[]>.Failure(comResult.RetCode, comResult.Message);

        try
        {
            var users = comResult.Data.Select(u => u.ToModel()).ToArray();
            return MT5Result<UserModel[]>.Success(users);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "GetUsersAsync conversion error");
            return MT5Result<UserModel[]>.Failure($"Conversion error: {ex.Message}");
        }
        finally
        {
            // CRITICAL: Dispose temporary COM objects to prevent memory leaks
            foreach (var comUser in comResult.Data)
            {
                comUser?.Dispose();
            }
        }
    }

    public async Task<MT5Result<ProtoUser[]>> GetUsersProtoAsync(string? groupMask = null, int offset = 0, int limit = 100, CancellationToken cancellationToken = default)
    {
        var comResult = await GetUsersComAsync(groupMask, offset, limit, cancellationToken);
        if (!comResult.IsSuccess || comResult.Data == null)
            return MT5Result<ProtoUser[]>.Failure(comResult.RetCode, comResult.Message);

        try
        {
            var users = comResult.Data.Select(u => MT5Bridge.Manager.Models.Proto.UserProtoExtensions.ToProto(u)).ToArray();
            return MT5Result<ProtoUser[]>.Success(users);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "GetUsersProtoAsync conversion error");
            return MT5Result<ProtoUser[]>.Failure($"Conversion error: {ex.Message}");
        }
        finally
        {
            // CRITICAL: Dispose temporary COM objects to prevent memory leaks
            foreach (var comUser in comResult.Data)
            {
                comUser?.Dispose();
            }
        }
    }

    private async Task<MT5Result<CIMTUser[]>> GetUsersComAsync(string? groupMask = null, int offset = 0, int limit = 100, CancellationToken cancellationToken = default)
    {
        if (!IsConnected) return MT5Result<CIMTUser[]>.Failure("Not connected");
        if (offset < 0) return MT5Result<CIMTUser[]>.Failure("Offset must be >= 0");
        if (limit <= 0 || limit > 10000) return MT5Result<CIMTUser[]>.Failure("Limit must be between 1 and 10000");

        await _lock.WaitAsync(cancellationToken);
        try
        {
            var mask = string.IsNullOrWhiteSpace(groupMask) ? "*" : groupMask;

            // Check if the mask contains wildcards or multiple groups
            var hasWildcard = mask.Contains('*') || mask.Contains('!') || mask.Contains(',');

            if (hasWildcard)
            {
                // Use UserGetByGroup (loads all matching users, then slices in memory)
                return await GetUsersByGroupMaskAsync(mask, offset, limit);
            }
            else
            {
                // Use UserLogins + UserGetByLogins (true pagination)
                return await GetUsersByGroupPaginatedAsync(mask, offset, limit);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"GetUsers error");
            return MT5Result<CIMTUser[]>.Failure($"GetUsers error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Get users using UserLogins + UserGetByLogins (true pagination for single group)
    /// </summary>
    private async Task<MT5Result<CIMTUser[]>> GetUsersByGroupPaginatedAsync(string groupName, int offset, int limit)
    {
        if (_userArray == null)
        {
            return MT5Result<CIMTUser[]>.Failure("User array object not initialized");
        }

        // Step 1: Get all login numbers for the group (lightweight)
        var logins = _manager!.UserLogins(groupName, out var res);

        if (res != MTRetCode.MT_RET_OK)
        {
            _logger.Error($"UserLogins({groupName}) failed: {res}");
            return MT5Result<CIMTUser[]>.Failure($"UserLogins failed: {res}");
        }

        if (logins == null || logins.Length == 0)
        {
            return MT5Result<CIMTUser[]>.Success(Array.Empty<CIMTUser>(), "No users found in group");
        }

        var total = logins.Length;

        // Step 2: Paginate the login array
        var actualOffset = Math.Min(offset, total);
        var actualLimit = Math.Min(limit, total - actualOffset);

        if (actualLimit <= 0)
        {
            return MT5Result<CIMTUser[]>.Success(Array.Empty<CIMTUser>(), $"Offset {offset} exceeds total {total}");
        }

        var pagedLogins = logins.Skip(actualOffset).Take(actualLimit).ToArray();

        // Step 3: Batch fetch user details for the paged logins
        _userArray.Clear();
        res = _manager.UserGetByLogins(pagedLogins, _userArray);

        if (res != MTRetCode.MT_RET_OK)
        {
            _logger.Error($"UserGetByLogins failed: {res}");
            return MT5Result<CIMTUser[]>.Failure($"UserGetByLogins failed: {res}");
        }

        // Step 4: Clone users from the array using List to avoid null elements
        var userCount = _userArray.Total();
        var result = new List<CIMTUser>((int)userCount);

        for (uint i = 0; i < userCount; i++)
        {
            var sourceUser = _userArray.Next(i);
            if (sourceUser == null) continue;

            var userCopy = _manager.UserCreate();
            if (userCopy != null)
            {
                userCopy.Assign(sourceUser);
                result.Add(userCopy);
            }
            else
            {
                _logger.Warning($"Failed to create user copy for user at index {i}");
            }
        }

        var message = $"Retrieved {result.Count} users (offset={actualOffset}, total={total}, group={groupName})";
        _logger.Debug(message);

        return MT5Result<CIMTUser[]>.Success(result.ToArray(), message);
    }

    /// <summary>
    /// Get users using UserGetByGroup (memory pagination for wildcards/multiple groups)
    /// </summary>
    private async Task<MT5Result<CIMTUser[]>> GetUsersByGroupMaskAsync(string mask, int offset, int limit)
    {
        if (_userArray == null)
        {
            return MT5Result<CIMTUser[]>.Failure("User array object not initialized");
        }

        // Load all matching users
        _userArray.Clear();
        var res = _manager!.UserGetByGroup(mask, _userArray);

        if (res != MTRetCode.MT_RET_OK)
        {
            _logger.Error($"UserGetByGroup({mask}) failed: {res}");
            return MT5Result<CIMTUser[]>.Failure($"UserGetByGroup failed: {res}");
        }

        var total = _userArray.Total();
        if (total == 0)
        {
            return MT5Result<CIMTUser[]>.Success(Array.Empty<CIMTUser>(), "No users found");
        }

        // Paginate in memory
        var actualOffset = Math.Min(offset, (int)total);
        var actualLimit = Math.Min(limit, (int)total - actualOffset);

        if (actualLimit <= 0)
        {
            return MT5Result<CIMTUser[]>.Success(Array.Empty<CIMTUser>(), $"Offset {offset} exceeds total {total}");
        }

        var result = new List<CIMTUser>(actualLimit);
        for (var i = 0; i < actualLimit; i++)
        {
            var sourceUser = _userArray.Next((uint)(actualOffset + i));
            if (sourceUser == null) break;

            var userCopy = _manager.UserCreate();
            if (userCopy != null)
            {
                userCopy.Assign(sourceUser);
                result.Add(userCopy);
            }
        }

        var message = $"Retrieved {result.Count} users (offset={actualOffset}, total={total}, mask={mask}, mode=memory-paging)";
        _logger.Debug(message);

        return MT5Result<CIMTUser[]>.Success(result.ToArray(), message);
    }

    #endregion

    #region User Operations

    public async Task<MT5Result<UserModel>> GetUserAsync(ulong login, CancellationToken cancellationToken = default)
    {
        var comResult = await GetUserComAsync(login, cancellationToken);
        if (!comResult.IsSuccess || comResult.Data == null)
            return MT5Result<UserModel>.Failure(comResult.RetCode, comResult.Message);

        try
        {
            return MT5Result<UserModel>.Success(comResult.Data.ToModel());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"GetUserAsync({login}) conversion error");
            return MT5Result<UserModel>.Failure($"Conversion error: {ex.Message}");
        }
    }

    public async Task<MT5Result<ProtoUser>> GetUserProtoAsync(ulong login, CancellationToken cancellationToken = default)
    {
        var comResult = await GetUserComAsync(login, cancellationToken);
        if (!comResult.IsSuccess || comResult.Data == null)
            return MT5Result<ProtoUser>.Failure(comResult.RetCode, comResult.Message);

        try
        {
            return MT5Result<ProtoUser>.Success(MT5Bridge.Manager.Models.Proto.UserProtoExtensions.ToProto(comResult.Data));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"GetUserProtoAsync({login}) conversion error");
            return MT5Result<ProtoUser>.Failure($"Conversion error: {ex.Message}");
        }
    }

    private async Task<MT5Result<CIMTUser>> GetUserComAsync(ulong login, CancellationToken cancellationToken = default)
    {
        if (!IsConnected) return MT5Result<CIMTUser>.Failure("Not connected");

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_user == null)
            {
                return MT5Result<CIMTUser>.Failure("User object not initialized");
            }

            var result = await Task.Run(() =>
            {
                _user.Clear();
                var res = _manager!.UserRequest(login, _user);

                if (res != MTRetCode.MT_RET_OK)
                {
                    _logger.Error($"UserRequest({login}) failed: {res}");
                    return MT5Result<CIMTUser>.Failure(res, $"UserRequest failed: {res}");
                }

                _logger.Debug($"Retrieved user: {login}");
                return MT5Result<CIMTUser>.Success(_user);
            }, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"GetUser({login}) error");
            return MT5Result<CIMTUser>.Failure($"GetUser error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<MT5Result<UserModel>> CreateUserAsync(UserModel user, string masterPassword, string investorPassword, CancellationToken cancellationToken = default)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        await _lock.WaitAsync(cancellationToken);
        try
        {
            var comUser = _manager!.UserCreate();
            if (comUser == null) return MT5Result<UserModel>.Failure("Failed to create COM user object");

            try
            {
                comUser.UpdateFromModel(user);
                var res = _manager.UserAdd(comUser, masterPassword, investorPassword);

                if (res != MTRetCode.MT_RET_OK)
                {
                    _logger.Error($"UserAdd failed: {res}");
                    return MT5Result<UserModel>.Failure(res, $"UserAdd failed: {res}");
                }

                _logger.Information($"User created: {comUser.Login()}");
                return MT5Result<UserModel>.Success(comUser.ToModel());
            }
            finally
            {
                comUser.Dispose();
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "CreateUserAsync error");
            return MT5Result<UserModel>.Failure($"CreateUser error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<MT5Result<ProtoUser>> CreateUserProtoAsync(ProtoUser user, string masterPassword, string investorPassword, CancellationToken cancellationToken = default)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        await _lock.WaitAsync(cancellationToken);
        try
        {
            var comUser = _manager!.UserCreate();
            if (comUser == null) return MT5Result<ProtoUser>.Failure("Failed to create COM user object");

            try
            {
                MT5Bridge.Manager.Models.Proto.UserProtoExtensions.UpdateFromProto(comUser, user);
                var res = _manager.UserAdd(comUser, masterPassword, investorPassword);

                if (res != MTRetCode.MT_RET_OK)
                {
                    _logger.Error($"UserAdd failed: {res}");
                    return MT5Result<ProtoUser>.Failure(res, $"UserAdd failed: {res}");
                }

                _logger.Information($"User created: {comUser.Login()}");
                return MT5Result<ProtoUser>.Success(MT5Bridge.Manager.Models.Proto.UserProtoExtensions.ToProto(comUser));
            }
            finally
            {
                comUser.Dispose();
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "CreateUserProtoAsync error");
            return MT5Result<ProtoUser>.Failure($"CreateUser error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<MT5Result<CIMTUser>> CreateUserComAsync(CIMTUser user, string masterPassword, string investorPassword, CancellationToken cancellationToken = default)
    {
        if (!IsConnected) return MT5Result<CIMTUser>.Failure("Not connected");
        if (user == null) throw new ArgumentNullException(nameof(user));

        await _lock.WaitAsync(cancellationToken);
        try
        {
            var result = await Task.Run(() =>
            {
                var res = _manager!.UserAdd(user, masterPassword, investorPassword);

                if (res != MTRetCode.MT_RET_OK)
                {
                    _logger.Error($"UserAdd failed: {res}");
                    return MT5Result<CIMTUser>.Failure(res, $"UserAdd failed: {res}");
                }

                _logger.Information($"User created: {user.Login()}");
                return MT5Result<CIMTUser>.Success(user);
            }, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "CreateUser error");
            return MT5Result<CIMTUser>.Failure($"CreateUser error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<MT5Result<UserModel>> UpdateUserAsync(UserModel user, CancellationToken cancellationToken = default)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        await _lock.WaitAsync(cancellationToken);
        try
        {
            var comUser = _manager!.UserCreate();
            if (comUser == null) return MT5Result<UserModel>.Failure("Failed to create COM user object");

            try
            {
                // First request current user to ensure we have all fields (some might not be in POCO)
                var res = _manager.UserRequest(user.Login, comUser);
                if (res != MTRetCode.MT_RET_OK)
                {
                    return MT5Result<UserModel>.Failure(res, $"UserRequest({user.Login}) failed: {res}");
                }

                // Update with POCO values
                comUser.UpdateFromModel(user);
                res = _manager.UserUpdate(comUser);

                if (res != MTRetCode.MT_RET_OK)
                {
                    _logger.Error($"UserUpdate failed: {res}");
                    return MT5Result<UserModel>.Failure(res, $"UserUpdate failed: {res}");
                }

                _logger.Information($"User updated: {comUser.Login()}");
                return MT5Result<UserModel>.Success(comUser.ToModel());
            }
            finally
            {
                comUser.Dispose();
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "UpdateUserAsync error");
            return MT5Result<UserModel>.Failure($"UpdateUser error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<MT5Result<ProtoUser>> UpdateUserProtoAsync(ProtoUser user, CancellationToken cancellationToken = default)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        await _lock.WaitAsync(cancellationToken);
        try
        {
            var comUser = _manager!.UserCreate();
            if (comUser == null) return MT5Result<ProtoUser>.Failure("Failed to create COM user object");

            try
            {
                // First request current user
                var res = _manager.UserRequest(user.Login, comUser);
                if (res != MTRetCode.MT_RET_OK)
                {
                    return MT5Result<ProtoUser>.Failure(res, $"UserRequest({user.Login}) failed: {res}");
                }

                // Update with Proto values
                MT5Bridge.Manager.Models.Proto.UserProtoExtensions.UpdateFromProto(comUser, user);
                res = _manager.UserUpdate(comUser);

                if (res != MTRetCode.MT_RET_OK)
                {
                    _logger.Error($"UserUpdate failed: {res}");
                    return MT5Result<ProtoUser>.Failure(res, $"UserUpdate failed: {res}");
                }

                _logger.Information($"User updated: {comUser.Login()}");
                return MT5Result<ProtoUser>.Success(MT5Bridge.Manager.Models.Proto.UserProtoExtensions.ToProto(comUser));
            }
            finally
            {
                comUser.Dispose();
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "UpdateUserProtoAsync error");
            return MT5Result<ProtoUser>.Failure($"UpdateUser error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<MT5Result<CIMTUser>> UpdateUserComAsync(CIMTUser user, CancellationToken cancellationToken = default)
    {
        if (!IsConnected) return MT5Result<CIMTUser>.Failure("Not connected");
        if (user == null) throw new ArgumentNullException(nameof(user));

        await _lock.WaitAsync(cancellationToken);
        try
        {
            var result = await Task.Run(() =>
            {
                var res = _manager!.UserUpdate(user);

                if (res != MTRetCode.MT_RET_OK)
                {
                    _logger.Error($"UserUpdate failed: {res}");
                    return MT5Result<CIMTUser>.Failure(res, $"UserUpdate failed: {res}");
                }

                _logger.Information($"User updated: {user.Login()}");
                return MT5Result<CIMTUser>.Success(user);
            }, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "UpdateUser error");
            return MT5Result<CIMTUser>.Failure($"UpdateUser error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<MT5Result> DeleteUserAsync(ulong login, CancellationToken cancellationToken = default)
    {
        if (!IsConnected) return MT5Result.Failure("Not connected");

        await _lock.WaitAsync(cancellationToken);
        try
        {
            var result = await Task.Run(() =>
            {
                var res = _manager!.UserDelete(login);

                if (res != MTRetCode.MT_RET_OK)
                {
                    _logger.Error($"UserDelete({login}) failed: {res}");
                    return MT5Result.Failure(res, $"UserDelete failed: {res}");
                }

                _logger.Information($"User deleted: {login}");
                return MT5Result.Success();
            }, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"DeleteUser({login}) error");
            return MT5Result.Failure($"DeleteUser error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    #endregion

    #region Account Operations

    public async Task<MT5Result<AccountModel>> GetAccountAsync(ulong login, CancellationToken cancellationToken = default)
    {
        var comResult = await GetAccountComAsync(login, cancellationToken);
        if (!comResult.IsSuccess || comResult.Data == null)
            return MT5Result<AccountModel>.Failure(comResult.RetCode, comResult.Message);

        try
        {
            return MT5Result<AccountModel>.Success(comResult.Data.ToModel());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"GetAccountAsync({login}) conversion error");
            return MT5Result<AccountModel>.Failure($"Conversion error: {ex.Message}");
        }
    }

    public async Task<MT5Result<ProtoAccount>> GetAccountProtoAsync(ulong login, CancellationToken cancellationToken = default)
    {
        var comResult = await GetAccountComAsync(login, cancellationToken);
        if (!comResult.IsSuccess || comResult.Data == null)
            return MT5Result<ProtoAccount>.Failure(comResult.RetCode, comResult.Message);

        try
        {
            return MT5Result<ProtoAccount>.Success(MT5Bridge.Manager.Models.Proto.AccountProtoExtensions.ToProto(comResult.Data));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"GetAccountProtoAsync({login}) conversion error");
            return MT5Result<ProtoAccount>.Failure($"Conversion error: {ex.Message}");
        }
    }

    private async Task<MT5Result<CIMTAccount>> GetAccountComAsync(ulong login, CancellationToken cancellationToken = default)
    {
        if (!IsConnected) return MT5Result<CIMTAccount>.Failure("Not connected");

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_account == null)
            {
                return MT5Result<CIMTAccount>.Failure("Account object not initialized");
            }

            var result = await Task.Run(() =>
            {
                _account.Clear();
                var res = _manager!.UserAccountRequest(login, _account);

                if (res != MTRetCode.MT_RET_OK)
                {
                    _logger.Error($"UserAccountRequest({login}) failed: {res}");
                    return MT5Result<CIMTAccount>.Failure(res, $"UserAccountRequest failed: {res}");
                }

                _logger.Debug($"Retrieved account: {login}, balance={_account.Balance()}");
                return MT5Result<CIMTAccount>.Success(_account);
            }, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"GetAccount({login}) error");
            return MT5Result<CIMTAccount>.Failure($"GetAccount error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<MT5Result> DepositAsync(ulong login, decimal amount, string comment, CancellationToken cancellationToken = default)
    {
        if (!IsConnected) return MT5Result.Failure("Not connected");
        if (amount <= 0) throw new ArgumentException("Amount must be positive", nameof(amount));

        await _lock.WaitAsync(cancellationToken);
        try
        {
            var result = await Task.Run(() =>
            {
                ulong dealId;
                var res = _manager!.DealerBalance(
                    login,
                    (double)amount,
                    (uint)CIMTDeal.EnDealAction.DEAL_BALANCE,
                    comment,
                    out dealId);

                if (res != MTRetCode.MT_RET_REQUEST_DONE && res != MTRetCode.MT_RET_OK)
                {
                    _logger.Error($"DealerBalance (deposit {login}, {amount}) failed: {res}");
                    return MT5Result.Failure(res, $"Deposit failed: {res}");
                }

                _logger.Information($"Deposit: login={login}, amount={amount}, dealId={dealId}");
                return MT5Result.Success($"Deposit successful, deal ID: {dealId}");
            }, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Deposit({login}, {amount}) error");
            return MT5Result.Failure($"Deposit error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<MT5Result> WithdrawAsync(ulong login, decimal amount, string comment, CancellationToken cancellationToken = default)
    {
        if (!IsConnected) return MT5Result.Failure("Not connected");
        if (amount <= 0) throw new ArgumentException("Amount must be positive", nameof(amount));

        await _lock.WaitAsync(cancellationToken);
        try
        {
            var result = await Task.Run(() =>
            {
                // Negative amount for withdrawal
                ulong dealId;
                var res = _manager!.DealerBalance(
                    login,
                    -(double)amount,
                    (uint)CIMTDeal.EnDealAction.DEAL_BALANCE,
                    comment,
                    out dealId);

                if (res != MTRetCode.MT_RET_REQUEST_DONE && res != MTRetCode.MT_RET_OK)
                {
                    _logger.Error($"DealerBalance (withdraw {login}, {amount}) failed: {res}");
                    return MT5Result.Failure(res, $"Withdraw failed: {res}");
                }

                _logger.Information($"Withdraw: login={login}, amount={amount}, dealId={dealId}");
                return MT5Result.Success($"Withdraw successful, deal ID: {dealId}");
            }, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Withdraw({login}, {amount}) error");
            return MT5Result.Failure($"Withdraw error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    #endregion

    #region Deal Operations

    public async Task<MT5Result<DealModel[]>> GetDealsAsync(ulong login, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await GetDealsAsync(login, SMTTime.FromDateTime(from), SMTTime.FromDateTime(to), cancellationToken);
    }

    public async Task<MT5Result<DealModel[]>> GetDealsAsync(ulong login, long from, long to, CancellationToken cancellationToken = default)
    {
        var comResult = await GetDealsComAsync(login, from, to, cancellationToken);
        if (!comResult.IsSuccess || comResult.Data == null)
            return MT5Result<DealModel[]>.Failure(comResult.RetCode, comResult.Message);

        try
        {
            var deals = new List<DealModel>();
            var dealArray = comResult.Data;

            for (uint i = 0; i < dealArray.Total(); i++)
            {
                var deal = dealArray.Next(i);
                if (deal != null)
                {
                    deals.Add(deal.ToModel());
                }
            }

            return MT5Result<DealModel[]>.Success(deals.ToArray());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"GetDealsAsync({login}) conversion error");
            return MT5Result<DealModel[]>.Failure($"Conversion error: {ex.Message}");
        }
    }

    public async Task<MT5Result<MT5Bridge.Manager.Models.Proto.DealModel[]>> GetDealsProtoAsync(ulong login, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await GetDealsProtoAsync(login, SMTTime.FromDateTime(from), SMTTime.FromDateTime(to), cancellationToken);
    }

    public async Task<MT5Result<MT5Bridge.Manager.Models.Proto.DealModel[]>> GetDealsProtoAsync(ulong login, long from, long to, CancellationToken cancellationToken = default)
    {
        var comResult = await GetDealsComAsync(login, from, to, cancellationToken);
        if (!comResult.IsSuccess || comResult.Data == null)
            return MT5Result<MT5Bridge.Manager.Models.Proto.DealModel[]>.Failure(comResult.RetCode, comResult.Message);

        try
        {
            var deals = new List<MT5Bridge.Manager.Models.Proto.DealModel>();
            var dealArray = comResult.Data;

            for (uint i = 0; i < dealArray.Total(); i++)
            {
                var deal = dealArray.Next(i);
                if (deal != null)
                {
                    deals.Add(deal.ToProto());
                }
            }

            return MT5Result<MT5Bridge.Manager.Models.Proto.DealModel[]>.Success(deals.ToArray());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"GetDealsProtoAsync({login}) conversion error");
            return MT5Result<MT5Bridge.Manager.Models.Proto.DealModel[]>.Failure($"Conversion error: {ex.Message}");
        }
    }

    public async Task<MT5Result<DealModel>> GetDealAsync(ulong ticket, CancellationToken cancellationToken = default)
    {
        var comResult = await GetDealComAsync(ticket, cancellationToken);
        if (!comResult.IsSuccess || comResult.Data == null)
            return MT5Result<DealModel>.Failure(comResult.RetCode, comResult.Message);

        try
        {
            using (comResult.Data)
            {
                return MT5Result<DealModel>.Success(comResult.Data.ToModel());
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"GetDealAsync({ticket}) conversion error");
            return MT5Result<DealModel>.Failure($"Conversion error: {ex.Message}");
        }
    }

    public async Task<MT5Result<MT5Bridge.Manager.Models.Proto.DealModel>> GetDealProtoAsync(ulong ticket, CancellationToken cancellationToken = default)
    {
        var comResult = await GetDealComAsync(ticket, cancellationToken);
        if (!comResult.IsSuccess || comResult.Data == null)
            return MT5Result<MT5Bridge.Manager.Models.Proto.DealModel>.Failure(comResult.RetCode, comResult.Message);

        try
        {
            using (comResult.Data)
            {
                return MT5Result<MT5Bridge.Manager.Models.Proto.DealModel>.Success(comResult.Data.ToProto());
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"GetDealProtoAsync({ticket}) conversion error");
            return MT5Result<MT5Bridge.Manager.Models.Proto.DealModel>.Failure($"Conversion error: {ex.Message}");
        }
    }

    public async Task<MT5Result<DealModel[]>> GetDealsAsync(DateTime from, DateTime to, string groupMask = "*", CancellationToken cancellationToken = default)
    {
        return await GetDealsAsync(SMTTime.FromDateTime(from), SMTTime.FromDateTime(to), groupMask, cancellationToken);
    }

    public async Task<MT5Result<DealModel[]>> GetDealsAsync(long from, long to, string groupMask = "*", CancellationToken cancellationToken = default)
    {
        var comResult = await GetDealsComAsync(groupMask, from, to, cancellationToken);
        if (!comResult.IsSuccess || comResult.Data == null)
            return MT5Result<DealModel[]>.Failure(comResult.RetCode, comResult.Message);

        try
        {
            var deals = new List<DealModel>();
            var dealArray = comResult.Data;

            for (uint i = 0; i < dealArray.Total(); i++)
            {
                var deal = dealArray.Next(i);
                if (deal != null)
                {
                    deals.Add(deal.ToModel());
                }
            }

            return MT5Result<DealModel[]>.Success(deals.ToArray());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"GetDealsAsync({groupMask}) conversion error");
            return MT5Result<DealModel[]>.Failure($"Conversion error: {ex.Message}");
        }
    }

    public async Task<MT5Result<MT5Bridge.Manager.Models.Proto.DealModel[]>> GetDealsProtoAsync(DateTime from, DateTime to, string groupMask = "*", CancellationToken cancellationToken = default)
    {
        return await GetDealsProtoAsync(SMTTime.FromDateTime(from), SMTTime.FromDateTime(to), groupMask, cancellationToken);
    }

    public async Task<MT5Result<MT5Bridge.Manager.Models.Proto.DealModel[]>> GetDealsProtoAsync(long from, long to, string groupMask = "*", CancellationToken cancellationToken = default)
    {
        var comResult = await GetDealsComAsync(groupMask, from, to, cancellationToken);
        if (!comResult.IsSuccess || comResult.Data == null)
            return MT5Result<MT5Bridge.Manager.Models.Proto.DealModel[]>.Failure(comResult.RetCode, comResult.Message);

        try
        {
            var deals = new List<MT5Bridge.Manager.Models.Proto.DealModel>();
            var dealArray = comResult.Data;

            for (uint i = 0; i < dealArray.Total(); i++)
            {
                var deal = dealArray.Next(i);
                if (deal != null)
                {
                    deals.Add(deal.ToProto());
                }
            }

            return MT5Result<MT5Bridge.Manager.Models.Proto.DealModel[]>.Success(deals.ToArray());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"GetDealsProtoAsync({groupMask}) conversion error");
            return MT5Result<MT5Bridge.Manager.Models.Proto.DealModel[]>.Failure($"Conversion error: {ex.Message}");
        }
    }

    public async Task<MT5Result<DealModel[]>> GetBalanceHistoryAsync(ulong login, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await GetBalanceHistoryAsync(login, SMTTime.FromDateTime(from), SMTTime.FromDateTime(to), cancellationToken);
    }

    public async Task<MT5Result<DealModel[]>> GetBalanceHistoryAsync(ulong login, long from, long to, CancellationToken cancellationToken = default)
    {
        var comResult = await GetBalanceHistoryComAsync(login, from, to, cancellationToken);
        if (!comResult.IsSuccess || comResult.Data == null)
            return MT5Result<DealModel[]>.Failure(comResult.RetCode, comResult.Message);

        try
        {
            var deals = new List<DealModel>();
            var dealArray = comResult.Data;

            for (uint i = 0; i < dealArray.Total(); i++)
            {
                var deal = dealArray.Next(i);
                if (deal != null && deal.Action() == (uint)CIMTDeal.EnDealAction.DEAL_BALANCE)
                {
                    deals.Add(deal.ToModel());
                }
            }

            return MT5Result<DealModel[]>.Success(deals.ToArray());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"GetBalanceHistoryAsync({login}) conversion error");
            return MT5Result<DealModel[]>.Failure($"Conversion error: {ex.Message}");
        }
    }

    public async Task<MT5Result<MT5Bridge.Manager.Models.Proto.DealModel[]>> GetBalanceHistoryProtoAsync(ulong login, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await GetBalanceHistoryProtoAsync(login, SMTTime.FromDateTime(from), SMTTime.FromDateTime(to), cancellationToken);
    }

    public async Task<MT5Result<MT5Bridge.Manager.Models.Proto.DealModel[]>> GetBalanceHistoryProtoAsync(ulong login, long from, long to, CancellationToken cancellationToken = default)
    {
        var comResult = await GetBalanceHistoryComAsync(login, from, to, cancellationToken);
        if (!comResult.IsSuccess || comResult.Data == null)
            return MT5Result<MT5Bridge.Manager.Models.Proto.DealModel[]>.Failure(comResult.RetCode, comResult.Message);

        try
        {
            var deals = new List<MT5Bridge.Manager.Models.Proto.DealModel>();
            var dealArray = comResult.Data;

            for (uint i = 0; i < dealArray.Total(); i++)
            {
                var deal = dealArray.Next(i);
                if (deal != null && deal.Action() == (uint)CIMTDeal.EnDealAction.DEAL_BALANCE)
                {
                    deals.Add(deal.ToProto());
                }
            }

            return MT5Result<MT5Bridge.Manager.Models.Proto.DealModel[]>.Success(deals.ToArray());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"GetBalanceHistoryProtoAsync({login}) conversion error");
            return MT5Result<MT5Bridge.Manager.Models.Proto.DealModel[]>.Failure($"Conversion error: {ex.Message}");
        }
    }

    private async Task<MT5Result<CIMTDealArray>> GetDealsComAsync(ulong login, long from, long to, CancellationToken cancellationToken = default)
    {
        if (!IsConnected) return MT5Result<CIMTDealArray>.Failure("Not connected");

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_dealArray == null)
            {
                return MT5Result<CIMTDealArray>.Failure("DealArray not initialized");
            }

            var result = await Task.Run(() =>
            {
                _dealArray.Clear();
                var res = _manager!.DealRequest(
                    login,
                    from,
                    to,
                    _dealArray);

                if (res != MTRetCode.MT_RET_OK)
                {
                    _logger.Error($"DealRequest({login}) failed: {res}");
                    return MT5Result<CIMTDealArray>.Failure(res, $"DealRequest failed: {res}");
                }

                _logger.Debug($"Retrieved {_dealArray.Total()} deals for user {login}");
                return MT5Result<CIMTDealArray>.Success(_dealArray);
            }, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"GetDealsCom({login}) error");
            return MT5Result<CIMTDealArray>.Failure($"GetDeals error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<MT5Result<CIMTDealArray>> GetDealsComAsync(ulong login, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await GetDealsComAsync(login, SMTTime.FromDateTime(from), SMTTime.FromDateTime(to), cancellationToken);
    }

    private async Task<MT5Result<CIMTDealArray>> GetDealsComAsync(string groupMask, long from, long to, CancellationToken cancellationToken = default)
    {
        if (!IsConnected) return MT5Result<CIMTDealArray>.Failure("Not connected");

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_dealArray == null)
            {
                return MT5Result<CIMTDealArray>.Failure("DealArray not initialized");
            }

            var result = await Task.Run(() =>
            {
                _dealArray.Clear();
                var res = _manager!.DealRequestByGroup(
                    groupMask,
                    from,
                    to,
                    _dealArray);

                if (res != MTRetCode.MT_RET_OK)
                {
                    _logger.Error($"DealRequestByGroup({groupMask}) failed: {res}");
                    return MT5Result<CIMTDealArray>.Failure(res, $"DealRequestByGroup failed: {res}");
                }

                _logger.Debug($"Retrieved {_dealArray.Total()} deals for group {groupMask}");
                return MT5Result<CIMTDealArray>.Success(_dealArray);
            }, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"GetDealsCom({groupMask}) error");
            return MT5Result<CIMTDealArray>.Failure($"GetDeals error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<MT5Result<CIMTDealArray>> GetDealsComAsync(string groupMask, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await GetDealsComAsync(groupMask, SMTTime.FromDateTime(from), SMTTime.FromDateTime(to), cancellationToken);
    }

    private async Task<MT5Result<CIMTDeal>> GetDealComAsync(ulong ticket, CancellationToken cancellationToken = default)
    {
        if (!IsConnected) return MT5Result<CIMTDeal>.Failure("Not connected");

        await _lock.WaitAsync(cancellationToken);
        try
        {
            var deal = _manager!.DealCreate();
            if (deal == null)
            {
                return MT5Result<CIMTDeal>.Failure("Failed to create deal object");
            }

            var result = await Task.Run(() =>
            {
                var res = _manager.DealRequest(ticket, deal);

                if (res != MTRetCode.MT_RET_OK)
                {
                    _logger.Error($"DealRequest({ticket}) failed: {res}");
                    deal.Dispose();
                    return MT5Result<CIMTDeal>.Failure(res, $"DealRequest failed: {res}");
                }

                _logger.Debug($"Retrieved deal: {ticket}");
                return MT5Result<CIMTDeal>.Success(deal);
            }, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"GetDealCom({ticket}) error");
            return MT5Result<CIMTDeal>.Failure($"GetDeal error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<MT5Result<CIMTDealArray>> GetBalanceHistoryComAsync(ulong login, long from, long to, CancellationToken cancellationToken = default)
    {
        // Re-use GetDealsComAsync logic
        return await GetDealsComAsync(login, from, to, cancellationToken);
    }

    private async Task<MT5Result<CIMTDealArray>> GetBalanceHistoryComAsync(ulong login, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        // Re-use GetDealsComAsync logic
        return await GetDealsComAsync(login, from, to, cancellationToken);
    }

    #endregion

    #region Order & Position Operations

    public async Task<MT5Result<OrderModel[]>> GetOrdersAsync(ulong login, CancellationToken cancellationToken = default)
    {
        var comResult = await GetOrdersComAsync(login, cancellationToken);
        if (!comResult.IsSuccess || comResult.Data == null)
            return MT5Result<OrderModel[]>.Failure(comResult.RetCode, comResult.Message);

        try
        {
            var orders = new List<OrderModel>();
            var orderArray = comResult.Data;

            for (uint i = 0; i < orderArray.Total(); i++)
            {
                var order = orderArray.Next(i);
                if (order != null)
                {
                    orders.Add(order.ToModel());
                }
            }

            return MT5Result<OrderModel[]>.Success(orders.ToArray());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "GetOrdersAsync conversion error");
            return MT5Result<OrderModel[]>.Failure($"Conversion error: {ex.Message}");
        }
    }

    public async Task<MT5Result<ProtoOrder[]>> GetOrdersProtoAsync(ulong login, CancellationToken cancellationToken = default)
    {
        var comResult = await GetOrdersComAsync(login, cancellationToken);
        if (!comResult.IsSuccess || comResult.Data == null)
            return MT5Result<ProtoOrder[]>.Failure(comResult.RetCode, comResult.Message);

        try
        {
            var orders = new List<ProtoOrder>();
            var orderArray = comResult.Data;

            for (uint i = 0; i < orderArray.Total(); i++)
            {
                var order = orderArray.Next(i);
                if (order != null)
                {
                    orders.Add(MT5Bridge.Manager.Models.Proto.OrderProtoExtensions.ToProto(order));
                }
            }

            return MT5Result<ProtoOrder[]>.Success(orders.ToArray());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "GetOrdersProtoAsync conversion error");
            return MT5Result<ProtoOrder[]>.Failure($"Conversion error: {ex.Message}");
        }
    }

    public async Task<MT5Result<OrderModel[]>> GetHistoryOrdersAsync(ulong login, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await GetHistoryOrdersAsync(login, SMTTime.FromDateTime(from), SMTTime.FromDateTime(to), cancellationToken);
    }

    public async Task<MT5Result<OrderModel[]>> GetHistoryOrdersAsync(ulong login, long from, long to, CancellationToken cancellationToken = default)
    {
        var comResult = await GetHistoryOrdersComAsync(login, from, to, cancellationToken);
        if (!comResult.IsSuccess || comResult.Data == null)
            return MT5Result<OrderModel[]>.Failure(comResult.RetCode, comResult.Message);

        try
        {
            var orders = new List<OrderModel>();
            var orderArray = comResult.Data;

            for (uint i = 0; i < orderArray.Total(); i++)
            {
                var order = orderArray.Next(i);
                if (order != null)
                {
                    orders.Add(order.ToModel());
                }
            }

            return MT5Result<OrderModel[]>.Success(orders.ToArray());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"GetHistoryOrdersAsync({login}) conversion error");
            return MT5Result<OrderModel[]>.Failure($"Conversion error: {ex.Message}");
        }
    }

    public async Task<MT5Result<MT5Bridge.Manager.Models.Proto.OrderModel[]>> GetHistoryOrdersProtoAsync(ulong login, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await GetHistoryOrdersProtoAsync(login, SMTTime.FromDateTime(from), SMTTime.FromDateTime(to), cancellationToken);
    }

    public async Task<MT5Result<MT5Bridge.Manager.Models.Proto.OrderModel[]>> GetHistoryOrdersProtoAsync(ulong login, long from, long to, CancellationToken cancellationToken = default)
    {
        var comResult = await GetHistoryOrdersComAsync(login, from, to, cancellationToken);
        if (!comResult.IsSuccess || comResult.Data == null)
            return MT5Result<MT5Bridge.Manager.Models.Proto.OrderModel[]>.Failure(comResult.RetCode, comResult.Message);

        try
        {
            var orders = new List<MT5Bridge.Manager.Models.Proto.OrderModel>();
            var orderArray = comResult.Data;

            for (uint i = 0; i < orderArray.Total(); i++)
            {
                var order = orderArray.Next(i);
                if (order != null)
                {
                    orders.Add(MT5Bridge.Manager.Models.Proto.OrderProtoExtensions.ToProto(order));
                }
            }

            return MT5Result<MT5Bridge.Manager.Models.Proto.OrderModel[]>.Success(orders.ToArray());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"GetHistoryOrdersProtoAsync({login}) conversion error");
            return MT5Result<MT5Bridge.Manager.Models.Proto.OrderModel[]>.Failure($"Conversion error: {ex.Message}");
        }
    }

    public async Task<MT5Result<OrderModel[]>> GetHistoryOrdersAsync(DateTime from, DateTime to, string groupMask = "*", CancellationToken cancellationToken = default)
    {
        return await GetHistoryOrdersAsync(SMTTime.FromDateTime(from), SMTTime.FromDateTime(to), groupMask, cancellationToken);
    }

    public async Task<MT5Result<OrderModel[]>> GetHistoryOrdersAsync(long from, long to, string groupMask = "*", CancellationToken cancellationToken = default)
    {
        var comResult = await GetHistoryOrdersComAsync(groupMask, from, to, cancellationToken);
        if (!comResult.IsSuccess || comResult.Data == null)
            return MT5Result<OrderModel[]>.Failure(comResult.RetCode, comResult.Message);

        try
        {
            var orders = new List<OrderModel>();
            var orderArray = comResult.Data;

            for (uint i = 0; i < orderArray.Total(); i++)
            {
                var order = orderArray.Next(i);
                if (order != null)
                {
                    orders.Add(order.ToModel());
                }
            }

            return MT5Result<OrderModel[]>.Success(orders.ToArray());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"GetHistoryOrdersAsync({groupMask}) conversion error");
            return MT5Result<OrderModel[]>.Failure($"Conversion error: {ex.Message}");
        }
    }

    public async Task<MT5Result<MT5Bridge.Manager.Models.Proto.OrderModel[]>> GetHistoryOrdersProtoAsync(DateTime from, DateTime to, string groupMask = "*", CancellationToken cancellationToken = default)
    {
        return await GetHistoryOrdersProtoAsync(SMTTime.FromDateTime(from), SMTTime.FromDateTime(to), groupMask, cancellationToken);
    }

    public async Task<MT5Result<MT5Bridge.Manager.Models.Proto.OrderModel[]>> GetHistoryOrdersProtoAsync(long from, long to, string groupMask = "*", CancellationToken cancellationToken = default)
    {
        var comResult = await GetHistoryOrdersComAsync(groupMask, from, to, cancellationToken);
        if (!comResult.IsSuccess || comResult.Data == null)
            return MT5Result<MT5Bridge.Manager.Models.Proto.OrderModel[]>.Failure(comResult.RetCode, comResult.Message);

        try
        {
            var orders = new List<MT5Bridge.Manager.Models.Proto.OrderModel>();
            var orderArray = comResult.Data;

            for (uint i = 0; i < orderArray.Total(); i++)
            {
                var order = orderArray.Next(i);
                if (order != null)
                {
                    orders.Add(MT5Bridge.Manager.Models.Proto.OrderProtoExtensions.ToProto(order));
                }
            }

            return MT5Result<MT5Bridge.Manager.Models.Proto.OrderModel[]>.Success(orders.ToArray());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"GetHistoryOrdersProtoAsync({groupMask}) conversion error");
            return MT5Result<MT5Bridge.Manager.Models.Proto.OrderModel[]>.Failure($"Conversion error: {ex.Message}");
        }
    }

    public async Task<MT5Result<PositionModel[]>> GetPositionsAsync(ulong login, CancellationToken cancellationToken = default)
    {
        var comResult = await GetPositionsComAsync(login, cancellationToken);
        if (!comResult.IsSuccess || comResult.Data == null)
            return MT5Result<PositionModel[]>.Failure(comResult.RetCode, comResult.Message);

        try
        {
            var positions = new List<PositionModel>();
            var positionArray = comResult.Data;

            for (uint i = 0; i < positionArray.Total(); i++)
            {
                var position = positionArray.Next(i);
                if (position != null)
                {
                    positions.Add(position.ToModel());
                }
            }

            return MT5Result<PositionModel[]>.Success(positions.ToArray());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "GetPositionsAsync conversion error");
            return MT5Result<PositionModel[]>.Failure($"Conversion error: {ex.Message}");
        }
    }

    public async Task<MT5Result<ProtoPosition[]>> GetPositionsProtoAsync(ulong login, CancellationToken cancellationToken = default)
    {
        var comResult = await GetPositionsComAsync(login, cancellationToken);
        if (!comResult.IsSuccess || comResult.Data == null)
            return MT5Result<ProtoPosition[]>.Failure(comResult.RetCode, comResult.Message);

        try
        {
            var positions = new List<ProtoPosition>();
            var positionArray = comResult.Data;

            for (uint i = 0; i < positionArray.Total(); i++)
            {
                var position = positionArray.Next(i);
                if (position != null)
                {
                    positions.Add(MT5Bridge.Manager.Models.Proto.PositionProtoExtensions.ToProto(position));
                }
            }

            return MT5Result<ProtoPosition[]>.Success(positions.ToArray());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "GetPositionsProtoAsync conversion error");
            return MT5Result<ProtoPosition[]>.Failure($"Conversion error: {ex.Message}");
        }
    }

    private async Task<MT5Result<CIMTOrderArray>> GetOrdersComAsync(ulong login, CancellationToken cancellationToken = default)
    {
        if (!IsConnected) return MT5Result<CIMTOrderArray>.Failure("Not connected");

        await _lock.WaitAsync(cancellationToken);
        try
        {
            return await Task.Run(() =>
            {
                _orderArray!.Clear();
                var res = _manager!.OrderRequestOpen(login, _orderArray);
                if (res != MTRetCode.MT_RET_OK)
                {
                    _logger.Error($"OrderRequestOpen({login}) failed: {res}");
                    return MT5Result<CIMTOrderArray>.Failure(res, $"OrderRequestOpen failed: {res}");
                }
                return MT5Result<CIMTOrderArray>.Success(_orderArray);
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"GetOrdersCom({login}) error");
            return MT5Result<CIMTOrderArray>.Failure($"GetOrders error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<MT5Result<CIMTOrderArray>> GetHistoryOrdersComAsync(ulong login, long from, long to, CancellationToken cancellationToken = default)
    {
        if (!IsConnected) return MT5Result<CIMTOrderArray>.Failure("Not connected");

        await _lock.WaitAsync(cancellationToken);
        try
        {
            return await Task.Run(() =>
            {
                _orderArray!.Clear();
                var res = _manager!.HistoryRequest(
                    login,
                    from,
                    to,
                    _orderArray);

                if (res != MTRetCode.MT_RET_OK)
                {
                    _logger.Error($"HistoryRequest({login}) failed: {res}");
                    return MT5Result<CIMTOrderArray>.Failure(res, $"HistoryRequest failed: {res}");
                }

                _logger.Debug($"Retrieved {_orderArray.Total()} history orders for user {login}");
                return MT5Result<CIMTOrderArray>.Success(_orderArray);
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"GetHistoryOrdersCom({login}) error");
            return MT5Result<CIMTOrderArray>.Failure($"GetHistoryOrders error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<MT5Result<CIMTOrderArray>> GetHistoryOrdersComAsync(ulong login, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await GetHistoryOrdersComAsync(login, SMTTime.FromDateTime(from), SMTTime.FromDateTime(to), cancellationToken);
    }

    private async Task<MT5Result<CIMTOrderArray>> GetHistoryOrdersComAsync(string groupMask, long from, long to, CancellationToken cancellationToken = default)
    {
        if (!IsConnected) return MT5Result<CIMTOrderArray>.Failure("Not connected");

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_orderArray == null)
            {
                return MT5Result<CIMTOrderArray>.Failure("OrderArray not initialized");
            }

            var result = await Task.Run(() =>
            {
                _orderArray.Clear();
                var res = _manager!.HistoryRequestByGroup(
                    groupMask,
                    from,
                    to,
                    _orderArray);

                if (res != MTRetCode.MT_RET_OK)
                {
                    _logger.Error($"HistoryRequestByGroup({groupMask}) failed: {res}");
                    return MT5Result<CIMTOrderArray>.Failure(res, $"HistoryRequestByGroup failed: {res}");
                }

                _logger.Debug($"Retrieved {_orderArray.Total()} history orders for group {groupMask}");
                return MT5Result<CIMTOrderArray>.Success(_orderArray);
            }, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"GetHistoryOrdersCom({groupMask}) error");
            return MT5Result<CIMTOrderArray>.Failure($"GetHistoryOrders error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<MT5Result<CIMTOrderArray>> GetHistoryOrdersComAsync(string groupMask, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await GetHistoryOrdersComAsync(groupMask, SMTTime.FromDateTime(from), SMTTime.FromDateTime(to), cancellationToken);
    }

    private async Task<MT5Result<CIMTPositionArray>> GetPositionsComAsync(ulong login, CancellationToken cancellationToken = default)
    {
        if (!IsConnected) return MT5Result<CIMTPositionArray>.Failure("Not connected");

        await _lock.WaitAsync(cancellationToken);
        try
        {
            return await Task.Run(() =>
            {
                _positionArray!.Clear();
                var res = _manager!.PositionRequest(login, _positionArray);
                if (res != MTRetCode.MT_RET_OK)
                {
                    _logger.Error($"PositionRequest({login}) failed: {res}");
                    return MT5Result<CIMTPositionArray>.Failure(res, $"PositionRequest failed: {res}");
                }
                return MT5Result<CIMTPositionArray>.Success(_positionArray);
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"GetPositionsCom({login}) error");
            return MT5Result<CIMTPositionArray>.Failure($"GetPositions error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    #endregion

    private MT5Result InitializeManager()
    {
        try
        {
            MTRetCode res;
            lock (_factoryLock)
            {
                // Initialize API factory
                res = SMTManagerAPIFactory.Initialize(null);
                if (res != MTRetCode.MT_RET_OK)
                {
                    return MT5Result.Failure(res, $"SMTManagerAPIFactory.Initialize failed: {res}");
                }
            }

            // Create manager
            _manager = SMTManagerAPIFactory.CreateManager(SMTManagerAPIFactory.ManagerAPIVersion, out res);
            if (res != MTRetCode.MT_RET_OK || _manager == null)
            {
                SMTManagerAPIFactory.Shutdown();
                return MT5Result.Failure(res, $"CreateManager failed: {res}");
            }

            // Create helper objects
            _dealArray = _manager.DealCreateArray();
            if (_dealArray == null)
            {
                return MT5Result.Failure("DealCreateArray failed");
            }

            _user = _manager.UserCreate();
            if (_user == null)
            {
                return MT5Result.Failure("UserCreate failed");
            }

            _account = _manager.UserCreateAccount();
            if (_account == null)
            {
                return MT5Result.Failure("UserCreateAccount failed");
            }

            _group = _manager.GroupCreate();
            if (_group == null)
            {
                return MT5Result.Failure("GroupCreate failed");
            }

            _groupArray = _manager.GroupCreateArray();
            if (_groupArray == null)
            {
                return MT5Result.Failure("GroupCreateArray failed");
            }

            _userArray = _manager.UserCreateArray();
            if (_userArray == null)
            {
                return MT5Result.Failure("UserCreateArray failed");
            }

            _orderArray = _manager.OrderCreateArray();
            if (_orderArray == null)
            {
                return MT5Result.Failure("OrderCreateArray failed");
            }

            _positionArray = _manager.PositionCreateArray();
            if (_positionArray == null)
            {
                return MT5Result.Failure("PositionCreateArray failed");
            }

            // Initialize manager sink for connection events
            _managerSink = new MT5ManagerSink(() =>
            {
                if (_state == ConnectionState.Connected)
                {
                    _logger.Warning("MT5 Server disconnected (detected via ManagerSink)");
                    SetState(ConnectionState.Disconnected, "Server disconnected");
                }
            });

            _logger.Information("MT5 Manager initialized");
            return MT5Result.Success();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "InitializeManager error");
            return MT5Result.Failure($"InitializeManager error: {ex.Message}");
        }
    }

    private void SetState(ConnectionState newState, string? message = null)
    {
        var oldState = _state;
        _state = newState;

        if (oldState != newState)
        {
            _logger.Debug($"Connection state: {oldState} -> {newState}");
            ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(oldState, newState, message));
        }
    }

    /// <summary>
    /// Finalizer to ensure cleanup if Dispose is not called
    /// </summary>
    ~MT5Manager()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected dispose method implementing the standard Dispose pattern
    /// </summary>
    /// <param name="disposing">True if called from Dispose(), false from finalizer</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        _disposed = true;

        if (disposing)
        {
            // Dispose managed resources
            try
            {
                Disconnect();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error during disconnect: {ex.Message}");
            }

            // Dispose managed COM objects
            _dealArray?.Dispose();
            _dealArray = null;

            _user?.Dispose();
            _user = null;

            _userArray?.Dispose();
            _userArray = null;

            _account?.Dispose();
            _account = null;

            _group?.Dispose();
            _group = null;

            _groupArray?.Dispose();
            _groupArray = null;

            _orderArray?.Dispose();
            _orderArray = null;

            _positionArray?.Dispose();
            _positionArray = null;

            _managerSink?.Dispose();
            _managerSink = null;

            _manager?.Dispose();
            _manager = null;

            // Dispose generic sinks
            foreach (var sink in _genericSinks)
            {
                if (sink is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            _genericSinks.Clear();

            lock (_factoryLock)
            {
                _instanceCount--;
                if (_instanceCount <= 0)
                {
                    SMTManagerAPIFactory.Shutdown();
                    _instanceCount = 0;
                }
            }
            _lock.Dispose();

            _logger.Information("MT5Manager disposed");
        }
    }
}

