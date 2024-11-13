using Pek.Helpers;

namespace Pek;

/// <summary>
/// 系统扩展 - 类型转换扩展
/// </summary>
public static partial class DHExtensions
{

    #region ToDate(转换为日期)
    /// <summary>
    /// 转换为日期
    /// </summary>
    /// <param name="obj">数据</param>
    public static DateTime ToDate(this string obj) => Conv.ToDGDate(obj);

    /// <summary>
    /// 转换为可空日期
    /// </summary>
    /// <param name="obj">数据</param>
    public static DateTime? ToDateOrNull(this string obj) => Conv.ToDGDateOrNull(obj);
    #endregion

}
