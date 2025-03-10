﻿namespace Pek.Timing;

/// <summary>
/// 时间工厂
/// </summary>
public static class DateTimeFactory
{
    /// <summary>
    /// 获取当前本地时间
    /// </summary>
    public static DateTime Now() => DateTime.Now;

    /// <summary>
    /// 获取当前UTC时间
    /// </summary>
    public static DateTime UtcNow() => DateTime.UtcNow;

    /// <summary>
    /// 根据指定的日期创建 <see cref="DateTime"/>
    /// </summary>
    /// <param name="year">年</param>
    /// <param name="month">月</param>
    /// <param name="day">日</param>
    public static DateTime Create(Int32 year, Int32 month, Int32 day) => new(year, month, day);

    /// <summary>
    /// 根据指定的日期创建 <see cref="DateTime"/>
    /// </summary>
    /// <param name="year">年</param>
    /// <param name="month">月</param>
    /// <param name="day">日</param>
    /// <param name="hour">时</param>
    /// <param name="minute">分</param>
    /// <param name="second">秒</param>
    public static DateTime Create(Int32 year, Int32 month, Int32 day, Int32 hour, Int32 minute, Int32 second) =>
        new(year, month, day, hour, minute, second);

    /// <summary>
    /// 根据指定的日期创建 <see cref="DateTime"/>
    /// </summary>
    /// <param name="year">年</param>
    /// <param name="month">月</param>
    /// <param name="day">日</param>
    /// <param name="hour">时</param>
    /// <param name="minute">分</param>
    /// <param name="second">秒</param>
    /// <param name="millisecond">毫秒</param>
    public static DateTime Create(Int32 year, Int32 month, Int32 day, Int32 hour, Int32 minute, Int32 second, Int32 millisecond) =>
        new(year, month, day, hour, minute, second, millisecond);

    /// <summary>
    /// 根据指定的日期创建 <see cref="DateTime"/>
    /// </summary>
    /// <param name="year">年</param>
    /// <param name="month">月</param>
    /// <param name="day">日</param>
    /// <param name="hour">时</param>
    /// <param name="minute">分</param>
    /// <param name="second">秒</param>
    /// <param name="millisecond">毫秒</param>
    /// <param name="kind">时间种类</param>
    public static DateTime Create(Int32 year, Int32 month, Int32 day, Int32 hour, Int32 minute, Int32 second, Int32 millisecond,
        DateTimeKind kind) =>
        new(year, month, day, hour, minute, second, millisecond, kind);

    /// <summary>
    /// 根据指定的年月和偏移信息创建 <see cref="DateTime"/>
    /// </summary>
    /// <param name="year">年</param>
    /// <param name="month">月</param>
    /// <param name="weekAtMonth">当月第几个星期</param>
    /// <param name="dayOfWeek">星期几</param>
    public static DateTime OffsetByWeek(Int32 year, Int32 month, Int32 weekAtMonth, Int32 dayOfWeek)
    {
        var fd = Create(year, month, 1);
        var fDayOfWeek = fd.DayOfWeek.ToInt();
        var restDayOfFdInWeek = 7 - fDayOfWeek + 1;// 计算第一周剩余天数

        var targetDay = fDayOfWeek > dayOfWeek
            ? (weekAtMonth - 1) * 7 + dayOfWeek + restDayOfFdInWeek
            : (weekAtMonth - 2) * 7 + dayOfWeek + restDayOfFdInWeek;
        return Create(year, month, targetDay);
    }

    /// <summary>
    /// 根据指定的年月和偏移信息创建 <see cref="DateTime"/>
    /// </summary>
    /// <param name="year">年</param>
    /// <param name="month">月</param>
    /// <param name="weekAtMonth">当月第几个星期</param>
    /// <param name="dayOfWeek">星期几</param>
    public static DateTime OffsetByWeek(Int32 year, Int32 month, Int32 weekAtMonth, DayOfWeek dayOfWeek) =>
        OffsetByWeek(year, month, weekAtMonth, dayOfWeek.ToInt());

