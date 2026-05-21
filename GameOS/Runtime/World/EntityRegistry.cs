using System;
using System.Collections.Generic;
using SlimeAI.GameOS.Observation;
using SlimeAI.GameOS.Runtime.CommandBuffer;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Events.Core;
using SlimeAI.GameOS.Runtime.Schedule;

namespace SlimeAI.GameOS.Runtime.World;

/// <summary>
/// RuntimeWorld 持有的 Entity 注册表和生命周期门面。
/// </summary>
public sealed class EntityRegistry : IEntityRegistry
{
    private static readonly GameOSContextLog Log = GameOSLog.For("EntityManager");

    private readonly Dictionary<EntityId, IEntity> entities = new();
    private readonly ILifecycleTree lifecycle;
    private readonly IWorldEventBus events;
    private readonly RuntimeCommandBuffer commands;

    /// <summary>
    /// 创建实体注册表实例。
    /// </summary>
    public EntityRegistry(ILifecycleTree lifecycle, IWorldEventBus events, RuntimeCommandBuffer commands)
    {
        this.lifecycle = lifecycle ?? throw new ArgumentNullException(nameof(lifecycle));
        this.events = events ?? throw new ArgumentNullException(nameof(events));
        this.commands = commands ?? throw new ArgumentNullException(nameof(commands));
    }

    /// <inheritdoc />
    public RuntimeEntity Spawn(EntitySpawnConfig config = default)
    {
        if (commands.IsGuarded && !commands.IsPlayingBack)
        {
            return SpawnDeferred(config);
        }

        return SpawnImmediate(config);
    }

    internal RuntimeEntity SpawnImmediate(EntitySpawnConfig config = default)
    {
        var entityId = config.EntityId.IsEmpty
            ? EntityId.From(Guid.NewGuid().ToString("N"))
            : config.EntityId;
        var entity = new RuntimeEntity(entityId, config.DataCatalog, events);

        Register(entity);
        AttachLifecycleParentFromConfig(entity.EntityId, config);
        Log.Info(
            $"Entity spawned: {entity.EntityId}",
            new Dictionary<string, object?>
            {
                ["entityId"] = entity.EntityId
            });
        return entity;
    }

    /// <inheritdoc />
    public bool Register(IEntity entity)
    {
        return RegisterImmediate(entity);
    }

    internal bool RegisterImmediate(IEntity entity)
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
        if (commands.IsGuarded && !commands.IsPlayingBack)
        {
            if (entityId.IsEmpty)
            {
                return false;
            }

            commands.Enqueue(DeferredRuntimeCommand.ForDestroy(entityId, SchedulePhase.EndOfFrame));
            return true;
        }

        return DestroyImmediate(entityId);
    }

    internal bool DestroyImmediate(EntityId entityId)
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
        Log.Info(
            $"Entity destroyed: {entityId}",
            new Dictionary<string, object?>
            {
                ["entityId"] = entityId
            });
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

    internal bool Contains(EntityId entityId)
    {
        return entities.ContainsKey(entityId);
    }

    private RuntimeEntity SpawnDeferred(EntitySpawnConfig config)
    {
        var entityId = config.EntityId.IsEmpty
            ? EntityId.From(Guid.NewGuid().ToString("N"))
            : config.EntityId;
        var reservedConfig = config with { EntityId = entityId };
        var entity = new RuntimeEntity(entityId, reservedConfig.DataCatalog, events);
        commands.Enqueue(DeferredRuntimeCommand.ForSpawn(reservedConfig, entityId, SchedulePhase.EndOfFrame));
        commands.ReserveEntity(entity);
        return entity;
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
