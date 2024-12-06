using System.Collections.Concurrent;
using System.ComponentModel.Design;
using System.Reflection;

namespace Pek.Helpers;

public static class CacheUtil
{
    /// <summary>
    /// TypePropertyCache
    /// </summary>
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> TypePropertyCache = new();

    public static PropertyInfo[] GetTypeProperties(Type type) => null == type ? throw new ArgumentNullException(nameof(type)) : TypePropertyCache.GetOrAdd(type, t => t.GetProperties());

    public static FieldInfo[] GetTypeFields(Type type) => null == type ? throw new ArgumentNullException(nameof(type)) : TypeFieldCache.GetOrAdd(type, t => t.GetFields());

    private static readonly ConcurrentDictionary<Type, FieldInfo[]> TypeFieldCache = new();

    internal static readonly ConcurrentDictionary<Type, MethodInfo[]> TypeMethodCache = new();

    internal static readonly ConcurrentDictionary<Type, Func<ServiceContainer, Object>> TypeNewFuncCache = new();

    internal static readonly ConcurrentDictionary<Type, ConstructorInfo> TypeConstructorCache = new();

    internal static readonly ConcurrentDictionary<Type, Func<Object>> TypeEmptyConstructorFuncCache = new();

    internal static readonly ConcurrentDictionary<Type, Func<Object[], Object>> TypeConstructorFuncCache = new();

    internal static readonly ConcurrentDictionary<PropertyInfo, Func<Object, Object>> PropertyValueGetters = new();

    internal static readonly ConcurrentDictionary<PropertyInfo, Action<Object, Object>> PropertyValueSetters = new();
}

internal static class StrongTypedCache<T>
{
    public static readonly ConcurrentDictionary<PropertyInfo, Func<T, Object>> PropertyValueGetters = new();

    public static readonly ConcurrentDictionary<PropertyInfo, Action<T, Object>> PropertyValueSetters = new();
}