using SlimeAI.GameOS.Capabilities.Collision;
using SlimeAI.GameOS.Capabilities.Damage;
using SlimeAI.GameOS.Capabilities.Movement;
using SlimeAI.GameOS.Runtime.Entity;
using MovementEvents = SlimeAI.GameOS.Capabilities.Movement.Events;
using static TestAssert;

internal partial class Program
{
    static void TestMovementChargeTick()
    {
        EntityManager.Clear();
        var entity = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-charge") });
        entity.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);

        var movement = new MovementSystem();
        var started = movement.Start(entity, new MovementParams
        {
            Mode = MoveMode.Charge,
            Direction = new Vector2Value(1f, 0f),
            Speed = 100f,
            MaxDistance = 50f
        });

        movement.Tick(0.25f);
        AssertEqual("movement started", true, started);
        AssertEqual("movement still active", true, movement.IsMoving(entity));
        AssertNear("movement x after first tick", 25f, entity.Data.Get<Vector2Value>(MovementDataKeys.Position).X);
        AssertNear("movement facing x", 1f, entity.Data.Get<Vector2Value>(MovementDataKeys.FacingDirection).X);

        movement.Tick(0.5f);
        AssertEqual("movement stopped by distance", false, movement.IsMoving(entity));
        AssertNear("movement clamped x", 50f, entity.Data.Get<Vector2Value>(MovementDataKeys.Position).X);
        AssertEqual("movement velocity reset", Vector2Value.Zero, entity.Data.Get<Vector2Value>(MovementDataKeys.Velocity));

