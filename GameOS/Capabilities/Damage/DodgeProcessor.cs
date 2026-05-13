using SlimeAI.GameOS.Capabilities.Damage.Events;

namespace SlimeAI.GameOS.Capabilities.Damage;

/// <summary>
/// 闪避处理器。
/// </summary>
public sealed class DodgeProcessor : IDamageProcessor
{
    /// <inheritdoc />
    public int Priority => DamageProcessorPriority.Dodge;

    /// <inheritdoc />
    public void Process(DamageInfo info)
    {
        if (info.Type == DamageType.True || info.FinalDamage <= 0f)
        {
            return;
        }

        var chance = info.Victim.Data.Get<float>(DamageDataKeys.DodgeChance, 0f);
        if (!DamageMath.CheckProbability(chance))
        {
            return;
        }

        info.IsDodged = true;
        info.IsEnd = true;
        info.FinalDamage = 0f;
        info.AddLog($"Dodged({chance})");

        if (info.IsSimulation)
        {
            return;
        }

        info.Victim.Events.Publish(new Dodged(info.Victim, info.Attacker, info));
    }
}
