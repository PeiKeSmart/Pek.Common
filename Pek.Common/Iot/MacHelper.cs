using System.Globalization;
using System.Text.RegularExpressions;

using NewLife;

namespace Pek.Iot;

/// <summary>
/// Mac地址处理工具类
/// </summary>
public class MacHelper
{
    /// <summary>
    /// 获取两个MAC地址之间的数量（包含首尾）
    /// </summary>
    /// <param name="startMac">起始MAC地址</param>
    /// <param name="endMac">结束MAC地址</param>
    /// <returns>数量</returns>
    public static Int64 GetMacAddressCount(String startMac, String endMac)
    {
        var start = ParseMacToLong(startMac);
        var end = ParseMacToLong(endMac);
        if (start > end) throw new ArgumentException("startMac不能大于endMac");
        return end - start + 1;
    }

    /// <summary>
    /// 获取两个MAC地址之间的所有实际MAC地址（包含首尾）
    /// </summary>
    /// <param name="startMac">起始MAC地址</param>
    /// <param name="endMac">结束MAC地址</param>
    /// <returns>MAC地址列表</returns>
    // 默认格式为冒号
    public static List<String> GetMacAddresses(String startMac, String endMac) => GetMacAddresses(startMac, endMac, "colon");

    /// <summary>
    /// 解析MAC地址为long类型
    /// </summary>
    public static Int64 ParseMacToLong(String mac)
    {
        if (String.IsNullOrWhiteSpace(mac)) throw new ArgumentNullException(nameof(mac));
        // 兼容多种格式
        var cleaned = Regex.Replace(mac, "[^0-9A-Fa-f]", "").ToUpperInvariant();
        if (cleaned.Length != 12) throw new FormatException("MAC地址格式不正确");
        return Int64.Parse(cleaned, NumberStyles.HexNumber);
    }

    /// <summary>
    /// long类型转为指定格式的MAC地址
    /// </summary>
    /// <param name="value">MAC地址long值</param>
    /// <param name="format">格式：dash（XX-XX-XX-XX-XX-XX，默认）、colon（XX:XX:XX:XX:XX:XX）、plain（XXXXXXXXXXXX）</param>
    public static String LongToMac(Int64 value, String format)
    {
        var hex = value.ToString("X12");
        return (format ?? "dash").ToLowerInvariant() switch
        {
            "colon" => ToMacFormat(hex, ':'),
            "plain" => hex,
            _ => ToMacFormat(hex, '-'),
        };
    }

    /// <summary>
    /// 获取两个MAC地址之间的所有实际MAC地址（包含首尾，支持格式指定）
    /// </summary>
    /// <param name="startMac">起始MAC地址</param>
    /// <param name="endMac">结束MAC地址</param>
    /// <param name="format">格式：dash、colon、plain</param>
    /// <returns>MAC地址列表</returns>
    public static List<String> GetMacAddresses(String startMac, String endMac, String format)
    {
        var start = ParseMacToLong(startMac);
        var end = ParseMacToLong(endMac);
        if (start > end) throw new ArgumentException("startMac不能大于endMac");
        var list = new List<String>();
        for (var i = start; i <= end; i++)
        {
            list.Add(LongToMac(i, format));
        }
        return list;
    }

    /// <summary>
    /// 将12位无分隔符的十六进制MAC地址字符串格式化为带指定分隔符的MAC地址字符串。
    /// </summary>
    /// <param name="hex">12位无分隔符的十六进制MAC地址字符串 (例如 "001122AABBCC")。</param>
    /// <param name="separator">用于分隔MAC地址中每两个十六进制字符的分隔符 (例如 ':' 或 '-')。</param>
    /// <returns>带指定分隔符的格式化MAC地址字符串 (例如 "00:11:22:AA:BB:CC" 或 "00-11-22-AA-BB-CC")。</returns>
    public static String ToMacFormat(String hex, Char separator)
    {
        var macChars = new Char[17];
        for (var i = 0; i < 6; i++)
        {
            macChars[i * 3] = hex[i * 2];
            macChars[i * 3 + 1] = hex[i * 2 + 1];
            if (i < 5)
                macChars[i * 3 + 2] = separator;
        }
        return new String(macChars);
    }

    /// <summary>
    /// 将任意格式的MAC地址字符串转为冒号格式（XX:XX:XX:XX:XX:XX）
    /// </summary>
    /// <param name="mac">任意格式的MAC地址</param>
    /// <returns>冒号格式MAC地址</returns>
    public static String NormalizeToColonFormat(String mac)
    {
        if (mac.IsNullOrWhiteSpace()) return String.Empty; 
        
        // 移除所有非十六进制字符，保留0-9，A-F，a-f
        var cleaned = Regex.Replace(mac, "[^0-9A-Fa-f]", "").ToUpperInvariant();
        
        // 验证清理后的字符串长度必须是12位
        if (cleaned.Length != 12) 
            throw new FormatException($"MAC地址格式不正确，期望12位十六进制字符，实际得到{cleaned.Length}位");
        
        // 转换为冒号格式
        return ToMacFormat(cleaned, ':');
    }

    /// <summary>
    /// 默认格式为冒号
    /// </summary>
    /// <param name="value">MAC地址long值</param>
    /// <returns>格式化后的MAC地址</returns>
    public static String LongToMac(Int64 value) => LongToMac(value, "colon");
}
