using Godot;
using SlimeAI.GameOS.Capabilities.Collision;
using SlimeAI.GameOS.Capabilities.Damage;
using SlimeAI.GameOS.Capabilities.Movement;
using SlimeAI.GameOS.Capabilities.Projectile;
using ProjectileEvents = SlimeAI.GameOS.Capabilities.Projectile.Events;
using SlimeAI.GameOS.GodotBridge;
using SlimeAI.GameOS.Observation;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;
using SlimeAI.GameOS.Runtime.Pool;

namespace SlimeAI.SceneTests.GameOS.Capabilities.Projectile;

public partial class ProjectileCapabilityValidationScene : Node
{
    private const string ScenePath = "res://SlimeAI/Src/Validation/GameOS/Capabilities/Projectile/ProjectileCapabilityValidation.tscn";
    private const string PassMarker = "GameOS Capability Projectile validation PASS";
    private const string FailMarker = "GameOS Capability Projectile validation FAIL";

    public override void _Ready()
    {
        CapabilityValidationSupport.Run(
            this,
            ScenePath,
            "ProjectileCapabilityValidation",
            "GameOS/Capabilities/Projectile",
            "projectile-capability-validation.json",
            PassMarker,
            FailMarker,
            new[] { "SlimeAI.GameOS.Capabilities.Projectile", "SlimeAI.GameOS.Capabilities.Movement", "SlimeAI.GameOS.Capabilities.Damage" },
            new[] { "Projectile visuals are not loaded; ScenePath is validated as data." },
            new[] { "source, projectile and target entities", "ProjectileSpawnOptions", "MovementSystem", "collision query", "DamageService" },
            new[]
            {
                "ProjectileTool.Spawn writes projectile data and spawned event",
                "movement collision emits hit events",
                "pierce/max hits and lifetime destroy are enforced",
                "damage integration applies projectile damage",
                "projectile visual node returns to pool on runtime entity destruction"
            },
            new[] { $"stdout contains {PassMarker}", "artifact status is pass", "failureReasons is empty" },
            new[] { $"stdout contains {FailMarker}", "any Projectile check fails", "artifact standard-answer fields are missing" },
            validation =>
            {
                validation.Check("spawn_data_and_event", "ProjectileSpawn", ValidateSpawnDataAndEvent);
                validation.Check("movement_hit_and_damage", "ProjectileHit", ValidateMovementHitAndDamage);
                validation.Check("pierce_max_hits", "ProjectileLifecycle", ValidatePierceMaxHits);
                validation.Check("lifetime_destroy", "ProjectileLifecycle", ValidateLifetimeDestroy);
                validation.Check("visual_lifecycle_pool_cleanup", "ProjectileVisualLifecycle", () => ValidateVisualLifecycle(this));
            });
    }

    private static CheckResult ValidateSpawnDataAndEvent()
    {
        EntityManager.Clear();
        WorldEvents.World.Clear();
        var source = CapabilityValidationSupport.Spawn("projectile-scene-source");
        var target = CapabilityValidationSupport.Spawn("projectile-scene-target");
        target.Data.Set(MovementDataKeys.Position, new Vector2Value(8f, 0f));
        var spawned = 0;
        using var token = WorldEvents.World.Subscribe<ProjectileEvents.Spawned>(_ => spawned++);

        var result = ProjectileTool.Spawn(new ProjectileSpawnOptions
        {
            Source = source,
            Target = target,
            EntityId = new EntityId("projectile-scene-projectile"),
            ScenePath = "res://does-not-load/projectile.tscn",
            SpawnPosition = Vector2Value.Zero,
            Speed = 10f,
            MaxHitCount = 2,
            MaxLifeTime = 1f,
            Damage = 4f
        });
        var projectile = result.Projectile;
        var ownerList = source.Data.Get(ProjectileDataKeys.SpawnedProjectileIds);
        var success = result.Created
            && spawned == 1
            && projectile.Data.Get<EntityId?>(ProjectileDataKeys.SourceEntity, null) == source.EntityId
            && projectile.Data.Get<EntityId?>(ProjectileDataKeys.TargetEntity, null) == target.EntityId
            && projectile.Data.Get<int>(ProjectileDataKeys.MaxHitCount, 0) == 2
            && ownerList.Contains(projectile.EntityId);

        return CheckResult.From(success, success ? "projectile spawn data and event passed" : "projectile spawn mismatch", CapabilityValidationSupport.Details(
            ("created", result.Created),
            ("spawnedEvents", spawned),
            ("ownerListCount", ownerList.Count),
            ("maxHitCount", projectile.Data.Get<int>(ProjectileDataKeys.MaxHitCount, 0))));
    }

