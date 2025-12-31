using MetaQuotes.MT5CommonAPI;
using Xunit.Abstractions;

namespace MT5Bridge.Manager.Tests;

public class SMTTimeTests
{
    private readonly ITestOutputHelper _output;

    public SMTTimeTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Test_SMTTime_FromDateTime_Conversion()
    {
        // Arrange
        var now = DateTime.Now;
        var utcNow = DateTime.UtcNow;

        // Act
        // SMTTime.FromDateTime usually expects a DateTime and converts it to MT5 timestamp (seconds since 1970)
        var smtTimeNow = SMTTime.FromDateTime(now);
        var smtTimeUtcNow = SMTTime.FromDateTime(utcNow);

        // Calculate expected Unix timestamp (seconds)
        // Note: ToUnixTimeSeconds() converts the DateTimeOffset to UTC before calculating, 
        // so we need to be careful about what we compare against.

        // If we treat 'now' as if it were UTC (ignoring Kind), what would be the timestamp?
        var unixIfNowWasUtc = new DateTimeOffset(now.Ticks, TimeSpan.Zero).ToUnixTimeSeconds();

        // Standard Unix Timestamp (based on UTC)
        var realUnixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Output
        _output.WriteLine($"Current System Time (Local): {now:yyyy-MM-dd HH:mm:ss.fff zzz}");
        _output.WriteLine($"Current System Time (UTC)  : {utcNow:yyyy-MM-dd HH:mm:ss.fff zzz}");
        _output.WriteLine("");

        _output.WriteLine($"1. Testing with Local Time: {now}");
        _output.WriteLine($"   SMTTime.FromDateTime(Local): {smtTimeNow}");
        _output.WriteLine($"   Unix Timestamp (Real UTC)  : {realUnixTimestamp}");
        _output.WriteLine($"   Unix Timestamp (If Local treated as UTC): {unixIfNowWasUtc}");

        _output.WriteLine("");

        _output.WriteLine($"2. Testing with UTC Time: {utcNow}");
        _output.WriteLine($"   SMTTime.FromDateTime(UTC)  : {smtTimeUtcNow}");
        _output.WriteLine($"   Unix Timestamp (Real UTC)  : {realUnixTimestamp}");

        _output.WriteLine("");
        var unspecified = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Unspecified);
        var smtUnspecified = SMTTime.FromDateTime(unspecified);
        _output.WriteLine($"3. Testing with Unspecified Time: {unspecified}");
        _output.WriteLine($"   SMTTime.FromDateTime(Unspecified): {smtUnspecified}");

        // Calculate what 12:00 corresponds to if treated as UTC
        var unixIfUnspecifiedWasUtc = new DateTimeOffset(unspecified, TimeSpan.Zero).ToUnixTimeSeconds();
        _output.WriteLine($"   Unix Timestamp (If treated as UTC): {unixIfUnspecifiedWasUtc}");

        // Assert
        // We verify if it produces a valid long value (not 0)
        Assert.NotEqual(0, smtTimeNow);
        Assert.NotEqual(0, smtTimeUtcNow);
    }
}
