using SlimeAI.GameOS.Capabilities.Damage;
using SlimeAI.GameOS.Capabilities.Feature;

namespace SlimeAI.GameOS.Capabilities.Ability;

/// <summary>
/// 技能执行结果。
/// </summary>
public sealed class AbilityExecutedResult : IFeatureExecutionResult
{
    /// <summary>命中的目标数。</summary>
    public int TargetsHit { get; init; }

    /// <summary>造成的总伤害。</summary>
    public float TotalDamage { get; init; }

    /// <summary>DamageTool 结果。</summary>
    public DamageApplyResult? DamageResult { get; init; }
}
