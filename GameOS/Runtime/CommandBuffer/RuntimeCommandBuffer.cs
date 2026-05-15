using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Resource;
using SlimeAI.GameOS.Runtime.Schedule;
using SlimeAI.GameOS.Runtime.World;

namespace SlimeAI.GameOS.Runtime.CommandBuffer;

/// <summary>
/// Runtime 延迟命令队列；受保护区域内的结构变更统一进入这里。
/// </summary>
public sealed class RuntimeCommandBuffer : IRuntimeCommandBuffer
{
    private const int GuardScopeCommandLimit = 1000;

    private readonly RuntimeWorld world;
    private readonly List<DeferredRuntimeCommand> queue = new();
    private readonly Stack<string> guardReasons = new();
    private readonly Dictionary<EntityId, RuntimeEntity> reservedEntities = new();
    private int nextCommandId = 1;
    private int nextSequence = 1;
    private int guardCount;
    private int currentGuardCommandCount;
    private bool isDisposed;

    /// <summary>
    /// 创建 CommandBuffer。
    /// </summary>
    public RuntimeCommandBuffer(RuntimeWorld world)
    {
        this.world = world ?? throw new ArgumentNullException(nameof(world));
        LastDiscardReport = CommandPlaybackReport.Empty(SchedulePhase.Manual);
    }

    /// <inheritdoc />
    public bool IsGuarded => guardCount > 0;

    /// <summary>当前是否处于 playback 内部。</summary>
    internal bool IsPlayingBack { get; private set; }

    /// <inheritdoc />
    public int QueuedCount => queue.Count;

    /// <inheritdoc />
    public IGodotNodeCommandHandler? GodotNodeCommandHandler { get; set; }

    /// <inheritdoc />
    public CommandPlaybackReport LastDiscardReport { get; private set; }

    /// <inheritdoc />
    public ImmutableArray<DeferredRuntimeCommand> Snapshot()
    {
        return queue.ToImmutableArray();
    }

    /// <inheritdoc />
    public DeferredRuntimeCommand Enqueue(DeferredRuntimeCommand command)
    {
        if (isDisposed)
        {
            throw new ObjectDisposedException(nameof(RuntimeCommandBuffer));
        }

        if (IsGuarded)
        {
            currentGuardCommandCount++;
            if (currentGuardCommandCount > GuardScopeCommandLimit)
            {
                throw new InvalidOperationException("Guard scope command limit exceeded");
            }
        }

        var requestedBy = string.IsNullOrWhiteSpace(command.RequestedBy)
            ? CurrentReason()
            : command.RequestedBy;
        var queued = command.WithQueueMetadata(nextCommandId++, nextSequence++, requestedBy);
        queue.Add(queued);
        return queued;
    }

    /// <summary>
    /// 保存 guarded spawn 返回的 reserved entity，确保 guard 内 Data.Set 不丢失。
    /// </summary>
    internal void ReserveEntity(RuntimeEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        reservedEntities[entity.EntityId] = entity;
    }

    /// <inheritdoc />
    public CommandPlaybackReport Playback(SchedulePhase phase)
    {
        if (isDisposed)
        {
            throw new ObjectDisposedException(nameof(RuntimeCommandBuffer));
        }

        var selected = new List<DeferredRuntimeCommand>();
        for (var i = 0; i < queue.Count; i++)
        {
            if (queue[i].TargetPhase == phase)
            {
                selected.Add(queue[i]);
            }
        }

        if (selected.Count == 0)
        {
            return CommandPlaybackReport.Empty(phase);
        }

        selected.Sort(static (left, right) => left.Sequence.CompareTo(right.Sequence));
        queue.RemoveAll(command => command.TargetPhase == phase);

        var stopwatch = Stopwatch.StartNew();
        var entries = ImmutableArray.CreateBuilder<CommandPlaybackEntry>(selected.Count);
        var played = 0;
        var failed = 0;
        IsPlayingBack = true;
        try
        {
            for (var i = 0; i < selected.Count; i++)
            {
                var command = selected[i];
                var (status, reason, createdEntityId) = Play(command);
                if (status == DeferredCommandStatus.Played)
                {
                    played++;
                }
                else if (status == DeferredCommandStatus.Failed)
                {
                    failed++;
                }

                entries.Add(CommandPlaybackEntry.From(command, status, reason, createdEntityId));
            }
        }
        finally
        {
            IsPlayingBack = false;
            stopwatch.Stop();
        }

        return new CommandPlaybackReport(phase, selected.Count, played, failed, 0, stopwatch.Elapsed.TotalMilliseconds, entries.ToImmutable());
    }

