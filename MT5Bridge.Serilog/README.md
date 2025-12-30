# MT5Bridge.Serilog

[English](#english) | [中文](#chinese)

---

<a name="english"></a>

## English Documentation

Enterprise-grade Serilog configuration library for MT5Bridge with production-ready logging infrastructure.

### 🚀 Key Features

#### Serilog Native Experience
- **Direct Serilog Usage**: Zero abstraction overhead, use Serilog's native structured logging
- **Zero-Reflection**: Serilog's built-in template syntax provides zero-reflection performance
- **Minimal Allocations**: Direct parameter passing, no intermediate objects or dictionaries
- **Type-Safe**: Compile-time checking of log parameters

#### Enterprise Architecture
- **Multiple Sinks**: Console (with ANSI colors), rolling File (by date/size), and AWS CloudWatch
- **Flexible Configuration**: JSON-based configuration with validation and defaults
- **Comprehensive Validation**: All configuration validated at startup, directories auto-created
- **Thread-Safe**: Safe concurrent access to logging infrastructure

#### Robustness & Reliability
- **Strict Config Validation**: Null checks, parameter ranges, path validation, AWS region verification
- **Resource Management**: `FlushAndCloseAsync()` for graceful shutdown, prevents log loss
- **Error Recovery**: Clear error messages, exception handling at every level
- **Per-Sink Log Levels**: Independent log level control for Console, File, and CloudWatch

#### 📖 Complete Documentation
- Best practices for structured logging
- Performance recommendations
- Multi-sink configuration patterns

---

### Quick Start

```csharp
// 1. Load configuration from appsettings.json
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var serilogConfig = new SerilogConfig();
configuration.Bind(serilogConfig);

// 2. Create logger
var logger = SerilogBootstrapper.CreateLogger(serilogConfig);

// 3. Use Serilog's native structured logging
logger.Information("Deal executed: {DealId} {Symbol} {Action} {Volume}@{Price}",
    deal.Deal, deal.Symbol, deal.Action, deal.Volume, deal.Price);

logger.Warning("High latency detected: {LatencyMs}ms for {Symbol}",
    latency, symbol);

logger.Error(ex, "Trade failed: {DealId} {Symbol}",
    dealId, symbol);

// 4. Shutdown gracefully to flush all logs
await SerilogBootstrapper.FlushAndCloseAsync();
```

**Why Direct Serilog?**
- ✅ Zero reflection (Serilog uses compiled expressions)
- ✅ Minimal allocations (no intermediate objects)
- ✅ Type-safe (compile-time checking)
- ✅ JSON output support (all sinks can use JSON formatters)
- ✅ Community standard (widely understood pattern)

---

### Configuration

#### Complete Configuration Example

```json
{
  "MinimumLevel": "Information",
  "Console": {
    "Enabled": true,
    "MinimumLevel": null,
    "TextFormatter": "plain",
    "UseAnsiColors": true
  },
  "File": {
    "Enabled": true,
    "MinimumLevel": null,
    "TextFormatter": "json",
    "Path": "logs/app-.log",
    "RollingInterval": "Day",
    "FileSizeLimitBytes": 104857600,
    "RetainedFileCountLimit": 30
  },
  "CloudWatch": {
    "Enabled": false,
    "MinimumLevel": null,
    "TextFormatter": "compact",
    "LogGroup": "/production/mt5bridge",
    "CreateLogGroup": false,
    "LogStreamPrefix": "app-",
    "Region": "us-east-1",
    "AccessKeyId": "",
    "SecretKey": "",
    "BatchSizeLimit": 500,
    "PeriodSeconds": 5,
    "LogStreamNamingStrategy": "configurable",
    "LogStreamIncludeHostname": true,
    "LogStreamIncludeGuid": true,
    "QueueSizeLimit": 10000,
    "RetryAttempts": 5
  },
  "Diagnostics": {
    "ThrottleWindowSeconds": 300,
    "ThrottleLimit": 100,
    "Console": {
      "Enabled": false
    },
    "File": {
      "Enabled": false,
      "Path": "logs/serilog-internal.log"
    }
  }
}
```

#### Configuration Reference

| Option                                | Type           | Default                   | Description                                                                      |
| ------------------------------------- | -------------- | ------------------------- | -------------------------------------------------------------------------------- |
| `MinimumLevel`                        | LogEventLevel  | Information               | Minimum log level: Verbose, Debug, Information, Warning, Error, Fatal            |
| `Console.Enabled`                     | bool           | true                      | Enable console output                                                            |
| `Console.MinimumLevel`                | LogEventLevel? | null (global)             | Console-specific log level (overrides global MinimumLevel if set)                |
| `Console.TextFormatter`               | string         | "" (plain)                | Format: "plain" or empty (text), "json", "compact", "rendered-compact"           |
| `Console.UseAnsiColors`               | bool           | true                      | Use ANSI colors (set false for containers/non-TTY)                               |
| `File.Enabled`                        | bool           | false                     | Enable file logging                                                              |
| `File.MinimumLevel`                   | LogEventLevel? | null (global)             | File-specific log level (overrides global MinimumLevel if set)                   |
| `File.TextFormatter`                  | string         | "" (plain)                | Format: "plain" or empty (text), "json", "compact", "rendered-compact"           |
| `File.Path`                           | string         | logs/log-.txt             | File path pattern (creates directories automatically)                            |
| `File.RollingInterval`                | string         | Day                       | Rolling strategy: Infinite, Year, Month, Day, Hour, Minute                       |
| `File.FileSizeLimitBytes`             | long?          | 10MB (10485760)           | Max file size before rolling                                                     |
| `File.RetainedFileCountLimit`         | int?           | 31                        | Number of old files to keep                                                      |
| `CloudWatch.Enabled`                  | bool           | false                     | Enable CloudWatch Logs integration                                               |
| `CloudWatch.MinimumLevel`             | LogEventLevel? | null (global)             | CloudWatch-specific log level (overrides global MinimumLevel if set)             |
| `CloudWatch.TextFormatter`            | string         | compact                   | Text formatter: "json", "compact" (recommended), "rendered-compact"              |
| `CloudWatch.LogGroup`                 | string         | (empty)                   | CloudWatch log group name (required when enabled, 1-256 characters)              |
| `CloudWatch.CreateLogGroup`           | bool           | false                     | Whether to create the log group if it doesn't exist                              |
| `CloudWatch.LogStreamPrefix`          | string         | (empty)                   | Prefix for log stream names (optional)                                           |
| `CloudWatch.Region`                   | string         | (empty)                   | AWS region (required when enabled, e.g., us-east-1, eu-west-1)                   |
| `CloudWatch.AccessKeyId`              | string         | (empty)                   | AWS Access Key ID (optional, uses default credential chain if empty)             |
| `CloudWatch.SecretKey`                | string         | (empty)                   | AWS Secret Access Key (required if AccessKeyId is set)                           |
| `CloudWatch.BatchSizeLimit`           | int            | 100                       | Events per batch to CloudWatch (1-1000, AWS limit)                               |
| `CloudWatch.PeriodSeconds`            | int            | 5                         | Flush interval to CloudWatch in seconds (1-300)                                  |
| `CloudWatch.LogStreamNamingStrategy`  | string         | configurable              | Log stream naming strategy: "default", "constant", "configurable"                |
| `CloudWatch.LogStreamIncludeHostname` | bool           | true                      | Include hostname in log stream name (for "configurable" strategy)                |
| `CloudWatch.LogStreamIncludeGuid`     | bool           | true                      | Include GUID in log stream name (for "configurable" strategy)                    |
| `CloudWatch.QueueSizeLimit`           | int            | 10000                     | Maximum queue size before dropping events (100-100000)                           |
| `CloudWatch.RetryAttempts`            | byte           | 5                         | Number of retry attempts for failed uploads (0-255)                              |
| `Diagnostics.ThrottleWindowSeconds`   | int            | 300                       | Time window in seconds for throttling Serilog internal errors (must be positive) |
| `Diagnostics.ThrottleLimit`           | int            | 100                       | Max internal error messages per window (0 = disable diagnostic output)           |
| `Diagnostics.Console.Enabled`         | bool           | false                     | Enable internal diagnostic logging to Console                                    |
| `Diagnostics.File.Enabled`            | bool           | false                     | Enable internal diagnostic logging to a file                                     |
| `Diagnostics.File.Path`               | string         | logs/serilog-internal.log | Path for internal diagnostic logs                                                |

---

### AWS CloudWatch Authentication

The library supports two authentication methods for AWS CloudWatch:

#### Method 1: Explicit Credentials (AccessKeyId & SecretKey)

Provide credentials directly in configuration:

```json
{
  "CloudWatch": {
    "Enabled": true,
    "MinimumLevel": "Warning",
    "TextFormatter": "compact",
    "LogGroup": "/production/mt5bridge",
    "LogStreamPrefix": "app-",
    "Region": "us-east-1",
    "AccessKeyId": "YOUR_ACCESS_KEY_ID_HERE",
    "SecretKey": "YOUR_SECRET_KEY_HERE",
    "BatchSizeLimit": 100,
    "PeriodSeconds": 5,
    "LogStreamNamingStrategy": "configurable",
    "LogStreamIncludeHostname": true,
    "LogStreamIncludeGuid": true,
    "QueueSizeLimit": 10000,
    "RetryAttempts": 5
  }
}
```

**⚠️ Security Note**: Never commit credentials to source control. Use environment variables or secrets management:
- Store in environment variables and load at runtime
- Use AWS Secrets Manager or Parameter Store
- Use encrypted configuration files

#### Method 2: Default Credential Chain (Recommended)

Leave `AccessKeyId` and `SecretKey` empty to use AWS default credential chain:

```json
{
  "CloudWatch": {
    "Enabled": true,
    "MinimumLevel": "Warning",
    "TextFormatter": "compact",
    "LogGroup": "/production/mt5bridge",
    "LogStreamPrefix": "app-",
    "Region": "us-east-1",
    "AccessKeyId": "",
    "SecretKey": "",
    "BatchSizeLimit": 100,
    "PeriodSeconds": 5,
    "LogStreamNamingStrategy": "configurable",
    "LogStreamIncludeHostname": true,
    "LogStreamIncludeGuid": true,
    "QueueSizeLimit": 10000,
    "RetryAttempts": 5
  }
}
```

The SDK will automatically search for credentials in this order:
1. **Environment variables**: `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`
2. **AWS credentials file**: `~/.aws/credentials` (Windows: `%USERPROFILE%\.aws\credentials`)
3. **IAM role**: EC2 instance profile or ECS task role
4. **AWS SSO**: If configured

**Recommended for**:
- Production environments (use IAM roles)
- Development with AWS CLI configured
- CI/CD pipelines with environment variables

---

### Per-Sink Log Level Configuration

Each sink (Console, File, CloudWatch) can have its own minimum log level, allowing fine-grained control over what gets logged where.

#### Configuration Strategy

**Global MinimumLevel**: Acts as the first filter - log events below this level are never created.

**Sink-specific MinimumLevel**: Optional override for each sink. If not set (null), the sink uses the global level.

#### Example: Production Multi-Level Logging

```json
{
  "MinimumLevel": "Debug",  // Create all Debug+ events
  "Console": {
    "Enabled": true,
    "TextFormatter": "plain",
    "MinimumLevel": "Information"  // Console only shows Information+
  },
  "File": {
    "Enabled": true,
    "Path": "logs/app-.log",
    "TextFormatter": "plain",
    "MinimumLevel": "Debug"  // File captures everything (Debug+)
  },
  "CloudWatch": {
    "Enabled": true,
    "Region": "us-east-1",
    "LogGroup": "/production/mt5bridge",
    "MinimumLevel": "Warning",  // CloudWatch only stores Warning+ (cost optimization)
    "BatchSizeLimit": 500,
    "PeriodSeconds": 10
  }
}
```

#### Use Cases

**Development**: Console=Debug, File=Verbose, CloudWatch=Disabled
```json
{
  "MinimumLevel": "Verbose",
  "Console": { "Enabled": true, "MinimumLevel": "Debug" },
  "File": { "Enabled": true, "MinimumLevel": "Verbose" },
  "CloudWatch": { "Enabled": false }
}
```

**Production**: Console=Information, File=Debug, CloudWatch=Warning
```json
{
  "MinimumLevel": "Debug",
  "Console": { "Enabled": true, "MinimumLevel": "Information" },
  "File": { "Enabled": true, "MinimumLevel": "Debug" },
  "CloudWatch": { "Enabled": true, "MinimumLevel": "Warning" }
}
```

**Troubleshooting**: Console=Debug, File=Verbose, CloudWatch=Debug
```json
{
  "MinimumLevel": "Verbose",
  "Console": { "Enabled": true, "MinimumLevel": "Debug" },
  "File": { "Enabled": true, "MinimumLevel": "Verbose" },
  "CloudWatch": { "Enabled": true, "MinimumLevel": "Debug" }
}
```

**Benefits**:
- **Cost Optimization**: Send only critical logs to CloudWatch (Warning+) while File captures Debug
- **Performance**: Reduce console noise (Information+) while preserving detailed logs in files
- **Flexibility**: Different environments can have different logging strategies without code changes

---

### CloudWatch Advanced Configuration

#### Log Stream Naming Strategies

The library provides three strategies for naming CloudWatch log streams:

**1. Default Strategy** (`"default"`)
- Format: `{DateTime}_{HostName}_{Guid}`
- Example: `2025-12-26-14-30-45_PRODSERVER_a1b2c3d4-e5f6-...`
- Use case: When you need full traceability with timestamp and host

**2. Constant Strategy** (`"constant"`)
- Format: `{LogStreamPrefix}_{Guid}`
- Example: `app-server_a1b2c3d4-e5f6-...`
- Use case: Simple naming with unique identifier

**3. Configurable Strategy** (`"configurable"`) ⭐ Recommended
- Format: `{LogStreamPrefix}/[hostname]/[guid]` (based on Include flags)
- Examples:
  - All enabled: `app-server/PRODSERVER/a1b2c3d4-e5f6-...`
  - Only prefix: `app-server`
  - Prefix + hostname: `app-server/PRODSERVER`
- Use case: Flexible naming for different deployment scenarios

#### Text Formatters

The library supports three JSON formatters with different trade-offs:

**1. JSON Formatter** (`"json"`)
- Standard Serilog JSON format
- Size: 292 bytes (baseline)
- Performance: Baseline
- Use case: When you need standard JSON format

**2. Compact JSON Formatter** (`"compact"`) ⭐ Recommended
- CompactJsonFormatter from Serilog.Formatting.Compact
- Size: 187 bytes (36% smaller than standard)
- Performance: 1.89x faster than standard
- Preserves message templates with `@mt` field
- **Log level behavior**:
  * Information logs: Omits `@l` field (for size optimization)
  * Other levels: Includes `@l` field (Verbose, Debug, Warning, Error, Fatal)
  * **Solution**: LogLevelEnricher adds `"l"` field with u3 format (VRB/DBG/INF/WRN/ERR/FTL) to all levels
- Use case: Production environments requiring optimal performance

**3. Rendered Compact JSON Formatter** (`"rendered-compact"`)
- Pre-renders message templates
- Includes both `@mt` (template) and `@r` (rendered message)
- Includes `@i` (event ID) for correlation
- **Same log level behavior as compact formatter** (uses LogLevelEnricher)
- Use case: When you need both structured and human-readable messages

#### Queue and Retry Configuration

**QueueSizeLimit** (default: 10000)
- Maximum number of log events buffered in memory
- When queue is full, new events are **dropped** (not blocked)
- Range: 100-100000
- Recommendation: Increase for high-throughput systems (20000-50000)

**RetryAttempts** (default: 5)
- Number of retry attempts for failed CloudWatch API calls
- Range: 0-255
- Recommendation: Keep default (5) for most scenarios

#### Log Level Enricher

**Automatic Level Field Addition**

When using `compact` or `rendered-compact` formatters, the library automatically adds a `LogLevelEnricher` that includes a short-form log level field (`"l"`) in all log events:

```json
// Information level (without enricher)
{"@t":"2025-12-27T06:00:00.000Z","@mt":"User logged in",...}

// Information level (with enricher)
{"@t":"2025-12-27T06:00:00.000Z","@mt":"User logged in","l":"INF",...}

// Warning level (has both @l and l)
{"@t":"2025-12-27T06:00:00.000Z","@mt":"Retry attempt","@l":"Warning","l":"WRN",...}
```

**Level Abbreviations (u3 format)**:
- `VRB` - Verbose
- `DBG` - Debug
- `INF` - Information
- `WRN` - Warning
- `ERR` - Error
- `FTL` - Fatal

**Benefits**:
- ✅ Consistent level field across all log levels
- ✅ Compact 3-letter format saves bandwidth
- ✅ Easy filtering in log aggregation systems (e.g., `l:INF OR l:WRN`)
- ✅ Automatic activation when compact formatters are used

---

### Usage Patterns

#### 1. Standard Structured Logging (Recommended)

```csharp
var logger = SerilogBootstrapper.CreateLogger(config);

// Simple messages
logger.Information("Application started");
logger.Warning("Configuration incomplete");
logger.Error(ex, "Operation failed");

// Structured data with Serilog's native template syntax
logger.Information("Deal executed: {DealId} {Symbol} {Action} {Volume}@{Price}",
    deal.Deal, deal.Symbol, deal.Action, deal.Volume, deal.Price);

logger.Warning("High latency: {LatencyMs}ms for {Symbol} at {Timestamp}",
    latency, symbol, DateTime.UtcNow);

logger.Error(ex, "Trade failed: {DealId} {Symbol} {ErrorCode}",
    dealId, symbol, errorCode);
```

**Why This Works**:
- ✅ Zero reflection - Serilog compiles property extraction
- ✅ Minimal allocations - direct parameter passing
- ✅ Type-safe - compile-time checking
- ✅ JSON-friendly - outputs structured data to all sinks

#### 2. Complex Objects

```csharp
// Serilog automatically serializes objects using destructuring
logger.Information("User action: {@User} {@Action}",
    user,      // @ prefix for destructuring (full object serialization)
    action);

// Output to JSON sink:
// {
//   "User": { "Id": 123, "Name": "John", "Email": "john@example.com" },
//   "Action": { "Type": "Login", "Timestamp": "2025-12-27T10:30:00Z" }
// }
```

#### 3. Performance-Critical Logging

```csharp
// For high-frequency logging, check log level first
if (logger.IsEnabled(LogEventLevel.Debug))
{
    logger.Debug("High-frequency tick: {Symbol} {Bid} {Ask}",
        symbol, bid, ask);
}

// Or use Serilog's built-in lazy evaluation with message templates
logger.Debug("Expensive operation: {Data}",
    new { Result = ExpensiveComputation() });  // Only evaluated if Debug is enabled
```

#### 4. Error Logging with Context

```csharp
try
{
    ProcessTrade(deal);
}
catch (Exception ex)
{
    logger.Error(ex, "Trade processing failed: {DealId} {Symbol} {Volume} {Reason}",
        deal.Deal, deal.Symbol, deal.Volume, "ValidationError");
}
```

#### 5. Batch Operations

```csharp
var stopwatch = Stopwatch.StartNew();
ProcessBatch(deals);
stopwatch.Stop();

logger.Information("Batch processed: {TotalDeals} deals in {DurationMs}ms, avg {AvgMs}ms/deal",
    deals.Count, stopwatch.ElapsedMilliseconds, stopwatch.ElapsedMilliseconds / (double)deals.Count);
```

---

### Best Practices

#### 1. Use Structured Logging Everywhere

```csharp
// ❌ Bad - string concatenation, not searchable
logger.Information($"User {userId} logged in from {ipAddress}");

// ✅ Good - structured data, searchable in CloudWatch/ELK
logger.Information("User login: {UserId} from {IpAddress}",
    userId, ipAddress);
```

#### 2. Use Meaningful Property Names

```csharp
// ❌ Bad - generic names
logger.Information("Processing: {Id} {Value}", dealId, price);

// ✅ Good - specific, searchable names
logger.Information("Deal processing: {DealId} {Price}", dealId, price);
```

#### 3. Destructure Complex Objects

```csharp
// Use @ prefix to destructure objects into structured data
logger.Information("Order created: {@Order}", order);

// Output: { "Order": { "Id": 123, "Symbol": "EURUSD", "Volume": 1.5 } }
```

#### 4. Per-Environment Configuration

**Development**: Verbose console output
```json
{
  "MinimumLevel": "Debug",
  "Console": { "Enabled": true, "TextFormatter": "plain" },
  "File": { "Enabled": true, "MinimumLevel": "Verbose" },
  "CloudWatch": { "Enabled": false }
}
```

**Production**: Minimal console, full file logs, critical CloudWatch
```json
{
  "MinimumLevel": "Information",
  "Console": { "Enabled": true, "MinimumLevel": "Warning" },
  "File": { "Enabled": true, "TextFormatter": "json" },
  "CloudWatch": { "Enabled": true, "MinimumLevel": "Warning", "TextFormatter": "compact" }
}
```

#### 5. Performance Considerations

```csharp
// For high-frequency logging, check level first
if (logger.IsEnabled(LogEventLevel.Debug))
{
    logger.Debug("Tick: {Symbol} {Bid}/{Ask}", symbol, bid, ask);
}

// Serilog's lazy evaluation works automatically with message templates
logger.Debug("Data: {ComplexData}", GetComplexData());  // Only called if Debug enabled
```

---

### Graceful Shutdown

Always call `FlushAndCloseAsync()` on application exit to ensure:
- All buffered logs are flushed to disk/CloudWatch
- AWS CloudWatch client is properly disposed
- No log loss on shutdown

```csharp
try
{
    await app.RunAsync();
}
finally
{
    await SerilogBootstrapper.FlushAndCloseAsync();
}
```

---

### Error Handling

The logger validates all configuration at startup. Common errors and fixes:

| Error                                                                             | Cause                   | Fix                                                                    |
| --------------------------------------------------------------------------------- | ----------------------- | ---------------------------------------------------------------------- |
| `InvalidOperationException: At least one logging sink must be enabled`            | All sinks disabled      | Enable at least Console or File in config                              |
| `InvalidOperationException: File.Path cannot be empty`                            | Missing path            | Provide valid file path in config                                      |
| `InvalidOperationException: CloudWatch.BatchSizeLimit must be between 1 and 1000` | Invalid batch size      | Set BatchSizeLimit between 1-1000                                      |
| `InvalidOperationException: Invalid AWS region`                                   | Wrong region name       | Use valid AWS region (us-east-1, eu-west-1, etc.)                      |
| `InvalidOperationException: Failed to create CloudWatch client`                   | Missing AWS credentials | Configure AWS credentials (environment vars, config file, or IAM role) |

---

### Thread Safety

The library is fully thread-safe:
- `SerilogBootstrapper.CreateLogger()` can be called from multiple threads
- `SerilogBootstrapper.Dispose()` uses lock for thread-safe cleanup
- `ModelLogger` instances are thread-safe (mutable Logger field is safely managed)

Example:
```csharp
// Safe in multi-threaded scenarios
var tasks = Enumerable.Range(0, 100)
    .Select(_ => Task.Run(() => logger.Information("Thread {Id}", Thread.CurrentThread.ManagedThreadId)))
    .ToArray();
await Task.WhenAll(tasks);
```

---

### Troubleshooting

**Logs not appearing in CloudWatch?**
- Check AWS credentials are configured
- Verify IAM role has CloudWatchLogs permissions
- Check log group name exists (or enable auto-creation in AWS)
- Verify region is correct

**High memory usage with file logging?**
- Reduce `File.FileSizeLimitBytes` or `File.RetainedFileCountLimit`
- Disable Debug logs in production (`MinimumLevel: Information`)

**CloudWatch API rate limit errors?**
- Increase `CloudWatch.PeriodSeconds` to batch more logs
- Increase `CloudWatch.BatchSizeLimit` to 800-1000

---

### Dependencies

- **Serilog** 4.0.0 - Core logging framework
- **Serilog.Sinks.Console** 6.0.0 - Console output
- **Serilog.Sinks.File** 6.0.0 - File output with rolling
- **Serilog.Sinks.AwsCloudWatch** 4.0.171 - CloudWatch integration
- **Serilog.Formatting.Compact** 3.0.0 - Compact JSON format
- **AWSSDK.CloudWatchLogs** 3.7.300 - AWS CloudWatch API
- **.NET 8.0** - Target framework (uses Source-Generated JSON)

---

---

<a name="chinese"></a>

## 中文文档

MT5Bridge 的企业级 Serilog 配置库，提供生产就绪的日志基础设施。

### 🚀 核心特性

#### Serilog 原生体验
- **直接使用 Serilog**：零抽象开销，使用 Serilog 原生的结构化日志
- **零反射**：Serilog 内置模板语法提供零反射性能
- **最小分配**：直接参数传递，无中间对象或字典
- **类型安全**：编译时检查日志参数类型

#### 企业级架构
- **多种 Sink**：Console（支持 ANSI 颜色）、滚动文件（按日期/大小）、AWS CloudWatch
- **灵活配置**：基于 JSON 的配置，带验证和默认值
- **完整验证**：启动时验证所有配置，自动创建目录
- **线程安全**：安全的并发日志访问

#### 健壮可靠
- **严格配置验证**：空值检查、参数范围、路径有效性、AWS 区域验证
- **资源管理**：`FlushAndCloseAsync()` 优雅关闭，防止日志丢失
- **异常恢复**：清晰的错误消息，每层级完整的异常处理
- **独立 Sink 日志级别**：Console、File、CloudWatch 可独立控制日志级别

#### 📖 完整文档
- 结构化日志最佳实践
- 性能优化建议
- 多 Sink 配置模式

---

### 快速开始

```csharp
// 1. 从 appsettings.json 加载配置
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var serilogConfig = new SerilogConfig();
configuration.Bind(serilogConfig);

// 2. 创建 logger
var logger = SerilogBootstrapper.CreateLogger(serilogConfig);

// 3. 使用 Serilog 原生的结构化日志
logger.Information("交易执行: {DealId} {Symbol} {Action} {Volume}@{Price}",
    deal.Deal, deal.Symbol, deal.Action, deal.Volume, deal.Price);

logger.Warning("高延迟检测: {Symbol} 延迟 {LatencyMs}ms",
    symbol, latency);

logger.Error(ex, "交易失败: {DealId} {Symbol}",
    dealId, symbol);

// 4. 优雅关闭以刷新所有日志
await SerilogBootstrapper.FlushAndCloseAsync();
```

**为什么使用直接 Serilog？**
- ✅ 零反射（Serilog 使用编译表达式）
- ✅ 最小分配（无中间对象）
- ✅ 类型安全（编译时检查）
- ✅ JSON 输出支持（所有 sink 都可使用 JSON 格式化器）
- ✅ 社区标准（广泛理解的模式）

---
置

#### 完整配置示例

```json
{
  "Logging": {
    "MinimumLevel": "Information",
    "Console": {
      "Enabled": true,
      "TextFormatter": "plain",
      "UseAnsiColors": false
    },
    "File": {
      "Enabled": true,
      "Path": "logs/app-.txt",
      "RollingInterval": "Day",
      "FileSizeLimitBytes": 104857600,
      "RetainedFileCountLimit": 30,
      "TextFormatter": "plain"
    },
    "CloudWatch": {
      "Enabled": false,
      "Region": "us-east-1",
      "LogGroup": "/production/mt5bridge",
      "LogStreamPrefix": "app-",
      "TextFormatter": "compact",
      "AccessKeyId": "",
      "SecretKey": "",
      "BatchSizeLimit": 500,
      "PeriodSeconds": 5,
      "LogStreamNamingStrategy": "configurable",
      "LogStreamIncludeHostname": true,
      "LogStreamIncludeGuid": true,
      "QueueSizeLimit": 10000,
      "RetryAttempts": 5
    },
    "Diagnostics": {
      "ThrottleWindowSeconds": 300,
      "ThrottleLimit": 100,
      "Console": {
        "Enabled": false
      },
      "File": {
        "Enabled": false,
        "Path": "logs/serilog-internal.log"
      }
    }    
  }
}
```

#### 配置说明

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
| `Diagnostics.ThrottleWindowSeconds`   | int            | 300                       | 限流时间窗口（秒），用于 Serilog 内部错误消息限流（必须为正数）     |
| `Diagnostics.ThrottleLimit`           | int            | 100                       | 在限流窗口内允许的最大内部错误消息数量（0=禁用诊断输出）            |
| `Diagnostics.Console.Enabled`         | bool           | false                     | 是否启用内部诊断日志输出到控制台                                    |
| `Diagnostics.File.Enabled`            | bool           | false                     | 是否启用内部诊断日志输出到文件                                      |
| `Diagnostics.File.Path`               | string         | logs/serilog-internal.log | 内部诊断日志的文件路径                                              |

---

### AWS CloudWatch 认证配置

本库支持两种 AWS CloudWatch 认证方式：

#### 方式 1：显式凭证（AccessKeyId & SecretKey）

在配置中直接提供凭证：

```json
{
  "CloudWatch": {
    "Enabled": true,
    "Region": "us-east-1",
    "LogGroup": "/production/mt5bridge",
    "LogStreamPrefix": "app-",
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

**⚠️ 安全提示**：切勿将凭证提交到源代码管理！建议使用：
- 环境变量加载凭证
- AWS Secrets Manager 或 Parameter Store
- 加密的配置文件

#### 方式 2：默认凭证链（推荐）

将 `AccessKeyId` 和 `SecretKey` 留空，使用 AWS 默认凭证链：

```json
{
  "CloudWatch": {
    "Enabled": true,
    "Region": "us-east-1",
    "LogGroup": "/production/mt5bridge",
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

SDK 将按以下顺序自动搜索凭证：
1. **环境变量**：`AWS_ACCESS_KEY_ID`、`AWS_SECRET_ACCESS_KEY`
2. **AWS 凭证文件**：`~/.aws/credentials`（Windows：`%USERPROFILE%\.aws\credentials`）
3. **IAM 角色**：EC2 实例配置文件或 ECS 任务角色
4. **AWS SSO**：如已配置

**推荐场景**：
- 生产环境（使用 IAM 角色）
- 已配置 AWS CLI 的开发环境
- CI/CD 管道（使用环境变量）

---

### 每个 Sink 的独立日志级别配置

每个 sink（Console、File、CloudWatch）可以拥有自己的最小日志级别，实现对不同输出目标的精细控制。

#### 配置策略

**全局 MinimumLevel**：作为第一道过滤器 - 低于此级别的日志事件永远不会被创建。

**Sink 专属 MinimumLevel**：每个 sink 的可选覆盖级别。如果未设置（null），sink 使用全局级别。

#### 示例：生产环境多级别日志

```json
{
  "MinimumLevel": "Debug",  // 创建所有 Debug+ 事件
  "Console": {
    "Enabled": true,
    "TextFormatter": "plain",
    "MinimumLevel": "Information"  // Console 仅显示 Information+
  },
  "File": {
    "Enabled": true,
    "Path": "logs/app-.log",
    "TextFormatter": "plain",
    "MinimumLevel": "Debug"  // File 捕获所有内容（Debug+）
  },
  "CloudWatch": {
    "Enabled": true,
    "Region": "us-east-1",
    "LogGroup": "/production/mt5bridge",
    "MinimumLevel": "Warning",  // CloudWatch 仅存储 Warning+（成本优化）
    "BatchSizeLimit": 500,
    "PeriodSeconds": 10
  }
}
```

#### 使用场景

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

**故障排查**：Console=Debug, File=Verbose, CloudWatch=Debug
```json
{
  "MinimumLevel": "Verbose",
  "Console": { "Enabled": true, "MinimumLevel": "Debug" },
  "File": { "Enabled": true, "MinimumLevel": "Verbose" },
  "CloudWatch": { "Enabled": true, "MinimumLevel": "Debug" }
}
```

**优势**：
- **成本优化**：仅将关键日志发送到 CloudWatch（Warning+），而 File 捕获 Debug
- **性能优化**：减少控制台噪音（Information+），同时保留文件中的详细日志
- **灵活性**：不同环境可以拥有不同的日志策略，无需修改代码

---

### CloudWatch 高级配置

#### 日志流命名策略

本库提供三种 CloudWatch 日志流命名策略：

**1. Default 策略** (`"default"`)
- 格式：`{DateTime}_{HostName}_{Guid}`
- 示例：`2025-12-26-14-30-45_PRODSERVER_a1b2c3d4-e5f6-...`
- 使用场景：需要完整的时间戳和主机名追溯

**2. Constant 策略** (`"constant"`)
- 格式：`{LogStreamPrefix}_{Guid}`
- 示例：`app-server_a1b2c3d4-e5f6-...`
- 使用场景：简单命名 + 唯一标识符

**3. Configurable 策略** (`"configurable"`) ⭐ 推荐
- 格式：`{LogStreamPrefix}/[hostname]/[guid]`（基于 Include 标志）
- 示例：
  - 全部启用：`app-server/PRODSERVER/a1b2c3d4-e5f6-...`
  - 仅前缀：`app-server`
  - 前缀 + 主机名：`app-server/PRODSERVER`
- 使用场景：灵活命名，适应不同部署场景

#### 文本格式化器

本库支持三种 JSON 格式化器，各有不同的性能特点：

**1. JSON Formatter** (`"json"`)
- 标准 Serilog JSON 格式
- 大小：292 字节（基准）
- 性能：基准
- 使用场景：需要标准 JSON 格式时

**2. Compact JSON Formatter** (`"compact"`) ⭐ 推荐
- 来自 Serilog.Formatting.Compact
- 大小：187 字节（比标准小 36%）
- 性能：比标准快 1.89 倍
- 保留消息模板（`@mt` 字段）
- **日志级别行为**：
  * Information 级别：省略 `@l` 字段（优化大小）
  * 其他级别：包含 `@l` 字段（Verbose、Debug、Warning、Error、Fatal）
  * **解决方案**：LogLevelEnricher 为所有级别添加 `"l"` 字段（u3 格式：VRB/DBG/INF/WRN/ERR/FTL）
- 使用场景：需要最优性能的生产环境

**3. Rendered Compact JSON Formatter** (`"rendered-compact"`)
- 预渲染消息模板
- 同时包含 `@mt`（模板）和 `@r`（渲染后的消息）
- 包含 `@i`（事件 ID）用于关联
- **与 compact 格式相同的日志级别行为**（使用 LogLevelEnricher）
- 使用场景：需要结构化和人类可读消息

#### 队列和重试配置

**QueueSizeLimit**（默认：10000）
- 内存中缓冲的最大日志事件数
- 队列满时，新事件会被**丢弃**（不会阻塞）
- 范围：100-100000
- 建议：高吞吐量系统增加到 20000-50000

**RetryAttempts**（默认：5）
- CloudWatch API 调用失败时的重试次数
- 范围：0-255
- 建议：大多数场景保持默认值（5）

#### 日志级别富集器

**自动添加级别字段**

当使用 `compact` 或 `rendered-compact` 格式化器时，库会自动添加 `LogLevelEnricher`，在所有日志事件中包含短格式的日志级别字段（`"l"`）：

```json
// Information 级别（不使用 enricher）
{"@t":"2025-12-27T06:00:00.000Z","@mt":"用户登录",...}

// Information 级别（使用 enricher）
{"@t":"2025-12-27T06:00:00.000Z","@mt":"用户登录","l":"INF",...}

// Warning 级别（同时有 @l 和 l）
{"@t":"2025-12-27T06:00:00.000Z","@mt":"重试尝试","@l":"Warning","l":"WRN",...}
```

**级别缩写（u3 格式）**：
- `VRB` - Verbose
- `DBG` - Debug
- `INF` - Information
- `WRN` - Warning
- `ERR` - Error
- `FTL` - Fatal

**优势**：
- ✅ 所有日志级别的级别字段一致
- ✅ 3 字母紧凑格式节省带宽
- ✅ 在日志聚合系统中易于过滤（如：`l:INF OR l:WRN`）
- ✅ 使用 compact 格式化器时自动激活

---

### 使用示例

#### 1. 标准结构化日志（推荐）

```csharp
var logger = SerilogBootstrapper.CreateLogger(config);

// 简单消息
logger.Information("应用已启动");
logger.Warning("配置不完整");
logger.Error(ex, "操作失败");

// 使用 Serilog 原生模板语法的结构化数据
logger.Information("交易执行: {DealId} {Symbol} {Action} {Volume}@{Price}",
    deal.Deal, deal.Symbol, deal.Action, deal.Volume, deal.Price);

logger.Warning("高延迟: {Symbol} 延迟 {LatencyMs}ms，时间 {Timestamp}",
    symbol, latency, DateTime.UtcNow);

logger.Error(ex, "交易失败: {DealId} {Symbol} {ErrorCode}",
    dealId, symbol, errorCode);
```

**为什么这样做有效**：
- ✅ 零反射 - Serilog 编译属性提取
- ✅ 最小分配 - 直接参数传递
- ✅ 类型安全 - 编译时检查
- ✅ JSON 友好 - 向所有 sink 输出结构化数据

#### 2. 复杂对象

```csharp
// Serilog 使用解构自动序列化对象
logger.Information("用户操作: {@User} {@Action}",
    user,      // @ 前缀表示解构（完整对象序列化）
    action);

// 输出到 JSON sink：
// {
//   "User": { "Id": 123, "Name": "张三", "Email": "zhangsan@example.com" },
//   "Action": { "Type": "Login", "Timestamp": "2025-12-27T10:30:00Z" }
// }
```

#### 3. 性能关键日志

```csharp
// 对于高频日志，先检查日志级别
if (logger.IsEnabled(LogEventLevel.Debug))
{
    logger.Debug("高频 tick: {Symbol} {Bid} {Ask}",
        symbol, bid, ask);
}

// 或使用 Serilog 内置的消息模板惰性求值
logger.Debug("昂贵操作: {Data}",
    new { Result = ExpensiveComputation() });  // 仅在 Debug 启用时求值
```

#### 4. 带上下文的错误日志

```csharp
try
{
    ProcessTrade(deal);
}
catch (Exception ex)
{
    logger.Error(ex, "交易处理失败: {DealId} {Symbol} {Volume} {Reason}",
        deal.Deal, deal.Symbol, deal.Volume, "ValidationError");
}
```

#### 5. 批量操作

```csharp
var stopwatch = Stopwatch.StartNew();
ProcessBatch(deals);
stopwatch.Stop();

logger.Information("批量处理: {TotalDeals} 笔交易，耗时 {DurationMs}ms，平均 {AvgMs}ms/笔",
    deals.Count, stopwatch.ElapsedMilliseconds, stopwatch.ElapsedMilliseconds / (double)deals.Count);
```

---

### 最佳实践

#### 1. 始终使用结构化日志

```csharp
// ❌ 不好 - 字符串拼接，不可搜索
logger.Information($"用户 {userId} 从 {ipAddress} 登录");

// ✅ 好 - 结构化数据，可在 CloudWatch/ELK 中搜索
logger.Information("用户登录: {UserId} 来自 {IpAddress}",
    userId, ipAddress);
```

#### 2. 使用有意义的属性名

```csharp
// ❌ 不好 - 通用名称
logger.Information("处理中: {Id} {Value}", dealId, price);

// ✅ 好 - 具体的、可搜索的名称
logger.Information("交易处理: {DealId} {Price}", dealId, price);
```

#### 3. 解构复杂对象

```csharp
// 使用 @ 前缀将对象解构为结构化数据
logger.Information("订单创建: {@Order}", order);

// 输出: { "Order": { "Id": 123, "Symbol": "EURUSD", "Volume": 1.5 } }
```

#### 4. 不同环境的配置

**开发环境**: 详细的控制台输出
```json
{
  "MinimumLevel": "Debug",
  "Console": { "Enabled": true, "TextFormatter": "plain" },
  "File": { "Enabled": true, "MinimumLevel": "Verbose" },
  "CloudWatch": { "Enabled": false }
}
```

**生产环境**: 最小化控制台，完整文件日志，关键 CloudWatch
```json
{
  "MinimumLevel": "Information",
  "Console": { "Enabled": true, "MinimumLevel": "Warning" },
  "File": { "Enabled": true, "TextFormatter": "json" },
  "CloudWatch": { "Enabled": true, "MinimumLevel": "Warning", "TextFormatter": "compact" }
}
```

#### 5. 性能考虑

```csharp
// 对于高频日志，先检查级别
if (logger.IsEnabled(LogEventLevel.Debug))
{
    logger.Debug("Tick: {Symbol} {Bid}/{Ask}", symbol, bid, ask);
}

// Serilog 的惰性求值自动配合消息模板工作
logger.Debug("数据: {ComplexData}", GetComplexData());  // 仅在 Debug 启用时调用
```

---

### 性能优化

#### Serilog 性能特点

Serilog 的原生结构化日志已经非常高效：

1. **零反射**：消息模板在第一次使用时编译，后续调用直接使用编译后的表达式
2. **最小分配**：参数直接传递，不创建中间对象
3. **惰性求值**：日志级别关闭时，参数表达式不会被求值
4. **高效序列化**：内置的 JSON 格式化器经过高度优化

#### 高性能日志模式

```csharp
// ✅ 推荐：直接使用 Serilog 模板
logger.Information("交易: {DealId} {Symbol} {Volume}@{Price}",
    deal.Deal, deal.Symbol, deal.Volume, deal.Price);

// ✅ 高频日志：先检查级别
if (logger.IsEnabled(LogEventLevel.Debug))
{
    logger.Debug("Tick: {Symbol} {Bid}/{Ask}", symbol, bid, ask);
}

// ✅ 惰性求值：仅在启用时计算
logger.Debug("详细数据: {Data}",
    new { Result = ExpensiveComputation() });  // Debug 关闭时不调用
```

#### CloudWatch 高吞吐量调优

生产环境高流量配置：
```json
{
  "CloudWatch": {
    "Enabled": true,
    "BatchSizeLimit": 800,      // 增加批次大小
    "PeriodSeconds": 10,         // 增加刷新间隔
    "QueueSizeLimit": 50000,     // 增加队列大小
    "MinimumLevel": "Warning"    // 仅记录 Warning+
  }
}
```

---

### 优雅关闭

应用退出时总是调用 `FlushAndCloseAsync()` 以确保：
- 所有缓冲日志刷新到磁盘/CloudWatch
- AWS CloudWatch 客户端正确释放
- 没有日志丢失

```csharp
try
{
    await app.RunAsync();
}
finally
{
    await SerilogBootstrapper.FlushAndCloseAsync();
}
```

---

### 错误处理

Logger 在启动时验证所有配置。常见错误和修复：

| 错误                                                                              | 原因           | 修复                                           |
| --------------------------------------------------------------------------------- | -------------- | ---------------------------------------------- |
| `InvalidOperationException: At least one logging sink must be enabled`            | 所有 Sink 禁用 | 至少启用 Console 或 File                       |
| `InvalidOperationException: File.Path cannot be empty`                            | 缺少路径       | 在配置中提供有效的文件路径                     |
| `InvalidOperationException: CloudWatch.BatchSizeLimit must be between 1 and 1000` | 无效批次大小   | 设置 BatchSizeLimit 为 1-1000 之间             |
| `InvalidOperationException: Invalid AWS region`                                   | 错误的区域名   | 使用有效的 AWS 区域 (us-east-1, eu-west-1 等)  |
| `InvalidOperationException: Failed to create CloudWatch client`                   | 缺少 AWS 凭证  | 配置 AWS 凭证（环境变量、配置文件或 IAM 角色） |

---

### 线程安全

库完全线程安全：
- `SerilogBootstrapper.CreateLogger()` 支持多线程调用
- `SerilogBootstrapper.Dispose()` 使用锁实现线程安全
- `ModelLogger` 线程安全（Logger 字段管理正确）

示例：
```csharp
// 多线程场景安全
var tasks = Enumerable.Range(0, 100)
    .Select(_ => Task.Run(() => logger.Information("线程 {Id}", Thread.CurrentThread.ManagedThreadId)))
    .ToArray();
await Task.WhenAll(tasks);
```

---

### 故障排查

**CloudWatch 中没有日志？**
- 检查 AWS 凭证是否已配置
- 验证 IAM 角色是否有 CloudWatchLogs 权限
- 检查日志组是否存在（或在 AWS 启用自动创建）
- 验证区域是否正确

**文件日志内存使用过高？**
- 降低 `File.FileSizeLimitBytes` 或 `File.RetainedFileCountLimit`
- 在生产环境禁用 Debug 日志（`MinimumLevel: Information`）

**CloudWatch API 速率限制错误？**
- 增加 `CloudWatch.PeriodSeconds` 以批量处理更多日志
- 增加 `CloudWatch.BatchSizeLimit` 至 800-1000

---

### 依赖项

- **Serilog** 4.0.0 - 核心日志框架
- **Serilog.Sinks.Console** 6.0.0 - 控制台输出
- **Serilog.Sinks.File** 6.0.0 - 文件输出带滚动
- **Serilog.Sinks.AwsCloudWatch** 4.0.171 - CloudWatch 集成
- **Serilog.Formatting.Compact** 3.0.0 - 紧凑 JSON 格式
- **AWSSDK.CloudWatchLogs** 3.7.300 - AWS CloudWatch API
- **.NET 8.0** - 目标框架（使用 Source-Generated JSON）

---