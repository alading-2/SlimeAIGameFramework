using SlimeAI.GameOS.Runtime.Data;

namespace SlimeAI.GameOS.Capabilities.Feature;

/// <summary>
/// Feature Capability 使用的运行时 DataKey。
/// </summary>
public static class FeatureDataKeys
{
    /// <summary>Feature 稳定 Id。</summary>
    public static readonly DataKey<string> FeatureId = DataKey.Create<string>("Feature.Id",
        defaultValue: string.Empty,
        category: FeatureCategory.Basic);

    /// <summary>Feature Handler Id。</summary>
    public static readonly DataKey<string> HandlerId = DataKey.Create<string>("Feature.HandlerId",
        defaultValue: string.Empty,
        category: FeatureCategory.Basic);

    /// <summary>Feature 描述。</summary>
    public static readonly DataKey<string> Description = DataKey.Create<string>("Feature.Description",
        defaultValue: string.Empty,
        category: FeatureCategory.Basic);

    /// <summary>Feature 分类。</summary>
    public static readonly DataKey<string> Category = DataKey.Create<string>("Feature.Category",
        defaultValue: string.Empty,
        category: FeatureCategory.Basic);

    /// <summary>Feature 触发模式。</summary>
    public static readonly DataKey<FeatureTriggerMode> TriggerMode = DataKey.Create<FeatureTriggerMode>("Feature.TriggerMode",
        defaultValue: FeatureTriggerMode.None,
        category: FeatureCategory.Basic);

    /// <summary>Feature 周期触发间隔 / 冷却时间。</summary>
    public static readonly DataKey<float> Cooldown = DataKey.Create<float>("Feature.Cooldown",
        defaultValue: 1f,
        category: FeatureCategory.Basic,
        minValue: 0.01f);

    /// <summary>OnEvent 触发模式监听的事件类型 stable key。</summary>
    public static readonly DataKey<string> TriggerEventType = DataKey.Create<string>("Feature.TriggerEventType",
        defaultValue: string.Empty,
        category: FeatureCategory.Basic);

    /// <summary>OnEvent 触发概率，0-100。</summary>
    public static readonly DataKey<float> TriggerChance = DataKey.Create<float>("Feature.TriggerChance",
        defaultValue: 100f,
        category: FeatureCategory.Basic,
        minValue: 0f,
        maxValue: 100f,
        isPercentage: true);

    /// <summary>修改器目标 DataKey。</summary>
    public static readonly DataKey<string> ModifierTargetKey = DataKey.Create<string>("Feature.Modifier.TargetKey",
        defaultValue: string.Empty,
        category: FeatureCategory.Modifier);

    /// <summary>修改器类型。</summary>
    public static readonly DataKey<string> ModifierType = DataKey.Create<string>("Feature.Modifier.Type",
        defaultValue: string.Empty,
        category: FeatureCategory.Modifier);

    /// <summary>修改器数值。</summary>
    public static readonly DataKey<float> ModifierValue = DataKey.Create<float>("Feature.Modifier.Value",
        defaultValue: 0f,
        category: FeatureCategory.Modifier);

    /// <summary>修改器优先级。</summary>
    public static readonly DataKey<int> ModifierPriority = DataKey.Create<int>("Feature.Modifier.Priority",
        defaultValue: 0,
        category: FeatureCategory.Modifier);

    /// <summary>是否启用。</summary>
    public static readonly DataKey<bool> IsEnabled = DataKey.Create<bool>("Feature.IsEnabled",
        defaultValue: true,
        category: FeatureCategory.Runtime);

    /// <summary>是否激活中。</summary>
    public static readonly DataKey<bool> IsActive = DataKey.Create<bool>("Feature.IsActive",
        defaultValue: false,
        category: FeatureCategory.Runtime);

    /// <summary>激活次数。</summary>
    public static readonly DataKey<int> ActivationCount = DataKey.Create<int>("Feature.ActivationCount",
        defaultValue: 0,
        category: FeatureCategory.Runtime,
        minValue: 0f);

    /// <summary>
    /// 显式触发静态 DataKey 注册。
    /// </summary>
    public static void RegisterAll()
    {
        _ = FeatureId;
        _ = HandlerId;
        _ = Description;
        _ = Category;
        _ = TriggerMode;
        _ = Cooldown;
        _ = TriggerEventType;
        _ = TriggerChance;
        _ = ModifierTargetKey;
        _ = ModifierType;
        _ = ModifierValue;
        _ = ModifierPriority;
        _ = IsEnabled;
        _ = IsActive;
        _ = ActivationCount;
    }
}
