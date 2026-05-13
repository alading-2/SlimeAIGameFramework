using System.Collections.Generic;
using SkilmeAI.GameOS.Runtime.Data;

namespace SkilmeAI.GameOS.Capabilities.Ability;

/// <summary>
/// Ability Capability 使用的运行时 DataKey。
/// </summary>
public static class AbilityDataKeys
{
    /// <summary>技能名称。</summary>
    public static readonly DataKey<string> Name = DataKey.Create<string>("Ability.Name",
        defaultValue: string.Empty,
        category: AbilityCategory.Basic);

    /// <summary>技能类型。</summary>
    public static readonly DataKey<AbilityType> Type = DataKey.Create<AbilityType>("Ability.Type",
        defaultValue: AbilityType.Passive,
        category: AbilityCategory.Basic);

    /// <summary>触发模式。</summary>
    public static readonly DataKey<AbilityTriggerMode> TriggerMode = DataKey.Create<AbilityTriggerMode>("Ability.TriggerMode",
        defaultValue: AbilityTriggerMode.None,
        category: AbilityCategory.Basic);

    /// <summary>目标选择方式。</summary>
    public static readonly DataKey<AbilityTargetSelection> TargetSelection = DataKey.Create<AbilityTargetSelection>("Ability.TargetSelection",
        defaultValue: AbilityTargetSelection.None,
        category: AbilityCategory.Target);

    /// <summary>自动索敌半径，-1 表示不限制距离。</summary>
    public static readonly DataKey<float> AutoTargetRange = DataKey.Create<float>("Ability.AutoTargetRange",
        defaultValue: -1f,
        category: AbilityCategory.Target,
        minValue: -1f,
        supportsModifiers: true);

    /// <summary>自动索敌最大目标数，-1 表示不限制数量。</summary>
    public static readonly DataKey<int> AutoTargetMaxTargets = DataKey.Create<int>("Ability.AutoTargetMaxTargets",
        defaultValue: 1,
        category: AbilityCategory.Target,
        minValue: -1f);

    /// <summary>自动索敌是否过滤同队目标。</summary>
    public static readonly DataKey<bool> AutoTargetIgnoreSameTeam = DataKey.Create<bool>("Ability.AutoTargetIgnoreSameTeam",
        defaultValue: true,
        category: AbilityCategory.Target);

    /// <summary>自动索敌是否只选择具备 HP Data 的实体。</summary>
    public static readonly DataKey<bool> AutoTargetRequiresDamageable = DataKey.Create<bool>("Ability.AutoTargetRequiresDamageable",
        defaultValue: true,
        category: AbilityCategory.Target);

    /// <summary>Ability 对应的 Feature Handler Id。</summary>
    public static readonly DataKey<string> FeatureHandlerId = DataKey.Create<string>("Ability.FeatureHandlerId",
        defaultValue: string.Empty,
        category: AbilityCategory.Basic);

    /// <summary>技能分组 Id。</summary>
    public static readonly DataKey<string> FeatureGroupId = DataKey.Create<string>("Ability.FeatureGroupId",
        defaultValue: string.Empty,
        category: AbilityCategory.Basic);

    /// <summary>技能描述。</summary>
    public static readonly DataKey<string> Description = DataKey.Create<string>("Ability.Description",
        defaultValue: string.Empty,
        category: AbilityCategory.Basic);

    /// <summary>技能图标路径。</summary>
    public static readonly DataKey<string> IconPath = DataKey.Create<string>("Ability.IconPath",
        defaultValue: string.Empty,
        category: AbilityCategory.Visual);

    /// <summary>当前等级。</summary>
    public static readonly DataKey<int> Level = DataKey.Create<int>("Ability.Level",
        defaultValue: 1,
        category: AbilityCategory.Basic,
        minValue: 0f);

    /// <summary>最大等级。</summary>
    public static readonly DataKey<int> MaxLevel = DataKey.Create<int>("Ability.MaxLevel",
        defaultValue: 1,
        category: AbilityCategory.Basic,
        minValue: 0f);

    /// <summary>消耗类型。</summary>
    public static readonly DataKey<string> CostType = DataKey.Create<string>("Ability.CostType",
        defaultValue: string.Empty,
        category: AbilityCategory.Cooldown);

    /// <summary>消耗数值。</summary>
    public static readonly DataKey<float> CostAmount = DataKey.Create<float>("Ability.CostAmount",
        defaultValue: 0f,
        category: AbilityCategory.Cooldown,
        minValue: 0f);

    /// <summary>充能恢复时间。</summary>
    public static readonly DataKey<float> ChargeTime = DataKey.Create<float>("Ability.ChargeTime",
        defaultValue: 0f,
        category: AbilityCategory.Charge,
        minValue: 0f);

    /// <summary>施法距离。</summary>
    public static readonly DataKey<float> CastRange = DataKey.Create<float>("Ability.CastRange",
        defaultValue: -1f,
        category: AbilityCategory.Target,
        minValue: -1f,
        supportsModifiers: true);

