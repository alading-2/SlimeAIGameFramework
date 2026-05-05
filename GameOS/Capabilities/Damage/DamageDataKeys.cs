using SkilmeAI.GameOS.Runtime.Data;

namespace SkilmeAI.GameOS.Capabilities.Damage;

/// <summary>
/// Damage Capability 使用的运行时 DataKey。
/// </summary>
public static class DamageDataKeys
{
    /// <summary>当前生命值。</summary>
    public static readonly DataMeta CurrentHp = DataRegistry.Register(new DataMeta
    {
        Key = "Damage.CurrentHp",
        DisplayName = "Current HP",
        Type = typeof(float),
        Category = DamageCategory.Runtime,
        DefaultValue = 0f,
        MinValue = 0f,
        Description = "实体当前生命值。"
    });

    /// <summary>最大生命值。</summary>
    public static readonly DataMeta MaxHp = DataRegistry.Register(new DataMeta
    {
        Key = "Damage.MaxHp",
        DisplayName = "Max HP",
        Type = typeof(float),
        Category = DamageCategory.Config,
        DefaultValue = 0f,
        MinValue = 0f,
        SupportModifiers = true,
        Description = "实体最大生命值。"
    });

    /// <summary>实体是否死亡。</summary>
    public static readonly DataMeta IsDead = DataRegistry.Register(new DataMeta
    {
        Key = "Damage.IsDead",
        DisplayName = "Is Dead",
        Type = typeof(bool),
        Category = DamageCategory.Runtime,
        DefaultValue = false,
        Description = "实体生命值是否已经归零。"
    });

    /// <summary>实体是否处于无敌状态。</summary>
    public static readonly DataMeta IsInvulnerable = DataRegistry.Register(new DataMeta
    {
        Key = "Damage.IsInvulnerable",
        DisplayName = "Is Invulnerable",
        Type = typeof(bool),
        Category = DamageCategory.Runtime,
        DefaultValue = false,
        Description = "实体当前是否免疫伤害。"
    });

    /// <summary>护甲值。</summary>
    public static readonly DataMeta Armor = DataRegistry.Register(new DataMeta
    {
        Key = "Damage.Armor",
        DisplayName = "Armor",
        Type = typeof(float),
        Category = DamageCategory.Config,
        DefaultValue = 0f,
        SupportModifiers = true,
        Description = "物理和魔法伤害的护甲减免值。"
    });

    /// <summary>闪避概率，0-100。</summary>
    public static readonly DataMeta DodgeChance = DataRegistry.Register(new DataMeta
    {
        Key = "Damage.DodgeChance",
        DisplayName = "Dodge Chance",
        Type = typeof(float),
        Category = DamageCategory.Config,
        DefaultValue = 0f,
        MinValue = 0f,
        MaxValue = 100f,
        IsPercentage = true,
        SupportModifiers = true,
        Description = "非真实伤害的闪避概率。"
    });

    /// <summary>暴击概率，0-100。</summary>
    public static readonly DataMeta CritRate = DataRegistry.Register(new DataMeta
    {
        Key = "Damage.CritRate",
        DisplayName = "Critical Rate",
        Type = typeof(float),
        Category = DamageCategory.Config,
        DefaultValue = 0f,
        MinValue = 0f,
        MaxValue = 100f,
        IsPercentage = true,
        SupportModifiers = true,
        Description = "攻击者触发暴击的概率。"
    });

    /// <summary>暴击伤害百分比，100 表示 1 倍。</summary>
    public static readonly DataMeta CritDamage = DataRegistry.Register(new DataMeta
    {
        Key = "Damage.CritDamage",
        DisplayName = "Critical Damage",
        Type = typeof(float),
        Category = DamageCategory.Config,
        DefaultValue = 100f,
        MinValue = 0f,
        IsPercentage = true,
        SupportModifiers = true,
        Description = "暴击后的最终伤害倍率百分比。"
    });

