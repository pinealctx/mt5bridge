namespace MT5Bridge.Logging.CloudWatch;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Amazon;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using global::NLog;
using global::NLog.Common;
using global::NLog.Layouts;
using global::NLog.Targets;

/// <summary>
/// NLog Target for AWS CloudWatch Logs
/// Sends logs to AWS CloudWatch
/// </summary>
[Target("AWSCloudWatch")]
public class AwsCloudWatchTarget : Target
{
    private IAmazonCloudWatchLogs? _cloudWatchClient;
    private readonly BlockingCollection<LogEventInfo> _logQueue = new(new ConcurrentQueue<LogEventInfo>(), 50000);
    private int _maxBatchSize = 100;
    private TimeSpan _batchTimeout = TimeSpan.FromSeconds(5);
    private readonly object _sequenceTokenLock = new();
    private string? _nextSequenceToken;
    private Thread? _processingThread;
    private volatile bool _isDisposing;
    private volatile bool _logQueueDropped;
    private volatile bool _logStreamExists;
    private readonly ManualResetEventSlim _wakeUpEvent = new(false);

    // Log throttling: avoid frequent printing of same errors
    private readonly ConcurrentDictionary<string, long> _logThrottleMap = new();
    private const long ThrottleIntervalTicks = 5 * TimeSpan.TicksPerMinute; // 5 minutes

