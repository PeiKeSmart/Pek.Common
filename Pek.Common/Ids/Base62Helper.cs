using System.Text;

namespace Pek.Ids;

/// <summary>
/// Base62编码解码工具类
/// </summary>
public static class Base62Helper
{
    /// <summary>
    /// 默认Base62字符集：a-z, A-Z, 0-9 (共62个字符)
    /// 适用于ID转换场景
    /// </summary>
    private static readonly String idChars = "aAbBcCdDeEfFgGhHiIjJkKlLmMnNoOpPqQrRsStTuUvVwWxXyYzZ0123456789";

    /// <summary>
    /// 标准Base62字符集：0-9, A-Z, a-z (共62个字符)
    /// 适用于字节数组编码场景
    /// </summary>
    private static readonly String standardChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    /// <summary>
    /// 反转Base62字符集：0-9, a-z, A-Z (共62个字符)
    /// </summary>
    private static readonly String invertedChars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    #region ID转换方法 (使用idChars字符集)

    /// <summary>
    /// 将Int64数值转换为Base62字符串 (ID专用字符集)
    /// </summary>
    /// <param name="value">要转换的数值</param>
    /// <returns>Base62编码的字符串</returns>
    public static String Encode(Int64 value)
    {
        if (value <= 0)
            return "0";

        var list = new List<Char>();
        var id = (UInt64)value;
        
        while (id > 0)
        {
            var remainder = (Int32)(id % 62);
            list.Add(idChars[remainder]);
            id /= 62;
        }
        
        list.Reverse();
        return new String([.. list]);
    }

    /// <summary>
    /// 将UInt32数值转换为Base62字符串 (ID专用字符集)
    /// </summary>
    /// <param name="value">要转换的数值</param>
    /// <returns>Base62编码的字符串</returns>
    public static String Encode(UInt32 value)
    {
        if (value == 0)
            return "0";

        var list = new List<Char>();
        var id = value;
        
        while (id > 0)
        {
            var remainder = (Int32)(id % 62);
            list.Add(idChars[remainder]);
            id /= 62;
        }
        
        list.Reverse();
        return new String([.. list]);
    }

    /// <summary>
    /// 将Base62字符串解码为Int64数值 (ID专用字符集)
    /// </summary>
    /// <param name="base62String">Base62编码的字符串</param>
    /// <returns>解码后的数值</returns>
    /// <exception cref="ArgumentException">当字符串包含无效字符时抛出</exception>
    public static Int64 DecodeToInt64(String base62String)
    {
        if (String.IsNullOrEmpty(base62String))
            return 0;

        UInt64 result = 0;
        UInt64 power = 1;

        for (var i = base62String.Length - 1; i >= 0; i--)
        {
            var charIndex = idChars.IndexOf(base62String[i]);
            if (charIndex == -1)
                throw new ArgumentException($"Invalid character '{base62String[i]}' in Base62 string");
            
            result += (UInt64)charIndex * power;
            power *= 62;
        }

        return (Int64)result;
    }

    /// <summary>
    /// 将Base62字符串解码为UInt32数值 (ID专用字符集)
    /// </summary>
    /// <param name="base62String">Base62编码的字符串</param>
    /// <returns>解码后的数值</returns>
    /// <exception cref="ArgumentException">当字符串包含无效字符时抛出</exception>
    public static UInt32 DecodeToUInt32(String base62String)
    {
        if (String.IsNullOrEmpty(base62String))
            return 0;

        UInt32 result = 0;
        UInt32 power = 1;

        for (var i = base62String.Length - 1; i >= 0; i--)
        {
            var charIndex = idChars.IndexOf(base62String[i]);
            if (charIndex == -1)
                throw new ArgumentException($"Invalid character '{base62String[i]}' in Base62 string");
            
            result += (UInt32)charIndex * power;
            power *= 62;
        }

        return result;
    }

    #endregion

    #region 字节数组转换方法 (使用标准字符集)

    /// <summary>
    /// 将字节数组转换为Base62字符串 (使用标准字符集)
    /// </summary>
    /// <param name="bytes">字节数组</param>
    /// <param name="inverted">是否使用反转字符集</param>
    /// <returns>Base62编码的字符串</returns>
    public static String Encode(Byte[] bytes, Boolean inverted = false)
    {
        if (bytes == null || bytes.Length == 0)
            return "0";

        // 对于简单的4字节转换，使用原来的方法
        if (bytes.Length == 4)
        {
            var value = BitConverter.ToUInt32(bytes, 0);
            return EncodeNumber(value, inverted);
        }

        // 对于任意长度的字节数组，使用复杂的进制转换
        return EncodeByteArray(bytes, inverted);
    }

