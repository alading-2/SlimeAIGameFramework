using SkilmeAI.GameOS.Runtime.Data;

namespace SkilmeAI.GameOS.Capabilities.Damage;

/// <summary>
/// Damage Capability 使用的运行时 DataKey。
/// </summary>
public static class DamageDataKeys
{
    /// <summary>当前生命值。</summary>
    public static readonly DataKey<float> CurrentHp = DataKey.Create<float>("Damage.CurrentHp",
        defaultValue: 0f,
        category: DamageCategory.Runtime,
        minValue: 0f);

    /// <summary>最大生命值。</summary>
    public static readonly DataKey<float> MaxHp = DataKey.Create<float>("Damage.MaxHp",
        defaultValue: 0f,
        category: DamageCategory.Config,
        minValue: 0f,
        supportsModifiers: true);

    /// <summary>实体是否死亡。</summary>
    public static readonly DataKey<bool> IsDead = DataKey.Create<bool>("Damage.IsDead",
        defaultValue: false,
        category: DamageCategory.Runtime);

    /// <summary>实体是否处于无敌状态。</summary>
    public static readonly DataKey<bool> IsInvulnerable = DataKey.Create<bool>("Damage.IsInvulnerable",
        defaultValue: false,
        category: DamageCategory.Runtime);

    /// <summary>护甲值。</summary>
    public static readonly DataKey<float> Armor = DataKey.Create<float>("Damage.Armor",
        defaultValue: 0f,
        category: DamageCategory.Config,
        supportsModifiers: true);

    /// <summary>闪避概率，0-100。</summary>
    public static readonly DataKey<float> DodgeChance = DataKey.Create<float>("Damage.DodgeChance",
        defaultValue: 0f,
        category: DamageCategory.Config,
        minValue: 0f,
        maxValue: 100f,
        isPercentage: true,
        supportsModifiers: true);

    /// <summary>暴击概率，0-100。</summary>
    public static readonly DataKey<float> CritRate = DataKey.Create<float>("Damage.CritRate",
        defaultValue: 0f,
        category: DamageCategory.Config,
        minValue: 0f,
        maxValue: 100f,
        isPercentage: true,
        supportsModifiers: true);

    /// <summary>暴击伤害百分比，100 表示 1 倍。</summary>
    public static readonly DataKey<float> CritDamage = DataKey.Create<float>("Damage.CritDamage",
        defaultValue: 100f,
        category: DamageCategory.Config,
        minValue: 0f,
        isPercentage: true,
        supportsModifiers: true);

    /// <summary>受伤倍率，1 表示正常。</summary>
    public static readonly DataKey<float> DamageTakenMultiplier = DataKey.Create<float>("Damage.DamageTakenMultiplier",
        defaultValue: 1f,
        category: DamageCategory.Config,
        minValue: 0f,
        supportsModifiers: true);

    /// <summary>吸血概率 / 比例，0-100。</summary>
    public static readonly DataKey<float> LifeSteal = DataKey.Create<float>("Damage.LifeSteal",
        defaultValue: 0f,
        category: DamageCategory.Config,
        minValue: 0f,
        maxValue: 100f,
        isPercentage: true,
        supportsModifiers: true);

    /// <summary>护盾值。</summary>
    public static readonly DataKey<float> Shield = DataKey.Create<float>("Damage.Shield",
        defaultValue: 0f,
        category: DamageCategory.Runtime,
        minValue: 0f,
        supportsModifiers: true);

    /// <summary>接触伤害数值。</summary>
    public static readonly DataKey<float> ContactDamage = DataKey.Create<float>("Damage.ContactDamage",
        defaultValue: 0f,
        category: DamageCategory.Config,
        minValue: 0f,
        supportsModifiers: true);

    /// <summary>接触伤害循环间隔。</summary>
    public static readonly DataKey<float> ContactDamageInterval = DataKey.Create<float>("Damage.ContactDamageInterval",
        defaultValue: 1f,
        category: DamageCategory.Config,
        minValue: 0.01f);

    /// <summary>累计造成伤害。</summary>
    public static readonly DataKey<float> TotalDamageDealt = DataKey.Create<float>("Damage.TotalDamageDealt",
        defaultValue: 0f,
        category: DamageCategory.Statistics,
        minValue: 0f);

