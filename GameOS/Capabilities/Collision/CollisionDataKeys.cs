using SkilmeAI.GameOS.Runtime.Data;

namespace SkilmeAI.GameOS.Capabilities.Collision;

/// <summary>
/// Collision Capability 使用的运行时 DataKey。
/// </summary>
public static class CollisionDataKeys
{
    /// <summary>实体所属碰撞层。</summary>
    public static readonly DataMeta CollisionLayer = DataRegistry.Register(new DataMeta
    {
        Key = "Collision.Layer",
        DisplayName = "Collision Layer",
        Type = typeof(uint),
        Category = CollisionCategory.Config,
        DefaultValue = 0u,
        Description = "实体自身碰撞身份层。"
    });

    /// <summary>实体感兴趣的碰撞层。</summary>
    public static readonly DataMeta CollisionMask = DataRegistry.Register(new DataMeta
    {
        Key = "Collision.Mask",
        DisplayName = "Collision Mask",
        Type = typeof(uint),
        Category = CollisionCategory.Config,
        DefaultValue = 0u,
        Description = "实体碰撞查询或事件过滤使用的目标层掩码。"
    });

    /// <summary>实体队伍标识。</summary>
    public static readonly DataMeta Team = DataRegistry.Register(new DataMeta
    {
        Key = "Collision.Team",
        DisplayName = "Team",
        Type = typeof(int),
        Category = CollisionCategory.Config,
        DefaultValue = 0,
        Description = "用于碰撞过滤和后续伤害敌对判断的队伍标识。"
    });

    /// <summary>纯运行时碰撞扫描使用的圆形半径，单位像素。</summary>
    public static readonly DataMeta CollisionRadius = DataRegistry.Register(new DataMeta
    {
        Key = "Collision.Radius",
        DisplayName = "Collision Radius",
        Type = typeof(float),
        Category = CollisionCategory.Config,
        DefaultValue = 0f,
        MinValue = 0f,
        Description = "Movement 纯运行时碰撞扫描使用的圆形碰撞半径。"
    });

    /// <summary>
    /// 显式触发静态 DataKey 注册。
    /// </summary>
    public static void RegisterAll()
    {
        _ = CollisionLayer;
        _ = CollisionMask;
        _ = Team;
        _ = CollisionRadius;
    }
}
