# MT5Bridge.Manager.Tests

[English](#english) | [中文](#chinese)

---

<a name="english"></a>

## English Documentation

Comprehensive test suite for MT5Bridge.Manager library, covering generic event handlers, dual serialization systems (POCO + Protobuf), model conversions, and result patterns.

**Test Framework**: xUnit 2.5.3 | **Coverage Tool**: Coverlet | **Total Tests**: 38 | **Status**: ✅ All Passing

---

### 🎯 Overview

This test project validates the core functionality of MT5Bridge.Manager without requiring a live MT5 Server connection. It focuses on:

- **Generic Event Handler System** - Registration and lifecycle management
- **Dual Serialization** - POCO JSON and Protobuf binary formats
- **Model Conversions** - Data transformations between MT5 SDK and application models
- **Result Patterns** - Error handling and result wrapper validation

---

### 📊 Test Coverage

#### 1. GenericEventHandlerRegistrationTests (11 tests)

Validates the generic event handler registration system for both POCO and Protobuf models.

**What's Tested:**
- ✅ Manager creation and proper disposal
- ✅ POCO handler registration (Deal, Order, Position)
- ✅ Protobuf handler registration (Deal, Order, Position)
- ✅ Multiple handlers for the same event type
- ✅ Handlers with all callbacks (onAdd, onUpdate, onDelete)
- ✅ Handlers with partial/null callbacks
- ✅ Handler lifecycle management

**Key Test Cases:**
```csharp
[Fact] public void RegisterDealHandler_ShouldNotThrow()
[Fact] public void RegisterDealProtoHandler_ShouldNotThrow()
[Fact] public void RegisterOrderHandler_ShouldNotThrow()
[Fact] public void RegisterPositionHandler_ShouldNotThrow()
[Fact] public void RegisterMultipleDealHandlers_ShouldNotThrow()
```

**Limitations:**
- These tests verify registration doesn't throw exceptions
- Actual event firing requires a live MT5 Server connection
- For integration testing, see [MT5Bridge.MT5.Demo](../MT5Bridge.MT5.Demo)

---

#### 2. JsonSerializationTests (6 tests)

Validates source-generated JSON serialization for POCO models.

**What's Tested:**
- ✅ Fast JSON mode (compact, enums as numbers) for network/database
- ✅ Readable JSON mode (indented, enums as strings) for logging
- ✅ Round-trip deserialization (serialize → deserialize → equals)
- ✅ Complex nested types (ApiDataModel collections)
- ✅ Enum and flags serialization (TradeActivationFlags, etc.)

**JSON Contexts Tested:**
- `FastJsonContext` - Compact, numbers for enums (network/DB)
- `ReadableJsonContext` - Indented, strings for enums (logging)

**Example Output:**
```json
// Fast JSON (Database/Network)
{"deal":12345678,"action":0,"entry":0,"reason":3}

// Readable JSON (Logging)
{
  "deal": 12345678,
  "action": "Buy",
  "entry": "In",
  "reason": "Expert"
}
```

---

#### 3. ProtobufSerializationTests (8 tests)

Validates Protobuf binary serialization for high-performance scenarios.

**What's Tested:**
- ✅ Binary serialization round-trip (Deal, Order, Position, User, Account, Group)
- ✅ Protobuf JSON formatting (ToReadableJson extension methods)
- ✅ Size comparison (Protobuf vs JSON - typically 60-70% smaller)
- ✅ Complex nested types and collections

**Performance Characteristics:**
- **Protobuf Binary**: 3-5x faster serialization, 60-70% smaller size
- **Protobuf JSON**: Human-readable format for debugging

---

#### 4. PocoModelTests (5 tests)

Validates POCO model properties, defaults, and business logic.

**What's Tested:**
- ✅ All model properties (Deal, Order, Position, Account, Group)
- ✅ PriceSL and PriceTP computed properties
- ✅ Collection properties (ApiData)
- ✅ Enum and flags support (TradeActivationFlags, TradeModifyFlags)
- ✅ Default values and property validation

---

#### 5. MT5ResultTests (4 tests)

Validates the Result pattern wrapper for operation results.

**What's Tested:**
- ✅ Success state creation and validation
- ✅ Failure state with error codes and messages
- ✅ Generic data handling (MT5Result<T>)
- ✅ RetCode propagation from MT5 SDK

---

#### 6. UserModelTests (2 tests)

Validates user model specifics and legacy compatibility.

**What's Tested:**
- ✅ Property assignment and retrieval
- ✅ Legacy `Name` property support (marked Obsolete)

---

#### 7. MT5ManagerSinkTests (2 tests)

Validates the manager sink for connection events.

**What's Tested:**
- ✅ Sink creation and registration
- ✅ Disconnect event callback

---

### 🚀 Running Tests

#### Quick Run

```bash
# Run all tests
dotnet test

# Run in Release mode
dotnet test -c Release

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"
```

#### Using Test Runner Scripts

**Windows PowerShell:**
```powershell
# Basic run
.\run-tests.ps1

# With code coverage
.\run-tests.ps1 -Coverage

# Filter specific tests
.\run-tests.ps1 -Filter "DealHandler"

# Verbose output
.\run-tests.ps1 -Verbose

# All options
.\run-tests.ps1 -Coverage -Filter "Json" -Verbose
```

**Linux/macOS:**
```bash
# Basic run
./run-tests.sh

# With code coverage
./run-tests.sh --coverage

# Filter specific tests
./run-tests.sh --filter "DealHandler"

# Verbose output
./run-tests.sh --verbose
```

---

### 📈 Code Coverage

Generate code coverage reports using Coverlet:

```bash
# Generate coverage
dotnet test --collect:"XPlat Code Coverage"

# Coverage output location
TestResults/{guid}/coverage.cobertura.xml
```

**Install Coverage Report Generator:**
```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

**Generate HTML Report:**
```bash
reportgenerator `
  -reports:"TestResults/*/coverage.cobertura.xml" `
  -targetdir:"TestResults/CoverageReport" `
  -reporttypes:Html
```

**View Report:**
```bash
# Windows
start TestResults/CoverageReport/index.html

# Linux/macOS
open TestResults/CoverageReport/index.html
```

---

### 🏗️ Project Structure

```
MT5Bridge.Manager.Tests/
├── GenericEventHandlerRegistrationTests.cs  # Event handler registration
├── JsonSerializationTests.cs               # POCO JSON serialization
├── ProtobufSerializationTests.cs           # Protobuf binary serialization
├── PocoModelTests.cs                       # POCO model properties
├── MT5ResultTests.cs                       # Result pattern
├── UserModelTests.cs                       # User model specifics
├── MT5ManagerSinkTests.cs                  # Manager sink events
├── run-tests.ps1                           # PowerShell test runner
├── run-tests.sh                            # Bash test runner
├── MT5Bridge.Manager.Tests.csproj         # Test project file
└── README.md                               # This file
```

---

### 🔧 Dependencies

#### NuGet Packages

| Package                   | Version | Purpose                    |
| ------------------------- | ------- | -------------------------- |
| xUnit                     | 2.5.3   | Test framework             |
| xunit.runner.visualstudio | 2.5.3   | Visual Studio test adapter |
| Microsoft.NET.Test.Sdk    | 17.8.0  | Test SDK                   |
| coverlet.collector        | 6.0.0   | Code coverage              |
| Serilog                   | 4.1.0   | Logging in tests           |
| Serilog.Sinks.XUnit       | 3.0.5   | Test output logging        |
| Roslynator.Analyzers      | 4.12.8  | Code analysis              |

#### Project References

- **MT5Bridge.Manager** - Library under test

#### Native References

- **MetaQuotes.MT5CommonAPI64.dll** - MT5 Common API
- **MetaQuotes.MT5ManagerAPI64.dll** - MT5 Manager API

---

### ⚠️ Important Notes

#### Test Scope

**What These Tests DO:**
- ✅ Verify generic handler registration doesn't throw
- ✅ Validate JSON serialization works correctly
- ✅ Test Protobuf binary serialization
- ✅ Validate model properties and conversions
- ✅ Test result pattern implementations
- ✅ Verify manager lifecycle management

**What These Tests DON'T DO:**
- ❌ Connect to actual MT5 Server
- ❌ Fire real events from MT5
- ❌ Test network communication
- ❌ Validate MT5 SDK integration end-to-end

#### Integration Testing

For end-to-end testing with a real MT5 Server:

1. **Use the Demo Project**: [MT5Bridge.MT5.Demo](../MT5Bridge.MT5.Demo)
2. **Configure Connection**: Set up `appsettings.json` with real credentials
3. **Run Scenarios**: Execute various commands to test connectivity and events

See [MT5Bridge.MT5.Demo/README.md](../MT5Bridge.MT5.Demo/README.md) for details.

---

### 🛠️ Development Workflow

#### Adding New Tests

1. **Create Test Class**:
```csharp
public class MyNewTests
{
    private readonly ITestOutputHelper _output;
    private readonly Serilog.ILogger _logger;

    public MyNewTests(ITestOutputHelper output)
    {
        _output = output;
        _logger = new LoggerConfiguration()
            .WriteTo.TestOutput(output)
            .CreateLogger();
    }

    [Fact]
    public void MyTest_Should_DoSomething()
    {
        // Arrange
        // Act
        // Assert
    }
}
```

2. **Run Tests**:
```bash
dotnet test --filter "MyNewTests"
```

3. **Update README**: Add test coverage information

#### Best Practices

- **Follow AAA Pattern**: Arrange, Act, Assert
- **Use ITestOutputHelper**: For debugging output
- **Test One Thing**: Each test should verify one behavior
- **Meaningful Names**: Test names should describe what they verify
- **Clean Up Resources**: Use `using` statements for IDisposable
- **Avoid External Dependencies**: Keep tests isolated

---

### 📝 Test Naming Convention

Format: `{MethodName}_{Scenario}_{ExpectedBehavior}`

Examples:
- `RegisterDealHandler_ShouldNotThrow`
- `JsonSerialization_FastMode_ShouldCompactEnums`
- `CreateManager_WithValidLogger_ShouldSucceed`

---

### 🐛 Debugging Tests

#### Visual Studio
1. Open Test Explorer (Test → Test Explorer)
2. Right-click test → Debug
3. Set breakpoints in test or source code

#### VS Code
1. Install C# extension
2. Set breakpoints
3. Run test with debugger attached

#### Command Line
```bash
# Run specific test with detailed output
dotnet test --filter "FullyQualifiedName~DealHandler" --logger "console;verbosity=detailed"
```

---

### 🤝 Contributing

When contributing tests:

1. **Follow Conventions**: Use xUnit patterns and naming
2. **Add Documentation**: XML comments for complex tests
3. **Verify Coverage**: Run coverage reports
4. **Update README**: Document new test classes
5. **All Tests Must Pass**: Before submitting PR

---

### 📄 License

MIT License - See [LICENSE](../LICENSE) for details.

---

### 🔗 Related Projects

- **[MT5Bridge.Manager](../MT5Bridge.Manager/)** - Library under test
- **[MT5Bridge.MT5.Demo](../MT5Bridge.MT5.Demo/)** - Integration testing
- **[MT5Bridge.Core](../MT5Bridge.Core/)** - Core utilities

---

<a name="chinese"></a>

## 中文文档

MT5Bridge.Manager 库的综合测试套件，涵盖泛型事件处理器、双重序列化系统（POCO + Protobuf）、模型转换和结果模式。

**测试框架**: xUnit 2.5.3 | **覆盖率工具**: Coverlet | **测试总数**: 38 | **状态**: ✅ 全部通过

---

### 🎯 概述

本测试项目在不需要实时 MT5 Server 连接的情况下验证 MT5Bridge.Manager 的核心功能。重点关注：

- **泛型事件处理器系统** - 注册和生命周期管理
- **双重序列化** - POCO JSON 和 Protobuf 二进制格式
- **模型转换** - MT5 SDK 和应用程序模型之间的数据转换
- **结果模式** - 错误处理和结果包装器验证

---

### 📊 测试覆盖

#### 1. GenericEventHandlerRegistrationTests（11 个测试）

验证 POCO 和 Protobuf 模型的泛型事件处理器注册系统。

**测试内容：**
- ✅ Manager 创建和正确释放
- ✅ POCO 处理器注册（成交、订单、持仓）
- ✅ Protobuf 处理器注册（成交、订单、持仓）
- ✅ 同一事件类型的多个处理器
- ✅ 包含所有回调的处理器（onAdd、onUpdate、onDelete）
- ✅ 部分/空回调的处理器
- ✅ 处理器生命周期管理

**限制：**
- 这些测试验证注册不会抛出异常
- 实际的事件触发需要实时 MT5 Server 连接
- 集成测试请参见 [MT5Bridge.MT5.Demo](../MT5Bridge.MT5.Demo)

---

#### 2. JsonSerializationTests（6 个测试）

验证 POCO 模型的源生成 JSON 序列化。

**测试内容：**
- ✅ 快速 JSON 模式（紧凑，枚举为数字）用于网络/数据库
- ✅ 可读 JSON 模式（缩进，枚举为字符串）用于日志记录
- ✅ 往返反序列化（序列化 → 反序列化 → 相等）
- ✅ 复杂嵌套类型（ApiDataModel 集合）
- ✅ 枚举和标志序列化（TradeActivationFlags 等）

**测试的 JSON 上下文：**
- `FastJsonContext` - 紧凑，枚举为数字（网络/数据库）
- `ReadableJsonContext` - 缩进，枚举为字符串（日志记录）

**示例输出：**
```json
// 快速 JSON（数据库/网络）
{"deal":12345678,"action":0,"entry":0,"reason":3}

// 可读 JSON（日志记录）
{
  "deal": 12345678,
  "action": "Buy",
  "entry": "In",
  "reason": "Expert"
}
```

---

#### 3. ProtobufSerializationTests（8 个测试）

验证高性能场景的 Protobuf 二进制序列化。

**测试内容：**
- ✅ 二进制序列化往返（成交、订单、持仓、用户、账户、组）
- ✅ Protobuf JSON 格式化（ToReadableJson 扩展方法）
- ✅ 大小对比（Protobuf vs JSON - 通常小 60-70%）
- ✅ 复杂嵌套类型和集合

**性能特征：**
- **Protobuf 二进制**: 序列化快 3-5 倍，大小减少 60-70%
- **Protobuf JSON**: 用于调试的人类可读格式

---

#### 4. PocoModelTests（5 个测试）

验证 POCO 模型属性、默认值和业务逻辑。

**测试内容：**
- ✅ 所有模型属性（成交、订单、持仓、账户、组）
- ✅ PriceSL 和 PriceTP 计算属性
- ✅ 集合属性（ApiData）
- ✅ 枚举和标志支持（TradeActivationFlags、TradeModifyFlags）
- ✅ 默认值和属性验证

---

#### 5. MT5ResultTests（4 个测试）

验证操作结果的 Result 模式包装器。

**测试内容：**
- ✅ 成功状态创建和验证
- ✅ 失败状态与错误码和消息
- ✅ 泛型数据处理（MT5Result<T>）
- ✅ 来自 MT5 SDK 的 RetCode 传播

---

#### 6. UserModelTests（2 个测试）

验证用户模型特性和遗留兼容性。

**测试内容：**
- ✅ 属性赋值和检索
- ✅ 遗留 `Name` 属性支持（已标记为过时）

---

#### 7. MT5ManagerSinkTests（2 个测试）

验证连接事件的 manager sink。

**测试内容：**
- ✅ Sink 创建和注册
- ✅ 断开连接事件回调

---

### 🚀 运行测试

#### 快速运行

```bash
# 运行所有测试
dotnet test

# Release 模式运行
dotnet test -c Release

# 详细输出
dotnet test --logger "console;verbosity=detailed"
```

#### 使用测试运行脚本

**Windows PowerShell：**
```powershell
# 基本运行
.\run-tests.ps1

# 带代码覆盖率
.\run-tests.ps1 -Coverage

# 过滤特定测试
.\run-tests.ps1 -Filter "DealHandler"

# 详细输出
.\run-tests.ps1 -Verbose

# 所有选项
.\run-tests.ps1 -Coverage -Filter "Json" -Verbose
```

**Linux/macOS：**
```bash
# 基本运行
./run-tests.sh

# 带代码覆盖率
./run-tests.sh --coverage

# 过滤特定测试
./run-tests.sh --filter "DealHandler"

# 详细输出
./run-tests.sh --verbose
```

---

### 📈 代码覆盖率

使用 Coverlet 生成代码覆盖率报告：

```bash
# 生成覆盖率
dotnet test --collect:"XPlat Code Coverage"

# 覆盖率输出位置
TestResults/{guid}/coverage.cobertura.xml
```

**安装覆盖率报告生成器：**
```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

**生成 HTML 报告：**
```bash
reportgenerator `
  -reports:"TestResults/*/coverage.cobertura.xml" `
  -targetdir:"TestResults/CoverageReport" `
  -reporttypes:Html
```

**查看报告：**
```bash
# Windows
start TestResults/CoverageReport/index.html

# Linux/macOS
open TestResults/CoverageReport/index.html
```

---

### 🏗️ 项目结构

```
MT5Bridge.Manager.Tests/
├── GenericEventHandlerRegistrationTests.cs  # 事件处理器注册
├── JsonSerializationTests.cs               # POCO JSON 序列化
├── ProtobufSerializationTests.cs           # Protobuf 二进制序列化
├── PocoModelTests.cs                       # POCO 模型属性
├── MT5ResultTests.cs                       # Result 模式
├── UserModelTests.cs                       # 用户模型特定测试
├── MT5ManagerSinkTests.cs                  # Manager sink 事件
├── run-tests.ps1                           # PowerShell 测试运行器
├── run-tests.sh                            # Bash 测试运行器
├── MT5Bridge.Manager.Tests.csproj         # 测试项目文件
└── README.md                               # 本文件
```

---

### 🔧 依赖项

#### NuGet 包

| 包                        | 版本   | 用途                     |
| ------------------------- | ------ | ------------------------ |
| xUnit                     | 2.5.3  | 测试框架                 |
| xunit.runner.visualstudio | 2.5.3  | Visual Studio 测试适配器 |
| Microsoft.NET.Test.Sdk    | 17.8.0 | 测试 SDK                 |
| coverlet.collector        | 6.0.0  | 代码覆盖率               |
| Serilog                   | 4.1.0  | 测试中的日志记录         |
| Serilog.Sinks.XUnit       | 3.0.5  | 测试输出日志记录         |
| Roslynator.Analyzers      | 4.12.8 | 代码分析                 |

#### 项目引用

- **MT5Bridge.Manager** - 被测试的库

#### 原生引用

- **MetaQuotes.MT5CommonAPI64.dll** - MT5 Common API
- **MetaQuotes.MT5ManagerAPI64.dll** - MT5 Manager API

---

### ⚠️ 重要说明

#### 测试范围

**这些测试做什么：**
- ✅ 验证泛型处理器注册不会抛出异常
- ✅ 验证 JSON 序列化正常工作
- ✅ 测试 Protobuf 二进制序列化
- ✅ 验证模型属性和转换
- ✅ 测试结果模式实现
- ✅ 验证 manager 生命周期管理

**这些测试不做什么：**
- ❌ 连接到实际的 MT5 Server
- ❌ 触发来自 MT5 的真实事件
- ❌ 测试网络通信
- ❌ 端到端验证 MT5 SDK 集成

#### 集成测试

使用真实 MT5 Server 进行端到端测试：

1. **使用演示项目**: [MT5Bridge.MT5.Demo](../MT5Bridge.MT5.Demo)
2. **配置连接**: 在 `appsettings.json` 中设置真实凭据
3. **运行场景**: 执行各种命令来测试连接和事件

详见 [MT5Bridge.MT5.Demo/README.md](../MT5Bridge.MT5.Demo/README.md)。

---

### 🛠️ 开发工作流程

#### 添加新测试

1. **创建测试类**:
```csharp
public class MyNewTests
{
    private readonly ITestOutputHelper _output;
    private readonly Serilog.ILogger _logger;

    public MyNewTests(ITestOutputHelper output)
    {
        _output = output;
        _logger = new LoggerConfiguration()
            .WriteTo.TestOutput(output)
            .CreateLogger();
    }

    [Fact]
    public void MyTest_Should_DoSomething()
    {
        // Arrange
        // Act
        // Assert
    }
}
```

2. **运行测试**:
```bash
dotnet test --filter "MyNewTests"
```

3. **更新 README**: 添加测试覆盖率信息

#### 最佳实践

- **遵循 AAA 模式**: Arrange、Act、Assert
- **使用 ITestOutputHelper**: 用于调试输出
- **测试一件事**: 每个测试应验证一个行为
- **有意义的名称**: 测试名称应描述它们验证的内容
- **清理资源**: 对 IDisposable 使用 `using` 语句
- **避免外部依赖**: 保持测试隔离

---

### 📝 测试命名约定

格式: `{方法名}_{场景}_{预期行为}`

示例:
- `RegisterDealHandler_ShouldNotThrow`
- `JsonSerialization_FastMode_ShouldCompactEnums`
- `CreateManager_WithValidLogger_ShouldSucceed`

---

### 🐛 调试测试

#### Visual Studio
1. 打开测试资源管理器（测试 → 测试资源管理器）
2. 右键单击测试 → 调试
3. 在测试或源代码中设置断点

#### VS Code
1. 安装 C# 扩展
2. 设置断点
3. 使用附加调试器运行测试

#### 命令行
```bash
# 运行特定测试并详细输出
dotnet test --filter "FullyQualifiedName~DealHandler" --logger "console;verbosity=detailed"
```

---

### 🤝 贡献

贡献测试时：

1. **遵循约定**: 使用 xUnit 模式和命名
2. **添加文档**: 为复杂测试添加 XML 注释
3. **验证覆盖率**: 运行覆盖率报告
4. **更新 README**: 记录新的测试类
5. **所有测试必须通过**: 提交 PR 之前

---

### 📄 许可证

MIT 许可证 - 详见 [LICENSE](../LICENSE)

---

### 🔗 相关项目

- **[MT5Bridge.Manager](../MT5Bridge.Manager/)** - 被测试的库
- **[MT5Bridge.MT5.Demo](../MT5Bridge.MT5.Demo/)** - 集成测试
- **[MT5Bridge.Core](../MT5Bridge.Core/)** - 核心工具
