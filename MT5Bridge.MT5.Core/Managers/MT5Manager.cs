using MetaQuotes.MT5CommonAPI;
using MetaQuotes.MT5ManagerAPI;
using MT5Bridge.Core.Logging;

namespace MT5Bridge.MT5.Core.Managers;

/// <summary>
/// MT5 Manager implementation
/// Thread-safe wrapper around CIMTManagerAPI
/// </summary>
public class MT5Manager : IMT5Manager
{
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private CIMTManagerAPI? _manager;
    private CIMTDealArray? _dealArray;
    private CIMTUser? _user;
    private CIMTAccount? _account;
    private CIMTConGroup? _group;
    private CIMTConGroupArray? _groupArray;

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
    }

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

                _logger.Info($"MT5 connected: {settings.Server}, login={settings.Login}");
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
            _logger.Error("Connect error", ex);
            SetState(ConnectionState.Failed, ex.Message);
            return MT5Result.Failure($"Connect error: {ex.Message}");
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
            if (_manager != null && _state == ConnectionState.Connected)
            {
                await Task.Run(() => _manager.Disconnect());
                _logger.Info("MT5 disconnected");
            }
            SetState(ConnectionState.Disconnected);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<MT5Result<CIMTConGroupArray>> GetGroupsAsync(CancellationToken cancellationToken = default)
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
            _logger.Error("GetGroups error", ex);
            return MT5Result<CIMTConGroupArray>.Failure($"GetGroups error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<MT5Result<CIMTConGroup>> GetGroupAsync(string groupName, CancellationToken cancellationToken = default)
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
            _logger.Error($"GetGroup({groupName}) error", ex);
            return MT5Result<CIMTConGroup>.Failure($"GetGroup error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<MT5Result<CIMTUser>> GetUserAsync(ulong login, CancellationToken cancellationToken = default)
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
            _logger.Error($"GetUser({login}) error", ex);
            return MT5Result<CIMTUser>.Failure($"GetUser error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<MT5Result<CIMTUser>> CreateUserAsync(CIMTUser user, string masterPassword, string investorPassword, CancellationToken cancellationToken = default)
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

                _logger.Info($"User created: {user.Login()}");
                return MT5Result<CIMTUser>.Success(user);
            }, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            _logger.Error("CreateUser error", ex);
            return MT5Result<CIMTUser>.Failure($"CreateUser error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<MT5Result<CIMTUser>> UpdateUserAsync(CIMTUser user, CancellationToken cancellationToken = default)
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

                _logger.Info($"User updated: {user.Login()}");
                return MT5Result<CIMTUser>.Success(user);
            }, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            _logger.Error("UpdateUser error", ex);
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

                _logger.Info($"User deleted: {login}");
                return MT5Result.Success();
            }, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            _logger.Error($"DeleteUser({login}) error", ex);
            return MT5Result.Failure($"DeleteUser error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<MT5Result<CIMTAccount>> GetAccountAsync(ulong login, CancellationToken cancellationToken = default)
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
            _logger.Error($"GetAccount({login}) error", ex);
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

                _logger.Info($"Deposit: login={login}, amount={amount}, dealId={dealId}");
                return MT5Result.Success($"Deposit successful, deal ID: {dealId}");
            }, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            _logger.Error($"Deposit({login}, {amount}) error", ex);
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

                _logger.Info($"Withdraw: login={login}, amount={amount}, dealId={dealId}");
                return MT5Result.Success($"Withdraw successful, deal ID: {dealId}");
            }, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            _logger.Error($"Withdraw({login}, {amount}) error", ex);
            return MT5Result.Failure($"Withdraw error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<MT5Result<CIMTDealArray>> GetDealsAsync(ulong login, DateTime from, DateTime to, CancellationToken cancellationToken = default)
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
                    SMTTime.FromDateTime(from),
                    SMTTime.FromDateTime(to),
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
            _logger.Error($"GetDeals({login}) error", ex);
            return MT5Result<CIMTDealArray>.Failure($"GetDeals error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<MT5Result<CIMTDeal>> GetDealAsync(ulong ticket, CancellationToken cancellationToken = default)
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
            _logger.Error($"GetDeal({ticket}) error", ex);
            return MT5Result<CIMTDeal>.Failure($"GetDeal error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    private MT5Result InitializeManager()
    {
        try
        {
            // Initialize API factory
            var res = SMTManagerAPIFactory.Initialize(null);
            if (res != MTRetCode.MT_RET_OK)
            {
                return MT5Result.Failure(res, $"SMTManagerAPIFactory.Initialize failed: {res}");
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

            _logger.Info("MT5 Manager initialized");
            return MT5Result.Success();
        }
        catch (Exception ex)
        {
            _logger.Error("InitializeManager error", ex);
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

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        DisconnectAsync().GetAwaiter().GetResult();

        _dealArray?.Dispose();
        _user?.Dispose();
        _account?.Dispose();
        _group?.Dispose();
        _groupArray?.Dispose();
        _manager?.Dispose();

        SMTManagerAPIFactory.Shutdown();
        _lock.Dispose();

        _logger.Info("MT5Manager disposed");
    }
}