    /// <inheritdoc />
    public IStructuralChangeGuard EnterGuard(string reason)
    {
        if (isDisposed)
        {
            return NoopGuard.Instance;
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        if (guardCount == 0)
        {
            currentGuardCommandCount = 0;
        }

        guardCount++;
        guardReasons.Push(reason);
        return new StructuralChangeGuard(this, reason);
    }

    /// <inheritdoc />
    public CommandPlaybackReport Clear()
    {
        if (isDisposed)
        {
            return LastDiscardReport;
        }

        var builder = ImmutableArray.CreateBuilder<CommandPlaybackEntry>(queue.Count);
        for (var i = 0; i < queue.Count; i++)
        {
            builder.Add(CommandPlaybackEntry.From(
                queue[i],
                DeferredCommandStatus.Skipped,
                DeferredCommandFailureReason.WorldDisposing));
        }

        LastDiscardReport = new CommandPlaybackReport(
            SchedulePhase.Manual,
            queue.Count,
            0,
            0,
            queue.Count,
            0,
            builder.ToImmutable());
        queue.Clear();
        reservedEntities.Clear();
        guardReasons.Clear();
        guardCount = 0;
        currentGuardCommandCount = 0;
        isDisposed = true;
        return LastDiscardReport;
    }

    private (DeferredCommandStatus Status, DeferredCommandFailureReason Reason, EntityId CreatedEntityId) Play(DeferredRuntimeCommand command)
    {
        try
        {
            return command.Kind switch
            {
                DeferredCommandKind.Spawn => PlaySpawn(command),
                DeferredCommandKind.Destroy => PlayDestroy(command),
                DeferredCommandKind.Attach => PlayAttach(command),
                DeferredCommandKind.Detach => PlayDetach(command),
                DeferredCommandKind.QueuedEvent => PlayQueuedEvent(command),
                DeferredCommandKind.ResourceRequest => PlayResourceRequest(command),
                DeferredCommandKind.GodotNodeInstantiate => PlayGodotInstantiate(command),
                DeferredCommandKind.GodotNodeFree => PlayGodotFree(command),
                _ => Failed(DeferredCommandFailureReason.UnsupportedKind)
            };
        }
        catch (Exception)
        {
            return Failed(DeferredCommandFailureReason.HandlerException);
        }
    }

    private (DeferredCommandStatus, DeferredCommandFailureReason, EntityId) PlaySpawn(DeferredRuntimeCommand command)
    {
        if (!command.Spawn.HasValue)
        {
            return Failed(DeferredCommandFailureReason.InvalidPayload);
        }

        var config = command.Spawn.Value.Config;
        var entityId = ResolveSpawnEntityId(config, command.CapturedEntityId);
        if (entityId.IsEmpty)
        {
            entityId = EntityId.From(Guid.NewGuid().ToString("N"));
        }

        if (world.EntityRegistry.Contains(entityId))
        {
            reservedEntities.Remove(entityId);
            return Failed(DeferredCommandFailureReason.DuplicateEntityId);
        }

        if (!config.ParentEntityId.IsEmpty && !world.EntityRegistry.Contains(config.ParentEntityId))
        {
            reservedEntities.Remove(entityId);
            return Failed(DeferredCommandFailureReason.MissingParent);
        }

        config = config with { EntityId = entityId };
        var entity = reservedEntities.TryGetValue(entityId, out var reserved)
            ? reserved
            : new RuntimeEntity(entityId, config.DataCatalog, world.EventBus);

        reservedEntities.Remove(entityId);
        if (!world.EntityRegistry.RegisterImmediate(entity))
        {
            return Failed(DeferredCommandFailureReason.DuplicateEntityId);
        }

        if (!config.ParentEntityId.IsEmpty && !world.LifecycleTree.AttachImmediate(config.ParentEntityId, entityId, config.ParentDestroyPolicy))
        {
            return Failed(DeferredCommandFailureReason.RelationshipConflict);
        }

        return Played(entityId);
    }

    private (DeferredCommandStatus, DeferredCommandFailureReason, EntityId) PlayDestroy(DeferredRuntimeCommand command)
    {
        if (!command.Destroy.HasValue)
        {
            return Failed(DeferredCommandFailureReason.InvalidPayload);
        }

        var target = command.Destroy.Value.Target;
        if (target.IsEmpty || !world.EntityRegistry.Contains(target))
        {
            return Failed(DeferredCommandFailureReason.MissingTargetEntity);
        }

        return world.EntityRegistry.DestroyImmediate(target)
            ? Played()
            : Failed(DeferredCommandFailureReason.MissingTargetEntity);
    }

    private (DeferredCommandStatus, DeferredCommandFailureReason, EntityId) PlayAttach(DeferredRuntimeCommand command)
    {
        if (!command.Attach.HasValue)
        {
            return Failed(DeferredCommandFailureReason.InvalidPayload);
        }

        var payload = command.Attach.Value;
        if (payload.Parent.IsEmpty || !world.EntityRegistry.Contains(payload.Parent))
        {
            return Failed(DeferredCommandFailureReason.MissingParent);
        }

        if (payload.Child.IsEmpty || !world.EntityRegistry.Contains(payload.Child))
        {
            return Failed(DeferredCommandFailureReason.MissingChild);
        }

        return world.LifecycleTree.AttachImmediate(payload.Parent, payload.Child, payload.Policy, payload.Priority)
            ? Played()
            : Failed(DeferredCommandFailureReason.RelationshipConflict);
    }

    private (DeferredCommandStatus, DeferredCommandFailureReason, EntityId) PlayDetach(DeferredRuntimeCommand command)
    {
        if (!command.Detach.HasValue)
        {
            return Failed(DeferredCommandFailureReason.InvalidPayload);
        }

        var payload = command.Detach.Value;
        if (payload.Parent.IsEmpty || !world.EntityRegistry.Contains(payload.Parent))
        {
            return Failed(DeferredCommandFailureReason.MissingParent);
        }

        if (payload.Child.IsEmpty || !world.EntityRegistry.Contains(payload.Child))
        {
            return Failed(DeferredCommandFailureReason.MissingChild);
        }

        return world.LifecycleTree.DetachImmediate(payload.Parent, payload.Child)
            ? Played()
            : Failed(DeferredCommandFailureReason.RelationshipConflict);
    }

    private (DeferredCommandStatus, DeferredCommandFailureReason, EntityId) PlayQueuedEvent(DeferredRuntimeCommand command)
    {
        if (!command.QueuedEvent.HasValue)
        {
            return Failed(DeferredCommandFailureReason.InvalidPayload);
        }

        var reason = FrameworkEventTypeRegistry.Publish(world.EventBus, command.QueuedEvent.Value);
        return reason == DeferredCommandFailureReason.None ? Played() : Failed(reason);
    }

    private (DeferredCommandStatus, DeferredCommandFailureReason, EntityId) PlayResourceRequest(DeferredRuntimeCommand command)
    {
        if (!command.ResourceRequest.HasValue)
        {
            return Failed(DeferredCommandFailureReason.InvalidPayload);
        }

        var payload = command.ResourceRequest.Value;
        if (string.IsNullOrWhiteSpace(payload.ResourceKey) || string.IsNullOrWhiteSpace(payload.ResourcePath))
        {
            return Failed(DeferredCommandFailureReason.InvalidPayload);
        }

        world.ResourceCatalog.Register(payload.ResourceKey, ResourceCategory.Other, payload.ResourcePath);
        return Played();
    }

    private (DeferredCommandStatus, DeferredCommandFailureReason, EntityId) PlayGodotInstantiate(DeferredRuntimeCommand command)
    {
        if (!command.GodotInstantiate.HasValue || string.IsNullOrWhiteSpace(command.GodotInstantiate.Value.ScenePath))
        {
            return Failed(DeferredCommandFailureReason.InvalidPayload);
        }

        if (GodotNodeCommandHandler == null)
        {
            return Failed(DeferredCommandFailureReason.BridgeTargetUnavailable);
        }

        return GodotNodeCommandHandler.InstantiateNode(command.GodotInstantiate.Value)
            ? Played()
            : Failed(DeferredCommandFailureReason.BridgeTargetUnavailable);
    }

    private (DeferredCommandStatus, DeferredCommandFailureReason, EntityId) PlayGodotFree(DeferredRuntimeCommand command)
    {
        if (!command.GodotFree.HasValue || string.IsNullOrWhiteSpace(command.GodotFree.Value.GodotNodePath))
        {
            return Failed(DeferredCommandFailureReason.InvalidPayload);
        }

        if (GodotNodeCommandHandler == null)
        {
            return Failed(DeferredCommandFailureReason.BridgeTargetUnavailable);
        }

        return GodotNodeCommandHandler.FreeNode(command.GodotFree.Value)
            ? Played()
            : Failed(DeferredCommandFailureReason.BridgeTargetUnavailable);
    }

    private EntityId ResolveSpawnEntityId(EntitySpawnConfig config, EntityId capturedId)
    {
        if (!capturedId.IsEmpty)
        {
            return capturedId;
        }

        return config.EntityId;
    }

    private string CurrentReason()
    {
        return guardReasons.Count > 0 ? guardReasons.Peek() : "manual";
    }

    private void ExitGuard(StructuralChangeGuard guard)
    {
        if (guard.IsDisposed)
        {
            return;
        }

        guard.IsDisposed = true;
        if (guardCount > 0)
        {
            guardCount--;
        }

        if (guardReasons.Count > 0)
        {
            guardReasons.Pop();
        }

        if (guardCount == 0)
        {
            currentGuardCommandCount = 0;
        }
    }

    private static (DeferredCommandStatus, DeferredCommandFailureReason, EntityId) Played(EntityId createdEntityId = default)
    {
        return (DeferredCommandStatus.Played, DeferredCommandFailureReason.None, createdEntityId);
    }

    private static (DeferredCommandStatus, DeferredCommandFailureReason, EntityId) Failed(DeferredCommandFailureReason reason)
    {
        return (DeferredCommandStatus.Failed, reason, EntityId.Empty);
    }

    private sealed class StructuralChangeGuard : IStructuralChangeGuard
    {
        private readonly RuntimeCommandBuffer owner;

        public StructuralChangeGuard(RuntimeCommandBuffer owner, string reason)
        {
            this.owner = owner;
            Reason = reason;
        }

        public string Reason { get; }

        public bool IsDisposed { get; set; }

        public void Dispose()
        {
            owner.ExitGuard(this);
        }
    }

    private sealed class NoopGuard : IStructuralChangeGuard
    {
        public static readonly NoopGuard Instance = new();

        private NoopGuard()
        {
        }

        public string Reason => "noop";

        public void Dispose()
        {
        }
    }
}
