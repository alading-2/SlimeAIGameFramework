using SlimeAI.GameOS.Capabilities.Collision;
using SlimeAI.GameOS.Capabilities.Damage;
using SlimeAI.GameOS.Capabilities.Movement;
using SlimeAI.GameOS.Capabilities.Projectile;
using SlimeAI.GameOS.Observation;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;
using SlimeAI.GameOS.Runtime.World;
using ProjectileEvents = SlimeAI.GameOS.Capabilities.Projectile.Events;
using static TestAssert;

internal partial class Program
{
    static void TestProjectileToolSpawnsRuntimeEntity()
    {
        GameOSLog.Reset(new GameOSLogOptions { EnableStdout = false, EnableJsonl = false });
        var memory = new GameOSMemoryLogSink();
        GameOSLog.AddSink(memory);
        EntityManager.Clear();
        WorldEvents.World.Clear();
        var source = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("projectile-source") });
        var ability = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("projectile-ability") });
        var target = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("projectile-target") });
        target.Data.Set(MovementDataKeys.Position, new Vector2Value(10f, 0f));

        var spawnedEvents = 0;
        var spawnSub = WorldEvents.World.Subscribe<ProjectileEvents.Spawned>(data =>
        {
            spawnedEvents++;
            AssertEqual("projectile event source", source.EntityId, data.Source.EntityId);
            AssertEqual("projectile event target", target.EntityId, data.Target?.EntityId);
        });

        var result = ProjectileTool.Spawn(new ProjectileSpawnOptions
        {
            Source = source,
            Ability = ability,
            Target = target,
            EntityId = new EntityId("projectile-runtime"),
            ScenePath = "res://Projectiles/Fireball.tscn",
            SpawnPosition = Vector2Value.Zero,
            Speed = 12f,
            MaxHitCount = 3,
            MaxLifeTime = 2f,
            Damage = 4f,
            DamageType = DamageType.Magical,
            DamageTags = DamageTags.Projectile | DamageTags.Ability
        });

        AssertEqual("projectile created", true, result.Created);
        AssertEqual("projectile registered", result.Projectile.EntityId, EntityManager.Get(new EntityId("projectile-runtime"))?.EntityId);
        AssertEqual("projectile source typed", source.EntityId, result.Projectile.Data.Get<EntityId?>(ProjectileDataKeys.SourceEntity, null) ?? EntityId.Empty);
        AssertEqual("projectile target typed", target.EntityId, result.Projectile.Data.Get<EntityId?>(ProjectileDataKeys.TargetEntity, null) ?? EntityId.Empty);
        AssertEqual("spawner spawned-projectile-id present", true, source.Data.Get(ProjectileDataKeys.SpawnedProjectileIds).Contains(result.Projectile.EntityId));
        AssertEqual("projectile lifecycle parent attached", true, LifecycleTree.IsAttached(source.EntityId, result.Projectile.EntityId));
        AssertEqual("projectile scene path", "res://Projectiles/Fireball.tscn", result.Projectile.Data.Get<string>(ProjectileDataKeys.ScenePath));
        AssertEqual("projectile direction", new Vector2Value(1f, 0f), result.Projectile.Data.Get<Vector2Value>(ProjectileDataKeys.Direction));
        AssertNear("projectile speed", 12f, result.Projectile.Data.Get<float>(ProjectileDataKeys.Speed));
        AssertEqual("projectile max hit count", 3, result.Projectile.Data.Get<int>(ProjectileDataKeys.MaxHitCount));
        AssertEqual("projectile hit count", 0, result.Projectile.Data.Get<int>(ProjectileDataKeys.HitCount));
        AssertNear("projectile max lifetime", 2f, result.Projectile.Data.Get<float>(ProjectileDataKeys.MaxLifeTime));
        AssertNear("projectile damage", 4f, result.Projectile.Data.Get<float>(ProjectileDataKeys.Damage));
        AssertEqual("projectile events", 1, spawnedEvents);
        AssertEqual("projectile fired log", true, memory.Entries.Any(entry =>
            entry.Context == "ProjectileTool" &&
            entry.Level == GameOSLogLevel.Info &&
            entry.Message == "Projectile fired: projectile-runtime, speed=12"));

        spawnSub.Dispose();
        WorldEvents.World.Clear();
        EntityManager.Clear();
        GameOSLog.Reset(new GameOSLogOptions { EnableStdout = false, EnableJsonl = false });
    }

    static void TestProjectileMovementHitDamagesAndDestroys()
    {
        GameOSLog.Reset(new GameOSLogOptions { EnableStdout = false, EnableJsonl = false });
        var memory = new GameOSMemoryLogSink();
        GameOSLog.AddSink(memory);
        EntityManager.Clear();
        WorldEvents.World.Clear();
        var source = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("projectile-hit-source") });
        source.Data.Set(CollisionDataKeys.Team, 1);

        var target = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("projectile-hit-target") });
        target.Data.Set(MovementDataKeys.Position, new Vector2Value(10f, 0f));
        target.Data.Set(CollisionDataKeys.CollisionLayer, CollisionLayers.EnemyHurtbox);
        target.Data.Set(CollisionDataKeys.CollisionRadius, 1f);
        target.Data.Set(CollisionDataKeys.Team, 2);
        target.Data.Set(DamageDataKeys.MaxHp, 20f);
        target.Data.Set(DamageDataKeys.CurrentHp, 20f);

        var projectile = ProjectileTool.Spawn(new ProjectileSpawnOptions
        {
            Source = source,
            Target = target,
            EntityId = new EntityId("projectile-hit-runtime"),
            SpawnPosition = Vector2Value.Zero,
            Speed = 20f,
            Damage = 6f,
            DamageTags = DamageTags.Projectile | DamageTags.Ability
        });
        projectile.Projectile.Data.Set(CollisionDataKeys.CollisionLayer, CollisionLayers.Projectile);
        projectile.Projectile.Data.Set(CollisionDataKeys.CollisionMask, CollisionLayers.EnemyHurtbox);
        projectile.Projectile.Data.Set(CollisionDataKeys.CollisionRadius, 1f);
        projectile.Projectile.Data.Set(CollisionDataKeys.Team, 1);

        var hitEvents = 0;
        var hitSub = WorldEvents.World.Subscribe<ProjectileEvents.Hit>(data =>
        {
            hitEvents++;
            AssertEqual("projectile hit source", source.EntityId, data.Source.EntityId);
            AssertEqual("projectile hit target", target.EntityId, data.Target.EntityId);
            AssertEqual("projectile hit damage applied", true, data.Damage.Applied);
        });

        var movement = new MovementSystem();
        var started = ProjectileTool.StartMovement(projectile.Projectile, movement);
        AssertEqual("projectile movement started", true, started);

        movement.Tick(1f);

        AssertEqual("projectile hit event count", 1, hitEvents);
        AssertNear("projectile target damaged", 14f, target.Data.Get<float>(DamageDataKeys.CurrentHp));
        AssertEqual("projectile destroyed", null, EntityManager.Get(projectile.Projectile.EntityId));
        AssertEqual("projectile movement stopped after destroy", false, movement.IsMoving(projectile.Projectile));
        AssertEqual("projectile hit log", true, memory.Entries.Any(entry =>
            entry.Context == "ProjectileTool" &&
            entry.Level == GameOSLogLevel.Info &&
            entry.Message == "Projectile hit: projectile-hit-runtime, target=projectile-hit-target"));

        hitSub.Dispose();
        WorldEvents.World.Clear();
        EntityManager.Clear();
        GameOSLog.Reset(new GameOSLogOptions { EnableStdout = false, EnableJsonl = false });
    }

    static void TestProjectileMovementPiercesAndDestroysAfterMaxHits()
    {
        EntityManager.Clear();
        WorldEvents.World.Clear();
        var source = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("projectile-pierce-source") });
        source.Data.Set(CollisionDataKeys.Team, 1);

        var firstTarget = CreateProjectileTarget("projectile-pierce-target-1", new Vector2Value(5f, 0f));
        var secondTarget = CreateProjectileTarget("projectile-pierce-target-2", new Vector2Value(10f, 0f));
        var thirdTarget = CreateProjectileTarget("projectile-pierce-target-3", new Vector2Value(15f, 0f));

        var projectile = ProjectileTool.Spawn(new ProjectileSpawnOptions
        {
            Source = source,
            EntityId = new EntityId("projectile-pierce-runtime"),
            SpawnPosition = Vector2Value.Zero,
            Direction = new Vector2Value(1f, 0f),
            Speed = 20f,
            MaxHitCount = 2,
            Damage = 3f,
            DamageTags = DamageTags.Projectile
        });
        projectile.Projectile.Data.Set(CollisionDataKeys.CollisionLayer, CollisionLayers.Projectile);
        projectile.Projectile.Data.Set(CollisionDataKeys.CollisionMask, CollisionLayers.EnemyHurtbox);
        projectile.Projectile.Data.Set(CollisionDataKeys.CollisionRadius, 1f);
        projectile.Projectile.Data.Set(CollisionDataKeys.Team, 1);

        var hitTargets = new List<EntityId>();
        var hitSub = WorldEvents.World.Subscribe<ProjectileEvents.Hit>(data =>
        {
            if (data.Projectile.EntityId == projectile.Projectile.EntityId)
            {
                hitTargets.Add(data.Target.EntityId);
            }
        });

        var movement = new MovementSystem();
        var started = ProjectileTool.StartMovement(projectile.Projectile, movement);
        AssertEqual("projectile pierce movement started", true, started);

        movement.Tick(1f);

        AssertEqual("projectile pierce hit count", 2, hitTargets.Count);
        AssertEqual("projectile pierce first target", firstTarget.EntityId, hitTargets[0]);
        AssertEqual("projectile pierce second target", secondTarget.EntityId, hitTargets[1]);
        AssertNear("projectile pierce first damaged", 17f, firstTarget.Data.Get<float>(DamageDataKeys.CurrentHp));
        AssertNear("projectile pierce second damaged", 17f, secondTarget.Data.Get<float>(DamageDataKeys.CurrentHp));
        AssertNear("projectile pierce third untouched", 20f, thirdTarget.Data.Get<float>(DamageDataKeys.CurrentHp));
        AssertEqual("projectile pierce destroyed", null, EntityManager.Get(projectile.Projectile.EntityId));
        AssertEqual("projectile pierce movement stopped", false, movement.IsMoving(projectile.Projectile));

        hitSub.Dispose();
        WorldEvents.World.Clear();
        EntityManager.Clear();
    }

    static void TestProjectileMovementLifetimeDestroys()
    {
        EntityManager.Clear();
        var source = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("projectile-lifetime-source") });
        var projectile = ProjectileTool.Spawn(new ProjectileSpawnOptions
        {
            Source = source,
            EntityId = new EntityId("projectile-lifetime-runtime"),
            SpawnPosition = Vector2Value.Zero,
            Direction = new Vector2Value(1f, 0f),
            Speed = 5f,
            MaxLifeTime = 0.5f,
            Damage = 0f
        });

        var movement = new MovementSystem();
        var started = ProjectileTool.StartMovement(projectile.Projectile, movement);
        AssertEqual("projectile lifetime movement started", true, started);

        movement.Tick(0.5f);

        AssertEqual("projectile lifetime destroyed", null, EntityManager.Get(projectile.Projectile.EntityId));
        AssertEqual("projectile lifetime movement stopped", false, movement.IsMoving(projectile.Projectile));

        EntityManager.Clear();
    }

    static IEntity CreateProjectileTarget(string entityId, Vector2Value position)
    {
        var target = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId(entityId) });
        target.Data.Set(MovementDataKeys.Position, position);
        target.Data.Set(CollisionDataKeys.CollisionLayer, CollisionLayers.EnemyHurtbox);
        target.Data.Set(CollisionDataKeys.CollisionRadius, 1f);
        target.Data.Set(CollisionDataKeys.Team, 2);
        target.Data.Set(DamageDataKeys.MaxHp, 20f);
        target.Data.Set(DamageDataKeys.CurrentHp, 20f);
        return target;
    }
}
