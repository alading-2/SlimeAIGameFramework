using SlimeAI.GameOS.Capabilities.Damage;
using SlimeAI.GameOS.Capabilities.Movement;
using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.Capabilities.Projectile;

/// <summary>
/// 投射物生成参数。
/// </summary>
public sealed class ProjectileSpawnOptions
{
    /// <summary>生成者实体。</summary>
    public required IEntity Source { get; init; }

    /// <summary>关联技能实体。</summary>
    public IEntity? Ability { get; init; }

    /// <summary>目标实体。</summary>
    public IEntity? Target { get; init; }

    /// <summary>投射物实体 Id；为 <see cref="Runtime.Entity.EntityId.Empty"/> 时由 EntityManager 生成。</summary>
    public EntityId EntityId { get; init; }

    /// <summary>投射物资源路径。</summary>
    public string? ScenePath { get; init; }

    /// <summary>生成位置。</summary>
    public Vector2Value SpawnPosition { get; init; } = Vector2Value.Zero;

    /// <summary>目标位置。</summary>
    public Vector2Value? TargetPosition { get; init; }

    /// <summary>显式方向；为空时从目标位置推导。</summary>
    public Vector2Value? Direction { get; init; }

    /// <summary>飞行速度。</summary>
    public float Speed { get; init; }

    /// <summary>最大有效命中数，-1 表示不限制。</summary>
    public int MaxHitCount { get; init; } = 1;

    /// <summary>最大存活时间，-1 表示不限制。</summary>
    public float MaxLifeTime { get; init; } = -1f;

    /// <summary>命中伤害。</summary>
    public float Damage { get; init; }

    /// <summary>伤害类型。</summary>
    public DamageType DamageType { get; init; } = DamageType.Physical;

    /// <summary>伤害标签。</summary>
    public DamageTags DamageTags { get; init; } = DamageTags.Projectile;
}
