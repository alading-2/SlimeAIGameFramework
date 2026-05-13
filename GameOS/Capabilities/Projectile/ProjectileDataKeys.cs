using SkilmeAI.GameOS.Capabilities.Damage;
using SkilmeAI.GameOS.Capabilities.Movement;
using SkilmeAI.GameOS.Runtime.Data;
using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.Capabilities.Projectile;

/// <summary>
/// Projectile Capability 使用的运行时 DataKey。
/// </summary>
public static class ProjectileDataKeys
{
    /// <summary>投射物资源路径。</summary>
    public static readonly DataKey<string> ScenePath = DataKey.Create<string>("Projectile.ScenePath",
        defaultValue: string.Empty,
        category: ProjectileCategory.Basic);

    /// <summary>发射者实体。</summary>
    public static readonly DataKey<IEntity?> SourceEntity = DataKey.Create<IEntity?>("Projectile.SourceEntity",
        category: ProjectileCategory.Runtime);

    /// <summary>关联技能实体。</summary>
    public static readonly DataKey<IEntity?> AbilityEntity = DataKey.Create<IEntity?>("Projectile.AbilityEntity",
        category: ProjectileCategory.Runtime);

    /// <summary>目标实体。</summary>
    public static readonly DataKey<IEntity?> TargetEntity = DataKey.Create<IEntity?>("Projectile.TargetEntity",
        category: ProjectileCategory.Runtime);

    /// <summary>生成位置。</summary>
    public static readonly DataKey<Vector2Value> SpawnPosition = DataKey.Create<Vector2Value>("Projectile.SpawnPosition",
        defaultValue: Vector2Value.Zero,
        category: ProjectileCategory.Runtime);

    /// <summary>目标位置。</summary>
    public static readonly DataKey<Vector2Value> TargetPosition = DataKey.Create<Vector2Value>("Projectile.TargetPosition",
        defaultValue: Vector2Value.Zero,
        category: ProjectileCategory.Runtime);

    /// <summary>发射方向。</summary>
    public static readonly DataKey<Vector2Value> Direction = DataKey.Create<Vector2Value>("Projectile.Direction",
        defaultValue: Vector2Value.Zero,
        category: ProjectileCategory.Runtime);

    /// <summary>飞行速度。</summary>
    public static readonly DataKey<float> Speed = DataKey.Create<float>("Projectile.Speed",
        defaultValue: 0f,
        category: ProjectileCategory.Basic,
        minValue: 0f,
        supportsModifiers: true);

    /// <summary>最大有效命中数，-1 表示不限制。</summary>
    public static readonly DataKey<int> MaxHitCount = DataKey.Create<int>("Projectile.MaxHitCount",
        defaultValue: 1,
        category: ProjectileCategory.Basic,
        minValue: -1f,
        supportsModifiers: true);

    /// <summary>已有效命中次数。</summary>
    public static readonly DataKey<int> HitCount = DataKey.Create<int>("Projectile.HitCount",
        defaultValue: 0,
        category: ProjectileCategory.Runtime,
        minValue: 0f);

    /// <summary>最大存活时间，-1 表示不限制。</summary>
    public static readonly DataKey<float> MaxLifeTime = DataKey.Create<float>("Projectile.MaxLifeTime",
        defaultValue: -1f,
        category: ProjectileCategory.Basic,
        minValue: -1f,
        supportsModifiers: true);

    /// <summary>命中伤害。</summary>
    public static readonly DataKey<float> Damage = DataKey.Create<float>("Projectile.Damage",
        defaultValue: 0f,
        category: ProjectileCategory.Basic,
        minValue: 0f,
        supportsModifiers: true);

    /// <summary>伤害类型。</summary>
    public static readonly DataKey<DamageType> DamageType = DataKey.Create<DamageType>("Projectile.DamageType",
        defaultValue: Capabilities.Damage.DamageType.Physical,
        category: ProjectileCategory.Basic);

    /// <summary>伤害标签。</summary>
    public static readonly DataKey<DamageTags> DamageTags = DataKey.Create<DamageTags>("Projectile.DamageTags",
        defaultValue: Capabilities.Damage.DamageTags.Projectile,
        category: ProjectileCategory.Basic);

    /// <summary>
    /// 显式触发静态 DataKey 注册。
    /// </summary>
    public static void RegisterAll()
    {
        _ = ScenePath;
        _ = SourceEntity;
        _ = AbilityEntity;
        _ = TargetEntity;
        _ = SpawnPosition;
        _ = TargetPosition;
        _ = Direction;
        _ = Speed;
        _ = MaxHitCount;
        _ = HitCount;
        _ = MaxLifeTime;
        _ = Damage;
        _ = DamageType;
        _ = DamageTags;
    }
}
