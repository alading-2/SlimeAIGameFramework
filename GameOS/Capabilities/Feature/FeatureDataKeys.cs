using SkilmeAI.GameOS.Runtime.Data;

namespace SkilmeAI.GameOS.Capabilities.Feature;

/// <summary>
/// Feature Capability 使用的运行时 DataKey。
/// </summary>
public static class FeatureDataKeys
{
    /// <summary>Feature 稳定 Id。</summary>
    public static readonly DataMeta FeatureId = DataRegistry.Register(new DataMeta
    {
        Key = "Feature.Id",
        DisplayName = "Feature Id",
        Type = typeof(string),
        Category = FeatureCategory.Basic,
        DefaultValue = string.Empty,
        Description = "Feature 的稳定唯一 Id。"
    });

    /// <summary>Feature Handler Id。</summary>
    public static readonly DataMeta HandlerId = DataRegistry.Register(new DataMeta
    {
        Key = "Feature.HandlerId",
        DisplayName = "Feature Handler Id",
        Type = typeof(string),
        Category = FeatureCategory.Basic,
        DefaultValue = string.Empty,
        Description = "运行时处理器注册 Id。"
    });

    /// <summary>Feature 描述。</summary>
    public static readonly DataMeta Description = DataRegistry.Register(new DataMeta
    {
        Key = "Feature.Description",
        DisplayName = "Feature Description",
        Type = typeof(string),
        Category = FeatureCategory.Basic,
        DefaultValue = string.Empty,
        Description = "Feature UI 和工具使用的描述文本。"
    });

    /// <summary>Feature 分类。</summary>
    public static readonly DataMeta Category = DataRegistry.Register(new DataMeta
    {
        Key = "Feature.Category",
        DisplayName = "Feature Category",
        Type = typeof(string),
        Category = FeatureCategory.Basic,
        DefaultValue = string.Empty,
        Description = "Feature 在游戏侧 authoring 中的分类。"
    });

    /// <summary>修改器目标 DataKey。</summary>
    public static readonly DataMeta ModifierTargetKey = DataRegistry.Register(new DataMeta
    {
        Key = "Feature.Modifier.TargetKey",
        DisplayName = "Modifier Target Key",
        Type = typeof(string),
        Category = FeatureCategory.Modifier,
        DefaultValue = string.Empty,
        Description = "Feature 修改器作用的 Runtime DataKey。"
    });

    /// <summary>修改器类型。</summary>
    public static readonly DataMeta ModifierType = DataRegistry.Register(new DataMeta
    {
        Key = "Feature.Modifier.Type",
        DisplayName = "Modifier Type",
        Type = typeof(string),
        Category = FeatureCategory.Modifier,
        DefaultValue = string.Empty,
        Description = "Feature 修改器类型。"
    });

    /// <summary>修改器数值。</summary>
    public static readonly DataMeta ModifierValue = DataRegistry.Register(new DataMeta
    {
        Key = "Feature.Modifier.Value",
        DisplayName = "Modifier Value",
        Type = typeof(float),
        Category = FeatureCategory.Modifier,
        DefaultValue = 0f,
        Description = "Feature 修改器数值。"
    });

    /// <summary>修改器优先级。</summary>
    public static readonly DataMeta ModifierPriority = DataRegistry.Register(new DataMeta
    {
        Key = "Feature.Modifier.Priority",
        DisplayName = "Modifier Priority",
        Type = typeof(int),
        Category = FeatureCategory.Modifier,
        DefaultValue = 0,
        Description = "Feature 修改器计算优先级。"
    });

    /// <summary>是否启用。</summary>
    public static readonly DataMeta IsEnabled = DataRegistry.Register(new DataMeta
    {
        Key = "Feature.IsEnabled",
        DisplayName = "Is Enabled",
        Type = typeof(bool),
        Category = FeatureCategory.Runtime,
        DefaultValue = true,
        Description = "Feature 是否启用。"
    });

    /// <summary>是否激活中。</summary>
    public static readonly DataMeta IsActive = DataRegistry.Register(new DataMeta
    {
        Key = "Feature.IsActive",
        DisplayName = "Is Active",
        Type = typeof(bool),
        Category = FeatureCategory.Runtime,
        DefaultValue = false,
        Description = "Feature 当前是否处于单次运行中。"
    });

    /// <summary>激活次数。</summary>
    public static readonly DataMeta ActivationCount = DataRegistry.Register(new DataMeta
    {
        Key = "Feature.ActivationCount",
        DisplayName = "Activation Count",
        Type = typeof(int),
        Category = FeatureCategory.Runtime,
        DefaultValue = 0,
        MinValue = 0f,
        Description = "Feature 累计激活次数。"
    });

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
