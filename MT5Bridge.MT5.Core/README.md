# MT5Bridge.MT5.Core

MT5 Manager API 线程安全异步封装库

---

## 📖 项目说明

MT5Bridge.MT5.Core 是 MT5 Manager API 的 .NET 8.0 封装库，提供：

- ✅ **线程安全**：使用 `SemaphoreSlim` 保护所有 API 调用
- ✅ **异步接口**：统一的 `async/await` 编程模型
- ✅ **Result 模式**：函数式错误处理，避免异常开销
- ✅ **状态管理**：连接状态监控和事件通知
- ✅ **资源管理**：自动的 COM 对象生命周期管理

---

## 🏗️ 项目结构

```
MT5Bridge.MT5.Core/
├── Managers/
│   ├── IMT5Manager.cs          # Manager 接口定义
│   └── MT5Manager.cs           # Manager 实现（656 行）
├── MT5ConnectionSettings.cs    # 连接配置类
├── ConnectionState.cs          # 连接状态枚举
└── MT5Result.cs                # Result 包装类
```

---

## ✨ 功能特性

### 已实现的 API（15+ 方法）

#### 1. 连接管理
- `ConnectAsync(settings, ct)` - 连接到 MT5 Server
- `DisconnectAsync()` - 断开连接
- `ConnectionStateChanged` 事件 - 状态变化通知

#### 2. 用户组管理
- `GetGroupsAsync(ct)` - 查询所有用户组
- `GetGroupAsync(groupName, ct)` - 查询指定用户组

#### 3. 用户管理
- `GetUserAsync(login, ct)` - 查询用户信息
- `CreateUserAsync(user, masterPwd, investorPwd, ct)` - 创建用户
- `UpdateUserAsync(user, ct)` - 更新用户信息
- `DeleteUserAsync(login, ct)` - 删除用户

#### 4. 账户管理
- `GetAccountAsync(login, ct)` - 查询账户信息
- `DepositAsync(login, amount, comment, ct)` - 充值
- `WithdrawAsync(login, amount, comment, ct)` - 扣款

#### 5. 交易记录
- `GetDealsAsync(login, from, to, ct)` - 查询交易记录
- `GetDealAsync(ticket, ct)` - 查询单笔交易

### 待实现功能

- 余额历史查询
- 实时事件订阅（Pump Mode）
- 连接池
- 自动重连机制

---

## 🚀 快速开始

### 安装依赖

```bash
cd mt5bridge
dotnet restore MT5Bridge.MT5.Core
```

### 编译项目

```bash
dotnet build MT5Bridge.MT5.Core
```

---

## 📝 使用示例

### 基础连接和查询

```csharp
using MT5Bridge.Core.Logging;
using MT5Bridge.Logging.NLog;
using MT5Bridge.MT5.Core;
using MT5Bridge.MT5.Core.Managers;

// 1. 创建 Logger
var logger = new NLogLogger("MyApp");

// 2. 创建 Manager（使用 using 自动释放资源）
using var manager = new MT5Manager(logger);

// 3. 配置连接
var settings = new MT5ConnectionSettings
{
    Server = "localhost:443",
    Login = 100,
    Password = "YourPassword",
    TimeoutMs = 30000,
    PumpMode = PumpMode.Full
};

// 4. 连接
var connectResult = await manager.ConnectAsync(settings);
if (!connectResult.IsSuccess)
{
    Console.WriteLine($"❌ Connection failed: {connectResult.Message}");
    return;
}

Console.WriteLine("✅ Connected successfully");

// 5. 查询用户组
var groupsResult = await manager.GetGroupsAsync();
if (groupsResult.IsSuccess)
{
    var groups = groupsResult.Data;
    Console.WriteLine($"Found {groups.Total()} groups");
    
    for (uint i = 0; i < groups.Total(); i++)
    {
        var group = groups.Next(i);
        Console.WriteLine($"  - {group.Group()}");
    }
}

// 6. 查询用户
var userResult = await manager.GetUserAsync(12345);
if (userResult.IsSuccess)
{
    var user = userResult.Data;
    Console.WriteLine($"User: {user.Name()}, Group: {user.Group()}");
}

// 7. 充值
var depositResult = await manager.DepositAsync(12345, 100.00m, "Test deposit");
if (depositResult.IsSuccess)
{
    Console.WriteLine($"✅ {depositResult.Message}");
}

// 8. 断开连接（using 会自动调用）
await manager.DisconnectAsync();
```

### 监听连接状态

```csharp
manager.ConnectionStateChanged += (sender, e) =>
{
    Console.WriteLine($"Connection: {e.OldState} → {e.NewState}");
    if (!string.IsNullOrEmpty(e.Message))
    {
        Console.WriteLine($"Message: {e.Message}");
    }
};

await manager.ConnectAsync(settings);
```

