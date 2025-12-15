# MT5Bridge

A collection of lightweight, high-performance utility libraries for .NET applications.

## Packages

### MT5Bridge.Core

Core utilities library with zero external dependencies.

**Features:**
- **Atomic Operations** - Thread-safe atomic types (AtomicBool, AtomicInt, AtomicLong, etc.)
- **Timing Utilities** - BackoffTimer, Unix timestamp conversions
- **Text Utilities** - String parsing and manipulation helpers
- **Collection Utilities** - Dictionary extensions
- **Logging Abstractions** - Simple, dependency-free logging interfaces

**Installation:**
```bash
dotnet add package MT5Bridge.Core
```

### MT5Bridge.Logging.NLog

NLog adapter for MT5Bridge.Core logging abstractions.

**Installation:**
```bash
dotnet add package MT5Bridge.Logging.NLog
```

## Quick Start

### Using Core Utilities

```csharp
using MT5Bridge.Core.Atomic;
using MT5Bridge.Core.Timing;
using MT5Bridge.Core.Text;

// Atomic operations
var counter = new AtomicLong(0);
counter.Increment();

// Backoff timer
var timer = new BackoffTimer(100, 5000, 2.0);
if (timer.CanExecute())
{
    // Execute your code
}

// Time utilities
long unixMillis = TimeX.CurrentUnixTimeMillis();
DateTime dt = TimeX.FromUnixTimeMillis(unixMillis);

// String utilities
var (part1, part2, success) = StringX.SplitBy("key:value", ":");
```

### Using Logging

```csharp
using MT5Bridge.Core.Logging;
using MT5Bridge.Logging.NLog;

// Create logger with NLog
ILoggerFactory factory = new NLogLoggerFactory();
ILogger logger = factory.CreateLogger("MyApp");

logger.Info("Application started");
logger.Warn("This is a warning");
logger.Error("An error occurred");
```

## Documentation

- [Usage Examples](USAGE_EXAMPLES.md) - Complete usage guide with examples
- [NuGet Guide](NUGET_GUIDE.md) - How to build and publish packages

## Building from Source

```bash
git clone https://github.com/pinealctx/mt5bridge.git
cd mt5bridge
dotnet build
```

## Creating NuGet Packages

```bash
dotnet pack -c Release
```

Packages will be generated in `bin/Release/` directory.

## License

MIT License

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
