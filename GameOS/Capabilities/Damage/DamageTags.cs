using System;

namespace SlimeAI.GameOS.Capabilities.Damage;

/// <summary>
/// 伤害标签，可叠加描述来源和表现语义。
/// </summary>
[Flags]
public enum DamageTags
{
    /// <summary>无标签。</summary>
    None = 0,

    /// <summary>普通攻击伤害。</summary>
    Attack = 1 << 0,

    /// <summary>技能伤害。</summary>
    Ability = 1 << 1,

    /// <summary>接触伤害。</summary>
    Contact = 1 << 2,

    /// <summary>持续伤害。</summary>
    Persistent = 1 << 3,

    /// <summary>范围伤害。</summary>
    Area = 1 << 4,

    /// <summary>投射物伤害。</summary>
    Projectile = 1 << 5
}
