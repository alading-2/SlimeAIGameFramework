using Godot;
using SlimeAI.GameOS.Capabilities.Collision;
using SlimeAI.GameOS.Capabilities.Movement;
using MovementEvents = SlimeAI.GameOS.Capabilities.Movement.Events;
using SlimeAI.GameOS.Observation;

namespace SlimeAI.SceneTests.GameOS.Capabilities.Movement;

public partial class MovementCapabilityValidationScene : Node
{
    private const string ScenePath = "res://SlimeAI/Src/Validation/GameOS/Capabilities/Movement/MovementCapabilityValidation.tscn";
    private const string PassMarker = "GameOS Capability Movement validation PASS";
    private const string FailMarker = "GameOS Capability Movement validation FAIL";

    public override void _Ready()
    {
        CapabilityValidationSupport.Run(
            this,
            ScenePath,
            "MovementCapabilityValidation",
            "GameOS/Capabilities/Movement",
            "movement-capability-validation.json",
            PassMarker,
            FailMarker,
            new[] { "SlimeAI.GameOS.Capabilities.Movement", "SlimeAI.GameOS.Capabilities.Collision" },
            new[] { "Collision candidates are supplied by injected IMovementCollisionTargetQuery." },
            new[] { "moving entities", "MovementParams", "active movement state", "collision target query", "stop event subscribers" },
            new[]
            {
                "deterministic Charge tick writes position and velocity",
                "only started entities are advanced by MovementSystem.Tick",
                "collision target query is injected and emits collision event",
                "Stop publishes Stopped with stop reason"
            },
            new[] { $"stdout contains {PassMarker}", "artifact status is pass", "failureReasons is empty" },
            new[] { $"stdout contains {FailMarker}", "any Movement check fails", "artifact standard-answer fields are missing" },
            validation =>
            {
                validation.Check("deterministic_charge_tick", "MovementTick", ValidateDeterministicChargeTick);
                validation.Check("active_movement_selector", "MovementSelector", ValidateActiveMovementSelector);
                validation.Check("collision_query_injection", "MovementCollision", ValidateCollisionQueryInjection);
                validation.Check("stop_event_payload", "MovementEvents", ValidateStopEventPayload);
            });
    }

    private static CheckResult ValidateDeterministicChargeTick()
    {
        var entity = CapabilityValidationSupport.Spawn("movement-scene-charge");
        entity.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
        var system = new MovementSystem();
        system.Start(entity, new MovementParams { Mode = MoveMode.Charge, Direction = new Vector2Value(1f, 0f), Speed = 10f, MaxDuration = -1f });
        system.Tick(0.5f);
        var position = entity.Data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero);
        var success = CapabilityValidationSupport.Approximately(position.X, 5f)
            && CapabilityValidationSupport.Approximately(position.Y, 0f)
            && system.IsMoving(entity);

        return CheckResult.From(success, success ? "deterministic charge tick moved entity" : "charge tick mismatch", CapabilityValidationSupport.Details(
            ("positionX", position.X),
            ("positionY", position.Y),
            ("isMoving", system.IsMoving(entity))));
    }

    private static CheckResult ValidateActiveMovementSelector()
    {
        var active = CapabilityValidationSupport.Spawn("movement-scene-active");
        var inactive = CapabilityValidationSupport.Spawn("movement-scene-inactive");
        active.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
        inactive.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
        var system = new MovementSystem();
        system.Start(active, new MovementParams { Mode = MoveMode.Charge, Direction = new Vector2Value(1f, 0f), Speed = 6f });
        system.Tick(0.5f);
        var activePosition = active.Data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero);
        var inactivePosition = inactive.Data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero);
        var success = activePosition.X > 0f
            && CapabilityValidationSupport.Approximately(inactivePosition.X, 0f)
            && !system.IsMoving(inactive);

        return CheckResult.From(success, success ? "MovementSystem advanced active movement only" : "active selector mismatch", CapabilityValidationSupport.Details(
            ("activeX", activePosition.X),
            ("inactiveX", inactivePosition.X),
            ("inactiveMoving", system.IsMoving(inactive))));
    }

    private static CheckResult ValidateCollisionQueryInjection()
    {
        var mover = CapabilityValidationSupport.Spawn("movement-scene-collision-mover");
        var target = CapabilityValidationSupport.Spawn("movement-scene-collision-target");
        mover.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
        mover.Data.Set(CollisionDataKeys.CollisionMask, 0b0010u);
        mover.Data.Set(CollisionDataKeys.CollisionRadius, 0.5f);
        target.Data.Set(MovementDataKeys.Position, new Vector2Value(4f, 0f));
        target.Data.Set(CollisionDataKeys.CollisionLayer, 0b0010u);
        target.Data.Set(CollisionDataKeys.CollisionRadius, 0.5f);
        var query = new FixedMovementCollisionTargetQuery(new[] { target });
        var system = new MovementSystem(query);
        var collisions = 0;
        using var token = mover.Events.Subscribe<MovementEvents.Collision>(_ => collisions++);
        system.Start(mover, new MovementParams
        {
            Mode = MoveMode.Charge,
            Direction = new Vector2Value(1f, 0f),
            Speed = 10f,
            CollisionParams = new MovementCollisionParams { StopAfterCollisionCount = 1 }
        });
        system.Tick(0.5f);
        var success = query.Calls == 1 && collisions == 1 && !system.IsMoving(mover);

        return CheckResult.From(success, success ? "collision query injection emitted hit and stopped" : "movement collision query mismatch", CapabilityValidationSupport.Details(
            ("queryCalls", query.Calls),
            ("collisions", collisions),
            ("isMoving", system.IsMoving(mover))));
    }

    private static CheckResult ValidateStopEventPayload()
    {
        var entity = CapabilityValidationSupport.Spawn("movement-scene-stop");
        var system = new MovementSystem();
        MovementStopReason? reason = null;
        using var token = entity.Events.Subscribe<MovementEvents.Stopped>(data => reason = data.Context.Reason);
        system.Start(entity, new MovementParams { Mode = MoveMode.Charge, Direction = new Vector2Value(1f, 0f), Speed = 1f });
        var stopped = system.Stop(entity, MovementStopReason.External);
        var success = stopped && reason == MovementStopReason.External && !system.IsMoving(entity);

        return CheckResult.From(success, success ? "stop event payload emitted" : "movement stop event mismatch", CapabilityValidationSupport.Details(
            ("stopped", stopped),
            ("reason", reason?.ToString()),
            ("isMoving", system.IsMoving(entity))));
    }
}
