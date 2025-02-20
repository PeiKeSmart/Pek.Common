using NewLife;
using NewLife.Model;

using Pek.Infrastructure;

namespace Pek.Iot;

/// <summary>
/// 进制转换
/// </summary>
public static class HexConv
{
    /// <summary>
    /// 基础字符
    /// </summary>
    private const String BaseChar = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    /// <summary>
    /// 二进制转换为八进制
    /// </summary>
    /// <param name="value">二进制</param>
    public static String BinToOct(String value) => X2X(value, 2, 8);

    /// <summary>
    /// 二进制转换为十进制
    /// </summary>
    /// <param name="value">二进制</param>
    public static String BinToDec(String value) => X2X(value, 2, 10);

    /// <summary>
    /// 二进制转换为十六进制
    /// </summary>
    /// <param name="value">二进制</param>
    public static String BinToHex(String value) => X2X(value, 2, 16);

    /// <summary>
    /// 八进制转换为二进制
    /// </summary>
    /// <param name="value">八进制</param>
    public static String OctToBin(String value) => X2X(value, 8, 2);

    /// <summary>
    /// 八进制转换为十进制
    /// </summary>
    /// <param name="value">八进制</param>
    public static String OctToDec(String value) => X2X(value, 8, 10);

    /// <summary>
    /// 八进制转换为十六进制
    /// </summary>
    /// <param name="value">八进制</param>
    public static String OctToHex(String value) => X2X(value, 8, 16);

    /// <summary>
    /// 十进制转换为二进制
    /// </summary>
    /// <param name="value">十进制</param>
    public static String DecToBin(String value) => X2X(value, 10, 2);

    /// <summary>
    /// 十进制转换为八进制
    /// </summary>
    /// <param name="value">十进制</param>
    public static String DecToOct(String value) => X2X(value, 10, 8);

    /// <summary>
    /// 十进制转换为十六进制
    /// </summary>
    /// <param name="value">十进制</param>
    public static String DecToHex(String value) => X2X(value, 10, 16);

