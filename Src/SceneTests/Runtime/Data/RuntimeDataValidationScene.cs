using System.Collections.Generic;
using System.IO;
using Godot;
using SlimeAI.GameOS.Observation;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Events.Core;
using RuntimeData = SlimeAI.GameOS.Runtime.Data.Data;
using RuntimeDataCatalog = SlimeAI.GameOS.Runtime.Data.DataCatalog;
using RuntimeDataKey = SlimeAI.GameOS.Runtime.Data.DataKey;
using RuntimeDataKeyFloat = SlimeAI.GameOS.Runtime.Data.DataKey<float>;
using RuntimeDataKeyInt = SlimeAI.GameOS.Runtime.Data.DataKey<int>;
using RuntimeDataModifier = SlimeAI.GameOS.Runtime.Data.DataModifier;
using RuntimeModifierType = SlimeAI.GameOS.Runtime.Data.ModifierType;

namespace SlimeAI.SceneTests.Runtime.Data;

/// <summary>
/// GameOS Runtime/Data 的 Godot headless 验证场景。
/// </summary>
public partial class RuntimeDataValidationScene : Node
{
    private const string ScenePath = "res://SlimeAI/Scenes/Validation/Runtime/Data/RuntimeDataValidation.tscn";
    private const string ArtifactFileName = "runtime-data-validation.json";
    private const string LogContext = "RuntimeDataValidation";

    private static readonly RuntimeDataKeyFloat Health = RuntimeDataKey.Create(
        "RuntimeDataValidation.Health",
        100f,
        RuntimeDataValidationCategory.Primary,
        minValue: 0f,
        maxValue: 200f,
        supportsModifiers: true);

    private static readonly RuntimeDataKeyFloat Energy = RuntimeDataKey.Create(
        "RuntimeDataValidation.Energy",
        20f,
        RuntimeDataValidationCategory.Primary,
        minValue: 0f,
        maxValue: 100f,
        supportsModifiers: true);

    private static readonly RuntimeDataKeyInt Mode = RuntimeDataKey.Create(
        "RuntimeDataValidation.Mode",
        0,
        RuntimeDataValidationCategory.Secondary,
        options: new[] { "Idle", "Active" });

