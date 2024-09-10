using NewLife;

namespace System;

/// <summary>
/// 字符串(<see cref="string"/>) 扩展
/// </summary>
public static partial class DHStringExtensions
{
    /// <summary>
    /// 如果给定字符串的结尾不以字符结尾，则将字符添加到该字符串的结尾。
    /// </summary>
    public static string EnsureEndsWith(this string str, char c, StringComparison comparisonType = StringComparison.Ordinal)
    {
        if (str.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(str));

        if (str.EndsWith(c.ToString(), comparisonType))
        {
            return str;
        }

        return str + c;
    }

    /// <summary>
    /// 如果给定字符串的开头不以字符开头，则将字符添加到该字符串的开头。
    /// </summary>
    public static string EnsureStartsWith(this string str, char c, StringComparison comparisonType = StringComparison.Ordinal)
    {
        if (str.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(str));

        if (str.StartsWith(c.ToString(), comparisonType))
        {
            return str;
        }

        return c + str;
    }
}