### 错误处理

```csharp
var result = await manager.GetUserAsync(12345);

// 检查是否成功
if (result.IsSuccess)
{
    var user = result.Data;
    Console.WriteLine($"User: {user.Login()}");
}
else
{
    // 获取错误信息
    Console.WriteLine($"Error: {result.Message}");
    Console.WriteLine($"Code: {result.RetCode}");
}
```

### 使用 CancellationToken

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

try
{
    var result = await manager.GetUserAsync(12345, cts.Token);
    if (result.IsSuccess)
    {
        Console.WriteLine("Success");
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operation timed out");
}
```

---

## 🏛️ 架构设计

### 线程安全机制

所有 API 调用都受 `SemaphoreSlim` 保护：

```csharp
private readonly SemaphoreSlim _lock = new(1, 1);

public async Task<MT5Result<T>> ApiMethodAsync(...)
{
    await _lock.WaitAsync(cancellationToken);
    try
    {
        // 线程安全的 MT5 API 调用
        return await Task.Run(() => {
            var res = _manager.SomeApiCall(...);
            return MT5Result<T>.Success(data);
        }, cancellationToken);
    }
    finally
    {
        _lock.Release();  // 确保释放锁
    }
}
```

### 异步封装原理

MT5 Manager API 是同步的，使用 `Task.Run` 包装：

```csharp
// 原生 SDK（同步阻塞）
MTRetCode res = manager.UserRequest(login, user);

// 我们的封装（异步）
var result = await Task.Run(() => {
    var res = _manager.UserRequest(login, _user);
    return MT5Result.Success(_user);
}, cancellationToken);
```

**重要说明**：
- ⚠️ 这不是真正的异步 I/O（IOCP）
- ⚠️ 仍会占用线程池线程等待响应
- ✅ 但释放了调用线程（适合 ASP.NET Core/gRPC）
- 详见：[TECHNICAL.md](../.task/TECHNICAL.md) 的"异步封装本质"部分

### Result 模式

统一的错误处理，避免异常开销：

```csharp
// 成功
public static MT5Result Success(string message = "Success")
public static MT5Result<T> Success(T data, string message = "Success")

// 失败
public static MT5Result Failure(MTRetCode retCode, string message)
public static MT5Result<T> Failure(string message)
```

使用示例：
```csharp
// 返回成功
return MT5Result<CIMTUser>.Success(user, "User retrieved");

// 返回失败
return MT5Result<CIMTUser>.Failure(retCode, "User not found");
```

### 资源管理

实现 `IDisposable`，自动管理 COM 对象：

```csharp
public void Dispose()
{
    if (_disposed) return;
    
    // 释放 COM 对象
    _dealArray?.Release();
    _user?.Release();
    _account?.Release();
    _group?.Release();
    _groupArray?.Release();
    _manager?.Release();
    
    // 释放锁
    _lock.Dispose();
    
    _disposed = true;
}
```

---

## 🔧 配置说明

### MT5ConnectionSettings

```csharp
public class MT5ConnectionSettings
{
    public string Server { get; set; }              // MT5 服务器地址（必需）
    public ulong Login { get; set; }                // Manager 登录号（必需）
    public string Password { get; set; }            // Manager 密码（必需）
    public uint TimeoutMs { get; set; } = 30000;   // 超时时间（毫秒）
    public PumpMode PumpMode { get; set; } = PumpMode.Full;  // 推送模式
    public bool AutoReconnect { get; set; } = true;          // 自动重连
    public int ReconnectIntervalMs { get; set; } = 5000;     // 重连间隔
    public int MaxReconnectAttempts { get; set; } = 3;       // 最大重连次数
}
```

### PumpMode 枚举

```csharp
public enum PumpMode
{
    None = 0,      // 不订阅事件
    Full = 1,      // 订阅所有事件
    Symbols = 2    // 仅订阅符号事件
}
```

### ConnectionState 枚举

```csharp
public enum ConnectionState
{
    Disconnected,   // 未连接
    Connecting,     // 连接中
    Connected,      // 已连接
    Reconnecting,   // 重连中
    Failed          // 连接失败
}
```

---

## 📦 依赖项

### NuGet 包

无需额外 NuGet 包，仅依赖项目引用。

### 项目引用

```xml
<ProjectReference Include="..\MT5Bridge.Core\MT5Bridge.Core.csproj" />
```

### MT5 SDK DLL

```xml
<!-- .NET 互操作程序集 -->
<Reference Include="MetaQuotes.MT5CommonAPI64">
  <HintPath>..\libs\MetaQuotes.MT5CommonAPI64.dll</HintPath>
</Reference>
<Reference Include="MetaQuotes.MT5ManagerAPI64">
  <HintPath>..\libs\MetaQuotes.MT5ManagerAPI64.dll</HintPath>
</Reference>

<!-- 原生 DLL（需要复制到输出目录）-->
<None Include="..\libs\MT5APIManager64.dll">
  <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
</None>
```

---

## ⚠️ 注意事项

### 1. 平台限制

- **仅支持 Windows x64**
- MT5 SDK 是 64 位 COM 组件
- 确保项目配置为 x64 或 AnyCPU

### 2. 线程安全

- 单个 `MT5Manager` 实例是线程安全的
- 但性能受单连接限制
- 高并发场景建议使用连接池（待实现）

### 3. COM 对象复用

- `CIMTUser`、`CIMTAccount` 等对象在内部被复用
- 每次调用会 `Clear()` 清空数据
- 不要在多个线程间共享这些对象

### 4. 资源释放

```csharp
// ✅ 推荐：使用 using 自动释放
using var manager = new MT5Manager(logger);
await manager.ConnectAsync(settings);
// ... 使用 manager
// Dispose 会自动调用

// ❌ 不推荐：手动管理
var manager = new MT5Manager(logger);
try
{
    await manager.ConnectAsync(settings);
}
finally
{
    manager.Dispose();  // 必须手动调用
}
```

---

## 🔗 相关文档

### 项目文档

- **Demo 项目**：[MT5Bridge.MT5.Demo/README.md](../MT5Bridge.MT5.Demo/README.md)
  - 使用示例和测试场景
  
- **任务文档**：[.task/task.md](../.task/task.md)
  - 项目目标和功能需求
  
- **进度跟踪**：[.task/PROGRESS.md](../.task/PROGRESS.md)
  - 开发进度和统计数据
  
- **技术文档**：[.task/TECHNICAL.md](../.task/TECHNICAL.md)
  - 架构设计和技术细节
  - ⭐ 包含"异步封装本质"重要说明
  
- **API 指南**：[.task/API_GUIDE.md](../.task/API_GUIDE.md)
  - 完整 API 参考和使用指南

### MT5 官方文档

- MT5 Manager API 文档：`e:\work\source\XSyphonBridge\.ref\mt5-ext\.doc\`

---

## 🛠️ 开发指南

### 添加新的 API 方法

1. **在 IMT5Manager 接口中声明**：
```csharp
public interface IMT5Manager
{
    Task<MT5Result<CIMTOrder>> GetOrderAsync(ulong ticket, CancellationToken ct = default);
}
```

2. **在 MT5Manager 中实现**：
```csharp
public async Task<MT5Result<CIMTOrder>> GetOrderAsync(ulong ticket, CancellationToken ct = default)
{
    if (!IsConnected) return MT5Result<CIMTOrder>.Failure("Not connected");
    
    await _lock.WaitAsync(ct);
    try
    {
        var result = await Task.Run(() =>
        {
            _order.Clear();
            var res = _manager!.OrderRequest(ticket, _order);
            
            if (res != MTRetCode.MT_RET_OK)
            {
                _logger.Error($"OrderRequest({ticket}) failed: {res}");
                return MT5Result<CIMTOrder>.Failure(res, $"Failed: {res}");
            }
            
            _logger.Debug($"Retrieved order: {ticket}");
            return MT5Result<CIMTOrder>.Success(_order);
        }, ct);
        
        return result;
    }
    catch (Exception ex)
    {
        _logger.Error($"GetOrder({ticket}) error", ex);
        return MT5Result<CIMTOrder>.Failure($"Error: {ex.Message}");
    }
    finally
    {
        _lock.Release();
    }
}
```

3. **在 Demo 项目中添加测试场景**

---

## 📊 性能考虑

### 单连接性能

- 单个 Manager 实例使用一个 TCP 连接
- 所有操作串行执行（线程安全锁）
- 适合低-中等并发场景（< 100 QPS）

### 高并发优化（未来）

```csharp
// 连接池模式（待实现）
public class MT5ManagerPool
{
    private readonly List<MT5Manager> _managers;
    
    public async Task<MT5Result<T>> ExecuteAsync<T>(
        Func<IMT5Manager, Task<MT5Result<T>>> action)
    {
        var manager = await GetAvailableManagerAsync();
        try
        {
            return await action(manager);
        }
        finally
        {
            ReleaseManager(manager);
        }
    }
}
```

---

## 📝 变更日志

### v1.0.0 - 2024-12-16

#### ✅ 初始发布
- 15+ 个 Manager API 方法
- 线程安全异步封装
- Result 模式错误处理
- 连接状态管理
- 完整的资源管理

#### 🔧 技术改进
- 使用 `net8.0` Target（非 Windows 专用）
- SDK DLL 相对路径引用
- 构建成功（0 错误）

---

**开发者**: Kun  
**公司**: xSyphon  
**许可证**: MIT
