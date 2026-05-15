using System.Collections.Immutable;
using SlimeAI.GameOS.Runtime.Schedule;

namespace SlimeAI.GameOS.Runtime.CommandBuffer;

/// <summary>
/// 一次 phase playback 的结构化报告。
/// </summary>
public sealed record CommandPlaybackReport(
    SchedulePhase Phase,
    int QueuedCount,
    int PlayedCount,
    int FailedCount,
    int SkippedCount,
    double DurationMs,
    ImmutableArray<CommandPlaybackEntry> Commands)
{
    /// <summary>创建空报告。</summary>
    public static CommandPlaybackReport Empty(SchedulePhase phase)
    {
        return new CommandPlaybackReport(phase, 0, 0, 0, 0, 0, ImmutableArray<CommandPlaybackEntry>.Empty);
    }
}
