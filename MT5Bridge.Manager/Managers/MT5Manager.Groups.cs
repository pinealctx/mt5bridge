using MetaQuotes.MT5CommonAPI;
using MT5Bridge.Manager.Models;

// Proto extensions
using MT5Bridge.Manager.Models.Proto;

// POCO models - use aliases to avoid conflicts
using GroupModel = MT5Bridge.Manager.Models.GroupModel;

// Protobuf models - use aliases
using ProtoGroup = MT5Bridge.Manager.Models.Proto.GroupModel;

namespace MT5Bridge.Manager.Managers;

public partial class MT5Manager
{
    #region Group Operations

    public async Task<MT5Result<GroupModel[]>> GetGroupsAsync(CancellationToken cancellationToken = default)
    {
        return await GetGroupsInternalAsync(g => g.ToModel(), cancellationToken).ConfigureAwait(false);
    }

    public async Task<MT5Result<ProtoGroup[]>> GetGroupsProtoAsync(CancellationToken cancellationToken = default)
    {
        return await GetGroupsInternalAsync(g => g.ToProto(), cancellationToken).ConfigureAwait(false);
    }

    private async Task<MT5Result<T[]>> GetGroupsInternalAsync<T>(Func<CIMTConGroup, T> converter, CancellationToken cancellationToken = default)
    {
        if (!IsConnected) return MT5Result<T[]>.Failure("Not connected");

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_groupArray == null)
            {
                return MT5Result<T[]>.Failure("GroupArray not initialized");
            }

            return await Task.Run(() =>
            {
                _groupArray.Clear();
                var res = _manager!.GroupRequestArray("*", _groupArray);

                if (res != MTRetCode.MT_RET_OK)
                {
                    _logger.Error($"GroupRequestArray failed: {res}");
                    return MT5Result<T[]>.Failure(res, $"GroupRequestArray failed: {res}");
                }

                var total = _groupArray.Total();
                var results = new List<T>((int)total);
                for (uint i = 0; i < total; i++)
                {
                    var group = _groupArray.Next(i);
                    if (group != null)
                    {
                        results.Add(converter(group));
                    }
                }

                _logger.Debug($"Retrieved {results.Count} groups");
                return MT5Result<T[]>.Success(results.ToArray());
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "GetGroups error");
            return MT5Result<T[]>.Failure($"GetGroups error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<MT5Result<GroupModel>> GetGroupAsync(string groupName, CancellationToken cancellationToken = default)
    {
        return await GetGroupInternalAsync(groupName, g => g.ToModel(), cancellationToken).ConfigureAwait(false);
    }

    public async Task<MT5Result<ProtoGroup>> GetGroupProtoAsync(string groupName, CancellationToken cancellationToken = default)
    {
        return await GetGroupInternalAsync(groupName, g => g.ToProto(), cancellationToken).ConfigureAwait(false);
    }

    private async Task<MT5Result<T>> GetGroupInternalAsync<T>(string groupName, Func<CIMTConGroup, T> converter, CancellationToken cancellationToken = default)
    {
        if (!IsConnected) return MT5Result<T>.Failure("Not connected");
        ArgumentNullException.ThrowIfNullOrEmpty(groupName);

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_group == null)
            {
                return MT5Result<T>.Failure("Group object not initialized");
            }

            return await Task.Run(() =>
            {
                _group.Clear();
                var res = _manager!.GroupRequest(groupName, _group);

                if (res != MTRetCode.MT_RET_OK)
                {
                    _logger.Error($"GroupRequest({groupName}) failed: {res}");
                    return MT5Result<T>.Failure(res, $"GroupRequest failed: {res}");
                }

                _logger.Debug($"Retrieved group: {groupName}");
                return MT5Result<T>.Success(converter(_group));
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"GetGroup error for {groupName}");
            return MT5Result<T>.Failure($"GetGroup error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    #endregion
}