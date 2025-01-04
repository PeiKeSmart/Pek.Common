using System.Runtime.CompilerServices;

using NewLife.Log;

namespace Pek.Log;

/// <summary>
/// 日志类，包含跟踪调试功能
/// </summary>
public static class DTrace
{
    /// <summary>日志提供者，默认使用文本文件日志</summary>
    public static ILog Log { get; } = XTrace.Log;

    /// <summary>输出日志</summary>
    /// <param name="msg">信息</param>
    /// <param name="memberName">方法名</param>
    public static void WriteLine(String msg, [CallerMemberName] String memberName = "") => XTrace.WriteLine($"[{memberName}]:{msg}");

    /// <summary>写日志</summary>
    /// <param name="format"></param>
    /// <param name="args"></param>
    /// <param name="memberName">方法名</param>
    public static void WriteLineF(String format, [CallerMemberName] String memberName = "", params Object?[] args) => XTrace.WriteLine($"[{memberName}]:{format}", args);

    /// <summary>输出异常日志</summary>
    /// <param name="ex">异常信息</param>
    public static void WriteException(Exception ex) => XTrace.WriteException(ex);
}
