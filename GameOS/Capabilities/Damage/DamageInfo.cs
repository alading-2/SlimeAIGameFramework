using System;
using System.Collections.Generic;
using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.Capabilities.Damage;

/// <summary>
/// 单次伤害上下文。
/// </summary>
public sealed class DamageInfo
{
    /// <summary>唯一追踪 Id。</summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>直接攻击来源；可以为 null。</summary>
    public IEntity? Attacker { get; init; }

    /// <summary>受击目标。</summary>
    public required IEntity Victim { get; init; }

    /// <summary>原始伤害。</summary>
    public float Damage { get; init; }

    /// <summary>最终结算伤害。</summary>
    public float FinalDamage { get; set; }

    /// <summary>结算前生命值。</summary>
    public float OldHp { get; set; }

    /// <summary>结算后生命值。</summary>
    public float NewHp { get; set; }

    /// <summary>伤害类型。</summary>
    public DamageType Type { get; init; } = DamageType.Physical;

    /// <summary>伤害标签。</summary>
    public DamageTags Tags { get; init; } = DamageTags.None;

    /// <summary>是否为模拟计算；模拟不会写入生命值。</summary>
    public bool IsSimulation { get; init; } = false;

    /// <summary>是否已被阻断。</summary>
    public bool IsBlocked { get; set; } = false;

    /// <summary>是否暴击。</summary>
    public bool IsCritical { get; set; } = false;

    /// <summary>是否闪避。</summary>
    public bool IsDodged { get; set; } = false;

    /// <summary>是否结束伤害管线。</summary>
    public bool IsEnd { get; set; } = false;

    /// <summary>是否致死。</summary>
    public bool IsFatal { get; set; } = false;

    /// <summary>护甲结算倍率。</summary>
    public float ArmorMultiplier { get; set; } = 1f;

    /// <summary>受伤倍率。</summary>
    public float DamageTakenMultiplier { get; set; } = 1f;

    /// <summary>本次吸血恢复量。</summary>
    public float LifestealAmount { get; set; } = 0f;

    /// <summary>本次护盾吸收量。</summary>
    public float ShieldDamageAbsorbed { get; set; } = 0f;

    /// <summary>调试日志。</summary>
    public List<string> Logs { get; } = new();

    /// <summary>
    /// 追加一条伤害管线调试日志。
    /// </summary>
    /// <param name="message">日志内容。</param>
    public void AddLog(string message)
    {
        Logs.Add(message);
    }
}
