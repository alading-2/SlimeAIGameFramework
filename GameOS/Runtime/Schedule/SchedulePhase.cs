namespace SlimeAI.GameOS.Runtime.Schedule;

/// <summary>
/// Runtime CommandBuffer 播放阶段。
/// </summary>
public enum SchedulePhase
{
    /// <summary>帧开始。</summary>
    BeginTick = 0,

    /// <summary>Capability tick 前。</summary>
    BeforeSystemTick = 1,

    /// <summary>Capability tick 后。</summary>
    AfterSystemTick = 2,

    /// <summary>事件派发后。</summary>
    AfterEventDispatch = 3,

    /// <summary>帧结束。</summary>
    EndOfFrame = 4,

    /// <summary>测试或显式完成用手动阶段。</summary>
    Manual = 100
}