    /// <summary>
    /// 十六进制转换为二进制
    /// </summary>
    /// <param name="value">十六进制</param>
    /// <param name="Reverse">是否翻转</param>
    public static String HexToBin(String value, Boolean Reverse = false)
    {
        if (value.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(value));

        // 处理奇数长度：补前导零使长度为偶数
        value = value.Length % 2 != 0 ? "0" + value : value;

        // 转换为字节数组
        var bytes = new Byte[value.Length / 2];
        for (var i = 0; i < bytes.Length; i++)
        {
            var byteStr = value.Substring(i * 2, 2);
            bytes[i] = Convert.ToByte(byteStr, 16);
        }

        // 每个字节转8位二进制
        return String.Join("", bytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0').Reverse()));
    }

    /// <summary>
    /// 十六进制转换为八进制
    /// </summary>
    /// <param name="value">十六进制</param>
    public static String HexToOct(String value) => X2X(value, 16, 8);

    /// <summary>
    /// 十六进制转换为十进制
    /// </summary>
    /// <param name="value">十六进制</param>
    public static String HexToDec(String value) => X2X(value, 16, 10);

    /// <summary>
    /// 任意进制转换，将源进制表示的value转换为目标进制，进制的字符排序为先大写后小写
    /// </summary>
    /// <param name="value">要转换的数据</param>
    /// <param name="fromRadix">源进制数，必须为[2,62]范围内</param>
    /// <param name="toRadix">目标进制数，必须为[2,62]范围内</param>
    public static String X2X(String value, Int32 fromRadix, Int32 toRadix)
    {
        if (String.IsNullOrWhiteSpace(value))
            throw new ArgumentNullException(nameof(value));
        if (fromRadix < 2 || fromRadix > 62)
            throw new ArgumentOutOfRangeException(nameof(fromRadix));
        if (toRadix < 2 || toRadix > 62)
            throw new ArgumentOutOfRangeException(nameof(toRadix));
        var num = X2H(value, fromRadix);
        return H2X(num, toRadix);
    }

    /// <summary>
    /// 将64位有符号整数形式的数值转换为指定基数的数值的字符串形式
    /// </summary>
    /// <param name="value">64位有符号整数形式的数值</param>
    /// <param name="toRadix">要转换的目标基数，必须为[2,62]范围内</param>
    public static String H2X(UInt64 value, Int32 toRadix)
    {
        if (toRadix < 2 || toRadix > 62)
            throw new ArgumentOutOfRangeException(nameof(toRadix));
        if (value == 0)
            return "0";
        var baseChar = GetBaseChar(toRadix);
        var result = String.Empty;
        while (value > 0)
        {
            var index = (Int32)(value % (UInt64)baseChar.Length);
            result = baseChar[index] + result;
            value /= (UInt64)baseChar.Length;
        }
        return result;
    }

    /// <summary>
    /// 将指定基数的数字的字符串表示形式转换为等效的64位有符号整数
    /// </summary>
    /// <param name="value">指定基数的数字的字符串表示</param>
    /// <param name="fromRadix">字符串的基数，必须为[2,62]范围内</param>
    public static UInt64 X2H(String value, Int32 fromRadix)
    {
        if (String.IsNullOrWhiteSpace(value))
            throw new ArgumentNullException(nameof(value));
        if (fromRadix < 2 || fromRadix > 62)
            throw new ArgumentOutOfRangeException(nameof(fromRadix));
        value = value.Trim();
        var baseChar = GetBaseChar(fromRadix);
        UInt64 result = 0;
        for (var i = 0; i < value.Length; i++)
        {
            var @char = value[i];
            if (!baseChar.Contains(@char))
            {
                var lang = ObjectContainer.Provider.GetPekService<IPekLanguage>();

                throw new ArgumentException(String.Format(lang?.Translate("参数中的字符\"{0}\"不是 {1} 进制数的有效字符。")!, @char, fromRadix));
            }
            result += (UInt64)baseChar.IndexOf(@char) * (UInt64)Math.Pow(baseChar.Length, value.Length - i - 1);
        }
        return result;
    }

    /// <summary>
    /// 获取基础字符串
    /// </summary>
    /// <param name="radix">进制数</param>
    private static String GetBaseChar(Int32 radix)
    {
        var result = radix switch
        {
            26 => "abcdefghijklmnopqrstuvwxyz",
            32 => "0123456789ABCDEFGHJKMNPQRSTVWXYZabcdefghijklmnopqrstuvwxyz",
            36 => "0123456789abcdefghijklmnopqrstuvwxyz",
            52 => "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ",
            58 => "123456789abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ",
            62 => "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ",
            _ => BaseChar,
        };
        return result[..radix];
    }

    /// <summary>
    /// 小端转为大端。C#数据格式默认为小端  低们在前，高位在后为小端模式
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static Byte[] ConvertLitterToBig(this Byte[] bytes) => [.. bytes.Reverse()];

    /// <summary>
    /// 大端转为小端。C#数据格式默认为小端  高位在前，低位在后为大端模式
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static Byte[] ConvertBigToLitter(this Byte[] bytes) => [.. bytes.Reverse()];

    /// <summary>
    /// 十六进制转换为带符号的short类型
    /// </summary>
    /// <param name="hexString"></param>
    /// <returns></returns>
    public static Int16 HexToShort(String hexString)
    {
        // 将字符串转换为32位无符号整数
        var number = UInt32.Parse(hexString, System.Globalization.NumberStyles.HexNumber);

        // 获取低16位并直接转换为带符号的short类型
        var signedShort = (Int16)(number & 0xFFFF);

        return signedShort;
    }

    /// <summary>
    /// 单字节转换为二进制字符串
    /// </summary>
    /// <param name="b">单字节</param>
    /// <returns></returns>
    public static String ByteToBin(Byte b) => Convert.ToString(b, 2).PadLeft(8, '0');

    /// <summary>
    /// 二进制字符串转换为单字节
    /// </summary>
    /// <param name="BinaryString">单字节</param>
    /// <returns></returns>
    public static Byte BinToByte(String BinaryString) => Convert.ToByte(BinaryString, 2);
}