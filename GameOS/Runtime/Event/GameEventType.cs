using SkilmeAI.GameOS.Runtime.Data;
using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.Runtime.Event;

/// <summary>
/// GameOS Runtime 内置事件名和 payload。
/// </summary>
public static partial class GameEventType
{
    /// <summary>
    /// Data 运行时事件。
    /// </summary>
    public static class Data
    {
        /// <summary>任意运行时 Data 值发生变化。</summary>
        public const string PropertyChanged = "data:property_changed";

        /// <summary>运行时 Data 已重置。</summary>
        public const string Reset = "data:reset";

        /// <summary>
        /// Data 变更 payload。
        /// </summary>
        public readonly record struct PropertyChangedEventData(DataChangedEventData Change);

        /// <summary>
        /// Data 重置 payload。
        /// </summary>
        public readonly record struct ResetEventData();
    }

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

    /// <summary>
    /// Relationship 运行时事件。
    /// </summary>
    public static class Relationship
    {
        /// <summary>关系已添加。</summary>
        public const string Added = "relationship:added";

        /// <summary>关系已移除。</summary>
        public const string Removed = "relationship:removed";

        /// <summary>
        /// 关系变更 payload。
        /// </summary>
        public readonly record struct ChangedEventData(
            string ParentEntityId,
            string ChildEntityId,
            string RelationType);
    }
}
