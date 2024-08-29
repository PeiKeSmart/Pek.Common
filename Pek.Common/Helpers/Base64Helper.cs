using System.Text;

namespace Pek.Common.Helpers;

/// <summary>
/// Base64帮助类
/// </summary>
public class Base64Helper
{
    /// <summary>
    /// 将Base64编码解析成字符串
    /// </summary>
    /// <param name="strbase">要解码的string字符</param>
    /// <param name="encoding">字符编码方案</param>
    /// <returns></returns>
    public static string Base64ToString(string strbase, Encoding encoding)
    {
        var buff = Convert.FromBase64String(strbase);
        return encoding.GetString(buff);
    }

    /// <summary>
    /// 将Base64编码解析成字符串
    /// </summary>
    /// <param name="strbase">要解码的string字符</param>
    /// <returns></returns>
    public static string Base64ToString(string strbase)
    {
        return Base64ToString(strbase, Encoding.UTF8);
    }

    /// <summary>
    /// 将Base64编码解析成字节数组
    /// </summary>
    /// <param name="strbase">要解码的string字符</param>
    /// <returns></returns>
    public static byte[] Base64ToBytes(string strbase)
    {
        return Convert.FromBase64String(strbase);
    }

    /// <summary>
    /// 将字节数组为Base64编码
    /// </summary>
    /// <param name="bytebase">要编码的byte[]</param>
    /// <returns></returns>
    public static string StringToBase64(byte[] bytebase)
    {
        return Convert.ToBase64String(bytebase);
    }

    /// <summary>
    /// 将字符串转为Base64编码
    /// </summary>
    /// <param name="str">要编码的string字符</param>
    /// <param name="encoding">字符编码方案</param>
    /// <returns></returns>
    public static string StringToBase64(string str, Encoding encoding)
    {
        var buff = encoding.GetBytes(str);
        return Convert.ToBase64String(buff);
    }

    /// <summary>
    /// 将字符串转为Base64编码
    /// </summary>
    /// <param name="str">要编码的string字符</param>
    /// <returns></returns>
    public static string StringToBase64(string str)
    {
        var buff = Encoding.UTF8.GetBytes(str);
        return Convert.ToBase64String(buff);
    }
}