    /// <summary>
    /// 寻找一个月中的最后一个工作日（如周一）
    /// </summary>
    /// <param name="year">日</param>
    /// <param name="month">月</param>
    /// <param name="dayOfWeek">星期几</param>
    public static DateTime FindLastDay(Int32 year, Int32 month, DayOfWeek dayOfWeek)
    {
        var resultedDay = FindDay(year, month, dayOfWeek, 5);
        if (resultedDay == DateTime.MinValue)
            resultedDay = FindDay(year, month, dayOfWeek, 4);
        return resultedDay;
    }

    /// <summary>
    /// 根据指定的日期，获得下一个工作日（如下周一）
    /// </summary>
    /// <param name="year">年</param>
    /// <param name="month">月</param>
    /// <param name="day">日</param>
    /// <param name="dayOfWeek">下星期几</param>
    public static DateTime FindNextDay(Int32 year, Int32 month, Int32 day, DayOfWeek dayOfWeek)
    {
        var calculationDay = Create(year, month, day);
        return FindNextDay(calculationDay, dayOfWeek);
    }

    /// <summary>
    /// 根据指定的日期，获得下一个工作日（如下周一）
    /// </summary>
    /// <param name="dt">时间</param>
    /// <param name="dayOfWeek">下星期几</param>
    public static DateTime FindNextDay(DateTime dt, DayOfWeek dayOfWeek)
    {
        var daysNeeded = (Int32)dayOfWeek - (Int32)dt.DayOfWeek;
        return (Int32)dayOfWeek >= (Int32)dt.DayOfWeek
            ? dt.AddDays(daysNeeded)
            : dt.AddDays(daysNeeded + 7);
    }

    /// <summary>
    /// 根据指定的日期，获得上一个工作日（如上周一）
    /// </summary>
    /// <param name="year">年</param>
    /// <param name="month">月</param>
    /// <param name="day">日</param>
    /// <param name="dayOfWeek">上星期几</param>
    public static DateTime FindDayBefore(Int32 year, Int32 month, Int32 day, DayOfWeek dayOfWeek)
    {
        var calculationDay = Create(year, month, day);
        return FindDayBefore(calculationDay, dayOfWeek);
    }

    /// <summary>
    /// 根据指定的日期，获得上一个工作日（如上周一）
    /// </summary>
    /// <param name="dt">时间</param>
    /// <param name="dayOfWeek">上星期几</param>
    public static DateTime FindDayBefore(DateTime dt, DayOfWeek dayOfWeek)
    {
        var daysSubtract = (Int32)dayOfWeek - (Int32)dt.DayOfWeek;
        return (Int32)dayOfWeek < (Int32)dt.DayOfWeek
            ? dt.AddDays(daysSubtract)
            : dt.AddDays(daysSubtract - 7);
    }

    /// <summary>
    /// 寻找指定的日期（如一个月的第三个周一）
    /// </summary>
    /// <param name="year">年</param>
    /// <param name="month">月</param>
    /// <param name="dayOfWeek">星期几</param>
    /// <param name="occurrence">第几个星期</param>
    public static DateTime FindDay(Int32 year, Int32 month, DayOfWeek dayOfWeek, Int32 occurrence)
    {
        // 一个月有5个星期，2月份可能只有4个星期
        if (occurrence == 0 || occurrence > 5)
            throw new IndexOutOfRangeException(nameof(occurrence));
        var firstDayOfMonth = Create(year, month, 1);
        var daysNeeded = (Int32)dayOfWeek - (Int32)firstDayOfMonth.DayOfWeek;
        if (daysNeeded < 0)
            daysNeeded += 7;
        // DayOfWeek 索引值从 0 开始
        var resultedDay = daysNeeded + 1 + 7 * (occurrence - 1);
        if (resultedDay > DateTime.DaysInMonth(year, month))
            return DateTime.MinValue;
        return Create(year, month, resultedDay);
    }
}