using Godot;
using SlimeAI.GameOS.Capabilities.Effect;
using SlimeAI.GameOS.Capabilities.Effect.Events;
using SlimeAI.GameOS.Capabilities.Movement;
using SlimeAI.GameOS.GodotBridge;
using SlimeAI.GameOS.Observation;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;
using SlimeAI.GameOS.Runtime.Pool;
using SlimeAI.GameOS.Runtime.Timer;

namespace SlimeAI.SceneTests.GameOS.Capabilities.Effect;

public partial class EffectCapabilityValidationScene : Node
{
    private const string ScenePath = "res://SlimeAI/Src/Validation/GameOS/Capabilities/Effect/EffectCapabilityValidation.tscn";
    private const string PassMarker = "GameOS Capability Effect validation PASS";
    private const string FailMarker = "GameOS Capability Effect validation FAIL";

    public override void _Ready()
    {
        CapabilityValidationSupport.Run(
            this,
            ScenePath,
            "EffectCapabilityValidation",
            "GameOS/Capabilities/Effect",
            "effect-capability-validation.json",
            PassMarker,
            FailMarker,
            new[] { "SlimeAI.GameOS.Capabilities.Effect", "SlimeAI.GameOS.Capabilities.Movement" },
            new[] { "ScenePath is treated as data only; the validation scene does not load game assets." },
            new[] { "source, ability and target entities", "EffectSpawnOptions", "effect spawned event subscriber" },
            new[]
            {
                "EffectTool.Spawn writes source, ability, target, path, name, position and duration data",
                "source SpawnedEffectIds owner list includes the spawned effect",
                "Effect Spawned event payload points to source/target/ability",
                "visual path remains a string data contract",
                "finite visual effects destroy runtime entity and return their Godot node to pool"
            },
            new[] { $"stdout contains {PassMarker}", "artifact status is pass", "failureReasons is empty" },
            new[] { $"stdout contains {FailMarker}", "any Effect check fails", "artifact standard-answer fields are missing" },
            validation =>
            {
                validation.Check("spawn_data_and_owner_list", "EffectSpawn", ValidateSpawnDataAndOwnerList);
                validation.Check("spawn_event_payload", "EffectEvents", ValidateSpawnEventPayload);
                validation.Check("visual_path_as_data", "EffectVisualBoundary", ValidateVisualPathAsData);
                validation.Check("finite_visual_lifecycle_pool_cleanup", "EffectVisualLifecycle", () => ValidateFiniteVisualLifecycle(this));
            });
    }

    private static CheckResult ValidateSpawnDataAndOwnerList()
    {
        EntityManager.Clear();
        var source = CapabilityValidationSupport.Spawn("effect-scene-source");
        var ability = CapabilityValidationSupport.Spawn("effect-scene-ability");
        var target = CapabilityValidationSupport.Spawn("effect-scene-target");
        target.Data.Set(MovementDataKeys.Position, new Vector2Value(7f, 8f));

        var result = EffectTool.Spawn(new EffectSpawnOptions
        {
            Source = source,
            Ability = ability,
            Target = target,
            EntityId = new EntityId("effect-scene-effect"),
            ScenePath = "res://does-not-load/effect.tscn",
            Name = "scene-effect",
            AnimationName = "pulse",
            Position = new Vector2Value(1f, 2f),
            Duration = 0.5f
        });
        var effect = result.Effect;
        var ownerList = source.Data.Get(EffectDataKeys.SpawnedEffectIds);
        var position = effect.Data.Get<Vector2Value>(EffectDataKeys.Position, Vector2Value.Zero);
        var success = result.Created
            && effect.Data.Get<EntityId?>(EffectDataKeys.SourceEntity, null) == source.EntityId
            && effect.Data.Get<EntityId?>(EffectDataKeys.AbilityEntity, null) == ability.EntityId
            && effect.Data.Get<EntityId?>(EffectDataKeys.TargetEntity, null) == target.EntityId
            && ownerList.Contains(effect.EntityId)
            && CapabilityValidationSupport.Approximately(position.X, 7f)
            && CapabilityValidationSupport.Approximately(effect.Data.Get<float>(EffectDataKeys.Duration, 0f), 0.5f);

        return CheckResult.From(success, success ? "effect spawn data and owner list written" : "effect spawn data mismatch", CapabilityValidationSupport.Details(
            ("created", result.Created),
            ("ownerListCount", ownerList.Count),
            ("positionX", position.X),
            ("duration", effect.Data.Get<float>(EffectDataKeys.Duration, 0f))));
    }