    public string LogGroup { get; set; } = string.Empty;
    public string LogStream { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string AccessKeyId { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;

    private Layout? _layout;
    public Layout? Layout
    {
        get => _layout;
        set => _layout = value;
    }

    public int LogBatchSize
    {
        get => _maxBatchSize;
        set => _maxBatchSize = Math.Max(1, Math.Min(value, 10000));
    }

    public int BatchTimeoutMs
    {
        get => (int)_batchTimeout.TotalMilliseconds;
        set => _batchTimeout = TimeSpan.FromMilliseconds(Math.Max(100, value));
    }

    protected override void InitializeTarget()
    {
        base.InitializeTarget();

        try
        {
            _cloudWatchClient = new AmazonCloudWatchLogsClient(
                AccessKeyId,
                SecretKey,
                RegionEndpoint.GetBySystemName(Region));

            _isDisposing = false;
            _processingThread = new Thread(ProcessLogQueue)
            {
                IsBackground = true,
                Name = "CloudWatchLogProcessor"
            };
            _processingThread.Start();
        }
        catch (Exception ex)
        {
            InternalLogger.Error(ex, "Failed to initialize AWS CloudWatch client");
        }
    }

    protected override void CloseTarget()
    {
        _isDisposing = true;
        _wakeUpEvent.Set();
        _logQueue.CompleteAdding();

        try
        {
            _processingThread?.Join(TimeSpan.FromSeconds(3));
        }
        catch (Exception ex)
        {
            InternalLogger.Warn(ex, "Error waiting for processing thread to complete");
        }

        _wakeUpEvent.Dispose();
        _cloudWatchClient?.Dispose();
        base.CloseTarget();
    }

    protected override void Write(LogEventInfo logEvent)
    {
        try
        {
            if (!_isDisposing && !_logQueue.TryAdd(logEvent))
            {
                _logQueueDropped = true;
                if (ShouldLogThrottled("AWSCloudWatchTarget_DropLog"))
                {
                    InternalLogger.Warn("CloudWatch log queue is full, dropping log events");
                }
            }
        }
        catch (Exception exception)
        {
            if (ShouldLogThrottled("Write_exception"))
            {
                InternalLogger.Warn(exception, "Write exception");
            }
        }
    }

    private void ProcessLogQueue()
    {
        var batch = new List<LogEventInfo>();
        var lastBatchTime = DateTime.UtcNow;

        while (!_isDisposing && !_logQueue.IsCompleted)
        {
            try
            {
                // Try to get log from queue
                if (_logQueue.TryTake(out var logEvent, 1000))
                {
                    batch.Add(logEvent);
                }

                // Determine if batch should be sent
                var shouldSend = batch.Count > 0 &&
                                (batch.Count >= _maxBatchSize ||
                                 (DateTime.UtcNow - lastBatchTime) >= _batchTimeout);

                if (shouldSend)
                {
                    SendBatchToCloudWatch(batch);
                    batch.Clear();
                    lastBatchTime = DateTime.UtcNow;
                }
            }
            catch (Exception ex)
            {
                if (ShouldLogThrottled("ProcessLogQueue_exception"))
                {
                    InternalLogger.Error(ex, "Error processing log queue");
                }
            }
        }

        // Process remaining logs
        if (batch.Count > 0)
        {
            try
            {
                SendBatchToCloudWatch(batch);
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "Error sending final batch to CloudWatch");
            }
        }
    }

    private void SendBatchToCloudWatch(List<LogEventInfo> batch)
    {
        if (_cloudWatchClient == null || batch.Count == 0)
        {
            return;
        }

        const int maxRetries = 3;
        var attempt = 0;

        while (attempt < maxRetries)
        {
            try
            {
                EnsureLogStreamExists();

                var logEvents = batch
                    .Select(logEvent => new InputLogEvent
                    {
                        Timestamp = logEvent.TimeStamp.ToUniversalTime(),
                        Message = RenderLogEvent(Layout, logEvent)
                    })
                    .OrderBy(e => e.Timestamp)
                    .ToList();

                var request = new PutLogEventsRequest
                {
                    LogGroupName = LogGroup,
                    LogStreamName = LogStream,
                    LogEvents = logEvents
                };

                lock (_sequenceTokenLock)
                {
                    if (!string.IsNullOrEmpty(_nextSequenceToken))
                    {
                        request.SequenceToken = _nextSequenceToken;
                    }

                    var response = _cloudWatchClient.PutLogEventsAsync(request).GetAwaiter().GetResult();
                    _nextSequenceToken = response.NextSequenceToken;
                }

                // 成功发送
                if (_logQueueDropped)
                {
                    _logQueueDropped = false;
                    InternalLogger.Info("CloudWatch log queue recovered");
                }

                return;
            }
            catch (InvalidSequenceTokenException ex)
            {
                // Token 失效，更新后重试
                lock (_sequenceTokenLock)
                {
                    _nextSequenceToken = ex.ExpectedSequenceToken;
                }
                attempt++;
            }
            catch (ResourceNotFoundException)
            {
                // Log stream 不存在，重新创建
                _logStreamExists = false;
                attempt++;
            }
            catch (Exception ex)
            {
                if (ShouldLogThrottled("SendBatchToCloudWatch_exception"))
                {
                    InternalLogger.Warn(ex, $"Failed to send logs to CloudWatch, attempt {attempt}");
                }
                attempt++;
                Thread.Sleep(TimeSpan.FromSeconds(Math.Pow(2, attempt))); // 指数退避
            }
        }
    }

    private void EnsureLogStreamExists()
    {
        if (_logStreamExists || _cloudWatchClient == null)
        {
            return;
        }

        try
        {
            // 检查 log stream 是否存在
            var describeRequest = new DescribeLogStreamsRequest
            {
                LogGroupName = LogGroup,
                LogStreamNamePrefix = LogStream
            };

            var describeResponse = _cloudWatchClient.DescribeLogStreamsAsync(describeRequest)
                .GetAwaiter().GetResult();

            var existingStream = describeResponse.LogStreams
                .FirstOrDefault(s => s.LogStreamName == LogStream);

            if (existingStream != null)
            {
                lock (_sequenceTokenLock)
                {
                    _nextSequenceToken = existingStream.UploadSequenceToken;
                }
                _logStreamExists = true;
                return;
            }

            // 创建 log stream
            var createRequest = new CreateLogStreamRequest
            {
                LogGroupName = LogGroup,
                LogStreamName = LogStream
            };

            _cloudWatchClient.CreateLogStreamAsync(createRequest).GetAwaiter().GetResult();

            lock (_sequenceTokenLock)
            {
                _nextSequenceToken = null;
            }

            _logStreamExists = true;
            InternalLogger.Info($"Created CloudWatch log stream: {LogStream}");
        }
        catch (Exception e)
        {
            if (ShouldLogThrottled("EnsureLogStreamExists_exception"))
            {
                InternalLogger.Warn(e, "Failed to ensure CloudWatch log stream exists");
            }
        }
    }

    /// <summary>
    /// Log throttling: avoid frequent printing of same errors
    /// </summary>
    private bool ShouldLogThrottled(string key)
    {
        var now = DateTime.UtcNow.Ticks;

        if (_logThrottleMap.TryGetValue(key, out var lastLogTime))
        {
            if (now - lastLogTime < ThrottleIntervalTicks)
            {
                return false; // Throttling in effect
            }
        }

        _logThrottleMap[key] = now;
        return true;
    }
}
