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
        _ = IsEnabled;
        _ = IsActive;
        _ = ActivationCount;
    }
}
