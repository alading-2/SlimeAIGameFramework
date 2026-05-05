namespace SkilmeAI.GameOS.Capabilities.Damage;

/// <summary>
/// 伤害统计处理器。
/// </summary>
public sealed class DamageStatisticsProcessor : IDamageProcessor
{
    /// <inheritdoc />
    public int Priority => DamageProcessorPriority.Statistics;

    /// <inheritdoc />
    public void Process(DamageInfo info)
    {
        if (info.FinalDamage <= 0f || info.IsSimulation)
        {
            return;
        }

        info.Attacker?.Data.Add(DamageDataKeys.TotalDamageDealt, info.FinalDamage);
        info.Attacker?.Data.Add(DamageDataKeys.WaveDamageDealt, info.FinalDamage);
        info.Attacker?.Data.Add(DamageDataKeys.TotalHits, 1);
        info.Attacker?.Data.Add(DamageDataKeys.WaveHits, 1);

        info.Victim.Data.Add(DamageDataKeys.TotalDamageTaken, info.FinalDamage);
        info.Victim.Data.Add(DamageDataKeys.WaveDamageTaken, info.FinalDamage);

        if (info.Attacker != null)
        {
            var highest = info.Attacker.Data.Get<float>(DamageDataKeys.HighestSingleDamage, 0f);
            if (info.FinalDamage > highest)
            {
                info.Attacker.Data.Set(DamageDataKeys.HighestSingleDamage, info.FinalDamage);
            }
        }

        if (info.IsCritical)
        {
            info.Attacker?.Data.Add(DamageDataKeys.TotalCriticalHits, 1);
            info.Attacker?.Data.Add(DamageDataKeys.WaveCriticalHits, 1);
        }

        if (!info.IsFatal)
        {
            return;
        }

        info.Attacker?.Data.Add(DamageDataKeys.TotalKills, 1);
        info.Attacker?.Data.Add(DamageDataKeys.WaveKills, 1);
    }
}
