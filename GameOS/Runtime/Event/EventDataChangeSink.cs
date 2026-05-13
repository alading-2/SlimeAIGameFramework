using SlimeAI.GameOS.Runtime.Data;
using SlimeAI.GameOS.Runtime.Events.Core;

namespace SlimeAI.GameOS.Runtime.Event;

/// <summary>
/// 把 Data 层的变化桥接到实体 EventBus，发布 Core.DataPropertyChanged 事件。
/// </summary>
public sealed class EventDataChangeSink : IDataChangeSink
{
    private readonly IEventBus events;

    /// <summary>
    /// 创建 Data 变化转事件的 sink。
    /// </summary>
    /// <param name="events">接收事件的实体 EventBus。</param>
    public EventDataChangeSink(IEventBus events)
    {
        this.events = events;
    }

    /// <inheritdoc />
    public void OnDataChanged(DataChangedEventData change)
    {
        events.Publish(new DataPropertyChanged(change));
    }
}
