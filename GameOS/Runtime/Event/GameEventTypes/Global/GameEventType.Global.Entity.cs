using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.Runtime.Event;

/// <summary>
/// Global 实体相关事件定义。
/// </summary>
public static partial class GameEventType
{
    /// <summary>
    /// 全局运行时事件。
    /// </summary>
    public static partial class Global
    {
        /// <summary>Entity 生成。</summary>
        public const string EntitySpawned = "global:entity:spawned";

        /// <summary>Entity 销毁。</summary>
        public const string EntityDestroyed = "global:entity:destroyed";

        /// <summary>Entity 迁移开始。</summary>
        public const string EntityMigrating = "global:entity:migrating";

        /// <summary>Entity 迁移完成。</summary>
        public const string EntityMigrated = "global:entity:migrated";

        /// <summary>Entity 关系添加。</summary>
        public const string RelationshipAdded = "global:entity:relationship_added";

        /// <summary>Entity 关系移除。</summary>
        public const string RelationshipRemoved = "global:entity:relationship_removed";

        /// <summary>Entity 生成事件数据。</summary>
        public readonly record struct EntitySpawnedEventData(IEntity Entity);

        /// <summary>Entity 销毁事件数据。</summary>
        public readonly record struct EntityDestroyedEventData(IEntity Entity);

        /// <summary>Entity 迁移开始事件数据。</summary>
        public readonly record struct EntityMigratingEventData(
            IEntity SourceEntity,
            string TargetEntityType,
            string ProfileName);

        /// <summary>Entity 迁移完成事件数据。</summary>
        public readonly record struct EntityMigratedEventData(
            IEntity SourceEntity,
            IEntity TargetEntity,
            string ProfileName);

        /// <summary>Entity 关系添加事件数据。</summary>
        public readonly record struct RelationshipAddedEventData(
            string ParentEntityId,
            string ChildEntityId,
            string RelationType);

        /// <summary>Entity 关系移除事件数据。</summary>
        public readonly record struct RelationshipRemovedEventData(
            string ParentEntityId,
            string ChildEntityId,
            string RelationType);
    }
}