        EntityManager.Clear();
    }

    static void TestMovementTargetStopEvent()
    {
        EntityManager.Clear();
        var entity = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-target") });
        entity.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);

        var stopped = 0;
        MovementStopReason? reason = null;
        entity.Events.Subscribe<MovementEvents.Stopped>(data =>
        {
            stopped++;
            reason = data.Context.Reason;
        });

        var movement = new MovementSystem();
        movement.Start(entity, new MovementParams
        {
            Mode = MoveMode.Charge,
            TargetPosition = new Vector2Value(30f, 0f),
            Speed = 100f,
            ReachDistance = 0f
        });

        movement.Tick(1f);

        AssertEqual("target stopped once", 1, stopped);
        AssertEqual("target stop reason", MovementStopReason.Completed, reason);
        AssertNear("target final x", 30f, entity.Data.Get<Vector2Value>(MovementDataKeys.Position).X);
        AssertEqual("target moving flag", false, entity.Data.Get<bool>(MovementDataKeys.IsMoving));

        EntityManager.Clear();
    }

    static void TestMovementOrbitTick()
    {
        EntityManager.Clear();
        var entity = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-orbit") });
        entity.Data.Set(MovementDataKeys.Position, new Vector2Value(10f, 0f));

        var movement = new MovementSystem();
        movement.Start(entity, new MovementParams
        {
            Mode = MoveMode.Orbit,
            OrbitCenter = Vector2Value.Zero,
            OrbitRadius = 10f,
            OrbitInitAngle = 0f,
            OrbitAngularSpeed = 90f,
            OrbitTotalAngle = 90f,
            IsOrbitClockwise = true
        });

        movement.Tick(1f);
        var position = entity.Data.Get<Vector2Value>(MovementDataKeys.Position);

        AssertEqual("orbit stopped", false, movement.IsMoving(entity));
        AssertNear("orbit x", 0f, position.X);
        AssertNear("orbit y", 10f, position.Y);

        EntityManager.Clear();
    }

    static void TestMovementSineWaveTick()
    {
        EntityManager.Clear();
        var entity = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-sine") });
        entity.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);

        var movement = new MovementSystem();
        movement.Start(entity, new MovementParams
        {
            Mode = MoveMode.SineWave,
            Angle = 0f,
            Speed = 10f,
            WaveAmplitude = 5f,
            WaveFrequency = 0.25f,
            WavePhase = 0f,
            MaxDuration = 1f
        });

        movement.Tick(1f);
        var position = entity.Data.Get<Vector2Value>(MovementDataKeys.Position);

        AssertEqual("sine stopped", false, movement.IsMoving(entity));
        AssertNear("sine x", 10f, position.X);
        AssertNear("sine y", 5f, position.Y);

        EntityManager.Clear();
    }

    static void TestMovementBezierCurveTick()
    {
        EntityManager.Clear();
        var entity = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-bezier") });
        entity.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);

        var movement = new MovementSystem();
        movement.Start(entity, new MovementParams
        {
            Mode = MoveMode.BezierCurve,
            TargetPosition = new Vector2Value(10f, 0f),
            BezierPoints = [new Vector2Value(5f, -10f), new Vector2Value(10f, 0f)],
            MaxDuration = 1f
        });

        movement.Tick(0.5f);
        var midpoint = entity.Data.Get<Vector2Value>(MovementDataKeys.Position);
        AssertEqual("bezier still moving", true, movement.IsMoving(entity));
        AssertNear("bezier mid x", 5f, midpoint.X);
        AssertNear("bezier mid y", -5f, midpoint.Y);

        movement.Tick(0.5f);
        var endpoint = entity.Data.Get<Vector2Value>(MovementDataKeys.Position);
        AssertEqual("bezier stopped", false, movement.IsMoving(entity));
        AssertNear("bezier end x", 10f, endpoint.X);
        AssertNear("bezier end y", 0f, endpoint.Y);

        EntityManager.Clear();
    }

    static void TestMovementBoomerangTick()
    {
        EntityManager.Clear();
        var entity = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-boomerang") });
        entity.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);

        var movement = new MovementSystem();
        movement.Start(entity, new MovementParams
        {
            Mode = MoveMode.Boomerang,
            TargetPosition = new Vector2Value(10f, 0f),
            Speed = 10f,
            BoomerangArcHeight = 0f,
            ReachDistance = 0.001f
        });

        movement.Tick(1f);
        AssertEqual("boomerang returning", true, movement.IsMoving(entity));

        movement.Tick(1f);
        var endpoint = entity.Data.Get<Vector2Value>(MovementDataKeys.Position);
        AssertEqual("boomerang stopped", false, movement.IsMoving(entity));
        AssertNear("boomerang return x", 0f, endpoint.X);
        AssertNear("boomerang return y", 0f, endpoint.Y);

        EntityManager.Clear();
    }

    static void TestMovementAttachToHostTick()
    {
        EntityManager.Clear();
        var host = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-host") });
        host.Data.Set(MovementDataKeys.Position, new Vector2Value(20f, 5f));
        var entity = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-attach") });
        entity.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);

        var movement = new MovementSystem();
        movement.Start(entity, new MovementParams
        {
            Mode = MoveMode.AttachToHost,
            TargetEntityId = host.EntityId,
            AttachOffset = new Vector2Value(2f, -1f),
            MaxDuration = 1f
        });

        movement.Tick(0.25f);
        var position = entity.Data.Get<Vector2Value>(MovementDataKeys.Position);
        AssertEqual("attach still moving", true, movement.IsMoving(entity));
        AssertNear("attach x", 22f, position.X);
        AssertNear("attach y", 4f, position.Y);

        EntityManager.Clear();
    }

    static void TestMovementPlayerInputTick()
    {
        EntityManager.Clear();
        var entity = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-player-input") });
        entity.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
        entity.Data.Set(MovementDataKeys.MoveSpeed, 12f);
        entity.Data.Set(MovementDataKeys.InputDirection, new Vector2Value(1f, 0f));

        var movement = new MovementSystem();
        movement.Start(entity, new MovementParams
        {
            Mode = MoveMode.PlayerInput,
            MaxDuration = 1f
        });

        movement.Tick(0.5f);
        AssertNear("player input x", 6f, entity.Data.Get<Vector2Value>(MovementDataKeys.Position).X);
        AssertEqual("player input last direction", new Vector2Value(1f, 0f), entity.Data.Get<Vector2Value>(MovementDataKeys.LastMoveDirection));

        EntityManager.Clear();
    }

    static void TestMovementAIControlledTick()
    {
        EntityManager.Clear();
        var entity = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-ai") });
        entity.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
        entity.Data.Set(MovementDataKeys.MoveSpeed, 20f);
        entity.Data.Set(MovementDataKeys.AIMoveDirection, new Vector2Value(0f, 1f));
        entity.Data.Set(MovementDataKeys.AIMoveSpeedMultiplier, 0.5f);

        var movement = new MovementSystem();
        movement.Start(entity, new MovementParams
        {
            Mode = MoveMode.AIControlled,
            MaxDuration = 1f
        });

        movement.Tick(0.5f);
        AssertNear("ai input y", 5f, entity.Data.Get<Vector2Value>(MovementDataKeys.Position).Y);

        EntityManager.Clear();
    }

    static void TestMovementParabolaTick()
    {
        EntityManager.Clear();
        var entity = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-parabola") });
        entity.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);

        var movement = new MovementSystem();
        movement.Start(entity, new MovementParams
        {
            Mode = MoveMode.Parabola,
            TargetPosition = new Vector2Value(10f, 0f),
            MaxDuration = 1f,
            ParabolaApexHeight = -5f
        });

        movement.Tick(0.5f);
        var midpoint = entity.Data.Get<Vector2Value>(MovementDataKeys.Position);
        AssertEqual("parabola still moving", true, movement.IsMoving(entity));
        AssertNear("parabola mid x", 5f, midpoint.X);
        AssertNear("parabola mid y", -5f, midpoint.Y);

        movement.Tick(0.5f);
        var endpoint = entity.Data.Get<Vector2Value>(MovementDataKeys.Position);
        AssertEqual("parabola stopped", false, movement.IsMoving(entity));
        AssertNear("parabola end x", 10f, endpoint.X);
        AssertNear("parabola end y", 0f, endpoint.Y);

        EntityManager.Clear();
    }

    static void TestMovementCircularArcTick()
    {
        EntityManager.Clear();
        var entity = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-circular-arc") });
        entity.Data.Set(MovementDataKeys.Position, new Vector2Value(10f, 0f));

        var movement = new MovementSystem();
        movement.Start(entity, new MovementParams
        {
            Mode = MoveMode.CircularArc,
            TargetPosition = new Vector2Value(-10f, 0f),
            CircularArcRadius = 10f,
            CircularArcClockwise = true,
            MaxDuration = 1f
        });

        movement.Tick(0.5f);
        var midpoint = entity.Data.Get<Vector2Value>(MovementDataKeys.Position);
        AssertEqual("arc still moving", true, movement.IsMoving(entity));
        AssertNear("arc mid x", 0f, midpoint.X);
        AssertNear("arc mid y", 10f, midpoint.Y);

        movement.Tick(0.5f);
        var endpoint = entity.Data.Get<Vector2Value>(MovementDataKeys.Position);
        AssertEqual("arc stopped", false, movement.IsMoving(entity));
        AssertNear("arc end x", -10f, endpoint.X);
        AssertNear("arc end y", 0f, endpoint.Y);

        EntityManager.Clear();
    }

    static void TestMovementOrientationParamsDefaults()
    {
        var orientation = new OrientationParams();
        AssertEqual("orientation mode default", OrientationMode.FollowMovement, orientation.Mode);
        AssertNear("orientation angular speed default", 0f, orientation.AngularSpeed);
        AssertNear("orientation total angle default", -1f, orientation.TotalAngle);
        AssertEqual("orientation clockwise default", true, orientation.IsClockwise);

        var movementParams = new MovementParams();
        AssertEqual("movement orientation default", null, movementParams.Orientation);
    }

    static void TestMovementCollisionStopTick()
    {
        EntityManager.Clear();
        var projectile = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-collision-projectile") });
        projectile.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
        projectile.Data.Set(CollisionDataKeys.CollisionLayer, CollisionLayers.Projectile);
        projectile.Data.Set(CollisionDataKeys.CollisionMask, CollisionLayers.EnemyHurtbox);
        projectile.Data.Set(CollisionDataKeys.CollisionRadius, 1f);
        projectile.Data.Set(CollisionDataKeys.Team, 1);

        var enemy = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-collision-enemy") });
        enemy.Data.Set(MovementDataKeys.Position, new Vector2Value(10f, 0f));
        enemy.Data.Set(CollisionDataKeys.CollisionLayer, CollisionLayers.EnemyHurtbox);
        enemy.Data.Set(CollisionDataKeys.CollisionRadius, 1f);
        enemy.Data.Set(CollisionDataKeys.Team, 2);

        var sameTeam = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-collision-same-team") });
        sameTeam.Data.Set(MovementDataKeys.Position, new Vector2Value(4f, 0f));
        sameTeam.Data.Set(CollisionDataKeys.CollisionLayer, CollisionLayers.EnemyHurtbox);
        sameTeam.Data.Set(CollisionDataKeys.CollisionRadius, 1f);
        sameTeam.Data.Set(CollisionDataKeys.Team, 1);

        var collisionEvents = 0;
        var stoppedEvents = 0;
        MovementCollisionContext? receivedCollision = null;
        MovementStopReason? stopReason = null;
        projectile.Events.Subscribe<MovementEvents.Collision>(data =>
        {
            collisionEvents++;
            receivedCollision = data.Context;
        });
        projectile.Events.Subscribe<MovementEvents.Stopped>(data =>
        {
            stoppedEvents++;
            stopReason = data.Context.Reason;
        });

        var movement = new MovementSystem();
        movement.Start(projectile, new MovementParams
        {
            Mode = MoveMode.Charge,
            Direction = new Vector2Value(1f, 0f),
            Speed = 20f,
            CollisionParams = new MovementCollisionParams
            {
                FilterPolicy = new CollisionFilterPolicy(IgnoreSameTeam: true),
                StopAfterCollisionCount = 1
            }
        });

        movement.Tick(1f);

        AssertEqual("collision stopped", false, movement.IsMoving(projectile));
        AssertEqual("collision event count", 1, collisionEvents);
        AssertEqual("collision stopped count", 1, stoppedEvents);
        AssertEqual("collision stop reason", MovementStopReason.Collision, stopReason);
        AssertEqual("collision target enemy", enemy.EntityId, receivedCollision?.Target.EntityId);
        AssertNear("collision contact x", 8f, projectile.Data.Get<Vector2Value>(MovementDataKeys.Position).X);

        EntityManager.Clear();
    }

    static void TestMovementCollisionTargetQueryInjection()
    {
        EntityManager.Clear();
        var projectile = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-collision-query-projectile") });
        projectile.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
        projectile.Data.Set(CollisionDataKeys.CollisionLayer, CollisionLayers.Projectile);
        projectile.Data.Set(CollisionDataKeys.CollisionMask, CollisionLayers.EnemyHurtbox);
        projectile.Data.Set(CollisionDataKeys.CollisionRadius, 1f);

        var ignoredNearTarget = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-collision-query-ignored") });
        ignoredNearTarget.Data.Set(MovementDataKeys.Position, new Vector2Value(4f, 0f));
        ignoredNearTarget.Data.Set(CollisionDataKeys.CollisionLayer, CollisionLayers.EnemyHurtbox);
        ignoredNearTarget.Data.Set(CollisionDataKeys.CollisionRadius, 1f);

        var queryTarget = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-collision-query-target") });
        queryTarget.Data.Set(MovementDataKeys.Position, new Vector2Value(10f, 0f));
        queryTarget.Data.Set(CollisionDataKeys.CollisionLayer, CollisionLayers.EnemyHurtbox);
        queryTarget.Data.Set(CollisionDataKeys.CollisionRadius, 1f);

        var query = new FixedMovementCollisionTargetQuery(queryTarget);
        var movement = new MovementSystem(query);
        movement.Start(projectile, new MovementParams
        {
            Mode = MoveMode.Charge,
            Direction = new Vector2Value(1f, 0f),
            Speed = 20f,
            CollisionParams = new MovementCollisionParams
            {
                StopAfterCollisionCount = 1
            }
        });

        movement.Tick(1f);

        AssertEqual("query called", 1, query.Calls);
        AssertEqual("query collision stopped", false, movement.IsMoving(projectile));
        AssertNear("query selected target contact", 8f, projectile.Data.Get<Vector2Value>(MovementDataKeys.Position).X);

        EntityManager.Clear();
    }

    static void TestMovementCollisionNotifyWithoutStop()
    {
        EntityManager.Clear();
        var projectile = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-collision-notify") });
        projectile.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
        projectile.Data.Set(CollisionDataKeys.CollisionLayer, CollisionLayers.Projectile);
        projectile.Data.Set(CollisionDataKeys.CollisionMask, CollisionLayers.EnemyHurtbox);
        projectile.Data.Set(CollisionDataKeys.CollisionRadius, 1f);

        var target = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-collision-notify-target") });
        target.Data.Set(MovementDataKeys.Position, new Vector2Value(5f, 0f));
        target.Data.Set(CollisionDataKeys.CollisionLayer, CollisionLayers.EnemyHurtbox);
        target.Data.Set(CollisionDataKeys.CollisionRadius, 1f);

        var collisionEvents = 0;
        projectile.Events.Subscribe<MovementEvents.Collision>(_ => collisionEvents++);

        var movement = new MovementSystem();
        movement.Start(projectile, new MovementParams
        {
            Mode = MoveMode.Charge,
            Direction = new Vector2Value(1f, 0f),
            Speed = 10f,
            CollisionParams = new MovementCollisionParams
            {
                StopAfterCollisionCount = -1
            }
        });

        movement.Tick(1f);
        movement.Tick(1f);

        AssertEqual("notify still moving", true, movement.IsMoving(projectile));
        AssertEqual("notify dedup count", 1, collisionEvents);
        AssertNear("notify reaches through target", 20f, projectile.Data.Get<Vector2Value>(MovementDataKeys.Position).X);

        EntityManager.Clear();
    }

    sealed class FixedMovementCollisionTargetQuery : IMovementCollisionTargetQuery
    {
        private readonly IReadOnlyList<IEntity> candidates;

        public FixedMovementCollisionTargetQuery(params IEntity[] candidates)
        {
            this.candidates = candidates;
        }

        public int Calls { get; private set; }

        public IReadOnlyList<IEntity> GetCandidates(
            IEntity source,
            in MovementParams movementParams,
            Vector2Value from,
            Vector2Value intended)
        {
            Calls++;
            return candidates;
        }
    }
}
