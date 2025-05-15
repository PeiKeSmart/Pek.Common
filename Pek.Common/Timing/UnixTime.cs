namespace Pek.Timing;

/// <summary>
/// Unix时间操作
/// </summary>
public static class UnixTime
{
    /// <summary>
    /// Unix纪元时间
    /// </summary>
    public static DateTime EpochTime = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// 转换为Unix时间戳
    /// </summary>
    /// <param name="isContainMillisecond">是否包含毫秒</param>
    /// <returns></returns>
    public static Int64 ToTimestamp(Boolean isContainMillisecond = true) => ToTimestamp(DateTime.Now, isContainMillisecond);

    /// <summary>
    /// 转换为Unix时间戳
    /// </summary>
    /// <param name="dateTime">时间</param>
    /// <param name="isContainMillisecond">是否包含毫秒</param>
    /// <returns></returns>
    public static Int64 ToTimestamp(DateTime dateTime, Boolean isContainMillisecond = true)
    {
        return dateTime.Kind == DateTimeKind.Utc
            ? Convert.ToInt64((dateTime - EpochTime).TotalMilliseconds / (isContainMillisecond ? 1 : 1000))
            : Convert.ToInt64((TimeZoneInfo.ConvertTimeToUtc(dateTime) - EpochTime).TotalMilliseconds /
                              (isContainMillisecond ? 1 : 1000));
    }

    /// <summary>
    /// 转换为DateTime对象
    /// </summary>
    /// <param name="timestamp">时间戳。</param>
    /// <param name="isContainMillisecond">是否包含毫秒</param>
    /// <returns></returns>
    public static DateTime ToDateTime(Int64 timestamp, Boolean isContainMillisecond = true)
    {
        if (isContainMillisecond)
            return EpochTime.AddMilliseconds(timestamp).ToLocalTime();

        return EpochTime.AddSeconds(timestamp).ToLocalTime();
    }

    /// <summary>
    /// 转换为DateTime对象
    /// </summary>
    /// <param name="timestamp">时间戳</param>
    /// <param name="timeZoneOffset">时区。如+1/-1等</param>
    /// <param name="isContainMillisecond">是否包含毫秒</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static DateTime ToDateTime(Int64 timestamp, String timeZoneOffset, Boolean isContainMillisecond = true)
    {
        DateTime utcDateTime;
        if (isContainMillisecond)
        {
            utcDateTime = EpochTime.AddMilliseconds(timestamp);
        }
        else
        {
            utcDateTime = EpochTime.AddSeconds(timestamp);
        }

        // 解析时区偏移
        if (TryParseTimeZoneOffset(timeZoneOffset, out var offset))
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, TimeZoneInfo.CreateCustomTimeZone(timeZoneOffset, offset, timeZoneOffset, timeZoneOffset));
        }
        else
        {
            throw new ArgumentException("Invalid time zone offset format.");
        }
    }

    /// <summary>
    /// 尝试解析时区偏移
    /// </summary>
    /// <param name="timeZoneOffset">时区。如+1/-1等</param>
    /// <param name="offset">解析后的 <see cref="TimeSpan"/> 对象。</param>
    /// <returns></returns>
    private static Boolean TryParseTimeZoneOffset(String timeZoneOffset, out TimeSpan offset)
    {
        offset = TimeSpan.Zero;
        if (String.IsNullOrEmpty(timeZoneOffset)) return false;

        var isNegative = timeZoneOffset[0] == '-';
        if (timeZoneOffset[0] != '+' && !isNegative) return false;

        if (Int32.TryParse(timeZoneOffset.Substring(1), out var hours))
        {
            offset = new TimeSpan(hours * (isNegative ? -1 : 1), 0, 0);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 转换为Utc DateTime时区为0的时间
    /// </summary>
    /// <param name="timestamp">时间戳。毫秒</param>
    /// <returns></returns>
    public static DateTimeOffset ToUtcDateTime(Int64 timestamp)
    {
        var utcTime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
        var zeroOffsetTime = utcTime.ToOffset(TimeSpan.Zero);

        return zeroOffsetTime;
    }

    /// <summary>
    /// 当前时间转换为Utc DateTime时区为0的时间
    /// </summary>
    /// <returns></returns>
    public static DateTimeOffset ToUtcZeroDateTime()
    {
        // 获取当前时间
        var localTime = DateTimeOffset.Now;

        // 获取时区为0的时间
        var utcTime = TimeZoneInfo.ConvertTime(localTime, TimeZoneInfo.Utc);

        return utcTime;
    }

    /// <summary>
    /// 指定时间转换为Utc DateTime时区为0的时间
    /// </summary>
    /// <param name="dateTime">带时区的UTC时间</param>
    /// <returns></returns>
    public static DateTimeOffset ToUtcZeroDateTime(DateTimeOffset dateTime)
    {
        // 获取时区为0的时间
        var utcTime = TimeZoneInfo.ConvertTime(dateTime, TimeZoneInfo.Utc);

        return utcTime;
    }

    /// <summary>
    /// 指定Utc DateTime时区为0的时间转化为本地时间
    /// </summary>
    /// <param name="dateTime">带时区的UTC时间</param>
    /// <returns></returns>
    public static DateTimeOffset ToUtcZeroDateTime(DateTime dateTime)
    {
        var inputTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);  // 时区为0的时间

        var utcTime = inputTime.ToUniversalTime().ToLocalTime();

        return utcTime;
    }
}