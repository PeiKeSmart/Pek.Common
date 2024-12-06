using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.Common;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Web;

namespace Pek;

/// <summary>
/// 字典(<see cref="IDictionary{TKey,TValue}"/>) 扩展
/// </summary>
public static class DictionaryExtensions
{
    #region GetOrDefault(获取指定Key对应的Value，若未找到则返回默认值)

    /// <summary>
    /// 获取指定Key对应的Value，若未找到则返回默认值
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    /// <param name="dictionary">字典</param>
    /// <param name="key">键</param>
    /// <param name="defaultValue">默认值</param>
    public static TValue? GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
        TValue? defaultValue = default) =>
        dictionary.TryGetValue(key, out var obj) ? obj : defaultValue;

    #endregion

    #region AddRange(批量添加键值对到字典)

    /// <summary>
    /// 批量添加键值对到字典中
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    /// <param name="dict">字典</param>
    /// <param name="values">键值对集合</param>
    /// <param name="replaceExisted">是否替换已存在的键值对</param>
    public static IDictionary<TKey, TValue> AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dict,
        IEnumerable<KeyValuePair<TKey, TValue>> values, bool replaceExisted)
    {
        foreach (var item in values)
        {
            if (dict.ContainsKey(item.Key) && replaceExisted)
            {
                dict[item.Key] = item.Value;
                continue;
            }
            if (!dict.ContainsKey(item.Key))
                dict.Add(item.Key, item.Value);
        }
        return dict;
    }

    #endregion

    #region GetOrAdd(获取指定键的值，不存在则按指定委托添加值)

    /// <summary>
    /// 获取指定键的值，不存在则按指定委托添加值
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    /// <param name="dict">字典</param>
    /// <param name="key">键</param>
    /// <param name="setValue">添加值的委托</param>
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key,
        Func<TKey, TValue> setValue)
    {
        if (!dict.TryGetValue(key, out var value))
        {
            value = setValue(key);
            dict.Add(key, value);
        }
        return value;
    }

    /// <summary>
    /// 获取指定键的值，不存在则按指定委托添加值
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    /// <param name="dictionary">字典</param>
    /// <param name="key">键</param>
    /// <param name="addFunc">添加值的委托</param>
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
        Func<TValue> addFunc)
    {
        if (dictionary.TryGetValue(key, out var value))
            return value;
        return dictionary[key] = addFunc();
    }

    #endregion

    #region Sort(字段排序)

    /// <summary>
    /// 对指定的字典进行排序
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    /// <param name="dictionary">字典</param>
    public static IDictionary<TKey, TValue> Sort<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
    {
        if (dictionary == null)
            throw new ArgumentNullException(nameof(dictionary));
        return new SortedDictionary<TKey, TValue>(dictionary);
    }

    /// <summary>
    /// 对指定的字典进行排序
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    /// <param name="dictionary">字典</param>
    /// <param name="comparer">比较器，用于排序字典</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static IDictionary<TKey, TValue> Sort<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
        IComparer<TKey> comparer)
    {
        if (dictionary == null)
            throw new ArgumentNullException(nameof(dictionary));
        if (comparer == null)
            throw new ArgumentNullException(nameof(comparer));
        return new SortedDictionary<TKey, TValue>(dictionary, comparer);
    }

    /// <summary>
    /// 对指定的字典进行排序，根据值元素进行排序
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    /// <param name="dictionary">字典</param>
    public static IDictionary<TKey, TValue> SortByValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) =>
        new SortedDictionary<TKey, TValue>(dictionary).OrderBy(x => x.Value)
            .ToDictionary(x => x.Key, x => x.Value);

    #endregion

    #region ToQueryString(将字典转换成查询字符串)

    /// <summary>
    /// 将字典转换成查询字符串
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    /// <param name="dictionary">字典</param>
    public static string ToQueryString<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
    {
        if (dictionary == null || !dictionary.Any())
            return string.Empty;
        var sb = new StringBuilder();
        foreach (var item in dictionary)
            sb.Append($"{item.Key}={item.Value}&");
        sb.TrimEnd("&");
        return sb.ToString();
    }

    #endregion

    #region GetKey(根据Value反向查找Key)

    /// <summary>
    /// 根据Value反向查找Key
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    /// <param name="dictionary">字典</param>
    /// <param name="value">值</param>
    public static TKey? GetKey<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TValue value)
    {
        foreach (var item in dictionary.Where(x => x.Value?.Equals(value) == true))
            return item.Key;
        return default;
    }

    #endregion

    #region TryAdd(尝试添加键值对到字典)

    /// <summary>
    /// 尝试将键值对添加到字典中。如果不存在，则添加；存在，不添加也不抛异常
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    /// <param name="dictionary">字典</param>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    public static IDictionary<TKey, TValue> TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
        TKey key, TValue value)
    {
        if (!dictionary.ContainsKey(key))
            dictionary.Add(key, value);
        return dictionary;
    }

    #endregion

    #region ToHashTable(将字典转换成哈希表)

    /// <summary>
    /// 将字典转换成哈希表
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    /// <param name="dictionary">字典</param>
    public static Hashtable ToHashTable<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
    {
        var table = new Hashtable();
        foreach (var item in dictionary)
            table.Add(item.Key, item.Value);
        return table;
    }

    #endregion

    #region Invert(字典颠倒)

    /// <summary>
    /// 对指定字典进行颠倒键值对，创建新字典（值为键，键为值）
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    /// <param name="dictionary">字典</param>
    public static IDictionary<TValue, TKey> Reverse<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
    {
        if (dictionary == null)
            throw new ArgumentNullException(nameof(dictionary));
        return dictionary.ToDictionary(x => x.Value, x => x.Key);
    }

    #endregion

    #region AsReadOnly(转换成只读字典)

    /// <summary>
    /// 转换成只读字典
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    /// <param name="dictionary">字典</param>
    public static IReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) => new ReadOnlyDictionary<TKey, TValue>(dictionary);

    #endregion

    #region EqualsTo(判断两个字典中的元素是否相等)

    /// <summary>
    /// 判断两个字典中的元素是否相等
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    /// <param name="sourceDict">源字典</param>
    /// <param name="targetDict">目标字典</param>
    /// <exception cref="ArgumentNullException">源字典对象为空、目标字典对象为空</exception>
    public static bool EqualsTo<TKey, TValue>(this IDictionary<TKey, TValue> sourceDict,
        IDictionary<TKey, TValue> targetDict)
    {
        if (sourceDict == null)
            throw new ArgumentNullException(nameof(sourceDict), $@"源字典对象不可为空！");
        if (targetDict == null)
            throw new ArgumentNullException(nameof(sourceDict), $@"目标字典对象不可为空！");
        // 长度对比
        if (sourceDict.Count != targetDict.Count)
            return false;
        if (!sourceDict.Any() && !targetDict.Any())
            return true;
        // 深度对比
        var sourceKeyValues = sourceDict.OrderBy(x => x.Key).ToArray();
        var targetKeyValues = targetDict.OrderBy(x => x.Key).ToArray();
        var sourceKeys = sourceKeyValues.Select(x => x.Key);
        var targetKeys = targetKeyValues.Select(x => x.Key);
        var sourceValues = sourceKeyValues.Select(x => x.Value);
        var targetValues = targetKeyValues.Select(x => x.Value);
        if (sourceKeys.EqualsTo(targetKeys) && sourceValues.EqualsTo(targetValues))
            return true;
        return false;
    }

    #endregion

    #region FillFormDataStream(填充表单信息的Stream)

    /// <summary>
    /// 填充表单信息的Stream
    /// </summary>
    /// <param name="formData">表单数据</param>
    /// <param name="stream">流</param>
    public static void FillFormDataStream(this IDictionary<String, String> formData, Stream stream)
    {
        var dataStr = ToQueryString(formData);
        var formDataBytes = formData == null ? [] : Encoding.UTF8.GetBytes(dataStr);
        stream.Write(formDataBytes, 0, formDataBytes.Length);
        stream.Seek(0, SeekOrigin.Begin);// 设置指针读取位置
    }

    /// <summary>
    /// 填充表单信息的Stream
    /// </summary>
    /// <param name="formData">表单数据</param>
    /// <param name="stream">流</param>
    public static async Task FillFormDataStreamAsync(this IDictionary<String, String> formData, Stream stream)
    {
        var dataStr = ToQueryString(formData);
        var formDataBytes = formData == null ? [] : Encoding.UTF8.GetBytes(dataStr);
        await stream.WriteAsync(formDataBytes, 0, formDataBytes.Length).ConfigureAwait(false);
        stream.Seek(0, SeekOrigin.Begin);// 设置指针读取位置
    }

    #endregion

    /// <summary>
    /// 使用指定的函数，在 IDictionary&lt;TKey, TValue&gt;中如果键不存在，则添加键/值对；如果键已存在，则更新键/值对。
    /// </summary>
    /// <typeparam name="TKey">键的类型。</typeparam>
    /// <typeparam name="TValue">值的类型。</typeparam>
    /// <param name="this">要操作的对象 @this。</param>
    /// <param name="key">要添加或更新值的键。</param>
    /// <param name="value">要添加或更新的值。</param>
    /// <returns>键的新值。</returns>
    public static TValue AddOrUpdate<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> @this, TKey key, TValue value)
    {
        if (!@this.ContainsKey(key))
        {
            @this.Add(new KeyValuePair<TKey, TValue>(key, value));
        }
        else
        {
            @this[key] = value;
        }

        return @this[key];
    }

    /// <summary>
    /// 根据key获取Dictionary中元素
    /// </summary>
    /// <typeparam name="TKey">key类型</typeparam>
    /// <typeparam name="TValue">value类型</typeparam>
    /// <param name="dictionary">字典</param>
    /// <param name="key">key</param>
    /// <param name="value">value</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static Boolean TryGetValue<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> dictionary, TKey key, out TValue? value, TValue defaultValue)
    {
        var result = dictionary.TryGetValue(key, out value);
        if (!result)
        {
            value = defaultValue;
        }
        return result;
    }

    /// <summary>
    ///     An IDictionary&lt;TKey,TValue&gt; extension method that adds if not contains key.
    /// </summary>
    /// <typeparam name="TKey">Type of the key.</typeparam>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns>true if it succeeds, false if it fails.</returns>
    public static Boolean AddIfNotContainsKey<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> @this, TKey key, TValue value)
    {
        if (!@this.ContainsKey(key))
        {
            @this.Add(key, value);
            return true;
        }

        return false;
    }

    /// <summary>
    ///     An IDictionary&lt;TKey,TValue&gt; extension method that adds if not contains key.
    /// </summary>
    /// <typeparam name="TKey">Type of the key.</typeparam>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <param name="key">The key.</param>
    /// <param name="valueFactory">The value factory.</param>
    /// <returns>true if it succeeds, false if it fails.</returns>
    public static Boolean AddIfNotContainsKey<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> @this, TKey key, Func<TValue> valueFactory)
    {
        if (!@this.ContainsKey(key))
        {
            @this.Add(key, valueFactory());
            return true;
        }

        return false;
    }

    /// <summary>
    ///     An IDictionary&lt;TKey,TValue&gt; extension method that adds if not contains key.
    /// </summary>
    /// <typeparam name="TKey">Type of the key.</typeparam>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <param name="key">The key.</param>
    /// <param name="valueFactory">The value factory.</param>
    /// <returns>true if it succeeds, false if it fails.</returns>
    public static Boolean AddIfNotContainsKey<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> @this, TKey key, Func<TKey, TValue> valueFactory)
    {
        if (!@this.ContainsKey(key))
        {
            @this.Add(key, valueFactory(key));
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Adds a key/value pair to the IDictionary&lt;TKey, TValue&gt; if the key does not already exist.
    /// </summary>
    /// <typeparam name="TKey">Type of the key.</typeparam>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The value to be added, if the key does not already exist.</param>
    /// <returns>
    ///     The value for the key. This will be either the existing value for the key if the key is already in the
    ///     dictionary, or the new value if the key was not in the dictionary.
    /// </returns>
    public static TValue GetOrAdd<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> @this, TKey key, TValue value)
    {
        if (!@this.ContainsKey(key))
        {
            @this.Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        return @this[key];
    }

    /// <summary>
    ///     Uses the specified functions to add a key/value pair to the IDictionary&lt;TKey, TValue&gt; if the key does
    ///     not already exist, or to update a key/value pair in the IDictionary&lt;TKey, TValue&gt;> if the key already
    ///     exists.
    /// </summary>
    /// <typeparam name="TKey">Type of the key.</typeparam>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <param name="key">The key to be added or whose value should be updated.</param>
    /// <param name="addValue">The value to be added for an absent key.</param>
    /// <param name="updateValueFactory">
    ///     The function used to generate a new value for an existing key based on the key's
    ///     existing value.
    /// </param>
    /// <returns>
    ///     The new value for the key. This will be either be addValue (if the key was absent) or the result of
    ///     updateValueFactory (if the key was present).
    /// </returns>
    public static TValue AddOrUpdate<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> @this, TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
    {
        if (!@this.ContainsKey(key))
        {
            @this.Add(new KeyValuePair<TKey, TValue>(key, addValue));
        }
        else
        {
            @this[key] = updateValueFactory(key, @this[key]);
        }

        return @this[key];
    }

    /// <summary>
    ///     Uses the specified functions to add a key/value pair to the IDictionary&lt;TKey, TValue&gt; if the key does
    ///     not already exist, or to update a key/value pair in the IDictionary&lt;TKey, TValue&gt;> if the key already
    ///     exists.
    /// </summary>
    /// <typeparam name="TKey">Type of the key.</typeparam>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <param name="key">The key to be added or whose value should be updated.</param>
    /// <param name="addValueFactory">The function used to generate a value for an absent key.</param>
    /// <param name="updateValueFactory">
    ///     The function used to generate a new value for an existing key based on the key's
    ///     existing value.
    /// </param>
    /// <returns>
    ///     The new value for the key. This will be either be the result of addValueFactory (if the key was absent) or
    ///     the result of updateValueFactory (if the key was present).
    /// </returns>
    public static TValue AddOrUpdate<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> @this, TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
    {
        if (!@this.ContainsKey(key))
        {
            @this.Add(new KeyValuePair<TKey, TValue>(key, addValueFactory(key)));
        }
        else
        {
            @this[key] = updateValueFactory(key, @this[key]);
        }

        return @this[key];
    }

    /// <summary>
    ///     An IDictionary&lt;TKey,TValue&gt; extension method that removes if contains key.
    /// </summary>
    /// <typeparam name="TKey">Type of the key.</typeparam>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <param name="key">The key.</param>
    public static void RemoveIfContainsKey<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> @this, TKey key)
    {
        if (@this.ContainsKey(key))
        {
            @this.Remove(key);
        }
    }

    /// <summary>
    ///     An IDictionary&lt;TKey,TValue&gt; extension method that converts the @this to a sorted dictionary.
    /// </summary>
    /// <typeparam name="TKey">Type of the key.</typeparam>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <returns>@this as a SortedDictionary&lt;TKey,TValue&gt;</returns>
    public static SortedDictionary<TKey, TValue> ToSortedDictionary<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> @this)
    {
        return new SortedDictionary<TKey, TValue>(@this);
    }

    /// <summary>
    ///     An IDictionary&lt;TKey,TValue&gt; extension method that converts the @this to a sorted dictionary.
    /// </summary>
    /// <typeparam name="TKey">Type of the key.</typeparam>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <param name="comparer">The comparer.</param>
    /// <returns>@this as a SortedDictionary&lt;TKey,TValue&gt;</returns>
    public static SortedDictionary<TKey, TValue> ToSortedDictionary<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> @this, IComparer<TKey> comparer)
    {
        return new SortedDictionary<TKey, TValue>(@this, comparer);
    }

    /// <summary>
    ///     An IDictionary&lt;TKey,TValue&gt; extension method that query if '@this' contains any key.
    /// </summary>
    /// <typeparam name="TKey">Type of the key.</typeparam>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <param name="keys">A variable-length parameters list containing keys.</param>
    /// <returns>true if it succeeds, false if it fails.</returns>
    public static bool ContainsAnyKey<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> @this, params TKey[] keys)
    {
        foreach (var value in keys)
        {
            if (@this.ContainsKey(value))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     An IDictionary&lt;TKey,TValue&gt; extension method that query if '@this' contains all key.
    /// </summary>
    /// <typeparam name="TKey">Type of the key.</typeparam>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <param name="keys">A variable-length parameters list containing keys.</param>
    /// <returns>true if it succeeds, false if it fails.</returns>
    public static bool ContainsAllKey<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> @this, params TKey[] keys)
    {
        foreach (var value in keys)
        {
            if (!@this.ContainsKey(value))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     An IDictionary&lt;string,string&gt; extension method that converts the @this to a name value collection.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>@this as a NameValueCollection.</returns>
    public static NameValueCollection ToNameValueCollection(this IDictionary<string, string> @this)
    {
        if (@this == null)
        {
            return null;
        }
        var col = new NameValueCollection();
        foreach (var item in @this)
        {
            col.Add(item.Key, item.Value);
        }
        return col;
    }

    /// <summary>
    ///     An IDictionary&lt;string,object&gt; extension method that converts this object to a database parameters.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="command">The command.</param>
    /// <returns>The given data converted to a DbParameter[].</returns>
    public static DbParameter[] ToDbParameters([NotNull] this IDictionary<string, object> @this, DbCommand command)
    {
        return @this.Select(x =>
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = x.Key;
            parameter.Value = x.Value;
            return parameter;
        }).ToArray();
    }

    /// <summary>
    ///     An IDictionary&lt;string,object&gt; extension method that converts this object to a database parameters.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="connection">The connection.</param>
    /// <returns>The given data converted to a DbParameter[].</returns>
    public static DbParameter[] ToDbParameters([NotNull] this IDictionary<string, object> @this, DbConnection connection)
    {
        var command = connection.CreateCommand();

        return @this.Select(x =>
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = x.Key;
            parameter.Value = x.Value;
            return parameter;
        }).ToArray();
    }

    /// <summary>
    /// IDictionary to dataTable
    /// </summary>
    /// <param name="dictionary">IDictionary</param>
    /// <returns></returns>
    public static DataTable ToDataTable([NotNull] this IDictionary<string, object> dictionary)
    {
        if (null == dictionary)
        {
            throw new ArgumentNullException(nameof(dictionary));
        }
        var dataTable = new DataTable();
        if (dictionary.Keys.Count == 0)
        {
            return dataTable;
        }
        dataTable.Columns.AddRange(dictionary.Keys.Select(key => new DataColumn(key, dictionary[key].GetType())).ToArray());
        foreach (var key in dictionary.Keys)
        {
            var row = dataTable.NewRow();
            row[key] = dictionary[key];
            dataTable.Rows.Add(row);
        }
        return dataTable;
    }

    public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>([NotNull] this IEnumerable<KeyValuePair<TKey, TValue>> source) => source.ToDictionary(pair => pair.Key, pair => pair.Value);

    public static IEnumerable<KeyValuePair<string, string>> ToKeyValuePair(this NameValueCollection collection)
    {
        if (collection == null || collection.Count == 0)
        {
            yield break;
        }

        foreach (var key in collection.AllKeys)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            yield return new KeyValuePair<string, string>(key, collection[key]);
        }
    }

    public static NameValueCollection ToNameValueCollection(this IEnumerable<KeyValuePair<string, string>> source)
    {
        if (source == null)
        {
            return null;
        }

        var collection = new NameValueCollection();

        foreach (var item in source)
        {
            if (string.IsNullOrWhiteSpace(item.Key))
            {
                continue;
            }
            collection.Add(item.Key, item.Value);
        }

        return collection;
    }

    /// <summary>将键值集合转换成字符串，key1=value1&amp;key2=value2，k/v会编码</summary>
    /// <param name="source">数据源</param>
    /// <returns>字符串</returns>
    public static string ToQueryString(this IEnumerable<KeyValuePair<string, string>> source)
    {
        if (source == null)
        {
            return string.Empty;
        }

        var sb = new StringBuilder(1024);

        foreach (var item in source)
        {
            if (string.IsNullOrWhiteSpace(item.Key))
            {
                continue;
            }
            sb.Append("&");
            sb.Append(HttpUtility.UrlEncode(item.Key));
            sb.Append("=");
            if (item.Value != null)
                sb.Append(HttpUtility.UrlEncode(item.Value));
        }

        return sb.Length > 1 ? sb.ToString(1, sb.Length - 1) : "";
    }
}