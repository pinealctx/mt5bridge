using MetaQuotes.MT5CommonAPI;
using MT5Bridge.Manager.Sinks;
using MT5Bridge.Manager.Models;

// Proto extensions
using MT5Bridge.Manager.Models.Proto;

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

public partial class MT5Manager
{
    #region Typed Event Handlers

    /// <summary>
    /// Register a deal event handler (POCO)
    /// </summary>
    public MT5Result RegisterDealHandler(
        Action<DealModel>? onAdd = null,
        Action<DealModel>? onUpdate = null,
        Action<DealModel>? onDelete = null,
        Action<ulong>? onClean = null,
        Action? onSync = null,
        Action<DealModel, AccountModel, PositionModel>? onPerform = null)
    {
        var initResult = EnsureInitialized();
        if (!initResult.IsSuccess)
        {
            return initResult;
        }

        _dealSinkPoco = new MT5GenericDealSink<DealModel, AccountModel, PositionModel>(
            m => m.ToModel(), onAdd, onUpdate, onDelete, onClean, onSync,
            a => a.ToModel(), p => p.ToModel(), onPerform, _logger);

        if (_dealSinkPoco.RegisterSink() != MTRetCode.MT_RET_OK)
        {
            return MT5Result.Failure("Failed to register deal sink (POCO)");
        }

        _manager?.DealSubscribe(_dealSinkPoco);

        _logger.Debug("Registered deal handler (POCO)");
        return MT5Result.Success();
    }

    /// <summary>
    /// Register a deal event handler (Protobuf)
    /// </summary>
    public MT5Result RegisterDealProtoHandler(
        Action<ProtoDeal>? onAdd = null,
        Action<ProtoDeal>? onUpdate = null,
        Action<ProtoDeal>? onDelete = null,
        Action<ulong>? onClean = null,
        Action? onSync = null,
        Action<ProtoDeal, ProtoAccount, ProtoPosition>? onPerform = null)
    {
        var initResult = EnsureInitialized();
        if (!initResult.IsSuccess)
        {
            return initResult;
        }

        _dealSinkProto = new MT5GenericDealSink<ProtoDeal, ProtoAccount, ProtoPosition>(
            m => m.ToProto(), onAdd, onUpdate, onDelete, onClean, onSync,
            a => a.ToProto(), p => p.ToProto(), onPerform, _logger);

        if (_dealSinkProto.RegisterSink() != MTRetCode.MT_RET_OK)
        {
            return MT5Result.Failure("Failed to register deal sink (Protobuf)");
        }

        _manager?.DealSubscribe(_dealSinkProto);

        _logger.Debug("Registered deal handler (Protobuf)");
        return MT5Result.Success();
    }

    /// <summary>
    /// Register an order event handler (POCO)
    /// </summary>
    public MT5Result RegisterOrderHandler(
        Action<OrderModel>? onAdd = null,
        Action<OrderModel>? onUpdate = null,
        Action<OrderModel>? onDelete = null,
        Action<ulong>? onClean = null,
        Action? onSync = null)
    {
        var initResult = EnsureInitialized();
        if (!initResult.IsSuccess)
        {
            return initResult;
        }

        _orderSinkPoco = new MT5GenericOrderSink<OrderModel>(
            m => m.ToModel(), onAdd, onUpdate, onDelete, onClean, onSync, _logger);

        if (_orderSinkPoco.RegisterSink() != MTRetCode.MT_RET_OK)
        {
            return MT5Result.Failure("Failed to register order sink (POCO)");
        }

        _manager?.OrderSubscribe(_orderSinkPoco);

        _logger.Debug("Registered order handler (POCO)");
        return MT5Result.Success();
    }

    /// <summary>
    /// Register an order event handler (Protobuf)
    /// </summary>
    public MT5Result RegisterOrderProtoHandler(
        Action<ProtoOrder>? onAdd = null,
        Action<ProtoOrder>? onUpdate = null,
        Action<ProtoOrder>? onDelete = null,
        Action<ulong>? onClean = null,
        Action? onSync = null)
    {
        var initResult = EnsureInitialized();
        if (!initResult.IsSuccess)
        {
            return initResult;
        }

        _orderSinkProto = new MT5GenericOrderSink<ProtoOrder>(
            m => m.ToProto(), onAdd, onUpdate, onDelete, onClean, onSync, _logger);

        if (_orderSinkProto.RegisterSink() != MTRetCode.MT_RET_OK)
        {
            return MT5Result.Failure("Failed to register order sink (Protobuf)");
        }

        _manager?.OrderSubscribe(_orderSinkProto);

        _logger.Debug("Registered order handler (Protobuf)");
        return MT5Result.Success();
    }

