using System;

namespace SlimeAI.GameOS.Capabilities.Damage;

/// <summary>
/// 护盾处理器，优先于护甲吸收最终伤害。
/// </summary>
public sealed class ShieldDamageProcessor : IDamageProcessor
{
    /// <inheritdoc />
    public int Priority => DamageProcessorPriority.Shield;

    /// <inheritdoc />
    public void Process(DamageInfo info)
    {
        if (info.FinalDamage <= 0f)
        {
            return;
        }

        var shield = info.Victim.Data.Get<float>(DamageDataKeys.Shield, 0f);
        if (shield <= 0f)
        {
            return;
        }

        var absorbed = MathF.Min(shield, info.FinalDamage);
        info.ShieldDamageAbsorbed = absorbed;
        info.FinalDamage = MathF.Max(0f, info.FinalDamage - absorbed);
        info.AddLog($"Shield({absorbed})");

        if (!info.IsSimulation)
        {
            info.Victim.Data.Set(DamageDataKeys.Shield, shield - absorbed);
            info.Victim.Data.Add(DamageDataKeys.TotalShieldAbsorbed, absorbed);
        }

        if (info.FinalDamage > 0f)
        {
            return;
        }

        info.IsBlocked = true;
        info.IsEnd = true;
    }
}
