# MT5Bridge.MT5.Demo

MT5 Manager API 演示/测试控制台

---

## 📖 项目说明

这是一个命令行演示程序，用于测试和展示 MT5Bridge.MT5.Core 库的功能。

**用途**：
- ✅ 验证 MT5 Manager API 封装的正确性
- ✅ 演示各个 API 的使用方法
- ✅ 作为集成测试工具
- ✅ 快速调试和排查问题

**不是单元测试**：
- 需要真实的 MT5 Server 连接
- 需要有效的 Manager 账号和密码
- 会执行真实的 MT5 操作（如充值）
- 适合手动运行验证

---

## 🚀 快速开始

### 1. 配置连接信息

有两种方式配置 MT5 Server 连接信息：

#### 方式 1: 使用配置文件（推荐）

编辑 `appsettings.json` 文件：

```json
{
  "MT5Connection": {
    "Server": "localhost:443",
    "Login": 100,
    "Password": "your_password_here",
    "TimeoutMs": 30000
  },
  "Demo": {
    "DefaultTest": "groups",
    "DefaultUserLogin": 0
  }
}
```

**优点**：
- ✅ 密码不会出现在命令行历史中
- ✅ 多次运行无需重复输入
- ✅ 便于版本控制（可 .gitignore 排除）
- ✅ 可以设置默认测试场景

**注意**: ⚠️ **不要将包含真实密码的配置文件提交到 Git！**

#### 方式 2: 使用命令行参数

```bash
.\MT5Bridge.MT5.Demo.exe --server localhost:443 --login 100 --password admin --test groups
```

**优点**：
- ✅ 灵活方便，适合快速测试
- ✅ 可以覆盖配置文件的默认值

**混合使用**：
- 配置文件设置默认值
- 命令行参数覆盖特定选项

```bash
# 使用配置文件的 Server/Login/Password，但指定不同的测试
.\MT5Bridge.MT5.Demo.exe --test user --user-login 12345
```

### 2. 编译项目

```bash
cd e:\work\source\XSyphonBridge\mt5bridge
dotnet build MT5Bridge.MT5.Demo
```

### 3. 查看帮助

```bash
cd MT5Bridge.MT5.Demo\bin\Debug\net8.0
.\MT5Bridge.MT5.Demo.exe --help
```

**输出**：
```
=== MT5Bridge Demo Console ===

MT5Bridge.MT5.Demo 1.0.0
Copyright (C) 2025 xSyphon

  -s, --server        MT5 server address (e.g., localhost:443)
  -l, --login         Manager login
  -p, --password      Manager password
  -t, --test          Test to run: groups, user, account, deposit, deals
  -u, --user-login    User login for user/account/deposit/deals tests
  --help              Display this help screen.
  --version           Display version information.
```

---

## 🎯 测试场景

### 场景 1: 查询所有用户组

**命令（使用配置文件）**：
```bash
# 前提：appsettings.json 已配置 Server/Login/Password
.\MT5Bridge.MT5.Demo.exe --test groups
```

**命令（使用命令行参数）**：
```bash
.\MT5Bridge.MT5.Demo.exe --server localhost:443 --login 100 --password admin --test groups
```

**功能**：
- 连接到 MT5 Server
- 查询所有用户组
- 显示每个组的基本信息

**输出示例**：
```
=== MT5Bridge Demo Console ===
Connecting to localhost:443...
✅ Connected successfully

=== Test: Get All Groups ===

✅ Found 3 groups:
  - demo (Currency: USD, Leverage: 1:100)
  - real (Currency: USD, Leverage: 1:50)
  - contest (Currency: USD, Leverage: 1:200)

Disconnecting...
✅ Disconnected

Press any key to exit...
```

**测试的 API**：
- `ConnectAsync(settings)`
- `GetGroupsAsync()`
- `DisconnectAsync()`

---

### 场景 2: 查询用户信息

**命令（使用配置文件）**：
```bash
.\MT5Bridge.MT5.Demo.exe --test user --user-login 12345
```

**命令（使用命令行参数）**：
```bash
.\MT5Bridge.MT5.Demo.exe --server localhost:443 --login 100 --password admin --test user --user-login 12345
```

**功能**：
- 查询指定用户的详细信息
- 显示用户名、组、邮箱、注册时间等

**输出示例**：
```
=== Test: Get User 12345 ===

✅ User information:
   Login: 12345
   Name: John Doe
   Group: demo
   Email: john@example.com
   Country: United States
   City: New York
   Phone: +1234567890
   Registration: 2024-01-15 10:30:00
   Last Access: 2024-12-16 14:25:30
```

