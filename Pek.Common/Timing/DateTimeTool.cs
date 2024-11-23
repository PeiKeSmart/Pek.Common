namespace Pek.Timing;

public class DateTimeTool
{
    private static readonly DateTimeTool sDTTool = new();

    private DateTimeTool()
    {
    }

    public static DateTimeTool New()
    {
        return sDTTool;
    }

    public Int64 GetUtcTimestamp10()
    {
        var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
        return (Int64)ts.TotalSeconds;
    }

    public Int64 GetUtcTimestamp13()
    {
        var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
        return (Int64)ts.TotalMilliseconds;
    }

    public DateTime GetUtcDateTimeFromUtcTimestamp10(Int64 ts)
    {
        var st = new DateTime(1970, 1, 1, 0, 0, 0);
        return st.AddSeconds(ts);
    }

    public DateTime GetUtcDateTimeFromUtcTimestamp13(Int64 ts)
    {
        var st = new DateTime(1970, 1, 1, 0, 0, 0);
        return st.AddMilliseconds(ts);
    }

    public DateTime GetLocalDateTimeFromUtcTimestamp10(Int64 ts)
    {
        var st = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(1970, 1, 1, 0, 0, 0), TimeZoneInfo.Local);
        return st.AddSeconds(ts);
    }

    public DateTime GetLocalDateTimeFromUtcTimestamp13(Int64 ts)
    {
        var st = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(1970, 1, 1, 0, 0, 0), TimeZoneInfo.Local);
        return st.AddMilliseconds(ts);
    }

    public TimeSpan? GetTimeSpanFromString(String timespan, Boolean throwError = true)
    {
        if (!String.IsNullOrWhiteSpace(timespan))
        {
            Int32 value;
            var trimedTime = timespan.Trim().ToLower();
            if (trimedTime.EndsWith("ms"))
            {
                trimedTime = trimedTime[..^2];
                if (Int32.TryParse(trimedTime, out value))
                {
                    return TimeSpan.FromMilliseconds(value);
                }
            }
            else if (trimedTime.EndsWith("s"))
            {
                trimedTime = trimedTime[..^1];
                if (Int32.TryParse(trimedTime, out value))
                {
                    return TimeSpan.FromSeconds(value);
                }
            }
            else if (trimedTime.EndsWith("m"))
            {
                trimedTime = trimedTime.Substring(0, trimedTime.Length - 1);
                if (Int32.TryParse(trimedTime, out value))
                {
                    return TimeSpan.FromMinutes(value);
                }
            }
            else if (trimedTime.EndsWith("h"))
            {
                trimedTime = trimedTime[..^1];
                if (Int32.TryParse(trimedTime, out value))
                {
                    return TimeSpan.FromHours(value);
                }
            }
            else if (trimedTime.EndsWith("d"))
            {
                trimedTime = trimedTime[..^1];
                if (Int32.TryParse(trimedTime, out value))
                {
                    return TimeSpan.FromDays(value);
                }
            }
            else
            {
                if (Int32.TryParse(trimedTime, out value))
                {
                    return TimeSpan.FromSeconds(value);
                }
            }
        }

        if (throwError)
        {
            throw new Exception("不识别的时间范围：" + timespan);
        }
        else
        {
            return null;
        }
    }

    public Boolean CheckStringTimeSpan(String timespan, Boolean throwError = true)
    {
        var result = true;

        if (!String.IsNullOrWhiteSpace(timespan))
        {
            Int32 value;
            var trimedTime = timespan.Trim().ToLower();
            if (trimedTime.EndsWith("ms"))
            {
                trimedTime = trimedTime[..^2];
                result = Int32.TryParse(trimedTime, out value);
            }
            else if (trimedTime.EndsWith("s") || trimedTime.EndsWith("m") ||
                trimedTime.EndsWith("h") || trimedTime.EndsWith("d"))
            {
                trimedTime = trimedTime[..^1];
                result = Int32.TryParse(trimedTime, out value);
            }
            else
            {
                result = Int32.TryParse(trimedTime, out value);
            }

            if (!true)
            {
                if (throwError)
                {
                    throw new Exception(String.Concat("错误的时间段格式，单位仅支持ms、s、m、h、d：", timespan));
                }
            }
            else
            {
                if (value < 0)
                {
                    if (throwError)
                    {
                        throw new Exception(String.Concat("时间段必须大于等于0：", timespan));
                    }
                    else
                    {
                        result = false;
                    }
                }
            }
        }
        else
        {
            if (throwError)
            {
                throw new Exception("时间段不能设置空值。");
            }
            else
            {
                result = false;
            }
        }

        return true;
    }
}