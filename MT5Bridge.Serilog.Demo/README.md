# MT5Bridge Serilog Performance Test Demo

[English](#english) | [中文](#chinese)

---

<a name="english"></a>

## English Documentation

Performance testing tool for **MT5Bridge.Serilog** that continuously logs comprehensive trading deal data and measures logging performance across different model types.

**Status**: ✅ Production Ready | **Purpose**: Performance Testing & Benchmarking

## Overview

This demo is a performance testing tool that:
- Continuously logs comprehensive structured trading deal data (30+ fields per log)
- Measures **pure logging performance** (excludes data generation time)
- Tests both **POCO** and **Protobuf** model serialization performance
- Provides detailed performance statistics with percentile analysis

### Features

#### Model Type Support
- **POCO Model**: `MT5Bridge.Manager.Models.DealModel` - Hand-written C# classes
- **Protobuf Model**: `MT5Bridge.Manager.Models.Proto.DealModel` - Protocol Buffers generated models
- **Performance Comparison**: Compare serialization overhead between POCO and Protobuf

#### Comprehensive Field Logging (30+ Fields)
Logs all DealModel fields except `ApiData` collection:
- **Core**: Deal, ExternalId, Login, Dealer, Order, Symbol, Action, Entry, Reason
- **Pricing**: Price, PricePosition, PriceSl, PriceTp, PriceGateway, PriceSLOnOpen, PriceTPOnOpen
- **Volume**: Volume, VolumeClosed, VolumeClosedExt, VolExt
- **Financial**: Profit, ProfitRaw, Commission, CommissionAgent, Storage, Fee
- **Market Data**: MarketBid, MarketAsk, MarketLast, TickValue, TickSize
- **Rates**: RateProfit, RateMargin
- **Metadata**: Time, TimeMsc, Digits, DigitsCurrency, ContractSize, ExpertId, PositionId, Comment, Gateway, PriceSource, Flags, ModificationFlags

#### Performance Measurement
- **Isolated Measurement**: Uses `Stopwatch` to measure **only** `logger.Information()` call time
- **Throughput**: Logs per second measurement
- **Latency Statistics**: Average, Min, Max, 95th percentile, 99th percentile
- **Configurable Duration**: Specify test duration in seconds
- **Variable Interval**: Control logging frequency with min/max intervals (milliseconds)

#### Logging Configuration
- **Structured Logging**: Native Serilog template syntax with typed parameters
- **Random Data**: Generates realistic trading data (8 currency pairs, varied actions/reasons/entries)
- **Multiple Sinks**: Test Console, File, and CloudWatch simultaneously

---

## Running the Demo

### POCO Model Performance Test
```powershell
dotnet run -- -c appsettings.json -d 10 -m poco --min-interval 10 --max-interval 50
```

### Protobuf Model Performance Test
```powershell
dotnet run -- -c appsettings.json -d 10 -m protobuf --min-interval 10 --max-interval 50
```

### Model Comparison (Run both sequentially)
```powershell
# Test POCO
dotnet run -- -c appsettings.json -d 30 -m poco --min-interval 10 --max-interval 50

# Test Protobuf
dotnet run -- -c appsettings.json -d 30 -m protobuf --min-interval 10 --max-interval 50
```

### High-Frequency Test (1-10ms interval)
```powershell
dotnet run -- -c appsettings.json -d 30 -m poco --min-interval 1 --max-interval 10
```

### Maximum Throughput Test (no delay between logs)
```powershell
dotnet run -- -c appsettings.json -d 5 -m protobuf --min-interval 0 --max-interval 0
```

### Long-Running Stress Test (5 minutes)
```powershell
dotnet run -- -c appsettings.json -d 300 -m poco --min-interval 10 --max-interval 50
```

### View Help
```powershell
dotnet run -- --help
```

### Command Line Options

| Option           | Short | Default  | Description                            |
| ---------------- | ----- | -------- | -------------------------------------- |
| `--config`       | `-c`  | Required | Path to appsettings configuration file |
| `--duration`     | `-d`  | 10       | Test duration in seconds               |
| `--model-type`   | `-m`  | poco     | Model type: "poco" or "protobuf"       |
| `--min-interval` |       | 1        | Minimum interval between logs (ms)     |
| `--max-interval` |       | 100      | Maximum interval between logs (ms)     |

---

## Sample Output

### POCO Model Test
```
MT5Bridge Serilog Performance Test
===================================

Configuration:
  Model Type: POCO
  Duration: 5 seconds
  Interval: 10-50 ms
  Min Log Level: Debug
  Sinks: Console=True, File=True, CloudWatch=False

Starting performance test (POCO Model)...

[2025-12-27 13:15:42.419 INF] Deal: 295542662 ExtID:EXT-77783 Login:8112 Dealer:633 Order:333491953 Symbol:USDCAD Action:Buy Entry:InOut Reason:Dealer Price:1.4923534669025087 Vol:634 VolClosed:320 Profit:489.77044577618943 ProfitRaw:453.6684564241047 Storage:5.826580300267928 Comm:21.486279155171168 Fee:3.336350507670357 SL:0 TP:1.4947983602483417 PricePos:1.405938611131992 Time:1766812542 TimeMsc:1766812542416 RateProfit:1.0955829318030397 RateMargin:1.0417620228935245 TickVal:0.9632615850441728 TickSize:1E-05 Bid:1.413021058197144 Ask:1.450098168232524 Last:1.3175127803174644 ExpertID:51943 PosID:273904275 Comment:

[2025-12-27 13:15:42.553 INF] Deal: 920496691 ExtID:EXT-34915 Login:3284 Dealer:979 Order:720926607 Symbol:EURUSD Action:Buy Entry:In Reason:TP Price:1.2726597998894493 Vol:494 VolClosed:319 Profit:383.38014000528744 ProfitRaw:-190.08936864185867 Storage:7.364733749697217 Comm:10.921450327536137 Fee:1.082227091430767 SL:1.3131671278944228 TP:0 PricePos:1.4725140543474786 Time:1766812542 TimeMsc:1766812542553 RateProfit:1.0311684213215158 RateMargin:1.0018967669267882 TickVal:0.4422332141153029 TickSize:1E-05 Bid:1.2207804616876015 Ask:1.217843415929545 Last:1.0848996165474356 ExpertID:32296 PosID:252074094 Comment:Test comment

============================================================
Performance Statistics
============================================================
Total Logs:        96
Total Duration:    5.02 seconds
Logs/Second:       19.12
Avg Log Time:      15.8589 ms
Min Log Time:      3.7462 ms
Max Log Time:      92.5346 ms
95th Percentile:   23.5538 ms
99th Percentile:   92.5346 ms
============================================================

Test completed successfully!
```

### Protobuf Model Test
```
MT5Bridge Serilog Performance Test
===================================

Configuration:
  Model Type: PROTOBUF
  Duration: 5 seconds
  Interval: 10-50 ms
  Min Log Level: Debug
  Sinks: Console=True, File=True, CloudWatch=False

Starting performance test (Protobuf Model)...

[2025-12-27 13:15:58.681 INF] Deal: 208109769 ExtID:EXT-44695 Login:7669 Dealer:993 Order:279143550 Symbol:EURJPY Action:Buy Entry:Out Reason:Tp Price:1.2479950802853992 Vol:732 VolClosed:129 Profit:-375.5147290907629 ProfitRaw:63.286268427389494 Storage:5.882244653451499 Comm:23.525950235867025 Fee:3.562220711504967 SL:1.3501712946663176 TP:1.4788714229291648 PricePos:1.2220268352887214 Time:1766812558 TimeMsc:1766812558677 RateProfit:1.062083003563762 RateMargin:1.0907097962952312 TickVal:3.0360837710192623 TickSize:1E-05 Bid:1.2059921317871771 Ask:1.1552521575296326 Last:1.4503175397116523 ExpertID:91645 PosID:532987517 Comment:Test comment

============================================================
Performance Statistics
============================================================
Total Logs:        109
Total Duration:    5.02 seconds
Logs/Second:       21.71
Avg Log Time:      10.7013 ms
Min Log Time:      2.9699 ms
Max Log Time:      69.4492 ms
95th Percentile:   19.8455 ms
99th Percentile:   24.8439 ms
============================================================

Test completed successfully!
```

### Performance Comparison Summary

Based on 5-second tests with 10-50ms intervals:

| Metric              | POCO Model | Protobuf Model | Difference |
| ------------------- | ---------- | -------------- | ---------- |
| **Total Logs**      | 96         | 109            | +13.5%     |
| **Logs/Second**     | 19.12      | 21.71          | +13.5%     |
| **Avg Latency**     | 15.86 ms   | 10.70 ms       | **-32.5%** |
| **Min Latency**     | 3.75 ms    | 2.97 ms        | -20.8%     |
| **Max Latency**     | 92.53 ms   | 69.45 ms       | -24.9%     |
| **95th Percentile** | 23.55 ms   | 19.85 ms       | -15.7%     |
| **99th Percentile** | 92.53 ms   | 24.84 ms       | **-73.2%** |

**Conclusion**: Protobuf model shows superior performance with **13.5% higher throughput** and **32.5% lower average latency**.

---

## Configuration Files for Testing

Create different `appsettings*.json` files for various test scenarios:

**appsettings.development.json** - Development with verbose logging:
```json
{
  "MinimumLevel": "Debug",
  "Console": { "Enabled": true, "MinimumLevel": "Debug" },
  "File": { "Enabled": true, "MinimumLevel": "Debug" },
  "CloudWatch": { "Enabled": false }
}
```

**appsettings.production.json** - Production with cost optimization:
```json
{
  "MinimumLevel": "Debug",
  "Console": { "Enabled": true, "MinimumLevel": "Information" },
  "File": { "Enabled": true, "MinimumLevel": "Debug" },
  "CloudWatch": { "Enabled": true, "MinimumLevel": "Warning" }
}
```

**appsettings.test.json** - Testing performance with minimal output:
```json
{
  "MinimumLevel": "Information",
  "Console": { "Enabled": true, "MinimumLevel": "Information" },
  "File": { "Enabled": true, "MinimumLevel": "Information" },
  "CloudWatch": { "Enabled": false }
}
```

---

## Configuration

Configuration is managed via `appsettings.json` following the `SerilogConfig` schema:

### Complete Configuration Example

```json
{
  "MinimumLevel": "Debug",
  "Console": {
    "Enabled": true,
    "TextFormatter": "plain",
    "UseAnsiColors": true
  },
  "File": {
    "Enabled": true,
    "Path": "logs/mt5bridge-.txt",
    "RollingInterval": "Day",
    "FileSizeLimitBytes": 104857600,
    "RetainedFileCountLimit": 30,
    "TextFormatter": "plain"
  },
  "CloudWatch": {
    "Enabled": false,
    "Region": "us-east-1",
    "LogGroup": "/mt5bridge/serilog/demo",
    "LogStreamPrefix": "demo-",
    "AccessKeyId": "",
    "SecretKey": "",
    "BatchSizeLimit": 100,
    "PeriodSeconds": 15,
    "LogStreamNamingStrategy": "configurable",
    "LogStreamIncludeHostname": true,
    "LogStreamIncludeGuid": true,
    "TextFormatter": "compact",
    "QueueSizeLimit": 10000,
    "RetryAttempts": 5
  }
}
```

### Configuration Reference

| Option                                | Type           | Default                   | Description                                                            |
| ------------------------------------- | -------------- | ------------------------- | ---------------------------------------------------------------------- |
| `MinimumLevel`                        | LogEventLevel  | Information               | Minimum log level: Verbose, Debug, Information, Warning, Error, Fatal  |
| `Console.Enabled`                     | bool           | true                      | Enable console output                                                  |
| `Console.TextFormatter`               | string         | "" (plain)                | Format: "plain" or empty (text), "json", "compact", "rendered-compact" |
| `Console.UseAnsiColors`               | bool           | true                      | Use ANSI colors (set false for containers/non-TTY)                     |
| `Console.MinimumLevel`                | LogEventLevel? | null (global)             | Console-specific log level (overrides global MinimumLevel if set)      |
| `File.Enabled`                        | bool           | false                     | Enable file logging                                                    |
| `File.Path`                           | string         | logs/log-.txt             | File path pattern (creates directories automatically)                  |
| `File.RollingInterval`                | string         | Day                       | Rolling strategy: Infinite, Year, Month, Day, Hour, Minute             |
| `File.FileSizeLimitBytes`             | long?          | 10MB (10485760)           | Max file size before rolling                                           |
| `File.RetainedFileCountLimit`         | int?           | 31                        | Number of old files to keep                                            |
| `File.TextFormatter`                  | string         | "" (plain)                | Format: "plain" or empty (text), "json", "compact", "rendered-compact" |
| `File.MinimumLevel`                   | LogEventLevel? | null (global)             | File-specific log level (overrides global MinimumLevel if set)         |
| `CloudWatch.Enabled`                  | bool           | false                     | Enable CloudWatch Logs integration                                     |
| `CloudWatch.Region`                   | string         | (empty)                   | AWS region (required when enabled, e.g., us-east-1, eu-west-1)         |
| `CloudWatch.LogGroup`                 | string         | (empty)                   | CloudWatch log group name (required when enabled, 1-256 characters)    |
| `CloudWatch.CreateLogGroup`           | bool           | false                     | Whether to create the log group if it doesn't exist                    |
| `CloudWatch.LogStreamPrefix`          | string         | (empty)                   | Prefix for log stream names (optional)                                 |
| `CloudWatch.AccessKeyId`              | string         | (empty)                   | AWS Access Key ID (optional, uses default credential chain if empty)   |
| `CloudWatch.SecretKey`                | string         | (empty)                   | AWS Secret Access Key (required if AccessKeyId is set)                 |
| `CloudWatch.BatchSizeLimit`           | int            | 100                       | Events per batch to CloudWatch (1-1000, AWS limit)                     |
| `CloudWatch.PeriodSeconds`            | int            | 5                         | Flush interval to CloudWatch in seconds (1-300)                        |
| `CloudWatch.LogStreamNamingStrategy`  | string         | configurable              | Log stream naming strategy: "default", "constant", "configurable"      |
| `CloudWatch.LogStreamIncludeHostname` | bool           | true                      | Include hostname in log stream name (for "configurable" strategy)      |
| `CloudWatch.LogStreamIncludeGuid`     | bool           | true                      | Include GUID in log stream name (for "configurable" strategy)          |
| `CloudWatch.TextFormatter`            | string         | compact                   | Text formatter: "json", "compact" (recommended), "rendered-compact"    |
| `CloudWatch.QueueSizeLimit`           | int            | 10000                     | Maximum queue size before dropping events (100-100000)                 |
| `CloudWatch.RetryAttempts`            | byte           | 5                         | Number of retry attempts for failed uploads (0-255)                    |
| `CloudWatch.MinimumLevel`             | LogEventLevel? | null (global)             | CloudWatch-specific log level (overrides global MinimumLevel if set)   |
| `Diagnostics.ThrottleWindowSeconds`   | int            | 300                       | Time window in seconds for throttling Serilog internal errors          |
| `Diagnostics.ThrottleLimit`           | int            | 100                       | Max internal error messages per window (0 = disable diagnostic output) |
| `Diagnostics.Console.Enabled`         | bool           | false                     | Enable internal diagnostic logging to Console                          |
| `Diagnostics.File.Enabled`            | bool           | false                     | Enable internal diagnostic logging to a file                           |
| `Diagnostics.File.Path`               | string         | logs/serilog-internal.log | Path for internal diagnostic logs                                      |

---

## Per-Sink Log Level Configuration

Each sink (Console, File, CloudWatch) can have its own minimum log level, enabling fine-grained control over what gets logged where.

### Configuration Strategy

**Global MinimumLevel**: Acts as the first filter - log events below this level are never created (performance optimization).

**Sink-specific MinimumLevel**: Optional override for each sink. If not set (null), the sink uses the global level.

### Example: Production Multi-Level Logging

```json
{
  "MinimumLevel": "Debug",        // Create all Debug+ events
  "Console": {
    "Enabled": true,
    "MinimumLevel": "Information"  // Console only shows Information+
  },
  "File": {
    "Enabled": true,
    "Path": "logs/app-.log",
    "MinimumLevel": "Debug"        // File captures everything (Debug+)
  },
  "CloudWatch": {
    "Enabled": true,
    "Region": "us-east-1",
    "LogGroup": "/production/mt5bridge",
    "MinimumLevel": "Warning",     // CloudWatch only stores Warning+ (cost optimization)
    "BatchSizeLimit": 500,
    "PeriodSeconds": 10
  }
}
```

### Common Scenarios

**Development Environment**: Console=Debug, File=Verbose, CloudWatch=Disabled
```json
{
  "MinimumLevel": "Verbose",
  "Console": { "Enabled": true, "MinimumLevel": "Debug" },
  "File": { "Enabled": true, "MinimumLevel": "Verbose" },
  "CloudWatch": { "Enabled": false }
}
```

**Production Environment**: Console=Information, File=Debug, CloudWatch=Warning
```json
{
  "MinimumLevel": "Debug",
  "Console": { "Enabled": true, "MinimumLevel": "Information" },
  "File": { "Enabled": true, "MinimumLevel": "Debug" },
  "CloudWatch": { "Enabled": true, "MinimumLevel": "Warning" }
}
```

**Troubleshooting Mode**: Console=Debug, File=Verbose, CloudWatch=Debug
```json
{
  "MinimumLevel": "Verbose",
  "Console": { "Enabled": true, "MinimumLevel": "Debug" },
  "File": { "Enabled": true, "MinimumLevel": "Verbose" },
  "CloudWatch": { "Enabled": true, "MinimumLevel": "Debug" }
}
```

### Benefits

- 🚀 **Performance**: Reduce console noise (Information+) while preserving detailed logs
- 💰 **Cost Optimization**: Send only critical logs to CloudWatch (Warning+) while File captures Debug
- 🔍 **Troubleshooting**: Different environments can have different logging strategies
- 🎯 **Flexibility**: Change logging strategies without modifying code - just configuration

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
    "PeriodSeconds": 5,
    "LogStreamNamingStrategy": "configurable",
    "LogStreamIncludeHostname": true,
    "LogStreamIncludeGuid": true,
    "TextFormatter": "compact",
    "QueueSizeLimit": 10000,
    "RetryAttempts": 5
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
    "PeriodSeconds": 5,
    "LogStreamNamingStrategy": "configurable",
    "LogStreamIncludeHostname": true,
    "LogStreamIncludeGuid": true,
    "TextFormatter": "compact",
    "QueueSizeLimit": 10000,
    "RetryAttempts": 5
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
- Check IAM role has `logs:CreateLogStream`, `logs:PutLogEvents` permissions.
- If `CreateLogGroup` is set to `true`, also requires `logs:CreateLogGroup` and `logs:DescribeLogGroups`.
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

**MT5Bridge.Serilog** 的性能测试工具，持续记录完整的交易成交数据并测量不同模型类型的日志性能。

**状态**: ✅ 生产就绪 | **用途**: 性能测试与基准测试

## 概述

本演示是一个性能测试工具：
- 持续记录完整的结构化交易成交数据（每条日志 30+ 字段）
- 测量**纯日志性能**（排除数据生成时间）
- 测试 **POCO** 和 **Protobuf** 模型序列化性能
- 提供详细的性能统计，包含百分位分析

### 功能特性

#### 模型类型支持
- **POCO 模型**: `MT5Bridge.Manager.Models.DealModel` - 手写 C# 类
- **Protobuf 模型**: `MT5Bridge.Manager.Models.Proto.DealModel` - Protocol Buffers 生成的模型
- **性能对比**: 比较 POCO 和 Protobuf 之间的序列化开销

#### 完整字段记录（30+ 字段）
记录除 `ApiData` 集合外的所有 DealModel 字段：
- **核心字段**: Deal、ExternalId、Login、Dealer、Order、Symbol、Action、Entry、Reason
- **价格相关**: Price、PricePosition、PriceSl、PriceTp、PriceGateway、PriceSLOnOpen、PriceTPOnOpen
- **成交量**: Volume、VolumeClosed、VolumeClosedExt、VolExt
- **财务数据**: Profit、ProfitRaw、Commission、CommissionAgent、Storage、Fee
- **市场数据**: MarketBid、MarketAsk、MarketLast、TickValue、TickSize
- **汇率**: RateProfit、RateMargin
- **元数据**: Time、TimeMsc、Digits、DigitsCurrency、ContractSize、ExpertId、PositionId、Comment、Gateway、PriceSource、Flags、ModificationFlags

#### 性能测量
- **隔离测量**: 使用 `Stopwatch` 仅测量 `logger.Information()` 调用时间
- **吞吐量**: 每秒日志数测量
- **延迟统计**: 平均值、最小值、最大值、95 分位数、99 分位数
- **可配置时长**: 指定测试持续时间（秒）
- **可变间隔**: 通过最小/最大间隔控制日志频率（毫秒）

#### 日志配置
- **结构化日志**: 原生 Serilog 模板语法和类型化参数
- **随机数据**: 生成真实的交易数据（8 个货币对，多种操作/原因/进场方式）
- **多目标输出**: 同时测试控制台、文件和 CloudWatch

---

## 运行演示

### POCO 模型性能测试
```powershell
dotnet run -- -c appsettings.json -d 10 -m poco --min-interval 10 --max-interval 50
```

### Protobuf 模型性能测试
```powershell
dotnet run -- -c appsettings.json -d 10 -m protobuf --min-interval 10 --max-interval 50
```

### 模型对比（依次运行两者）
```powershell
# 测试 POCO
dotnet run -- -c appsettings.json -d 30 -m poco --min-interval 10 --max-interval 50

# 测试 Protobuf
dotnet run -- -c appsettings.json -d 30 -m protobuf --min-interval 10 --max-interval 50
```

### 高频测试（1-10ms 间隔）
```powershell
dotnet run -- -c appsettings.json -d 30 -m poco --min-interval 1 --max-interval 10
```

### 最大吞吐量测试（日志间无延迟）
```powershell
dotnet run -- -c appsettings.json -d 5 -m protobuf --min-interval 0 --max-interval 0
```

### 长时间压力测试（5 分钟）
```powershell
dotnet run -- -c appsettings.json -d 300 -m poco --min-interval 10 --max-interval 50
```

### 查看帮助信息
```powershell
dotnet run -- --help
```

### 命令行选项

| 选项             | 简写 | 默认值 | 说明                           |
| ---------------- | ---- | ------ | ------------------------------ |
| `--config`       | `-c` | 必需   | appsettings 配置文件路径       |
| `--duration`     | `-d` | 10     | 测试持续时间（秒）             |
| `--model-type`   | `-m` | poco   | 模型类型："poco" 或 "protobuf" |
| `--min-interval` |      | 1      | 日志间最小间隔（毫秒）         |
| `--max-interval` |      | 100    | 日志间最大间隔（毫秒）         |

---

## 示例输出

### POCO 模型测试
```
MT5Bridge Serilog Performance Test
===================================

Configuration:
  Model Type: POCO
  Duration: 5 seconds
  Interval: 10-50 ms
  Min Log Level: Debug
  Sinks: Console=True, File=True, CloudWatch=False

Starting performance test (POCO Model)...

[2025-12-27 13:15:42.419 INF] Deal: 295542662 ExtID:EXT-77783 Login:8112 Dealer:633 Order:333491953 Symbol:USDCAD Action:Buy Entry:InOut Reason:Dealer Price:1.4923534669025087 Vol:634 VolClosed:320 Profit:489.77044577618943 ProfitRaw:453.6684564241047 Storage:5.826580300267928 Comm:21.486279155171168 Fee:3.336350507670357 SL:0 TP:1.4947983602483417 PricePos:1.405938611131992 Time:1766812542 TimeMsc:1766812542416 RateProfit:1.0955829318030397 RateMargin:1.0417620228935245 TickVal:0.9632615850441728 TickSize:1E-05 Bid:1.413021058197144 Ask:1.450098168232524 Last:1.3175127803174644 ExpertID:51943 PosID:273904275 Comment:

============================================================
Performance Statistics
============================================================
Total Logs:        96
Total Duration:    5.02 seconds
Logs/Second:       19.12
Avg Log Time:      15.8589 ms
Min Log Time:      3.7462 ms
Max Log Time:      92.5346 ms
95th Percentile:   23.5538 ms
99th Percentile:   92.5346 ms
============================================================

Test completed successfully!
```

### Protobuf 模型测试
```
MT5Bridge Serilog Performance Test
===================================

Configuration:
  Model Type: PROTOBUF
  Duration: 5 seconds
  Interval: 10-50 ms
  Min Log Level: Debug
  Sinks: Console=True, File=True, CloudWatch=False

Starting performance test (Protobuf Model)...

[2025-12-27 13:15:58.681 INF] Deal: 208109769 ExtID:EXT-44695 Login:7669 Dealer:993 Order:279143550 Symbol:EURJPY Action:Buy Entry:Out Reason:Tp Price:1.2479950802853992 Vol:732 VolClosed:129 Profit:-375.5147290907629 ProfitRaw:63.286268427389494 Storage:5.882244653451499 Comm:23.525950235867025 Fee:3.562220711504967 SL:1.3501712946663176 TP:1.4788714229291648 PricePos:1.2220268352887214 Time:1766812558 TimeMsc:1766812558677 RateProfit:1.062083003563762 RateMargin:1.0907097962952312 TickVal:3.0360837710192623 TickSize:1E-05 Bid:1.2059921317871771 Ask:1.1552521575296326 Last:1.4503175397116523 ExpertID:91645 PosID:532987517 Comment:Test comment

============================================================
Performance Statistics
============================================================
Total Logs:        109
Total Duration:    5.02 seconds
Logs/Second:       21.71
Avg Log Time:      10.7013 ms
Min Log Time:      2.9699 ms
Max Log Time:      69.4492 ms
95th Percentile:   19.8455 ms
99th Percentile:   24.8439 ms
============================================================

Test completed successfully!
```

### 性能对比总结

基于 5 秒测试，10-50ms 间隔：

| 指标            | POCO 模型 | Protobuf 模型 | 差异       |
| --------------- | --------- | ------------- | ---------- |
| **总日志数**    | 96        | 109           | +13.5%     |
| **每秒日志数**  | 19.12     | 21.71         | +13.5%     |
| **平均延迟**    | 15.86 ms  | 10.70 ms      | **-32.5%** |
| **最小延迟**    | 3.75 ms   | 2.97 ms       | -20.8%     |
| **最大延迟**    | 92.53 ms  | 69.45 ms      | -24.9%     |
| **95 分位延迟** | 23.55 ms  | 19.85 ms      | -15.7%     |
| **99 分位延迟** | 92.53 ms  | 24.84 ms      | **-73.2%** |

**结论**: Protobuf 模型表现更优，**吞吐量提升 13.5%**，**平均延迟降低 32.5%**。

---

## 用于测试的配置文件

为不同的测试场景创建不同的 `appsettings*.json` 文件：

**appsettings.development.json** - 开发环境，详细日志：
```json
{
  "MinimumLevel": "Debug",
  "Console": { "Enabled": true, "MinimumLevel": "Debug" },
  "File": { "Enabled": true, "MinimumLevel": "Debug" },
  "CloudWatch": { "Enabled": false }
}
```

**appsettings.production.json** - 生产环境，成本优化：
```json
{
  "MinimumLevel": "Debug",
  "Console": { "Enabled": true, "MinimumLevel": "Information" },
  "File": { "Enabled": true, "MinimumLevel": "Debug" },
  "CloudWatch": { "Enabled": true, "MinimumLevel": "Warning" }
}
```

**appsettings.test.json** - 测试性能，最小化输出：
```json
{
  "MinimumLevel": "Information",
  "Console": { "Enabled": true, "MinimumLevel": "Information" },
  "File": { "Enabled": true, "MinimumLevel": "Information" },
  "CloudWatch": { "Enabled": false }
}
```

---

## 配置

配置通过 `appsettings.json` 管理，遵循 `SerilogConfig` 架构：

### 完整配置示例

```json
{
  "MinimumLevel": "Debug",
  "Console": {
    "Enabled": true,
    "TextFormatter": "plain",
    "UseAnsiColors": true
  },
  "File": {
    "Enabled": true,
    "Path": "logs/mt5bridge-.txt",
    "RollingInterval": "Day",
    "FileSizeLimitBytes": 104857600,
    "RetainedFileCountLimit": 30,
    "TextFormatter": "plain"
  },
  "CloudWatch": {
    "Enabled": false,
    "Region": "us-east-1",
    "LogGroup": "/mt5bridge/serilog/demo",
    "LogStreamPrefix": "demo-",
    "AccessKeyId": "",
    "SecretKey": "",
    "BatchSizeLimit": 100,
    "PeriodSeconds": 15,
    "LogStreamNamingStrategy": "configurable",
    "LogStreamIncludeHostname": true,
    "LogStreamIncludeGuid": true,
    "TextFormatter": "compact",
    "QueueSizeLimit": 10000,
    "RetryAttempts": 5
  }
}
```

### 配置说明

| 选项                                  | 类型           | 默认值                    | 说明                                                                |
| ------------------------------------- | -------------- | ------------------------- | ------------------------------------------------------------------- |
| `MinimumLevel`                        | LogEventLevel  | Information               | 最小日志级别：Verbose, Debug, Information, Warning, Error, Fatal    |
| `Console.Enabled`                     | bool           | true                      | 启用控制台输出                                                      |
| `Console.TextFormatter`               | string         | "" (纯文本)               | 格式：纯文本（"plain" 或空），"json"、"compact"、"rendered-compact" |
| `Console.UseAnsiColors`               | bool           | true                      | 使用 ANSI 颜色（容器环境设为 false）                                |
| `Console.MinimumLevel`                | LogEventLevel? | null (全局)               | Console 专属日志级别（覆盖全局 MinimumLevel）                       |
| `File.Enabled`                        | bool           | false                     | 启用文件日志                                                        |
| `File.Path`                           | string         | logs/log-.txt             | 文件路径模式（自动创建目录）                                        |
| `File.RollingInterval`                | string         | Day                       | 滚动策略：Infinite, Year, Month, Day, Hour, Minute                  |
| `File.FileSizeLimitBytes`             | long?          | 10MB                      | 文件大小超过此值时滚动                                              |
| `File.RetainedFileCountLimit`         | int?           | 31                        | 保留的旧日志文件数                                                  |
| `File.TextFormatter`                  | string         | "" (纯文本)               | 格式：纯文本（"plain" 或空），"json"、"compact"、"rendered-compact" |
| `File.MinimumLevel`                   | LogEventLevel? | null (全局)               | File 专属日志级别（覆盖全局 MinimumLevel）                          |
| `CloudWatch.Enabled`                  | bool           | false                     | 启用 CloudWatch 日志                                                |
| `CloudWatch.Region`                   | string         | (空)                      | AWS 区域（启用时必填，如 us-east-1, eu-west-1）                     |
| `CloudWatch.LogGroup`                 | string         | (空)                      | CloudWatch 日志组名称（启用时必填，1-256 字符）                     |
| `CloudWatch.CreateLogGroup`           | bool           | false                     | 是否在日志组不存在时自动创建                                        |
| `CloudWatch.LogStreamPrefix`          | string         | (空)                      | 日志流名称前缀（可选）                                              |
| `CloudWatch.AccessKeyId`              | string         | (空)                      | AWS 访问密钥 ID（可选，留空则使用默认凭证链）                       |
| `CloudWatch.SecretKey`                | string         | (空)                      | AWS 密钥（若设置 AccessKeyId 则必须提供）                           |
| `CloudWatch.BatchSizeLimit`           | int            | 100                       | 批量发送的事件数 (1-1000)                                           |
| `CloudWatch.PeriodSeconds`            | int            | 5                         | 刷新到 CloudWatch 的间隔秒数 (1-300)                                |
| `CloudWatch.LogStreamNamingStrategy`  | string         | configurable              | 日志流命名策略："default"、"constant"、"configurable"               |
| `CloudWatch.LogStreamIncludeHostname` | bool           | true                      | 日志流名称中包含主机名（"configurable" 策略）                       |
| `CloudWatch.LogStreamIncludeGuid`     | bool           | true                      | 日志流名称中包含 GUID（"configurable" 策略）                        |
| `CloudWatch.TextFormatter`            | string         | compact                   | 文本格式化器："json"、"compact"（推荐）、"rendered-compact"         |
| `CloudWatch.QueueSizeLimit`           | int            | 10000                     | 队列满时丢弃事件的最大队列大小 (100-100000)                         |
| `CloudWatch.RetryAttempts`            | byte           | 5                         | 失败上传的重试次数 (0-255)                                          |
| `CloudWatch.MinimumLevel`             | LogEventLevel? | null (全局)               | CloudWatch 专属日志级别（覆盖全局 MinimumLevel）                    |
| `Diagnostics.ThrottleWindowSeconds`   | int            | 300                       | 限流时间窗口（秒），用于 Serilog 内部错误消息限流                   |
| `Diagnostics.ThrottleLimit`           | int            | 100                       | 在限流窗口内允许的最大内部错误消息数量（0=禁用诊断输出）            |
| `Diagnostics.Console.Enabled`         | bool           | false                     | 是否启用内部诊断日志输出到控制台                                    |
| `Diagnostics.File.Enabled`            | bool           | false                     | 是否启用内部诊断日志输出到文件                                      |
| `Diagnostics.File.Path`               | string         | logs/serilog-internal.log | 内部诊断日志的文件路径                                              |

---

## 各 Sink 独立日志级别配置

每个 sink（Console、File、CloudWatch）可以拥有自己的最小日志级别，实现对不同输出目标的精细控制。

### 配置策略

**全局 MinimumLevel**：作为第一道过滤器 - 低于此级别的日志事件永远不会被创建（性能优化）。

**Sink 专属 MinimumLevel**：每个 sink 的可选覆盖级别。如果未设置（null），sink 使用全局级别。

### 示例：生产环境多级别日志

```json
{
  "MinimumLevel": "Debug",         // 创建所有 Debug+ 事件
  "Console": {
    "Enabled": true,
    "MinimumLevel": "Information"  // Console 仅显示 Information+
  },
  "File": {
    "Enabled": true,
    "Path": "logs/app-.log",
    "MinimumLevel": "Debug"        // File 捕获所有内容（Debug+）
  },
  "CloudWatch": {
    "Enabled": true,
    "Region": "us-east-1",
    "LogGroup": "/production/mt5bridge",
    "MinimumLevel": "Warning",     // CloudWatch 仅存储 Warning+（成本优化）
    "BatchSizeLimit": 500,
    "PeriodSeconds": 10
  }
}
```

### 常见场景

**开发环境**：Console=Debug, File=Verbose, CloudWatch=禁用
```json
{
  "MinimumLevel": "Verbose",
  "Console": { "Enabled": true, "MinimumLevel": "Debug" },
  "File": { "Enabled": true, "MinimumLevel": "Verbose" },
  "CloudWatch": { "Enabled": false }
}
```

**生产环境**：Console=Information, File=Debug, CloudWatch=Warning
```json
{
  "MinimumLevel": "Debug",
  "Console": { "Enabled": true, "MinimumLevel": "Information" },
  "File": { "Enabled": true, "MinimumLevel": "Debug" },
  "CloudWatch": { "Enabled": true, "MinimumLevel": "Warning" }
}
```

**故障排查模式**：Console=Debug, File=Verbose, CloudWatch=Debug
```json
{
  "MinimumLevel": "Verbose",
  "Console": { "Enabled": true, "MinimumLevel": "Debug" },
  "File": { "Enabled": true, "MinimumLevel": "Verbose" },
  "CloudWatch": { "Enabled": true, "MinimumLevel": "Debug" }
}
```

### 优势

- 🚀 **性能优化**：减少控制台输出噪音（Information+），同时保留文件中的详细日志
- 💰 **成本优化**：仅将关键日志发送到 CloudWatch（Warning+），而 File 捕获 Debug
- 🔍 **故障排查**：不同环境可拥有不同的日志策略，无需修改代码
- 🎯 **灵活性**：通过配置改变日志策略，无需修改代码

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
    "PeriodSeconds": 5,
    "LogStreamNamingStrategy": "configurable",
    "LogStreamIncludeHostname": true,
    "LogStreamIncludeGuid": true,
    "TextFormatter": "compact",
    "QueueSizeLimit": 10000,
    "RetryAttempts": 5
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
    "PeriodSeconds": 5,
    "LogStreamNamingStrategy": "configurable",
    "LogStreamIncludeHostname": true,
    "LogStreamIncludeGuid": true,
    "TextFormatter": "compact",
    "QueueSizeLimit": 10000,
    "RetryAttempts": 5
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
- 检查 IAM 角色是否有 `logs:CreateLogStream`、`logs:PutLogEvents` 权限。
- 如果 `CreateLogGroup` 设置为 `true`，还需要 `logs:CreateLogGroup` 和 `logs:DescribeLogGroups` 权限。
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
- Confirm IAM permissions: `logs:CreateLogStream`, `logs:PutLogEvents`.
- If `CreateLogGroup` is `true`, also include `logs:CreateLogGroup` and `logs:DescribeLogGroups`.
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
    "PeriodSeconds": 5,
    "LogStreamNamingStrategy": "configurable",
    "LogStreamIncludeHostname": true,
    "LogStreamIncludeGuid": true,
    "TextFormatter": "compact",
    "QueueSizeLimit": 10000,
    "RetryAttempts": 5
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
    "PeriodSeconds": 5,
    "LogStreamNamingStrategy": "configurable",
    "LogStreamIncludeHostname": true,
    "LogStreamIncludeGuid": true,
    "TextFormatter": "compact",
    "QueueSizeLimit": 10000,
    "RetryAttempts": 5
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

