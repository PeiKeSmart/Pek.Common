namespace Pek;

/// <summary>
/// 集合(<see cref="ICollection{T}"/>) 扩展
/// </summary>
public static class CollectionExtensions
{
    #region AddIfNotContains(添加项。如果未包含，则添加)

    /// <summary>
    /// 添加项。如果未包含，则添加
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="source">集合</param>
    /// <param name="item">项</param>
    public static Boolean AddIfNotContains<T>(this ICollection<T> source, T item)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        if (source.Contains(item))
            return false;
        source.Add(item);
        return true;
    }

    /// <summary>
    /// 添加项集合。如果未包含，则添加
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="source">集合</param>
    /// <param name="items">项集合</param>
    public static IEnumerable<T> AddIfNotContains<T>(this ICollection<T> source, IEnumerable<T> items)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var addedItems = new List<T>();
        foreach (var item in items)
        {
            if (source.Contains(item))
                continue;
            source.Add(item);
            addedItems.Add(item);
        }
        return addedItems;
    }

    /// <summary>
    /// 添加项。如果未包含
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="source">集合</param>
    /// <param name="predicate">条件</param>
    /// <param name="itemFactory">获取项函数</param>
    public static Boolean AddIfNotContains<T>(this ICollection<T> source, Func<T, Boolean> predicate, Func<T> itemFactory)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        if (itemFactory == null) throw new ArgumentNullException(nameof(itemFactory));

        if (source.Any(predicate))
            return false;
        source.Add(itemFactory());
        return true;
    }

    #endregion

    #region RemoveAll(移除项)

    /// <summary>
    /// 移除项。指定集合
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="source">集合</param>
    /// <param name="items">集合项</param>
    public static void RemoveAll<T>(this ICollection<T> source, IEnumerable<T> items)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (items == null) throw new ArgumentNullException(nameof(items));

        foreach (var item in items)
            source.Remove(item);
    }

    /// <summary>
    /// 移除项。按条件移除
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="source">集合</param>
    /// <param name="predicate">条件</param>
    public static IList<T> RemoveAll<T>(this ICollection<T> source, Func<T, Boolean> predicate)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        var items = source.Where(predicate).ToList();
        foreach (var item in items)
            source.Remove(item);
        return items;
    }

    #endregion

    #region AddRange(添加批量项)

    /// <summary>
    /// 添加批量项。
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="collection">集合</param>
    /// <param name="enumerable">元素集合</param>
    /// <exception cref="ArgumentNullException">源集合对象为空、添加的集合项为空</exception>
    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> enumerable)
    {
        if (collection == null)
            throw new ArgumentNullException(nameof(collection), $@"源{typeof(T).Name}集合对象不可为空！");
        if (enumerable == null)
            throw new ArgumentNullException(nameof(enumerable), $@"要添加的{typeof(T).Name}集合项不可为空！");
        enumerable.ForEach(collection.Add);
    }

    /// <summary>
    /// 添加多个元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="values"></param>
    public static void AddRange<T>(this ICollection<T> @this, params T[] values)
    {
        foreach (var obj in values)
        {
            @this.Add(obj);
        }
    }

    #endregion

    #region Sort(排序)

    /// <summary>
    /// 排序
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="collection">集合</param>
    /// <param name="comparer">比较器</param>
    public static void Sort<T>(this ICollection<T> collection, IComparer<T>? comparer = null)
    {
        comparer ??= Comparer<T>.Default;
        var list = new List<T>(collection);
        list.Sort(comparer);
        collection.ReplaceItems(list);
    }

    #endregion

    #region ReplaceItems(替换项)

    /// <summary>
    /// 替换项
    /// </summary>
    /// <typeparam name="TItem">项类型</typeparam>
    /// <typeparam name="TNewItem">新项类型</typeparam>
    /// <param name="collection">集合</param>
    /// <param name="newItems">新项集合</param>
    /// <param name="createItemAction">创建项操作</param>
    public static void ReplaceItems<TItem, TNewItem>(this ICollection<TItem> collection,
        IEnumerable<TNewItem> newItems, Func<TNewItem, TItem> createItemAction)
    {
        if (collection == null) throw new ArgumentNullException(nameof(collection));
        if (newItems == null) throw new ArgumentNullException(nameof(newItems));
        if (createItemAction == null) throw new ArgumentNullException(nameof(createItemAction));

        collection.Clear();
        var convertedNewItems = newItems.Select(createItemAction);
        collection.AddRange(convertedNewItems);
    }

    /// <summary>
    /// 替换项
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="collection">集合</param>
    /// <param name="newItems">新项集合</param>
    public static void ReplaceItems<T>(this ICollection<T> collection, IEnumerable<T> newItems)
    {
        if (collection == null) throw new ArgumentNullException(nameof(collection));
        if (newItems == null) throw new ArgumentNullException(nameof(newItems));

        collection.ReplaceItems(newItems, x => x);
    }

    #endregion

}