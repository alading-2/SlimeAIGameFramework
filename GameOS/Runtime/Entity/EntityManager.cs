using System;
using System.Collections.Generic;
using SkilmeAI.GameOS.Runtime.Event;
using SkilmeAI.GameOS.Runtime.Events.Core;
using SkilmeAI.GameOS.Runtime.Relationship;

namespace SkilmeAI.GameOS.Runtime.Entity;

/// <summary>
/// Entity 注册表和生命周期门面。
/// </summary>
public static class EntityManager
{
    private static readonly Dictionary<string, IEntity> Entities = new(StringComparer.Ordinal);

    /// <summary>
    /// 生成并注册一个纯 Runtime Entity。
    /// </summary>
    /// <param name="config">生成参数。</param>
    public static RuntimeEntity Spawn(EntitySpawnConfig config = default)
    {
        var entity = new RuntimeEntity(string.IsNullOrWhiteSpace(config.EntityId)
            ? Guid.NewGuid().ToString("N")
            : config.EntityId);

        if (config.Config != null)
        {
            entity.Data.LoadFromConfig(config.Config);
        }

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
    public static bool Destroy(string entityId)
    {
        if (!Entities.TryGetValue(entityId, out var entity))
        {
            return false;
        }

        DestroyOwnedChildren(entityId);
        RelationshipManager.RemoveAllForEntity(entityId);
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
    public static IEntity? Get(string entityId)
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
        string childEntityId,
        string parentEntityId,
        bool autoAddParentRelation = true,
        ParentDestroyPolicy parentDestroyPolicy = ParentDestroyPolicy.DestroyRecursively,
        params string[] relationTypes)
    {
        if (!Entities.ContainsKey(childEntityId) || !Entities.ContainsKey(parentEntityId))
        {
            return false;
        }

        return RelationshipManager.BindParentRelationships(
            childEntityId,
            parentEntityId,
            autoAddParentRelation,
            parentDestroyPolicy,
            relationTypes);
    }

    private static void BindSpawnRelationships(string childEntityId, EntitySpawnConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.ParentEntityId))
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

    private static void DestroyOwnedChildren(string parentEntityId)
    {
        var ownedChildren = RelationshipManager.GetChildRelationshipsByParentAndType(parentEntityId, RelationshipType.Parent);
        foreach (var relationship in ownedChildren)
        {
            var policy = RelationshipLifecycle.ReadParentDestroyPolicy(parentEntityId, relationship.ChildEntityId);
            if (policy == ParentDestroyPolicy.DestroyRecursively)
            {
                Destroy(relationship.ChildEntityId);
                continue;
            }

            RelationshipManager.RemoveRelationship(parentEntityId, relationship.ChildEntityId, RelationshipType.Parent);
        }
    }
}
