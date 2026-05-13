using System.Collections.Generic;
using SkilmeAI.GameOS.Runtime.Data;

namespace SkilmeAI.GameOS.Capabilities.Unit;

/// <summary>
/// Unit 表现层使用的运行时 DataKey。
/// </summary>
public static class UnitDataKeys
{
    /// <summary>单位显示名。</summary>
    public static readonly DataKey<string> Name = DataKey.Create<string>("Unit.Name",
        defaultValue: string.Empty,
        category: UnitCategory.Basic);

    /// <summary>单位实体类型标记。</summary>
    public static readonly DataKey<string> EntityType = DataKey.Create<string>("Unit.EntityType",
        defaultValue: "Unit",
        category: UnitCategory.Basic);

    /// <summary>死亡处理类型。</summary>
    public static readonly DataKey<string> DeathType = DataKey.Create<string>("Unit.DeathType",
        defaultValue: string.Empty,
        category: UnitCategory.Basic);

    /// <summary>视觉场景路径。</summary>
    public static readonly DataKey<string> VisualScenePath = DataKey.Create<string>("Unit.VisualScenePath",
        defaultValue: string.Empty,
        category: UnitCategory.Visual);

    /// <summary>血条显示高度。</summary>
    public static readonly DataKey<float> HealthBarHeight = DataKey.Create<float>("Unit.HealthBarHeight",
        defaultValue: 0f,
        category: UnitCategory.Visual);

    /// <summary>是否显示血条。</summary>
    public static readonly DataKey<bool> IsShowHealthBar = DataKey.Create<bool>("Unit.IsShowHealthBar",
        defaultValue: true,
        category: UnitCategory.Visual);

    /// <summary>拾取范围。</summary>
    public static readonly DataKey<float> PickupRange = DataKey.Create<float>("Unit.PickupRange",
        defaultValue: 0f,
        category: UnitCategory.Basic,
        minValue: 0f,
        supportsModifiers: true);

    /// <summary>击杀经验奖励。</summary>
    public static readonly DataKey<int> ExpReward = DataKey.Create<int>("Unit.ExpReward",
        defaultValue: 0,
        category: UnitCategory.Basic,
        minValue: 0f);

    /// <summary>AI 检测范围。</summary>
    public static readonly DataKey<float> DetectionRange = DataKey.Create<float>("Unit.DetectionRange",
        defaultValue: -1f,
        category: UnitCategory.Basic,
        minValue: -1f,
        supportsModifiers: true);

    /// <summary>当前 AnimatedSprite2D 上可用的动画名列表。</summary>
    public static readonly DataKey<List<string>> AvailableAnimations = DataKey.Create<List<string>>("Unit.AvailableAnimations",
        defaultValue: new List<string>(),
        category: UnitCategory.Runtime);

    /// <summary>
    /// 显式触发静态 DataKey 注册。
    /// </summary>
    public static void RegisterAll()
    {
        _ = Name;
        _ = EntityType;
        _ = DeathType;
        _ = VisualScenePath;
        _ = HealthBarHeight;
        _ = IsShowHealthBar;
        _ = PickupRange;
        _ = ExpReward;
        _ = DetectionRange;
        _ = AvailableAnimations;
    }
}
