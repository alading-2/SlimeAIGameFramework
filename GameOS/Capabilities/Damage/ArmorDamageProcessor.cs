namespace SkilmeAI.GameOS.Capabilities.Damage;

/// <summary>
/// 护甲处理器。
/// </summary>
public sealed class ArmorDamageProcessor : IDamageProcessor
{
    /// <inheritdoc />
    public int Priority => DamageProcessorPriority.Armor;

    /// <inheritdoc />
    public void Process(DamageInfo info)
    {
        if (info.Type == DamageType.True || info.FinalDamage <= 0f)
        {
            return;
        }

        var armor = info.Victim.Data.Get<float>(DamageDataKeys.Armor, 0f);
        if (armor == 0f)
        {
            return;
        }

        info.ArmorMultiplier = DamageMath.CalculateArmorDamageMultiplier(armor);
        info.FinalDamage *= info.ArmorMultiplier;
        info.AddLog($"Armor({armor})x{info.ArmorMultiplier}");
    }
}
