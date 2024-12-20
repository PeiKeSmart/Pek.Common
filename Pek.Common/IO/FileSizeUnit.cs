﻿using System.ComponentModel;

namespace Pek.IO;

/// <summary>
/// 文件大小单位
/// </summary>
public enum FileSizeUnit
{
    /// <summary>
    /// 字节
    /// </summary>
    [Description("B")]
    Byte,

    /// <summary>
    /// K字节
    /// </summary>
    [Description("KB")]
    K,

    /// <summary>
    /// M字节
    /// </summary>
    [Description("MB")]
    M,

    /// <summary>
    /// G字节
    /// </summary>
    [Description("GB")]
    G
}

/// <summary>
/// 文件大小单位枚举扩展
/// </summary>
public static class FileSizeUnitExtensionss
{
    /// <summary>
    /// 获取描述
    /// </summary>
    /// <param name="unit">文件大小单位</param>
    public static String Description(this FileSizeUnit? unit) => unit == null ? String.Empty : unit.Value.Description();

    /// <summary>
    /// 获取值
    /// </summary>
    /// <param name="unit">文件大小单位</param>
    public static Int32? Value(this FileSizeUnit? unit) => unit?.Value();
}