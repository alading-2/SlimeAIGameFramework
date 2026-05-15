using System;
using System.Collections.Generic;
using SlimeAI.GameOS.Runtime.Event;
using SlimeAI.GameOS.Runtime.Events.Core;

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
        AttachLifecycleParentFromConfig(entity.EntityId, config);
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
        LifecycleTree.DetachAll(entityId);
        RuntimeOwnedReferenceRegistry.NotifyDestroying(entity);
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
    /// 清空实体、lifecycle 树和 world bus 状态。
    /// </summary>
    public static void Clear()
    {
        foreach (var entity in Entities.Values)
        {
            entity.Data.Reset();
        }

        Entities.Clear();
        LifecycleTree.Clear();
        WorldEvents.World.Clear();
    }

    /// <summary>
    /// 为已存在实体绑定 lifecycle parent；薄包装做实体存在性校验，然后委托 <see cref="LifecycleTree.Attach"/>。
    /// </summary>
    /// <param name="childEntityId">子实体 Id。</param>
    /// <param name="parentEntityId">父实体 Id。</param>
    /// <param name="parentDestroyPolicy">父实体销毁策略。</param>
    public static bool AttachLifecycleParent(
        EntityId childEntityId,
        EntityId parentEntityId,
        ParentDestroyPolicy parentDestroyPolicy = ParentDestroyPolicy.DestroyRecursively)
    {
        if (!Entities.ContainsKey(childEntityId) || !Entities.ContainsKey(parentEntityId))
        {
            return false;
        }

        return LifecycleTree.Attach(parentEntityId, childEntityId, parentDestroyPolicy);
    }

    private static void AttachLifecycleParentFromConfig(EntityId childEntityId, EntitySpawnConfig config)
    {
        if (config.ParentEntityId.IsEmpty)
        {
            return;
        }

        LifecycleTree.Attach(config.ParentEntityId, childEntityId, config.ParentDestroyPolicy);
    }

    private static void DestroyOwnedChildren(EntityId parentEntityId)
    {
        var children = LifecycleTree.GetChildren(parentEntityId);
        for (var i = 0; i < children.Count; i++)
        {
            var link = children[i];
            if (link.DestroyPolicy == ParentDestroyPolicy.DestroyRecursively)
            {
                Destroy(link.ChildEntityId);
            }
        }
    }
}
