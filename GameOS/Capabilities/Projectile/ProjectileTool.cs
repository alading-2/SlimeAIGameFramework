using System;
using SkilmeAI.GameOS.Capabilities.Collision;
using SkilmeAI.GameOS.Capabilities.Damage;
using SkilmeAI.GameOS.Capabilities.Movement;
using SkilmeAI.GameOS.Runtime.Entity;
using SkilmeAI.GameOS.Runtime.Event;
using SkilmeAI.GameOS.Runtime.Relationship;

namespace SkilmeAI.GameOS.Capabilities.Projectile;

/// <summary>
/// Projectile Capability 最小 Runtime 入口，负责生成投射物实体并写入运行时 Data。
/// </summary>
public static class ProjectileTool
{
    /// <summary>
    /// 生成纯 Runtime 投射物实体。
    /// </summary>
    /// <param name="options">投射物生成参数。</param>
    public static ProjectileSpawnResult Spawn(ProjectileSpawnOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ProjectileDataKeys.RegisterAll();

        var projectile = EntityManager.Spawn(new EntitySpawnConfig
        {
            EntityId = options.EntityId,
            ParentEntityId = options.Source.EntityId,
            AutoAddParentRelation = true,
            ParentRelationTypes = [RelationshipType.EntityToProjectile, RelationshipType.Source]
        });

        var targetPosition = options.TargetPosition
            ?? options.Target?.Data.Get<Vector2Value>(MovementDataKeys.Position, options.SpawnPosition)
            ?? options.SpawnPosition;
        var direction = options.Direction ?? (targetPosition - options.SpawnPosition).Normalized();

        projectile.Data.Set(ProjectileDataKeys.SourceEntity, options.Source);
        if (options.Ability != null)
        {
            projectile.Data.Set(ProjectileDataKeys.AbilityEntity, options.Ability);
        }

        if (options.Target != null)
        {
            projectile.Data.Set(ProjectileDataKeys.TargetEntity, options.Target);
            RelationshipManager.AddRelationship(
                projectile.EntityId,
                options.Target.EntityId,
                RelationshipType.Target);
        }

        projectile.Data.Set(ProjectileDataKeys.ScenePath, options.ScenePath ?? string.Empty);
        projectile.Data.Set(ProjectileDataKeys.SpawnPosition, options.SpawnPosition);
        projectile.Data.Set(ProjectileDataKeys.TargetPosition, targetPosition);
        projectile.Data.Set(ProjectileDataKeys.Direction, direction);
        projectile.Data.Set(ProjectileDataKeys.Speed, options.Speed);
        projectile.Data.Set(ProjectileDataKeys.MaxHitCount, options.MaxHitCount);
        projectile.Data.Set(ProjectileDataKeys.HitCount, 0);
        projectile.Data.Set(ProjectileDataKeys.MaxLifeTime, options.MaxLifeTime);
        projectile.Data.Set(ProjectileDataKeys.Damage, options.Damage);
        projectile.Data.Set(ProjectileDataKeys.DamageType, options.DamageType);
        projectile.Data.Set(ProjectileDataKeys.DamageTags, options.DamageTags);
        projectile.Data.Set(MovementDataKeys.Position, options.SpawnPosition);
        projectile.Data.Set(MovementDataKeys.FacingDirection, direction);

        var data = new GameEventType.Projectile.SpawnedEventData(projectile, options.Source, options.Ability, options.Target);
        projectile.Events.Emit(GameEventType.Projectile.Spawned, data);
        GlobalEventBus.Global.Emit(GameEventType.Projectile.Spawned, data);
        return new ProjectileSpawnResult(projectile, true);
    }

