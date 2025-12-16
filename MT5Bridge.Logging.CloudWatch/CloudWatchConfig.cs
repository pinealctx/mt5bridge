namespace MT5Bridge.Logging.CloudWatch;

using System.Text.Json.Serialization;

/// <summary>
/// AWS CloudWatch logging configuration
/// </summary>
public class CloudWatchConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("log_group")]
    public string LogGroup { get; set; } = string.Empty;

    [JsonPropertyName("log_stream")]
    public string LogStream { get; set; } = string.Empty;

    [JsonPropertyName("region")]
    public string Region { get; set; } = string.Empty;

    [JsonPropertyName("access_key_id")]
    public string AccessKeyId { get; set; } = string.Empty;

    [JsonPropertyName("secret_key")]
    public string SecretKey { get; set; } = string.Empty;

    [JsonPropertyName("batch_size")]
    public int BatchSize { get; set; } = 100;

    [JsonPropertyName("batch_timeout_ms")]
    public int BatchTimeoutMs { get; set; } = 10000;

    /// <summary>
    /// Validate if the configuration is valid
    /// </summary>
    /// <returns>Validation result and error message</returns>
    public (bool IsValid, string ErrorMessage) Validate()
    {
        if (string.IsNullOrEmpty(LogGroup))
        {
            return (false, "LogGroup is required");
        }
        if (string.IsNullOrEmpty(LogStream))
        {
            return (false, "LogStream is required");
        }
        if (string.IsNullOrEmpty(Region))
        {
            return (false, "Region is required");
        }
        if (string.IsNullOrEmpty(AccessKeyId))
        {
            return (false, "AccessKeyId is required");
        }
        if (string.IsNullOrEmpty(SecretKey))
        {
            return (false, "SecretKey is required");
        }

        return (true, string.Empty);
    }
}
