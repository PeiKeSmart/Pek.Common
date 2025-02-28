using System.Text;

namespace Pek.Helpers;

/// <summary>
/// 类型转换 操作
/// </summary>
public static partial class Conv
{
    #region ToByte(转换为byte)

    /// <summary>
    /// 转换为8位整型
    /// </summary>
    /// <param name="input">输入值</param>
    public static Byte ToDGByte(this Object input) => ToDGByte(input, default);

    /// <summary>
    /// 转换为8位整型
    /// </summary>
    /// <param name="input">输入值</param>
    /// <param name="defaultValue">默认值</param>
    public static Byte ToDGByte(this Object input, Byte defaultValue) => ToDGByteOrNull(input) ?? defaultValue;

    /// <summary>
    /// 转换为8位可空整型
    /// </summary>
    /// <param name="input">输入值</param>
    public static Byte? ToDGByteOrNull(this Object input)
    {
        var success = Byte.TryParse(input.SafeString(), out var result);
        if (success)
            return result;
        try
        {
            var temp = ToDGDoubleOrNull(input, 0);
            if (temp == null)
                return null;
            return Convert.ToByte(temp);
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region ToBytes(转换为bytes)

    /// <summary>
    /// 转换为字节数组
    /// </summary>
    /// <param name="input">输入值</param>        
    public static Byte[] ToDGBytes(this String input) => ToDGBytes(input, Encoding.UTF8);

    /// <summary>
    /// 转换为字节数组
    /// </summary>
    /// <param name="input">输入值</param>
    /// <param name="encoding">字符编码</param>
    public static Byte[] ToDGBytes(this String input, Encoding encoding) => String.IsNullOrWhiteSpace(input) ? [] : encoding.GetBytes(input);

    #endregion

    #region ToChar(转换为char)

    /// <summary>
    /// 转换为字符
    /// </summary>
    /// <param name="input">输入值</param>
    public static Char ToDGChar(this Object input) => ToDGChar(input, default);

    /// <summary>
    /// 转换为字符
    /// </summary>
    /// <param name="input">输入值</param>
    /// <param name="defaultValue">默认值</param>
    public static Char ToDGChar(this Object input, Char defaultValue) => ToDGCharOrNull(input) ?? defaultValue;

    /// <summary>
    /// 转换为可空字符
    /// </summary>
    /// <param name="input">输入值</param>
    public static Char? ToDGCharOrNull(this Object input)
    {
        var success = Char.TryParse(input.SafeString(), out var result);
        if (success)
            return result;
        return null;
    }

    #endregion

    #region ToShort(转换为short)

    /// <summary>
    /// 转换为16位整型
    /// </summary>
    /// <param name="input">输入值</param>
    public static Int16 ToDGShort(this Object input) => ToDGShort(input, default);

    /// <summary>
    /// 转换为16位整型
    /// </summary>
    /// <param name="input">输入值</param>
    /// <param name="defaultValue">默认值</param>
    public static Int16 ToDGShort(this Object input, Int16 defaultValue) => ToDGShortOrNull(input) ?? defaultValue;

    /// <summary>
    /// 转换为16位可空整型
    /// </summary>
    /// <param name="input">输入值</param>
    public static Int16? ToDGShortOrNull(this Object input)
    {
        var success = Int16.TryParse(input.SafeString(), out var result);
        if (success)
            return result;
        try
        {
            var temp = ToDGDoubleOrNull(input, 0);
            if (temp == null)
                return null;
            return System.Convert.ToInt16(temp);
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region ToInt(转换为int)

    /// <summary>
    /// 转换为32位整型
    /// </summary>
    /// <param name="input">输入值</param>
    public static Int32 ToDGInt(this Object? input) => ToDGInt(input, default);

    /// <summary>
    /// 转换为32位整型
    /// </summary>
    /// <param name="input">输入值</param>
    /// <param name="defaultValue">默认值</param>
    public static Int32 ToDGInt(this Object? input, Int32 defaultValue) => ToDGIntOrNull(input) ?? defaultValue;

    /// <summary>
    /// 转换为32位可空整型
    /// </summary>
    /// <param name="input">输入值</param>
    public static Int32? ToDGIntOrNull(this Object? input)
    {
        var success = Int32.TryParse(input.SafeString(), out var result);
        if (success)
            return result;
        try
        {
            var temp = ToDGDoubleOrNull(input, 0);
            if (temp == null)
                return null;
            return Convert.ToInt32(temp);
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region ToLong(转换为long)

    /// <summary>
    /// 转换为64位整型
    /// </summary>
    /// <param name="input">输入值</param>
    public static Int64 ToDGLong(this Object input) => ToDGLong(input, default);

    /// <summary>
    /// 转换为64位整型
    /// </summary>
    /// <param name="input">输入值</param>
    /// <param name="defaultValue">默认值</param>
    public static Int64 ToDGLong(this Object input, Int64 defaultValue) => ToDGLongOrNull(input) ?? defaultValue;

    /// <summary>
    /// 转换为64位可空整型
    /// </summary>
    /// <param name="input">输入值</param>
    public static Int64? ToDGLongOrNull(this Object input)
    {
        var success = Int64.TryParse(input.SafeString(), out var result);
        if (success)
            return result;
        try
        {
            var temp = ToDGDecimalOrNull(input, 0);
            if (temp == null)
                return null;
            return Convert.ToInt64(temp);
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region ToFloat(转换为float)

    /// <summary>
    /// 转换为32位浮点型，并按指定小数位舍入
    /// </summary>
    /// <param name="input">输入值</param>
    /// <param name="digits">小数位数</param>
    public static Single ToDGFloat(this Object input, Int32? digits = null) => ToDGFloat(input, default, digits);

    /// <summary>
    /// 转换为32位浮点型，并按指定小数位舍入
    /// </summary>
    /// <param name="input">输入值</param>
    /// <param name="defaultValue">默认值</param>
    /// <param name="digits">小数位数</param>
    public static Single ToDGFloat(this Object input, Single defaultValue, Int32? digits = null) => ToDGFloatOrNull(input, digits) ?? defaultValue;

    /// <summary>
    /// 转换为32位可空浮点型，并按指定小数位舍入
    /// </summary>
    /// <param name="input">输入值</param>
    /// <param name="digits">小数位数</param>
    public static Single? ToDGFloatOrNull(this Object input, Int32? digits = null)
    {
        var success = Single.TryParse(input.SafeString(), out var result);
        if (!success)
            return null;
        if (digits == null)
            return result;
        return (Single)Math.Round(result, digits.Value);
    }

    #endregion

    #region ToDouble(转换为double)

    /// <summary>
    /// 转换为64位浮点型，并按指定小数位舍入，温馨提示：4舍6入5成双
    /// </summary>
    /// <param name="input">输入值</param>
    /// <param name="digits">小数位数</param>
    public static Double ToDGDouble(this Object? input, Int32? digits = null) => ToDGDouble(input, default, digits);

    /// <summary>
    /// 转换为64位浮点型，并按指定小数位舍入，温馨提示：4舍6入5成双
    /// </summary>
    /// <param name="input">输入值</param>
    /// <param name="defaultValue">默认值</param>
    /// <param name="digits">小数位数</param>
    public static Double ToDGDouble(this Object? input, Double defaultValue, Int32? digits = null) => ToDGDoubleOrNull(input, digits) ?? defaultValue;

    /// <summary>
    /// 转换为64位可空浮点型，并按指定小数位舍入，温馨提示：4舍6入5成双
    /// </summary>
    /// <param name="input">输入值</param>
    /// <param name="digits">小数位数</param>
    public static Double? ToDGDoubleOrNull(this Object? input, Int32? digits = null)
    {
        var success = Double.TryParse(input.SafeString(), out var result);
        if (!success)
            return null;
        return digits == null ? result : Math.Round(result, digits.Value);
    }

    #endregion

    #region ToDecimal(转换为decimal)

    /// <summary>
    /// 转换为128位浮点型，并按指定小数位舍入，温馨提示：4舍6入5成双
    /// </summary>
    /// <param name="input">输入值</param>
    /// <param name="digits">小数位数</param>
    public static Decimal ToDGDecimal(this Object input, Int32? digits = null) => ToDGDecimal(input, default, digits);

    /// <summary>
    /// 转换为128位浮点型，并按指定小数位舍入，温馨提示：4舍6入5成双
    /// </summary>
    /// <param name="input">输入值</param>
    /// <param name="defaultValue">默认值</param>
    /// <param name="digits">小数位数</param>
    public static Decimal ToDGDecimal(this Object input, Decimal defaultValue, Int32? digits = null) => ToDGDecimalOrNull(input, digits) ?? defaultValue;

    /// <summary>
    /// 转换为128位可空浮点型，并按指定小数位舍入，温馨提示：4舍6入5成双
    /// </summary>
    /// <param name="input">输入值</param>
    /// <param name="digits">小数位数</param>
    public static Decimal? ToDGDecimalOrNull(this Object input, Int32? digits = null)
    {
        var success = Decimal.TryParse(input.SafeString(), out var result);
        if (!success)
            return null;
        return digits == null ? result : Math.Round(result, digits.Value);
    }

    #endregion

    #region ToBool(转换为bool)

    /// <summary>
    /// 转换为布尔值
    /// </summary>
    /// <param name="input">输入值</param>
    public static Boolean ToDGBool(this Object input) => ToDGBool(input, default);

    /// <summary>
    /// 转换为布尔值
    /// </summary>
    /// <param name="input">输入值</param>
    /// <param name="defaultValue">默认值</param>
    public static Boolean ToDGBool(this Object input, Boolean defaultValue) => ToDGBoolOrNull(input) ?? defaultValue;

    /// <summary>
    /// 转换为可空布尔值
    /// </summary>
    /// <param name="input">输入值</param>
    public static Boolean? ToDGBoolOrNull(this Object input)
    {
        var value = GetBool(input);
        if (value != null)
            return value.Value;
        return Boolean.TryParse(input.SafeString(), out var result) ? (Boolean?)result : null;
    }

    /// <summary>
    /// 获取布尔值
    /// </summary>
    /// <param name="input">输入值</param>
    private static Boolean? GetBool(Object input)
    {
        return input.SafeString().ToLower() switch
        {
            "0" or "否" or "不" or "no" or "fail" => false,
            "1" or "是" or "ok" or "yes" => true,
            _ => null,
        };
    }

    #endregion

    #region ToDate(转换为DateTime)

    /// <summary>
    /// 转换为日期
    /// </summary>
    /// <param name="input">输入值</param>
    public static DateTime ToDGDate(this Object input) => ToDGDateOrNull(input) ?? DateTime.MinValue;

    /// <summary>
    /// 转换为可空日期
    /// </summary>
    /// <param name="input">输入值</param>
    /// <param name="defaultValue">默认值</param>
    public static DateTime? ToDGDateOrNull(this Object input, DateTime? defaultValue = null)
    {
        if (input == null)
            return defaultValue;
        return DateTime.TryParse(input.SafeString(), out var result) ? result : defaultValue;
    }

    #endregion

    #region ToGuid(转换为Guid)

    /// <summary>
    /// 转换为Guid
    /// </summary>
    /// <param name="input">输入值</param>
    public static Guid ToDGGuid(this Object input) => ToDGGuidOrNull(input) ?? Guid.Empty;

    /// <summary>
    /// 转换为可空Guid
    /// </summary>
    /// <param name="input">输入值</param>
    public static Guid? ToDGGuidOrNull(this Object input) => Guid.TryParse(input.SafeString(), out var result) ? (Guid?)result : null;

    /// <summary>
    /// 转换为Guid集合
    /// </summary>
    /// <param name="input">输入值，以逗号分隔的Guid集合字符串，范例：83B0233C-A24F-49FD-8083-1337209EBC9A,EAB523C6-2FE7-47BE-89D5-C6D440C3033A</param>
    public static List<Guid> ToDGGuidList(this String input) => ToDGList<Guid>(input);

    #endregion

    #region ToList(泛型集合转换)

    /// <summary>
    /// 泛型集合转换
    /// </summary>
    /// <typeparam name="T">目标元素类型</typeparam>
    /// <param name="input">输入值，以逗号分隔的元素集合字符串，范例：83B0233C-A24F-49FD-8083-1337209EBC9A,EAB523C6-2FE7-47BE-89D5-C6D440C3033A</param>
    public static List<T> ToDGList<T>(this String input)
    {
        var result = new List<T>();
        if (String.IsNullOrWhiteSpace(input))
            return result;
        var array = input.Split(',');
        result.AddRange(from each in array where !String.IsNullOrWhiteSpace(each) select To<T>(each));
        return result;
    }

    #endregion

    #region ToEnum(转换为枚举)

    /// <summary>
    /// 转换为枚举
    /// </summary>
    /// <typeparam name="T">枚举类型</typeparam>
    /// <param name="input">输入值</param>
    public static T ToDGEnum<T>(this Object input) where T : struct => ToDGEnum<T>(input, default);

    /// <summary>
    /// 转换为枚举
    /// </summary>
    /// <typeparam name="T">枚举类型</typeparam>
    /// <param name="input">输入值</param>
    /// <param name="defaultValue">默认值</param>
    public static T ToDGEnum<T>(this Object input, T defaultValue) where T : struct => ToDGEnumOrNull<T>(input) ?? defaultValue;

    /// <summary>
    /// 转换为可空枚举
    /// </summary>
    /// <typeparam name="T">枚举类型</typeparam>
    /// <param name="input">输入值</param>
    public static T? ToDGEnumOrNull<T>(this Object input) where T : struct
    {
        var success = System.Enum.TryParse(input.SafeString(), true, out T result);
        if (success)
            return result;
        return null;
    }

    #endregion

    #region To(通用泛型转换)

    /// <summary>
    /// 通用泛型转换
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="input">输入值</param>
    public static T? To<T>(this Object input)
    {
        if (input == null)
            return default;
        if (input is String && String.IsNullOrWhiteSpace(input.ToString()))
            return default;

        var type = Common.GetType<T>();
        var typeName = type.Name.ToLower();
        try
        {
            if (typeName == "string")
                return (T)(Object)input.ToString()!;
            if (typeName == "guid")
                return (T)(Object)new Guid(input.ToString()!);
            if (type.IsEnum)
                return EnumHelper.Parse<T>(input);
            if (input is IConvertible)
                return (T)Convert.ChangeType(input, type);
            return (T)input;
        }
        catch
        {
            return default;
        }
    }

    #endregion

    #region 转换对象为字符串
    /// <summary>
    /// 转换对象为字符串
    /// </summary>
    /// <param name="Value">对象</param>
    /// <returns>转换后的字符串</returns>
    public static String? ObjToString(this Object Value)
    {
        if (Value.ObjIsNull())
        {
            return String.Empty;
        }
        return Value.ToString();
    }

    /// <summary>
    /// 转换对象为字符串
    /// </summary>
    /// <param name="Value">对象</param>
    /// <param name="DefaultValue">默认值</param>
    /// <param name="Trim">是否去除空格</param>
    /// <returns>转换后的字符串</returns>
    public static String? ObjToString(this Object Value, String DefaultValue, Boolean Trim)
    {
        if (Value.ObjIsNull())
        {
            return ObjToString(DefaultValue);
        }
        if (Trim)
        {
            return Value.ToString()?.Trim();
        }
        return Value.ToString();
    }

    #endregion

    /// <summary>
    /// 将ip地址转换成long类型
    /// </summary>
    /// <param name="ip">ip</param>
    /// <returns></returns>
    public static Int64 ConvertIPToLong(this String ip)
    {
        if (!ip.Contains('.'))
        {
            ip = "127.0.0.1";
        }
        if (!ip.IsIPAddress())
        {
            ip = "127.0.0.1";
        }
        var ips = ip.Split('.');
        var number = 16777216L * Int64.Parse(ips[0]) + 65536L * Int64.Parse(ips[1]) + 256 * Int64.Parse(ips[2]) + Int64.Parse(ips[3]);
        return number;
    }

}