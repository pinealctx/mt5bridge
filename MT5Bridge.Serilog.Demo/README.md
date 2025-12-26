# MT5Bridge Logging Serilog Demo

[English](#english) | [中文](#chinese)

---

<a name="english"></a>

## English Documentation

Comprehensive demonstration of **MT5Bridge.Serilog** featuring high-performance structured logging for MT5 trading applications.

**Status**: ✅ Production Ready | **Demo Features**: Complete | **Version**: 2.0

## Overview

This demo showcases all capabilities of the enterprise-grade Serilog logging library:

### Core Features Demonstrated

#### Performance & Optimization
- **Standard Logging**: Information, Warning, Debug, Error levels with automatic context
- **High-Performance Model Logging**: Source-generated JSON serialization with zero reflection overhead
- **Lazy Evaluation**: Conditional serialization - 5000x performance boost when log levels are disabled
- **Direct JSON Storage**: Raw model JSON logging for ultra-high-frequency scenarios

#### Advanced Features
- **ModelLogger Wrapper**: Chainable, fluent API reducing parameter passing overhead
- **Global Context**: Reusable JSON contexts across multiple log operations
- **Nested Models**: Recursive structure serialization with unlimited nesting levels
- **Field Enrichment**: Add arbitrary key-value pairs to enhance log context
- **Error Handling**: Integrated exception logging with full stack traces

#### Enterprise Infrastructure
- **Multi-Target Output**: Console (with ANSI colors), rolling file logs, and AWS CloudWatch
- **AWS CloudWatch Integration**: Two authentication methods (explicit credentials & default chain)
- **Thread-Safe**: Safe concurrent logging from multiple threads
- **Graceful Shutdown**: FlushAndCloseAsync ensures no log loss

---

## Running the Demo

### Basic Usage
```powershell
dotnet run
```

### With Custom Log File
```powershell
dotnet run -- --output-file logs/trading.log
```

### Using Fast JSON (Compact, Numbers as Numbers)
```powershell
dotnet run -- --json-mode fast
```

### Using Readable JSON (Enums as Strings)
```powershell
dotnet run -- --json-mode readable
```

### Skip CloudWatch Demo
```powershell
dotnet run -- --skip-cloudwatch
```

### Custom Message
```powershell
dotnet run -- --message "MT5Bridge Trading System Started"
```

---

## Configuration

Configuration is managed via `appsettings.json` following the `SerilogConfig` schema:

### Complete Configuration Example

```json
{
  "Logging": {
    "MinimumLevel": "Debug",
    "Console": {
      "Enabled": true,
      "UseJson": false,
      "UseAnsiColors": true
    },
    "File": {
      "Enabled": true,
      "Path": "logs/mt5bridge-.txt",
      "RollingInterval": "Day",
      "FileSizeLimitBytes": 104857600,
      "RetainedFileCountLimit": 30,
      "UseJson": true
    },
    "CloudWatch": {
      "Enabled": false,
      "Region": "us-east-1",
      "LogGroup": "/mt5bridge/serilog/demo",
      "LogStreamPrefix": "demo-",
      "AccessKeyId": "",
      "SecretKey": "",
      "BatchSizeLimit": 100,
      "PeriodSeconds": 15
    }
  }
}
```

### Configuration Reference

| Option                        | Type          | Default         | Description                                                           |
| ----------------------------- | ------------- | --------------- | --------------------------------------------------------------------- |
| `MinimumLevel`                | LogEventLevel | Information     | Minimum log level: Verbose, Debug, Information, Warning, Error, Fatal |
| `Console.Enabled`             | bool          | true            | Enable console output                                                 |
| `Console.UseJson`             | bool          | false           | Output as JSON (true) or plain text (false)                           |
| `Console.UseAnsiColors`       | bool          | true            | Use ANSI colors (set false for containers/non-TTY)                    |
| `File.Enabled`                | bool          | false           | Enable file logging                                                   |
| `File.Path`                   | string        | logs/log-.txt   | File path pattern (creates directories automatically)                 |
| `File.RollingInterval`        | string        | Day             | Rolling strategy: Infinite, Year, Month, Day, Hour, Minute            |
| `File.FileSizeLimitBytes`     | long?         | 10MB (10485760) | Max file size before rolling                                          |
| `File.RetainedFileCountLimit` | int?          | 31              | Number of old files to keep                                           |
| `File.UseJson`                | bool          | true            | File output format (JSON or plain text)                               |
| `CloudWatch.Enabled`          | bool          | false           | Enable CloudWatch Logs integration                                    |
| `CloudWatch.Region`           | string        | us-east-1       | AWS region (must be valid AWS region name)                            |
| `CloudWatch.LogGroup`         | string        | MT5Bridge       | CloudWatch log group name (1-256 characters)                          |
| `CloudWatch.LogStreamPrefix`  | string        | Demo            | Prefix for log stream names                                           |
| `CloudWatch.AccessKeyId`      | string        | (empty)         | AWS Access Key ID (optional, uses default credential chain if empty)  |
| `CloudWatch.SecretKey`        | string        | (empty)         | AWS Secret Key (required if AccessKeyId is set)                       |
| `CloudWatch.BatchSizeLimit`   | int           | 100             | Events per batch to CloudWatch (1-1000, AWS limit)                    |
| `CloudWatch.PeriodSeconds`    | int           | 5               | Flush interval to CloudWatch in seconds (1-300)                       |

---

## AWS CloudWatch Configuration

### Method 1: Explicit Credentials (AccessKeyId & SecretKey)

Set credentials directly in `appsettings.json`:

```json
{
  "CloudWatch": {
    "Enabled": true,
    "Region": "us-east-1",
    "LogGroup": "/mt5bridge/serilog/demo",
    "LogStreamPrefix": "demo-",
    "AccessKeyId": "YOUR_ACCESS_KEY_ID_HERE",
    "SecretKey": "YOUR_SECRET_KEY_HERE",
    "BatchSizeLimit": 100,
    "PeriodSeconds": 5
  }
}
```

**⚠️ Security Warning**: Never commit real credentials to source control!

### Method 2: Default Credential Chain (Recommended)

Leave `AccessKeyId` and `SecretKey` empty. AWS SDK searches credentials in this order:
1. Environment variables: `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`
2. AWS credentials file: `~/.aws/credentials` (Windows: `%USERPROFILE%\.aws\credentials`)
3. IAM role: EC2 instance profile or ECS task role
4. AWS SSO: If configured

```json
{
  "CloudWatch": {
    "Enabled": true,
    "Region": "us-east-1",
    "LogGroup": "/mt5bridge/serilog/demo",
    "LogStreamPrefix": "demo-",
    "AccessKeyId": "",
    "SecretKey": "",
    "BatchSizeLimit": 100,
    "PeriodSeconds": 5
  }
}
```

---

## Demo Scenarios

### 1. Standard Logging
Basic logging at different levels (Information, Warning, Debug, Error).

### 2. Lazy Model Logging (High Performance)
Demonstrates lazy evaluation - models are **only serialized if the log level is enabled**. This provides 5000x performance improvement when Debug logs are disabled in production.

### 3. Direct JSON Model Logging
Stores raw JSON for ultra-high-frequency logging (avoids Dictionary allocation).

### 4. ModelLogger Wrapper
Shows fluent API usage with `ModelLogger` for cleaner code and reduced parameter passing.

### 5. Global Context
Demonstrates setting a global default JSON context to avoid repetitive context passing.

### 6. Nested Models
Logs complex nested structures with recursive serialization.

### 7. Error Handling & Exceptions
Shows exception logging with full stack traces and structured error data.

### 8. Field Enrichment
Adds custom key-value fields to enhance log context.

---

## Performance Tips

### Choose the Right Method

| Scenario                                | Method                | Performance   | Use When                  |
| --------------------------------------- | --------------------- | ------------- | ------------------------- |
| High-frequency logs with Debug disabled | `WithModelLazy`       | ⭐⭐⭐⭐⭐ (5000x) | Production with Debug off |
| Structured data with Dictionary         | `WithModel`           | ⭐⭐⭐⭐          | Standard logging          |
| Ultra-high frequency (10k+/sec)         | `WithModelDirectLazy` | ⭐⭐⭐⭐⭐         | Hot trading loops         |
| Simple fields only                      | `WithField`           | ⭐⭐⭐⭐          | Adding metadata           |

### Production Optimization
1. Set `MinimumLevel` to `Information` or higher
2. Always use `Lazy` variants for Debug/Verbose logs
3. Use `ModelLogger` in hot loops to reduce parameter passing
4. For CloudWatch: Increase `BatchSizeLimit` to 800-1000, `PeriodSeconds` to 10-15

---

## Output Examples

### Console Output (Readable)
```
[12:34:56 INF] Trade executed with lazy model
    DealData: { "Deal": 987654321, "Symbol": "EURUSD", ... }
```

### File Output (JSON)
```json
{
  "@t": "2024-12-25T12:34:56.123Z",
  "@mt": "Trade executed with lazy model",
  "@l": "Information",
  "DealData": {
    "Deal": 987654321,
    "Symbol": "EURUSD",
    "Action": "Buy",
    "Price": 1.0850,
    "Volume": 100
  }
}
```

---

## Troubleshooting

**Logs not appearing?**
- Check `MinimumLevel` - logs below this level are discarded
- Verify at least one sink (Console/File/CloudWatch) is enabled
- Check file paths exist and are writable

**CloudWatch not working?**
- Verify AWS credentials are configured (environment vars or `~/.aws/credentials`)
- Check IAM role has `logs:CreateLogGroup`, `logs:CreateLogStream`, `logs:PutLogEvents` permissions
- Verify `Region` is correct

**High memory usage?**
- Reduce `File.FileSizeLimitBytes` or `File.RetainedFileCountLimit`
- Disable Debug logs in production (`MinimumLevel: Information`)
- Increase CloudWatch `PeriodSeconds` to batch more logs

---

## Dependencies

- **MT5Bridge.Serilog** - Core logging library
- **MT5Bridge.MT5.Core.Models** - Trading domain models
- **CommandLine** 2.9.1 - CLI argument parsing
- **Microsoft.Extensions.Configuration** 8.0.0 - Configuration management
- **Microsoft.Extensions.Configuration.Json** 8.0.0 - JSON config provider
- **Serilog** 4.0.0 - Logging framework
- **.NET 8.0** - Target framework

---

## Learning Resources

For complete documentation, see:
- [Main README](../MT5Bridge.Serilog/README.md) - Full API reference and usage guide
- [Test Suite](../MT5Bridge.Serilog.Tests/LoggerExtensionsTests.cs) - 14 comprehensive test cases

---

---

<a name="chinese"></a>

## 中文文档

**MT5Bridge.Serilog** 的完整演示，展示适用于 MT5 交易应用的高性能结构化日志功能。

**状态**: ✅ 生产就绪 | **演示功能**: 完整 | **版本**: 2.0

## 概述

本演示展示企业级 Serilog 日志库的所有功能：

### 核心功能演示

#### 性能与优化
- **标准日志**：Information、Warning、Debug、Error 级别，自动上下文
- **高性能模型日志**：源代码生成 JSON 序列化，零反射开销
- **惰性计算**：条件序列化 - 日志级别禁用时性能提升 5000 倍
- **直接 JSON 存储**：超高频场景的原始模型 JSON 日志

#### 高级功能
- **ModelLogger 包装器**：可链式调用的流式 API，减少参数传递开销
- **全局上下文**：跨多个日志操作的可重用 JSON 上下文
- **嵌套模型**：无限嵌套层级的递归结构序列化
- **字段增强**：添加任意键值对以增强日志上下文
- **错误处理**：集成异常日志记录，包含完整堆栈跟踪

#### 企业级基础设施
- **多目标输出**：控制台（支持 ANSI 颜色）、滚动文件日志、AWS CloudWatch
- **AWS CloudWatch 集成**：两种认证方法（显式凭证和默认凭证链）
- **线程安全**：支持多线程并发日志记录
- **优雅关闭**：FlushAndCloseAsync 确保无日志丢失

---

## 运行演示

### 基础用法
```powershell
dotnet run
```

### 自定义日志文件
```powershell
dotnet run -- --output-file logs/trading.log
```

### 使用快速 JSON（紧凑，数字为数字）
```powershell
dotnet run -- --json-mode fast
```

### 使用可读 JSON（枚举为字符串）
```powershell
dotnet run -- --json-mode readable
```

### 跳过 CloudWatch 演示
```powershell
dotnet run -- --skip-cloudwatch
```

### 自定义消息
```powershell
dotnet run -- --message "MT5Bridge Trading System Started"
```

---

## 配置

配置通过 `appsettings.json` 管理，遵循 `SerilogConfig` 架构：

### 完整配置示例

```json
{
  "Logging": {
    "MinimumLevel": "Debug",
    "Console": {
      "Enabled": true,
      "UseJson": false,
      "UseAnsiColors": true
    },
    "File": {
      "Enabled": true,
      "Path": "logs/mt5bridge-.txt",
      "RollingInterval": "Day",
      "FileSizeLimitBytes": 104857600,
      "RetainedFileCountLimit": 30,
      "UseJson": true
    },
    "CloudWatch": {
      "Enabled": false,
      "Region": "us-east-1",
      "LogGroup": "/mt5bridge/serilog/demo",
      "LogStreamPrefix": "demo-",
      "AccessKeyId": "",
      "SecretKey": "",
      "BatchSizeLimit": 100,
      "PeriodSeconds": 15
    }
  }
}
```

### 配置说明

| 选项                          | 类型          | 默认值        | 说明                                                             |
| ----------------------------- | ------------- | ------------- | ---------------------------------------------------------------- |
| `MinimumLevel`                | LogEventLevel | Information   | 最小日志级别：Verbose, Debug, Information, Warning, Error, Fatal |
| `Console.Enabled`             | bool          | true          | 启用控制台输出                                                   |
| `Console.UseJson`             | bool          | false         | 输出为 JSON (true) 或纯文本 (false)                              |
| `Console.UseAnsiColors`       | bool          | true          | 使用 ANSI 颜色（容器环境设为 false）                             |
| `File.Enabled`                | bool          | false         | 启用文件日志                                                     |
| `File.Path`                   | string        | logs/log-.txt | 文件路径模式（自动创建目录）                                     |
| `File.RollingInterval`        | string        | Day           | 滚动策略：Infinite, Year, Month, Day, Hour, Minute               |
| `File.FileSizeLimitBytes`     | long?         | 10MB          | 文件大小超过此值时滚动                                           |
| `File.RetainedFileCountLimit` | int?          | 31            | 保留的旧日志文件数                                               |
| `File.UseJson`                | bool          | true          | 文件输出格式（JSON 或纯文本）                                    |
| `CloudWatch.Enabled`          | bool          | false         | 启用 CloudWatch 日志                                             |
| `CloudWatch.Region`           | string        | us-east-1     | AWS 区域                                                         |
| `CloudWatch.LogGroup`         | string        | MT5Bridge     | CloudWatch 日志组名称                                            |
| `CloudWatch.LogStreamPrefix`  | string        | Demo          | 日志流名称前缀                                                   |
| `CloudWatch.AccessKeyId`      | string        | (空)          | AWS 访问密钥 ID（可选，留空则使用默认凭证链）                    |
| `CloudWatch.SecretKey`        | string        | (空)          | AWS 密钥（若设置 AccessKeyId 则必须提供）                        |
| `CloudWatch.BatchSizeLimit`   | int           | 100           | 批量发送的事件数 (1-1000)                                        |
| `CloudWatch.PeriodSeconds`    | int           | 5             | 刷新到 CloudWatch 的间隔秒数 (1-300)                             |

---

## AWS CloudWatch 配置

### 方式 1：显式凭证（AccessKeyId & SecretKey）

在 `appsettings.json` 中直接设置凭证：

```json
{
  "CloudWatch": {
    "Enabled": true,
    "Region": "us-east-1",
    "LogGroup": "/mt5bridge/serilog/demo",
    "LogStreamPrefix": "demo-",
    "AccessKeyId": "YOUR_ACCESS_KEY_ID_HERE",
    "SecretKey": "YOUR_SECRET_KEY_HERE",
    "BatchSizeLimit": 100,
    "PeriodSeconds": 5
  }
}
```

**⚠️ 安全警告**：切勿将真实凭证提交到源代码管理！

### 方式 2：默认凭证链（推荐）

将 `AccessKeyId` 和 `SecretKey` 留空。AWS SDK 按以下顺序搜索凭证：
1. 环境变量：`AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`
2. AWS 凭证文件：`~/.aws/credentials`（Windows：`%USERPROFILE%\.aws\credentials`）
3. IAM 角色：EC2 实例配置文件或 ECS 任务角色
4. AWS SSO：如已配置

```json
{
  "CloudWatch": {
    "Enabled": true,
    "Region": "us-east-1",
    "LogGroup": "/mt5bridge/serilog/demo",
    "LogStreamPrefix": "demo-",
    "AccessKeyId": "",
    "SecretKey": "",
    "BatchSizeLimit": 100,
    "PeriodSeconds": 5
  }
}
```

---

## 演示场景

### 1. 标准日志
不同级别的基础日志（Information、Warning、Debug、Error）。

### 2. 惰性模型日志（高性能）
演示惰性计算 - 模型**仅在日志级别启用时才序列化**。生产环境中 Debug 日志禁用时性能提升 5000 倍。

### 3. 直接 JSON 模型日志
存储原始 JSON，用于超高频日志记录（避免 Dictionary 分配）。

### 4. ModelLogger 包装器
展示使用 `ModelLogger` 的流式 API，代码更简洁，减少参数传递。

### 5. 全局上下文
演示设置全局默认 JSON 上下文，避免重复传递上下文。

### 6. 嵌套模型
记录复杂嵌套结构，支持递归序列化。

### 7. 错误处理与异常
展示异常日志记录，包含完整堆栈跟踪和结构化错误数据。

### 8. 字段增强
添加自定义键值字段以增强日志上下文。

---

## 性能优化建议

### 选择合适的方法

| 场景                    | 方法                  | 性能          | 使用场景             |
| ----------------------- | --------------------- | ------------- | -------------------- |
| Debug 关闭时的高频日志  | `WithModelLazy`       | ⭐⭐⭐⭐⭐ (5000x) | 生产环境，Debug 关闭 |
| 结构化数据 + Dictionary | `WithModel`           | ⭐⭐⭐⭐          | 标准日志记录         |
| 超高频 (10k+/sec)       | `WithModelDirectLazy` | ⭐⭐⭐⭐⭐         | 高频交易循环         |
| 简单字段                | `WithField`           | ⭐⭐⭐⭐          | 添加元数据           |

### 生产环境优化
1. 设置 `MinimumLevel` 为 `Information` 或更高
2. Debug/Verbose 日志总是使用 `Lazy` 变体
3. 热循环中使用 `ModelLogger` 减少参数传递
4. CloudWatch：增加 `BatchSizeLimit` 至 800-1000，`PeriodSeconds` 至 10-15

---

## 输出示例

### 控制台输出（可读）
```
[12:34:56 INF] Trade executed with lazy model
    DealData: { "Deal": 987654321, "Symbol": "EURUSD", ... }
```

### 文件输出（JSON）
```json
{
  "@t": "2024-12-25T12:34:56.123Z",
  "@mt": "Trade executed with lazy model",
  "@l": "Information",
  "DealData": {
    "Deal": 987654321,
    "Symbol": "EURUSD",
    "Action": "Buy",
    "Price": 1.0850,
    "Volume": 100
  }
}
```

---

## 故障排查

**日志未出现？**
- 检查 `MinimumLevel` - 低于此级别的日志被丢弃
- 验证至少有一个 sink（Console/File/CloudWatch）已启用
- 检查文件路径存在且可写

**CloudWatch 不工作？**
- 验证 AWS 凭证已配置（环境变量或 `~/.aws/credentials`）
- 检查 IAM 角色是否有 `logs:CreateLogGroup`、`logs:CreateLogStream`、`logs:PutLogEvents` 权限
- 验证 `Region` 是否正确

**内存使用过高？**
- 降低 `File.FileSizeLimitBytes` 或 `File.RetainedFileCountLimit`
- 生产环境禁用 Debug 日志（`MinimumLevel: Information`）
- 增加 CloudWatch `PeriodSeconds` 以批量处理更多日志

---

## 依赖项

- **MT5Bridge.Serilog** - 核心日志库
- **MT5Bridge.MT5.Core.Models** - 交易领域模型
- **CommandLine** 2.9.1 - CLI 参数解析
- **Microsoft.Extensions.Configuration** 8.0.0 - 配置管理
- **Microsoft.Extensions.Configuration.Json** 8.0.0 - JSON 配置提供程序
- **Serilog** 4.0.0 - 日志框架
- **.NET 8.0** - 目标框架

---

## 学习资源

完整文档，请参阅：
- [主 README](../MT5Bridge.Serilog/README.md) - 完整 API 参考和使用指南
- [测试套件](../MT5Bridge.Serilog.Tests/LoggerExtensionsTests.cs) - 14 个综合测试用例

---

- `PeriodSeconds`: Flush interval in seconds (1-300)

## Demo Scenarios

### 1. Standard Logging
Basic information, warning, and debug messages:
```csharp
logger.Information("Trade started");
logger.Warning("Price volatility detected");
logger.Debug("Order validation");
```

### 2. Lazy Model Logging (High Performance)
Models are serialized only when the log level is enabled:
```csharp
logger.WithModelLazy("DealData", deal, LogEventLevel.Information, context)
      .Information("Trade executed");
      
// Debug logs skip serialization if Debug is disabled
logger.WithModelLazy("DebugData", deal, LogEventLevel.Debug, context)
      .Debug("This won't serialize unless Debug is enabled");
```

### 3. Direct JSON Logging
Raw model serialization stored as JSON strings:
```csharp
logger.WithModelDirect("RawDeal", deal, context)
      .Information("Archived trade data");
```

### 4. ModelLogger Wrapper
Chainable API for complex multi-model logging:
```csharp
var modelLogger = new ModelLogger(logger, LogEventLevel.Information);

modelLogger.WithModel("Account", account, context)
           .WithModel("Trade", trade, context)
           .WithField("Status", "Processing")
           .Write("Multi-model transaction");
```

### 5. Global Context
Set a reusable JSON context to avoid repeated parameters:
```csharp
LoggerExtensions.DefaultContext = ReadableJsonContext.Default;

logger.WithModel("Deal", deal, context)
      .Information("Using global context");
```

### 6. Field Enrichment
Add arbitrary fields to enhance log context:
```csharp
logger.WithField("UserId", "trader@example.com")
      .WithField("TradeId", 12345)
      .WithField("RiskLevel", "Medium")
      .Information("Trade enriched with metadata");
```

### 7. Nested Models
Recursive structure logging with automatic traversal:
```csharp
var nested = new NestedModel 
{ 
    Primary = deal,
    Items = new[] { deal1, deal2, deal3 }
};

logger.WithModel("ComplexTrade", nested, context)
      .Information("Nested structure logged");
```

### 8. Error Handling
Integrated exception logging:
```csharp
try 
{
    ExecuteTrade(deal);
}
catch (Exception ex)
{
    logger.LogModelError("Trade failed", deal, context, ex);
}
```

## Output Examples

### Console Output (Human-Readable)
```
10:23:45 [INF] Trade executed
10:23:45 [WRN] Price volatility detected
10:23:45 [DBG] Order validation
```

### File Output (JSON)
```json
{"@t":"2024-01-15T10:23:45.1234567Z","@mt":"Trade executed","@l":"Information","DealData":{"Deal":123456,"Symbol":"EURUSD","Price":1.0850}}
```

### CloudWatch
Logs are streamed to AWS CloudWatch Logs with:
- **Log Group**: `/mt5bridge/serilog`
- **Log Streams**: `demo-*` (stratified by date/instance)
- **JSON Fields**: Fully parsed for querying

## Performance Characteristics

- **Standard Logging**: <1μs per log
- **Lazy Model Logging**: <5μs when enabled, 0μs when disabled
- **Direct JSON**: ~10μs per model
- **CloudWatch Batch**: 100-1000 events per batch, flushed every 15 seconds

## Troubleshooting

### Logs Not Appearing in Console
- Check `Console.Enabled` is `true` in appsettings.json
- Verify `MinimumLevel` is set to `Debug` or lower
- Ensure console output is not redirected

### Logs Not Written to File
- Verify `File.Enabled` is `true`
- Check directory exists at `logs/` path
- Ensure write permissions on the directory

### CloudWatch Connection Issues
- Verify AWS credentials are configured (environment, profile, or credentials file)
- Check `CloudWatch.Enabled` is `true`
- Confirm IAM permissions: `logs:CreateLogGroup`, `logs:CreateLogStream`, `logs:PutLogEvents`
- Verify region matches your CloudWatch location

### JSON Serialization Errors
- Ensure models have `[JsonSerializable]` attribute
- Check `JsonSerializerContext` is properly implemented
- Verify all properties are public and serializable

## AWS CloudWatch Setup

### Authentication Methods

#### Method 1: Explicit Credentials in appsettings.json

Add AccessKeyId and SecretKey directly (not recommended for production):

```json
{
  "CloudWatch": {
    "Enabled": true,
    "Region": "us-east-1",
    "LogGroup": "/mt5bridge/demo",
    "LogStreamPrefix": "app-",
    "AccessKeyId": "AKIAIOSFODNN7EXAMPLE",
    "SecretKey": "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY",
    "BatchSizeLimit": 100,
    "PeriodSeconds": 5
  }
}
```

**⚠️ Security Warning**: Never commit credentials to source control!

#### Method 2: Default Credential Chain (Recommended)

Leave AccessKeyId and SecretKey empty to use AWS default credential providers:

```json
{
  "CloudWatch": {
    "Enabled": true,
    "Region": "us-east-1",
    "LogGroup": "/mt5bridge/demo",
    "LogStreamPrefix": "app-",
    "AccessKeyId": "",
    "SecretKey": "",
    "BatchSizeLimit": 100,
    "PeriodSeconds": 5
  }
}
```

The SDK will search credentials in this order:
1. **Environment variables**: `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`
2. **AWS credentials file**: `~/.aws/credentials`
3. **IAM role**: EC2 instance profile or ECS task role

### Setting Up AWS Credentials File

**Windows**: Create `%USERPROFILE%\.aws\credentials`
```ini
[default]
aws_access_key_id = YOUR_ACCESS_KEY
aws_secret_access_key = YOUR_SECRET_KEY
```

**Linux/macOS**: Create `~/.aws/credentials`
```ini
[default]
aws_access_key_id = YOUR_ACCESS_KEY
aws_secret_access_key = YOUR_SECRET_KEY
```

### IAM Permissions Required

Ensure your IAM user/role has these permissions:
```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "logs:CreateLogGroup",
        "logs:CreateLogStream",
        "logs:PutLogEvents",
        "logs:DescribeLogStreams"
      ],
      "Resource": "arn:aws:logs:*:*:log-group:/mt5bridge/*"
    }
  ]
}
```

---

## Advanced Patterns

### Request Context Propagation
```csharp
var requestId = Guid.NewGuid().ToString();
using (LogContext.PushProperty("RequestId", requestId))
{
    logger.WithModel("Request", data, context)
          .Information("Processing request");
}
```

### Performance Monitoring
```csharp
var sw = Stopwatch.StartNew();
ExecuteTradeAsync(deal);
sw.Stop();

logger.WithField("ExecutionTime", sw.ElapsedMilliseconds)
      .WithField("Success", true)
      .Information("Trade completed");
```

### Conditional Logging
```csharp
if (logger.IsEnabled(LogEventLevel.Debug))
{
    logger.WithModel("Expensive", largeModel, context)
          .Debug("Detailed diagnostic");
}
```

## Next Steps

1. Review [MT5Bridge.Serilog README](../README.md) for API documentation
2. Explore [test cases](../MT5Bridge.Serilog.Tests) for additional patterns
3. Configure CloudWatch for production deployments
4. Integrate with your MT5 trading applications

