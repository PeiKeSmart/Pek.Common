﻿using System.Collections;

namespace Pek.SyntaxHighlighing;

/// <summary>
/// 排序比较。被包含的词置后。比如：api,webapi 输出==> webapi,api， api被包含，排序放后面
/// </summary>
public class SortComapre : IComparer
{
    /// <summary>
    /// 比较
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public Int32 Compare(Object? x, Object? y)
    {
        if (x == null && y == null) return 0;
        if (x == null && y != null) return -1;
        if (x != null && y == null) return 1;

        x = x?.ToString()?.ToLower();
        y = y?.ToString()?.ToLower();

        if (x?.ToString() == y?.ToString())
            return 0;
        else if (x?.ToString()?.Contains(y?.ToString()!) == true)
            return -1;
        else if (y?.ToString()?.Contains(x?.ToString()!) == true)
            return 1;
        else
            return 0;
    }

}