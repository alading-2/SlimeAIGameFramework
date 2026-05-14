namespace SlimeAI.GameOS.Runtime.Entity;

/// <summary>
/// Runtime Entity 最小契约。
/// </summary>
public interface IEntity
{
    /// <summary>
    /// 稳定运行时实体 Id。
    /// </summary>
    EntityId EntityId { get; }

    /// <summary>
    /// 共享运行时状态容器。
    /// </summary>
    Data.Data Data { get; }

    /// <summary>
    /// 实体局部事件总线，用于组件式通信。
    /// </summary>
    SlimeAI.GameOS.Runtime.Event.IEventBus Events { get; }
}
