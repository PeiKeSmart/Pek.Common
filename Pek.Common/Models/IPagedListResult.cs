using System.Diagnostics.CodeAnalysis;

namespace Pek.Models;

[Obsolete("Please use IPagedListResult", true)]
public interface IPagedListModel<out T> : IPagedListResult<T>
{
}

public interface IPagedListResult<out T>
{
    /// <summary>
    /// Data
    /// </summary>
    IReadOnlyList<T> Data { get; }

    Int32 Count { get; }

    /// <summary>
    /// PageNumber
    /// </summary>
    Int32 PageNumber { get; }

    /// <summary>
    /// PageSize
    /// </summary>
    Int32 PageSize { get; }

    /// <summary>
    /// TotalDataCount
    /// </summary>
    Int32 TotalCount { get; set; }

    /// <summary>
    /// PageCount
    /// </summary>
    Int32 PageCount { get; }
}

/// <summary>
/// 分页Model
/// </summary>
/// <typeparam name="T">Type</typeparam>
[Serializable]
public class PagedListResult<T> : IPagedListResult<T>
{
    public static readonly IPagedListResult<T> Empty = new PagedListResult<T>();

    private IReadOnlyList<T> _data = [];

    [NotNull]
    public IReadOnlyList<T> Data
    {
        get => _data;
        set
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (value != null)
            {
                _data = value;
            }
        }
    }

    private Int32 _pageNumber = 1;

    public Int32 PageNumber
    {
        get => _pageNumber;
        set
        {
            if (value > 0)
            {
                _pageNumber = value;
            }
        }
    }

    private Int32 _pageSize = 10;

    public Int32 PageSize
    {
        get => _pageSize;
        set
        {
            if (value > 0)
            {
                _pageSize = value;
            }
        }
    }

    private Int32 _totalCount;

    public Int32 TotalCount
    {
        get => _totalCount;
        set
        {
            if (value > 0)
            {
                _totalCount = value;
            }
        }
    }

    public Int32 PageCount => (_totalCount + _pageSize - 1) / _pageSize;

    public T this[Int32 index] => Data[index];

    public Int32 Count => Data.Count;
}