    /// <summary>效果半径。</summary>
    public static readonly DataKey<float> EffectRadius = DataKey.Create<float>("Ability.EffectRadius",
        defaultValue: 0f,
        category: AbilityCategory.Effect,
        minValue: 0f,
        supportsModifiers: true);

    /// <summary>链式弹跳次数。</summary>
    public static readonly DataKey<int> ChainCount = DataKey.Create<int>("Ability.ChainCount",
        defaultValue: 0,
        category: AbilityCategory.Chain,
        minValue: 0f);

    /// <summary>链式弹跳范围。</summary>
    public static readonly DataKey<float> ChainRange = DataKey.Create<float>("Ability.ChainRange",
        defaultValue: 0f,
        category: AbilityCategory.Chain,
        minValue: 0f,
        supportsModifiers: true);

    /// <summary>链式弹跳延迟。</summary>
    public static readonly DataKey<float> ChainDelay = DataKey.Create<float>("Ability.ChainDelay",
        defaultValue: 0f,
        category: AbilityCategory.Chain,
        minValue: 0f);

    /// <summary>链式伤害衰减，0-100。</summary>
    public static readonly DataKey<float> ChainDamageDecay = DataKey.Create<float>("Ability.ChainDamageDecay",
        defaultValue: 100f,
        category: AbilityCategory.Chain,
        minValue: 0f,
        maxValue: 100f,
        isPercentage: true,
        supportsModifiers: true);

    /// <summary>链式连线效果场景路径。</summary>
    public static readonly DataKey<string> LineEffectScenePath = DataKey.Create<string>("Ability.LineEffectScenePath",
        defaultValue: string.Empty,
        category: AbilityCategory.Visual);

    /// <summary>是否启用。</summary>
    public static readonly DataKey<bool> IsEnabled = DataKey.Create<bool>("Ability.IsEnabled",
        defaultValue: true,
        category: AbilityCategory.Runtime);

    /// <summary>是否执行中。</summary>
    public static readonly DataKey<bool> IsActive = DataKey.Create<bool>("Ability.IsActive",
        defaultValue: false,
        category: AbilityCategory.Runtime);

    /// <summary>冷却时间。</summary>
    public static readonly DataKey<float> Cooldown = DataKey.Create<float>("Ability.Cooldown",
        defaultValue: 0f,
        category: AbilityCategory.Cooldown,
        minValue: 0f,
        supportsModifiers: true);

    /// <summary>剩余冷却时间。</summary>
    public static readonly DataKey<float> CooldownRemaining = DataKey.Create<float>("Ability.CooldownRemaining",
        defaultValue: 0f,
        category: AbilityCategory.Runtime,
        minValue: 0f);

    /// <summary>是否使用充能。</summary>
    public static readonly DataKey<bool> UsesCharges = DataKey.Create<bool>("Ability.UsesCharges",
        defaultValue: false,
        category: AbilityCategory.Charge);

    /// <summary>最大充能。</summary>
    public static readonly DataKey<int> MaxCharges = DataKey.Create<int>("Ability.MaxCharges",
        defaultValue: 0,
        category: AbilityCategory.Charge,
        minValue: 0f);

    /// <summary>当前充能。</summary>
    public static readonly DataKey<int> CurrentCharges = DataKey.Create<int>("Ability.CurrentCharges",
        defaultValue: 0,
        category: AbilityCategory.Runtime,
        minValue: 0f);

    /// <summary>技能伤害。</summary>
    public static readonly DataKey<float> Damage = DataKey.Create<float>("Ability.Damage",
        defaultValue: 0f,
        category: AbilityCategory.Effect,
        minValue: 0f,
        supportsModifiers: true);

    /// <summary>伤害间隔。</summary>
    public static readonly DataKey<float> DamageInterval = DataKey.Create<float>("Ability.DamageInterval",
        defaultValue: 0f,
        category: AbilityCategory.Effect,
        minValue: 0f,
        supportsModifiers: true);

    /// <summary>伤害重复次数，-1 表示无限。</summary>
    public static readonly DataKey<int> DamageRepeatCount = DataKey.Create<int>("Ability.DamageRepeatCount",
        defaultValue: 1,
        category: AbilityCategory.Effect,
        minValue: -1f);

    /// <summary>是否立即造成第一次伤害。</summary>
    public static readonly DataKey<bool> ApplyImmediateDamage = DataKey.Create<bool>("Ability.ApplyImmediateDamage",
        defaultValue: true,
        category: AbilityCategory.Effect);

    /// <summary>当前选中的主动技能索引。</summary>
    public static readonly DataKey<int> CurrentAbilityIndex = DataKey.Create<int>("Ability.CurrentAbilityIndex",
        defaultValue: 0,
        category: AbilityCategory.Runtime,
        minValue: 0f);

    /// <summary>拥有的技能 EntityId 列表。</summary>
    public static readonly DataKey<List<string>> OwnedAbilityIds = DataKey.Create<List<string>>("Ability.OwnedAbilityIds",
        defaultValue: new List<string>(),
        category: AbilityCategory.Runtime);

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
        _ = CurrentAbilityIndex;
        _ = OwnedAbilityIds;
    }
}
