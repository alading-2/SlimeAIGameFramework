using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.Runtime.Event;

/// <summary>
/// Entity 运行时事件名和 payload。
/// </summary>
public static partial class GameEventType
{
    /// <summary>
    /// Entity 运行时事件。
    /// </summary>
    public static class Entity
    {
        /// <summary>Entity 已生成并注册。</summary>
        public const string Spawned = "entity:spawned";

        /// <summary>Entity 已销毁并注销。</summary>
        public const string Destroyed = "entity:destroyed";

        /// <summary>
        /// Entity 生命周期 payload。
        /// </summary>
        public readonly record struct LifecycleEventData(IEntity Entity);
    }
}
