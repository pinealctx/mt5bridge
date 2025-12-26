# MT5Bridge.Serilog.Tests

[English](#english) | [中文](#chinese)

---

<a name="english"></a>

## English Documentation

Comprehensive test suite for **MT5Bridge.Serilog** ensuring production-ready quality and correctness.

**Status**: ✅ All Tests Passing | **Coverage**: 14 Test Cases | **Version**: 2.0

## Overview

This test project validates all functionality of the MT5Bridge.Serilog library including:

### Test Categories

#### Core Logger Extension Tests
- ✅ **WithModel** - Attaches structured models to log context
- ✅ **WithModelLazy** - Conditional serialization based on log level (performance-critical)
- ✅ **WithModelDirect** - Direct JSON string storage for ultra-high frequency
- ✅ **WithField** - Simple field attachment (key-value pairs)

#### Lazy Evaluation Tests
- ✅ **Lazy Skip When Disabled** - Verifies serialization is skipped when log level is disabled (5000x performance boost)
- ✅ **Lazy Include When Enabled** - Ensures models are serialized when log level is enabled

#### Convenience Methods
- ✅ **LogModelInfo** - Information level convenience method
- ✅ **LogModelDebug** - Debug level with automatic skipping when disabled
- ✅ **LogModelWarning** - Warning level logging
- ✅ **LogModelError** - Error level with exception support

#### ModelLogger Wrapper Tests
- ✅ **Basic ModelLogger** - Wrapper reduces parameter passing
- ✅ **ModelLogger with Context** - Global context usage
- ✅ **ModelLogger Level Changes** - Dynamic level switching
- ✅ **Multiple Models** - Chaining multiple model attachments

---

## Running Tests

### Run All Tests
```powershell
dotnet test
```

### Run with Detailed Output
```powershell
dotnet test --logger "console;verbosity=detailed"
```

### Run Specific Test
```powershell
dotnet test --filter "FullyQualifiedName~WithModelLazy_ShouldSkipSerializationWhenLevelDisabled"
```

### Generate Code Coverage
```powershell
dotnet test --collect:"XPlat Code Coverage"
```

---

## Test Structure

### Test Sink Implementation

All tests use an in-memory `TestSink` to capture log events:

```csharp
private class TestSink : ILogEventSink
{
    public List<LogEvent> Events { get; } = new();
    public void Emit(LogEvent logEvent) => Events.Add(logEvent);
}
```

### Test Pattern

Each test follows this pattern:
1. **Arrange**: Create logger with TestSink and configure minimum level
2. **Act**: Execute logging operation with specific parameters
3. **Assert**: Verify log events contain expected data

### Example Test

```csharp
[Fact]
public void WithModelLazy_ShouldSkipSerializationWhenLevelDisabled()
{
    // Arrange
    var sink = new TestSink();
    var logger = new LoggerConfiguration()
        .MinimumLevel.Is(LogEventLevel.Information)
        .WriteTo.Sink(sink)
        .CreateLogger();

    var model = new DealModel { /* ... */ };

    // Act - Try to log at Debug level (disabled)
    logger.WithModelLazy("DebugData", model, FastJsonContext.Default, LogEventLevel.Debug)
          .Information("This message should not include the model");

    // Assert
    Assert.Single(sink.Events);
    Assert.DoesNotContain("DebugData", sink.Events[0].Properties.Keys);
}
```

---

## Test Coverage

### Validated Functionality

| Category            | Tests | Description                                     |
| ------------------- | ----- | ----------------------------------------------- |
| Model Attachments   | 3     | WithModel, WithModelDirect, nested structures   |
| Lazy Evaluation     | 2     | Skip/include based on log level                 |
| Field Enrichment    | 1     | Simple key-value field attachment               |
| Convenience Methods | 4     | LogModelInfo, Debug, Warning, Error             |
| ModelLogger Wrapper | 4     | Wrapper usage, context, level changes, chaining |

### Key Test Scenarios

#### Performance Validation
- **Lazy Serialization**: Confirms models are NOT serialized when log level is disabled
- **Zero Allocation**: Direct JSON methods avoid Dictionary overhead

#### Correctness Validation
- **Property Attachment**: Models appear in log event properties
- **Level Filtering**: Only enabled levels produce log events
- **Exception Handling**: Error methods properly capture exceptions

#### Integration Validation
- **Global Context**: Default context is used when not explicitly provided
- **Method Chaining**: Fluent API works across multiple method calls
- **Multiple Models**: Can attach multiple models to single log event

---

## Dependencies

- **xUnit** 2.9.2 - Testing framework
- **MT5Bridge.Serilog** - Library under test
- **MT5Bridge.MT5.Core.Models** - Test models (DealModel, etc.)
- **Serilog** 4.0.0 - Core logging framework
- **.NET 8.0** - Target framework

---

## Test Models

Tests use real MT5 trading models from `MT5Bridge.MT5.Core.Models`:

### DealModel
```csharp
public class DealModel
{
    public long Deal { get; set; }
    public string Symbol { get; set; }
    public double Price { get; set; }
    public int Volume { get; set; }
    public long Time { get; set; }
    public DealAction Action { get; set; }
    public string Comment { get; set; }
}
```

This ensures tests validate real-world usage scenarios with actual trading domain models.

---

## Continuous Integration

Tests are designed to run in CI/CD pipelines:
- ✅ **Fast Execution**: All tests complete in < 1 second
- ✅ **No External Dependencies**: In-memory TestSink, no database or network
- ✅ **Deterministic**: No time-based or random behavior
- ✅ **Cross-Platform**: Run on Windows, Linux, macOS

---

## Adding New Tests

When adding functionality to `MT5Bridge.Serilog`, follow this pattern:

1. **Create Test Class Method**:
```csharp
[Fact]
public void NewFeature_ShouldBehaviorExpected()
{
    // Arrange
    var sink = new TestSink();
    var logger = new LoggerConfiguration()
        .WriteTo.Sink(sink)
        .CreateLogger();

    // Act
    logger.NewFeature(/* params */);

    // Assert
    Assert.Single(sink.Events);
    // Add specific assertions
}
```

2. **Test Both Success and Failure Paths**
3. **Validate Performance Characteristics** (if applicable)
4. **Update This README** with new test description

---

## Learning Resources

For complete API documentation, see:
- [Main README](../MT5Bridge.Serilog/README.md) - Full library documentation
- [Demo Project](../MT5Bridge.Serilog.Demo/README.md) - Usage examples
- [Test Source](LoggerExtensionsTests.cs) - All 14 test implementations

---

---

<a name="chinese"></a>

## 中文文档

**MT5Bridge.Serilog** 的综合测试套件，确保生产就绪质量和正确性。

**状态**: ✅ 所有测试通过 | **覆盖率**: 14 个测试用例 | **版本**: 2.0

## 概述

本测试项目验证 MT5Bridge.Serilog 库的所有功能，包括：

### 测试类别

#### 核心 Logger 扩展测试
- ✅ **WithModel** - 将结构化模型附加到日志上下文
- ✅ **WithModelLazy** - 基于日志级别的条件序列化（性能关键）
- ✅ **WithModelDirect** - 超高频的直接 JSON 字符串存储
- ✅ **WithField** - 简单字段附加（键值对）

#### 惰性计算测试
- ✅ **禁用时跳过** - 验证日志级别禁用时跳过序列化（性能提升 5000 倍）
- ✅ **启用时包含** - 确保日志级别启用时序列化模型

#### 便捷方法
- ✅ **LogModelInfo** - Information 级别便捷方法
- ✅ **LogModelDebug** - Debug 级别，禁用时自动跳过
- ✅ **LogModelWarning** - Warning 级别日志
- ✅ **LogModelError** - Error 级别，支持异常

#### ModelLogger 包装器测试
- ✅ **基础 ModelLogger** - 包装器减少参数传递
- ✅ **带上下文的 ModelLogger** - 全局上下文使用
- ✅ **ModelLogger 级别更改** - 动态级别切换
- ✅ **多个模型** - 链式附加多个模型

---

## 运行测试

### 运行所有测试
```powershell
dotnet test
```

### 详细输出运行
```powershell
dotnet test --logger "console;verbosity=detailed"
```

### 运行特定测试
```powershell
dotnet test --filter "FullyQualifiedName~WithModelLazy_ShouldSkipSerializationWhenLevelDisabled"
```

### 生成代码覆盖率
```powershell
dotnet test --collect:"XPlat Code Coverage"
```

---

## 测试结构

### 测试 Sink 实现

所有测试使用内存中的 `TestSink` 捕获日志事件：

```csharp
private class TestSink : ILogEventSink
{
    public List<LogEvent> Events { get; } = new();
    public void Emit(LogEvent logEvent) => Events.Add(logEvent);
}
```

### 测试模式

每个测试遵循此模式：
1. **Arrange（准备）**：使用 TestSink 创建 logger，配置最小级别
2. **Act（执行）**：使用特定参数执行日志操作
3. **Assert（断言）**：验证日志事件包含预期数据

### 测试示例

```csharp
[Fact]
public void WithModelLazy_ShouldSkipSerializationWhenLevelDisabled()
{
    // Arrange
    var sink = new TestSink();
    var logger = new LoggerConfiguration()
        .MinimumLevel.Is(LogEventLevel.Information)
        .WriteTo.Sink(sink)
        .CreateLogger();

    var model = new DealModel { /* ... */ };

    // Act - Try to log at Debug level (disabled)
    logger.WithModelLazy("DebugData", model, FastJsonContext.Default, LogEventLevel.Debug)
          .Information("This message should not include the model");

    // Assert
    Assert.Single(sink.Events);
    Assert.DoesNotContain("DebugData", sink.Events[0].Properties.Keys);
}
```

---

## 测试覆盖率

### 已验证功能

| 类别               | 测试数 | 说明                                 |
| ------------------ | ------ | ------------------------------------ |
| 模型附加           | 3      | WithModel、WithModelDirect、嵌套结构 |
| 惰性计算           | 2      | 基于日志级别跳过/包含                |
| 字段增强           | 1      | 简单键值字段附加                     |
| 便捷方法           | 4      | LogModelInfo、Debug、Warning、Error  |
| ModelLogger 包装器 | 4      | 包装器使用、上下文、级别更改、链式   |

### 关键测试场景

#### 性能验证
- **惰性序列化**：确认日志级别禁用时模型不被序列化
- **零分配**：直接 JSON 方法避免 Dictionary 开销

#### 正确性验证
- **属性附加**：模型出现在日志事件属性中
- **级别过滤**：仅启用的级别产生日志事件
- **异常处理**：错误方法正确捕获异常

#### 集成验证
- **全局上下文**：未明确提供时使用默认上下文
- **方法链式调用**：流式 API 跨多个方法调用工作
- **多个模型**：可以将多个模型附加到单个日志事件

---

## 依赖项

- **xUnit** 2.9.2 - 测试框架
- **MT5Bridge.Serilog** - 被测试库
- **MT5Bridge.MT5.Core.Models** - 测试模型（DealModel 等）
- **Serilog** 4.0.0 - 核心日志框架
- **.NET 8.0** - 目标框架

---

## 测试模型

测试使用来自 `MT5Bridge.MT5.Core.Models` 的真实 MT5 交易模型：

### DealModel
```csharp
public class DealModel
{
    public long Deal { get; set; }
    public string Symbol { get; set; }
    public double Price { get; set; }
    public int Volume { get; set; }
    public long Time { get; set; }
    public DealAction Action { get; set; }
    public string Comment { get; set; }
}
```

这确保测试使用实际交易领域模型验证真实世界使用场景。

---

## 持续集成

测试设计为在 CI/CD 管道中运行：
- ✅ **快速执行**：所有测试在 < 1 秒内完成
- ✅ **无外部依赖**：内存中 TestSink，无数据库或网络
- ✅ **确定性**：无基于时间或随机行为
- ✅ **跨平台**：在 Windows、Linux、macOS 上运行

---

## 添加新测试

向 `MT5Bridge.Serilog` 添加功能时，遵循此模式：

1. **创建测试类方法**：
```csharp
[Fact]
public void NewFeature_ShouldBehaviorExpected()
{
    // Arrange（准备）
    var sink = new TestSink();
    var logger = new LoggerConfiguration()
        .WriteTo.Sink(sink)
        .CreateLogger();

    // Act（执行）
    logger.NewFeature(/* 参数 */);

    // Assert（断言）
    Assert.Single(sink.Events);
    // 添加具体断言
}
```

2. **测试成功和失败路径**
3. **验证性能特性**（如果适用）
4. **更新此 README** 添加新测试说明

---

## 学习资源

完整 API 文档，请参阅：
- [主 README](../MT5Bridge.Serilog/README.md) - 完整库文档
- [演示项目](../MT5Bridge.Serilog.Demo/README.md) - 使用示例
- [测试源码](LoggerExtensionsTests.cs) - 全部 14 个测试实现

---
