namespace SlimeAI.GameOS.Runtime.Schedule;

/// <summary>
/// 系统运行时信息快照。
/// </summary>
public sealed class SystemRuntimeInfo
{
    /// <summary>系统 Id。</summary>
    public string SystemId { get; init; } = string.Empty;

    /// <summary>是否已加载。</summary>
    public bool IsAdded { get; init; }

    /// <summary>人工开关是否启用。</summary>
    public bool IsEnabled { get; init; }

    /// <summary>是否处于运行态。</summary>
    public bool IsRunning { get; init; }

    /// <summary>是否满足项目状态运行条件。</summary>
    public bool IsStateAllowed { get; init; }

    /// <summary>状态门禁未通过原因。</summary>
    public string BlockedReason { get; init; } = string.Empty;

    /// <summary>系统分组。</summary>
    public SystemGroup Group { get; init; }

    /// <summary>系统标签。</summary>
    public SystemTag Tags { get; init; }
}
