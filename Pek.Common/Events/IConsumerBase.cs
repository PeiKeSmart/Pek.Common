namespace Pek.Events;

/// <summary>
/// 消费者基类
/// </summary>
public interface IConsumerBase
{
    /// <summary>
    /// 排序
    /// </summary>
    Int32 Sort { get; set; }
}