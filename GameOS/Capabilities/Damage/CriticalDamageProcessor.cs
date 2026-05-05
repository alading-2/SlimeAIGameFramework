namespace SkilmeAI.GameOS.Capabilities.Damage;

/// <summary>
/// 暴击处理器。
/// </summary>
public sealed class CriticalDamageProcessor : IDamageProcessor
{
    /// <inheritdoc />
    public int Priority => DamageProcessorPriority.Critical;

    /// <inheritdoc />
    public void Process(DamageInfo info)
    {
        if (info.Attacker == null || info.FinalDamage <= 0f)
        {
            return;
        }

        var chance = info.Attacker.Data.Get<float>(DamageDataKeys.CritRate, 0f);
        if (!DamageMath.CheckProbability(chance))
        {
            return;
        }

        var multiplier = info.Attacker.Data.Get<float>(DamageDataKeys.CritDamage, 100f) / 100f;
        info.IsCritical = true;
        info.FinalDamage *= multiplier;
        info.AddLog($"Critical({multiplier})");
    }
}
