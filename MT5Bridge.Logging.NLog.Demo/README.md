# MT5Bridge.Logging.NLog.Demo

Complete log rolling and archiving test tool for MT5Bridge NLog adapter.

## Features

- ✅ **Configurable Rolling Periods**: Minute, Hour, Day
- ✅ **Automatic Log Archiving**: Old logs automatically archived
- ✅ **Archive Cleanup**: Multiple retention policies (file count, days, size)
- ✅ **Size-Based Archiving**: Archive when file exceeds specified size
- ✅ **Time-Based Retention**: Keep archives for specified number of days
- ✅ **Multi-Level Logging**: Debug, Info, Warn, Error with exceptions
- ✅ **Console + File Output**: Dual output for easy monitoring
- ✅ **Graceful Exit**: Press any key to stop
- ✅ **Real-time Statistics**: Shows elapsed time and message count

## Command Line Options

| Option           | Short | Default           | Description                                         |
| ---------------- | ----- | ----------------- | --------------------------------------------------- |
| `--path`         | `-p`  | `./logs`          | Log directory path                                  |
| `--rolling`      | `-r`  | `day`             | Rolling period: minute, hour, day                   |
| `--interval`     | `-i`  | `1`               | Log output interval in seconds                      |
| `--max-files`    | `-m`  | `7`               | Maximum archive files to keep (0 = unlimited)       |
| `--max-days`     | `-d`  | `0`               | Maximum days to keep archives (0 = unlimited)       |
| `--archive-size` | `-s`  | `0`               | Archive when file exceeds size in MB (0 = disabled) |
| `--compress`     | `-z`  | `false`           | Compress archive files (not supported in NLog 6.0)  |
| `--date-format`  | `-f`  | `yyyy-MM-dd-HHmm` | Archive file date format                            |
| `--console`      | `-c`  | `true`            | Enable console output                               |

## Archive Retention Policies

### How It Works

**File Count Limit (`--max-files`)**:
- Keeps only the N most recent archive files
- Older files are automatically deleted when new archives are created
- Set to `0` for unlimited files

**Time-Based Retention (`--max-days`)**:
- Archives older than N days are automatically deleted
- Based on file's LastWriteTime
- Set to `0` for unlimited retention

**Size-Based Archive (`--archive-size`)**:
- Current log file is archived when it reaches the specified size
- Works in combination with time-based rolling (minute/hour/day)
- Set to `0` to disable size-based archiving

### Deletion Rules

Files are deleted if they meet **ANY** of the following conditions:
- File count > MaxArchiveFiles (when > 0)
- File age > MaxArchiveDays (when > 0)
- File size > ArchiveAboveSize (triggers archiving)

**Logic**: OR relationship - satisfying any condition triggers deletion/archiving.

## Usage Examples

### Quick Test (Minute Rolling)

Test log rolling every minute:

```bash
dotnet run -- -r minute -i 1
```

Run for 3-5 minutes and check the `logs/` directory:

```
logs/
├── mt5bridge-2025-12-15.log          # Current log file
└── archive/
    ├── mt5bridge-2025-12-15-23-30.log
    ├── mt5bridge-2025-12-15-23-31.log
    └── mt5bridge-2025-12-15-23-32.log
```

### Hour Rolling Test

```bash
dotnet run -- --path ./logs --rolling hour --interval 2
```

### Day Rolling (Production Scenario)

```bash
dotnet run -- -p /var/log/mt5bridge -r day -i 5 -m 30
```

### Disable Console Output

```bash
dotnet run -- -r minute -i 1 --console false
```

### Maximum Archive Files Test

Keep only 3 most recent archives:

```bash
dotnet run -- -r minute -i 1 -m 3
```

Run for 10 minutes, verify only 3 archive files kept.

### Archive Retention by Days

Keep archives for 30 days:

```bash
dotnet run -- -r day -i 5 -d 30
```

### Size-Based Archive

Archive when log file exceeds 10MB:

```bash
dotnet run -- -r day -i 1 -s 10
```

### Combined Retention Policies

Keep max 5 files OR 7 days, archive at 5MB:

```bash
dotnet run -- -r hour -i 2 -m 5 -d 7 -s 5
```

**Note**: NLog will apply ALL specified retention policies. Files are deleted if they exceed ANY limit.

### Recommended Configurations

