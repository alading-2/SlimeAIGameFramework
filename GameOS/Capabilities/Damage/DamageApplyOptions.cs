using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.Capabilities.Damage;

/// <summary>
/// DamageTool 应用伤害的参数。
/// </summary>
public readonly record struct DamageApplyOptions
{
    /// <summary>攻击来源。</summary>
    public IEntity? Attacker { get; init; }

    /// <summary>单次伤害量。</summary>
    public float Damage { get; init; }

    /// <summary>伤害类型。</summary>
    public DamageType Type { get; init; } = DamageType.Physical;

    /// <summary>伤害标签。</summary>
    public DamageTags Tags { get; init; } = DamageTags.None;

    /// <summary>是否只模拟计算。</summary>
    public bool IsSimulation { get; init; } = false;

    /// <summary>
    /// 创建伤害参数。
    /// </summary>
    /// <param name="damage">单次伤害量。</param>
    public DamageApplyOptions(float damage)
    {
        Damage = damage;
    }
}
