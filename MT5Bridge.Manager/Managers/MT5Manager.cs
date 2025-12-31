using MetaQuotes.MT5CommonAPI;
using MetaQuotes.MT5ManagerAPI;
using Serilog;
using MT5Bridge.Manager.Sinks;
using MT5Bridge.Manager.Reconnection;
using System.Diagnostics.CodeAnalysis;

// POCO models - use aliases to avoid conflicts
using UserModel = MT5Bridge.Manager.Models.UserModel;
using AccountModel = MT5Bridge.Manager.Models.AccountModel;
using DealModel = MT5Bridge.Manager.Models.DealModel;
using OrderModel = MT5Bridge.Manager.Models.OrderModel;
using PositionModel = MT5Bridge.Manager.Models.PositionModel;

// Protobuf models - use aliases
using ProtoUser = MT5Bridge.Manager.Models.Proto.UserModel;
using ProtoAccount = MT5Bridge.Manager.Models.Proto.AccountModel;
using ProtoDeal = MT5Bridge.Manager.Models.Proto.DealModel;
using ProtoOrder = MT5Bridge.Manager.Models.Proto.OrderModel;
using ProtoPosition = MT5Bridge.Manager.Models.Proto.PositionModel;

namespace MT5Bridge.Manager.Managers;

/// <summary>
/// MT5 Manager implementation
/// Thread-safe wrapper around CIMTManagerAPI
/// </summary>
public partial class MT5Manager : IMT5Manager
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
    private MT5GenericManagerSink<UserModel, AccountModel, OrderModel, PositionModel>? _managerSinkPoco;

    [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")]
    private MT5GenericManagerSink<ProtoUser, ProtoAccount, ProtoOrder, ProtoPosition>? _managerSinkProto;

    [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")]
    private MT5GenericDealSink<DealModel, AccountModel, PositionModel>? _dealSinkPoco;

    [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")]
    private MT5GenericDealSink<ProtoDeal, ProtoAccount, ProtoPosition>? _dealSinkProto;

    [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")]
    private MT5GenericOrderSink<OrderModel>? _orderSinkPoco;

    [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")]
    private MT5GenericOrderSink<ProtoOrder>? _orderSinkProto;

    [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")]
    private MT5GenericPositionSink<PositionModel>? _positionSinkPoco;

    [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")]
    private MT5GenericPositionSink<ProtoPosition>? _positionSinkProto;

    // Reconnection management
    private ReconnectExecutor? _reconnectExecutor;
    private CancellationTokenSource? _disconnectCancellationTokenSource;
    private MTRetCode _lastConnectionError = MTRetCode.MT_RET_OK;

    private ConnectionState _state = ConnectionState.Disconnected;
    private MT5ConnectionSettings? _settings;
    private bool _disposed;

    public bool IsConnected => _state == ConnectionState.Connected;
    public ConnectionState State => _state;
    public MT5ConnectionSettings? Settings => _settings;

    public event EventHandler<ConnectionStateChangedEventArgs>? ConnectionStateChanged;

    public MT5Manager(ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
        lock (_factoryLock)
        {
            _instanceCount++;
        }
    }



    public async Task<MT5Result> ConnectAsync(MT5ConnectionSettings settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_state == ConnectionState.Connected)
            {
                return MT5Result.Success("Already connected");
            }

            SetState(ConnectionState.Connecting);
            _settings = settings;

            // Ensure manager is initialized
            var initResult = EnsureInitialized();
            if (!initResult.IsSuccess)
            {
                SetState(ConnectionState.Failed, initResult.Message);
                return initResult;
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
                var res = _manager!.Connect(
                    settings.Server,
                    settings.Login,
                    settings.Password,
                    null,
                    pumpMode,
                    settings.TimeoutMs);

                if (res != MTRetCode.MT_RET_OK)
                {
                    _lastConnectionError = res;
                    _logger.Error($"MT5 connection failed: {res}");
                    return MT5Result.Failure(res, $"Connection failed: {res}");
                }

                _logger.Information($"MT5 connected: {settings.Server}, login={settings.Login}");
                return MT5Result.Success("Connected successfully");
            }, cancellationToken).ConfigureAwait(false);

            // FIXED C1: State changes now happen inside the lock
            if (result.IsSuccess)
            {
                SetState(ConnectionState.Connected);
            }
            else
            {
                SetState(ConnectionState.Failed, result.Message);
                // Note: Application is responsible for calling StartAutoReconnectAsync() if needed
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

    private MT5Result EnsureInitialized()
    {
        if (_manager == null)
        {
            var result = InitializeManager();
            if (!result.IsSuccess)
            {
                return result;
            }
        }
        return MT5Result.Success();
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
        await _lock.WaitAsync().ConfigureAwait(false);
        try
        {
            await Task.Run(DisconnectInternal).ConfigureAwait(false);
        }
        finally
        {
            _lock.Release();
        }
    }

    private void DisconnectInternal()
    {
        // Stop any ongoing reconnection
        _reconnectExecutor?.StopReconnect();

        if (_manager != null && _state == ConnectionState.Connected)
        {
            try
            {
                // Unsubscribe all sinks
                UnsubscribeAllSinks();

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

            // Initialize reconnect executor
            _reconnectExecutor ??= new ReconnectExecutor(_logger);

            _logger.Information("MT5 Manager initialized");
            return MT5Result.Success();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "InitializeManager error");
            return MT5Result.Failure($"InitializeManager error: {ex.Message}");
        }
    }

    /// <summary>
    /// Start automatic reconnection (application calls this when needed)
    /// </summary>
    public async Task StartAutoReconnectAsync()
    {
        if (_settings == null || _manager == null || _reconnectExecutor == null)
            return;

        try
        {
            var strategy = _settings.GetReconnectStrategy();
            if (strategy == null)
            {
                _logger.Warning("Reconnection disabled");
                return;
            }

            SetState(ConnectionState.Reconnecting);

            await _reconnectExecutor.StartReconnectAsync(
                strategy,
                async token =>
                {
                    try
                    {
                        return await ConnectAsync(_settings, token);
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Reconnection attempt failed");
                        return MT5Result.Failure($"Reconnection failed: {ex.Message}");
                    }
                },
                _lastConnectionError);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error starting auto-reconnect");
            SetState(ConnectionState.Failed, $"Auto-reconnect error: {ex.Message}");
        }
    }

    private void HandleServerDisconnect()
    {
        _logger.Warning("Server disconnected event received from sink");
        SetState(ConnectionState.Disconnected, "Server disconnected");
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
            // Stop reconnection before disconnecting
            try
            {
                _reconnectExecutor?.StopReconnect();
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Error stopping reconnect executor");
            }

            // FIXED C2: Use timeout to prevent deadlock when disposing
            // Try to acquire lock with timeout to avoid deadlock if called from event handler
            bool lockAcquired = false;
            try
            {
                lockAcquired = _lock.Wait(TimeSpan.FromSeconds(5));

                if (lockAcquired)
                {
                    try
                    {
                        DisconnectInternal();
                    }
                    finally
                    {
                        _lock.Release();
                    }
                }
                else
                {
                    _logger.Warning("Could not acquire lock during disposal within timeout, forcing disconnect");
                    // Force disconnect without lock - risky but prevents deadlock
                    try
                    {
                        DisconnectInternal();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Error during forced disconnect in disposal");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during disconnect in disposal");
            }

            // Dispose reconnect executor
            _reconnectExecutor = null;
            _disconnectCancellationTokenSource?.Dispose();
            _disconnectCancellationTokenSource = null;

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

            _managerSinkPoco?.Dispose();
            _managerSinkPoco = null;

            _managerSinkProto?.Dispose();
            _managerSinkProto = null;

            _dealSinkPoco?.Dispose();
            _dealSinkPoco = null;

            _dealSinkProto?.Dispose();
            _dealSinkProto = null;

            _orderSinkPoco?.Dispose();
            _orderSinkPoco = null;

            _orderSinkProto?.Dispose();
            _orderSinkProto = null;

            _positionSinkPoco?.Dispose();
            _positionSinkPoco = null;

            _positionSinkProto?.Dispose();
            _positionSinkProto = null;

            _manager?.Dispose();
            _manager = null;

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

