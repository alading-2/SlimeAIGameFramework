namespace SkilmeAI.GameOS.Capabilities.Damage;

/// <summary>
/// 基础伤害处理器，负责前置检查和初始化最终伤害。
/// </summary>
public sealed class BaseDamageProcessor : IDamageProcessor
{
    /// <inheritdoc />
    public int Priority => DamageProcessorPriority.Base;

    /// <inheritdoc />
    public void Process(DamageInfo info)
    {
        if (IsDeadAttackSource(info))
        {
            End(info, "Attack source is dead.");
            return;
        }

        if (info.Victim.Data.Get<bool>(DamageDataKeys.IsDead, false))
        {
            End(info, "Victim is dead.");
            return;
        }

        if (info.Victim.Data.Get<bool>(DamageDataKeys.IsInvulnerable, false))
        {
            End(info, "Victim is invulnerable.");
            return;
        }

        if (info.Damage <= 0f)
        {
            End(info, "Damage must be greater than zero.");
            return;
        }

        info.FinalDamage = info.Damage;
        info.AddLog($"BaseDamage={info.Damage}");
    }

    private static bool IsDeadAttackSource(DamageInfo info)
    {
        return (info.Tags & DamageTags.Attack) != 0
            && info.Attacker?.Data.Get<bool>(DamageDataKeys.IsDead, false) == true;
    }

    private static void End(DamageInfo info, string message)
    {
        info.FinalDamage = 0f;
        info.IsBlocked = true;
        info.IsEnd = true;
        info.AddLog(message);
    }
}
