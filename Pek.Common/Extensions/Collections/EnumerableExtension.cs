﻿using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

using Pek.Models;

namespace Pek;

public static class EnumerableExtension
{
    public static void ForEach<T>([NotNull] this IEnumerable<T> ts, Action<T, Int32> action)
    {
        var i = 0;
        foreach (var t in ts)
        {
            action(t, i);
            i++;
        }
    }

    public static async Task ForEachAsync<T>([NotNull] this IEnumerable<T> ts, Func<T, Task> action)
    {
        foreach (var t in ts)
        {
            await action(t).ConfigureAwait(false);
        }
    }

    public static async Task ForEachAsync<T>([NotNull] this IEnumerable<T> ts, Func<T, Int32, Task> action)
    {
        var i = 0;
        foreach (var t in ts)
        {
            await action(t, i).ConfigureAwait(false);
            i++;
        }
    }

    /// <summary>
    ///     A T[] extension method that converts an array to a read only.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <returns>A list of.</returns>
    public static ReadOnlyCollection<T> AsReadOnly<T>([NotNull] this IEnumerable<T> @this) => Array.AsReadOnly(@this.ToArray());

    /// <summary>
    ///     An IEnumerable&lt;T&gt; extension method that queries if a not null or is empty.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="this">The collection to act on.</param>
    /// <returns>true if a not null or is t>, false if not.</returns>
    public static Boolean IsNotNullOrEmpty<T>(this IEnumerable<T> @this) => @this != null && @this.Any();

    /// <summary>
    ///     Concatenates all the elements of a IEnumerable, using the specified separator between each element.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="this">An IEnumerable that contains the elements to concatenate.</param>
    /// <param name="separator">
    ///     The string to use as a separator. separator is included in the returned string only if
    ///     value has more than one element.
    /// </param>
    /// <returns>
    ///     A string that consists of the elements in value delimited by the separator string. If value is an empty array,
    ///     the method returns String.Empty.
    /// </returns>
    public static String StringJoin<T>([NotNull] this IEnumerable<T> @this, String separator) => String.Join(separator, @this);

    public static IEnumerable<TSource> Prepend<TSource>([NotNull] this IEnumerable<TSource> source, TSource value)
    {
        yield return value;

        foreach (var element in source)
        {
            yield return element;
        }
    }

    public static IEnumerable<TSource> Append<TSource>([NotNull] this IEnumerable<TSource> source, TSource value)
    {
        foreach (var element in source)
        {
            yield return element;
        }

        yield return value;
    }

    #region Split

    /// <summary>
    /// 将一维集合分割成二维集合
    /// </summary>
    /// <param name="source">source</param>
    /// <param name="batchSize">每个一维集合的数量</param>
    public static IEnumerable<T[]> Split<T>(this IEnumerable<T> source, Int32 batchSize)
    {
        using var enumerator = source.GetEnumerator();
        while (enumerator.MoveNext())
        {
            yield return Split(enumerator, batchSize).ToArray();
        }
    }

    private static IEnumerable<T> Split<T>(IEnumerator<T> enumerator, Int32 batchSize)
    {
        do
        {
            yield return enumerator.Current;
        } while (--batchSize > 0 && enumerator.MoveNext());
    }

    #endregion Split

    public static IEnumerable<T>? WhereNotNull<T>(this IEnumerable<T> source) where T : class
        => source?.Where(_ => _ != null);

    public static IEnumerable<T> Distinct<T>(this IEnumerable<T> source, Func<T?, T?, Boolean> comparer) where T : class
        => source.Distinct(new DynamicEqualityComparer<T>(comparer));

    // https://github.com/aspnet/EntityFrameworkCore/blob/release/3.0/src/EFCore.SqlServer/Utilities/EnumerableExtensions.cs
    private sealed class DynamicEqualityComparer<T>(Func<T?, T?, Boolean> func) : IEqualityComparer<T>
        where T : class
    {
        private readonly Func<T?, T?, Boolean> _func = func;

        public Boolean Equals(T? x, T? y) => _func(x, y);

        public Int32 GetHashCode(T obj) => 0; // force Equals
    }

    #region Linq

    /// <summary>
    /// LeftJoin extension
    /// </summary>
    /// <typeparam name="TOuter">outer</typeparam>
    /// <typeparam name="TInner">inner</typeparam>
    /// <typeparam name="TKey">TKey</typeparam>
    /// <typeparam name="TResult">TResult</typeparam>
    /// <param name="outer">outer collection</param>
    /// <param name="inner">inner collection</param>
    /// <param name="outerKeySelector">outerKeySelector</param>
    /// <param name="innerKeySelector">innerKeySelector</param>
    /// <param name="resultSelector">resultSelector</param>
    /// <returns></returns>
    public static IEnumerable<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer,
        IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector,
        Func<TOuter, TInner?, TResult> resultSelector)
    {
        return outer
            .GroupJoin(inner, outerKeySelector, innerKeySelector, (outerObj, inners) => new
            {
                outerObj,
                inners = inners.DefaultIfEmpty()
            })
            .SelectMany(a => a.inners.Select(innerObj => resultSelector(a.outerObj, innerObj)));
    }

    #endregion Linq

    #region ToPagedList

    /// <summary>
    /// ToPagedList
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="data">data</param>
    /// <param name="pageNumber">pageNumber</param>
    /// <param name="pageSize">pageSize</param>
    /// <param name="totalCount">totalCount</param>
    /// <returns></returns>
    public static IPagedListResult<T> ToPagedList<T>([NotNull] this IEnumerable<T> data, Int32 pageNumber, Int32 pageSize, Int32 totalCount)
        => new PagedListResult<T>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            Data = data is IReadOnlyList<T> dataList ? dataList : data.ToArray()
        };

    /// <summary>
    /// ToPagedList
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="data">data</param>
    /// <param name="pageNumber">pageNumber</param>
    /// <param name="pageSize">pageSize</param>
    /// <param name="totalCount">totalCount</param>
    /// <returns></returns>
    public static IPagedListResult<T> ToPagedList<T>([NotNull] this IReadOnlyList<T> data, Int32 pageNumber, Int32 pageSize, Int32 totalCount)
        => new PagedListResult<T>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            Data = data
        };

    #endregion ToPagedList
}