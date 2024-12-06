﻿using System.Diagnostics.CodeAnalysis;

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

    /// <summary>
    /// 检查任何给定的集合对象为空或没有项。
    /// </summary>
    public static bool IsNullOrEmpty<T>(this ICollection<T> source)
    {
        return source == null || source.Count <= 0;
    }

    /// <summary>
    /// 从集合中删除满足给定<paramref name="predicate"/>的所有项。
    /// </summary>
    /// <typeparam name="T">集合中项目的类型</typeparam>
    /// <param name="source">集合</param>
    /// <param name="predicate">删除项目的条件</param>
    /// <returns>删除的项目列表</returns>
    public static IList<T> RemoveAll<T>([NotNull] this ICollection<T> source, Func<T, bool> predicate)
    {
        var items = source.Where(predicate).ToList();

        foreach (var item in items)
        {
            source.Remove(item);
        }

        return items;
    }

    /// <summary>
    /// 从集合中删除所有项。
    /// </summary>
    /// <typeparam name="T">集合中项目的类型</typeparam>
    /// <param name="source">集合</param>
    /// <param name="items">要从列表中删除的项目</param>
    public static void RemoveAll<T>([NotNull] this ICollection<T> source, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            source.Remove(item);
        }
    }
}