| Scenario           | Command                            | Use Case                                  |
| ------------------ | ---------------------------------- | ----------------------------------------- |
| **Development**    | `-r minute -i 1 -m 5`              | Quick testing with fast rolling           |
| **Production**     | `-r day -i 10 -m 100 -d 30`        | Standard production with 30-day retention |
| **High Frequency** | `-r day -i 0.1 -m 100 -d 14 -s 50` | High-volume logging with 50MB size limit  |
| **Compliance**     | `-r day -i 10 -m 0 -d 90`          | 90-day retention for audit requirements   |
| **Limited Disk**   | `-r hour -i 5 -m 10 -s 50`         | Space-constrained environments            |

## Best Practices

### Performance Considerations

- **MaxArchiveFiles**: Keep below 1000 to avoid slow cleanup
- **ArchiveAboveSize**: Use 50-100MB for optimal performance
- **Cleanup**: Happens during archive creation, not background

### Monitoring

Regularly check:
```powershell
# Archive statistics
$files = Get-ChildItem logs/archive/*.log
Write-Host "Total files: $($files.Count)"
Write-Host "Total size: $(($files | Measure-Object Length -Sum).Sum / 1GB) GB"
Write-Host "Oldest: $(($files | Sort-Object LastWriteTime | Select-Object -First 1).LastWriteTime)"
```

### NLog 6.0 Important Notes

⚠️ **Removed Features**:
- `EnableArchiveFileCompression` no longer available
- Use external tools or NLog.Targets.Zip package for compression

✅ **Supported Features**:
- `ArchiveEvery`, `MaxArchiveFiles`, `MaxArchiveDays`
- `ArchiveAboveSize`, `ArchiveFileName` patterns

## Testing Scenarios

### 1. Basic Rolling Test

**Goal**: Verify log file rolls to archive

```bash
# Start with minute rolling
dotnet run -- -r minute -i 1

# Wait 2-3 minutes
# Check logs/ folder for:
# - Current log file
# - Archive folder with old files
```

### 2. Archive Cleanup Test

**Goal**: Verify old archives are deleted by file count

```bash
# Keep max 2 archives
dotnet run -- -r minute -i 1 -m 2

# Run for 10 minutes
# Verify only 2 archive files in archive/ folder
```

### 3. Time-Based Retention Test

**Goal**: Verify archives deleted after specified days

```bash
# Keep archives for 7 days (for testing, use short intervals)
dotnet run -- -r minute -i 1 -d 7

# Manually adjust file timestamps to test:
# (Get-Item logs/archive/*.log).LastWriteTime = (Get-Date).AddDays(-8)
# Restart demo, old files should be deleted
```

### 4. Size-Based Archive Test

**Goal**: Verify file archives when size limit reached

```bash
# Archive at 1MB (use fast logging to reach limit quickly)
dotnet run -- -r day -i 0.1 -s 1

# Monitor file size in logs/ folder
# When it reaches ~1MB, should roll to archive automatically
```

### 5. Combined Policy Test

**Goal**: Test multiple retention policies together

```bash
# Max 3 files, 7 days retention, archive at 5MB
dotnet run -- -r hour -i 1 -m 3 -d 7 -s 5

# Files deleted if: count > 3 OR age > 7 days OR size > 5MB
```

### 6. Long Running Test

**Goal**: Test stability over extended period

```bash
# Day rolling, 10 second interval
dotnet run -- -r day -i 10

# Let it run overnight
# Check logs next day
```

### 7. High Frequency Test

**Goal**: Test performance with rapid logging

```bash
# Minute rolling, 0.1 second (100ms) interval
dotnet run -- -r minute -i 0.1

# Monitor CPU and disk usage
# Verify no message loss
```

### 8. Exception Logging Test

**Goal**: Verify exceptions logged correctly

```bash
dotnet run -- -r minute -i 1

# Every 20th message logs an exception
# Check log files contain full stack traces
```

## Output Example

