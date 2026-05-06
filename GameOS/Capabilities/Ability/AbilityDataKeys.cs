using SkilmeAI.GameOS.Runtime.Data;

namespace SkilmeAI.GameOS.Capabilities.Ability;

/// <summary>
/// Ability Capability 使用的运行时 DataKey。
/// </summary>
public static class AbilityDataKeys
{
    /// <summary>技能名称。</summary>
    public static readonly DataMeta Name = DataRegistry.Register(new DataMeta
    {
        Key = "Ability.Name",
        DisplayName = "Ability Name",
        Type = typeof(string),
        Category = AbilityCategory.Basic,
        DefaultValue = string.Empty,
        Description = "技能显示名称。"
    });

    /// <summary>技能类型。</summary>
    public static readonly DataMeta Type = DataRegistry.Register(new DataMeta
    {
        Key = "Ability.Type",
        DisplayName = "Ability Type",
        Type = typeof(AbilityType),
        Category = AbilityCategory.Basic,
        DefaultValue = AbilityType.Passive,
        Description = "技能类型。"
    });

    /// <summary>触发模式。</summary>
    public static readonly DataMeta TriggerMode = DataRegistry.Register(new DataMeta
    {
        Key = "Ability.TriggerMode",
        DisplayName = "Trigger Mode",
        Type = typeof(AbilityTriggerMode),
        Category = AbilityCategory.Basic,
        DefaultValue = AbilityTriggerMode.None,
        Description = "技能触发模式。"
    });

    /// <summary>目标选择方式。</summary>
    public static readonly DataMeta TargetSelection = DataRegistry.Register(new DataMeta
    {
        Key = "Ability.TargetSelection",
        DisplayName = "Target Selection",
        Type = typeof(AbilityTargetSelection),
        Category = AbilityCategory.Target,
        DefaultValue = AbilityTargetSelection.None,
        Description = "技能需要的目标输入。"
    });

    /// <summary>自动索敌半径，-1 表示不限制距离。</summary>
    public static readonly DataMeta AutoTargetRange = DataRegistry.Register(new DataMeta
    {
        Key = "Ability.AutoTargetRange",
        DisplayName = "Auto Target Range",
        Type = typeof(float),
        Category = AbilityCategory.Target,
        DefaultValue = -1f,
        MinValue = -1f,
        SupportModifiers = true,
        Description = "自动索敌半径；-1 表示不限制距离。"
    });

    /// <summary>自动索敌最大目标数，-1 表示不限制数量。</summary>
    public static readonly DataMeta AutoTargetMaxTargets = DataRegistry.Register(new DataMeta
    {
        Key = "Ability.AutoTargetMaxTargets",
        DisplayName = "Auto Target Max Targets",
        Type = typeof(int),
        Category = AbilityCategory.Target,
        DefaultValue = 1,
        MinValue = -1f,
        Description = "自动索敌最多选中多少目标；-1 表示不限制数量。"
    });

    /// <summary>自动索敌是否过滤同队目标。</summary>
    public static readonly DataMeta AutoTargetIgnoreSameTeam = DataRegistry.Register(new DataMeta
    {
        Key = "Ability.AutoTargetIgnoreSameTeam",
        DisplayName = "Auto Target Ignore Same Team",
        Type = typeof(bool),
        Category = AbilityCategory.Target,
        DefaultValue = true,
        Description = "自动索敌时是否过滤与施法者同队的实体。"
    });

    /// <summary>自动索敌是否只选择具备 HP Data 的实体。</summary>
    public static readonly DataMeta AutoTargetRequiresDamageable = DataRegistry.Register(new DataMeta
    {
        Key = "Ability.AutoTargetRequiresDamageable",
        DisplayName = "Auto Target Requires Damageable",
        Type = typeof(bool),
        Category = AbilityCategory.Target,
        DefaultValue = true,
        Description = "自动索敌时是否只选择具备 DamageDataKeys.CurrentHp 的实体。"
    });

    /// <summary>Ability 对应的 Feature Handler Id。</summary>
    public static readonly DataMeta FeatureHandlerId = DataRegistry.Register(new DataMeta
    {
        Key = "Ability.FeatureHandlerId",
        DisplayName = "Feature Handler Id",
        Type = typeof(string),
        Category = AbilityCategory.Basic,
        DefaultValue = string.Empty,
        Description = "技能触发时调用的 Feature Handler Id。"
    });