    private static CheckResult ValidateSpawnEventPayload()
    {
        EntityManager.Clear();
        WorldEvents.World.Clear();
        var source = CapabilityValidationSupport.Spawn("effect-scene-event-source");
        var target = CapabilityValidationSupport.Spawn("effect-scene-event-target");
        var effectId = new EntityId("effect-scene-event-effect");
        var seen = 0;
        EntityId? eventEffect = null;
        using var token = WorldEvents.World.Subscribe<Spawned>(data =>
        {
            seen++;
            eventEffect = data.Effect.EntityId;
        });

        var result = EffectTool.Spawn(new EffectSpawnOptions { Source = source, Target = target, EntityId = effectId, Name = "event-effect" });
        var success = result.Created && seen == 1 && eventEffect == effectId;

        return CheckResult.From(success, success ? "effect spawned event payload emitted" : "effect event payload mismatch", CapabilityValidationSupport.Details(
            ("seen", seen),
            ("eventEffect", eventEffect?.Value),
            ("expected", effectId.Value)));
    }

    private static CheckResult ValidateVisualPathAsData()
    {
        EntityManager.Clear();
        var source = CapabilityValidationSupport.Spawn("effect-scene-path-source");
        var result = EffectTool.Spawn(new EffectSpawnOptions
        {
            Source = source,
            EntityId = new EntityId("effect-scene-path-effect"),
            ScenePath = "res://GameSpecific/Visuals/Effect.tscn",
            AnimationName = "impact"
        });
        var path = result.Effect.Data.Get(EffectDataKeys.ScenePath, string.Empty);
        var animation = result.Effect.Data.Get(EffectDataKeys.AnimationName, string.Empty);
        var success = path == "res://GameSpecific/Visuals/Effect.tscn" && animation == "impact";

        return CheckResult.From(success, success ? "visual path stayed as data" : "effect visual path mismatch", CapabilityValidationSupport.Details(
            ("scenePath", path),
            ("animation", animation)));
    }

    private static CheckResult ValidateFiniteVisualLifecycle(Node owner)
    {
        EntityManager.Clear();
        WorldEvents.World.Clear();
        GodotNodeRegistry.Clear();
        ObjectPoolManager.DestroyAll();

        var spawner = new GodotProjectileEffectSpawner
        {
            Name = "EffectVisualLifecycleSpawner",
            AutoSubscribe = false
        };
        owner.AddChild(spawner);
        spawner.Subscribe();

        var source = CapabilityValidationSupport.Spawn("effect-scene-visual-source");
        var effectId = new EntityId("effect-scene-visual-effect");
        var result = EffectTool.Spawn(new EffectSpawnOptions
        {
            Source = source,
            EntityId = effectId,
            ScenePath = "res://SlimeAI/Src/Validation/GameOS/Capabilities/Effect/EffectVisualProbe.tscn",
            Name = "visual-effect",
            Position = new Vector2Value(3f, 4f),
            Duration = 0.1f
        });

        var node = GodotNodeRegistry.GetNodeById(effectId.Value);
        var poolName = node != null && node.HasMeta("GameOSVisualPoolName")
            ? node.GetMeta("GameOSVisualPoolName").AsString()
            : string.Empty;
        TimerManager.Instance.Tick(0.11f, 0.11f);

        var destroyed = EntityManager.Get(effectId) == null;
        var registryCleared = GodotNodeRegistry.GetNodeById(effectId.Value) == null;
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

        return CheckResult.From(success, success ? "finite effect visual returned to pool" : "finite effect visual lifecycle mismatch", CapabilityValidationSupport.Details(
            ("created", result.Created),
            ("nodeRegisteredBeforeCleanup", node != null),
            ("poolName", poolName),
            ("runtimeDestroyed", destroyed),
            ("registryCleared", registryCleared),
            ("poolReturned", poolReturned)));
    }
}