    private static readonly RuntimeDataKeyFloat Power = RuntimeDataKey.Create(
        "RuntimeDataValidation.Power",
        0f,
        RuntimeDataValidationCategory.Primary,
        dependencies: new[] { Health, Energy },
        compute: data => data.Get(Health) + data.Get(Energy));

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
            "Runtime/Data",
            ArtifactFileName,
            new[]
            {
                "SlimeAI.GameOS.Runtime.Data",
                "SlimeAI.GameOS.Runtime.Entity for Data-to-Event bridge check",
                "Games/BrotatoLike Godot scene runner"
            },
            new[]
            {
                "This scene validates Runtime/Data typed contract only.",
                "DataOS SQLite schema, snapshot generator and validator are covered by Tools/run-dataos-validate.sh."
            },
            expectedInputs: new[]
            {
                "typed DataCatalog with Health, Energy, Mode and computed Power keys",
                "RuntimeData values with clamp, option, modifier and reset operations",
                "RuntimeEntity.Data.Set bridge for DataPropertyChanged event"
            },
            expectedObservations: new[]
            {
                "catalog resolves typed keys and capability metadata",
                "typed set/get/tryget/has/remove/reset and modifier/computed dirty behavior match Runtime/Data contract",
                "entity data change emits DataPropertyChanged with stable key, old value and new value"
            },
            passCriteria: new[]
            {
                "all Runtime/Data checks pass",
                "stdout contains GameOS Runtime Data validation PASS",
                "failureReasons is empty"
            },
            failCriteria: new[]
            {
                "any Runtime/Data lifecycle, clamp, option, modifier, reset or bridge check fails",
                "stdout contains GameOS Runtime Data validation FAIL",
                "failureReasons identifies the failed data invariant"
            });

        validation.Info("validation start");

        validation.Check("catalog_resolve_and_capability_metadata", "RuntimeDataCore", ValidateCatalogResolveAndCapabilityMetadata);
        validation.Check("typed_value_lifecycle", "RuntimeDataCore", ValidateTypedValueLifecycle);
        validation.Check("clamp_and_option_guard", "RuntimeDataCore", ValidateClampAndOptionGuard);
        validation.Check("modifier_and_computed_dirty", "RuntimeDataCore", ValidateModifierAndComputedDirty);
        validation.Check("reset_by_category", "RuntimeDataCore", ValidateResetByCategory);
        validation.Check("entity_data_change_event", "DataToEventBridge", ValidateEntityDataChangeEvent);

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

        GD.Print(success ? "GameOS Runtime Data validation PASS" : "GameOS Runtime Data validation FAIL");
        if (!success)
        {
            GD.Print($"GameOS Runtime Data validation failures: {string.Join("; ", validation.FailureReasons)}");
        }

        GetTree().Quit(success ? 0 : 1);
    }

    private static CheckResult ValidateCatalogResolveAndCapabilityMetadata()
    {
        var catalog = CreateCatalog();
        var resolvesFloat = catalog.TryResolve<float>(Health.StableKey, out var resolvedHealth);
        var resolvesInt = catalog.TryResolve<int>(Mode.StableKey, out var resolvedMode);
        var enabled = catalog.IsCapabilityEnabled("runtime-data-validation");
        var disabled = catalog.IsCapabilityEnabled("runtime-data-disabled");

        var success = resolvesFloat
            && ReferenceEquals(resolvedHealth, Health)
            && resolvesInt
            && ReferenceEquals(resolvedMode, Mode)
            && enabled
            && !disabled;

        return CheckResult.From(success, success ? "catalog resolved typed keys and capability metadata" : "catalog resolve or capability metadata mismatch", new Dictionary<string, object?>
        {
            ["resolvesFloat"] = resolvesFloat,
            ["resolvesInt"] = resolvesInt,
            ["enabledCapability"] = enabled,
            ["disabledCapability"] = disabled,
            ["catalogId"] = catalog.CatalogId
        });
    }

    private static CheckResult ValidateTypedValueLifecycle()
    {
        var data = CreateData();
        var defaultHealth = data.Get(Health);
        var hasBeforeSet = data.Has(Health);
        var tryBeforeSet = data.TryGet(Health, out var tryBeforeValue);
        var setChanged = data.Set(Health, 150f);
        var hasAfterSet = data.Has(Health);
        var tryAfterSet = data.TryGet(Health, out var tryAfterValue);
        var allAfterSet = data.GetAll();
        var removed = data.Remove(Health);
        var healthAfterRemove = data.Get(Health);

        var success = defaultHealth == 100f
            && !hasBeforeSet
            && !tryBeforeSet
            && tryBeforeValue == 100f
            && setChanged
            && hasAfterSet
            && tryAfterSet
            && tryAfterValue == 150f
            && allAfterSet.TryGetValue(Health.StableKey, out var allValue)
            && allValue is float allFloat
            && allFloat == 150f
            && removed
            && healthAfterRemove == 100f
            && !data.Has(Health);

        return CheckResult.From(success, success ? "typed value lifecycle passed" : "typed value lifecycle mismatch", new Dictionary<string, object?>
        {
            ["defaultHealth"] = defaultHealth,
            ["hasBeforeSet"] = hasBeforeSet,
            ["tryBeforeSet"] = tryBeforeSet,
            ["tryBeforeValue"] = tryBeforeValue,
            ["setChanged"] = setChanged,
            ["hasAfterSet"] = hasAfterSet,
            ["tryAfterSet"] = tryAfterSet,
            ["tryAfterValue"] = tryAfterValue,
            ["healthAfterRemove"] = healthAfterRemove
        });
    }

    private static CheckResult ValidateClampAndOptionGuard()
    {
        var data = CreateData();
        var healthChanged = data.Set(Health, 250f);
        var clampedHealth = data.Get(Health);
        var validModeChanged = data.Set(Mode, 1);
        var invalidModeChanged = data.Set(Mode, 2);
        var modeAfterInvalid = data.Get(Mode);

        var success = healthChanged
            && clampedHealth == 200f
            && validModeChanged
            && !invalidModeChanged
            && modeAfterInvalid == 1;

        return CheckResult.From(success, success ? "clamp and option guard passed" : "clamp or option guard mismatch", new Dictionary<string, object?>
        {
            ["clampedHealth"] = clampedHealth,
            ["validModeChanged"] = validModeChanged,
            ["invalidModeChanged"] = invalidModeChanged,
            ["modeAfterInvalid"] = modeAfterInvalid
        });
    }

    private static CheckResult ValidateModifierAndComputedDirty()
    {
        var data = CreateData();
        data.Set(Health, 100f);
        data.Set(Energy, 20f);

        var initialPower = data.Get(Power);
        var additiveAdded = data.AddModifier(Health, new RuntimeDataModifier(RuntimeModifierType.Additive, 50f, id: "runtime-data-validation-add"));
        var multiplicativeAdded = data.AddModifier(Health, new RuntimeDataModifier(RuntimeModifierType.Multiplicative, 2f, id: "runtime-data-validation-mul"));
        var modifiedHealth = data.Get(Health);
        var modifiedPower = data.Get(Power);
        var duplicateRejected = !data.AddModifier(Health, new RuntimeDataModifier(RuntimeModifierType.Additive, 10f, id: "runtime-data-validation-add"));
        var removedModifier = data.RemoveModifier(Health, "runtime-data-validation-add");
        var healthAfterRemove = data.Get(Health);
        var powerAfterRemove = data.Get(Power);

        var success = initialPower == 120f
            && additiveAdded
            && multiplicativeAdded
            && modifiedHealth == 200f
            && modifiedPower == 220f
            && duplicateRejected
            && removedModifier
            && healthAfterRemove == 200f
            && powerAfterRemove == 220f;

        return CheckResult.From(success, success ? "modifier and computed dirty passed" : "modifier or computed dirty mismatch", new Dictionary<string, object?>
        {
            ["initialPower"] = initialPower,
            ["modifiedHealth"] = modifiedHealth,
            ["modifiedPower"] = modifiedPower,
            ["duplicateRejected"] = duplicateRejected,
            ["healthAfterRemove"] = healthAfterRemove,
            ["powerAfterRemove"] = powerAfterRemove
        });
    }

    private static CheckResult ValidateResetByCategory()
    {
        var data = CreateData();
        data.Set(Health, 150f);
        data.Set(Energy, 80f);
        data.Set(Mode, 1);

        data.ResetByCategory(RuntimeDataValidationCategory.Primary);

        var success = data.Get(Health) == 100f
            && data.Get(Energy) == 20f
            && data.Get(Mode) == 1
            && data.Has(Health)
            && data.Has(Energy)
            && data.Has(Mode);

        return CheckResult.From(success, success ? "reset by category passed" : "reset by category mismatch", new Dictionary<string, object?>
        {
            ["health"] = data.Get(Health),
            ["energy"] = data.Get(Energy),
            ["mode"] = data.Get(Mode),
            ["hasHealth"] = data.Has(Health),
            ["hasEnergy"] = data.Has(Energy),
            ["hasMode"] = data.Has(Mode)
        });
    }

    private static CheckResult ValidateEntityDataChangeEvent()
    {
        var entity = new RuntimeEntity(new EntityId("runtime-data-validation-entity"), CreateCatalog());
        var receivedCount = 0;
        string? stableKey = null;
        object? oldValue = null;
        object? newValue = null;

        entity.Events.Subscribe<DataPropertyChanged>(data =>
        {
            receivedCount++;
            stableKey = data.Change.StableKey;
            oldValue = data.Change.OldValue;
            newValue = data.Change.NewValue;
        });

        var firstChanged = entity.Data.Set(Health, 120f);
        var secondChanged = entity.Data.Set(Health, 130f);

        var success = firstChanged
            && secondChanged
            && receivedCount == 2
            && stableKey == Health.StableKey
            && oldValue is float oldFloat
            && oldFloat == 120f
            && newValue is float newFloat
            && newFloat == 130f;

        return CheckResult.From(success, success ? "entity Data.Set emitted DataPropertyChanged" : "entity data change event mismatch", new Dictionary<string, object?>
        {
            ["receivedCount"] = receivedCount,
            ["stableKey"] = stableKey,
            ["oldValue"] = oldValue,
            ["newValue"] = newValue
        });
    }

    private static RuntimeData CreateData()
    {
        return new RuntimeData(CreateCatalog());
    }

    private static RuntimeDataCatalog CreateCatalog()
    {
        return RuntimeDataCatalog.CreateBuilder("runtime-data-validation")
            .Add(Health)
            .Add(Energy)
            .Add(Mode)
            .Add(Power)
            .AddCapability("runtime-data-validation", enabled: true, ownerSkill: "ecs-data")
            .AddCapability("runtime-data-disabled", enabled: false, ownerSkill: "ecs-data")
            .Build();
    }

    private enum RuntimeDataValidationCategory
    {
        Primary,
        Secondary
    }
}
