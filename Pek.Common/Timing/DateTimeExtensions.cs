﻿namespace Pek.Timing;

/// <summary>
/// 日期时间辅助扩展操作
/// </summary>
public static partial class DateTimeExtensions
{
    #region 字段

    /// <summary>
    /// 1970年1月1日
    /// </summary>
    public static readonly DateTime Date1970 = new(1970, 1, 1);

    /// <summary>
    /// 最小日期
    /// </summary>
    public static readonly DateTime MinDate = new(1900, 1, 1);

    /// <summary>
    /// 最大日期
    /// </summary>
    public static readonly DateTime MaxDate = new(9999, 12, 31, 23, 59, 59, 999);

    /// <summary>
    /// 初始化js日期时间戳
    /// </summary>
    public static Int64 InitialJavaScriptDateTicks = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;

    #endregion

    #region IsWeekend(当前时间是否周末)

    /// <summary>
    /// 当前时间是否周末
    /// </summary>
    /// <param name="dateTime">时间点</param>
    public static Boolean IsWeekend(this DateTime dateTime)
    {
        DayOfWeek[] weeks = [DayOfWeek.Saturday, DayOfWeek.Sunday];
        return weeks.Contains(dateTime.DayOfWeek);
    }

    #endregion

    #region IsWorkday(当前时间是否工作日)
    /// <summary>
    /// 当前时间是否工作日
    /// </summary>
    /// <param name="dateTime">时间点</param>
    public static Boolean IsWeekday(this DateTime dateTime)
    {
        DayOfWeek[] weeks =
            [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday];
        return weeks.Contains(dateTime.DayOfWeek);
    }

    #endregion

    #region ToUniqueString(获取时间相对唯一字符串)
    /// <summary>
    /// 获取时间相对唯一字符串
    /// </summary>
    /// <param name="dateTime">时间点</param>
    /// <param name="milsec">是否使用毫秒</param>
    /// <returns></returns>
    public static String ToUniqueString(this DateTime dateTime, Boolean milsec = false)
    {
        var sedonds = dateTime.Hour * 3600 + dateTime.Minute * 60 + dateTime.Second;
        var value = String.Format("{0}{1}{2}", dateTime.ToString("yy"), dateTime.DayOfWeek, sedonds);
        return milsec ? value + dateTime.ToString("fff") : value;
    }

    #endregion

    #region ToJsGetTime(将时间转换为Js时间格式)
    /// <summary>
    /// 将时间转换为Js时间格式（Date.getTiem()）
    /// </summary>
    /// <param name="dateTime">时间点</param>
    /// <param name="milsec"></param>
    public static String ToJsGetTime(this DateTime dateTime, Boolean milsec = true)
    {
        var utc = dateTime.ToUniversalTime();
        var span = utc.Subtract(new DateTime(1970, 1, 1));
        return Math.Round(milsec ? span.TotalMilliseconds : span.TotalSeconds).ToString();
    }

    #endregion

    #region SetTime(设置时间)
    /// <summary>
    /// 设置时间，设置时分秒
    /// </summary>
    /// <param name="date">时间</param>
    /// <param name="hours">小时</param>
    /// <param name="minutes">分钟</param>
    /// <param name="seconds">秒</param>
    /// <returns>返回设置后的时间</returns>
    public static DateTime SetTime(this DateTime date, Int32 hours, Int32 minutes, Int32 seconds) => date.SetTime(new TimeSpan(hours, minutes, seconds));

    /// <summary>
    /// 设置时间，设置时分秒毫秒
    /// </summary>
    /// <param name="date">时间</param>
    /// <param name="hours">小时</param>
    /// <param name="minutes">分钟</param>
    /// <param name="seconds">秒</param>
    /// <param name="milliseconds">毫秒</param>
    /// <returns>返回设置后的时间</returns>
    public static DateTime SetTime(this DateTime date, Int32 hours, Int32 minutes, Int32 seconds, Int32 milliseconds) => date.SetTime(new TimeSpan(0, hours, minutes, seconds, milliseconds));

    /// <summary>
    /// 设置时间，设置时间间隔
    /// </summary>
    /// <param name="date">时间</param>
    /// <param name="time">时间间隔</param>
    /// <returns>返回设置后的时间</returns>
    public static DateTime SetTime(this DateTime date, TimeSpan time) => date.Date.Add(time);
    #endregion

    #region EndOfDay(设置指定时间为当天的结束时间)
    /// <summary>
    /// 设置指定时间为当天的结束时间。范例：yyyy-MM-dd 23:59:59.999
    /// </summary>
    /// <param name="date">指定时间</param>
    /// <returns>当天的结束时间</returns>
    public static DateTime EndOfDay(this DateTime date) => date.SetTime(23, 59, 59, 999);
    #endregion

    #region BeginOfDay(设置指定时间为当天的开始时间)
    /// <summary>
    /// 设置指定时间为当天的开始时间（凌晨）。范例：yyyy-MM-dd 00:00:00
    /// </summary>
    /// <param name="time">指定时间</param>
    /// <returns>当天的开始时间</returns>
    public static DateTime BeginOfDay(this DateTime time) => time.SetTime(0, 0, 0, 0);
    #endregion

