# MT5Bridge Benchmarks

[English](#english) | [中文](#chinese)

---

<a name="english"></a>
## English Documentation

Performance benchmarks for the MT5Bridge project.

### Overview
This project uses [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet) to measure the performance of critical components, specifically focusing on serialization efficiency.

### Benchmarks

#### Serialization Benchmarks
Compares different JSON serialization approaches for MT5 models:
- **Fast JSON**: Compact format using source generation, optimized for performance.
- **Readable JSON**: Formatted JSON for better readability.
- **ToString()**: Baseline string representation.

### Running Benchmarks

To run the benchmarks, use the following command from the root directory:

`ash
dotnet run -c Release --project MT5Bridge.Benchmarks
`

### Results (Typical)

| Method                     | Mean    | Allocated |
| :------------------------- | :------ | :-------- |
| ToString_Serialization     | ~500 ns | 800 B     |
| FastJson_Serialization     | ~300 ns | 500 B     |
| ReadableJson_Serialization | ~400 ns | 900 B     |

---

<a name="chinese"></a>
## 中文文档

MT5Bridge 项目的性能基准测试。

### 概述
本项目使用 [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet) 来衡量关键组件的性能，目前主要关注序列化效率。

### 基准测试内容

#### 序列化基准测试
比较 MT5 模型的不同 JSON 序列化方法：
- **快速 JSON (Fast JSON)**：使用源代码生成的紧凑格式，针对性能进行了优化。
- **可读 JSON (Readable JSON)**：格式化的 JSON，便于阅读。
- **ToString()**：基准字符串表示。

### 运行基准测试

在根目录下运行以下命令：

`ash
dotnet run -c Release --project MT5Bridge.Benchmarks
`

### 典型结果

| 方法                       | 平均耗时 | 内存分配 |
| :------------------------- | :------- | :------- |
| ToString_Serialization     | ~500 ns  | 800 B    |
| FastJson_Serialization     | ~300 ns  | 500 B    |
| ReadableJson_Serialization | ~400 ns  | 900 B    |

---