**测试的 API**：
- `GetUserAsync(login)`
（使用配置文件）**：
```bash
.\MT5Bridge.MT5.Demo.exe --test account --user-login 12345
```

**命令（使用命令行参数）**：
```bash
.\MT5Bridge.MT5.Demo.exe --server localhost:443 --login 100 --password
### 场景 3: 查询账户信息

**命令**：
```bash
.\MT5Bridge.MT5.Demo.exe -s localhost:443 -l 100 -p admin --test account --user-login 12345
```

**功能**：
- 查询用户的账户余额、权益、保证金等

**输出示例**：
```
=== Test: Get Account 12345 ===

✅ Account information:
   Balance:      $10,000.00
   Credit:       $0.00
   Equity:       $10,150.50
   Margin:       $500.00
   Free Margin:  $9,650.50
   Margin Level: 2030.10%
   Profit:       $150.50
```

**测试的 API**：
- `GetAccountAsync(login)`
（使用配置文件）**：
```bash
.\MT5Bridge.MT5.Demo.exe --test deposit --user-login 12345
```

**命令（使用命令行参数）**：
```bash
.\MT5Bridge.MT5.Demo.exe --server localhost:443 --login 100 --password
### 场景 4: 充值测试

**命令**：
```bash
.\MT5Bridge.MT5.Demo.exe -s localhost:443 -l 100 -p admin --test deposit --user-login 12345
```

**功能**：
- 查询充值前的余额
- 执行充值操作（$100）
- 查询充值后的余额
- 验证余额变化

**⚠️ 注意**：这会执行真实的充值操作！

**输出示例**：
```
=== Test: Deposit to 12345 ===

📊 Before deposit:
   Balance: $10,000.00
   Equity:  $10,150.50

💰 Depositing $100.00...

✅ Deposit successful! Deal ID: 987654

📊 After deposit:
   Balance: $10,100.00
   Equity:  $10,250.50

✅ Balance changed: +$100.00
```

**测试的 API**：
- `GetAccountAsync(login)`
- `DepositAsync(login, amount, comment)`

---

### 场景 5: 查询交易记录

**命令（使用配置文件）**：
```bash
.\MT5Bridge.MT5.Demo.exe --test deals --user-login 12345
```

**命令（使用命令行参数）**：
```bash
.\MT5Bridge.MT5.Demo.exe --server localhost:443 --login 100 --password admin --test deals --user-login 12345
```

**功能**：
- 查询用户最近 7 天的交易记录
- 显示每笔交易的详细信息

**输出示例**：
```
=== Test: Get Deals for 12345 ===

Query range: 2024-12-09 to 2024-12-16

✅ Found 5 deals:

Deal #1:
   Ticket: 987654
   Time:   2024-12-16 10:30:00
   Action: Balance
   Volume: 0.00
   Price:  0.0000
   Profit: $100.00
   Comment: Test deposit

Deal #2:
   Ticket: 987653
   Time:   2024-12-15 14:20:00
   Action: Buy
   Symbol: EURUSD
   Volume: 1.00
   Price:  1.0850
   Profit: $25.50
   Comment: Take Profit

... (more deals)
```

**测试的 API**：
- `GetDealsAsync(login, from, to)`

---

## 📝 使用场景

### 开发调试

**使用配置文件（推荐）**：

1. 编辑 `appsettings.json`，配置开发环境连接：
```json
{
  "MT5Connection": {
    "Server": "dev.example.com:443",
    "Login": 100,
    "Password": "dev_password",
    "TimeoutMs": 30000
  }
}
```

2. 快速测试各种功能：
```bash
# 测试连接
.\MT5Bridge.MT5.Demo.exe --test groups

# 验证 API 修改
.\MT5Bridge.MT5.Demo.exe --test user --user-login 12345

# 调试充值功能
.\MT5Bridge.MT5.Demo.exe --test deposit --user-login 12345
```

**使用命令行参数（灵活切换）**：
```bash
# 快速连接到不同环境
.\MT5Bridge.MT5.Demo.exe --server testserver:443 --login 100 --password test --test groups
```

### 集成测试

**使用配置文件执行完整测试流程**：
```bash
# 1. 配置 appsettings.json 为测试环境
# 2. 执行所有核心功能测试
.\MT5Bridge.MT5.Demo.exe --test groups
.\MT5Bridge.MT5.Demo.exe --test user --user-login 12345
.\MT5Bridge.MT5.Demo.exe --test account --user-login 12345
.\MT5Bridge.MT5.Demo.exe --test deals --user-login 12345
```