    /// <summary>技能分组 Id。</summary>
    public static readonly DataMeta FeatureGroupId = DataRegistry.Register(new DataMeta
    {
        Key = "Ability.FeatureGroupId",
        DisplayName = "Feature Group Id",
        Type = typeof(string),
        Category = AbilityCategory.Basic,
        DefaultValue = string.Empty,
        Description = "技能在游戏侧 Feature 目录中的分组 Id。"
    });

    /// <summary>技能描述。</summary>
    public static readonly DataMeta Description = DataRegistry.Register(new DataMeta
    {
        Key = "Ability.Description",
        DisplayName = "Ability Description",
        Type = typeof(string),
        Category = AbilityCategory.Basic,
        DefaultValue = string.Empty,
        Description = "技能 UI 和工具使用的描述文本。"
    });

    /// <summary>技能图标路径。</summary>
    public static readonly DataMeta IconPath = DataRegistry.Register(new DataMeta
    {
        Key = "Ability.IconPath",
        DisplayName = "Ability Icon Path",
        Type = typeof(string),
        Category = AbilityCategory.Visual,
        DefaultValue = string.Empty,
        Description = "技能图标使用的 res:// 资源路径。"
    });

    /// <summary>当前等级。</summary>
    public static readonly DataMeta Level = DataRegistry.Register(new DataMeta
    {
        Key = "Ability.Level",
        DisplayName = "Ability Level",
        Type = typeof(int),
        Category = AbilityCategory.Basic,
        DefaultValue = 1,
        MinValue = 0f,
        Description = "技能当前等级。"
    });

    /// <summary>最大等级。</summary>
    public static readonly DataMeta MaxLevel = DataRegistry.Register(new DataMeta
    {
        Key = "Ability.MaxLevel",
        DisplayName = "Ability Max Level",
        Type = typeof(int),
        Category = AbilityCategory.Basic,
        DefaultValue = 1,
        MinValue = 0f,
        Description = "技能最大等级。"
    });

    /// <summary>消耗类型。</summary>
    public static readonly DataMeta CostType = DataRegistry.Register(new DataMeta
    {
        Key = "Ability.CostType",
        DisplayName = "Ability Cost Type",
        Type = typeof(string),
        Category = AbilityCategory.Cooldown,
        DefaultValue = string.Empty,
        Description = "技能消耗资源类型。"
    });

    /// <summary>消耗数值。</summary>
    public static readonly DataMeta CostAmount = DataRegistry.Register(new DataMeta
    {
        Key = "Ability.CostAmount",
        DisplayName = "Ability Cost Amount",
        Type = typeof(float),
        Category = AbilityCategory.Cooldown,
        DefaultValue = 0f,
        MinValue = 0f,
        Description = "技能触发需要消耗的资源数量。"
    });

    /// <summary>充能恢复时间。</summary>
    public static readonly DataMeta ChargeTime = DataRegistry.Register(new DataMeta
    {
        Key = "Ability.ChargeTime",
        DisplayName = "Ability Charge Time",
        Type = typeof(float),
        Category = AbilityCategory.Charge,
        DefaultValue = 0f,
        MinValue = 0f,
        Description = "恢复一层充能需要的时间。"
    });

    /// <summary>施法距离。</summary>
    public static readonly DataMeta CastRange = DataRegistry.Register(new DataMeta
    {
        Key = "Ability.CastRange",
        DisplayName = "Ability Cast Range",
        Type = typeof(float),
        Category = AbilityCategory.Target,
        DefaultValue = -1f,
        MinValue = -1f,
        SupportModifiers = true,
        Description = "技能施法或自动索敌参考距离；-1 表示不限制。"
    });

    /// <summary>效果半径。</summary>
    public static readonly DataMeta EffectRadius = DataRegistry.Register(new DataMeta
    {
        Key = "Ability.EffectRadius",
        DisplayName = "Ability Effect Radius",
        Type = typeof(float),
        Category = AbilityCategory.Effect,
        DefaultValue = 0f,
        MinValue = 0f,
        SupportModifiers = true,
        Description = "技能命中、AOE 或表现使用的效果半径。"
    });

