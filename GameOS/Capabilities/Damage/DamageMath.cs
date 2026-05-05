using System;

namespace SkilmeAI.GameOS.Capabilities.Damage;

/// <summary>
/// Damage Capability 内部数学工具。
/// </summary>
internal static class DamageMath
{
    private const float ArmorCoefficient = 15f;
    private const float MaxArmorReduction = 80f;
    private const float NegativeArmorCoefficient = 30f;

    /// <summary>
    /// 计算护甲造成的伤害倍率。
    /// </summary>
    /// <param name="armor">护甲值。</param>
    public static float CalculateArmorDamageMultiplier(float armor)
    {
        if (armor >= 0f)
        {
            var reductionRate = armor / (armor + ArmorCoefficient);
            reductionRate = Math.Clamp(reductionRate, 0f, MaxArmorReduction / 100f);
            return 1f - reductionRate;
        }

        var rate = 1f + MathF.Abs(armor) / NegativeArmorCoefficient;
        return 1f + rate;
    }

    /// <summary>
    /// 检查 0-100 概率是否触发。
    /// </summary>
    /// <param name="chance">概率百分比。</param>
    public static bool CheckProbability(float chance)
    {
        if (chance <= 0f)
        {
            return false;
        }

        if (chance >= 100f)
        {
            return true;
        }

        return Random.Shared.NextSingle() * 100f < chance;
    }
}