    /// <summary>受伤倍率，1 表示正常。</summary>
    public static readonly DataMeta DamageTakenMultiplier = DataRegistry.Register(new DataMeta
    {
        Key = "Damage.DamageTakenMultiplier",
        DisplayName = "Damage Taken Multiplier",
        Type = typeof(float),
        Category = DamageCategory.Config,
        DefaultValue = 1f,
        MinValue = 0f,
        SupportModifiers = true,
        Description = "目标承受最终伤害的倍率。"
    });

    /// <summary>吸血概率 / 比例，0-100。</summary>
    public static readonly DataMeta LifeSteal = DataRegistry.Register(new DataMeta
    {
        Key = "Damage.LifeSteal",
        DisplayName = "Life Steal",
        Type = typeof(float),
        Category = DamageCategory.Config,
        DefaultValue = 0f,
        MinValue = 0f,
        MaxValue = 100f,
        IsPercentage = true,
        SupportModifiers = true,
        Description = "触发吸血的概率，同时作为本次最终伤害的恢复比例。"
    });

    /// <summary>护盾值。</summary>
    public static readonly DataMeta Shield = DataRegistry.Register(new DataMeta
    {
        Key = "Damage.Shield",
        DisplayName = "Shield",
        Type = typeof(float),
        Category = DamageCategory.Runtime,
        DefaultValue = 0f,
        MinValue = 0f,
        SupportModifiers = true,
        Description = "优先于生命值承受的护盾值。"
    });

    /// <summary>接触伤害数值。</summary>
    public static readonly DataMeta ContactDamage = DataRegistry.Register(new DataMeta
    {
        Key = "Damage.ContactDamage",
        DisplayName = "Contact Damage",
        Type = typeof(float),
        Category = DamageCategory.Config,
        DefaultValue = 0f,
        MinValue = 0f,
        SupportModifiers = true,
        Description = "作为接触攻击源时造成的单次伤害。"
    });

    /// <summary>接触伤害循环间隔。</summary>
    public static readonly DataMeta ContactDamageInterval = DataRegistry.Register(new DataMeta
    {
        Key = "Damage.ContactDamageInterval",
        DisplayName = "Contact Damage Interval",
        Type = typeof(float),
        Category = DamageCategory.Config,
        DefaultValue = 1f,
        MinValue = 0.01f,
        Description = "持续接触时两次伤害之间的间隔秒数。"
    });

    /// <summary>累计造成伤害。</summary>
    public static readonly DataMeta TotalDamageDealt = DataRegistry.Register(new DataMeta
    {
        Key = "Damage.TotalDamageDealt",
        DisplayName = "Total Damage Dealt",
        Type = typeof(float),
        Category = DamageCategory.Statistics,
        DefaultValue = 0f,
        MinValue = 0f,
        Description = "实体累计造成的伤害。"
    });

    /// <summary>累计承受伤害。</summary>
    public static readonly DataMeta TotalDamageTaken = DataRegistry.Register(new DataMeta
    {
        Key = "Damage.TotalDamageTaken",
        DisplayName = "Total Damage Taken",
        Type = typeof(float),
        Category = DamageCategory.Statistics,
        DefaultValue = 0f,
        MinValue = 0f,
        Description = "实体累计承受的伤害。"
    });

    /// <summary>当前波次造成伤害。</summary>
    public static readonly DataMeta WaveDamageDealt = DataRegistry.Register(new DataMeta
    {
        Key = "Damage.WaveDamageDealt",
        DisplayName = "Wave Damage Dealt",
        Type = typeof(float),
        Category = DamageCategory.Statistics,
        DefaultValue = 0f,
        MinValue = 0f,
        Description = "实体当前波次累计造成的伤害。"
    });

    /// <summary>当前波次承受伤害。</summary>
    public static readonly DataMeta WaveDamageTaken = DataRegistry.Register(new DataMeta
    {
        Key = "Damage.WaveDamageTaken",
        DisplayName = "Wave Damage Taken",
        Type = typeof(float),
        Category = DamageCategory.Statistics,
        DefaultValue = 0f,
        MinValue = 0f,
        Description = "实体当前波次累计承受的伤害。"
    });

