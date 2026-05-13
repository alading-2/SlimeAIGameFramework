using System.Collections.Generic;
using SlimeAI.GameOS.Capabilities.Ability;
using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.Capabilities.AI;

/// <summary>
/// AI 行为树 Tick 上下文。调用方可以复用同一个实例以避免高频分配。
/// </summary>
public sealed class AIContext
{
    /// <summary>AI 所属实体。</summary>
    public required IEntity Entity { get; init; }

    /// <summary>本次 Tick 的经过秒数。</summary>
    public float Delta { get; set; }

    /// <summary>自动施法使用的 AbilityService。</summary>
    public AbilityService AbilityService { get; init; } = AbilityService.Instance;

    /// <summary>外部系统准备好的自动施法上下文。</summary>
    public IReadOnlyList<AbilityCastContext>? AbilityContexts { get; set; }

    /// <summary>需要由 AI 行为树自动准备目标的技能实体集合。</summary>
    public IReadOnlyList<IEntity>? AutoTargetAbilities { get; set; }

    /// <summary>最近一次自动施法报告。</summary>
    public IReadOnlyList<AbilityTriggerReport>? LastAbilityReports { get; set; }
}
