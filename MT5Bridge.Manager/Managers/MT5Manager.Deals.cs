using MT5Bridge.Manager.Models;
using MT5Bridge.Core.Collections;
using MetaQuotes.MT5CommonAPI;

// Proto extensions
using MT5Bridge.Manager.Models.Proto;

// POCO models
using DealModel = MT5Bridge.Manager.Models.DealModel;

// Protobuf models
using ProtoDeal = MT5Bridge.Manager.Models.Proto.DealModel;

namespace MT5Bridge.Manager.Managers;

public partial class MT5Manager
{
    #region Deal Operations

    public async Task<MT5Result<DealModel[]>> GetDealsAsync(ulong login, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await GetDealsAsync(login, SMTTime.FromDateTime(from), SMTTime.FromDateTime(to), cancellationToken).ConfigureAwait(false);
    }

    public async Task<MT5Result<DealModel[]>> GetDealsAsync(ulong login, long from, long to, CancellationToken cancellationToken = default)
    {
        return await GetDealsInternalAsync(
            arr => _manager!.DealRequest(login, from, to, arr),
            d => d.ToModel(),
            null,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<MT5Result<ProtoDeal[]>> GetDealsProtoAsync(ulong login, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await GetDealsProtoAsync(login, SMTTime.FromDateTime(from), SMTTime.FromDateTime(to), cancellationToken).ConfigureAwait(false);
    }

    public async Task<MT5Result<ProtoDeal[]>> GetDealsProtoAsync(ulong login, long from, long to, CancellationToken cancellationToken = default)
    {
        return await GetDealsInternalAsync(
            arr => _manager!.DealRequest(login, from, to, arr),
            d => d.ToProto(),
            null,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<MT5Result<DealModel>> GetDealAsync(ulong ticket, CancellationToken cancellationToken = default)
    {
        return await GetDealInternalAsync(ticket, d => d.ToModel(), cancellationToken).ConfigureAwait(false);
    }

    public async Task<MT5Result<ProtoDeal>> GetDealProtoAsync(ulong ticket, CancellationToken cancellationToken = default)
    {
        return await GetDealInternalAsync(ticket, d => d.ToProto(), cancellationToken).ConfigureAwait(false);
    }

    public async Task<MT5Result<DealModel[]>> GetDealsAsync(DateTime from, DateTime to, string groupMask = "*", CancellationToken cancellationToken = default)
    {
        return await GetDealsAsync(SMTTime.FromDateTime(from), SMTTime.FromDateTime(to), groupMask, cancellationToken).ConfigureAwait(false);
    }

    public async Task<MT5Result<DealModel[]>> GetDealsAsync(long from, long to, string groupMask = "*", CancellationToken cancellationToken = default)
    {
        return await GetDealsInternalAsync(
            arr => _manager!.DealRequestByGroup(groupMask, from, to, arr),
            d => d.ToModel(),
            null,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<MT5Result<ProtoDeal[]>> GetDealsProtoAsync(DateTime from, DateTime to, string groupMask = "*", CancellationToken cancellationToken = default)
    {
        return await GetDealsProtoAsync(SMTTime.FromDateTime(from), SMTTime.FromDateTime(to), groupMask, cancellationToken).ConfigureAwait(false);
    }

    public async Task<MT5Result<ProtoDeal[]>> GetDealsProtoAsync(long from, long to, string groupMask = "*", CancellationToken cancellationToken = default)
    {
        return await GetDealsInternalAsync(
            arr => _manager!.DealRequestByGroup(groupMask, from, to, arr),
            d => d.ToProto(),
            null,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<MT5Result<DealModel[]>> GetDealsPageAsync(ulong login, DateTime from, DateTime to, uint offset, uint total, CancellationToken cancellationToken = default)
    {
        return await GetDealsPageAsync(login, SMTTime.FromDateTime(from), SMTTime.FromDateTime(to), offset, total, cancellationToken).ConfigureAwait(false);
    }

    public async Task<MT5Result<DealModel[]>> GetDealsPageAsync(ulong login, long from, long to, uint offset, uint total, CancellationToken cancellationToken = default)
    {
        return await GetDealsInternalAsync(
            arr => _manager!.DealRequestPage(login, from, to, offset, total, arr),
            d => d.ToModel(),
            null,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<MT5Result<ProtoDeal[]>> GetDealsPageProtoAsync(ulong login, DateTime from, DateTime to, uint offset, uint total, CancellationToken cancellationToken = default)
    {
        return await GetDealsPageProtoAsync(login, SMTTime.FromDateTime(from), SMTTime.FromDateTime(to), offset, total, cancellationToken).ConfigureAwait(false);
    }

    public async Task<MT5Result<ProtoDeal[]>> GetDealsPageProtoAsync(ulong login, long from, long to, uint offset, uint total, CancellationToken cancellationToken = default)
    {
        return await GetDealsInternalAsync(
            arr => _manager!.DealRequestPage(login, from, to, offset, total, arr),
            d => d.ToProto(),
            null,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<MT5Result<DealModel[]>> GetBalanceHistoryAsync(ulong login, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await GetBalanceHistoryAsync(login, SMTTime.FromDateTime(from), SMTTime.FromDateTime(to), cancellationToken).ConfigureAwait(false);
    }

    public async Task<MT5Result<DealModel[]>> GetBalanceHistoryAsync(ulong login, long from, long to, CancellationToken cancellationToken = default)
    {
        return await GetDealsInternalAsync(
            arr => _manager!.DealRequest(login, from, to, arr),
            d => d.ToModel(),
            d => d.Action() == (uint)CIMTDeal.EnDealAction.DEAL_BALANCE,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<MT5Result<ProtoDeal[]>> GetBalanceHistoryProtoAsync(ulong login, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await GetBalanceHistoryProtoAsync(login, SMTTime.FromDateTime(from), SMTTime.FromDateTime(to), cancellationToken).ConfigureAwait(false);
    }

    public async Task<MT5Result<ProtoDeal[]>> GetBalanceHistoryProtoAsync(ulong login, long from, long to, CancellationToken cancellationToken = default)
    {
        return await GetDealsInternalAsync(
            arr => _manager!.DealRequest(login, from, to, arr),
            d => d.ToProto(),
            d => d.Action() == (uint)CIMTDeal.EnDealAction.DEAL_BALANCE,
            cancellationToken).ConfigureAwait(false);
    }

    private async Task<MT5Result<T>> GetDealInternalAsync<T>(ulong ticket, Func<CIMTDeal, T> converter, CancellationToken cancellationToken = default)
    {
        if (!IsConnected) return MT5Result<T>.Failure("Not connected");

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await Task.Run(() =>
            {
                using var deal = _manager!.DealCreate();
                if (deal == null)
                {
                    return MT5Result<T>.Failure("Failed to create deal object");
                }

                var res = _manager.DealRequest(ticket, deal);

                if (res != MTRetCode.MT_RET_OK)
                {
                    _logger.Error($"DealRequest({ticket}) failed: {res}");
                    return MT5Result<T>.Failure(res, $"DealRequest failed: {res}");
                }

                _logger.Debug($"Retrieved deal: {ticket}");
                return MT5Result<T>.Success(converter(deal));
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"GetDeal error for {ticket}");
            return MT5Result<T>.Failure($"GetDeal error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<MT5Result<T[]>> GetDealsInternalAsync<T>(
        Func<CIMTDealArray, MTRetCode> requestFunc,
        Func<CIMTDeal, T> converter,
        Func<CIMTDeal, bool>? filter = null,
        CancellationToken cancellationToken = default)
    {
        if (!IsConnected) return MT5Result<T[]>.Failure("Not connected");

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_dealArray == null)
            {
                return MT5Result<T[]>.Failure("DealArray not initialized");
            }

            return await Task.Run(() =>
            {
                _dealArray.Clear();
                var res = requestFunc(_dealArray);

                if (res != MTRetCode.MT_RET_OK)
                {
                    return MT5Result<T[]>.Failure(res, $"DealRequest failed: {res}");
                }

                var total = _dealArray.Total();
                var results = ArrayUtils.Convert(total, i => _dealArray.Next(i), converter, filter);
                return MT5Result<T[]>.Success(results);
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "GetDeals error");
            return MT5Result<T[]>.Failure($"GetDeals error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    #endregion
}