    /// <summary>链式弹跳次数。</summary>
    public static readonly DataMeta ChainCount = DataRegistry.Register(new DataMeta
    {
        Key = "Ability.ChainCount",
        DisplayName = "Ability Chain Count",
        Type = typeof(int),
        Category = AbilityCategory.Chain,
        DefaultValue = 0,
        MinValue = 0f,
        Description = "链式技能最多弹跳次数。"
    });

    /// <summary>链式弹跳范围。</summary>
    public static readonly DataMeta ChainRange = DataRegistry.Register(new DataMeta
    {
        Key = "Ability.ChainRange",
        DisplayName = "Ability Chain Range",
        Type = typeof(float),
        Category = AbilityCategory.Chain,
        DefaultValue = 0f,
        MinValue = 0f,
        SupportModifiers = true,
        Description = "链式技能每次弹跳查找目标的半径。"
    });

    /// <summary>链式弹跳延迟。</summary>
    public static readonly DataMeta ChainDelay = DataRegistry.Register(new DataMeta
    {
        Key = "Ability.ChainDelay",
        DisplayName = "Ability Chain Delay",
        Type = typeof(float),
        Category = AbilityCategory.Chain,
        DefaultValue = 0f,
        MinValue = 0f,
        Description = "链式技能每次弹跳之间的延迟秒数。"
    });

    /// <summary>链式伤害衰减，0-100。</summary>
    public static readonly DataMeta ChainDamageDecay = DataRegistry.Register(new DataMeta
    {
        Key = "Ability.ChainDamageDecay",
        DisplayName = "Ability Chain Damage Decay",
        Type = typeof(float),
        Category = AbilityCategory.Chain,
        DefaultValue = 100f,
        MinValue = 0f,
        MaxValue = 100f,
        IsPercentage = true,
        SupportModifiers = true,
        Description = "链式技能每次弹跳后的伤害保留百分比。"
    });

    /// <summary>链式连线效果场景路径。</summary>
    public static readonly DataMeta LineEffectScenePath = DataRegistry.Register(new DataMeta
    {
        Key = "Ability.LineEffectScenePath",
        DisplayName = "Ability Line Effect Scene Path",
        Type = typeof(string),
        Category = AbilityCategory.Visual,
        DefaultValue = string.Empty,
        Description = "链式技能连线特效使用的 res:// 场景路径。"
    });

    /// <summary>是否启用。</summary>
    public static readonly DataMeta IsEnabled = DataRegistry.Register(new DataMeta
    {
        Key = "Ability.IsEnabled",
        DisplayName = "Is Enabled",
        Type = typeof(bool),
        Category = AbilityCategory.Runtime,
        DefaultValue = true,
        Description = "技能是否可触发。"
    });

    /// <summary>是否执行中。</summary>
    public static readonly DataMeta IsActive = DataRegistry.Register(new DataMeta
    {
        Key = "Ability.IsActive",
        DisplayName = "Is Active",
        Type = typeof(bool),
        Category = AbilityCategory.Runtime,
        DefaultValue = false,
        Description = "技能当前是否处于执行中。"
    });

    /// <summary>冷却时间。</summary>
    public static readonly DataMeta Cooldown = DataRegistry.Register(new DataMeta
    {
        Key = "Ability.Cooldown",
        DisplayName = "Cooldown",
        Type = typeof(float),
        Category = AbilityCategory.Cooldown,
        DefaultValue = 0f,
        MinValue = 0f,
        SupportModifiers = true,
        Description = "技能触发后的冷却秒数。"
    });

    /// <summary>剩余冷却时间。</summary>
    public static readonly DataMeta CooldownRemaining = DataRegistry.Register(new DataMeta
    {
        Key = "Ability.CooldownRemaining",
        DisplayName = "Cooldown Remaining",
        Type = typeof(float),
        Category = AbilityCategory.Runtime,
        DefaultValue = 0f,
        MinValue = 0f,
        Description = "技能当前剩余冷却秒数。"
    });

