using Serilog;
using MetaQuotes.MT5CommonAPI;

namespace MT5Bridge.Manager.Reconnection;

/// <summary>
/// Manages automatic reconnection logic
/// </summary>
internal class ReconnectExecutor : IDisposable
{
    private readonly ILogger _logger;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _reconnectTask;
    private IReconnectStrategy? _strategy;
    private int _attemptNumber = 0;
    private MTRetCode _lastError = MTRetCode.MT_RET_OK;
    private bool _disposed;

    public ReconnectExecutor(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Start auto-reconnect loop
    /// </summary>
    /// <param name="strategy">Reconnection strategy</param>
    /// <param name="reconnectFunc">Async function to perform reconnection</param>
    /// <param name="initialError">Initial error code from failed connection (for logging)</param>
    /// <returns>Task that completes when reconnection succeeds or strategy decides to stop</returns>
    public Task StartReconnectAsync(
        IReconnectStrategy strategy,
        Func<CancellationToken, Task<MT5Result>> reconnectFunc,
        MTRetCode initialError = MTRetCode.MT_RET_OK)
    {
        if (strategy == null)
            throw new ArgumentNullException(nameof(strategy));
        if (reconnectFunc == null)
            throw new ArgumentNullException(nameof(reconnectFunc));

        // Stop any existing reconnect attempt
        StopReconnect();

        _strategy = strategy;
        _attemptNumber = 0;
        _lastError = initialError;
        _cancellationTokenSource = new CancellationTokenSource();

        _reconnectTask = RunReconnectLoopAsync(reconnectFunc, _cancellationTokenSource.Token);
        return _reconnectTask;
    }

    /// <summary>
    /// Stop auto-reconnect
    /// </summary>
    public void StopReconnect()
    {
        _cancellationTokenSource?.Cancel();

        try
        {
            _reconnectTask?.Wait(TimeSpan.FromSeconds(5));
        }
        catch (OperationCanceledException)
        {
            // Expected when cancelling
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Error waiting for reconnect task to stop");
        }

        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
        _reconnectTask = null;
    }

    /// <summary>
    /// Main reconnection loop
    /// </summary>
    private async Task RunReconnectLoopAsync(
        Func<CancellationToken, Task<MT5Result>> reconnectFunc,
        CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_strategy == null)
                    break;

                // Check if strategy decides to stop based on last error
                if (!_strategy.ShouldRetry(_lastError))
                {
                    _logger.Warning(
                        "Reconnect strategy decided to stop (attempt={Attempt}, error={Error})",
                        _attemptNumber, _lastError);
                    break;
                }

                _attemptNumber++;

                // Get next delay from strategy
                int delayMs = _strategy.GetNextDelayMs(_attemptNumber, _lastError);

                // Negative delay means stop
                if (delayMs < 0)
                {
                    _logger.Warning("Strategy indicates permanent failure (error={Error})", _lastError);
                    break;
                }

                if (delayMs > 0)
                {
                    _logger.Information(
                        "Auto-reconnecting (attempt={Attempt}, delay={DelayMs}ms, lastError={Error})",
                        _attemptNumber, delayMs, _lastError);

                    try
                    {
                        await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                }
                else
                {
                    _logger.Information(
                        "Auto-reconnecting immediately (attempt={Attempt}, lastError={Error})",
                        _attemptNumber, _lastError);
                }

                // Attempt to reconnect
                if (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var result = await reconnectFunc(cancellationToken).ConfigureAwait(false);

                        if (result.IsSuccess)
                        {
                            _logger.Information(
                                "Auto-reconnected successfully after {Attempts} attempt(s)",
                                _attemptNumber);
                            _strategy.Reset();
                            break; // Success - exit loop
                        }

                        // FIXED H3: Track error code from result
                        _lastError = result.RetCode;
                        _logger.Warning(
                            "Reconnection attempt {Attempt} failed: {Error} - {Message}",
                            _attemptNumber, _lastError, result.Message);
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Error during reconnection attempt {Attempt}", _attemptNumber);
                    }
                }

                // Brief pause before checking again
                await Task.Delay(100, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.Debug("Reconnect loop cancelled");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unexpected error in reconnect loop");
        }
    }

    /// <summary>
    /// Update last error for next decision
    /// </summary>
    public void UpdateLastError(MTRetCode errorCode)
    {
        _lastError = errorCode;
    }

    /// <summary>
    /// Check if reconnecting
    /// </summary>
    public bool IsReconnecting => _reconnectTask != null && !_reconnectTask.IsCompleted;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        StopReconnect();
        _cancellationTokenSource?.Dispose();
    }
}
