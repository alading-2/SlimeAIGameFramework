using System.Collections.Generic;
using SkilmeAI.GameOS.Runtime.Data;

namespace SkilmeAI.GameOS.Capabilities.Unit;

/// <summary>
/// Unit 表现层使用的运行时 DataKey。
/// </summary>
public static class UnitDataKeys
{
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
        _ = AvailableAnimations;
    }
}
