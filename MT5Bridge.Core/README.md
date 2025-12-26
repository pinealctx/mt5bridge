# MT5Bridge.Core

[English](#english) | [中文](#chinese)

---

<a name="english"></a>
## English Documentation

MT5Bridge.Core is the foundational library of the MT5Bridge project, providing high-performance utilities and atomic operations.

### Overview
This library is designed to be lightweight and efficient, focusing on core utilities needed by other MT5Bridge components.

### Core Features

#### 1. Atomic Operations
Thread-safe atomic types for high-concurrency scenarios:
- AtomicBool, AtomicInt, AtomicLong, AtomicDouble
- AtomicEnum, AtomicRef

#### 2. Timing Utilities
High-precision timing and backoff helpers:
- BackoffTimer for exponential backoff strategies
- TimeX for timestamp conversions

#### 3. Text Utilities
Optimized string extensions and text processing helpers.

#### 4. Collections
Dictionary utilities and optimized collection helpers.

---

<a name="chinese"></a>
## 中文文档

MT5Bridge.Core 是 MT5Bridge 项目的基础库，提供高性能工具和原子操作。

### 概述
该库旨在轻量且高效，专注于其他 MT5Bridge 组件所需的核心工具。

### 核心特性

#### 1. 原子操作
适用于高并发场景的线程安全原子类型：
- AtomicBool, AtomicInt, AtomicLong, AtomicDouble
- AtomicEnum, AtomicRef

#### 2. 时序工具
高精度时序和退避助手：
- BackoffTimer 用于指数退避策略
- TimeX 用于时间戳转换

#### 3. 文本工具
优化的字符串扩展和文本处理助手。

#### 4. 集合
字典工具和优化的集合助手。