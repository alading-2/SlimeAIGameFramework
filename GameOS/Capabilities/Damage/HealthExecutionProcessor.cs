using System;
using SkilmeAI.GameOS.Capabilities.Damage.Events;

namespace SkilmeAI.GameOS.Capabilities.Damage;

/// <summary>
/// 生命值结算处理器。
/// </summary>
public sealed class HealthExecutionProcessor : IDamageProcessor
{
    /// <inheritdoc />
    public int Priority => DamageProcessorPriority.HealthExecution;

    /// <inheritdoc />
    public void Process(DamageInfo info)
    {
        if (info.FinalDamage <= 0f)
        {
            info.IsBlocked = true;
            info.IsEnd = true;
            return;
        }

        var oldHp = DamageService.ReadCurrentHp(info.Victim);
        var newHp = MathF.Max(0f, oldHp - info.FinalDamage);
        info.OldHp = oldHp;
        info.NewHp = newHp;
        info.IsFatal = newHp <= 0f;

        if (info.IsSimulation)
        {
            return;
        }

        info.Victim.Data.Set(DamageDataKeys.CurrentHp, newHp);

        info.Victim.Events.Publish(new HealthChanged(info.Victim, oldHp, newHp));
        info.Victim.Events.Publish(new Damaged(info));

        if (!info.IsFatal)
        {
            return;
        }

        info.Victim.Data.Set(DamageDataKeys.IsDead, true);
        info.Victim.Events.Publish(new Killed(info.Victim, info.Attacker, info));
    }
}
