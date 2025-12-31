using MetaQuotes.MT5CommonAPI;
using MT5Bridge.Core.Collections;
using MT5Bridge.Manager.Models;

// Proto extensions
using MT5Bridge.Manager.Models.Proto;

// POCO models - use aliases to avoid conflicts
using UserModel = MT5Bridge.Manager.Models.UserModel;

// Protobuf models - use aliases
using ProtoUser = MT5Bridge.Manager.Models.Proto.UserModel;

namespace MT5Bridge.Manager.Managers;

public partial class MT5Manager
{
    #region User Operations

    public async Task<MT5Result<UserModel[]>> GetUsersAsync(string? groupMask = null, int offset = 0, int limit = 100, CancellationToken cancellationToken = default)
    {
        return await GetUsersInternalAsync(
            u => u.ToModel(),
            groupMask, offset, limit, cancellationToken).ConfigureAwait(false);
    }

    public async Task<MT5Result<ProtoUser[]>> GetUsersProtoAsync(string? groupMask = null, int offset = 0, int limit = 100, CancellationToken cancellationToken = default)
    {
        return await GetUsersInternalAsync(
            u => u.ToProto(),
            groupMask, offset, limit, cancellationToken).ConfigureAwait(false);
    }

    private async Task<MT5Result<T[]>> GetUsersInternalAsync<T>(
        Func<CIMTUser, T> converter,
        string? groupMask = null,
        int offset = 0,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        if (!IsConnected) return MT5Result<T[]>.Failure("Not connected");
        if (offset < 0) return MT5Result<T[]>.Failure("Offset must be >= 0");
        if (limit <= 0 || limit > 10000) return MT5Result<T[]>.Failure("Limit must be between 1 and 10000");

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var mask = string.IsNullOrWhiteSpace(groupMask) ? "*" : groupMask;

            // Check if the mask contains wildcards or multiple groups
            var hasWildcard = mask.Contains('*') || mask.Contains('!') || mask.Contains(',');

            if (hasWildcard)
            {
                // Use UserGetByGroup (loads all matching users, then slices in memory)
                return await GetUsersByGroupMaskInternalAsync(converter, mask, offset, limit).ConfigureAwait(false);
            }
            else
            {
                // Use UserLogins + UserGetByLogins (true pagination)
                return await GetUsersByGroupPaginatedInternalAsync(converter, mask, offset, limit).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"GetUsers error");
            return MT5Result<T[]>.Failure($"GetUsers error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Get users using UserLogins + UserGetByLogins (true pagination for single group)
    /// </summary>
    private async Task<MT5Result<T[]>> GetUsersByGroupPaginatedInternalAsync<T>(Func<CIMTUser, T> converter, string groupName, int offset, int limit)
    {
        if (_userArray == null)
        {
            return MT5Result<T[]>.Failure("User array object not initialized");
        }

        // Step 1: Get all login numbers for the group (lightweight)
        var logins = _manager!.UserLogins(groupName, out var res);

        if (res != MTRetCode.MT_RET_OK)
        {
            _logger.Error($"UserLogins({groupName}) failed: {res}");
            return MT5Result<T[]>.Failure($"UserLogins failed: {res}");
        }

        if (logins == null || logins.Length == 0)
        {
            return MT5Result<T[]>.Success(Array.Empty<T>(), "No users found in group");
        }

        var total = logins.Length;

        // Step 2: Paginate the login array
        var actualOffset = Math.Min(offset, total);
        var actualLimit = Math.Min(limit, total - actualOffset);

        if (actualLimit <= 0)
        {
            return MT5Result<T[]>.Success(Array.Empty<T>(), $"Offset {offset} exceeds total {total}");
        }

        var pagedLogins = new ulong[actualLimit];
        Array.Copy(logins, actualOffset, pagedLogins, 0, actualLimit);

        // Step 3: Batch fetch user details for the paged logins
        _userArray.Clear();
        res = _manager.UserGetByLogins(pagedLogins, _userArray);

        if (res != MTRetCode.MT_RET_OK)
        {
            _logger.Error($"UserGetByLogins failed: {res}");
            return MT5Result<T[]>.Failure($"UserGetByLogins failed: {res}");
        }

        // Step 4: Convert users from the array
        var userCount = _userArray.Total();
        var result = ArrayUtils.Convert(userCount, i => _userArray.Next(i), converter);

        var message = $"Retrieved {result.Length} users (offset={actualOffset}, total={total}, group={groupName})";
        _logger.Debug(message);

        return MT5Result<T[]>.Success(result, message);
    }

    /// <summary>
    /// Get users using UserGetByGroup (memory pagination for wildcards/multiple groups)
    /// </summary>
    private async Task<MT5Result<T[]>> GetUsersByGroupMaskInternalAsync<T>(Func<CIMTUser, T> converter, string mask, int offset, int limit)
    {
        if (_userArray == null)
        {
            return MT5Result<T[]>.Failure("User array object not initialized");
        }

        // Load all matching users
        _userArray.Clear();
        var res = _manager!.UserGetByGroup(mask, _userArray);

        if (res != MTRetCode.MT_RET_OK)
        {
            _logger.Error($"UserGetByGroup({mask}) failed: {res}");
            return MT5Result<T[]>.Failure($"UserGetByGroup failed: {res}");
        }

        var total = _userArray.Total();
        if (total == 0)
        {
            return MT5Result<T[]>.Success(Array.Empty<T>(), "No users found");
        }

        // Paginate in memory
        var actualOffset = Math.Min(offset, (int)total);
        var actualLimit = Math.Min(limit, (int)total - actualOffset);

        if (actualLimit <= 0)
        {
            return MT5Result<T[]>.Success(Array.Empty<T>(), $"Offset {offset} exceeds total {total}");
        }

        var result = ArrayUtils.Convert(
            (uint)actualLimit,
            i => _userArray.Next((uint)(actualOffset + i)),
            converter);

        var message = $"Retrieved {result.Length} users (offset={actualOffset}, total={total}, mask={mask}, mode=memory-paging)";
        _logger.Debug(message);

        return MT5Result<T[]>.Success(result, message);
    }

    public async Task<MT5Result<UserModel>> GetUserAsync(ulong login, CancellationToken cancellationToken = default)
    {
        return await GetUserInternalAsync(login, u => u.ToModel(), cancellationToken).ConfigureAwait(false);
    }

    public async Task<MT5Result<ProtoUser>> GetUserProtoAsync(ulong login, CancellationToken cancellationToken = default)
    {
        return await GetUserInternalAsync(login, u => u.ToProto(), cancellationToken).ConfigureAwait(false);
    }

    private async Task<MT5Result<T>> GetUserInternalAsync<T>(ulong login, Func<CIMTUser, T> converter, CancellationToken cancellationToken = default)
    {
        if (!IsConnected) return MT5Result<T>.Failure("Not connected");

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_user == null)
            {
                return MT5Result<T>.Failure("User object not initialized");
            }

            return await Task.Run(() =>
            {
                _user.Clear();
                var res = _manager!.UserRequest(login, _user);

                if (res != MTRetCode.MT_RET_OK)
                {
                    _logger.Error($"UserRequest({login}) failed: {res}");
                    return MT5Result<T>.Failure(res, $"UserRequest failed: {res}");
                }

                _logger.Debug($"Retrieved user: {login}");
                return MT5Result<T>.Success(converter(_user));
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"GetUser error for {login}");
            return MT5Result<T>.Failure($"GetUser error: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<MT5Result<UserModel>> CreateUserAsync(UserModel user, string masterPassword, string investorPassword, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
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
        ArgumentNullException.ThrowIfNull(user);

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var comUser = _manager!.UserCreate();
            if (comUser == null) return MT5Result<ProtoUser>.Failure("Failed to create COM user object");

            try
            {
                comUser.UpdateFromProto(user);
                var res = _manager.UserAdd(comUser, masterPassword, investorPassword);

                if (res != MTRetCode.MT_RET_OK)
                {
                    _logger.Error($"UserAdd failed: {res}");
                    return MT5Result<ProtoUser>.Failure(res, $"UserAdd failed: {res}");
                }

                _logger.Information($"User created: {comUser.Login()}");
                return MT5Result<ProtoUser>.Success(comUser.ToProto());
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

    public async Task<MT5Result<UserModel>> UpdateUserAsync(UserModel user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
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
        ArgumentNullException.ThrowIfNull(user);

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
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
                comUser.UpdateFromProto(user);
                res = _manager.UserUpdate(comUser);

                if (res != MTRetCode.MT_RET_OK)
                {
                    _logger.Error($"UserUpdate failed: {res}");
                    return MT5Result<ProtoUser>.Failure(res, $"UserUpdate failed: {res}");
                }

                _logger.Information($"User updated: {comUser.Login()}");
                return MT5Result<ProtoUser>.Success(comUser.ToProto());
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

    public async Task<MT5Result> DeleteUserAsync(ulong login, CancellationToken cancellationToken = default)
    {
        if (!IsConnected) return MT5Result.Failure("Not connected");

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
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
            }, cancellationToken).ConfigureAwait(false);

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
}