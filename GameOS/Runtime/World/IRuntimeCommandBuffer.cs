using System.Collections.Immutable;
using SlimeAI.GameOS.Runtime.CommandBuffer;
using SlimeAI.GameOS.Runtime.Schedule;

namespace SlimeAI.GameOS.Runtime.World;

/// <summary>
/// RuntimeWorld 持有的 CommandBuffer 句柄。
/// </summary>
public interface IRuntimeCommandBuffer
{
    bool IsGuarded { get; }
    int QueuedCount { get; }
    IGodotNodeCommandHandler? GodotNodeCommandHandler { get; set; }
    CommandPlaybackReport LastDiscardReport { get; }
    ImmutableArray<DeferredRuntimeCommand> Snapshot();
    DeferredRuntimeCommand Enqueue(DeferredRuntimeCommand command);
    CommandPlaybackReport Playback(SchedulePhase phase);
    IStructuralChangeGuard EnterGuard(string reason);
    CommandPlaybackReport Clear();
}