    /// <summary>
    /// Register a position event handler (POCO)
    /// </summary>
    public MT5Result RegisterPositionHandler(
        Action<PositionModel>? onAdd = null,
        Action<PositionModel>? onUpdate = null,
        Action<PositionModel>? onDelete = null,
        Action<ulong>? onClean = null,
        Action? onSync = null)
    {
        var initResult = EnsureInitialized();
        if (!initResult.IsSuccess)
        {
            return initResult;
        }

        _positionSinkPoco = new MT5GenericPositionSink<PositionModel>(
            m => m.ToModel(), onAdd, onUpdate, onDelete, onClean, onSync, _logger);

        if (_positionSinkPoco.RegisterSink() != MTRetCode.MT_RET_OK)
        {
            return MT5Result.Failure("Failed to register position sink (POCO)");
        }

        _manager?.PositionSubscribe(_positionSinkPoco);

        _logger.Debug("Registered position handler (POCO)");
        return MT5Result.Success();
    }

    /// <summary>
    /// Register a position event handler (Protobuf)
    /// </summary>
    public MT5Result RegisterPositionProtoHandler(
        Action<ProtoPosition>? onAdd = null,
        Action<ProtoPosition>? onUpdate = null,
        Action<ProtoPosition>? onDelete = null,
        Action<ulong>? onClean = null,
        Action? onSync = null)
    {
        var initResult = EnsureInitialized();
        if (!initResult.IsSuccess)
        {
            return initResult;
        }

        _positionSinkProto = new MT5GenericPositionSink<ProtoPosition>(
            m => m.ToProto(), onAdd, onUpdate, onDelete, onClean, onSync, _logger);

        if (_positionSinkProto.RegisterSink() != MTRetCode.MT_RET_OK)
        {
            return MT5Result.Failure("Failed to register position sink (Protobuf)");
        }

        _manager?.PositionSubscribe(_positionSinkProto);

        _logger.Debug("Registered position handler (Protobuf)");
        return MT5Result.Success();
    }

    /// <summary>
    /// Register a manager event handler (POCO)
    /// </summary>
    public MT5Result RegisterManagerHandler(
        Action? onConnect = null,
        Action? onDisconnect = null,
        Action<MTRetCode, long, UserModel, AccountModel, List<OrderModel>, List<PositionModel>>? onTradeAccountSet = null)
    {
        var initResult = EnsureInitialized();
        if (!initResult.IsSuccess)
        {
            return initResult;
        }

        _managerSinkPoco = new MT5GenericManagerSink<UserModel, AccountModel, OrderModel, PositionModel>(
            onConnect,
            () =>
            {
                HandleServerDisconnect();
                onDisconnect?.Invoke();
            },
            u => u.ToModel(),
            a => a.ToModel(),
            o => o.ToModel(),
            p => p.ToModel(),
            onTradeAccountSet,
            _logger);

        if (_managerSinkPoco.RegisterSink() != MTRetCode.MT_RET_OK)
        {
            return MT5Result.Failure("Failed to register manager sink (POCO)");
        }

        _manager?.Subscribe(_managerSinkPoco);

        _logger.Debug("Registered manager handler (POCO)");
        return MT5Result.Success();
    }

    /// <summary>
    /// Register a manager event handler (Protobuf)
    /// </summary>
    public MT5Result RegisterManagerProtoHandler(
        Action? onConnect = null,
        Action? onDisconnect = null,
        Action<MTRetCode, long, ProtoUser, ProtoAccount, List<ProtoOrder>, List<ProtoPosition>>? onTradeAccountSet = null)
    {
        var initResult = EnsureInitialized();
        if (!initResult.IsSuccess)
        {
            return initResult;
        }

        _managerSinkProto = new MT5GenericManagerSink<ProtoUser, ProtoAccount, ProtoOrder, ProtoPosition>(
            onConnect,
            () =>
            {
                HandleServerDisconnect();
                onDisconnect?.Invoke();
            },
            u => u.ToProto(),
            a => a.ToProto(),
            o => o.ToProto(),
            p => p.ToProto(),
            onTradeAccountSet,
            _logger);

        if (_managerSinkProto.RegisterSink() != MTRetCode.MT_RET_OK)
        {
            return MT5Result.Failure("Failed to register manager sink (Protobuf)");
        }

        _manager?.Subscribe(_managerSinkProto);

        _logger.Debug("Registered manager handler (Protobuf)");
        return MT5Result.Success();
    }

    #endregion

    private void UnsubscribeAllSinks()
    {
        // Unsubscribe all registered sinks
        if (_managerSinkPoco != null)
        {
            _manager?.Unsubscribe(_managerSinkPoco);
        }

        if (_managerSinkProto != null)
        {
            _manager?.Unsubscribe(_managerSinkProto);
        }

        if (_dealSinkPoco != null)
        {
            _manager?.DealUnsubscribe(_dealSinkPoco);
        }

        if (_dealSinkProto != null)
        {
            _manager?.DealUnsubscribe(_dealSinkProto);
        }

        if (_orderSinkPoco != null)
        {
            _manager?.OrderUnsubscribe(_orderSinkPoco);
        }

        if (_orderSinkProto != null)
        {
            _manager?.OrderUnsubscribe(_orderSinkProto);
        }

        if (_positionSinkPoco != null)
        {
            _manager?.PositionUnsubscribe(_positionSinkPoco);
        }

        if (_positionSinkProto != null)
        {
            _manager?.PositionUnsubscribe(_positionSinkProto);
        }
    }
}