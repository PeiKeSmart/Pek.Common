namespace Pek;

/// <summary>
/// 系统扩展 - 类型转换扩展
/// </summary>
public static partial class DHExtensions
{
    #region SafeString(安全转换为字符串)

    /// <summary>
    /// 安全转换为字符串，去除两端空格，当值为null时返回""
    /// </summary>
    /// <param name="input">输入值</param>
    public static String SafeString(this Object? input)
    {
       return input == null ? String.Empty : (input.ToString()?.Trim() ?? String.Empty);
    }

    #endregion
}
