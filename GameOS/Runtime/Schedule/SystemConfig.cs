namespace SkilmeAI.GameOS.Runtime.Schedule;

/// <summary>
/// 系统调度配置，后续由 DataOS 生成。
/// </summary>
public sealed class SystemConfig
{
    /// <summary>系统唯一 Id。</summary>
    public string SystemId { get; init; } = string.Empty;

    /// <summary>系统分组。</summary>
    public SystemGroup Group { get; init; } = SystemGroup.Else;

    /// <summary>系统标签。</summary>
    public SystemTag Tags { get; init; } = SystemTag.None;

    /// <summary>是否为必需系统。</summary>
    public bool Required { get; init; }

    /// <summary>初始人工启用状态。</summary>
    public bool StartEnabled { get; init; } = true;

    /// <summary>加载优先级，数值越小越优先。</summary>
    public int Priority { get; init; }

    /// <summary>依赖系统 Id 列表。</summary>
    public string[] Dependencies { get; init; } = [];

    /// <summary>运行条件。</summary>
    public SystemRunCondition RunCondition { get; init; } = SystemRunCondition.Always;
}
