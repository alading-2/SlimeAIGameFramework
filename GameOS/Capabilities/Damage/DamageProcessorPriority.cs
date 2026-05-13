namespace SlimeAI.GameOS.Capabilities.Damage;

/// <summary>
/// 默认 Damage 处理器优先级。
/// </summary>
public static class DamageProcessorPriority
{
    /// <summary>基础检查和初始伤害。</summary>
    public const int Base = 100;

    /// <summary>闪避。</summary>
    public const int Dodge = 200;

    /// <summary>暴击。</summary>
    public const int Critical = 300;

    /// <summary>护盾。</summary>
    public const int Shield = 400;

    /// <summary>护甲。</summary>
    public const int Armor = 500;

    /// <summary>受伤倍率。</summary>
    public const int DamageTakenAmplification = 600;

    /// <summary>生命值结算。</summary>
    public const int HealthExecution = 900;

    /// <summary>吸血。</summary>
    public const int Lifesteal = 950;

    /// <summary>统计。</summary>
    public const int Statistics = 1000;
}
