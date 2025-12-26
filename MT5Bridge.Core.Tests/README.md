# MT5Bridge.Core Tests

[English](#english) | [中文](#chinese)

---

<a name="english"></a>
## English Documentation

Comprehensive test suite for MT5Bridge.Core utilities.

### Overview
This test suite provides complete coverage of all public APIs in MT5Bridge.Core, including:
- **Atomic Operations**: Thread-safe operations for Bool, Int, Long, Double, Enum, and Ref.
- **Text Utilities**: String extensions and high-performance text processing.
- **Collections**: Dictionary utilities and optimized collection helpers.
- **Timing Utilities**: Backoff timers and high-precision timing helpers.

### Running Tests

#### All Tests
`ash
dotnet test MT5Bridge.Core.Tests
`

#### Specific Module
`ash
dotnet test MT5Bridge.Core.Tests --filter "FullyQualifiedName~Atomic"
`

### Test Organization

- **Atomic Operations**: Tests for thread-safe atomic types in the MT5Bridge.Core.Atomic namespace.
- **Text Utilities**: Tests for string extensions and text processing in the MT5Bridge.Core.Text namespace.
- **Collections**: Tests for dictionary and collection utilities in the MT5Bridge.Core.Collections namespace.
- **Timing**: Tests for timing and backoff utilities in the MT5Bridge.Core.Timing namespace.

---

<a name="chinese"></a>
## 中文文档

MT5Bridge.Core 工具库的全面测试套件。

### 概述
该测试套件提供了对 MT5Bridge.Core 中所有公共 API 的完整覆盖，包括：
- **原子操作**：Bool, Int, Long, Double, Enum 和 Ref 的线程安全操作。
- **文本工具**：字符串扩展和高性能文本处理。
- **集合**：字典工具和优化的集合助手。
- **时序工具**：退避定时器和高精度时序助手。

### 运行测试

#### 运行所有测试
`ash
dotnet test MT5Bridge.Core.Tests
`

#### 运行特定模块
`ash
dotnet test MT5Bridge.Core.Tests --filter "FullyQualifiedName~Atomic"
`

### 测试组织

- **原子操作**：针对 MT5Bridge.Core.Atomic 命名空间中线程安全原子类型的测试。
- **文本工具**：针对 MT5Bridge.Core.Text 命名空间中字符串扩展和文本处理的测试。
- **集合**：针对 MT5Bridge.Core.Collections 命名空间中字典和集合工具的测试。
- **时序**：针对 MT5Bridge.Core.Timing 命名空间中时序和退避工具的测试。
