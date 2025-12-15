using System.Globalization;

namespace MT5Bridge.Core.Timing;

public static class TimeX
{
    public static long CurrentUnixTimeMillis()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public static long CurrentUnixTimeSecond()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public static long ToUnixTimeMillis(DateTime dateTime)
    {
        var timeKind = dateTime.Kind;
        if (timeKind == DateTimeKind.Utc)
        {
            return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
        }
        else if (timeKind == DateTimeKind.Local)
        {
            return new DateTimeOffset(dateTime.ToUniversalTime()).ToUnixTimeMilliseconds();
        }
        else
        {
            // Assume Unspecified is utc
            dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
        }
    }

    public static long ToUnixTimeMillisHighPerformance(DateTime dateTime)
    {
        if (dateTime.Kind == DateTimeKind.Unspecified)
        {
            dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        }
        if (dateTime.Kind != DateTimeKind.Utc)
        {
            dateTime = dateTime.ToUniversalTime();
        }

        return (dateTime.Ticks - 621355968000000000L) / 10000;
    }

    public static long ToUnixTimeSec(DateTime dateTime)
    {
        return ToUnixTimeMillis(dateTime) / 1000;
    }

    public static DateTime FromUnixTimeMillis(long unixTimeMillis)
    {
        return DateTimeOffset.FromUnixTimeMilliseconds(unixTimeMillis).UtcDateTime;
    }

    public static DateTime FromUnixTimeSec(long unixTimeSec)
    {
        return DateTimeOffset.FromUnixTimeSeconds(unixTimeSec).UtcDateTime;
    }

    public static string ToExpiredDatePattern(long expireDateUnixSeconds)
    {
        var dateTime = DateTimeOffset.FromUnixTimeSeconds(expireDateUnixSeconds).UtcDateTime;

        return FormatToExpiredDatePattern(dateTime);
    }

    public static string FormatToExpiredDatePattern(DateTime dateTime)
    {
        return dateTime.ToString("yyyyMMdd-HH:mm");
    }

    public static DateTime ParseToUtc(string timeValue, string timeFmt)
    {
        if (string.IsNullOrWhiteSpace(timeValue) || string.IsNullOrWhiteSpace(timeFmt))
        {
            throw new ArgumentException("Time format and value cannot be null or empty.");
        }

        DateTime parsedDateTime;
        try
        {
            parsedDateTime = DateTime.ParseExact(timeValue,
                timeFmt,
                null,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
        }
        catch (FormatException)
        {
            throw new ArgumentException($"The time value '{timeValue}' does not match the format '{timeFmt}'.");
        }

        return parsedDateTime;
    }

    public static (DateTime, bool) ParseToUtcWithRet(string timeValue, string timeFmt)
    {
        try
        {
            var parsedDateTime = ParseToUtc(timeValue, timeFmt);
            return (parsedDateTime, true);
        }
        catch (ArgumentException)
        {
            return (DateTime.UtcNow, false);
        }
    }
}
