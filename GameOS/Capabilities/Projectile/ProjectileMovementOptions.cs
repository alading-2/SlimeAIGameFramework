using SlimeAI.GameOS.Capabilities.Collision;
using SlimeAI.GameOS.Capabilities.Damage;
using SlimeAI.GameOS.Capabilities.Movement;

namespace SlimeAI.GameOS.Capabilities.Projectile;

/// <summary>
/// Projectile 飞行和命中生命周期参数。
/// </summary>
public sealed class ProjectileMovementOptions
{
    internal const int UseProjectileDataMaxHitCount = int.MinValue;

    /// <summary>移动模式，默认直线 Charge。</summary>
    public MoveMode Mode { get; init; } = MoveMode.Charge;

    /// <summary>覆盖飞行速度；小于 0 时读取 ProjectileDataKeys.Speed。</summary>
    public float Speed { get; init; } = -1f;

    /// <summary>最大飞行时长；NaN 表示读取 Projectile.MaxLifeTime，-1 表示不限制。</summary>
    public float MaxDuration { get; init; } = float.NaN;

    /// <summary>最大飞行距离，-1 表示不限制。</summary>
    public float MaxDistance { get; init; } = -1f;

    /// <summary>到达目标点停止阈值。</summary>
    public float ReachDistance { get; init; } = 1f;

    /// <summary>是否到达目标点时停止。</summary>
    public bool StopAtTarget { get; init; } = false;

    /// <summary>是否启用命中伤害。</summary>
    public bool ApplyDamageOnHit { get; init; } = true;

    /// <summary>命中多少次后停止；默认读取 Projectile.MaxHitCount，-1 表示只通知和伤害，不停止。</summary>
    public int StopAfterHitCount { get; init; } = UseProjectileDataMaxHitCount;

    /// <summary>停止后是否销毁投射物。</summary>
    public bool DestroyOnStop { get; init; } = true;

    /// <summary>是否忽略同队目标。</summary>
    public bool IgnoreSameTeam { get; init; } = true;

    /// <summary>目标匹配模式。</summary>
    public MovementCollisionTargetMatchMode TargetMatchMode { get; init; } = MovementCollisionTargetMatchMode.Any;

    /// <summary>移动实体半径覆盖，-1 表示读取 CollisionDataKeys.CollisionRadius。</summary>
    public float SourceRadiusOverride { get; init; } = -1f;

    /// <summary>目标实体半径覆盖，-1 表示读取 CollisionDataKeys.CollisionRadius。</summary>
    public float TargetRadiusOverride { get; init; } = -1f;

    /// <summary>可选覆盖伤害值；小于 0 时读取 ProjectileDataKeys.Damage。</summary>
    public float Damage { get; init; } = -1f;

    /// <summary>可选覆盖伤害类型；为空时读取 ProjectileDataKeys.DamageType。</summary>
    public DamageType? DamageType { get; init; } = null;

    /// <summary>可选覆盖伤害标签；为空时读取 ProjectileDataKeys.DamageTags。</summary>
    public DamageTags? DamageTags { get; init; } = null;

    /// <summary>
    /// 转为 Movement 碰撞过滤策略。
    /// </summary>
    internal CollisionFilterPolicy ToFilterPolicy()
    {
        return new CollisionFilterPolicy(IgnoreSameTeam: IgnoreSameTeam);
    }
}
