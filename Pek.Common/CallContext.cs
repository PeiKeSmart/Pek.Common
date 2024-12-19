using System.Collections.Concurrent;

namespace Pek;

/// <summary>
/// 取线程内唯一对象
/// </summary>
public static class CallContext
{
    static readonly ConcurrentDictionary<String, AsyncLocal<Object>> state = new();

    public static void SetData(String name, Object data) =>
        state.GetOrAdd(name, _ => new AsyncLocal<Object>()).Value = data;

    public static Object? GetData(String name) =>
        state.TryGetValue(name, out var data) ? data.Value : null;
}

/// <summary>
/// 取线程内唯一对象
/// </summary>
/// <typeparam name="T"></typeparam>
public static class CallContext<T>
{
    static readonly ConcurrentDictionary<String, AsyncLocal<T>> state = new();

    public static void SetData(String name, T data) => state.GetOrAdd(name, _ => new AsyncLocal<T>()).Value = data;

    public static T? GetData(String name) => state.TryGetValue(name, out var data) ? data.Value : default;
}