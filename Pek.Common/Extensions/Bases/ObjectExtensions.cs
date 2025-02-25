﻿using System.ComponentModel;
using System.Dynamic;

namespace Pek;

/// <summary>
/// 对象(<see cref="Object"/>) 扩展
/// </summary>
public static partial class ObjectExtensions
{
    #region PropertyClone 对象值克隆

    /// <summary>
    /// 从源对象赋值到当前对象
    /// </summary>
    /// <param name="destination">当前对象</param>
    /// <param name="source">数据源对象</param>
    /// <returns>成功复制的值个数</returns>
    public static Int32 ClonePropertyFrom(this Object destination, Object source) => destination.ClonePropertyFrom(source, null);

    /// <summary>
    /// 从源对象赋值到当前对象
    /// </summary>
    /// <param name="destination">当前对象</param>
    /// <param name="source">数据源对象</param>
    /// <param name="excludeName">排除下不要复制的属性名称</param>
    /// <returns>成功复制的值个数</returns>
    public static Int32 ClonePropertyFrom(this Object destination, Object source, IEnumerable<String>? excludeName)
    {
        if (source == null)
        {
            return 0;
        }
        return destination.ClonePropertyFrom(source, source.GetType(), excludeName);
    }

    /// <summary>
    /// 从源对象赋值到当前对象
    /// </summary>
    /// <param name="this">当前对象</param>
    /// <param name="source">数据源对象</param>
    /// <param name="type">复制的属性的类型</param>
    /// <param name="excludeName">排除不要复制属性名称</param>
    /// <returns>成功复制的值个数</returns>
    public static Int32 ClonePropertyFrom(this Object @this, Object source, Type type, IEnumerable<String>? excludeName)
    {
        if (@this == null || source == null)
        {
            return 0;
        }

        if (@this == source)
        {
            return 0;
        }

        excludeName ??= [];

        var i = 0;
        var desType = @this.GetType();
        foreach (var mi in type.GetFields())
        {
            if (excludeName.Contains(mi.Name))
            {
                continue;
            }
            try
            {
                var des = desType.GetField(mi.Name);
                if (des != null && des.FieldType == mi.FieldType)
                {
                    des.SetValue(@this, mi.GetValue(source));
                    i++;
                }
            }
            catch
            {
            }
        }

        foreach (var pi in type.GetProperties())
        {
            if (excludeName.Contains(pi.Name))
            {
                continue;
            }
            try
            {
                var des = desType.GetProperty(pi.Name);
                if (des != null && des.PropertyType == pi.PropertyType && des.CanWrite && pi.CanRead)
                {
                    des.SetValue(@this, pi.GetValue(source, null), null);
                    i++;
                }
            }
            catch
            {
            }
        }
        return i;
    }

    /// <summary>
    /// 从当前对象赋值到目标对象
    /// </summary>
    /// <param name="source">当前对象</param>
    /// <param name="destination">目标对象</param>
    /// <returns>成功复制的值个数</returns>
    public static Int32 ClonePropertyTo(this Object source, Object destination) => source.ClonePropertyTo(destination, null);

    /// <summary>
    /// 从当前对象赋值到目标对象
    /// </summary>
    /// <param name="source">当前对象</param>
    /// <param name="destination">目标对象</param>
    /// <param name="excludeName">排除下列名称的属性不要复制</param>
    /// <returns>成功复制的值个数</returns>
    public static Int32 ClonePropertyTo(this Object source, Object destination, IEnumerable<String>? excludeName)
    {
        if (destination == null)
        {
            return 0;
        }
        return destination.ClonePropertyFrom(source, source.GetType(), excludeName);
    }

    #endregion

    #region ToDynamic(将对象转换为dynamic)

    /// <summary>
    /// 将对象[主要是匿名对象]转换为dynamic
    /// </summary>
    public static dynamic? ToDynamic(this Object value)
    {
        IDictionary<String, Object?> expando = new ExpandoObject();
        var type = value.GetType();
        var properties = TypeDescriptor.GetProperties(type);
        foreach (PropertyDescriptor property in properties)
        {
            var val = property.GetValue(value);
            if (property.PropertyType.FullName?.StartsWith("<>f__AnonymousType") == true)
            {
                dynamic? dval = val?.ToDynamic();
                expando.Add(property.Name, dval);
            }
            else
            {
                expando.Add(property.Name, val);
            }
        }
        return expando as ExpandoObject;
    }

    #endregion

    #region 将对象[主要是匿名对象]转换为List<dynamic>
    /// <summary>
    /// 将对象[主要是匿名对象]转换为List&lt;dynamic&gt;
    /// </summary>
    public static List<dynamic?> ToDynamicList(this IEnumerable<dynamic> values)
    {
        var list = new List<dynamic?>();
        if (values != null)
        {
            if (values.Any())
            {
                list.AddRange(values.Select(v => ((Object)v).ToDynamic()));
            }
        }

        return list;
    }
    #endregion

    #region ToNullable(将指定值转换为对应的可空类型)

    /// <summary>
    /// 将指定值转换为对应的可空类型
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="value">值</param>
    /// <returns></returns>
    public static T? ToNullable<T>(this T value) where T : struct => value.IsNull() ? null : value;

    #endregion

    #region Locking(锁定)

    /// <summary>
    /// 锁定
    /// </summary>
    /// <param name="source">锁定对象</param>
    /// <param name="action">操作</param>
    public static void Locking(this Object source, Action action)
    {
        lock (source)
            action?.Invoke();
    }

    /// <summary>
    /// 锁定
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="source">锁定对象</param>
    /// <param name="action">操作</param>
    public static void Locking<T>(this T source, Action<T> action) where T : class
    {
        lock (source)
            action?.Invoke(source);
    }

    /// <summary>
    /// 锁定
    /// </summary>
    /// <typeparam name="TResult">结果类型</typeparam>
    /// <param name="source">锁定对象</param>
    /// <param name="func">执行函数</param>
    public static TResult Locking<TResult>(this Object source, Func<TResult> func)
    {
        lock (source)
            return func();
    }

    /// <summary>
    /// 锁定
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <typeparam name="TResult">结果类型</typeparam>
    /// <param name="source">锁定对象</param>
    /// <param name="func">执行函数</param>
    public static TResult Locking<T, TResult>(this T source, Func<T, TResult> func) where T : class
    {
        lock (source)
            return func(source);
    }

    #endregion
}