    private static CheckResult ValidateMovementHitAndDamage()
    {
        EntityManager.Clear();
        WorldEvents.World.Clear();
        var source = CapabilityValidationSupport.Spawn("projectile-scene-hit-source");
        var target = CapabilityValidationSupport.Spawn("projectile-scene-hit-target");
        source.Data.Set(CollisionDataKeys.Team, 1);
        target.Data.Set(CollisionDataKeys.Team, 2);
        target.Data.Set(CollisionDataKeys.CollisionLayer, 0b0010u);
        target.Data.Set(CollisionDataKeys.CollisionRadius, 0.5f);
        target.Data.Set(MovementDataKeys.Position, new Vector2Value(4f, 0f));
        target.Data.Set(DamageDataKeys.CurrentHp, 10f);
        var projectile = ProjectileTool.Spawn(new ProjectileSpawnOptions
        {
            Source = source,
            Target = target,
            EntityId = new EntityId("projectile-scene-hit-projectile"),
            SpawnPosition = Vector2Value.Zero,
            Direction = new Vector2Value(1f, 0f),
            Speed = 10f,
            Damage = 3f
        }).Projectile;
        projectile.Data.Set(CollisionDataKeys.CollisionMask, 0b0010u);
        projectile.Data.Set(CollisionDataKeys.CollisionRadius, 0.5f);
        var hits = 0;
        using var token = WorldEvents.World.Subscribe<ProjectileEvents.Hit>(data => { if (data.Target.EntityId == target.EntityId) hits++; });
        var movement = new MovementSystem(new FixedMovementCollisionTargetQuery(new[] { target }));
        ProjectileTool.StartMovement(projectile, movement, damageService: new DamageService());
        movement.Tick(0.5f);
        var success = hits == 1
            && EntityManager.Get(projectile.EntityId) == null
            && CapabilityValidationSupport.Approximately(target.Data.Get<float>(DamageDataKeys.CurrentHp, 0f), 7f);

        return CheckResult.From(success, success ? "projectile movement hit applied damage" : "projectile hit/damage mismatch", CapabilityValidationSupport.Details(
            ("hits", hits),
            ("destroyed", EntityManager.Get(projectile.EntityId) == null),
            ("targetHp", target.Data.Get<float>(DamageDataKeys.CurrentHp, 0f))));
    }

    private static CheckResult ValidatePierceMaxHits()
    {
        EntityManager.Clear();
        WorldEvents.World.Clear();
        var source = CapabilityValidationSupport.Spawn("projectile-scene-pierce-source");
        var targetA = CapabilityValidationSupport.Spawn("projectile-scene-pierce-a");
        var targetB = CapabilityValidationSupport.Spawn("projectile-scene-pierce-b");
        targetA.Data.Set(CollisionDataKeys.CollisionLayer, 0b0010u);
        targetB.Data.Set(CollisionDataKeys.CollisionLayer, 0b0010u);
        targetA.Data.Set(MovementDataKeys.Position, new Vector2Value(3f, 0f));
        targetB.Data.Set(MovementDataKeys.Position, new Vector2Value(6f, 0f));
        var projectile = ProjectileTool.Spawn(new ProjectileSpawnOptions
        {
            Source = source,
            EntityId = new EntityId("projectile-scene-pierce-projectile"),
            SpawnPosition = Vector2Value.Zero,
            Direction = new Vector2Value(1f, 0f),
            Speed = 10f,
            MaxHitCount = 2,
            Damage = 0f
        }).Projectile;
        projectile.Data.Set(CollisionDataKeys.CollisionMask, 0b0010u);
        projectile.Data.Set(CollisionDataKeys.CollisionRadius, 0.5f);
        targetA.Data.Set(CollisionDataKeys.CollisionRadius, 0.5f);
        targetB.Data.Set(CollisionDataKeys.CollisionRadius, 0.5f);
        var hits = 0;
        using var token = WorldEvents.World.Subscribe<ProjectileEvents.Hit>(_ => hits++);
        var movement = new MovementSystem(new FixedMovementCollisionTargetQuery(new[] { targetA, targetB }));
        ProjectileTool.StartMovement(projectile, movement, new ProjectileMovementOptions { ApplyDamageOnHit = false });
        movement.Tick(1f);
        var success = hits == 2 && EntityManager.Get(projectile.EntityId) == null;

        return CheckResult.From(success, success ? "projectile pierced until max hits then destroyed" : "projectile pierce/max-hit mismatch", CapabilityValidationSupport.Details(
            ("hits", hits),
            ("destroyed", EntityManager.Get(projectile.EntityId) == null)));
    }

