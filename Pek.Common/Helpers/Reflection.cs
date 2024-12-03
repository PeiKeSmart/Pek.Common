using System.ComponentModel;
using System.Reflection;

namespace Pek.Helpers;

/// <summary>
/// 反射 操作
/// </summary>
public static class Reflection
{
    #region GetDescription(获取类型描述)

    /// <summary>
    /// 获取类型描述，使用<see cref="DescriptionAttribute"/>设置描述
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    public static String GetDescription<T>() => GetDescription(Common.GetType<T>());

    /// <summary>
    /// 获取类型成员描述，使用<see cref="DescriptionAttribute"/>设置描述
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="memberName">成员名称</param>
    public static String GetDescription<T>(String memberName) => GetDescription(Common.GetType<T>(), memberName);

    /// <summary>
    /// 获取类型成员描述，使用<see cref="DescriptionAttribute"/>设置描述
    /// </summary>
    /// <param name="type">类型</param>
    /// <param name="memberName">成员名称</param>
    public static String GetDescription(Type type, String memberName)
    {
        if (type == null)
            return String.Empty;
        return memberName.IsEmpty()
            ? String.Empty
            : GetDescription(type.GetTypeInfo().GetMember(memberName).FirstOrDefault());
    }

    /// <summary>
    /// 获取类型成员描述，使用<see cref="DescriptionAttribute"/>设置描述
    /// </summary>
    /// <param name="member">成员</param>
    public static String GetDescription(MemberInfo? member)
    {
        if (member == null)
            return String.Empty;
        return member.GetCustomAttribute<DescriptionAttribute>() is DescriptionAttribute attribute
            ? attribute.Description
            : member.Name;
    }

    #endregion

    #region IsDeriveClassFrom(判断当前类型是否可由指定类型派生)

    /// <summary>
    /// 判断当前类型是否可由指定类型派生
    /// </summary>
    /// <typeparam name="TBaseType">基类型</typeparam>
    /// <param name="type">当前类型</param>
    /// <param name="canAbstract">能否是抽象类</param>
    public static Boolean IsDeriveClassFrom<TBaseType>(Type type, Boolean canAbstract = false) => IsDeriveClassFrom(type, typeof(TBaseType), canAbstract);

    /// <summary>
    /// 判断当前类型是否可由指定类型派生
    /// </summary>
    /// <param name="type">当前类型</param>
    /// <param name="baseType">基类型</param>
    /// <param name="canAbstract">能否是抽象类</param>
    public static Boolean IsDeriveClassFrom(Type type, Type baseType, Boolean canAbstract = false)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (baseType == null) throw new ArgumentNullException(nameof(baseType));

        return type.IsClass && (!canAbstract && !type.IsAbstract) && type.IsBaseOn(baseType);
    }

    #endregion

    #region IsBaseOn(返回当前类型是否是指定基类的派生类)

    /// <summary>
    /// 返回当前类型是否是指定基类的派生类
    /// </summary>
    /// <typeparam name="TBaseType">基类型</typeparam>
    /// <param name="type">类型</param>
    public static Boolean IsBaseOn<TBaseType>(Type type) => IsBaseOn(type, typeof(TBaseType));

    /// <summary>
    /// 返回当前类型是否是指定基类的派生类
    /// </summary>
    /// <param name="type">类型</param>
    /// <param name="baseType">基类类型</param>
    public static Boolean IsBaseOn(Type type, Type baseType) => baseType.IsGenericTypeDefinition
        ? baseType.IsGenericAssignableFrom(type)
        : baseType.IsAssignableFrom(type);

    #endregion

    #region IsGenericAssignableFrom(判断当前泛型类型是否可由指定类型的实例填充)

    /// <summary>
    /// 判断当前泛型类型是否可由指定类型的实例填充
    /// </summary>
    /// <param name="genericType">泛型类型</param>
    /// <param name="type">指定类型</param>
    public static Boolean IsGenericAssignableFrom(Type genericType, Type type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (genericType == null) throw new ArgumentNullException(nameof(genericType));

        if (!genericType.IsGenericType)
            throw new ArgumentException("该功能只支持泛型类型的调用，非泛型类型可使用 IsAssignableFrom 方法。");
        var allOthers = new List<Type>() { type };
        if (genericType.IsInterface) allOthers.AddRange(type.GetInterfaces());

        foreach (var other in allOthers)
        {
            var cur = other;
            while (cur != null)
            {
                if (cur.IsGenericType)
                    cur = cur.GetGenericTypeDefinition();
                if (cur.IsSubclassOf(genericType) || cur == genericType)
                    return true;
                cur = cur.BaseType;
            }
        }
        return false;
    }

    #endregion
}