    /// <summary>是否使用充能。</summary>
    public static readonly DataMeta UsesCharges = DataRegistry.Register(new DataMeta
    {
        Key = "Ability.UsesCharges",
        DisplayName = "Uses Charges",
        Type = typeof(bool),
        Category = AbilityCategory.Charge,
        DefaultValue = false,
        Description = "技能触发是否消耗充能。"
    });

    /// <summary>最大充能。</summary>
    public static readonly DataMeta MaxCharges = DataRegistry.Register(new DataMeta
    {
        Key = "Ability.MaxCharges",
        DisplayName = "Max Charges",
        Type = typeof(int),
        Category = AbilityCategory.Charge,
        DefaultValue = 0,
        MinValue = 0f,
        Description = "技能最大充能数。"
    });

    /// <summary>当前充能。</summary>
    public static readonly DataMeta CurrentCharges = DataRegistry.Register(new DataMeta
    {
        Key = "Ability.CurrentCharges",
        DisplayName = "Current Charges",
        Type = typeof(int),
        Category = AbilityCategory.Runtime,
        DefaultValue = 0,
        MinValue = 0f,
        Description = "技能当前充能数。"
    });

    /// <summary>技能伤害。</summary>
    public static readonly DataMeta Damage = DataRegistry.Register(new DataMeta
    {
        Key = "Ability.Damage",
        DisplayName = "Ability Damage",
        Type = typeof(float),
        Category = AbilityCategory.Effect,
        DefaultValue = 0f,
        MinValue = 0f,
        SupportModifiers = true,
        Description = "技能单次基础伤害。"
    });

    /// <summary>伤害间隔。</summary>
    public static readonly DataMeta DamageInterval = DataRegistry.Register(new DataMeta
    {
        Key = "Ability.DamageInterval",
        DisplayName = "Damage Interval",
        Type = typeof(float),
        Category = AbilityCategory.Effect,
        DefaultValue = 0f,
        MinValue = 0f,
        SupportModifiers = true,
        Description = "持续伤害两次结算之间的间隔秒数。"
    });

    /// <summary>伤害重复次数，-1 表示无限。</summary>
    public static readonly DataMeta DamageRepeatCount = DataRegistry.Register(new DataMeta
    {
        Key = "Ability.DamageRepeatCount",
        DisplayName = "Damage Repeat Count",
        Type = typeof(int),
        Category = AbilityCategory.Effect,
        DefaultValue = 1,
        MinValue = -1f,
        Description = "单次技能触发内的总伤害次数。"
    });

    /// <summary>是否立即造成第一次伤害。</summary>
    public static readonly DataMeta ApplyImmediateDamage = DataRegistry.Register(new DataMeta
    {
        Key = "Ability.ApplyImmediateDamage",
        DisplayName = "Apply Immediate Damage",
        Type = typeof(bool),
        Category = AbilityCategory.Effect,
        DefaultValue = true,
        Description = "持续伤害是否在触发时立即造成第一次伤害。"
    });

    /// <summary>
    /// 显式触发静态 DataKey 注册。
    /// </summary>
    public static void RegisterAll()
    {
        _ = Name;
        _ = Type;
        _ = TriggerMode;
        _ = TargetSelection;
        _ = AutoTargetRange;
        _ = AutoTargetMaxTargets;
        _ = AutoTargetIgnoreSameTeam;
        _ = AutoTargetRequiresDamageable;
        _ = FeatureHandlerId;
        _ = FeatureGroupId;
        _ = Description;
        _ = IconPath;
        _ = Level;
        _ = MaxLevel;
        _ = CostType;
        _ = CostAmount;
        _ = ChargeTime;
        _ = CastRange;
        _ = EffectRadius;
        _ = ChainCount;
        _ = ChainRange;
        _ = ChainDelay;
        _ = ChainDamageDecay;
        _ = LineEffectScenePath;
        _ = IsEnabled;
        _ = IsActive;
        _ = Cooldown;
        _ = CooldownRemaining;
        _ = UsesCharges;
        _ = MaxCharges;
        _ = CurrentCharges;
        _ = Damage;
        _ = DamageInterval;
        _ = DamageRepeatCount;
        _ = ApplyImmediateDamage;
    }
}