    /// <summary>
    /// 启动投射物飞行，并把 Movement 碰撞转换为 Projectile 命中伤害。
    /// </summary>
    /// <param name="projectile">投射物实体。</param>
    /// <param name="movementSystem">Movement 调度系统。</param>
    /// <param name="options">飞行和命中参数。</param>
    /// <param name="damageService">伤害服务；为空时使用默认实例。</param>
    public static bool StartMovement(
        IEntity projectile,
        MovementSystem movementSystem,
        ProjectileMovementOptions? options = null,
        DamageService? damageService = null)
    {
        ArgumentNullException.ThrowIfNull(projectile);
        ArgumentNullException.ThrowIfNull(movementSystem);
        ProjectileDataKeys.RegisterAll();
        CollisionDataKeys.RegisterAll();
        options ??= new ProjectileMovementOptions();

        var source = projectile.Data.Get<IEntity?>(ProjectileDataKeys.SourceEntity, null);
        var target = projectile.Data.Get<IEntity?>(ProjectileDataKeys.TargetEntity, null);
        var direction = projectile.Data.Get<Vector2Value>(ProjectileDataKeys.Direction, Vector2Value.Zero).Normalized();
        var targetPosition = projectile.Data.Get<Vector2Value>(ProjectileDataKeys.TargetPosition, Vector2Value.Zero);
        var speed = options.Speed >= 0f
            ? options.Speed
            : projectile.Data.Get<float>(ProjectileDataKeys.Speed, 0f);

        var collisionParams = new MovementCollisionParams
        {
            FilterPolicy = options.ToFilterPolicy(),
            TargetMatchMode = ResolveTargetMatchMode(options, target),
            SpecificTargetEntityId = ResolveSpecificTargetEntityId(options, target),
            SourceRadiusOverride = options.SourceRadiusOverride,
            TargetRadiusOverride = options.TargetRadiusOverride,
            StopAfterCollisionCount = ResolveMaxHitCount(projectile, options),
            DestroyOnStop = options.DestroyOnStop,
            OnCollision = context => HandleProjectileHit(projectile, context, options, damageService)
        };

        var started = movementSystem.Start(projectile, new MovementParams
        {
            Mode = options.Mode,
            Direction = direction,
            TargetPosition = options.StopAtTarget ? targetPosition : null,
            TargetEntityId = target?.EntityId,
            Speed = speed,
            MaxDuration = ResolveMaxDuration(projectile, options),
            MaxDistance = options.MaxDistance,
            ReachDistance = options.ReachDistance,
            StopAtTarget = options.StopAtTarget,
            CollisionParams = collisionParams
        });
        if (started && options.DestroyOnStop)
        {
            projectile.Events.Once<GameEventType.Movement.StoppedEventData>(
                GameEventType.Movement.Stopped,
                _ => DestroyProjectileOnStop(projectile));
        }

        return started;
    }

    private static MovementCollisionTargetMatchMode ResolveTargetMatchMode(
        ProjectileMovementOptions options,
        IEntity? target)
    {
        if (options.TargetMatchMode == MovementCollisionTargetMatchMode.TrackedTargetOnly && target == null)
        {
            return MovementCollisionTargetMatchMode.Any;
        }

        return options.TargetMatchMode;
    }

    private static string? ResolveSpecificTargetEntityId(ProjectileMovementOptions options, IEntity? target)
    {
        return options.TargetMatchMode == MovementCollisionTargetMatchMode.SpecificEntity
            ? target?.EntityId
            : null;
    }

    private static void HandleProjectileHit(
        IEntity projectile,
        MovementCollisionContext context,
        ProjectileMovementOptions options,
        DamageService? damageService)
    {
        var source = projectile.Data.Get<IEntity?>(ProjectileDataKeys.SourceEntity, null);
        var hitCount = projectile.Data.Get<int>(ProjectileDataKeys.HitCount, 0) + 1;
        projectile.Data.Set(ProjectileDataKeys.HitCount, hitCount);

        var damage = default(DamageResult);
        if (options.ApplyDamageOnHit)
        {
            damageService ??= DamageService.Instance;
            damage = damageService.Process(new DamageInfo
            {
                Attacker = source,
                Victim = context.Target,
                Damage = ResolveDamage(projectile, options),
                Type = options.DamageType ?? projectile.Data.Get<DamageType>(ProjectileDataKeys.DamageType, DamageType.Physical),
                Tags = options.DamageTags ?? projectile.Data.Get<DamageTags>(ProjectileDataKeys.DamageTags, DamageTags.Projectile)
            });
        }

        var data = new GameEventType.Projectile.HitEventData(
            projectile,
            source ?? projectile,
            context.Target,
            context,
            damage);
        projectile.Events.Emit(GameEventType.Projectile.Hit, data);
        GlobalEventBus.Global.Emit(GameEventType.Projectile.Hit, data);
    }

    private static float ResolveDamage(IEntity projectile, ProjectileMovementOptions options)
    {
        return options.Damage >= 0f
            ? options.Damage
            : projectile.Data.Get<float>(ProjectileDataKeys.Damage, 0f);
    }

    private static int ResolveMaxHitCount(IEntity projectile, ProjectileMovementOptions options)
    {
        return options.StopAfterHitCount == ProjectileMovementOptions.UseProjectileDataMaxHitCount
            ? projectile.Data.Get<int>(ProjectileDataKeys.MaxHitCount, 1)
            : options.StopAfterHitCount;
    }

    private static float ResolveMaxDuration(IEntity projectile, ProjectileMovementOptions options)
    {
        return float.IsNaN(options.MaxDuration)
            ? projectile.Data.Get<float>(ProjectileDataKeys.MaxLifeTime, -1f)
            : options.MaxDuration;
    }

    private static void DestroyProjectileOnStop(IEntity projectile)
    {
        if (EntityManager.Get(projectile.EntityId) != null)
        {
            EntityManager.Destroy(projectile);
        }
    }
}
