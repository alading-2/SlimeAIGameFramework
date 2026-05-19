using System;
using System.Collections.Generic;

namespace SlimeAI.GameOS.Capabilities.Feature;

/// <summary>
/// 可授予能力定义。
/// </summary>
public sealed class FeatureDefinition
{
    /// <summary>Feature 稳定 Id。</summary>
    public required string FeatureId { get; init; }

    /// <summary>处理器 Id；纯 Modifier Feature 可以为空。</summary>
    public string HandlerId { get; init; } = string.Empty;

    /// <summary>授予时应用的修改器。</summary>
    public FeatureModifierEntry[] Modifiers { get; init; } = [];

    /// <summary>授予或 handler 执行时可消费的原子动作列表。</summary>
    public IReadOnlyList<IFeatureAction> Actions { get; init; } = Array.Empty<IFeatureAction>();
}
