using System;
using SkilmeAI.GameOS.Runtime.Event;

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

        var healthChanged = new GameEventType.Damage.HealthChangedEventData(info.Victim, oldHp, newHp);
        info.Victim.Events.Emit(GameEventType.Damage.HealthChanged, healthChanged);
        GlobalEventBus.Global.Emit(GameEventType.Damage.HealthChanged, healthChanged);

        var damaged = new GameEventType.Damage.DamagedEventData(info);
        info.Victim.Events.Emit(GameEventType.Damage.Damaged, damaged);
        GlobalEventBus.Global.Emit(GameEventType.Damage.Damaged, damaged);

        if (!info.IsFatal)
        {
            return;
        }

        info.Victim.Data.Set(DamageDataKeys.IsDead, true);
        var killed = new GameEventType.Damage.KilledEventData(info.Victim, info.Attacker, info);
        info.Victim.Events.Emit(GameEventType.Damage.Killed, killed);
        GlobalEventBus.Global.Emit(GameEventType.Damage.Killed, killed);
    }
}
