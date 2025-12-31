using MetaQuotes.MT5CommonAPI;

namespace MT5Bridge.Manager.Reconnection;

/// <summary>
/// Error analyzer for reconnection decisions
/// </summary>
public static class ErrorAnalyzer
{
    /// <summary>
    /// Check if error is transient (should retry) - Whitelist Strategy
    /// </summary>
    public static bool IsTransientError(MTRetCode errorCode)
    {
        return errorCode switch
        {
            // Network & Connection
            MTRetCode.MT_RET_ERR_NETWORK => true,           // 7
            MTRetCode.MT_RET_ERR_TIMEOUT => true,           // 9
            MTRetCode.MT_RET_ERR_CONNECTION => true,        // 10
            MTRetCode.MT_RET_ERR_NOSERVICE => true,         // 11
            MTRetCode.MT_RET_ERR_SHUTDOWN => true,          // 15
            MTRetCode.MT_RET_REQUEST_CONNECTION => true,    // 10031

            // Load & Frequency
            MTRetCode.MT_RET_ERR_FREQUENT => true,          // 12
            MTRetCode.MT_RET_AUTH_SERVER_BUSY => true,      // 1018
            MTRetCode.MT_RET_REQUEST_TOO_MANY => true,      // 10024
            MTRetCode.MT_RET_REQUEST_TIMEOUT => true,       // 10012

            // Concurrency
            MTRetCode.MT_RET_ERR_DEADLOCK => true,          // 12003
            MTRetCode.MT_RET_ERR_LOCKED => true,            // 12004

            _ => false
        };
    }
}