```
MT5Bridge NLog Demo - Log Rolling Test Tool
===========================================

Configuration:
  Log directory:    E:\work\source\XSyphonBridge\mt5bridge\MT5Bridge.Logging.NLog.Demo\logs
  Rolling period:   minute
  Output interval:  1 seconds
  Max archive files: 3
  Max archive days:  7
  Archive size limit: 10 MB
  Compress archives: False
  Archive date format: yyyy-MM-dd-HHmm
  Console output:   True

Application started. Press Ctrl+C or any key to exit.
============================================================

23:30:01 | INFO  | [1] Info message at 2025-12-15 23:30:01.123 (elapsed: 00:00:00)
[2025-12-15 23:30:01.123] Message #1 logged (elapsed: 00:00:00)
23:30:02 | DEBUG | [2] Debug message at 2025-12-15 23:30:02.124 (elapsed: 00:00:01)
[2025-12-15 23:30:02.124] Message #2 logged (elapsed: 00:00:01)
...
23:31:00 | INFO  | [60] Info message at 2025-12-15 23:31:00.125 (elapsed: 00:00:59)
[2025-12-15 23:31:00.125] Message #60 logged (elapsed: 00:00:59)

Application exited.
Total messages logged: 60
Log files are in: E:\work\source\XSyphonBridge\mt5bridge\MT5Bridge.Logging.NLog.Demo\logs
```

## Log File Format

```
2025-12-15 23:30:01.1234 | DEBUG | DemoApp | [1] Debug message at 2025-12-15 23:30:01.123 (elapsed: 00:00:00)
2025-12-15 23:30:02.1235 | INFO  | DemoApp | [2] Info message at 2025-12-15 23:30:02.124 (elapsed: 00:00:01)
2025-12-15 23:30:10.1236 | WARN  | DemoApp | [10] Warning message at 2025-12-15 23:30:10.125 (elapsed: 00:00:09)
2025-12-15 23:30:20.1237 | ERROR | DemoApp | [20] Error occurred at 2025-12-15 23:30:20.126 System.InvalidOperationException: Simulated error #1
   at MT5Bridge.Logging.NLog.Demo.Program.StartPeriodicLogging...
```

## Troubleshooting

### Logs not rolling?

**Symptoms**: All messages in one file after time boundary

**Checks**:
- System time advancing correctly?
- File permissions on log directory?
- `ArchiveEvery` property set correctly?

**Solution**:
```bash
# Enable NLog internal logging (add to code)
NLog.Common.InternalLogger.LogToConsole = true;
NLog.Common.InternalLogger.LogLevel = LogLevel.Debug;
```

### Archive files not cleaning up?

**Symptoms**: More than MaxArchiveFiles archives exist

**Checks**:
- Archive directory exists?
- File permissions (write access)?
- Files locked by another process?

**Verification**:
```powershell
# Check file locks
Get-Process | Where-Object { $_.Modules.FileName -like "*logs*" }

# Check permissions
icacls logs\archive
```

### Size-based archiving not working?

**Checks**:
- `ArchiveAboveSize` in bytes (MB * 1024 * 1024)
- Write frequency high enough to reach limit?

**Test**:
```bash
# Fast write test (should reach 1MB quickly)
dotnet run -- -r day -i 0.01 -s 1
```

### Testing time-based retention

**Manual time adjustment**:
```powershell
# Set file to 10 days old
$file = Get-Item logs\archive\mt5bridge-*.log | Select-Object -First 1
$file.LastWriteTime = (Get-Date).AddDays(-10)
# Restart demo - old file should be deleted
```

### High CPU usage?

- Increase interval time
- Reduce log frequency
- Check NLog configuration for async targets

## Integration with MT5Bridge

This demo shows how to integrate MT5Bridge logging abstractions with NLog:

```csharp
// 1. Configure NLog (rolling, targets, etc.)
var config = new LoggingConfiguration();
// ... configure targets

// 2. Use MT5Bridge abstraction
var factory = new NLogLoggerFactory();
var logger = factory.CreateLogger("MyApp");

// 3. Log as usual
logger.Debug("Debug message");
logger.Info("Info message");
logger.Warn("Warning message");
logger.Error("Error message", exception);
```

## Verification Methods

### View Configuration
```bash
# All settings displayed at startup
dotnet run -- -m 3 -d 7 -s 10
```

### Check Archive Files
```bash
# List archives
ls logs/archive/

# Count and size
Get-ChildItem logs/archive/*.log | Measure-Object Length -Sum

# Find oldest
Get-ChildItem logs/archive/*.log | Sort-Object LastWriteTime | Select-Object -First 1
```

### Monitor Current Log Size
```powershell
while ($true) {
    $size = (Get-Item logs/*.log).Length / 1MB
    Write-Host "Current size: $([math]::Round($size, 2)) MB"
    Start-Sleep -Seconds 5
}
```

## Related Documentation

- [NLog Configuration](https://nlog-project.org/config/)
- [MT5Bridge.Core Documentation](../README.md)
- [Unit Tests](../MT5Bridge.Logging.NLog.Tests/)
