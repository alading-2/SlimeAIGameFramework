using System.Collections.Generic;
using SkilmeAI.GameOS.Runtime.Data;

namespace SkilmeAI.GameOS.Capabilities.Unit;

/// <summary>
/// Unit 表现层使用的运行时 DataKey。
/// </summary>
public static class UnitDataKeys
{
    /// <summary>单位显示名。</summary>
    public static readonly DataMeta Name = DataRegistry.Register(new DataMeta
    {
        Key = "Unit.Name",
        DisplayName = "Unit Name",
        Type = typeof(string),
        Category = UnitCategory.Basic,
        DefaultValue = string.Empty,
        Description = "单位显示名称。"
    });

    /// <summary>单位实体类型标记。</summary>
    public static readonly DataMeta EntityType = DataRegistry.Register(new DataMeta
    {
        Key = "Unit.EntityType",
        DisplayName = "Unit Entity Type",
        Type = typeof(string),
        Category = UnitCategory.Basic,
        DefaultValue = "Unit",
        Description = "单位在游戏数据中的实体类型标记。"
    });

    /// <summary>死亡处理类型。</summary>
    public static readonly DataMeta DeathType = DataRegistry.Register(new DataMeta
    {
        Key = "Unit.DeathType",
        DisplayName = "Death Type",
        Type = typeof(string),
        Category = UnitCategory.Basic,
        DefaultValue = string.Empty,
        Description = "单位死亡时的游戏侧处理语义。"
    });

    /// <summary>视觉场景路径。</summary>
    public static readonly DataMeta VisualScenePath = DataRegistry.Register(new DataMeta
    {
        Key = "Unit.VisualScenePath",
        DisplayName = "Visual Scene Path",
        Type = typeof(string),
        Category = UnitCategory.Visual,
        DefaultValue = string.Empty,
        Description = "单位视觉表现使用的 res:// 场景路径。"
    });

    /// <summary>血条显示高度。</summary>
    public static readonly DataMeta HealthBarHeight = DataRegistry.Register(new DataMeta
    {
        Key = "Unit.HealthBarHeight",
        DisplayName = "Health Bar Height",
        Type = typeof(float),
        Category = UnitCategory.Visual,
        DefaultValue = 0f,
        Description = "单位血条相对根节点的显示高度。"
    });

    /// <summary>是否显示血条。</summary>
    public static readonly DataMeta IsShowHealthBar = DataRegistry.Register(new DataMeta
    {
        Key = "Unit.IsShowHealthBar",
        DisplayName = "Show Health Bar",
        Type = typeof(bool),
        Category = UnitCategory.Visual,
        DefaultValue = true,
        Description = "单位是否显示血条。"
    });

    /// <summary>拾取范围。</summary>
    public static readonly DataMeta PickupRange = DataRegistry.Register(new DataMeta
    {
        Key = "Unit.PickupRange",
        DisplayName = "Pickup Range",
        Type = typeof(float),
        Category = UnitCategory.Basic,
        DefaultValue = 0f,
        MinValue = 0f,
        SupportModifiers = true,
        Description = "玩家或单位拾取物品的半径。"
    });

    /// <summary>击杀经验奖励。</summary>
    public static readonly DataMeta ExpReward = DataRegistry.Register(new DataMeta
    {
        Key = "Unit.ExpReward",
        DisplayName = "Experience Reward",
        Type = typeof(int),
        Category = UnitCategory.Basic,
        DefaultValue = 0,
        MinValue = 0f,
        Description = "击杀该单位后给予的经验值。"
    });

    /// <summary>AI 检测范围。</summary>
    public static readonly DataMeta DetectionRange = DataRegistry.Register(new DataMeta
    {
        Key = "Unit.DetectionRange",
        DisplayName = "Detection Range",
        Type = typeof(float),
        Category = UnitCategory.Basic,
        DefaultValue = -1f,
        MinValue = -1f,
        SupportModifiers = true,
        Description = "单位 AI 或感知系统使用的检测范围；-1 表示不限制。"
    });

    /// <summary>当前 AnimatedSprite2D 上可用的动画名列表。</summary>
    public static readonly DataMeta AvailableAnimations = DataRegistry.Register(new DataMeta
    {
        Key = "Unit.AvailableAnimations",
        DisplayName = "Unit Available Animations",
        Type = typeof(List<string>),
        Category = UnitCategory.Runtime,
        DefaultValue = new List<string>(),
        Description = "Godot 动画桥缓存的可播放动画名列表。"
    });

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
