using MT5Bridge.Manager.Models;
using MetaQuotes.MT5CommonAPI;

// Proto extensions
using MT5Bridge.Manager.Models.Proto;

// POCO models
using OrderModel = MT5Bridge.Manager.Models.OrderModel;
using PositionModel = MT5Bridge.Manager.Models.PositionModel;

// Protobuf models
using ProtoOrder = MT5Bridge.Manager.Models.Proto.OrderModel;
using ProtoPosition = MT5Bridge.Manager.Models.Proto.PositionModel;

namespace MT5Bridge.Manager.Managers;

public partial class MT5Manager
{
    #region Order & Position Operations

    public async Task<MT5Result<OrderModel[]>> GetOrdersAsync(ulong login, CancellationToken cancellationToken = default)
    {
        return await GetOrdersInternalAsync(
            arr => _manager!.OrderRequestOpen(login, arr),
            o => o.ToModel(),
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<MT5Result<ProtoOrder[]>> GetOrdersProtoAsync(ulong login, CancellationToken cancellationToken = default)
    {
        return await GetOrdersInternalAsync(
            arr => _manager!.OrderRequestOpen(login, arr),
            o => o.ToProto(),
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<MT5Result<OrderModel[]>> GetHistoryOrdersAsync(ulong login, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await GetHistoryOrdersAsync(login, SMTTime.FromDateTime(from), SMTTime.FromDateTime(to), cancellationToken).ConfigureAwait(false);
    }

    public async Task<MT5Result<OrderModel[]>> GetHistoryOrdersAsync(ulong login, long from, long to, CancellationToken cancellationToken = default)
    {
        return await GetOrdersInternalAsync(
            arr => _manager!.HistoryRequest(login, from, to, arr),
            o => o.ToModel(),
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<MT5Result<ProtoOrder[]>> GetHistoryOrdersProtoAsync(ulong login, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await GetHistoryOrdersProtoAsync(login, SMTTime.FromDateTime(from), SMTTime.FromDateTime(to), cancellationToken).ConfigureAwait(false);
    }

    public async Task<MT5Result<ProtoOrder[]>> GetHistoryOrdersProtoAsync(ulong login, long from, long to, CancellationToken cancellationToken = default)
    {
        return await GetOrdersInternalAsync(
            arr => _manager!.HistoryRequest(login, from, to, arr),
            o => o.ToProto(),
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<MT5Result<OrderModel[]>> GetHistoryOrdersAsync(DateTime from, DateTime to, string groupMask = "*", CancellationToken cancellationToken = default)
    {
        return await GetHistoryOrdersAsync(SMTTime.FromDateTime(from), SMTTime.FromDateTime(to), groupMask, cancellationToken).ConfigureAwait(false);
    }

    public async Task<MT5Result<OrderModel[]>> GetHistoryOrdersAsync(long from, long to, string groupMask = "*", CancellationToken cancellationToken = default)
    {
        return await GetOrdersInternalAsync(
            arr => _manager!.HistoryRequestByGroup(groupMask, from, to, arr),
            o => o.ToModel(),
            cancellationToken);
    }

    public async Task<MT5Result<ProtoOrder[]>> GetHistoryOrdersProtoAsync(DateTime from, DateTime to, string groupMask = "*", CancellationToken cancellationToken = default)
    {
        return await GetHistoryOrdersProtoAsync(SMTTime.FromDateTime(from), SMTTime.FromDateTime(to), groupMask, cancellationToken).ConfigureAwait(false);
    }

    public async Task<MT5Result<ProtoOrder[]>> GetHistoryOrdersProtoAsync(long from, long to, string groupMask = "*", CancellationToken cancellationToken = default)
    {
        return await GetOrdersInternalAsync(
            arr => _manager!.HistoryRequestByGroup(groupMask, from, to, arr),
            o => o.ToProto(),
            cancellationToken);
    }

    public async Task<MT5Result<PositionModel[]>> GetPositionsAsync(ulong login, CancellationToken cancellationToken = default)
    {
        return await GetPositionsInternalAsync(
            arr => _manager!.PositionRequest(login, arr),
            p => p.ToModel(),
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<MT5Result<ProtoPosition[]>> GetPositionsProtoAsync(ulong login, CancellationToken cancellationToken = default)
    {
        return await GetPositionsInternalAsync(
            arr => _manager!.PositionRequest(login, arr),
            p => p.ToProto(),
            cancellationToken).ConfigureAwait(false);
    }

    private async Task<MT5Result<T[]>> GetOrdersInternalAsync<T>(
        Func<CIMTOrderArray, MTRetCode> requestFunc,
        Func<CIMTOrder, T> converter,
        CancellationToken cancellationToken = default)
    {
        if (!IsConnected) return MT5Result<T[]>.Failure("Not connected");

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_orderArray == null)
            {
                return MT5Result<T[]>.Failure("OrderArray not initialized");
            }

            return await Task.Run(() =>
            {
                _orderArray.Clear();
                var res = requestFunc(_orderArray);

                if (res != MTRetCode.MT_RET_OK)
                {
                    return MT5Result<T[]>.Failure(res, $"OrderRequest failed: {res}");
                }

                var total = _orderArray.Total();
                var results = new List<T>((int)total);
                for (uint i = 0; i < total; i++)
                {
                    var order = _orderArray.Next(i);
                    if (order != null)
                    {
                        results.Add(converter(order));
                    }
                }

                return MT5Result<T[]>.Success(results.ToArray());
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "GetOrders error");
            return MT5Result<T[]>.Failure($"GetOrders error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<MT5Result<T[]>> GetPositionsInternalAsync<T>(
        Func<CIMTPositionArray, MTRetCode> requestFunc,
        Func<CIMTPosition, T> converter,
        CancellationToken cancellationToken = default)
    {
        if (!IsConnected) return MT5Result<T[]>.Failure("Not connected");

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_positionArray == null)
            {
                return MT5Result<T[]>.Failure("PositionArray not initialized");
            }

            return await Task.Run(() =>
            {
                _positionArray.Clear();
                var res = requestFunc(_positionArray);

                if (res != MTRetCode.MT_RET_OK)
                {
                    return MT5Result<T[]>.Failure(res, $"PositionRequest failed: {res}");
                }

                var total = _positionArray.Total();
                var results = new List<T>((int)total);
                for (uint i = 0; i < total; i++)
                {
                    var position = _positionArray.Next(i);
                    if (position != null)
                    {
                        results.Add(converter(position));
                    }
                }

                return MT5Result<T[]>.Success(results.ToArray());
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "GetPositions error");
            return MT5Result<T[]>.Failure($"GetPositions error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    #endregion
}
