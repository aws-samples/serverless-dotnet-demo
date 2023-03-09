using System;

namespace GenerateLoadTestResults;

public static class DateUtils
{
    public static long AsUnixTimestamp(this DateTime date)
    {
        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        TimeSpan diff = date.ToUniversalTime() - origin;
        return (long)Math.Floor(diff.TotalSeconds);
    }
}