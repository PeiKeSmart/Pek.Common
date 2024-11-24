namespace Pek.Timing;

/// <summary>
/// 时间段
/// </summary>
public class DateTimeRange
{
    public DateTimeRange(DateTime start, DateTime end)
    {
        if (start > end)
        {
            throw new Exception("开始时间不能大于结束时间");
        }

        Start = start;
        End = end;
    }

    /// <summary>
    /// 起始时间
    /// </summary>
    public DateTime Start { get; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime End { get; }

    /// <summary>
    /// 是否相交
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public Boolean HasIntersect(DateTime start, DateTime end) => HasIntersect(new DateTimeRange(start, end));

    /// <summary>
    /// 是否相交
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public Boolean HasIntersect(DateTimeRange range) => Start.In(range.Start, range.End) || End.In(range.Start, range.End);

    /// <summary>
    /// 相交时间段
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public (Boolean intersected, DateTimeRange? range) Intersect(DateTimeRange range)
    {
        if (HasIntersect(range.Start, range.End))
        {
            var list = new List<DateTime>() { Start, range.Start, End, range.End };
            list.Sort();
            return (true, new DateTimeRange(list[1], list[2]));
        }

        return (false, null);
    }

    /// <summary>
    /// 相交时间段
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public (Boolean intersected, DateTimeRange? range) Intersect(DateTime start, DateTime end) => Intersect(new DateTimeRange(start, end));

    /// <summary>
    /// 是否包含时间段
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public Boolean Contains(DateTimeRange range) => range.Start.In(Start, End) && range.End.In(Start, End);

    /// <summary>
    /// 是否包含时间段
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public Boolean Contains(DateTime start, DateTime end) => Contains(new DateTimeRange(start, end));

    /// <summary>
    /// 是否在时间段内
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public Boolean In(DateTimeRange range) => Start.In(range.Start, range.End) && End.In(range.Start, range.End);

    /// <summary>
    /// 是否在时间段内
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public Boolean In(DateTime start, DateTime end) => In(new DateTimeRange(start, end));

    /// <summary>
    /// 合并时间段
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public DateTimeRange Union(DateTimeRange range)
    {
        if (HasIntersect(range))
        {
            var list = new List<DateTime>() { Start, range.Start, End, range.End };
            list.Sort();
            return new DateTimeRange(list[0], list[3]);
        }

        throw new Exception("不相交的时间段不能合并");
    }

    /// <summary>
    /// 合并时间段
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public DateTimeRange Union(DateTime start, DateTime end) => Union(new DateTimeRange(start, end));

    /// <summary>返回一个表示当前对象的 string。</summary>
    /// <returns>表示当前对象的字符串。</returns>
    public override String ToString() => $"{Start:yyyy-MM-dd HH:mm:ss}~{End:yyyy-MM-dd HH:mm:ss}";
}