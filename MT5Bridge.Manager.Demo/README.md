# MT5Bridge.MT5.Demo

[English](#english) | [中文](#chinese)

---

<a name="english"></a>

## English Documentation

Command-line demonstration application for MT5Bridge.Manager library, showcasing query operations, real-time event listening, and trade operations.

**Framework**: .NET 8.0 | **Platform**: x64 | **Interface**: CLI (CommandLineParser) | **Status**: Production Ready

---

### 🚀 Features

#### Command Categories

1. **Query Commands** - Retrieve MT5 server data
   - `query-groups` - List all user groups
   - `query-user --user-login <login>` - Query user information
   - `query-account --user-login <login>` - Query account balance and equity
   - `query-deals --user-login <login>` - Query user's deal history
   - `query-balance --user-login <login>` - Query deposit/withdrawal history

2. **Listen Commands** - Subscribe to real-time events
   - `listen --mode <mode> --types <types>`
     - Modes: `poco` (default), `protobuf`, `mixed`
     - Types: `deal` (default), `order`, `position`, `all`

3. **Trade Commands** - Execute trade operations
   - `deposit --user-login <login> --amount <amount>` - Deposit funds
   - `withdraw --user-login <login> --amount <amount>` - Withdraw funds

---

### 📦 Installation

#### Prerequisites
- .NET 8.0 SDK or Runtime
- MT5 Server with Manager API enabled
- MetaQuotes MT5 SDK DLLs (included in `libs/`)

#### Build from Source

```bash
# Clone repository
cd mt5bridge

# Build Release version
dotnet build MT5Bridge.MT5.Demo/MT5Bridge.MT5.Demo.csproj -c Release

# Or build all projects
dotnet build mt5bridge.sln -c Release
```

#### Publish Self-Contained Executable

```powershell
# Windows PowerShell
cd MT5Bridge.MT5.Demo
.\publish.ps1
```

This creates a single-file executable in `publish/` folder.

---

### ⚙️ Configuration

#### 1. Create Configuration File

Copy the example configuration:

```bash
cp appsettings.example.json appsettings.json
```

#### 2. Edit Connection Settings

Edit `appsettings.json`:

```json
{
  "MT5Connection": {
    "Server": "localhost:443",
    "Login": "1001",
    "Password": "YourManagerPassword",
    "TimeoutMs": "30000"
  },
  "Logging": {
    "Console": {
      "Enabled": true,
      "MinimumLevel": "Information"
    },
    "File": {
      "Enabled": false,
      "Path": "logs/mt5bridge-.log",
      "MinimumLevel": "Debug",
      "RollingInterval": "Day",
      "RetainedFileCountLimit": 7
    }
  }
}
```

---

### 🎯 Usage Examples

#### Getting Help

To see all available commands and options:
```bash
MT5Bridge.Manager.Demo --help
```

To see help for a specific command:
```bash
MT5Bridge.Manager.Demo listen --help
```

#### Query Commands

**List All Groups:**
```bash
MT5Bridge.Manager.Demo query-groups -s localhost:443 -l 1001 -p password
```

**Query User Information:**
```bash
MT5Bridge.Manager.Demo query-user --user-login 12345 -s localhost:443 -l 1001 -p password
```

**Query Account Balance:**
```bash
MT5Bridge.Manager.Demo query-account --user-login 12345
```

**Query Deal History (Last 7 Days):**
```bash
MT5Bridge.Manager.Demo query-deals --user-login 12345 --days 7
```

**Query Balance Operations (Last 30 Days):**
```bash
MT5Bridge.Manager.Demo query-balance --user-login 12345 --days 30
```

#### Listen Commands

**Subscribe to Deal Events (Default):**
```bash
MT5Bridge.Manager.Demo listen --types deal
```

**Subscribe to Specific Events:**
Use comma-separated values for `--types` (or `-t`).
Available types: `deal`, `order`, `position`, `all`.

```bash
# Listen to deals and orders
MT5Bridge.Manager.Demo listen --types deal,order

# Listen to all events
MT5Bridge.Manager.Demo listen --types all
```

**Subscribe using Protobuf Models (Binary):**
```bash
MT5Bridge.Manager.Demo listen --mode protobuf --types deal
```

**Mixed Handlers (Multiple Serialization Formats):**
```bash
MT5Bridge.Manager.Demo listen --mode mixed --types deal,order
```

Press any key to stop listening.

#### Trade Commands

**Deposit Funds:**
```bash
MT5Bridge.Manager.Demo deposit --user-login 12345 --amount 1000.00 --comment "Initial deposit"
```

**Withdraw Funds:**
```bash
MT5Bridge.Manager.Demo withdraw --user-login 12345 --amount 500.00 --comment "Withdrawal request"
```

---

### 🏗️ Project Structure

```
MT5Bridge.MT5.Demo/
├── Program.cs                      # Main entry point, CLI setup
├── appsettings.json                # Configuration (create from example)
├── appsettings.example.json        # Configuration template
├── Commands/
│   ├── BaseCommand.cs              # Base class with common functionality
│   ├── QueryCommand.cs             # Query commands implementation
│   ├── ListenCommand.cs            # Event listening implementation
│   └── TradeCommand.cs             # Trade operations implementation
├── publish.ps1                     # Publishing script
├── .doc/
└── README.md                       # This file
```

---

### 🔧 Command-Line Options

#### Global Options

Available for all commands:

| Option       | Alias | Description        | Example          |
| ------------ | ----- | ------------------ | ---------------- |
| `--server`   | `-s`  | MT5 server address | `localhost:443`  |
| `--login`    | `-l`  | Manager login      | `1001`           |
| `--password` | `-p`  | Manager password   | `SecurePassword` |

**Note:** Options override values in `appsettings.json`.

#### Command-Specific Options

**Query Deals:**
- `--days` / `-d`: Number of days to query (default: 7)

**Query Balance History:**
- `--days` / `-d`: Number of days to query (default: 30)

**Listen Commands:**
- `--types` / `-t`: Event types to subscribe to (default: `deal`)
  - Values: `deal`, `order`, `position`, `all`
  - Multiple types: `--types deal,order,position`

**Trade Commands:**
- `--comment` / `-c`: Operation comment (optional)

---

### 🔗 Related Projects

- **[MT5Bridge.Manager](../MT5Bridge.Manager/)** - Core library
- **[MT5Bridge.Core](../MT5Bridge.Core/)** - Foundation utilities
- **[MT5Bridge.Serilog](../MT5Bridge.Serilog/)** - Logging infrastructure
- **[MT5Bridge.Manager.Tests](../MT5Bridge.Manager.Tests/)** - Unit tests

---

### 📚 Additional Documentation

- [Configuration Guide](README-CONFIG.md) - Detailed configuration options

---

<a name="chinese"></a>

## 中文文档

MT5Bridge.Manager 库的命令行演示应用程序，展示查询操作、实时事件监听和交易操作。

**框架**: .NET 8.0 | **平台**: x64 | **接口**: CLI (CommandLineParser) | **状态**: 生产就绪

---

### 🚀 特性

#### 命令分类

1. **查询命令** - 检索 MT5 服务器数据
   - `query-groups` - 列出所有用户组
   - `query-user --user-login <login>` - 查询用户信息
   - `query-account --user-login <login>` - 查询账户余额和净值
   - `query-deals --user-login <login>` - 查询用户交易历史
   - `query-balance --user-login <login>` - 查询存取款历史

2. **监听命令** - 订阅实时事件
   - `listen --mode <mode> --types <types>`
     - 模式: `poco` (默认), `protobuf`, `mixed`
     - 类型: `deal` (默认), `order`, `position`, `all`

3. **交易命令** - 执行交易操作
   - `deposit --user-login <login> --amount <amount>` - 存款
   - `withdraw --user-login <login> --amount <amount>` - 取款

---

### 📦 安装

#### 前置要求
- .NET 8.0 SDK 或运行时
- 启用了 Manager API 的 MT5 服务器
- MetaQuotes MT5 SDK DLL（包含在 `libs/` 中）

#### 从源码构建

```bash
# 克隆仓库
cd mt5bridge

# 构建 Release 版本
dotnet build MT5Bridge.MT5.Demo/MT5Bridge.MT5.Demo.csproj -c Release

# 或构建所有项目
dotnet build mt5bridge.sln -c Release
```

#### 发布自包含可执行文件

```powershell
# Windows PowerShell
cd MT5Bridge.MT5.Demo
.\publish.ps1
```

这将在 `publish/` 文件夹中创建单文件可执行文件。

---

### ⚙️ 配置

#### 1. 创建配置文件

复制示例配置：

```bash
cp appsettings.example.json appsettings.json
```

#### 2. 编辑连接设置

编辑 `appsettings.json`：

```json
{
  "MT5Connection": {
    "Server": "localhost:443",
    "Login": "1001",
    "Password": "YourManagerPassword",
    "TimeoutMs": "30000"
  },
  "Logging": {
    "Console": {
      "Enabled": true,
      "MinimumLevel": "Information"
    },
    "File": {
      "Enabled": false,
      "Path": "logs/mt5bridge-.log",
      "MinimumLevel": "Debug",
      "RollingInterval": "Day",
      "RetainedFileCountLimit": 7
    }
  }
}
```

---

### 🎯 使用示例

#### 获取帮助

查看所有可用命令和选项：
```bash
MT5Bridge.Manager.Demo --help
```

查看特定命令的帮助：
```bash
MT5Bridge.Manager.Demo listen --help
```

#### 查询命令

**列出所有组：**
```bash
MT5Bridge.Manager.Demo query-groups -s localhost:443 -l 1001 -p password
```

**查询用户信息：**
```bash
MT5Bridge.Manager.Demo query-user --user-login 12345 -s localhost:443 -l 1001 -p password
```

**查询账户余额：**
```bash
MT5Bridge.Manager.Demo query-account --user-login 12345
```

**查询交易历史（最近 7 天）：**
```bash
MT5Bridge.Manager.Demo query-deals --user-login 12345 --days 7
```

**查询余额操作（最近 30 天）：**
```bash
MT5Bridge.Manager.Demo query-balance --user-login 12345 --days 30
```

#### 监听命令

**订阅成交事件（默认）：**
```bash
MT5Bridge.Manager.Demo listen --types deal
```

**订阅特定事件：**
使用逗号分隔的值指定 `--types`（或 `-t`）。
可用类型：`deal`、`order`、`position`、`all`。

```bash
# 监听成交和订单
MT5Bridge.Manager.Demo listen --types deal,order

# 监听所有事件
MT5Bridge.Manager.Demo listen --types all
```

**订阅成交事件（Protobuf - 二进制）：**
```bash
MT5Bridge.Manager.Demo listen --mode protobuf --types deal
```

**混合处理器（多种序列化格式）：**
```bash
MT5Bridge.Manager.Demo listen --mode mixed --types deal,order
```

按任意键停止监听。

#### 交易命令

**存款：**
```bash
MT5Bridge.Manager.Demo deposit --user-login 12345 --amount 1000.00 --comment "Initial deposit"
```

**取款：**
```bash
MT5Bridge.Manager.Demo withdraw --user-login 12345 --amount 500.00 --comment "Withdrawal request"
```

---

### 🏗️ 项目结构

```
MT5Bridge.MT5.Demo/
├── Program.cs                      # 主入口点，CLI 设置
├── appsettings.json                # 配置（从示例创建）
├── appsettings.example.json        # 配置模板
├── Commands/
│   ├── BaseCommand.cs              # 基类，包含通用功能
│   ├── QueryCommand.cs             # 查询命令实现
│   ├── ListenCommand.cs            # 事件监听实现
│   └── TradeCommand.cs             # 交易操作实现
├── publish.ps1                     # 发布脚本
└── README.md                       # 本文件
```

---

### 🔧 命令行选项

#### 全局选项

所有命令可用：

| 选项         | 别名 | 描述           | 示例             |
| ------------ | ---- | -------------- | ---------------- |
| `--server`   | `-s` | MT5 服务器地址 | `localhost:443`  |
| `--login`    | `-l` | Manager 登录   | `1001`           |
| `--password` | `-p` | Manager 密码   | `SecurePassword` |

**注意：** 选项覆盖 `appsettings.json` 中的值。

#### 命令特定选项

**Query Deals：**
- `--days` / `-d`：查询天数（默认：7）

**Query Balance History：**
- `--days` / `-d`：查询天数（默认：30）

**Listen Commands：**
- `--types` / `-t`：订阅的事件类型（默认：`deal`）
  - 值：`deal`、`order`、`position`、`all`
  - 多个类型：`--types deal,order,position`

**Trade Commands：**
- `--comment` / `-c`：操作备注（可选）

---

### 🔗 相关项目

- **[MT5Bridge.Manager](../MT5Bridge.Manager/)** - 核心库
- **[MT5Bridge.Core](../MT5Bridge.Core/)** - 基础工具
- **[MT5Bridge.Serilog](../MT5Bridge.Serilog/)** - 日志基础设施
- **[MT5Bridge.Manager.Tests](../MT5Bridge.Manager.Tests/)** - 单元测试

---

### 📚 其他文档

- [配置指南](README-CONFIG.md) - 详细配置选项
