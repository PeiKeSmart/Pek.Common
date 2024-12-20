﻿using System.ComponentModel;
using System.Reflection;

namespace Pek.Helpers;

/// <summary>
/// 枚举 操作
/// </summary>
public static partial class EnumHelper
{
    /// <summary>
    /// 枚举值字段
    /// </summary>
    private const String EnumValueField = "value__";

    #region Parse(获取实例)

    /// <summary>
    /// 获取实例
    /// </summary>
    /// <typeparam name="TEnum">枚举类型</typeparam>
    /// <param name="member">成员名或值，范例：Enum1枚举有成员A=0，则传入"A"或"0"获取 Enum1.A</param>
    public static TEnum? Parse<TEnum>(Object member)
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
    public static Int32 GetValue<TEnum>(Object member) => GetValue(Common.GetType<TEnum>(), member);

    /// <summary>
    /// 获取成员值
    /// </summary>
    /// <param name="type">枚举类型</param>
    /// <param name="member">成员名、值、实例均可，范例:Enum1枚举有成员A=0,可传入"A"、0、Enum1.A，获取值0</param>
    /// <exception cref="ArgumentNullException">成员为空</exception>
    public static Int32 GetValue(Type type, Object member)
    {
        var value = member.SafeString();
        if (value.IsEmpty())
            throw new ArgumentNullException(nameof(member));
        return (Int32)System.Enum.Parse(type, member.ToString()!, true);
    }

    #endregion

    #region ParseByDescription(通过描述获取实例)

