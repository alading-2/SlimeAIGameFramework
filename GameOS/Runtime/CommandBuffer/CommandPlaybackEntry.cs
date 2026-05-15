using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Schedule;

namespace SlimeAI.GameOS.Runtime.CommandBuffer;

/// <summary>
/// 单条延迟命令播放结果。
/// </summary>
public readonly record struct CommandPlaybackEntry(
    int CommandId,
    int Sequence,
    DeferredCommandKind Kind,
    SchedulePhase TargetPhase,
    string RequestedBy,
    EntityId CapturedEntityId,
    EntityId TargetEntityId,
    EntityId CreatedEntityId,
    DeferredCommandStatus Status,
    DeferredCommandFailureReason FailureReason,
    string DebugLabel,
    SpawnCommandPayload? Spawn,
    DestroyCommandPayload? Destroy,
    AttachCommandPayload? Attach,
    DetachCommandPayload? Detach,
    QueuedEventCommandPayload? QueuedEvent,
    ResourceRequestCommandPayload? ResourceRequest,
    GodotNodeInstantiatePayload? GodotInstantiate,
    GodotNodeFreeCommandPayload? GodotFree)
{
    /// <summary>
    /// 从命令和播放结果创建 entry。
    /// </summary>
    public static CommandPlaybackEntry From(
        DeferredRuntimeCommand command,
        DeferredCommandStatus status,
        DeferredCommandFailureReason failureReason,
        EntityId createdEntityId = default)
    {
        return new CommandPlaybackEntry(
            command.CommandId,
            command.Sequence,
            command.Kind,
            command.TargetPhase,
            command.RequestedBy,
            command.CapturedEntityId,
            command.TargetEntityId,
            createdEntityId,
            status,
            failureReason,
            command.DebugLabel,
            command.Spawn,
            command.Destroy,
            command.Attach,
            command.Detach,
            command.QueuedEvent,
            command.ResourceRequest,
            command.GodotInstantiate,
            command.GodotFree);
    }
}