### 生产环境验证

**使用命令行参数验证（避免配置文件泄露）**：
```bash
# 连接到生产服务器验证
.\MT5Bridge.MT5.Demo.exe --server prod.example.com:443 --login 200 --password prod_password --test groups
```

**或者使用独立的生产配置文件**：
```bash
# 使用 appsettings.Production.json
# 注意：确保此文件已加入 .gitignore
```

---

## 🔧 配置说明

### 连接配置

`appsettings.json` 配置文件结构：

```json
{
  "MT5Connection": {
    "Server": "localhost:443",        // MT5 Server 地址和端口
    "Login": 100,                      // Manager 账号
    "Password": "your_password",       // Manager 密码
    "TimeoutMs": 30000                 // 连接超时（毫秒）
  },
  "Demo": {
    "DefaultTest": "groups",           // 默认测试场景
    "DefaultUserLogin": 0              // 默认测试用户 ID
  }
}
```

**配置优先级**：命令行参数 > appsettings.json

**安全提示**：
- ⚠️ **不要将包含真实密码的配置文件提交到 Git**
- ✅ 将 `appsettings.json` 加入 `.gitignore`
- ✅ 使用环境变量或密钥管理服务（生产环境）
- ✅ 创建 `appsettings.example.json` 作为模板

**推荐的 .gitignore 配置**：
```gitignore
# MT5Bridge.MT5.Demo
appsettings.json
appsettings.*.json
!appsettings.example.json

# 日志文件
logs/
*.log

# 生成文件
bin/
obj/
```

**首次使用步骤**：
1. 复制 `appsettings.example.json` 为 `appsettings.json`
2. 编辑 `appsettings.json` 填入真实连接信息
3. 确保 `appsettings.json` 已在 `.gitignore` 中

### 日志配置

日志使用 NLog，配置文件：`NLog.config`

**默认配置**：
```xml
<nlog>
  <targets>
    <target name="console" xsi:type="Console" />
    <target name="file" xsi:type="File" fileName="logs/mt5demo-${shortdate}.log" />
  </targets>
  
  <rules>
    <logger name="*" minlevel="Debug" writeTo="console" />
    <logger name="*" minlevel="Info" writeTo="file" />
  </rules>
</nlog>
```

**日志级别**：
- Debug - 详细调试信息（包括 API 调用参数）
- Info - 正常操作信息（连接、查询、充值等）
- Error - 错误信息（连接失败、API 错误等）

**日志文件位置**：
```
MT5Bridge.MT5.Demo\bin\Debug\net8.0\logs\
├── mt5demo-2024-12-16.log
├── mt5demo-2024-12-15.log
└── ...
```

---

## 📊 代码结构

```csharp
// Program.cs 主要结构

public class Options
{
    [Option('s', "server", Required = true)]
    public string Server { get; set; }
    
    [Option('l', "login", Required = true)]
    public ulong Login { get; set; }
    
    [Option('p', "password", Required = true)]
    public string Password { get; set; }
    
    [Option('t', "test", Default = "groups")]
    public string Test { get; set; }
    
    [Option('u', "user-login")]
    public ulong? UserLogin { get; set; }
}

// 主函数
static async Task Main(string[] args)
{
    var parser = new Parser(settings => {
        settings.HelpWriter = Console.Out;
    });
    
    await parser.ParseArguments<Options>(args)
        .WithParsedAsync(async options => {
            await RunTestAsync(options);
        });
}

// 测试执行
static async Task RunTestAsync(Options options)
{
    using var manager = new MT5Manager(logger);
    
    // 连接
    var connectResult = await manager.ConnectAsync(settings);
    
    // 根据测试类型执行
    switch (options.Test.ToLower())
    {
        case "groups":
            await TestGetGroupsAsync(manager);
            break;
        case "user":
            await TestGetUserAsync(manager, options.UserLogin);
            break;
        // ... 其他测试
    }
    
    // 断开连接
    await manager.DisconnectAsync();
}
```

---

## ⚠️ 注意事项

### 1. 真实操作警告

**Deposit 测试会执行真实充值**：
```bash
# ⚠️ 这会真的给账户充值 $100！
.\MT5Bridge.MT5.Demo.exe -s prod:443 -l 100 -p admin --test deposit -u 12345
```

**建议**：
- 使用测试服务器进行 deposit 测试
- 或使用小金额测试
- 生产环境慎用

### 2. 账号权限

需要 **Manager 级别**的账号才能执行操作：
- 普通用户账号无法使用
- Investor 账号无法使用
- 确保有足够的权限

### 3. 服务器连接

