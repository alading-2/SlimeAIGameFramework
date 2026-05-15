using System;
using System.Collections.Generic;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Events.Core;

namespace SlimeAI.GameOS.Runtime.World;

/// <summary>
/// RuntimeWorld 持有的 Entity 注册表和生命周期门面。
/// </summary>
public sealed class EntityRegistry : IEntityRegistry
{
    private readonly Dictionary<EntityId, IEntity> entities = new();
    private readonly ILifecycleTree lifecycle;
    private readonly IWorldEventBus events;

    /// <summary>
    /// 创建实体注册表实例。
    /// </summary>
    public EntityRegistry(ILifecycleTree lifecycle, IWorldEventBus events)
    {
        this.lifecycle = lifecycle ?? throw new ArgumentNullException(nameof(lifecycle));
        this.events = events ?? throw new ArgumentNullException(nameof(events));
    }

    /// <inheritdoc />
    public RuntimeEntity Spawn(EntitySpawnConfig config = default)
    {
        var entityId = config.EntityId.IsEmpty
            ? EntityId.From(Guid.NewGuid().ToString("N"))
            : config.EntityId;
        var entity = new RuntimeEntity(entityId, config.DataCatalog, events);

        Register(entity);
        AttachLifecycleParentFromConfig(entity.EntityId, config);
        return entity;
    }

    /// <inheritdoc />
    public bool Register(IEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        if (entities.ContainsKey(entity.EntityId))
        {
            return false;
        }

        entities.Add(entity.EntityId, entity);
        entity.Events.Publish(new EntitySpawned(entity));
        return true;
    }

    /// <inheritdoc />
    public bool Destroy(EntityId entityId)
    {
        if (!entities.TryGetValue(entityId, out var entity))
        {
            return false;
        }

        DestroyOwnedChildren(entityId);
        lifecycle.DetachAll(entityId);
        RuntimeOwnedReferenceRegistry.NotifyDestroying(entity, Get);
        entities.Remove(entityId);
        entity.Data.Reset();
        entity.Events.Publish(new EntityDestroyed(entity));
        return true;
    }

    /// <inheritdoc />
    public bool Destroy(IEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return Destroy(entity.EntityId);
    }

    /// <inheritdoc />
    public IEntity? Get(EntityId entityId)
    {
        return entities.GetValueOrDefault(entityId);
    }

    /// <inheritdoc />
    public IReadOnlyList<IEntity> GetAll()
    {
        return new List<IEntity>(entities.Values);
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<EntityId, IEntity> Snapshot()
    {
        return new Dictionary<EntityId, IEntity>(entities);
    }

    /// <inheritdoc />
    public void Clear()
    {
        var snapshot = new List<EntityId>(entities.Keys);
        for (var i = 0; i < snapshot.Count; i++)
        {
            Destroy(snapshot[i]);
        }

        entities.Clear();
    }

    /// <inheritdoc />
    public bool AttachLifecycleParent(
        EntityId childEntityId,
        EntityId parentEntityId,
        ParentDestroyPolicy parentDestroyPolicy = ParentDestroyPolicy.DestroyRecursively)
    {
        if (!entities.ContainsKey(childEntityId) || !entities.ContainsKey(parentEntityId))
        {
            return false;
        }

        return lifecycle.Attach(parentEntityId, childEntityId, parentDestroyPolicy);
    }

    private void AttachLifecycleParentFromConfig(EntityId childEntityId, EntitySpawnConfig config)
    {
        if (config.ParentEntityId.IsEmpty)
        {
            return;
        }

        lifecycle.Attach(config.ParentEntityId, childEntityId, config.ParentDestroyPolicy);
    }

    private void DestroyOwnedChildren(EntityId parentEntityId)
    {
        var children = lifecycle.GetChildren(parentEntityId);
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
