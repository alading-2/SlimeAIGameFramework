namespace SkilmeAI.GameOS.Capabilities.Collision;

/// <summary>
/// GameOS 2D 碰撞层常量。
/// </summary>
public static class CollisionLayers
{
    /// <summary>地形层。</summary>
    public const uint Terrain = 1u << 0;

    /// <summary>玩家实体层。</summary>
    public const uint Player = 1u << 1;

    /// <summary>敌人实体层。</summary>
    public const uint Enemy = 1u << 2;

    /// <summary>玩家受击区层。</summary>
    public const uint PlayerHurtbox = 1u << 3;

    /// <summary>玩家拾取检测层。</summary>
    public const uint PlayerPickup = 1u << 4;

    /// <summary>投射物层。</summary>
    public const uint Projectile = 1u << 5;

    /// <summary>敌人受击区层。</summary>
    public const uint EnemyHurtbox = 1u << 6;

    /// <summary>武器命中体层。</summary>
    public const uint WeaponHitbox = 1u << 7;

    /// <summary>选择拾取检测层。</summary>
    public const uint SelectionPickable = 1u << 8;

    /// <summary>全部碰撞层。</summary>
    public const uint All = uint.MaxValue;
}
