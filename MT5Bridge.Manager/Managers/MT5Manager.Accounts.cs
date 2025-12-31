using MT5Bridge.Manager.Models;
using MetaQuotes.MT5CommonAPI;

// Proto extensions
using MT5Bridge.Manager.Models.Proto;

// POCO models
using AccountModel = MT5Bridge.Manager.Models.AccountModel;

// Protobuf models
using ProtoAccount = MT5Bridge.Manager.Models.Proto.AccountModel;

namespace MT5Bridge.Manager.Managers;

public partial class MT5Manager
{
    #region Account Operations

    public async Task<MT5Result<AccountModel>> GetAccountAsync(ulong login, CancellationToken cancellationToken = default)
    {
        return await GetAccountInternalAsync(login, a => a.ToModel(), cancellationToken).ConfigureAwait(false);
    }

    public async Task<MT5Result<ProtoAccount>> GetAccountProtoAsync(ulong login, CancellationToken cancellationToken = default)
    {
        return await GetAccountInternalAsync(login, a => a.ToProto(), cancellationToken).ConfigureAwait(false);
    }

    private async Task<MT5Result<T>> GetAccountInternalAsync<T>(ulong login, Func<CIMTAccount, T> converter, CancellationToken cancellationToken = default)
    {
        if (!IsConnected) return MT5Result<T>.Failure("Not connected");

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_account == null)
            {
                return MT5Result<T>.Failure("Account object not initialized");
            }

            return await Task.Run(() =>
            {
                _account.Clear();
                var res = _manager!.UserAccountRequest(login, _account);

                if (res != MTRetCode.MT_RET_OK)
                {
                    _logger.Error($"UserAccountRequest({login}) failed: {res}");
                    return MT5Result<T>.Failure(res, $"UserAccountRequest failed: {res}");
                }

                _logger.Debug($"Retrieved account: {login}, balance={_account.Balance()}");
                return MT5Result<T>.Success(converter(_account));
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"GetAccount error for {login}");
            return MT5Result<T>.Failure($"GetAccount error: {ex.Message}");
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

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
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

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
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
}
