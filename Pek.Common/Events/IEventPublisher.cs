namespace Pek.Events;

/// <summary>
/// 表示事件发布者
/// </summary>
public partial interface IEventPublisher
{
    /// <summary>
    /// 发布事件给消费者
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <param name="event">事件对象</param>
    /// <returns>一个表示异步操作的任务</returns>
    Task PublishAsync<TEvent>(TEvent @event);

    /// <summary>
    /// 发布事件到消费者
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <param name="event">事件对象</param>
    void Publish<TEvent>(TEvent @event);
}