using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Runtime.World;

/// <summary>
/// RuntimeWorld 内部的 world event bus 句柄。
/// </summary>
public interface IWorldEventBus : IEventBus, IWorldEventBusRouter
{
    /// <summary>清空订阅与派发中状态。</summary>
    void Clear();
}
