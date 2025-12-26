# MT5Bridge

[English](#english) | [中文](#chinese)

---

<a name="english"></a>

## English Documentation

**MT5Bridge** is a high-performance, enterprise-grade .NET 8.0 toolkit for MetaTrader 5 (MT5) Server management. It provides a robust infrastructure for building trading systems, risk management tools, and server-side automation with a focus on performance, thread-safety, and reliability.

**Status**: ✅ Production Ready | **Quality**: ⭐⭐⭐⭐⭐ (5/5) | **Framework**: .NET 8.0

### 🚀 Solution Overview

The solution is organized into several specialized modules:

#### 🏗️ Core Infrastructure (`MT5Bridge.Core`)
- **High-Performance Atomics**: Thread-safe `AtomicBool`, `AtomicInt`, `AtomicLong`, `AtomicDouble`, and `AtomicRef` for lock-free concurrency.
- **Timing Utilities**: `BackoffTimer` for intelligent retry logic and `TimeX` for high-precision timestamping.
- **Utility Extensions**: Optimized collection utilities (`DictUtil`) and string extensions (`StringX`).

#### 🔌 MT5 Management (`MT5Bridge.Manager`)
- **Manager API Wrapper**: Clean, async-first wrapper for the MT5 Server Manager API.
- **State Management**: Robust connection state tracking and automatic reconnection logic.
- **Data Models**: Comprehensive POCO and Protobuf models for Deals, Orders, Users, and Accounts.
- **Event System**: High-performance event dispatching for real-time server events.

#### 📝 Advanced Logging (`MT5Bridge.Serilog`)
- **Zero-Reflection Logging**: High-performance Serilog integration using .NET 8 Source Generators.
- **Multi-Sink Support**: Console (ANSI), Rolling File, and AWS CloudWatch integration.
- **Lazy Evaluation**: Extreme performance optimization for disabled log levels.

#### 📊 Performance & Testing
- **MT5Bridge.Benchmarks**: Comprehensive BenchmarkDotNet suite for performance verification.
- **MT5Bridge.Tests**: Extensive unit test coverage for all core components.

---

### 🛠️ Quick Start

#### Prerequisites
- .NET 8.0 SDK
- Windows x64 (Required for MT5 Manager API native dependencies)
- MT5 Server Manager API credentials

#### Build the Solution
```powershell
# Full rebuild
dotnet build /t:Rebuild
```

#### Run Tests
```powershell
# Run all tests using the provided runner
.\run-tests.ps1 -Mode all
```

---

### 📖 Project Structure

| Project                | Description                                       |
| :--------------------- | :------------------------------------------------ |
| `MT5Bridge.Core`       | Fundamental utilities and thread-safe types.      |
| `MT5Bridge.Manager`    | MT5 Server API integration and domain models.     |
| `MT5Bridge.Serilog`    | High-performance logging infrastructure.          |
| `MT5Bridge.Benchmarks` | Performance measurement and optimization tools.   |
| `MT5Bridge.*.Demo`     | Example applications demonstrating library usage. |
| `MT5Bridge.*.Tests`    | Unit and integration test suites.                 |

---

<a name="chinese"></a>

## 中文文档

**MT5Bridge** 是一个面向企业级应用的高性能 .NET 8.0 MetaTrader 5 (MT5) 服务端管理工具集。它为构建交易系统、风控工具和服务器端自动化提供了坚实的基础设施，重点关注性能、线程安全和可靠性。

**状态**: ✅ 生产就绪 | **质量**: ⭐⭐⭐⭐⭐ (5/5) | **框架**: .NET 8.0

### 🚀 方案概览

本解决方案由多个专业模块组成：

#### 🏗️ 核心基础设施 (`MT5Bridge.Core`)
- **高性能原子类型**: 线程安全的 `AtomicBool`, `AtomicInt`, `AtomicLong`, `AtomicDouble` 和 `AtomicRef`，用于无锁并发编程。
- **定时工具**: 用于智能重试逻辑的 `BackoffTimer` 和用于高精度时间戳的 `TimeX`。
- **实用扩展**: 优化的集合工具 (`DictUtil`) 和字符串扩展 (`StringX`)。

#### 🔌 MT5 管理核心 (`MT5Bridge.Manager`)
- **Manager API 封装**: 为 MT5 Server Manager API 提供简洁、异步优先的封装。
- **状态管理**: 可靠的连接状态追踪和自动重连逻辑。
- **数据模型**: 针对成交 (Deals)、订单 (Orders)、用户 (Users) 和账户 (Accounts) 的完整 POCO 和 Protobuf 模型。
- **事件系统**: 针对实时服务器事件的高性能事件分发机制。

#### 📝 高级日志系统 (`MT5Bridge.Serilog`)
- **零反射日志**: 使用 .NET 8 源码生成器 (Source Generators) 实现的高性能 Serilog 集成。
- **多输出支持**: 集成控制台 (ANSI 彩色)、滚动文件和 AWS CloudWatch。
- **延迟计算**: 针对未启用日志级别的极致性能优化。

#### 📊 性能与测试
- **MT5Bridge.Benchmarks**: 使用 BenchmarkDotNet 构建的完整性能基准测试套件。
- **MT5Bridge.Tests**: 覆盖所有核心组件的详尽单元测试。

---

### 🛠️ 快速入门

#### 环境要求
- .NET 8.0 SDK
- Windows x64 (MT5 Manager API 原生依赖项要求)
- MT5 Server Manager API 访问权限

#### 构建项目
```powershell
# 完整重新构建
dotnet build /t:Rebuild
```

#### 运行测试
```powershell
# 使用提供的运行脚本执行所有测试
.\run-tests.ps1 -Mode all
```

---

### 📖 项目结构

| 项目                   | 描述                            |
| :--------------------- | :------------------------------ |
| `MT5Bridge.Core`       | 基础工具类和线程安全类型。      |
| `MT5Bridge.Manager`    | MT5 服务器 API 集成和领域模型。 |
| `MT5Bridge.Serilog`    | 高性能日志基础设施。            |
| `MT5Bridge.Benchmarks` | 性能测量和优化工具。            |
| `MT5Bridge.*.Demo`     | 演示库用法的示例程序。          |
| `MT5Bridge.*.Tests`    | 单元测试和集成测试套件。        |

---
