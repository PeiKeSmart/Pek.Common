using NewLife.Serialization;

namespace Pek.Helpers;

/// <summary>
/// 常用公共操作
/// </summary>
public static partial class Common
{
    #region GetType(获取类型)

    /// <summary>
    /// 获取类型
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    public static Type GetType<T>() => GetType(typeof(T));

    /// <summary>
    /// 获取类型
    /// </summary>
    /// <param name="type">类型</param>
    public static Type GetType(Type type) => Nullable.GetUnderlyingType(type) ?? type;

    #endregion

    /// <summary>Json数组字符串对象转为名值字典</summary>
    /// <param name="listJson">Json数组字符串对象</param>
    /// <returns></returns>
    public static IDictionary<String, Object?> ToDictionary(this String listJson)
    {
        var parser = new JsonParser(listJson);
        var result = parser.Decode();

        var dic = new Dictionary<String, Object?>();

        if (result is List<object> jsonArray)
        {
            foreach (var item in jsonArray)
            {
                if (item is Dictionary<string, object> dictItem)
                {
                    foreach (var kvp in dictItem)
                    {
                        dic[kvp.Key] = kvp.Value;
                    }
                }
            }
        }

        return dic;
    }
}