    /// <summary>
    /// 通过描述获取实例
    /// </summary>
    /// <typeparam name="TEnum">枚举类型</typeparam>
    /// <param name="desc">描述</param>
    public static TEnum? ParseByDescription<TEnum>(String desc)
    {
        if (desc.IsEmpty())
        {
            if (typeof(TEnum).IsGenericType)
                return default;
            throw new ArgumentNullException(nameof(desc));
        }
        var type = Common.GetType<TEnum>();
        var fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.Default);
        var fieldInfo =
            fieldInfos.FirstOrDefault(p => p.GetCustomAttribute<DescriptionAttribute>(false)?.Description == desc);
        return fieldInfo == null
            ? throw new ArgumentNullException($"在枚举（{type.FullName}）中，未发现描述为“{desc}”的枚举项。")
            : (TEnum)System.Enum.Parse(type, fieldInfo.Name);
    }

    #endregion

    #region GetName(获取成员名)

    /// <summary>
    /// 获取成员名
    /// </summary>
    /// <typeparam name="TEnum">枚举类型</typeparam>
    /// <param name="member">成员名、值、实例均可，范例：Enum1枚举有成员A=0，则传入Enum1.A或0，获取成员名"A"</param>
    public static String GetName<TEnum>(Object member) => GetName(Common.GetType<TEnum>(), member);

    /// <summary>
    /// 获取成员名
    /// </summary>
    /// <param name="type">枚举类型</param>
    /// <param name="member">成员名、值、实例均可，范例：Enum1枚举有成员A=0，则传入Enum1.A或0，获取成员名"A"</param>
    public static String GetName(Type type, Object member)
    {
        if (type == null)
            return String.Empty;
        if (member == null)
            return String.Empty;
        if (member is String)
            return member.ToString()!;
        if (type.GetTypeInfo().IsEnum == false)
            return String.Empty;
        return System.Enum.GetName(type, member)!;
    }

    #endregion

    #region GetNames(获取枚举所有成员名称)

    /// <summary>
    /// 获取枚举所有成员名称
    /// </summary>
    /// <typeparam name="TEnum">枚举类型</typeparam>
    public static String[] GetNames<TEnum>() where TEnum : struct => GetNames(typeof(TEnum));

    /// <summary>
    /// 获取枚举所有成员名称
    /// </summary>
    /// <param name="type">枚举类型</param>
    public static String[] GetNames(Type type) => System.Enum.GetNames(type);

    #endregion

    #region GetItems(获取描述项集合)

    /// <summary>
    /// 获取描述项集合，文本设置为Description，值为Value
    /// </summary>
    /// <typeparam name="TEnum">枚举类型</typeparam>
    public static List<Item> GetItems<TEnum>() => GetItems(typeof(TEnum));

    /// <summary>
    /// 获取描述项集合，文本设置为Description，值为Value
    /// </summary>
    /// <param name="type">枚举类型</param>
    public static List<Item> GetItems(Type type)
    {
        type = Common.GetType(type);
        ValidateEnum(type);
        var result = new List<Item>();
        foreach (var field in type.GetFields())
            AddItem(type, result, field);
        return [.. result.OrderBy(t => t.SortId)];
    }

    /// <summary>
    /// 添加描述项
    /// </summary>
    /// <param name="type">枚举类型</param>
    /// <param name="result">集合</param>
    /// <param name="field">字段</param>
    private static void AddItem(Type type, List<Item> result, FieldInfo field)
    {
        if (!field.FieldType.IsEnum)
            return;
        var value = GetValue(type, field.Name);
        var description = Reflection.GetDescription(field);
        result.Add(new Item(description, value, value));
    }

    #endregion

    #region GetDescription(获取描述)

    /// <summary>
    /// 获取描述，使用<see cref="DescriptionAttribute"/>特性设置描述
    /// </summary>
    /// <typeparam name="TEnum">枚举类型</typeparam>
    /// <param name="member">成员名、值、实例均可,范例:Enum1枚举有成员A=0,可传入"A"、0、Enum1.A，获取值0</param>
    public static String GetDescription<TEnum>(Object member) => Reflection.GetDescription<TEnum>(GetName<TEnum>(member));

    /// <summary>
    /// 获取描述，使用<see cref="DescriptionAttribute"/>特性设置描述
    /// </summary>
    /// <param name="type">枚举类型</param>
    /// <param name="member">成员名、值、实例均可,范例:Enum1枚举有成员A=0,可传入"A"、0、Enum1.A，获取值0</param>
    public static String GetDescription(Type type, Object member) => Reflection.GetDescription(type, GetName(type, member));

    #endregion

    #region GetDictionary(获取枚举字典)

    /// <summary>
    /// 验证是否枚举类型
    /// </summary>
    /// <param name="enumType">类型</param>
    /// <exception cref="InvalidOperationException"></exception>
    private static void ValidateEnum(Type enumType)
    {
        if (enumType.IsEnum == false)
            throw new InvalidOperationException($"类型 {enumType} 不是枚举");
    }

    /// <summary>
    /// 获取枚举字典
    /// </summary>
    /// <typeparam name="TEnum">枚举类型</typeparam>
    public static IDictionary<Int32, String> GetDictionary<TEnum>() where TEnum : struct
    {
        var enumType = Common.GetType<TEnum>().GetTypeInfo();
        ValidateEnum(enumType);
        var dic = new Dictionary<Int32, String>();
        foreach (var field in enumType.GetFields())
            AddItem<TEnum>(dic, field);
        return dic;
    }

    /// <summary>
    /// 添加描述项
    /// </summary>
    /// <typeparam name="TEnum">枚举类型</typeparam>
    /// <param name="result">集合</param>
    /// <param name="field">字典</param>
    private static void AddItem<TEnum>(Dictionary<Int32, String> result, FieldInfo field) where TEnum : struct
    {
        if (!field.FieldType.GetTypeInfo().IsEnum)
            return;
        var value = GetValue<TEnum>(field.Name);
        var description = Reflection.GetDescription(field);
        result.Add(value, description);
    }

    #endregion

    #region GetMemberInfos(获取枚举成员信息)

    /// <summary>
    /// 获取枚举成员信息
    /// </summary>
    /// <typeparam name="TEnum">枚举类型</typeparam>
    public static IEnumerable<Tuple<Int32, String, String>> GetMemberInfos<TEnum>() where TEnum : struct
    {
        var type = typeof(TEnum);
        ValidateEnum(type);
        var fields = type.GetFields();
        ICollection<Tuple<Int32, String, String>> collection = [];
        foreach (var field in fields.Where(x => x.Name != EnumValueField))
        {
            var value = GetValue<TEnum>(field.Name);
            var description = Reflection.GetDescription(field);
            collection.Add(new Tuple<Int32, String, String>(value, field.Name,
                String.IsNullOrWhiteSpace(description) ? field.Name : description));
        }

        return collection;
    }

    #endregion
}