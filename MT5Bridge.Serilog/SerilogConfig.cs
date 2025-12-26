using Serilog.Events;

namespace MT5Bridge.Serilog;

public class SerilogConfig
{
    public ConsoleConfig Console { get; set; } = new();
    public FileConfig File { get; set; } = new();
    public CloudWatchConfig CloudWatch { get; set; } = new();
    public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Information;

    public class ConsoleConfig
    {
        public bool Enabled { get; set; } = true;
        public bool UseJson { get; set; } = false;

        /// <summary>
        /// Use ANSI color codes in console output. Set to false for non-TTY environments.
        /// Default: true (colors enabled).
        /// </summary>
        public bool UseAnsiColors { get; set; } = true;
    }

    public class FileConfig
    {
        public bool Enabled { get; set; } = false;
        public string Path { get; set; } = "logs/log-.txt";
        public string RollingInterval { get; set; } = "Day"; // Day, Hour, etc.
        public long? FileSizeLimitBytes { get; set; } = 10 * 1024 * 1024; // 10MB
        public int? RetainedFileCountLimit { get; set; } = 31;
        public bool UseJson { get; set; } = true;
    }

    public class CloudWatchConfig
    {
        public bool Enabled { get; set; } = false;
        public string LogGroup { get; set; } = "MT5Bridge";
        public string LogStreamPrefix { get; set; } = "Demo";
        public string Region { get; set; } = "us-east-1";
        public bool UseJson { get; set; } = true;

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
    }
}
