using Pek.Helpers;

namespace Pek.Timing;

/// <summary>
/// 时间操作
/// </summary>
public static class Time
{
    /// <summary>
    /// 日期
    /// </summary>
    private static DateTime? _dateTime;

    /// <summary>
    /// 设置时间
    /// </summary>
    /// <param name="dateTime">时间</param>
    public static void SetTime(DateTime? dateTime) => _dateTime = dateTime;

    /// <summary>
    /// 设置时间
    /// </summary>
    /// <param name="dateTime">时间</param>
    public static void SetTime(String dateTime) => _dateTime = dateTime.ToDGDateOrNull();

    /// <summary>
    /// 重置时间
    /// </summary>
    public static void Reset() => _dateTime = null;

    /// <summary>
    /// 获取当前日期时间
    /// </summary>
    public static DateTime GetDateTime() => _dateTime ?? DateTime.Now;

    /// <summary>
    /// 获取当前日期，不带时间
    /// </summary>
    public static DateTime GetDate() => GetDateTime().Date;

    /// <summary>
    /// 获取Unix时间戳
    /// </summary>
    public static Int64 GetUnixTimestamp() => GetUnixTimestamp(DateTime.Now);

    /// <summary>
    /// 获取Unix时间戳
    /// </summary>
    /// <param name="time">时间</param>
    public static Int64 GetUnixTimestamp(DateTime time)
    {
        var start = TimeZoneInfo.ConvertTime(DateTimeExtensions.Date1970, TimeZoneInfo.Local);
        var ticks = (time - start.Add(new TimeSpan(8, 0, 0))).Ticks;
        return (ticks / TimeSpan.TicksPerSecond).ToDGLong();
    }

    /// <summary>
    /// 从Unix时间戳获取时间
    /// </summary>
    /// <param name="timestamp">Unix时间戳</param>
    public static DateTime GetTimeFromUnixTimestamp(Int64 timestamp)
    {
        var start = TimeZoneInfo.ConvertTime(DateTimeExtensions.Date1970, TimeZoneInfo.Local);
        var span = new TimeSpan(Int64.Parse(timestamp + "0000000"));
        return start.Add(span).Add(new TimeSpan(8, 0, 0));
    }
}