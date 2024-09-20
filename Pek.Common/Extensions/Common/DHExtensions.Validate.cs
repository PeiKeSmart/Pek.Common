using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

namespace Pek;

/// <summary>
/// 系统扩展 - 验证
/// </summary>
public static partial class DHExtensions
{
    #region 验证通用参数

    /// <summary>
    /// 快速验证一个字符串是否符合指定的正则表达式。
    /// </summary>
    /// <param name="_express">正则表达式的内容。</param>
    /// <param name="_value">需验证的字符串。</param>
    /// <returns></returns>
    public static bool QuickValidate(this object _value, string _express)
    {
        return QuickValidate(_value, _express, true);
    }

    /// <summary>
    /// 快速验证一个字符串是否符合指定的正则表达式。
    /// </summary>
    /// <param name="_value">正则表达式的内容。</param>
    /// <param name="_express">需验证的字符串。</param>
    /// <param name="_bool">True区分大小写,False不区分大小写</param>
    /// <returns>是否合法的bool值。</returns>
    public static bool QuickValidate(this object _value, string _express, bool _bool)
    {
        if (ObjIsNull(_value))
        {
            return false;
        }
        if (_bool)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(_value.ToString(), _express);
        }
        else
        {
            return System.Text.RegularExpressions.Regex.IsMatch(_value.ToString(), _express, RegexOptions.IgnoreCase);//不区分大小写
        }
    }

    #endregion 验证通用参数

    #region CheckNull(检查对象是否为null)

    /// <summary>
    /// 检查对象是否为null，为null则抛出<see cref="ArgumentNullException"/>异常
    /// </summary>
    /// <param name="obj">对象</param>
    /// <param name="parameterName">参数名</param>
    public static void CheckNull(this object obj, string parameterName)
    {
        if (obj == null)
            throw new ArgumentNullException(parameterName);
    }

    #endregion

    #region IsEmpty(是否为空)

    /// <summary>
    /// 判断 字符串 是否为空、null或空白字符串
    /// </summary>
    /// <param name="value">数据</param>
    public static bool IsEmpty(this string value) => string.IsNullOrWhiteSpace(value);

    /// <summary>
    /// 判断 Guid 是否为空、null或Guid.Empty
    /// </summary>
    /// <param name="value">数据</param>
    public static bool IsEmpty(this Guid value) => value == Guid.Empty;

    /// <summary>
    /// 判断 Guid 是否为空、null或Guid.Empty
    /// </summary>
    /// <param name="value">数据</param>
    public static bool IsEmpty(this Guid? value) => value == null || IsEmpty(value.Value);

    /// <summary>
    /// 判断 StringBuilder 是否为空
    /// </summary>
    /// <param name="sb">数据</param>
    public static bool IsEmpty(this StringBuilder sb) => sb == null || sb.Length == 0 || sb.ToString().IsEmpty();

    /// <summary>
    /// 判断 迭代集合 是否为空
    /// </summary>
    /// <typeparam name="T">泛型对象</typeparam>
    /// <param name="list">数据</param>
    public static bool IsEmpty<T>(this IEnumerable<T> list) => null == list || !list.Any();

    /// <summary>
    /// 判断 字典 是否为空
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    /// <param name="dictionary">数据</param>
    public static bool IsEmpty<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) => null == dictionary || dictionary.Count == 0;

    /// <summary>
    /// 判断 字典 是否为空
    /// </summary>
    /// <param name="dictionary">数据</param>
    public static bool IsEmpty(this IDictionary dictionary) => null == dictionary || dictionary.Count == 0;

    #endregion

    #region 判断对象是否为空

    /// <summary>
    /// 字段串是否为Null或为""(空)
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool StrIsNullOrEmpty(this string str)
    {
        if (str == null || str.Trim() == string.Empty)
            return true;

        return false;
    }

    public static bool IsNotNullAndWhiteSpace(this string instance)
    {
        return !string.IsNullOrWhiteSpace(instance);
    }

    /// <summary>
    /// 判断对象是否为空
    /// </summary>
    /// <param name="Value">对象</param>
    /// <returns>bool 空为 true ，否则 false</returns>
    public static bool ObjIsNull(this object Value)
    {
        return ((((Value == null) || (Value == DBNull.Value)) || (Value.ToString() == string.Empty)) || (Value.ToString().Trim() == ""));
    }

    #endregion 判断对象是否为空

    #region 判断是否是IP地址格式

    /// <summary>
    /// 判断一个字符串是否为IP地址
    /// </summary>
    /// <param name="_value"></param>
    /// <returns></returns>
    public static bool IsIPAddress(this string _value)
    {
        return QuickValidate(_value, @"^(((2[0-4]{1}[0-9]{1})|(25[0-5]{1}))|(1[0-9]{2})|([1-9]{1}[0-9]{1})|([0-9]{1})).(((2[0-4]{1}[0-9]{1})|(25[0-5]{1}))|(1[0-9]{2})|([1-9]{1}[0-9]{1})|([0-9]{1})).(((2[0-4]{1}[0-9]{1})|(25[0-5]{1}))|(1[0-9]{2})|([1-9]{1}[0-9]{1})|([0-9]{1})).(((2[0-4]{1}[0-9]{1})|(25[0-5]{1}))|(1[0-9]{2})|([1-9]{1}[0-9]{1})|([0-9]{1}))$", false);
    }

    /// <summary>
    /// 是否为ip
    /// </summary>
    /// <param name="ip"></param>
    /// <returns></returns>
    public static bool IsIP(this string ip)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
    }

    public static bool IsIPSect(this string ip)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){2}((2[0-4]\d|25[0-5]|[01]?\d\d?|\*)\.)(2[0-4]\d|25[0-5]|[01]?\d\d?|\*)$");
    }

    #endregion 判断是否是IP地址格式
}
