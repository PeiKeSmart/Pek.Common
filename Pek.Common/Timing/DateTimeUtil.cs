﻿using System.Globalization;

namespace Pek.Timing;

/// <summary>
/// 时间操作辅助类
/// </summary>
public static class DateTimeUtil
{
    public static readonly DateTime MinValue = new(1900, 1, 1);

    public static readonly DateTime MaxValue = DateTime.MaxValue;

    #region GetDays(获取总天数)

    /// <summary>
    /// 获取指定年的总天数
    /// </summary>
    /// <param name="year">指定年</param>
    public static Int32 GetDays(Int32 year) => GetDays(year, CultureInfo.CurrentCulture);

    /// <summary>
    /// 获取指定年的总天数，使用指定区域性
    /// </summary>
    /// <param name="year">指定年</param>
    /// <param name="culture">指定区域性</param>
    public static Int32 GetDays(Int32 year, CultureInfo culture)
    {
        var first = new DateTime(year, 1, 1, culture.Calendar);
        var last = new DateTime(year + 1, 1, 1, culture.Calendar);
        return GetDays(first, last);
    }

    /// <summary>
    /// 获取指定时间的年的总天数
    /// </summary>
    /// <param name="date">指定时间</param>
    public static Int32 GetDays(DateTime date) => GetDays(date.Year, CultureInfo.CurrentCulture);

    /// <summary>
    /// 获取两个时间之间的天数
    /// </summary>
    /// <param name="fromDate">开始时间</param>
    /// <param name="toDate">结束时间</param>
    public static Int32 GetDays(DateTime fromDate, DateTime toDate) => Convert.ToInt32(toDate.Subtract(fromDate).TotalDays);

    #endregion

    #region CalculateAge(计算年龄)

    /// <summary>
    /// 计算年龄
    /// </summary>
    /// <param name="dateOfBirth">出生日期</param>
    public static Int32 CalculateAge(DateTime dateOfBirth) => CalculateAge(dateOfBirth, DateTime.Now.Date);

    /// <summary>
    /// 计算年龄，指定参考日期
    /// </summary>
    /// <param name="dateOfBirth">出生日期</param>
    /// <param name="referenceDate">参考日期</param>
    public static Int32 CalculateAge(DateTime dateOfBirth, DateTime referenceDate)
    {
        var years = referenceDate.Year - dateOfBirth.Year;
        if (referenceDate.Month < dateOfBirth.Month ||
            (referenceDate.Month == dateOfBirth.Month && referenceDate.Day < dateOfBirth.Day))
            --years;
        return years;
    }

    #endregion

    #region BusinessDateFormat(业务时间格式化)

    /// <summary>
    /// 业务时间格式化，返回:大于60天-"yyyy-MM-dd",31~60天-1个月前，15~30天-2周前,8~14天-1周前,1~7天-x天前 ,大于1小时-x小时前,x秒前
    /// </summary>
    /// <param name="dateTime">时间</param>
    public static String BusinessDateFormat(DateTime dateTime)
    {
        var span = (DateTime.Now - dateTime).Duration();
        if (span.TotalDays > 60)
        {
            return dateTime.ToString("yyyy-MM-dd");
        }
        if (span.TotalDays > 30)
        {
            return "1个月前";
        }
        if (span.TotalDays > 14)
        {
            return "2周前";
        }
        if (span.TotalDays > 7)
        {
            return "1周前";
        }
        if (span.TotalDays > 1)
        {
            return $"{(Int32)Math.Floor(span.TotalDays)}天前";
        }
        if (span.TotalHours > 1)
        {
            return $"{(Int32)Math.Floor(span.TotalHours)}小时前";
        }
        if (span.TotalMinutes > 1)
        {
            return $"{(Int32)Math.Floor(span.TotalMinutes)}分钟前";
        }
        return "1秒前";
    }

    /// <summary>
    /// 获取时间字符串(小于5分-刚刚、5~60分-x分钟前、1~24小时-x小时前、1~60天-x天前、yyyy-MM-dd HH:mm:ss)
    /// </summary>
    /// <param name="dt"></param>
    /// <param name="defaultFormat"></param>
    public static String BusinessDateFormat(DateTime dt, String defaultFormat = "yyyy-MM-dd HH:mm:ss")
    {
        var timeSpan = DateTime.Now - dt;
        String? result;
        if (timeSpan.TotalMinutes < 5)
            result = String.Format("刚刚");
        else if (timeSpan.TotalMinutes < 60)
            result = String.Format("{0}分钟前", (Int32)timeSpan.TotalMinutes);
        else if (timeSpan.TotalMinutes < 60 * 24)
            result = String.Format("{0}小时前", (Int32)timeSpan.TotalHours);
        else if (timeSpan.TotalMinutes <= 60 * 24 * 7)
            result = String.Format("{0}天前", (Int32)timeSpan.TotalDays);
        else
            result = dt.ToString(defaultFormat);

        return result;
    }

    #endregion

    #region GetWeekDay(计算当前为星期几)