连接参数格式：
```bash
# 正确
-s "localhost:443"
-s "192.168.1.100:443"
-s "mt5server.example.com:443"

# 错误
-s "localhost"        # 缺少端口
-s "443"              # 缺少地址
-s "https://server"   # 不需要协议前缀
```

### 4. 防火墙

确保防火墙允许连接：
- 默认端口：443
- 协议：TCP
- 如果连接失败，检查防火墙设置

---

## 🐛 故障排查

### 问题 1: 连接失败

```
❌ Connection failed: MT_RET_ERR_CONNECTION
```

**可能原因**：
1. 服务器地址或端口错误
2. MT5 Server 未运行
3. 网络不通或防火墙阻止
4. Manager API 未启用

**解决方案**：
```bash
# 1. 检查服务器地址
ping mt5server.example.com

# 2. 检查端口
telnet mt5server.example.com 443

# 3. 确认 MT5 Server 运行状态
# 4. 检查 Manager API 配置
```

### 问题 2: 认证失败

```
❌ Connection failed: MT_RET_ERR_INVALID_PASSWORD
```

**可能原因**：
1. 用户名或密码错误
2. 账号没有 Manager 权限
3. 账号被禁用

**解决方案**：
- 确认 Manager 账号和密码
- 检查账号权限级别
- 联系 MT5 管理员

### 问题 3: 找不到用户

```
❌ User request failed: MT_RET_ERR_NOTFOUND
```

**可能原因**：
- 用户 login 不存在
- 输入的 login 错误

**解决方案**：
```bash
# 1. 先查询所有组，确认服务器连接正常
.\MT5Bridge.MT5.Demo.exe -s localhost:443 -l 100 -p admin --test groups

# 2. 确认用户 login 是否存在
# 登录 MT5 Manager 查看用户列表
```

### 问题 4: DLL 找不到

```
System.DllNotFoundException: Unable to load DLL 'MT5APIManager64.dll'
```

**解决方案**：
```bash
# 确保 DLL 文件在正确位置
ls MT5Bridge.MT5.Demo\bin\Debug\net8.0\

# 应该有以下文件：
# - MT5APIManager64.dll
# - MetaQuotes.MT5CommonAPI64.dll
# - MetaQuotes.MT5ManagerAPI64.dll

# 如果缺失，从 libs 目录复制
Copy-Item ..\libs\*.dll .\
```

---

## 🔗 相关项目

- **MT5Bridge.MT5.Core** - 核心封装库
  - 文档：`mt5bridge\MT5Bridge.MT5.Core\README.md`
  
- **项目文档** - 完整项目文档
  - 任务文档：`mt5bridge\.task\task.md`
  - 进度跟踪：`mt5bridge\.task\PROGRESS.md`
  - 技术文档：`mt5bridge\.task\TECHNICAL.md`
  - API 指南：`mt5bridge\.task\API_GUIDE.md`

---

## 📚 扩展阅读

### 添加新的测试场景

```csharp
// 在 Program.cs 中添加

case "newtest":
    await TestNewFeatureAsync(manager, options);
    break;

static async Task TestNewFeatureAsync(IMT5Manager manager, Options options)
{
    Console.WriteLine("\n=== Test: New Feature ===\n");
    
    // 实现测试逻辑
    var result = await manager.SomeNewMethodAsync();
    
    if (result.IsSuccess)
    {
        Console.WriteLine("✅ Test passed");
    }
    else
    {
        Console.WriteLine($"❌ Test failed: {result.Message}");
    }
}
```

### 作为库使用

Demo 项目也可以作为参考代码：

```csharp
// 参考 Program.cs 中的实现
// 在你的项目中这样使用：

using var manager = new MT5Manager(logger);

var settings = new MT5ConnectionSettings
{
    Server = "localhost:443",
    Login = 100,
    Password = "admin"
};

var connectResult = await manager.ConnectAsync(settings);
if (connectResult.IsSuccess)
{
    var groupsResult = await manager.GetGroupsAsync();
    // ... 处理结果
}
```

---

## 📝 更新日志

### v1.0.0 - 2024-12-16
- ✅ 初始版本
- ✅ 5 个测试场景：groups, user, account, deposit, deals
- ✅ 支持配置文件 (appsettings.json) 和命令行参数
- ✅ 配置文件 + 命令行参数混合使用
- ✅ CommandLineParser 参数支持
- ✅ NLog 日志集成
- ✅ 详细的输出格式
- ✅ 配置文件模板 (appsettings.example.json)

---

**开发者**: Kun  
**公司**: xSyphon  
**许可证**: MIT
