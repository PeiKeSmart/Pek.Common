namespace Pek.Ids;

/// <summary>
/// Base62编码解码工具类
/// </summary>
public static class Base62Helper
{
    /// <summary>
    /// Base62字符集：a-z, A-Z, 0-9 (共62个字符)
    /// </summary>
    private static readonly String chars = "aAbBcCdDeEfFgGhHiIjJkKlLmMnNoOpPqQrRsStTuUvVwWxXyYzZ0123456789";

    /// <summary>
    /// 将Int64数值转换为Base62字符串
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
            list.Add(chars[remainder]);
            id /= 62;
        }
        
        list.Reverse();
        return new String([.. list]);
    }

    /// <summary>
    /// 将UInt32数值转换为Base62字符串
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
            list.Add(chars[remainder]);
            id /= 62;
        }
        
        list.Reverse();
        return new String([.. list]);
    }

    /// <summary>
    /// 将字节数组转换为Base62字符串
    /// </summary>
    /// <param name="bytes">字节数组</param>
    /// <returns>Base62编码的字符串</returns>
    public static String Encode(Byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
            return "0";

        var value = BitConverter.ToUInt32(bytes, 0);
        return Encode(value);
    }

    /// <summary>
    /// 将Base62字符串解码为Int64数值
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
            var charIndex = chars.IndexOf(base62String[i]);
            if (charIndex == -1)
                throw new ArgumentException($"Invalid character '{base62String[i]}' in Base62 string");
            
            result += (UInt64)charIndex * power;
            power *= 62;
        }

        return (Int64)result;
    }

    /// <summary>
    /// 将Base62字符串解码为UInt32数值
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
            var charIndex = chars.IndexOf(base62String[i]);
            if (charIndex == -1)
                throw new ArgumentException($"Invalid character '{base62String[i]}' in Base62 string");
            
            result += (UInt32)charIndex * power;
            power *= 62;
        }

        return result;
    }

    /// <summary>
    /// 验证字符串是否为有效的Base62格式
    /// </summary>
    /// <param name="input">要验证的字符串</param>
    /// <returns>是否为有效的Base62字符串</returns>
    public static Boolean IsValidBase62(String input)
    {
        if (String.IsNullOrEmpty(input))
            return false;

        return input.All(c => chars.Contains(c));
    }
}
