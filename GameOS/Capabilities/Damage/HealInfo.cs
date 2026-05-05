using System;
using System.Collections.Generic;
using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.Capabilities.Damage;

/// <summary>
/// 单次治疗上下文。
/// </summary>
public sealed class HealInfo
{
    /// <summary>唯一追踪 Id。</summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>治疗来源实体；可以为 null。</summary>
    public IEntity? Healer { get; init; }

    /// <summary>治疗目标。</summary>
    public required IEntity Target { get; init; }

    /// <summary>原始治疗量。</summary>
    public float Amount { get; init; }

    /// <summary>最终有效治疗量。</summary>
    public float FinalAmount { get; set; }

    /// <summary>治疗来源类型。</summary>
    public HealSource Source { get; init; } = HealSource.Direct;

    /// <summary>关联的源伤害；吸血等场景使用。</summary>
    public DamageInfo? SourceDamage { get; init; }

    /// <summary>是否为模拟计算；模拟不会写入生命值。</summary>
    public bool IsSimulation { get; init; } = false;

    /// <summary>治疗前生命值。</summary>
    public float OldHp { get; set; }

    /// <summary>治疗后生命值。</summary>
    public float NewHp { get; set; }

    /// <summary>是否被阻断。</summary>
    public bool IsBlocked { get; set; }

    /// <summary>调试日志。</summary>
    public List<string> Logs { get; } = new();

    /// <summary>
    /// 追加一条治疗日志。
    /// </summary>
    /// <param name="message">日志内容。</param>
    public void AddLog(string message)
    {
        Logs.Add(message);
    }
}