    /// <summary>累计承受伤害。</summary>
    public static readonly DataKey<float> TotalDamageTaken = DataKey.Create<float>("Damage.TotalDamageTaken",
        defaultValue: 0f,
        category: DamageCategory.Statistics,
        minValue: 0f);

    /// <summary>当前波次造成伤害。</summary>
    public static readonly DataKey<float> WaveDamageDealt = DataKey.Create<float>("Damage.WaveDamageDealt",
        defaultValue: 0f,
        category: DamageCategory.Statistics,
        minValue: 0f);

    /// <summary>当前波次承受伤害。</summary>
    public static readonly DataKey<float> WaveDamageTaken = DataKey.Create<float>("Damage.WaveDamageTaken",
        defaultValue: 0f,
        category: DamageCategory.Statistics,
        minValue: 0f);

    /// <summary>最高单次伤害。</summary>
    public static readonly DataKey<float> HighestSingleDamage = DataKey.Create<float>("Damage.HighestSingleDamage",
        defaultValue: 0f,
        category: DamageCategory.Statistics,
        minValue: 0f);

    /// <summary>累计命中次数。</summary>
    public static readonly DataKey<int> TotalHits = DataKey.Create<int>("Damage.TotalHits",
        defaultValue: 0,
        category: DamageCategory.Statistics,
        minValue: 0f);

    /// <summary>当前波次命中次数。</summary>
    public static readonly DataKey<int> WaveHits = DataKey.Create<int>("Damage.WaveHits",
        defaultValue: 0,
        category: DamageCategory.Statistics,
        minValue: 0f);

    /// <summary>累计暴击次数。</summary>
    public static readonly DataKey<int> TotalCriticalHits = DataKey.Create<int>("Damage.TotalCriticalHits",
        defaultValue: 0,
        category: DamageCategory.Statistics,
        minValue: 0f);

    /// <summary>当前波次暴击次数。</summary>
    public static readonly DataKey<int> WaveCriticalHits = DataKey.Create<int>("Damage.WaveCriticalHits",
        defaultValue: 0,
        category: DamageCategory.Statistics,
        minValue: 0f);

    /// <summary>累计击杀次数。</summary>
    public static readonly DataKey<int> TotalKills = DataKey.Create<int>("Damage.TotalKills",
        defaultValue: 0,
        category: DamageCategory.Statistics,
        minValue: 0f);

    /// <summary>当前波次击杀次数。</summary>
    public static readonly DataKey<int> WaveKills = DataKey.Create<int>("Damage.WaveKills",
        defaultValue: 0,
        category: DamageCategory.Statistics,
        minValue: 0f);

    /// <summary>累计治疗量。</summary>
    public static readonly DataKey<float> TotalHealingDone = DataKey.Create<float>("Damage.TotalHealingDone",
        defaultValue: 0f,
        category: DamageCategory.Statistics,
        minValue: 0f);

    /// <summary>累计承受治疗量。</summary>
    public static readonly DataKey<float> TotalHealingReceived = DataKey.Create<float>("Damage.TotalHealingReceived",
        defaultValue: 0f,
        category: DamageCategory.Statistics,
        minValue: 0f);

    /// <summary>累计护盾吸收量。</summary>
    public static readonly DataKey<float> TotalShieldAbsorbed = DataKey.Create<float>("Damage.TotalShieldAbsorbed",
        defaultValue: 0f,
        category: DamageCategory.Statistics,
        minValue: 0f);

    /// <summary>
    /// 显式触发静态 DataKey 注册。
    /// </summary>
    public static void RegisterAll()
    {
        _ = CurrentHp;
        _ = MaxHp;
        _ = IsDead;
        _ = IsInvulnerable;
        _ = Armor;
        _ = DodgeChance;
        _ = CritRate;
        _ = CritDamage;
        _ = DamageTakenMultiplier;
        _ = LifeSteal;
        _ = Shield;
        _ = ContactDamage;
        _ = ContactDamageInterval;
        _ = TotalDamageDealt;
        _ = TotalDamageTaken;
        _ = WaveDamageDealt;
        _ = WaveDamageTaken;
        _ = HighestSingleDamage;
        _ = TotalHits;
        _ = WaveHits;
        _ = TotalCriticalHits;
        _ = WaveCriticalHits;
        _ = TotalKills;
        _ = WaveKills;
        _ = TotalHealingDone;
        _ = TotalHealingReceived;
        _ = TotalShieldAbsorbed;
    }
}
