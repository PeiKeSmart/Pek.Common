﻿using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using Pek.Helpers;

namespace Pek;

public static class TypeExtension
{
    /// <summary>
    /// 基础类型
    /// </summary>
    private static readonly Type[] BasicTypes =
    {
            typeof(Boolean),

            typeof(SByte),
            typeof(Byte),
            typeof(Int32),
            typeof(UInt32),
            typeof(Int16),
            typeof(UInt16),
            typeof(Int64),
            typeof(UInt64),
            typeof(Single),
            typeof(Double),
            typeof(Decimal),

            typeof(Guid),

            typeof(DateTime),// IsPrimitive:False
            typeof(TimeSpan),// IsPrimitive:False
            typeof(DateTimeOffset),

            typeof(Char),
            typeof(String),// IsPrimitive:False

            //typeof(object),// IsPrimitive:False
        };

    /// <summary>
    /// 是否是 ValueTuple
    /// </summary>
    /// <param name="type">type</param>
    /// <returns></returns>
    public static Boolean IsValueTuple([NotNull] this Type type)
            => type.IsValueType && type.FullName?.StartsWith("System.ValueTuple`", StringComparison.Ordinal) == true;

    /// <summary>
    /// GetDescription
    /// </summary>
    /// <param name="type">type</param>
    /// <returns></returns>
    public static String GetDescription([NotNull] this Type type) =>
        type.GetCustomAttribute<DescriptionAttribute>()?.Description ?? String.Empty;

    /// <summary>
    /// 判断是否基元类型，如果是可空类型会先获取里面的类型，如 int? 也是基元类型
    /// The primitive types are Boolean, Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, IntPtr, UIntPtr, Char, Double, and Single.
    /// </summary>
    /// <param name="type">type</param>
    /// <returns></returns>
    public static Boolean IsPrimitiveType([NotNull] this Type type)
        => (Nullable.GetUnderlyingType(type) ?? type).IsPrimitive;

    public static Boolean IsPrimitiveType<T>() => typeof(T).IsPrimitiveType();

    public static Boolean IsBasicType([NotNull] this Type type)
    {
        var unWrappedType = type.Unwrap();
        return unWrappedType.IsEnum || BasicTypes.Contains(unWrappedType);
    }

    public static Boolean IsBasicType<T>() => typeof(T).IsBasicType();

    public static Boolean IsBasicType<T>(this T value) => typeof(T).IsBasicType();

    public static Boolean HasNamespace(this Type type) => type?.Namespace != null;

    /// <summary>
    /// Finds best constructor, least parameter
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="parameterTypes"></param>
    /// <returns>Matching constructor or default one</returns>
    public static ConstructorInfo? GetConstructor(this Type type, params Type[] parameterTypes)
    {
        if (parameterTypes == null || parameterTypes.Length == 0)
            return GetEmptyConstructor(type);

        ActivatorHelper.FindApplicableConstructor(type, parameterTypes, out var ctor, out _);
        return ctor;
    }

    public static ConstructorInfo? GetEmptyConstructor(this Type type)
    {
        var constructors = type.GetConstructors();

        var ctor = constructors.OrderBy(c => c.IsPublic ? 0 : (c.IsPrivate ? 2 : 1))
            .ThenBy(c => c.GetParameters().Length).FirstOrDefault();

        return ctor?.GetParameters().Length == 0 ? ctor : null;
    }

    /// <summary>
    /// Determines whether this type is assignable to <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to test assignability to.</typeparam>
    /// <param name="this">The type to test.</param>
    /// <returns>True if this type is assignable to references of type
    /// <typeparamref name="T"/>; otherwise, False.</returns>
    public static Boolean IsAssignableTo<T>(this Type @this) => @this == null ? throw new ArgumentNullException(nameof(@this)) : typeof(T).IsAssignableFrom(@this);

    /// <summary>
    /// Finds a constructor with the matching type parameters.
    /// </summary>
    /// <param name="type">The type being tested.</param>
    /// <param name="constructorParameterTypes">The types of the contractor to find.</param>
    /// <returns>The <see cref="ConstructorInfo"/> is a match is found; otherwise, <c>null</c>.</returns>
    public static ConstructorInfo? GetMatchingConstructor(this Type type, Type[] constructorParameterTypes)
    {
        if (constructorParameterTypes == null || constructorParameterTypes.Length == 0)
            return GetEmptyConstructor(type);

        return type.GetConstructors()
            .FirstOrDefault(c => c.GetParameters()
                .Select(p => p.ParameterType)
                .SequenceEqual(constructorParameterTypes)
            );
    }

    /// <summary>
    /// Get ImplementedInterfaces
    /// </summary>
    /// <param name="type">type</param>
    /// <returns></returns>
    public static IEnumerable<Type> GetImplementedInterfaces([NotNull] this Type type) => type.GetTypeInfo().ImplementedInterfaces;
}