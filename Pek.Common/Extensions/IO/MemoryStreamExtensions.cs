﻿using System.Text;

namespace Pek;

/// <summary>
/// 内存流(<see cref="MemoryStream"/>) 扩展
/// </summary>
public static class MemoryStreamExtensions
{
    /// <summary>
    /// 转换成字符串输出
    /// </summary>
    /// <param name="ms">内存流</param>
    /// <param name="encoding">字符编码，默认值：UTF-8</param>
    /// <returns></returns>
    public static String AsString(this MemoryStream ms, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        return encoding.GetString(ms.ToArray());
    }

    /// <summary>
    /// 写入字符串到内存流中
    /// </summary>
    /// <param name="ms">内存流</param>
    /// <param name="input">输入值</param>
    /// <param name="encoding">字符编码，默认值：UTF-8</param>
    public static void FromString(this MemoryStream ms, String input, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        var buffer = encoding.GetBytes(input);
        ms.Write(buffer, 0, buffer.Length);
    }
}