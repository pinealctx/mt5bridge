using Serilog.Events;

namespace MT5Bridge.Serilog;

public class SerilogConfig
{
    public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Information;
    public ConsoleConfig Console { get; set; } = new();
    public FileConfig File { get; set; } = new();
    public CloudWatchConfig CloudWatch { get; set; } = new();
    public DiagnosticsConfig Diagnostics { get; set; } = new();

    public class DiagnosticsConfig
    {
        public DiagnosticConsoleConfig Console { get; set; } = new();
        public DiagnosticFileConfig File { get; set; } = new();

        private int _throttleWindowSeconds = 300;
        private int _throttleLimit = 100;

        /// <summary>
        /// Throttle window in seconds for Serilog internal error messages.
        /// Default: 300 seconds (5 minutes). Must be positive.
        /// </summary>
        public int ThrottleWindowSeconds
        {
            get => _throttleWindowSeconds;
            set => _throttleWindowSeconds = value > 0 ? value : 300;
        }

        /// <summary>
        /// Maximum number of Serilog internal error messages allowed within the throttle window.
        /// Default: 100 messages. Must be non-negative (0 = no internal logging).
        /// </summary>
        public int ThrottleLimit
        {
            get => _throttleLimit;
            set => _throttleLimit = value >= 0 ? value : 100;
        }
    }

    public class DiagnosticConsoleConfig
    {
        public bool Enabled { get; set; } = false;
    }

    public class DiagnosticFileConfig
    {
        public bool Enabled { get; set; } = false;
        public string Path { get; set; } = "logs/serilog-internal.log";
    }

    public class ConsoleConfig
    {
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Minimum log level for Console sink. If null, uses global MinimumLevel.
        /// This allows Console to have a different log level than File or CloudWatch.
        /// Example: Set to Information to reduce console noise while File captures Debug.
        /// </summary>
        public LogEventLevel? MinimumLevel { get; set; } = null;

        /// <summary>
        /// Text formatter for console output: "plain", "json", "compact", "rendered-compact".
        /// - plain (or empty): Plain text with timestamp, level, message (default)
        /// - json: Standard Serilog JSON formatter
        /// - compact: CompactJsonFormatter (smaller, faster)
        /// - rendered-compact: RenderedCompactJsonFormatter (includes rendered message)
        /// Default: "plain" (empty string)
        /// </summary>
        public string TextFormatter { get; set; } = string.Empty;

        /// <summary>
        /// Use ANSI color codes in console output. Set to false for non-TTY environments.
        /// Default: true (colors enabled).
        /// </summary>
        public bool UseAnsiColors { get; set; } = true;
    }

    public class FileConfig
    {
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Minimum log level for File sink. If null, uses global MinimumLevel.
        /// This allows File to have a different log level than Console or CloudWatch.
        /// Example: Set to Debug for detailed troubleshooting while Console shows Information.
        /// </summary>
        public LogEventLevel? MinimumLevel { get; set; } = null;

        /// <summary>
        /// Text formatter for file output: "plain", "json", "compact", "rendered-compact".
        /// - plain (or empty): Plain text with timestamp, level, message (default)
        /// - json: Standard Serilog JSON formatter
        /// - compact: CompactJsonFormatter (smaller, faster)
        /// - rendered-compact: RenderedCompactJsonFormatter (includes rendered message)
        /// Default: "plain" (empty string)
        /// </summary>
        public string TextFormatter { get; set; } = string.Empty;

        public string Path { get; set; } = "logs/log-.txt";
        public string RollingInterval { get; set; } = "Day"; // Day, Hour, etc.
        public long? FileSizeLimitBytes { get; set; } = 10 * 1024 * 1024; // 10MB
        public int? RetainedFileCountLimit { get; set; } = 31;
    }

    public class CloudWatchConfig
    {
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Minimum log level for CloudWatch sink. If null, uses global MinimumLevel.
        /// This allows CloudWatch to have a different log level than Console or File.
        /// Example: Set to Warning to reduce AWS costs while File captures Debug.
        /// </summary>
        public LogEventLevel? MinimumLevel { get; set; } = null;

        /// <summary>
        /// Text formatter: "json" (standard), "compact" (recommended), "rendered-compact".
        /// - json: Standard Serilog JSON formatter (292 bytes, baseline performance)
        /// - compact: CompactJsonFormatter (187 bytes, 36% smaller, 47% faster) ⭐
        /// - rendered-compact: RenderedCompactJsonFormatter (includes rendered message)
        /// Default: "compact"
        /// </summary>
        public string TextFormatter { get; set; } = "compact";


        public string LogGroup { get; set; } = "";

        /// <summary>
        /// Whether to create the log group if it doesn't exist.
        /// Default: false. Set to true if you want the library to automatically create the log group.
        /// Note: Setting this to true requires logs:CreateLogGroup and logs:DescribeLogGroups permissions.
        /// </summary>
        public bool CreateLogGroup { get; set; } = false;

        public string LogStreamPrefix { get; set; } = "";
        public string Region { get; set; } = "";

        /// <summary>
        /// AWS Access Key ID. If specified along with SecretKey, uses explicit credentials.
        /// If both are empty, uses default AWS credential chain (profile, environment, IAM role).
        /// </summary>
        public string AccessKeyId { get; set; } = string.Empty;

        /// <summary>
        /// AWS Secret Access Key. Required if AccessKeyId is specified.
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>
        /// Maximum number of log events to batch before sending to CloudWatch.
        /// Default: 100. Higher values reduce API calls but increase latency.
        /// </summary>
        public int BatchSizeLimit { get; set; } = 100;

        /// <summary>
        /// Time interval in seconds to flush logs to CloudWatch.
        /// Default: 5. Higher values batch more logs but increase latency.
        /// </summary>
        public int PeriodSeconds { get; set; } = 5;

        /// <summary>
        /// Log stream naming strategy: "default", "constant", "configurable".
        /// - default: {DateTime}_{HostName}_{Guid}
        /// - constant: {LogStreamPrefix}_{Guid}
        /// - configurable: {LogStreamPrefix}/[hostname]/[guid] (based on Include flags)
        /// Default: "configurable"
        /// </summary>
        public string LogStreamNamingStrategy { get; set; } = "configurable";

        /// <summary>
        /// Include hostname in log stream name (for "configurable" strategy).
        /// Default: true
        /// </summary>
        public bool LogStreamIncludeHostname { get; set; } = true;

        /// <summary>
        /// Include GUID in log stream name (for "configurable" strategy).
        /// Default: true
        /// </summary>
        public bool LogStreamIncludeGuid { get; set; } = true;

        /// <summary>
        /// Maximum queue size before dropping events.
        /// Default: 10000. When queue is full, new events are dropped (not blocked).
        /// </summary>
        public int QueueSizeLimit { get; set; } = 10_000;

        /// <summary>
        /// Number of retry attempts for failed uploads.
        /// Default: 5. Range: 0-255.
        /// </summary>
        public byte RetryAttempts { get; set; } = 5;
    }
}