    private static CheckResult ValidateLifetimeDestroy()
    {
        EntityManager.Clear();
        var source = CapabilityValidationSupport.Spawn("projectile-scene-lifetime-source");
        var projectile = ProjectileTool.Spawn(new ProjectileSpawnOptions
        {
            Source = source,
            EntityId = new EntityId("projectile-scene-lifetime-projectile"),
            SpawnPosition = Vector2Value.Zero,
            Direction = new Vector2Value(1f, 0f),
            Speed = 1f,
            MaxLifeTime = 0.1f
        }).Projectile;
        var movement = new MovementSystem(new FixedMovementCollisionTargetQuery(System.Array.Empty<IEntity>()));
        ProjectileTool.StartMovement(projectile, movement);
        movement.Tick(0.2f);
        var success = EntityManager.Get(projectile.EntityId) == null;

        return CheckResult.From(success, success ? "projectile destroyed after lifetime" : "projectile lifetime destroy mismatch", CapabilityValidationSupport.Details(
            ("destroyed", success)));
    }

    private static CheckResult ValidateVisualLifecycle(Node owner)
    {
        EntityManager.Clear();
        WorldEvents.World.Clear();
        GodotNodeRegistry.Clear();
        ObjectPoolManager.DestroyAll();

        var spawner = new GodotProjectileEffectSpawner
        {
            Name = "ProjectileVisualLifecycleSpawner",
            AutoSubscribe = false
        };
        owner.AddChild(spawner);
        spawner.Subscribe();

        var source = CapabilityValidationSupport.Spawn("projectile-scene-visual-source");
        var projectileId = new EntityId("projectile-scene-visual-projectile");
        var result = ProjectileTool.Spawn(new ProjectileSpawnOptions
        {
            Source = source,
            EntityId = projectileId,
            ScenePath = "res://SlimeAI/Src/Validation/GameOS/Capabilities/Projectile/ProjectileVisualProbe.tscn",
            SpawnPosition = new Vector2Value(5f, 6f),
            Direction = new Vector2Value(1f, 0f),
            Speed = 1f
        });

        var node = GodotNodeRegistry.GetNodeById(projectileId.Value);
        var poolName = node != null && node.HasMeta("GameOSVisualPoolName")
            ? node.GetMeta("GameOSVisualPoolName").AsString()
            : string.Empty;
        EntityManager.Destroy(result.Projectile);

        var destroyed = EntityManager.Get(projectileId) == null;
        var registryCleared = GodotNodeRegistry.GetNodeById(projectileId.Value) == null;
        var poolReturned = !string.IsNullOrWhiteSpace(poolName)
            && ObjectPoolManager.GetAllStats().TryGetValue(poolName, out var stats)
            && stats.TotalReleased >= 1
            && stats.Count >= 1;
        var success = result.Created
            && node != null
            && destroyed
            && registryCleared
            && poolReturned;

        spawner.Unsubscribe();
        spawner.QueueFree();

        return CheckResult.From(success, success ? "projectile visual returned to pool" : "projectile visual lifecycle mismatch", CapabilityValidationSupport.Details(
            ("created", result.Created),
            ("nodeRegisteredBeforeCleanup", node != null),
            ("poolName", poolName),
            ("runtimeDestroyed", destroyed),
            ("registryCleared", registryCleared),
            ("poolReturned", poolReturned)));
    }
}
