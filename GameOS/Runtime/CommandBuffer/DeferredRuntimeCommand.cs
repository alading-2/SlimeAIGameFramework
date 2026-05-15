using System.Text.Json;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;
using SlimeAI.GameOS.Runtime.Schedule;

namespace SlimeAI.GameOS.Runtime.CommandBuffer;

/// <summary>
/// 不可变延迟 Runtime 命令；payload 使用 typed nullable fields 表达。
/// </summary>
public readonly record struct DeferredRuntimeCommand
{
    private DeferredRuntimeCommand(
        int commandId,
        int sequence,
        DeferredCommandKind kind,
        SchedulePhase targetPhase,
        string requestedBy,
        EntityId capturedEntityId,
        EntityId targetEntityId,
        SpawnCommandPayload? spawn,
        DestroyCommandPayload? destroy,
        AttachCommandPayload? attach,
        DetachCommandPayload? detach,
        QueuedEventCommandPayload? queuedEvent,
        ResourceRequestCommandPayload? resourceRequest,
        GodotNodeInstantiatePayload? godotInstantiate,
        GodotNodeFreeCommandPayload? godotFree,
        string debugLabel)
    {
        CommandId = commandId;
        Sequence = sequence;
        Kind = kind;
        TargetPhase = targetPhase;
        RequestedBy = requestedBy;
        CapturedEntityId = capturedEntityId;
        TargetEntityId = targetEntityId;
        Spawn = spawn;
        Destroy = destroy;
        Attach = attach;
        Detach = detach;
        QueuedEvent = queuedEvent;
        ResourceRequest = resourceRequest;
        GodotInstantiate = godotInstantiate;
        GodotFree = godotFree;
        DebugLabel = debugLabel;
    }

    public int CommandId { get; }
    public int Sequence { get; }
    public DeferredCommandKind Kind { get; }
    public SchedulePhase TargetPhase { get; }
    public string RequestedBy { get; }
    public EntityId CapturedEntityId { get; }
    public EntityId TargetEntityId { get; }
    public SpawnCommandPayload? Spawn { get; }
    public DestroyCommandPayload? Destroy { get; }
    public AttachCommandPayload? Attach { get; }
    public DetachCommandPayload? Detach { get; }
    public QueuedEventCommandPayload? QueuedEvent { get; }
    public ResourceRequestCommandPayload? ResourceRequest { get; }
    public GodotNodeInstantiatePayload? GodotInstantiate { get; }
    public GodotNodeFreeCommandPayload? GodotFree { get; }
    public string DebugLabel { get; }

    public static DeferredRuntimeCommand ForSpawn(
        EntitySpawnConfig config,
        EntityId capturedId = default,
        SchedulePhase targetPhase = SchedulePhase.EndOfFrame,
        string requestedBy = "",
        string debugLabel = "")
    {
        if (capturedId.IsEmpty)
        {
            capturedId = config.EntityId;
        }

        return new DeferredRuntimeCommand(
            0,
            0,
            DeferredCommandKind.Spawn,
            targetPhase,
            requestedBy,
            capturedId,
            capturedId,
            new SpawnCommandPayload(config),
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            debugLabel);
    }

    public static DeferredRuntimeCommand ForDestroy(
        EntityId target,
        SchedulePhase targetPhase = SchedulePhase.EndOfFrame,
        string requestedBy = "",
        string debugLabel = "")
    {
        return new DeferredRuntimeCommand(0, 0, DeferredCommandKind.Destroy, targetPhase, requestedBy, EntityId.Empty, target, null, new DestroyCommandPayload(target), null, null, null, null, null, null, debugLabel);
    }

    public static DeferredRuntimeCommand ForAttach(
        EntityId parent,
        EntityId child,
        ParentDestroyPolicy policy = ParentDestroyPolicy.DestroyRecursively,
        int priority = 0,
        SchedulePhase targetPhase = SchedulePhase.EndOfFrame,
        string requestedBy = "",
        string debugLabel = "")
    {
        return new DeferredRuntimeCommand(0, 0, DeferredCommandKind.Attach, targetPhase, requestedBy, EntityId.Empty, child, null, null, new AttachCommandPayload(parent, child, policy, priority), null, null, null, null, null, debugLabel);
    }

    public static DeferredRuntimeCommand ForDetach(
        EntityId parent,
        EntityId child,
        SchedulePhase targetPhase = SchedulePhase.EndOfFrame,
        string requestedBy = "",
        string debugLabel = "")
    {
        return new DeferredRuntimeCommand(0, 0, DeferredCommandKind.Detach, targetPhase, requestedBy, EntityId.Empty, child, null, null, null, new DetachCommandPayload(parent, child), null, null, null, null, debugLabel);
    }

    public static DeferredRuntimeCommand ForQueuedEvent<TEvent>(
        TEvent @event,
        SchedulePhase targetPhase = SchedulePhase.EndOfFrame,
        string requestedBy = "",
        string debugLabel = "")
        where TEvent : struct, IGlobalEvent
    {
        var payloadBytes = JsonSerializer.SerializeToUtf8Bytes(@event);
        var payload = new QueuedEventCommandPayload(typeof(TEvent).FullName ?? typeof(TEvent).Name, payloadBytes);
        return new DeferredRuntimeCommand(0, 0, DeferredCommandKind.QueuedEvent, targetPhase, requestedBy, EntityId.Empty, EntityId.Empty, null, null, null, null, payload, null, null, null, debugLabel);
    }

    public static DeferredRuntimeCommand ForResourceRequest(
        string resourceKey,
        string resourcePath,
        SchedulePhase targetPhase = SchedulePhase.EndOfFrame,
        string requestedBy = "",
        string debugLabel = "")
    {
        return new DeferredRuntimeCommand(0, 0, DeferredCommandKind.ResourceRequest, targetPhase, requestedBy, EntityId.Empty, EntityId.Empty, null, null, null, null, null, new ResourceRequestCommandPayload(resourceKey, resourcePath), null, null, debugLabel);
    }

    public static DeferredRuntimeCommand ForGodotInstantiate(
        string scenePath,
        EntityId? associatedEntity = null,
        SchedulePhase targetPhase = SchedulePhase.EndOfFrame,
        string requestedBy = "",
        string debugLabel = "")
    {
        return new DeferredRuntimeCommand(0, 0, DeferredCommandKind.GodotNodeInstantiate, targetPhase, requestedBy, associatedEntity ?? EntityId.Empty, associatedEntity ?? EntityId.Empty, null, null, null, null, null, null, new GodotNodeInstantiatePayload(scenePath, associatedEntity), null, debugLabel);
    }

    public static DeferredRuntimeCommand ForGodotFree(
        string godotNodePath,
        SchedulePhase targetPhase = SchedulePhase.EndOfFrame,
        string requestedBy = "",
        string debugLabel = "")
    {
        return new DeferredRuntimeCommand(0, 0, DeferredCommandKind.GodotNodeFree, targetPhase, requestedBy, EntityId.Empty, EntityId.Empty, null, null, null, null, null, null, null, new GodotNodeFreeCommandPayload(godotNodePath), debugLabel);
    }

    internal DeferredRuntimeCommand WithQueueMetadata(int commandId, int sequence, string requestedBy)
    {
        return new DeferredRuntimeCommand(
            commandId,
            sequence,
            Kind,
            TargetPhase,
            requestedBy,
            CapturedEntityId,
            TargetEntityId,
            Spawn,
            Destroy,
            Attach,
            Detach,
            QueuedEvent,
            ResourceRequest,
            GodotInstantiate,
            GodotFree,
            DebugLabel);
    }
}