    /// <summary>
    /// 根据当前日期确定当前是星期几
    /// </summary>
    /// <param name="strDate">The String date.</param>
    /// <returns>System.String.</returns>
    /// <exception cref="System.Exception"></exception>
    public static DayOfWeek GetWeekDay(String strDate)
    {
        try
        {
            //需要判断的时间
            var dTime = Convert.ToDateTime(strDate);
            return GetWeekDay(dTime);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    /// <summary>
    /// 根据当前日期确定当前是星期几
    /// </summary>
    /// <param name="dTime">The d time.</param>
    /// <returns>System.String.</returns>
    /// <exception cref="System.Exception"></exception>
    public static DayOfWeek GetWeekDay(DateTime dTime)
    {
        try
        {
            //确定星期几
            var index = (Int32)dTime.DayOfWeek;
            return GetWeekDay(index);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    /// <summary>
    /// 转换星期的表示方法
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>System.String.</returns>
    private static DayOfWeek GetWeekDay(Int32 index) => (DayOfWeek)index;

    /// <summary>
    /// 转换星期的表示方法
    /// </summary>
    /// <param name="dayOfWeek">The index.</param>
    /// <returns>System.String.</returns>
    public static String GetChineseWeekDay(DayOfWeek dayOfWeek)
    {
        var retVal = String.Empty;

        switch (dayOfWeek)
        {
            case DayOfWeek.Sunday:
                retVal = "星期日";
                break;

            case DayOfWeek.Monday:
                retVal = "星期一";
                break;

            case DayOfWeek.Tuesday:
                retVal = "星期二";
                break;

            case DayOfWeek.Wednesday:
                retVal = "星期三";
                break;

            case DayOfWeek.Thursday:
                retVal = "星期四";
                break;

            case DayOfWeek.Friday:
                retVal = "星期五";
                break;

            case DayOfWeek.Saturday:
                retVal = "星期六";
                break;

            default:
                break;
        }

        return retVal;
    }

    #endregion

    #region GetMaxWeekOfYear(计算当前年的最大周数)

    /// <summary>
    /// 获取当前年的最大周数
    /// </summary>
    /// <param name="year">The year.</param>
    /// <returns>System.Int32.</returns>
    /// <exception cref="System.Exception"></exception>
    public static Int32 GetMaxWeekOfYear(Int32 year)
    {
        try
        {
            var tempDate = new DateTime(year, 12, 31);
            var tempDayOfWeek = (Int32)tempDate.DayOfWeek;
            if (tempDayOfWeek != 0)
            {
                tempDate = tempDate.Date.AddDays(-tempDayOfWeek);
            }
            return GetWeekIndex(tempDate);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    /// <summary>
    /// 获取当前年的最大周数
    /// </summary>
    /// <param name="dTime">The d time.</param>
    /// <returns>System.Int32.</returns>
    /// <exception cref="System.Exception"></exception>
    public static Int32 GetMaxWeekOfYear(DateTime dTime)
    {
        try
        {
            return GetMaxWeekOfYear(dTime.Year);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    #endregion

    #region GetWeekIndex(计算当前是第几周)

    /// <summary>
    /// 根据时间获取当前是第几周
    /// </summary>
    /// <param name="dTime">The d time.</param>
    /// <returns>System.Int32.</returns>
    /// <exception cref="System.Exception"></exception>
    public static Int32 GetWeekIndex(DateTime dTime)
    {
        //如果12月31号与下一年的1月1好在同一个星期则算下一年的第一周
        try
        {
            //确定此时间在一年中的位置
            var dayOfYear = dTime.DayOfYear;

            //当年第一天
            var tempDate = new DateTime(dTime.Year, 1, 1);

            //确定当年第一天
            var tempDayOfWeek = (Int32)tempDate.DayOfWeek;
            tempDayOfWeek = tempDayOfWeek == 0 ? 7 : tempDayOfWeek;

            //确定星期几
            var index = (Int32)dTime.DayOfWeek;
            index = index == 0 ? 7 : index;

            //当前周的范围
            var retStartDay = dTime.AddDays(-(index - 1));
            var retEndDay = dTime.AddDays(7 - index);

            //确定当前是第几周
            var weekIndex = (Int32)Math.Ceiling(((Double)dayOfYear + tempDayOfWeek - 1) / 7);

            if (retStartDay.Year < retEndDay.Year)
            {
                weekIndex = 1;
            }

            return weekIndex;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    /// <summary>
    /// 根据时间获取当前是第几周
    /// </summary>
    /// <param name="strDate">The String date.</param>
    /// <returns>System.Int32.</returns>
    /// <exception cref="System.Exception"></exception>
    public static Int32 GetWeekIndex(String strDate)
    {
        try
        {
            //需要判断的时间
            var dTime = Convert.ToDateTime(strDate);
            return GetWeekIndex(dTime);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    #endregion

    #region GetWeekRange(计算周范围)

    /// <summary>
    /// 根据时间取周的日期范围
    /// </summary>
    /// <param name="strDate">The String date.</param>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>System.String.</returns>
    /// <exception cref="System.Exception"></exception>
    public static void GetWeekRange(String strDate, out DateTime startDate, out DateTime endDate)
    {
        try
        {
            //需要判断的时间
            var dTime = Convert.ToDateTime(strDate);
            GetWeekRange(dTime, out startDate, out endDate);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    /// <summary>
    /// 根据时间取周的日期范围
    /// </summary>
    /// <param name="dTime">The d time.</param>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>System.String.</returns>
    /// <exception cref="System.Exception"></exception>
    public static void GetWeekRange(DateTime dTime, out DateTime startDate, out DateTime endDate)
    {
        try
        {
            var index = (Int32)dTime.DayOfWeek;
            index = index == 0 ? 7 : index;

            startDate = dTime.AddDays(-(index - 1));
            endDate = dTime.AddDays(7 - index);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    /// <summary>
    /// 根据时间取周的日期范围
    /// </summary>
    /// <param name="year">The year.</param>
    /// <param name="weekIndex">Index of the week.</param>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>System.String.</returns>
    /// <exception cref="System.Exception">
    /// 请输入大于0的整数
    /// or
    /// 今年没有第 + weekIndex + 周。
    /// or
    /// </exception>
    public static void GetWeekRange(Int32 year, Int32 weekIndex, out DateTime startDate, out DateTime endDate)
    {
        if (weekIndex < 1)
        {
            throw new Exception("请输入大于0的整数");
        }

        var allDays = (weekIndex - 1) * 7;

        //确定当年第一天
        var firstDate = new DateTime(year, 1, 1);
        var firstDayOfWeek = (Int32)firstDate.DayOfWeek;
        firstDayOfWeek = firstDayOfWeek == 0 ? 7 : firstDayOfWeek;

        //周开始日
        var startAddDays = allDays + (1 - firstDayOfWeek);
        var weekRangeStart = firstDate.AddDays(startAddDays);

        //周结束日
        var endAddDays = allDays + (7 - firstDayOfWeek);
        var weekRangeEnd = firstDate.AddDays(endAddDays);

        if (weekRangeStart.Year > year ||
         (weekRangeStart.Year == year && weekRangeEnd.Year > year))
        {
            throw new Exception("今年没有第" + weekIndex + "周。");
        }

        startDate = weekRangeStart;
        endDate = weekRangeEnd;
    }

    /// <summary>
    /// 根据时间取周的日期范围
    /// </summary>
    /// <param name="weekIndex">Index of the week.</param>
    /// <param name="startDate">输出开始日期</param>
    /// <param name="endDate">输出结束日期</param>
    /// <returns>System.String.</returns>
    /// <exception cref="System.Exception"></exception>
    public static void GetWeekRange(Int32 weekIndex, out DateTime startDate, out DateTime endDate)
    {
        try
        {
            GetWeekRange(DateTime.Now.Year, weekIndex, out startDate, out endDate);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    #endregion

    #region GetDateRange(计算当前时间范围)

    /// <summary>
    /// 获取当前的时间范围
    /// </summary>
    /// <param name="range"></param>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    public static void GetDateRange(DateRangeEnum range, out DateTime startDate, out DateTime endDate) => GetDateRange(DateTime.Now, range, out startDate, out endDate);

    /// <summary>
    /// 获取当前时间范围
    /// </summary>
    /// <param name="date">当前日期</param>
    /// <param name="range">日期范围</param>
    /// <param name="startDate">输出开始日期</param>
    /// <param name="endDate">输出结束日期</param>
    public static void GetDateRange(DateTime date, DateRangeEnum range, out DateTime startDate, out DateTime endDate)
    {
        switch (range)
        {
            case DateRangeEnum.Week:

                startDate = date.AddDays(-(Int32)date.DayOfWeek).Date;
                endDate = date.AddDays(6 - (Int32)date.DayOfWeek + 1).Date.AddSeconds(-1);
                break;

            case DateRangeEnum.Month:
                startDate = new DateTime(date.Year, date.Month, 1);
                endDate = startDate.AddMonths(1).Date.AddSeconds(-1);
                break;

            case DateRangeEnum.Quarter:
                if (date.Month <= 3)
                {
                    startDate = new DateTime(date.Year, 1, 1);
                }
                else if (date.Month <= 6)
                {
                    startDate = new DateTime(date.Year, 4, 1);
                }
                else if (date.Month <= 9)
                {
                    startDate = new DateTime(date.Year, 7, 1);
                }
                else
                {
                    startDate = new DateTime(date.Year, 10, 1);
                }
                endDate = startDate.AddMonths(3).AddSeconds(-1);
                break;

            case DateRangeEnum.HalfYear:
                if (date.Month <= 6)
                {
                    startDate = new DateTime(date.Year, 1, 1);
                }
                else
                {
                    startDate = new DateTime(date.Year, 7, 1);
                }
                endDate = startDate.AddMonths(6).AddSeconds(-1);
                break;

            case DateRangeEnum.Year:
                startDate = new DateTime(date.Year, 1, 1);
                endDate = startDate.AddYears(1).AddSeconds(-1);
                break;

            default:
                startDate = DateTime.MinValue;
                endDate = DateTime.MinValue;
                break;
        }
    }

    #endregion
    #region 格式化日期时间,0(yyyy-MM-dd),1(yyyy-MM-dd HH:mm:ss),2(yyyy/MM/dd),3(yyyy年MM月dd日),4(MM-dd),5(MM/dd),6(MM月dd日),7(yyyy-MM),8(yyyy/MM),9(yyyy年MM月),10(HH:mm:ss),11(yyyy-MM-dd HH:mm)
    /// <summary>
    /// 格式化日期时间,0(yyyy-MM-dd),1(yyyy-MM-dd HH:mm:ss),2(yyyy/MM/dd),3(yyyy年MM月dd日),4(MM-dd),5(MM/dd),6(MM月dd日),7(yyyy-MM),8(yyyy/MM),9(yyyy年MM月),10(HH:mm:ss),11(yyyy-MM-dd HH:mm)
    /// </summary>
    /// <param name="dateMode">显示模式</param>
    /// <returns>0-9种模式的日期</returns>
    public static String GetNow(Int32 dateMode) => FormatDate(DateTime.Now, dateMode);

    /// <summary>
    /// 格式化日期时间,0(yyyy-MM-dd),1(yyyy-MM-dd HH:mm:ss),2(yyyy/MM/dd),3(yyyy年MM月dd日),4(MM-dd),5(MM/dd),6(MM月dd日),7(yyyy-MM),8(yyyy/MM),9(yyyy年MM月),10(HH:mm:ss),11(yyyy-MM-dd HH:mm)
    /// </summary>
    /// <param name="dateTime">日期时间</param>
    /// <param name="dateMode">显示模式</param>
    /// <returns>0-9种模式的日期</returns>
    public static String FormatDate(this DateTime dateTime, Int32 dateMode)
    {
        if (dateTime.IsNull()) return "";
        switch (dateMode)
        {
            case 0:
                return dateTime.ToString("yyyy-MM-dd");

            case 1:
                return dateTime.ToString("yyyy-MM-dd HH:mm:ss");

            case 2:
                return dateTime.ToString("yyyy/MM/dd");

            case 3:
                return dateTime.ToString("yyyy年MM月dd日");

            case 4:
                return dateTime.ToString("MM-dd");

            case 5:
                return dateTime.ToString("MM/dd");

            case 6:
                return dateTime.ToString("MM月dd日");

            case 7:
                return dateTime.ToString("yyyy-MM");

            case 8:
                return dateTime.ToString("yyyy/MM");

            case 9:
                return dateTime.ToString("yyyy年MM月");

            case 10:
                return dateTime.ToString("HH:mm:ss");
            case 11:
                return dateTime.ToString("yyyy-MM-dd HH:mm");
            default:
                break;
        }

        return dateTime.ToString();
    }
    #endregion
    #region PHP时间转换
    /// <summary>
    /// PHP时间值
    /// </summary>
    /// <returns></returns>
    public static Int64 PHP_Time()
    {
        var time = new DateTime(0x7b2, 1, 1);
        return ((DateTime.UtcNow.Ticks - time.Ticks) / 0x989680L);
    }

    /// <summary>
    /// PHP时间转移为普通时间
    /// </summary>
    /// <returns></returns>
    public static DateTime PHPTOCTime(Int64 time)
    {
        var timeStamp = new DateTime(1970, 1, 1);  //得到1970年的时间戳
        var t = (time + 8 * 60 * 60) * 10000000 + timeStamp.Ticks;
        var dt = new DateTime(t);
        return dt;
    }
    #endregion

    #region 根据时间来提醒打招呼
    /// <summary>
    /// 根据时间来提醒打招呼
    /// </summary>
    /// <returns></returns>
    public static String RemindTime()
    {
        var date = DateTime.Now.Hour;
        if (date >= 23 || date < 5)
        {
            return "凌晨好！";
        }
        else if (date >= 5 && date < 11)
        {
            return "上午好！";
        }
        else if (date >= 11 && date < 14)
        {
            return "中午好！";
        }
        else if (date >= 14 && date < 19)
        {
            return "下午好！";
        }
        else
        {
            return "晚上好！";
        }
    }
    #endregion

    #region 日期比较
    /// <summary>
    /// 日期比较
    /// </summary>
    /// <param name="today">距离某个日期</param>
    /// <param name="writeDate">输入日期</param>
    /// <param name="n">比较天数</param>
    /// <returns>大于天数返回true，小于返回false</returns>
    public static Boolean CompareDate(String today, String writeDate, Int32 n)
    {
        var Today = Convert.ToDateTime(today);
        var WriteDate = Convert.ToDateTime(writeDate);
        WriteDate = WriteDate.AddDays(n);
        if (Today >= WriteDate)
            return false;
        else
            return true;
    }
    #endregion

    #region 转换日期值与今天的时间段
    /// <summary>
    /// 转换日期值与今天的时间段
    /// </summary>
    /// <param name="Date">日期值</param>
    /// <returns>时间段,N天N小时N分钟N秒</returns>
    public static String ToTimeSpan(DateTime Date)
    {
        var span = (TimeSpan)(DateTime.Now - Date);
        if (span.TotalDays > 1.0)
        {
            return (Math.Round(span.TotalDays).ToString() + "天");
        }
        if (span.TotalHours > 1.0)
        {
            return (Math.Round(span.TotalHours).ToString() + "小时");
        }
        if (span.TotalMinutes > 1.0)
        {
            return (Math.Round(span.TotalMinutes).ToString() + "分钟");
        }
        return (Math.Round(span.TotalSeconds).ToString() + "秒");
    }

    #endregion

    #region 判断日期是否过期
    /// <summary>
    /// 判断日期是否过期
    /// </summary>
    /// <param name="myDate">所要判断的日期</param>
    public static Boolean ValidDate(String myDate)
    {
        if (!myDate.IsDateTime())
            return true;
        return CompareDate(myDate, DateTime.Now.Date.ToString(), 0);
    }
    #endregion

    #region 时间转换为字符格式
    /// <summary>
    /// 时间字符串转换为字符格式yyyy-MM-dd
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static String CDateString(String input)
    {
        if (!DateTime.TryParse(input, out var time))
        {
            return String.Empty;
        }
        return time.ToString("yyyy-MM-dd");
    }

    /// <summary>
    /// 格式化DateTime类型为字符串类型，精确到天，如：2008/01/01
    /// </summary>
    /// <param name="dateTime">要格式化的时间变量</param>
    /// <returns></returns>
    public static String ConvertToDayString(DateTime dateTime)
    {
        if (String.IsNullOrWhiteSpace(dateTime.ToString()))
        {
            return "";
        }
        return dateTime.ToString(@"yyyy\/MM\/dd");
    }

    /// <summary>
    /// 格式化object类型为字符串类型，精确到天，如：2008/01/01
    /// </summary>
    /// <param name="dateTime">要格式化的object</param>
    /// <returns></returns>
    public static String ConvertToDayString(Object dateTime)
    {
        if (String.IsNullOrWhiteSpace(dateTime.ToString()))
        {
            return "";
        }
        return ConvertToDayString((DateTime)dateTime);
    }

    /// <summary>
    /// 格式化DateTime类型为字符串类型，精确到小时，如：2008/01/01 18
    /// </summary>
    /// <param name="dateTime">要格式化的时间变量</param>
    /// <returns></returns>
    public static String ConvertToHourString(DateTime dateTime)
    {
        if (String.IsNullOrWhiteSpace(dateTime.ToString()))
        {
            return "";
        }
        return dateTime.ToString(@"yyyy\/MM\/dd HH");
    }

    /// <summary>
    /// 格式化object类型为字符串类型，精确到小时，如：2008/01/01 18
    /// </summary>
    /// <param name="dateTime">要格式化的object</param>
    /// <returns></returns>
    public static String ConvertToHourString(Object dateTime)
    {
        if (String.IsNullOrWhiteSpace(dateTime.ToString()))
        {
            return "";
        }
        return ConvertToHourString((DateTime)dateTime);
    }

    /// <summary>
    /// 格式化DateTime类型为字符串类型，精确到分钟，如：2008/01/01 18:09
    /// </summary>
    /// <param name="dateTime">要格式化的时间变量</param>
    /// <returns></returns>
    public static String ConvertToMiniuteString(DateTime dateTime)
    {
        if (String.IsNullOrWhiteSpace(dateTime.ToString()))
        {
            return "";
        }
        return dateTime.ToString(@"yyyy\/MM\/dd HH:mm");
    }

    /// <summary>
    /// 格式化object类型为字符串类型，精确到分钟，如：2008/01/01 18:09
    /// </summary>
    /// <param name="dateTime">要格式化的object</param>
    /// <returns></returns>
    public static String ConvertToMiniuteString(Object dateTime)
    {
        if (String.IsNullOrWhiteSpace(dateTime.ToString()))
        {
            return "";
        }
        return ConvertToMiniuteString((DateTime)dateTime);
    }

    /// <summary>
    /// 格式化DateTime类型为字符串类型，精确到秒，如：2008/01/01 18:09:20
    /// </summary>
    /// <param name="dateTime">要格式化的时间变量</param>
    /// <returns></returns>
    public static String ConvertToSecondString(DateTime dateTime)
    {
        if (String.IsNullOrWhiteSpace(dateTime.ToString()))
        {
            return "";
        }
        return dateTime.ToString(@"yyyy\/MM\/dd HH:mm:ss");
    }

    /// <summary>
    /// 格式化object类型为字符串类型，精确到秒，如：2008/01/01 18:09:20
    /// </summary>
    /// <param name="dateTime">要格式化的object</param>
    /// <returns></returns>
    public static String ConvertToSecondString(Object dateTime)
    {
        if (String.IsNullOrWhiteSpace(dateTime.ToString()))
        {
            return "";
        }
        return ConvertToSecondString((DateTime)dateTime);
    }

    #endregion

    #region 把秒转换成分钟
    /// <summary>
    /// 把秒转换成分钟
    /// </summary>
    /// <returns></returns>
    public static Int32 SecondToMinute(Int32 Second)
    {
        var mm = Second / (Decimal)60;
        return Convert.ToInt32(Math.Ceiling(mm));
    }
    #endregion

    #region 返回某年某月最后一天
    /// <summary>
    /// 返回某年某月最后一天
    /// </summary>
    /// <param name="year">年份</param>
    /// <param name="month">月份</param>
    /// <returns>日</returns>
    public static Int32 GetMonthLastDate(Int32 year, Int32 month)
    {
        var lastDay = new DateTime(year, month, new GregorianCalendar().GetDaysInMonth(year, month));
        var Day = lastDay.Day;
        return Day;
    }
    #endregion

    #region 获取一个月有多少天！
    /// <summary>
    /// 获取一个月有多少天！
    /// </summary>
    /// <param name="iYear"></param>
    /// <param name="iMonth"></param>
    /// <returns></returns>
    public static Int32 GetMonthLen(String iYear, String iMonth) => GetMonthLen(Int32.Parse(iYear), Int32.Parse(iMonth));

    /// <summary>
    /// 获取一个月有多少天！
    /// </summary>
    /// <param name="iYear"></param>
    /// <param name="iMonth"></param>
    /// <returns></returns>
    public static Int32 GetMonthLen(Int32 iYear, Int32 iMonth) => DateTime.DaysInMonth(iYear, iMonth);

    #endregion

    #region 获取一个月多少个周末日
    /// <summary>
    /// 获取一个月多少个周末日
    /// </summary>
    /// <param name="iYear"></param>
    /// <param name="iMonth"></param>
    /// <returns></returns>
    public static Int32 GetWeekLen(String iYear, String iMonth)
    {
        var days = GetMonthLen(iYear, iMonth);
        Int32 i;
        var num1 = 0;
        for (i = 1; i < days + 1; i++)
        {
            var num = (Int32)(iYear + "-" + iMonth + "-" + i).ToDate().DayOfWeek;
            if (num == 0 | num == 6)
            {
                num1 += 1;
            }
        }
        return num1;
    }

    #endregion

    #region 取指定日期是一年中的第几周
    /// <summary>
    /// 取指定日期是一年中的第几周
    /// </summary>
    /// <param name="dtime">给定的日期</param>
    /// <returns>数字 一年中的第几周</returns>
    public static Int32 Weekofyear(DateTime dtime)
    {
        var weeknum = 0;
        var tmpdate = DateTime.Parse(dtime.Year.ToString() + "-1" + "-1");
        var firstweek = tmpdate.DayOfWeek;
        //if(firstweek) 
        for (var i = (Int32)firstweek + 1; i <= dtime.DayOfYear; i += 7)
        {
            weeknum++;
        }
        return weeknum;
    }
    #endregion

    #region 获取当前星期的第一天，i为以星期几天周期与星期日的差值
    /// <summary>
    /// 获取当前星期的第一天，i为以星期几天周期与星期日的差值
    /// </summary>
    /// <param name="dtime">日期</param>
    /// <param name="i">i为以星期几天周期与星期日的差值</param>
    /// <returns></returns>
    public static DateTime Startweekday(DateTime dtime, Int32 i) => dtime.AddDays(Convert.ToDouble((0 - Convert.ToInt16(dtime.DayOfWeek) - i)));
    #endregion

    #region 获取当前星期的最后一天,i为以星期几天周期与星期日的差值.
    /// <summary>
    /// 获取当前星期的最后一天,i为以星期几天周期与星期日的差值.
    /// </summary>
    /// <param name="dtime"></param>
    /// <param name="i"></param>
    /// <returns></returns>
    public static DateTime Endweekday(DateTime dtime, Int32 i) => dtime.AddDays(Convert.ToDouble(6 - Convert.ToInt16(dtime.DayOfWeek) - i));
    #endregion

    #region 获取当前星期的周一

    /// <summary>
    /// 获取当前星期的周一
    /// </summary>
    /// <returns></returns>
    public static DateTime GetMonday()
    {
        var now = DateTime.Now;
        var temp = new DateTime(now.Year, now.Month, now.Day);
        var count = now.DayOfWeek - DayOfWeek.Monday;
        if (count == -1) count = 6;

        return temp.AddDays(-count);
    }

    #endregion

    #region 获取当前星期的周天

    /// <summary>
    /// 获取当前星期的周天
    /// </summary>
    /// <returns></returns>
    public static DateTime GetSunday()
    {
        var now = DateTime.Now;
        var temp = new DateTime(now.Year, now.Month, now.Day);
        var count = now.DayOfWeek - DayOfWeek.Sunday;
        if (count != 0) count = 7 - count;

        return temp.AddDays(count);
    }

    #endregion

    #region 得到随机日期
    /// <summary>
    /// 得到随机日期
    /// </summary>
    /// <param name="time1">起始日期</param>
    /// <param name="time2">结束日期</param>
    /// <returns></returns>
    public static DateTime GetRandomTime(DateTime time1, DateTime time2)
    {
        var random = new Random();
        var minTime = new DateTime();
        var maxTime = new DateTime();
        var ts = new TimeSpan(time1.Ticks - time2.Ticks);
        //获取两个时间相隔的秒数
        var dTotalSecontds = ts.TotalSeconds;
        var iTotalSecontds = 0;
        if (dTotalSecontds > Int32.MaxValue)
        {
            iTotalSecontds = Int32.MaxValue;
        }
        else if (dTotalSecontds < Int32.MinValue)
        {
            iTotalSecontds = Int32.MinValue;
        }
        else
        {
            iTotalSecontds = Int32.Parse(dTotalSecontds.ToString());
        }
        if (iTotalSecontds > 0)
        {
            minTime = time2;
            maxTime = time1;
        }
        else if (iTotalSecontds < 0)
        {
            minTime = time1;
            maxTime = time2;
        }
        else
        {
            return time1;
        }
        var maxValue = iTotalSecontds;
        if (iTotalSecontds <= Int32.MinValue)
        {
            maxValue = Int32.MinValue + 1;
        }
        var i = random.Next(Math.Abs(maxValue));
        return new DateTime().AddSeconds(i);
    }
    #endregion

    #region 获取今天是星期几
    /// <summary>
    /// 获取今天是星期几
    /// </summary>
    /// <param name="ints">0为英文星期，1为中文星期</param>
    /// <returns></returns>
    public static String GetDateweek(Int32 ints)
    {
        if (ints == 0)
        {
            switch (DateTime.Now.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    return "Sun";
                case DayOfWeek.Monday:
                    return "Mon";
                case DayOfWeek.Tuesday:
                    return "Tue";
                case DayOfWeek.Wednesday:
                    return "Wed";
                case DayOfWeek.Thursday:
                    return "Thu";
                case DayOfWeek.Friday:
                    return "Fri";
                case DayOfWeek.Saturday:
                    return "Sat";
            }
        }
        else
        {
            switch (DateTime.Now.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    return "星期天";
                case DayOfWeek.Monday:
                    return "星期一";
                case DayOfWeek.Tuesday:
                    return "星期二";
                case DayOfWeek.Wednesday:
                    return "星期三";
                case DayOfWeek.Thursday:
                    return "星期四";
                case DayOfWeek.Friday:
                    return "星期五";
                case DayOfWeek.Saturday:
                    return "星期六";
            }
        }
        return String.Empty;
    }
    #endregion

    #region 获得月份
    /// <summary>
    /// 获得月份
    /// </summary>
    /// <param name="ints">0为英文简写,1为中文</param>
    /// <returns></returns>
    public static String GetDateMonth(Int32 ints)
    {
        if (ints == 0)
        {
            switch (DateTime.Now.Month)
            {
                case 1:
                    return "Jan";
                case 2:
                    return "Feb";
                case 3:
                    return "Mar";
                case 4:
                    return "Apr";
                case 5:
                    return "May";
                case 6:
                    return "Jun";
                case 7:
                    return "Jul";
                case 8:
                    return "Aug";
                case 9:
                    return "Sep";
                case 10:
                    return "Oct";
                case 11:
                    return "Nov";
                case 12:
                    return "Dec";
            }
        }
        else
        {
            switch (DateTime.Now.Month)
            {
                case 1:
                    return "一月";
                case 2:
                    return "二月";
                case 3:
                    return "三月";
                case 4:
                    return "四月";
                case 5:
                    return "五月";
                case 6:
                    return "六月";
                case 7:
                    return "七月";
                case 8:
                    return "八月";
                case 9:
                    return "九月";
                case 10:
                    return "十月";
                case 11:
                    return "十一月";
                case 12:
                    return "十二月";
            }
        }
        return String.Empty;
    }
    #endregion

    #region 返回相差的秒数
    /// <summary>
    /// 返回相差的秒数
    /// </summary>
    /// <param name="Time"></param>
    /// <param name="Sec"></param>
    /// <returns></returns>
    public static Int32 StrDateDiffSeconds(String Time, Int32 Sec)
    {
        var ts = DateTime.Now - DateTime.Parse(Time).AddSeconds(Sec);
        if (ts.TotalSeconds > Int32.MaxValue)
            return Int32.MaxValue;

        else if (ts.TotalSeconds < Int32.MinValue)
            return Int32.MinValue;

        return (Int32)ts.TotalSeconds;
    }

    /// <summary>
    /// 返回相差的秒数
    /// </summary>
    /// <param name="Time"></param>
    /// <param name="Sec"></param>
    /// <returns></returns>
    public static Int32 StrDateDiffSeconds(DateTime Time, Int32 Sec)
    {
        var ts = Time.AddSeconds(Sec) - DateTime.Now;
        if (ts.TotalSeconds > Int32.MaxValue)
            return Int32.MaxValue;

        else if (ts.TotalSeconds < Int32.MinValue)
            return Int32.MinValue;

        return (Int32)ts.TotalSeconds;
    }
    #endregion

    #region 返回相差的分钟数
    /// <summary>
    /// 返回相差的分钟数
    /// </summary>
    /// <param name="time"></param>
    /// <param name="minutes"></param>
    /// <returns></returns>
    public static Int32 StrDateDiffMinutes(String time, Int32 minutes)
    {
        if (String.IsNullOrEmpty(time))
            return 1;

        var ts = DateTime.Now - DateTime.Parse(time).AddMinutes(minutes);
        if (ts.TotalMinutes > Int32.MaxValue)
            return Int32.MaxValue;
        else if (ts.TotalMinutes < Int32.MinValue)
            return Int32.MinValue;

        return (Int32)ts.TotalMinutes;
    }

    /// <summary>
    /// 返回相差的分钟数
    /// </summary>
    /// <param name="time"></param>
    /// <param name="minutes"></param>
    /// <returns></returns>
    public static Int32 StrDateDiffMinutes(DateTime time, Int32 minutes)
    {
        var ts = DateTime.Now - time.AddMinutes(minutes);
        if (ts.TotalMinutes > Int32.MaxValue)
            return Int32.MaxValue;
        else if (ts.TotalMinutes < Int32.MinValue)
            return Int32.MinValue;

        return (Int32)ts.TotalMinutes;
    }
    #endregion

    #region  返回相差的小时数
    /// <summary>
    /// 返回相差的小时数
    /// </summary>
    /// <param name="time"></param>
    /// <param name="hours"></param>
    /// <returns></returns>
    public static Int32 StrDateDiffHours(String time, Int32 hours)
    {
        if (String.IsNullOrEmpty(time))
            return 1;

        var ts = DateTime.Now - DateTime.Parse(time).AddHours(hours);
        if (ts.TotalHours > Int32.MaxValue)
            return Int32.MaxValue;
        else if (ts.TotalHours < Int32.MinValue)
            return Int32.MinValue;

        return (Int32)ts.TotalHours;
    }

    /// <summary>
    /// 返回相差的小时数
    /// </summary>
    /// <param name="time"></param>
    /// <param name="hours"></param>
    /// <returns></returns>
    public static Int32 StrDateDiffHours(DateTime time, Int32 hours)
    {
        var ts = DateTime.Now - time.AddHours(hours);
        if (ts.TotalHours > Int32.MaxValue)
            return Int32.MaxValue;
        else if (ts.TotalHours < Int32.MinValue)
            return Int32.MinValue;

        return (Int32)ts.TotalHours;
    }
    #endregion

    #region 把两个时间差，三天内的时间用今天，昨天，前天表示，后跟时间，无日期
    /// <summary>
    /// 把两个时间差，三天内的时间用今天，昨天，前天表示，后跟时间，无日期
    /// </summary>
    /// <param name="date">被比较的时间</param>
    /// <param name="currentDateTime">目标时间</param>
    /// <returns></returns>
    public static String ConvertDateTime(String date, DateTime currentDateTime)
    {
        if (String.IsNullOrEmpty(date))
            return "";

        if (!DateTime.TryParse(date, out var time))
            return "";

        var result = "";
        if (DateDiff("hour", time, currentDateTime) <= 3)
        {
            if (DateDiff("hour", time, currentDateTime) > 0)
                return DateDiff("hour", time, currentDateTime) + "小时前";

            if (DateDiff("minute", time, currentDateTime) > 0)
                return DateDiff("minute", time, currentDateTime) + "分钟前";

            if (DateDiff("second", time, currentDateTime) > 0)
                return DateDiff("second", time, currentDateTime) + "秒前";
        }
        else
        {
            result = (currentDateTime.Day - time.Day) switch
            {
                0 => "今天 " + time.ToString("HH") + ":" + time.ToString("mm"),
                1 => "昨天 " + time.ToString("HH") + ":" + time.ToString("mm"),
                2 => "前天 " + time.ToString("HH") + ":" + time.ToString("mm"),
                _ => time.ToString("yyyy-MM-dd HH:mm"),
            };
        }
        return result;
    }
    #endregion

    #region 返回时间差
    /// <summary>
    /// 返回时间差
    /// </summary>
    /// <param name="DateTime1"></param>
    /// <param name="DateTime2"></param>
    /// <returns></returns>
    public static String? DateDiff(DateTimeOffset DateTime1, DateTimeOffset DateTime2)
    {
        String? dateDiff = null;
        try
        {
            var ts = DateTime2 - DateTime1;
            if (ts.Days >= 1)
            {
                dateDiff = DateTime1.Month.ToString() + "月" + DateTime1.Day.ToString() + "日";
            }
            else
            {
                if (ts.Hours > 1)
                {
                    dateDiff = ts.Hours.ToString() + "小时前";
                }
                else
                {
                    dateDiff = ts.Minutes.ToString() + "分钟前";
                }
            }
        }
        catch
        { }
        return dateDiff;
    }

    /// <summary>
    /// 获得两个日期的间隔。
    /// </summary>
    /// <param name="DateTime1">日期一</param>
    /// <param name="Datetime2">日期二</param>
    /// <returns>日期间隔TimeSpan</returns>
    public static TimeSpan DateDiff1(DateTimeOffset DateTime1, DateTimeOffset Datetime2)
    {
        var span = new TimeSpan(DateTime1.Ticks);
        var ts = new TimeSpan(Datetime2.Ticks);
        return span.Subtract(ts).Duration();
    }
    #endregion

    #region 两个时间的差值，可以为秒，小时，天，分钟
    /// <summary>
    /// 两个时间的差值，可以为秒，小时，天，分钟
    /// </summary>
    /// <param name="Interval">需要得到的时差方式</param>
    /// <param name="StartDate">起始时间</param>
    /// <param name="EndDate">结束时间</param>
    /// <returns></returns>
    public static Int64 DateDiff(String Interval, DateTimeOffset StartDate, DateTimeOffset EndDate)
    {
        Int64 lngDateDiffValue = 0;
        var TS = new TimeSpan(EndDate.Ticks - StartDate.Ticks);
        switch (Interval)
        {
            case "second":
                lngDateDiffValue = (Int64)TS.TotalSeconds;
                break;
            case "minute":
                lngDateDiffValue = (Int64)TS.TotalMinutes;
                break;
            case "hour":
                lngDateDiffValue = (Int64)TS.TotalHours;
                break;
            case "day":
                lngDateDiffValue = (Int64)TS.Days;
                break;
            case "week":
                lngDateDiffValue = (Int64)(TS.Days / 7);
                break;
            case "month":
                lngDateDiffValue = (Int64)(TS.Days / 30);
                break;
            case "quarter":
                lngDateDiffValue = (Int64)((TS.Days / 30) / 3);
                break;
            case "year":
                lngDateDiffValue = (Int64)(TS.Days / 365);
                break;
        }
        return (lngDateDiffValue);
    }
    #endregion

    #region 把三天内的时间用今天，昨天，前天表示，后跟时间，无日期
    /// <summary>
    /// 把三天内的时间用今天，昨天，前天表示，后跟时间，无日期
    /// </summary>
    /// <param name="date">被转换的时间</param>
    /// <returns></returns>
    public static String ConvertDateTime(String date) => ConvertDateTime(date, DateTime.Now);
    #endregion
}