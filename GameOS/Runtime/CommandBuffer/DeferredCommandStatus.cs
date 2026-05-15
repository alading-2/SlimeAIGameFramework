namespace SlimeAI.GameOS.Runtime.CommandBuffer;

/// <summary>
/// 延迟命令播放状态。
/// </summary>
public enum DeferredCommandStatus
{
    Pending = 0,
    Played = 1,
    Failed = 2,
    Skipped = 3
}
