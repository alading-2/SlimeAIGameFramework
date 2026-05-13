using System.Collections.Generic;
using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.Capabilities.Feature;

/// <summary>
/// Feature 生命周期上下文。
/// </summary>
public sealed class FeatureContext
{
    /// <summary>拥有者。</summary>
    public IEntity? Owner { get; init; }

    /// <summary>Feature 实体。</summary>
    public IEntity? Feature { get; init; }

    /// <summary>Feature 定义。</summary>
    public FeatureDefinition? Definition { get; init; }

    /// <summary>子系统激活数据。</summary>
    public object? ActivationData { get; init; }

    /// <summary>执行结果。</summary>
    public object? ExecuteResult { get; set; }

    /// <summary>触发源事件数据。</summary>
    public object? SourceEventData { get; init; }

    /// <summary>临时扩展数据。</summary>
    public Dictionary<string, object> ExtraData { get; } = new();
}
