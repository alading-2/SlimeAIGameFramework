namespace SkilmeAI.GameOS.Capabilities.Damage;

/// <summary>
/// 吸血处理器。
/// </summary>
public sealed class LifestealProcessor : IDamageProcessor
{
    /// <inheritdoc />
    public int Priority => DamageProcessorPriority.Lifesteal;

    /// <inheritdoc />
    public void Process(DamageInfo info)
    {
        if (info.Attacker == null || info.FinalDamage <= 0f || info.IsSimulation)
        {
            return;
        }

        var chance = info.Attacker.Data.Get<float>(DamageDataKeys.LifeSteal, 0f);
        if (!DamageMath.CheckProbability(chance))
        {
            return;
        }

        var healAmount = info.FinalDamage * (chance / 100f);
        var result = HealService.Instance.Process(new HealInfo
        {
            Healer = info.Attacker,
            Target = info.Attacker,
            Amount = healAmount,
            Source = HealSource.Lifesteal,
            SourceDamage = info
        });
        info.LifestealAmount = result.Info.FinalAmount;
        if (result.Applied)
        {
            info.AddLog($"Lifesteal({info.LifestealAmount})");
        }
    }
}
