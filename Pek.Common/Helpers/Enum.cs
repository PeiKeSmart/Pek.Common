namespace Pek.Helpers;

/// <summary>
/// 枚举 操作
/// </summary>
public static partial class Enum
{
    /// <summary>
    /// 枚举值字段
    /// </summary>
    private const string EnumValueField = "value__";

    #region Parse(获取实例)

    /// <summary>
    /// 获取实例
    /// </summary>
    /// <typeparam name="TEnum">枚举类型</typeparam>
    /// <param name="member">成员名或值，范例：Enum1枚举有成员A=0，则传入"A"或"0"获取 Enum1.A</param>
    public static TEnum Parse<TEnum>(object member)
    {
        var value = member.SafeString();
        if (value.IsEmpty())
        {
            if (typeof(TEnum).IsGenericType)
                return default;
            throw new ArgumentNullException(nameof(member));
        }
        return (TEnum)System.Enum.Parse(Common.GetType<TEnum>(), value, true);
    }

    #endregion

    #region GetValue(获取成员值)

    /// <summary>
    /// 获取成员值
    /// </summary>
    /// <typeparam name="TEnum">枚举类型</typeparam>
    /// <param name="member">成员名、值、实例均可，范例:Enum1枚举有成员A=0,可传入"A"、0、Enum1.A，获取值0</param>
    /// <exception cref="ArgumentNullException">成员为空</exception>
    public static int GetValue<TEnum>(object member) => GetValue(Common.GetType<TEnum>(), member);

    /// <summary>
    /// 获取成员值
    /// </summary>
    /// <param name="type">枚举类型</param>
    /// <param name="member">成员名、值、实例均可，范例:Enum1枚举有成员A=0,可传入"A"、0、Enum1.A，获取值0</param>
    /// <exception cref="ArgumentNullException">成员为空</exception>
    public static int GetValue(Type type, object member)
    {
        string value = member.SafeString();
        if (value.IsEmpty())
            throw new ArgumentNullException(nameof(member));
        return (int)System.Enum.Parse(type, member.ToString(), true);
    }

    #endregion
}