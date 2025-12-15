using MT5Bridge.Core.Timing;

namespace MT5Bridge.Core.Tests.Timing;

public class TimeXTests
{
    [Fact]
    public void CurrentUnixTimeMillis_ShouldReturnReasonableValue()
    {
        long unixMillis = TimeX.CurrentUnixTimeMillis();

        // Should be somewhere around December 2025 (roughly 1734220800000)
        Assert.True(unixMillis > 1700000000000L); // After 2023
        Assert.True(unixMillis < 2000000000000L); // Before 2033
    }

    [Fact]
    public void CurrentUnixTimeSecond_ShouldReturnReasonableValue()
    {
        long unixSeconds = TimeX.CurrentUnixTimeSecond();

        // Should be somewhere around December 2025 (roughly 1734220800)
        Assert.True(unixSeconds > 1700000000L);
        Assert.True(unixSeconds < 2000000000L);
    }

    [Fact]
    public void ToUnixTimeMillis_WithUtcDateTime_ShouldConvert()
    {
        var dt = new DateTime(2023, 12, 15, 12, 30, 45, DateTimeKind.Utc);

        long unixMillis = TimeX.ToUnixTimeMillis(dt);

        // Use DateTimeOffset for expected value to avoid timezone issues
        long expected = new DateTimeOffset(dt).ToUnixTimeMilliseconds();
        Assert.Equal(expected, unixMillis);
    }

    [Fact]
    public void ToUnixTimeMillis_WithLocalDateTime_ShouldConvert()
    {
        var utcDt = new DateTime(2023, 12, 15, 12, 30, 45, DateTimeKind.Utc);
        var localDt = utcDt.ToLocalTime();

        long unixMillis = TimeX.ToUnixTimeMillis(localDt);

        // Should convert to same UTC time
        long expected = new DateTimeOffset(utcDt).ToUnixTimeMilliseconds();
        Assert.Equal(expected, unixMillis);
    }

    [Fact]
    public void ToUnixTimeMillis_WithUnspecifiedKind_ShouldAssumeUtc()
    {
        var dt = new DateTime(2023, 12, 15, 12, 30, 45, DateTimeKind.Unspecified);

        long unixMillis = TimeX.ToUnixTimeMillis(dt);

        // Should treat as UTC
        var utcDt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        long expected = new DateTimeOffset(utcDt).ToUnixTimeMilliseconds();
        Assert.Equal(expected, unixMillis);
    }

    [Fact]
    public void ToUnixTimeMillisHighPerformance_ShouldMatchRegularMethod()
    {
        var dt = new DateTime(2023, 12, 15, 12, 30, 45, DateTimeKind.Utc);

        long regular = TimeX.ToUnixTimeMillis(dt);
        long highPerf = TimeX.ToUnixTimeMillisHighPerformance(dt);

        Assert.Equal(regular, highPerf);
    }

    [Fact]
    public void ToUnixTimeSec_ShouldConvertToSeconds()
    {
        var dt = new DateTime(2023, 12, 15, 12, 30, 45, DateTimeKind.Utc);

        long unixSec = TimeX.ToUnixTimeSec(dt);

        long expected = new DateTimeOffset(dt).ToUnixTimeSeconds();
        Assert.Equal(expected, unixSec);
    }

    [Fact]
    public void FromUnixTimeMillis_ShouldConvertBackToDateTime()
    {
        var originalDt = new DateTime(2023, 12, 15, 12, 30, 45, DateTimeKind.Utc);
        long unixMillis = new DateTimeOffset(originalDt).ToUnixTimeMilliseconds();

        DateTime dt = TimeX.FromUnixTimeMillis(unixMillis);

        Assert.Equal(originalDt, dt);
    }

    [Fact]
    public void FromUnixTimeSec_ShouldConvertBackToDateTime()
    {
        var originalDt = new DateTime(2023, 12, 15, 12, 30, 45, DateTimeKind.Utc);
        long unixSec = new DateTimeOffset(originalDt).ToUnixTimeSeconds();

        DateTime dt = TimeX.FromUnixTimeSec(unixSec);

        Assert.Equal(originalDt, dt);
    }

    [Fact]
    public void RoundTrip_MillisToDateTimeToMillis_ShouldMatch()
    {
        long originalMillis = 1702645845123L;

        DateTime dt = TimeX.FromUnixTimeMillis(originalMillis);
        long convertedBack = TimeX.ToUnixTimeMillis(dt);

        Assert.Equal(originalMillis, convertedBack);
    }

    [Fact]
    public void RoundTrip_SecondsToDateTimeToSeconds_ShouldMatch()
    {
        long originalSec = 1702645845L;

        DateTime dt = TimeX.FromUnixTimeSec(originalSec);
        long convertedBack = TimeX.ToUnixTimeSec(dt);

        Assert.Equal(originalSec, convertedBack);
    }

    [Fact]
    public void ToExpiredDatePattern_ShouldFormatCorrectly()
    {
        var dt = new DateTime(2023, 12, 15, 12, 30, 45, DateTimeKind.Utc);
        long unixSec = new DateTimeOffset(dt).ToUnixTimeSeconds();

        string formatted = TimeX.ToExpiredDatePattern(unixSec);

        Assert.Equal("20231215-12:30", formatted);
    }

    [Fact]
    public void FormatToExpiredDatePattern_ShouldFormatDateTime()
    {
        var dt = new DateTime(2023, 12, 15, 14, 45, 30, DateTimeKind.Utc);

        string formatted = TimeX.FormatToExpiredDatePattern(dt);

        Assert.Equal("20231215-14:45", formatted);
    }

    [Fact]
    public void ParseToUtc_WithValidInput_ShouldParse()
    {
        string timeValue = "20231215-12:30";
        string timeFmt = "yyyyMMdd-HH:mm";

        DateTime parsed = TimeX.ParseToUtc(timeValue, timeFmt);

        Assert.Equal(new DateTime(2023, 12, 15, 12, 30, 0, DateTimeKind.Utc), parsed);
        Assert.Equal(DateTimeKind.Utc, parsed.Kind);
    }

    [Fact]
    public void ParseToUtc_WithInvalidFormat_ShouldThrow()
    {
        string timeValue = "2023-12-15";
        string timeFmt = "yyyyMMdd-HH:mm";

        Assert.Throws<ArgumentException>(() => TimeX.ParseToUtc(timeValue, timeFmt));
    }

    [Fact]
    public void ParseToUtc_WithEmptyString_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => TimeX.ParseToUtc("", "yyyyMMdd"));
        Assert.Throws<ArgumentException>(() => TimeX.ParseToUtc("20231215", ""));
    }

    [Fact]
    public void ParseToUtcWithRet_WithValidInput_ShouldReturnSuccess()
    {
        string timeValue = "20231215-12:30";
        string timeFmt = "yyyyMMdd-HH:mm";

        var (parsed, success) = TimeX.ParseToUtcWithRet(timeValue, timeFmt);

        Assert.True(success);
        Assert.Equal(new DateTime(2023, 12, 15, 12, 30, 0, DateTimeKind.Utc), parsed);
    }

    [Fact]
    public void ParseToUtcWithRet_WithInvalidInput_ShouldReturnFailure()
    {
        string timeValue = "invalid";
        string timeFmt = "yyyyMMdd-HH:mm";

        var (parsed, success) = TimeX.ParseToUtcWithRet(timeValue, timeFmt);

        Assert.False(success);
        // Should return current time on failure
        Assert.True((DateTime.UtcNow - parsed).TotalSeconds < 5);
    }
}
