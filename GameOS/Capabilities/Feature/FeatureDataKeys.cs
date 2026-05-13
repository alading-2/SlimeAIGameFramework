using SkilmeAI.GameOS.Runtime.Data;

namespace SkilmeAI.GameOS.Capabilities.Feature;

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
        _ = ModifierTargetKey;
        _ = ModifierType;
        _ = ModifierValue;
        _ = ModifierPriority;
        _ = IsEnabled;
        _ = IsActive;
        _ = ActivationCount;
    }
}
