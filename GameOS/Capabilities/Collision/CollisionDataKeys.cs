using SkilmeAI.GameOS.Runtime.Data;

namespace SkilmeAI.GameOS.Capabilities.Collision;

/// <summary>
/// Collision Capability 使用的运行时 DataKey。
/// </summary>
public static class CollisionDataKeys
{
    /// <summary>实体所属碰撞层。</summary>
    public static readonly DataKey<uint> CollisionLayer = DataKey.Create<uint>("Collision.Layer",
        defaultValue: 0u,
        category: CollisionCategory.Config);

    /// <summary>实体感兴趣的碰撞层。</summary>
    public static readonly DataKey<uint> CollisionMask = DataKey.Create<uint>("Collision.Mask",
        defaultValue: 0u,
        category: CollisionCategory.Config);

    /// <summary>实体队伍标识。</summary>
    public static readonly DataKey<int> Team = DataKey.Create<int>("Collision.Team",
        defaultValue: 0,
        category: CollisionCategory.Config);

    /// <summary>纯运行时碰撞扫描使用的圆形半径，单位像素。</summary>
    public static readonly DataKey<float> CollisionRadius = DataKey.Create<float>("Collision.Radius",
        defaultValue: 0f,
        category: CollisionCategory.Config,
        minValue: 0f);

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
