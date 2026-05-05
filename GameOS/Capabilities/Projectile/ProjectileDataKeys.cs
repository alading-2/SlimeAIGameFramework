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
    public static readonly DataMeta ScenePath = DataRegistry.Register(new DataMeta
    {
        Key = "Projectile.ScenePath",
        DisplayName = "Projectile Scene Path",
        Type = typeof(string),
        Category = ProjectileCategory.Basic,
        DefaultValue = string.Empty,
        Description = "投射物 Godot 场景或资源路径。"
    });

    /// <summary>发射者实体。</summary>
    public static readonly DataMeta SourceEntity = DataRegistry.Register(new DataMeta
    {
        Key = "Projectile.SourceEntity",
        DisplayName = "Source Entity",
        Type = typeof(IEntity),
        Category = ProjectileCategory.Runtime,
        Description = "生成该投射物的实体。"
    });

    /// <summary>关联技能实体。</summary>
    public static readonly DataMeta AbilityEntity = DataRegistry.Register(new DataMeta
    {
        Key = "Projectile.AbilityEntity",
        DisplayName = "Ability Entity",
        Type = typeof(IEntity),
        Category = ProjectileCategory.Runtime,
        Description = "生成该投射物的技能实体。"
    });

    /// <summary>目标实体。</summary>
    public static readonly DataMeta TargetEntity = DataRegistry.Register(new DataMeta
    {
        Key = "Projectile.TargetEntity",
        DisplayName = "Target Entity",
        Type = typeof(IEntity),
        Category = ProjectileCategory.Runtime,
        Description = "投射物锁定的目标实体。"
    });

    /// <summary>生成位置。</summary>
    public static readonly DataMeta SpawnPosition = DataRegistry.Register(new DataMeta
    {
        Key = "Projectile.SpawnPosition",
        DisplayName = "Spawn Position",
        Type = typeof(Vector2Value),
        Category = ProjectileCategory.Runtime,
        DefaultValue = Vector2Value.Zero,
        Description = "投射物生成位置。"
    });

    /// <summary>目标位置。</summary>
    public static readonly DataMeta TargetPosition = DataRegistry.Register(new DataMeta
    {
        Key = "Projectile.TargetPosition",
        DisplayName = "Target Position",
        Type = typeof(Vector2Value),
        Category = ProjectileCategory.Runtime,
        DefaultValue = Vector2Value.Zero,
        Description = "投射物目标位置。"
    });

    /// <summary>发射方向。</summary>
    public static readonly DataMeta Direction = DataRegistry.Register(new DataMeta
    {
        Key = "Projectile.Direction",
        DisplayName = "Direction",
        Type = typeof(Vector2Value),
        Category = ProjectileCategory.Runtime,
        DefaultValue = Vector2Value.Zero,
        Description = "投射物初始飞行方向。"
    });

    /// <summary>飞行速度。</summary>
    public static readonly DataMeta Speed = DataRegistry.Register(new DataMeta
    {
        Key = "Projectile.Speed",
        DisplayName = "Speed",
        Type = typeof(float),
        Category = ProjectileCategory.Basic,
        DefaultValue = 0f,
        MinValue = 0f,
        SupportModifiers = true,
        Description = "投射物飞行速度。"
    });

    /// <summary>最大有效命中数，-1 表示不限制。</summary>
    public static readonly DataMeta MaxHitCount = DataRegistry.Register(new DataMeta
    {
        Key = "Projectile.MaxHitCount",
        DisplayName = "Max Hit Count",
        Type = typeof(int),
        Category = ProjectileCategory.Basic,
        DefaultValue = 1,
        MinValue = -1f,
        SupportModifiers = true,
        Description = "投射物最大有效命中数，-1 表示不限制。"
    });

    /// <summary>已有效命中次数。</summary>
    public static readonly DataMeta HitCount = DataRegistry.Register(new DataMeta
    {
        Key = "Projectile.HitCount",
        DisplayName = "Hit Count",
        Type = typeof(int),
        Category = ProjectileCategory.Runtime,
        DefaultValue = 0,
        MinValue = 0f,
        Description = "投射物本次生命周期内已经命中的目标数量。"
    });

    /// <summary>最大存活时间，-1 表示不限制。</summary>
    public static readonly DataMeta MaxLifeTime = DataRegistry.Register(new DataMeta
    {
        Key = "Projectile.MaxLifeTime",
        DisplayName = "Max Life Time",
        Type = typeof(float),
        Category = ProjectileCategory.Basic,
        DefaultValue = -1f,
        MinValue = -1f,
        SupportModifiers = true,
        Description = "投射物最大存活时间，-1 表示不限制。"
    });

    /// <summary>命中伤害。</summary>
    public static readonly DataMeta Damage = DataRegistry.Register(new DataMeta
    {
        Key = "Projectile.Damage",
        DisplayName = "Damage",
        Type = typeof(float),
        Category = ProjectileCategory.Basic,
        DefaultValue = 0f,
        MinValue = 0f,
        SupportModifiers = true,
        Description = "投射物命中伤害。"
    });

    /// <summary>伤害类型。</summary>
    public static readonly DataMeta DamageType = DataRegistry.Register(new DataMeta
    {
        Key = "Projectile.DamageType",
        DisplayName = "Damage Type",
        Type = typeof(DamageType),
        Category = ProjectileCategory.Basic,
        DefaultValue = Capabilities.Damage.DamageType.Physical,
        Description = "投射物命中伤害类型。"
    });

    /// <summary>伤害标签。</summary>
    public static readonly DataMeta DamageTags = DataRegistry.Register(new DataMeta
    {
        Key = "Projectile.DamageTags",
        DisplayName = "Damage Tags",
        Type = typeof(DamageTags),
        Category = ProjectileCategory.Basic,
        DefaultValue = Capabilities.Damage.DamageTags.Projectile,
        Description = "投射物命中伤害标签。"
    });

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
