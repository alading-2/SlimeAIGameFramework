using System;
using System.Collections.Generic;
using SlimeAI.GameOS.Runtime.Event;
using SlimeAI.GameOS.Runtime.Events.Core;
using SlimeAI.GameOS.Runtime.Relationship;

namespace SlimeAI.GameOS.Runtime.Entity;

/// <summary>
/// Entity 注册表和生命周期门面。
/// </summary>
public static class EntityManager
{
    private static readonly Dictionary<EntityId, IEntity> Entities = new();

    /// <summary>
    /// 生成并注册一个纯 Runtime Entity。
    /// </summary>
    /// <param name="config">生成参数。</param>
    public static RuntimeEntity Spawn(EntitySpawnConfig config = default)
    {
        var entityId = config.EntityId.IsEmpty
            ? EntityId.From(Guid.NewGuid().ToString("N"))
            : config.EntityId;
        var entity = new RuntimeEntity(entityId, config.DataCatalog);

        Register(entity);
        BindSpawnRelationships(entity.EntityId, config);
        return entity;
    }

    /// <summary>
    /// 注册已有实体。
    /// </summary>
    /// <param name="entity">运行时实体实例。</param>
    public static bool Register(IEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        if (Entities.ContainsKey(entity.EntityId))
        {
            return false;
        }

        Entities.Add(entity.EntityId, entity);
        entity.Events.Publish(new EntitySpawned(entity));
        return true;
    }

    /// <summary>
    /// 按 Id 销毁实体。
    /// </summary>
    /// <param name="entityId">稳定运行时实体 Id。</param>
    public static bool Destroy(EntityId entityId)
    {
        if (!Entities.TryGetValue(entityId, out var entity))
        {
            return false;
        }

        DestroyOwnedChildren(entityId);
        RelationshipManager.RemoveAllForEntity(entityId.Value);
        Entities.Remove(entityId);
        entity.Data.Reset();
        entity.Events.Publish(new EntityDestroyed(entity));
        return true;
    }

    /// <summary>
    /// 销毁实体实例。
    /// </summary>
    /// <param name="entity">运行时实体实例。</param>
    public static bool Destroy(IEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return Destroy(entity.EntityId);
    }

    /// <summary>
    /// 按 Id 查找实体。
    /// </summary>
    /// <param name="entityId">稳定运行时实体 Id。</param>
    public static IEntity? Get(EntityId entityId)
    {
        return Entities.GetValueOrDefault(entityId);
    }

    /// <summary>
    /// 返回所有已注册实体的稳定快照。
    /// </summary>
    public static IReadOnlyList<IEntity> GetAll()
    {
        return new List<IEntity>(Entities.Values);
    }

    /// <summary>
    /// 清空实体、关系图和 world bus 状态。
    /// </summary>
    public static void Clear()
    {
        foreach (var entity in Entities.Values)
        {
            entity.Data.Reset();
        }

        Entities.Clear();
        RelationshipManager.Clear();
        WorldEvents.World.Clear();
    }

    /// <summary>
    /// 为已存在实体绑定父子归属关系。
    /// </summary>
    /// <param name="childEntityId">子实体 Id。</param>
    /// <param name="parentEntityId">父实体 Id。</param>
    /// <param name="autoAddParentRelation">是否自动补充 PARENT 关系。</param>
    /// <param name="parentDestroyPolicy">父实体销毁策略。</param>
    /// <param name="relationTypes">额外业务关系类型。</param>
    public static bool BindParentRelationships(
        EntityId childEntityId,
        EntityId parentEntityId,
        bool autoAddParentRelation = true,
        ParentDestroyPolicy parentDestroyPolicy = ParentDestroyPolicy.DestroyRecursively,
        params string[] relationTypes)
    {
        if (!Entities.ContainsKey(childEntityId) || !Entities.ContainsKey(parentEntityId))
        {
            return false;
        }

        return RelationshipManager.BindParentRelationships(
            childEntityId.Value,
            parentEntityId.Value,
            autoAddParentRelation,
            parentDestroyPolicy,
            relationTypes);
    }

    private static void BindSpawnRelationships(EntityId childEntityId, EntitySpawnConfig config)
    {
        if (config.ParentEntityId.IsEmpty)
        {
            return;
        }

        BindParentRelationships(
            childEntityId,
            config.ParentEntityId,
            config.AutoAddParentRelation || config.ParentRelationTypes == null || config.ParentRelationTypes.Length == 0,
            config.ParentDestroyPolicy,
            config.ParentRelationTypes ?? []);
    }

    private static void DestroyOwnedChildren(EntityId parentEntityId)
    {
        var ownedChildren = RelationshipManager.GetChildRelationshipsByParentAndType(parentEntityId.Value, RelationshipType.Parent);
        foreach (var relationship in ownedChildren)
        {
            var policy = RelationshipLifecycle.ReadParentDestroyPolicy(parentEntityId.Value, relationship.ChildEntityId);
            var childId = EntityId.From(relationship.ChildEntityId);
            if (policy == ParentDestroyPolicy.DestroyRecursively)
            {
                Destroy(childId);
                continue;
            }

            RelationshipManager.RemoveRelationship(parentEntityId.Value, relationship.ChildEntityId, RelationshipType.Parent);
        }
    }
}
