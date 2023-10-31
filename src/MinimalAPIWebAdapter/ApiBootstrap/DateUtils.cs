using System;

namespace GetProducts;

public static class DateUtils
{
    public static long AsUnixTimestamp(this DateTime date)
    {
        //why not using DateTimeOffset.UtcNow.ToUnixTimeSeconds()?
        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        TimeSpan diff = date.ToUniversalTime() - origin;
        return (long)Math.Floor(diff.TotalSeconds);
    }
}