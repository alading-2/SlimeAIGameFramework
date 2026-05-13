using System.Collections.Generic;
using SlimeAI.GameOS.Runtime.Timer;

namespace SlimeAI.GameOS.Capabilities.Damage;

/// <summary>
/// DamageTool 应用结果。
/// </summary>
public sealed class DamageApplyResult
{
    /// <summary>目标数量。</summary>
    public int TargetCount { get; internal set; }

    /// <summary>成功应用的伤害次数。</summary>
    public int AppliedCount { get; internal set; }

    /// <summary>周期伤害计时器；非周期伤害为空。</summary>
    public GameTimer? Timer { get; internal set; }

    /// <summary>每个目标的伤害结果。</summary>
    public List<DamageResult> Results { get; } = new();
}
