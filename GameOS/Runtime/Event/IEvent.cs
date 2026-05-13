namespace SlimeAI.GameOS.Runtime.Event;

/// <summary>
/// Runtime 事件的基础 marker interface。所有事件 payload MUST 实现 IEntityEvent / IGlobalEvent / IBroadcastEvent 之一。
/// </summary>
public interface IEvent
{
}

/// <summary>
/// 只派发到目标实体 bus 的事件。
/// </summary>
public interface IEntityEvent : IEvent
{
}

/// <summary>
/// 只派发到 world bus 的全局事件。
/// </summary>
public interface IGlobalEvent : IEvent
{
}

/// <summary>
/// 在实体 bus 和 world bus 同时派发的广播事件。
/// </summary>
public interface IBroadcastEvent : IEntityEvent, IGlobalEvent
{
}
