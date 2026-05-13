using SlimeAI.GameOS.Runtime.Data;

namespace SlimeAI.GameOS.Capabilities.Feature;

/// <summary>
/// Feature 授予时应用的 Data 修改器配置。
/// </summary>
public readonly record struct FeatureModifierEntry(
    IDataKey DataKey,
    ModifierType ModifierType,
    float Value,
    int Priority = 0);
