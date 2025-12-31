using MetaQuotes.MT5CommonAPI;
using MT5Bridge.Manager.Reconnection;
using Xunit;

namespace MT5Bridge.Manager.Tests;

/// <summary>
/// Error classification tests - verify error classification logic is correct
/// 
/// These tests verify that ErrorAnalyzer can correctly distinguish between:
/// - Transient errors (should retry) - Whitelist
/// - Permanent errors (should not retry) - Everything else
/// 
/// This is critical for the automatic reconnection logic.
/// </summary>
public class ErrorAnalysisTests
{
    /// <summary>
    /// Transient errors should be classified as transient (retryable)
    /// </summary>
    [Theory]
    // Network & Connection
    [InlineData(MTRetCode.MT_RET_ERR_NETWORK)]     // 7
    [InlineData(MTRetCode.MT_RET_ERR_TIMEOUT)]     // 9
    [InlineData(MTRetCode.MT_RET_ERR_CONNECTION)]  // 10
    [InlineData(MTRetCode.MT_RET_ERR_NOSERVICE)]   // 11
    [InlineData(MTRetCode.MT_RET_ERR_SHUTDOWN)]    // 15
    [InlineData(MTRetCode.MT_RET_REQUEST_CONNECTION)] // 10031
    // Load & Frequency
    [InlineData(MTRetCode.MT_RET_ERR_FREQUENT)]    // 12
    [InlineData(MTRetCode.MT_RET_AUTH_SERVER_BUSY)] // 1018
    [InlineData(MTRetCode.MT_RET_REQUEST_TOO_MANY)] // 10024
    [InlineData(MTRetCode.MT_RET_REQUEST_TIMEOUT)]  // 10012
    // Concurrency
    [InlineData(MTRetCode.MT_RET_ERR_DEADLOCK)]    // 12003
    [InlineData(MTRetCode.MT_RET_ERR_LOCKED)]      // 12004
    public void TransientErrors_ShouldBeTransientAndRetryable(MTRetCode errorCode)
    {
        // Act
        var isTransient = ErrorAnalyzer.IsTransientError(errorCode);

        // Assert
        Assert.True(isTransient, $"Error code {errorCode} should be classified as transient (retryable)");
    }

    /// <summary>
    /// Verify error classification: Parameter errors should be permanent errors (no retry)
    /// </summary>
    [Theory]
    [InlineData(MTRetCode.MT_RET_ERR_PARAMS)]      // Parameter error
    [InlineData(MTRetCode.MT_RET_ERROR)]           // General error
    [InlineData(MTRetCode.MT_RET_AUTH_ACCOUNT_INVALID)] // Auth error
    public void PermanentErrors_ShouldNotBeRetried(MTRetCode errorCode)
    {
        // Act
        var isTransient = ErrorAnalyzer.IsTransientError(errorCode);

        // Assert
        Assert.False(isTransient, $"Error code {errorCode} should NOT be classified as transient");
    }

    /// <summary>
    /// Verify: Success code (MT_RET_OK) should not be classified as transient or permanent error
    /// </summary>
    [Fact]
    public void SuccessCode_ShouldNotBeTransientOrPermanent()
    {
        // Act
        var isTransient = ErrorAnalyzer.IsTransientError(MTRetCode.MT_RET_OK);

        // Assert
        Assert.False(isTransient, "MT_RET_OK should not be transient");
    }

    /// <summary>
    /// Verify: Unknown error codes should be handled conservatively (Whitelist Strategy)
    /// 
    /// When encountering unexpected error codes:
    /// - Should NOT be classified as transient (to avoid infinite loops)
    /// - Should be classified as permanent (give up)
    /// </summary>
    [Fact]
    public void UnknownErrorCodes_ShouldNotBeRetried_InWhitelistStrategy()
    {
        // Arrange - use an error code not in the predefined list
        var unknownCode = (MTRetCode)9999; // Undefined error code

        // Act
        var isTransient = ErrorAnalyzer.IsTransientError(unknownCode);

        // Assert - Unknown errors should be treated as permanent in whitelist strategy
        Assert.False(isTransient, "Unknown error codes should NOT be treated as transient");
    }

    /// <summary>
    /// Integration test: Verify reconnection strategy uses error classification correctly
    /// </summary>
    [Fact]
    public void ReconnectStrategy_ShouldUseErrorClassificationCorrectly()
    {
        // Arrange
        var transientError = MTRetCode.MT_RET_ERR_NETWORK;
        var permanentError = MTRetCode.MT_RET_ERR_PARAMS;
        var strategy = new ExponentialBackoffReconnectStrategy();

        // Act & Assert
        Assert.True(strategy.ShouldRetry(transientError), "Strategy should retry transient error");
        Assert.False(strategy.ShouldRetry(permanentError), "Strategy should NOT retry permanent error");
    }
}
