using System;
using System.Collections.Generic;
using System.IO;
using Godot;
using SlimeAI.GameOS.Observation;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Events.Core;
using SlimeAI.GameOS.Runtime.World;

namespace SlimeAI.SceneTests.Runtime.Entity;

/// <summary>
/// Runtime/Entity 的 Godot headless 验证场景。
/// </summary>
public partial class RuntimeEntityValidationScene : Node
{
    private const string ScenePath = "res://SlimeAI/Scenes/Validation/Runtime/Entity/RuntimeEntityValidation.tscn";
    private const string ArtifactFileName = "runtime-entity-validation.json";
    private const string LogContext = "RuntimeEntityValidation";

    /// <inheritdoc />
    public override void _Ready()
    {
        using var observation = GameOSObservationSession.FromEnvironment(
            ScenePath,
            "validation",
            Path.Combine(Directory.GetCurrentDirectory(), ".ai-temp", "scene-tests", "manual", "artifacts"));
        using var validation = new SceneValidationSession(
            observation,
            LogContext,
            "Runtime/Entity",
            ArtifactFileName,
            new[]
            {
                "SlimeAI.GameOS.Runtime.Entity",
                "SlimeAI.GameOS.Runtime.World",
                "SlimeAI.GameOS.Runtime.Events.Core",
                "Games/BrotatoLike Godot scene runner"
            },
            new[]
            {
                "This scene validates typed EntityId and Entity lifecycle behavior.",
                "Lifecycle parent-child semantics are covered by Runtime/Lifecycle validation."
            },
            expectedInputs: new[]
            {
                "EntityId.Empty, default(EntityId), EntityId.From(null), EntityId.From(\"\")",
                "RuntimeWorld.CreateScoped() with custom and generated entity spawn",
                "EntitySpawned and EntityDestroyed subscriptions on the scoped world"
            },
            expectedObservations: new[]
            {
                "empty/default/null/empty-string entity ids are empty and equal",
                "custom spawn keeps runtime-entity-custom while generated spawn is a 32-char Guid N value",
                "spawn and destroy events carry typed EntityId payloads matching the entity"
            },
            passCriteria: new[]
            {
                "all checks have status=pass",
                "stdout contains GameOS Runtime Entity validation PASS",
                "failureReasons is empty"
            },
            failCriteria: new[]
            {
                "any check has status=fail",
                "stdout contains GameOS Runtime Entity validation FAIL",
                "failureReasons contains the failed check name and reason"
            });

        validation.Info("validation start");
        validation.Check("entity_id_empty_and_boundary", "EntityId", ValidateEntityIdEmptyAndBoundary);
        validation.Check("spawn_custom_and_generated_id", "EntityRegistry", ValidateSpawnCustomAndGeneratedId);
        validation.Check("entity_events_typed_payload", "EntityEvents", ValidateEntityEventsTypedPayload);

        var success = validation.Success;
        if (success)
        {
            validation.Pass("all checks passed");
        }
        else
        {
            validation.Fail($"{validation.FailureReasons.Count} checks failed");
        }

        validation.WriteArtifact();
        GD.Print(success ? "GameOS Runtime Entity validation PASS" : "GameOS Runtime Entity validation FAIL");
        if (!success)
        {
            GD.Print($"GameOS Runtime Entity validation failures: {string.Join("; ", validation.FailureReasons)}");
        }

        GetTree().Quit(success ? 0 : 1);
    }

    private static CheckResult ValidateEntityIdEmptyAndBoundary()
    {
        var defaultId = default(EntityId);
        var fromNull = EntityId.From(null);
        var fromEmpty = EntityId.From(string.Empty);
        var custom = new EntityId("runtime-entity-validation");

        var success = defaultId.Equals(EntityId.Empty)
            && fromNull.Equals(EntityId.Empty)
            && fromEmpty.Equals(EntityId.Empty)
            && defaultId.IsEmpty
            && fromNull.IsEmpty
            && !custom.IsEmpty
            && custom.Value == "runtime-entity-validation"
            && custom.ToString() == "runtime-entity-validation";

        return CheckResult.From(success, success ? "typed EntityId boundary passed" : "typed EntityId boundary mismatch", new Dictionary<string, object?>
        {
            ["defaultIsEmpty"] = defaultId.IsEmpty,
            ["fromNullIsEmpty"] = fromNull.IsEmpty,
            ["fromEmptyIsEmpty"] = fromEmpty.IsEmpty,
            ["customValue"] = custom.Value,
            ["customToString"] = custom.ToString()
        });
    }

    private static CheckResult ValidateSpawnCustomAndGeneratedId()
    {
        using var world = RuntimeWorld.CreateScoped();

        var custom = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("runtime-entity-custom") });
        var generated = world.Entities.Spawn(new EntitySpawnConfig());

        var success = custom.EntityId == new EntityId("runtime-entity-custom")
            && world.Entities.Get(custom.EntityId)?.EntityId == custom.EntityId
            && !generated.EntityId.IsEmpty
            && generated.EntityId.Value.Length == 32
            && Guid.TryParseExact(generated.EntityId.Value, "N", out _)
            && world.Entities.Get(generated.EntityId)?.EntityId == generated.EntityId;

        return CheckResult.From(success, success ? "custom and generated spawn ids passed" : "spawn id mismatch", new Dictionary<string, object?>
        {
            ["customId"] = custom.EntityId.Value,
            ["generatedId"] = generated.EntityId.Value,
            ["generatedLength"] = generated.EntityId.Value.Length
        });
    }

    private static CheckResult ValidateEntityEventsTypedPayload()
    {
        using var world = RuntimeWorld.CreateScoped();
        EntityId spawnedId = EntityId.Empty;
        EntityId destroyedId = EntityId.Empty;
        var spawnedCount = 0;
        var destroyedCount = 0;

        using var spawnSub = world.Events.Subscribe<EntitySpawned>(data =>
        {
            spawnedCount++;
            spawnedId = data.Entity.EntityId;
        });
        using var destroySub = world.Events.Subscribe<EntityDestroyed>(data =>
        {
            destroyedCount++;
            destroyedId = data.Entity.EntityId;
        });

        var entity = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("runtime-entity-event") });
        var destroyed = world.Entities.Destroy(entity.EntityId);

        var success = destroyed
            && spawnedCount == 1
            && destroyedCount == 1
            && spawnedId == entity.EntityId
            && destroyedId == entity.EntityId
            && world.Entities.Get(entity.EntityId) == null;

        return CheckResult.From(success, success ? "typed lifecycle event payloads passed" : "typed lifecycle event payload mismatch", new Dictionary<string, object?>
        {
            ["spawnedCount"] = spawnedCount,
            ["destroyedCount"] = destroyedCount,
            ["spawnedId"] = spawnedId.Value,
            ["destroyedId"] = destroyedId.Value,
            ["destroyed"] = destroyed
        });
    }
}