    /// <summary>
    /// 将Base62字符串解码为字节数组 (使用标准字符集)
    /// </summary>
    /// <param name="base62String">Base62编码的字符串</param>
    /// <param name="inverted">是否使用反转字符集</param>
    /// <returns>解码后的字节数组</returns>
    public static Byte[] Decode(String base62String, Boolean inverted = false)
    {
        if (String.IsNullOrEmpty(base62String))
            return [];

        return DecodeByteArray(base62String, inverted);
    }

    #endregion

    #region 私有辅助方法

    /// <summary>
    /// 使用简单方法编码数值
    /// </summary>
    private static String EncodeNumber(UInt32 value, Boolean inverted)
    {
        if (value == 0)
            return "0";

        var chars = inverted ? invertedChars : standardChars;
        var list = new List<Char>();
        
        while (value > 0)
        {
            var remainder = (Int32)(value % 62);
            list.Add(chars[remainder]);
            value /= 62;
        }
        
        list.Reverse();
        return new String([.. list]);
    }

    /// <summary>
    /// 使用复杂方法编码字节数组
    /// </summary>
    private static String EncodeByteArray(Byte[] original, Boolean inverted)
    {
        var characterSet = inverted ? invertedChars : standardChars;
        var array = BaseConvert(Array.ConvertAll<Byte, Int32>(original, b => (Int32)b), 256, 62);
        var builder = new StringBuilder();
        foreach (var t in array)
        {
            builder.Append(characterSet[t]);
        }
        return builder.ToString();
    }

    /// <summary>
    /// 解码字节数组
    /// </summary>
    private static Byte[] DecodeByteArray(String base62, Boolean inverted)
    {
        if (String.IsNullOrWhiteSpace(base62))
            throw new ArgumentNullException(nameof(base62));

        var characterSet = inverted ? invertedChars : standardChars;
        return Array.ConvertAll<Int32, Byte>(
            BaseConvert(
                Array.ConvertAll<Char, Int32>(base62.ToCharArray(), c => characterSet.IndexOf(c)), 
                62, 
                256
            ), 
            Convert.ToByte
        );
    }

    /// <summary>
    /// 进制转换核心算法
    /// </summary>
    private static Int32[] BaseConvert(Int32[] source, Int32 sourceBase, Int32 targetBase)
    {
        var result = new List<Int32>();
        var leadingZeroCount = Math.Min(source.TakeWhile(x => x == 0).Count(), source.Length - 1);
        
        while (source.Length > 0)
        {
            var quotient = new List<Int32>();
            var remainder = 0;
            
            for (var i = 0; i < source.Length; i++)
            {
                var num = source[i] + remainder * sourceBase;
                var digit = num / targetBase;
                remainder = num % targetBase;
                
                if (quotient.Count > 0 || digit > 0)
                {
                    quotient.Add(digit);
                }
            }
            
            result.Insert(0, remainder);
            source = [.. quotient];
        }
        
        result.InsertRange(0, Enumerable.Repeat(0, leadingZeroCount));
        return [.. result];
    }

    #endregion

    #region 工具方法

    /// <summary>
    /// 验证字符串是否为有效的Base62格式 (ID字符集)
    /// </summary>
    /// <param name="input">要验证的字符串</param>
    /// <returns>是否为有效的Base62字符串</returns>
    public static Boolean IsValidBase62(String input)
    {
        if (String.IsNullOrEmpty(input))
            return false;

        return input.All(c => idChars.Contains(c));
    }

    /// <summary>
    /// 验证字符串是否为有效的Base62格式 (标准字符集)
    /// </summary>
    /// <param name="input">要验证的字符串</param>
    /// <param name="inverted">是否使用反转字符集</param>
    /// <returns>是否为有效的Base62字符串</returns>
    public static Boolean IsValidBase62Standard(String input, Boolean inverted = false)
    {
        if (String.IsNullOrEmpty(input))
            return false;

        var chars = inverted ? invertedChars : standardChars;
        return input.All(c => chars.Contains(c));
    }

    #endregion
}
