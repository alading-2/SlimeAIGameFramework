namespace SlimeAI.GameOS.Capabilities.Damage;

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
        info.Attacker?.Data.Add(DamageDataKeys.EncounterDamageDealt, info.FinalDamage);
        info.Attacker?.Data.Add(DamageDataKeys.TotalHits, 1);
        info.Attacker?.Data.Add(DamageDataKeys.EncounterHits, 1);

        info.Victim.Data.Add(DamageDataKeys.TotalDamageTaken, info.FinalDamage);
        info.Victim.Data.Add(DamageDataKeys.EncounterDamageTaken, info.FinalDamage);

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
            info.Attacker?.Data.Add(DamageDataKeys.EncounterCriticalHits, 1);
        }

        if (!info.IsFatal)
        {
            return;
        }

        info.Attacker?.Data.Add(DamageDataKeys.TotalKills, 1);
        info.Attacker?.Data.Add(DamageDataKeys.EncounterKills, 1);
    }
}
