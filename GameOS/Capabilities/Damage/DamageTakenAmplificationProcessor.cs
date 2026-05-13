namespace SlimeAI.GameOS.Capabilities.Damage;

/// <summary>
/// 受伤倍率处理器。
/// </summary>
public sealed class DamageTakenAmplificationProcessor : IDamageProcessor
{
    /// <inheritdoc />
    public int Priority => DamageProcessorPriority.DamageTakenAmplification;

    /// <inheritdoc />
    public void Process(DamageInfo info)
    {
        if (info.FinalDamage <= 0f)
        {
            return;
        }

        var multiplier = info.Victim.Data.Get<float>(DamageDataKeys.DamageTakenMultiplier, 1f);
        if (multiplier == 1f)
        {
            return;
        }

        info.DamageTakenMultiplier = multiplier;
        info.FinalDamage *= multiplier;
        info.AddLog($"TakenMultiplier({multiplier})");
    }
}
