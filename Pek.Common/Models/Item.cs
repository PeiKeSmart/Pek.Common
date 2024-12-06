using System.Text.Json.Serialization;

namespace Pek;

/// <summary>
/// 列表项
/// </summary>
[Serializable]
public class Item : IComparable<Item>
{
    /// <summary>
    /// 初始化一个<see cref="Item"/>类型的实例
    /// </summary>
    public Item() { }

    /// <summary>
    /// 初始化一个<see cref="Item"/>类型的实例
    /// </summary>
    /// <param name="text">文本</param>
    /// <param name="value">值</param>
    /// <param name="sortId">排序号</param>
    /// <param name="group">组</param>
    /// <param name="disabled">禁用</param>
    public Item(String text, Object value, Int32? sortId = null, String? group = null, Boolean? disabled = null)
    {
        Text = text;
        Value = value;
        SortId = sortId;
        Group = group;
        Disabled = disabled;
    }

    /// <summary>
    /// 文本
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public String? Text { get; set; }

    /// <summary>
    /// 值
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Object? Value { get; set; }

    /// <summary>
    /// 排序号
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Int32? SortId { get; set; }

    /// <summary>
    /// 组
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public String? Group { get; set; }

    /// <summary>
    /// 禁用
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Boolean? Disabled { get; set; }

    /// <summary>
    /// 比较
    /// </summary>
    /// <param name="other">其他列表项</param>
    public Int32 CompareTo(Item? other) => String.Compare(Text, other?.Text, StringComparison.CurrentCulture);
}