    /// <summary>最高单次伤害。</summary>
    public static readonly DataMeta HighestSingleDamage = DataRegistry.Register(new DataMeta
    {
        Key = "Damage.HighestSingleDamage",
        DisplayName = "Highest Single Damage",
        Type = typeof(float),
        Category = DamageCategory.Statistics,
        DefaultValue = 0f,
        MinValue = 0f,
        Description = "实体造成过的最高单次最终伤害。"
    });

    /// <summary>累计命中次数。</summary>
    public static readonly DataMeta TotalHits = DataRegistry.Register(new DataMeta
    {
        Key = "Damage.TotalHits",
        DisplayName = "Total Hits",
        Type = typeof(int),
        Category = DamageCategory.Statistics,
        DefaultValue = 0,
        MinValue = 0f,
        Description = "实体累计有效命中次数。"
    });

    /// <summary>当前波次命中次数。</summary>
    public static readonly DataMeta WaveHits = DataRegistry.Register(new DataMeta
    {
        Key = "Damage.WaveHits",
        DisplayName = "Wave Hits",
        Type = typeof(int),
        Category = DamageCategory.Statistics,
        DefaultValue = 0,
        MinValue = 0f,
        Description = "实体当前波次有效命中次数。"
    });

    /// <summary>累计暴击次数。</summary>
    public static readonly DataMeta TotalCriticalHits = DataRegistry.Register(new DataMeta
    {
        Key = "Damage.TotalCriticalHits",
        DisplayName = "Total Critical Hits",
        Type = typeof(int),
        Category = DamageCategory.Statistics,
        DefaultValue = 0,
        MinValue = 0f,
        Description = "实体累计暴击次数。"
    });

    /// <summary>当前波次暴击次数。</summary>
    public static readonly DataMeta WaveCriticalHits = DataRegistry.Register(new DataMeta
    {
        Key = "Damage.WaveCriticalHits",
        DisplayName = "Wave Critical Hits",
        Type = typeof(int),
        Category = DamageCategory.Statistics,
        DefaultValue = 0,
        MinValue = 0f,
        Description = "实体当前波次暴击次数。"
    });

    /// <summary>累计击杀次数。</summary>
    public static readonly DataMeta TotalKills = DataRegistry.Register(new DataMeta
    {
        Key = "Damage.TotalKills",
        DisplayName = "Total Kills",
        Type = typeof(int),
        Category = DamageCategory.Statistics,
        DefaultValue = 0,
        MinValue = 0f,
        Description = "实体累计击杀次数。"
    });

    /// <summary>当前波次击杀次数。</summary>
    public static readonly DataMeta WaveKills = DataRegistry.Register(new DataMeta
    {
        Key = "Damage.WaveKills",
        DisplayName = "Wave Kills",
        Type = typeof(int),
        Category = DamageCategory.Statistics,
        DefaultValue = 0,
        MinValue = 0f,
        Description = "实体当前波次击杀次数。"
    });

    /// <summary>累计治疗量。</summary>
    public static readonly DataMeta TotalHealingDone = DataRegistry.Register(new DataMeta
    {
        Key = "Damage.TotalHealingDone",
        DisplayName = "Total Healing Done",
        Type = typeof(float),
        Category = DamageCategory.Statistics,
        DefaultValue = 0f,
        MinValue = 0f,
        Description = "实体累计造成的治疗量。"
    });

    /// <summary>累计承受治疗量。</summary>
    public static readonly DataMeta TotalHealingReceived = DataRegistry.Register(new DataMeta
    {
        Key = "Damage.TotalHealingReceived",
        DisplayName = "Total Healing Received",
        Type = typeof(float),
        Category = DamageCategory.Statistics,
        DefaultValue = 0f,
        MinValue = 0f,
        Description = "实体累计承受的治疗量。"
    });

    /// <summary>累计护盾吸收量。</summary>
    public static readonly DataMeta TotalShieldAbsorbed = DataRegistry.Register(new DataMeta
    {
        Key = "Damage.TotalShieldAbsorbed",
        DisplayName = "Total Shield Absorbed",
        Type = typeof(float),
        Category = DamageCategory.Statistics,
        DefaultValue = 0f,
        MinValue = 0f,
        Description = "实体累计通过护盾吸收的伤害。"
    });

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
