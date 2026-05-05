using System.Collections.Generic;
using SkilmeAI.GameOS.Capabilities.Damage;
using SkilmeAI.GameOS.Capabilities.Movement;
using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.Capabilities.Ability;

/// <summary>
/// 单次技能触发上下文。
/// </summary>
public sealed class AbilityCastContext
{
    /// <summary>施法者。</summary>
    public required IEntity Caster { get; init; }

    /// <summary>技能实体。</summary>
    public required IEntity Ability { get; init; }

    /// <summary>显式目标列表。</summary>
    public IReadOnlyList<IEntity>? Targets { get; init; }

    /// <summary>点选目标位置，由输入层在正式触发前写入。</summary>
    public Vector2Value? TargetPosition { get; init; }

    /// <summary>伤害类型。</summary>
    public DamageType DamageType { get; init; } = DamageType.Physical;

    /// <summary>额外伤害标签。</summary>
    public DamageTags DamageTags { get; init; } = DamageTags.None;
}