    #region EndOfMonth(设置指定时间为当月的结束时间)

    /// <summary>
    /// 设置指定时间为当月的结束时间。范例：yyyy-MM-dd 23:59:59:999
    /// </summary>
    /// <param name="date">时间</param>
    /// <returns>当月的结束时间</returns>
    public static DateTime EndOfMonth(this DateTime date) => new(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month), 23, 59, 59, 999);

    #endregion

    #region BeginOfMonth(设置指定时间为当月的开始时间)

    /// <summary>
    /// 设置指定时间为当月的开始时间。范例：yyyy-MM-01 00:00:00.000
    /// </summary>
    /// <param name="date">时间</param>
    /// <returns>当月的开始时间</returns>
    public static DateTime BeginOfMonth(this DateTime date) => new(date.Year, date.Month, 1, 0, 0, 0, 0);

    #endregion

    #region GetFirstDayOfMonth(获取指定日期的月份第一天)
    /// <summary>
    /// 获取指定日期的月份第一天
    /// </summary>
    /// <param name="date">日期</param>
    /// <returns>月份第一天</returns>
    public static DateTime GetFirstDayOfMonth(this DateTime date) => new(date.Year, date.Month, 1);
    /// <summary>
    /// 获取指定日期的月份第一天，指定星期几
    /// </summary>
    /// <param name="date">日期</param>
    /// <param name="dayOfWeek">星期几</param>
    /// <returns>月份第一天</returns>
    public static DateTime GetFirstDayOfMonth(this DateTime date, DayOfWeek dayOfWeek)
    {
        var dt = date.GetFirstDayOfMonth();
        while (dt.DayOfWeek != dayOfWeek)
            dt = dt.AddDays(1);
        return dt;
    }
    #endregion

    #region GetLastDayOfMonth(获取指定日期的月份最后一天)
    /// <summary>
    /// 获取指定日期的月份最后一天
    /// </summary>
    /// <param name="date">日期</param>
    /// <returns>最后一天</returns>
    public static DateTime GetLastDayOfMonth(this DateTime date) => new(date.Year, date.Month, GetCountDaysOfMonth(date));
    /// <summary>
    /// 获取指定日期的月份最后一天，指定星期几
    /// </summary>
    /// <param name="date">日期</param>
    /// <param name="dayOfWeek">星期几</param>
    /// <returns>最后一天</returns>
    public static DateTime GetLastDayOfMonth(this DateTime date, DayOfWeek dayOfWeek)
    {
        var dt = date.GetLastDayOfMonth();
        while (dt.DayOfWeek != dayOfWeek)
            dt = dt.AddDays(-1);
        return dt;
    }
    #endregion

    #region GetCountDaysOfMonth(获取月总天数)
    /// <summary>
    /// 获取月总天数
    /// </summary>
    /// <param name="date">日期</param>
    /// <returns>月总天数</returns>
    public static Int32 GetCountDaysOfMonth(this DateTime date)
    {
        var nextMonth = date.AddMonths(1);
        return new DateTime(nextMonth.Year, nextMonth.Month, 1).AddDays(-1).Day;
    }
    #endregion

    #region GetMillisecondsSince1970(获取当前毫秒数)
    /// <summary>
    /// 获取当前毫秒数，毫秒数=1970年1月1日-当前时间，UNIX
    /// </summary>
    /// <param name="datetime">当前时间</param>
    /// <returns>毫秒数</returns>
    public static Int64 GetMillisecondsSince1970(this DateTime datetime)
    {
        var ts = datetime.Subtract(Date1970);
        return (Int64)ts.TotalMilliseconds;
    }
    #endregion

    #region CompareInterval(计算两个时间的间隔)

    /// <summary>
    /// 计算两个时间的间隔
    /// </summary>
    /// <param name="begin">开始时间</param>
    /// <param name="end">结束时间</param>
    /// <param name="dateFormat">间隔格式(y:年,M:月,d:天,h:小时,m:分钟,s:秒,fff:毫秒)</param>
    /// <returns></returns>
    public static Int64 CompareInterval(this DateTime begin, DateTime end, String dateFormat)
    {
        var interval = begin.Ticks - end.Ticks;
        DateTime dt1;
        DateTime dt2;
        switch (dateFormat)
        {
            case "fff":
                interval /= 10000;
                break;
            case "s":
                interval /= 10000000;
                break;
            case "m":
                interval /= 600000000;
                break;
            case "h":
                interval /= 36000000000;
                break;
            case "d":
                interval /= 864000000000;
                break;
            case "M":
                dt1 = (begin.CompareTo(end) >= 0) ? end : begin;
                dt2 = (begin.CompareTo(end) >= 0) ? begin : end;
                interval = -1;
                while (dt2.CompareTo(dt1) >= 0)
                {
                    interval++;
                    dt1 = dt1.AddMonths(1);
                }
                break;
            case "y":
                dt1 = (begin.CompareTo(end) >= 0) ? end : begin;
                dt2 = (begin.CompareTo(end) >= 0) ? begin : end;
                interval = -1;
                while (dt2.CompareTo(dt1) >= 0)
                {
                    interval++;
                    dt1 = dt1.AddMonths(1);
                }

                interval /= 12;
                break;
        }

        return interval;
    }

    #endregion

    #region IsBetweenTime(判断当前时间是否在指定时间段内)

    /// <summary>
    /// 判断当前时间是否在指定时间段内，格式：hh:mm:ss
    /// </summary>
    /// <param name="currentTime">当前时间</param>
    /// <param name="beginTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <returns></returns>
    public static Boolean IsBetweenTime(this DateTime currentTime, DateTime beginTime, DateTime endTime)
    {
        var am = beginTime.TimeOfDay;
        var pm = endTime.TimeOfDay;

        var now = currentTime.TimeOfDay;
        if (pm < am)//截止时间小于开始时间，表示跨天
        {
            if (now <= pm || now >= am)
            {
                return true;
            }
        }

        if (now >= am && now <= pm)
        {
            return true;
        }

        return false;
    }

    #endregion

    #region IsBetweenDate(判断当前时间是否在指定日期时间段内)

    /// <summary>
    /// 判断当前时间是否在指定日期时间段内，格式：yyyy-MM-dd
    /// </summary>
    /// <param name="currentDate">当前日期</param>
    /// <param name="beginDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns></returns>
    public static Boolean IsBetweenDate(this DateTime currentDate, DateTime beginDate, DateTime endDate)
    {
        var begin = beginDate.Date;
        var end = endDate.Date;
        var now = currentDate.Date;

        return now >= begin && now <= end;
    }

    #endregion

    #region IsBetween(判断当前时间是否在指定时间范围内)

    /// <summary>
    /// 判断当前时间是否在指定时间范围内，格式：yyyy-MM-dd HH:mm:ss
    /// </summary>
    /// <param name="current">当前时间</param>
    /// <param name="begin">开始时间</param>
    /// <param name="end">结束时间</param>
    /// <returns></returns>
    public static Boolean IsBetween(this DateTime current, DateTime begin, DateTime end)
    {
        var ticks = current.Ticks;
        return ticks >= begin.Ticks && ticks <= end.Ticks;
    }

    #endregion

    #region IsValid(是否有效时间)

    /// <summary>
    /// 是否有效时间
    /// </summary>
    /// <param name="value">值</param>
    /// <returns></returns>
    public static Boolean IsValid(this DateTime value) => (value >= MinDate) && (value <= MaxDate);

    #endregion

    #region ToTimeStamp(将时间转换为时间戳)

    /// <summary>
    /// 将时间转换为时间戳
    /// </summary>
    /// <param name="time">时间</param>
    /// <returns></returns>
    public static Int32 ToTimeStamp(this DateTime time) => (Int32)(time.ToUniversalTime().Ticks / 10000000 - 62135596800);

    #endregion

    #region CsharpTime2JavascriptTime(将C#时间转换为Javascript时间)

    /// <summary>
    /// 将C#时间转换为Javascript时间
    /// </summary>
    /// <param name="dateTime">时间</param>
    /// <returns></returns>
    public static Int64 CsharpTime2JavascriptTime(this DateTime dateTime) => (Int64)new TimeSpan(dateTime.Ticks - Date1970.Ticks).TotalMilliseconds;

    #endregion

    #region PhpTime2CsharpTime(将PHP时间转换为C#时间)

    /// <summary>
    /// 将PHP时间转换为C#时间
    /// </summary>
    /// <param name="time">PHP的时间</param>
    /// <returns></returns>
    public static DateTime PhpTime2CsharpTime(Int64 time)
    {
        var t = (time + 8 * 60 * 60) * 10000000 + Date1970.Ticks;
        return new DateTime(t);
    }

    #endregion

    #region CsharpTime2PhpTime(将C#时间转换为PHP时间)

    /// <summary>
    /// 将C#时间转换为PHP时间
    /// </summary>
    /// <returns></returns>
    public static Int64 CsharpTime2PhpTime() => (DateTime.UtcNow.Ticks - Date1970.Ticks) / 10000000;

    #endregion

    #region AddWeeks(添加星期)

    /// <summary>
    /// 添加星期
    /// </summary>
    /// <param name="dateTime">时间</param>
    /// <param name="weeks">周</param>
    /// <returns></returns>
    public static DateTime AddWeeks(this DateTime dateTime, Int32 weeks) => dateTime.AddDays(weeks * 7);

    #endregion

    #region ConvertToTimeZone(将当前时间转换为特定时区的时间)

    /// <summary>
    /// 将当前时间转换为特定时区的时间
    /// </summary>
    /// <param name="dateTime">时间</param>
    /// <param name="timeZone">时区</param>
    /// <returns></returns>
    public static DateTime ConvertToTimeZone(this DateTime dateTime, TimeZoneInfo timeZone)
    {
        return TimeZoneInfo.ConvertTime(dateTime, timeZone);
    }

    #endregion
}