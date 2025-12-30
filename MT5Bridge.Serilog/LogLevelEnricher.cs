using Serilog.Core;
using Serilog.Events;

namespace MT5Bridge.Serilog;

/// <summary>
/// Enriches log events with a short-form log level property (l) for use with compact JSON formatters.
/// This enricher adds the log level in a format similar to {Level:u3}, making it available in
/// CompactJsonFormatter and RenderedCompactJsonFormatter outputs.
/// </summary>
/// <remarks>
/// <para>
/// The Serilog.Formatting.Compact formatters have the following default behavior:
/// - Information level: Omits @l field (to reduce size for high-volume logs)
/// - Other levels (Verbose, Debug, Warning, Error, Fatal): Includes @l field
/// </para>
/// <para>
/// This enricher adds a custom "l" property with the three-letter log level abbreviation (u3 format)
/// to all log events, ensuring consistent level information across all log levels. This is especially
/// useful when you need to filter Information-level logs by level in log aggregation systems.
/// </para>
/// <para>
/// Output format in CompactJsonFormatter:
/// - Information: {"@t":"...","@mt":"...","l":"INF",...}
/// - Warning: {"@t":"...","@mt":"...","@l":"Warning","l":"WRN",...}
/// - Error: {"@t":"...","@mt":"...","@l":"Error","l":"ERR",...}
/// </para>
/// <para>
/// The property is named "l" (without @) because CompactJsonFormatter treats @ as a reserved prefix
/// for system fields. The "l" property provides a consistent, compact level indicator across all
/// log levels.
/// </para>
/// <para>
/// Level abbreviations (u3 format):
/// - Verbose: "VRB"
/// - Debug: "DBG"
/// - Information: "INF"
/// - Warning: "WRN"
/// - Error: "ERR"
/// - Fatal: "FTL"
/// </para>
/// </remarks>
public class LogLevelEnricher : ILogEventEnricher
{
    /// <summary>
    /// Enriches the log event by adding an "l" property with the short-form log level.
    /// </summary>
    /// <param name="logEvent">The log event to enrich</param>
    /// <param name="propertyFactory">Factory for creating new properties</param>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var levelShort = GetShortLevel(logEvent.Level);
        var property = propertyFactory.CreateProperty("l", levelShort);
        logEvent.AddPropertyIfAbsent(property);
    }

    /// <summary>
    /// Converts LogEventLevel to its three-letter abbreviation (u3 format).
    /// </summary>
    /// <param name="level">The log event level</param>
    /// <returns>Three-letter abbreviation (VRB, DBG, INF, WRN, ERR, FTL)</returns>
    private static string GetShortLevel(LogEventLevel level)
    {
        return level switch
        {
            LogEventLevel.Verbose => "VRB",
            LogEventLevel.Debug => "DBG",
            LogEventLevel.Information => "INF",
            LogEventLevel.Warning => "WRN",
            LogEventLevel.Error => "ERR",
            LogEventLevel.Fatal => "FTL",
            _ => level.ToString().ToUpperInvariant()[..3] // Fallback: first 3 chars
        };
    }
}
