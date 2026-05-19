using System.Collections.Generic;
using Godot;
using SlimeAI.GameOS.Capabilities.Unit;
using SlimeAI.GameOS.Capabilities.Unit.Events;
using SlimeAI.GameOS.Observation;

namespace SlimeAI.SceneTests.GameOS.Capabilities.Unit;

public partial class UnitCapabilityValidationScene : Node
{
    private const string ScenePath = "res://SlimeAI/Src/Validation/GameOS/Capabilities/Unit/UnitCapabilityValidation.tscn";
    private const string PassMarker = "GameOS Capability Unit validation PASS";
    private const string FailMarker = "GameOS Capability Unit validation FAIL";

    public override void _Ready()
    {
        CapabilityValidationSupport.Run(
            this,
            ScenePath,
            "UnitCapabilityValidation",
            "GameOS/Capabilities/Unit",
            "unit-capability-validation.json",
            PassMarker,
            FailMarker,
            new[] { "SlimeAI.GameOS.Capabilities.Unit" },
            new[] { "No HUD, AnimatedSprite2D, or game visual scene is required by Unit owner validation." },
            new[] { "unit Runtime entity", "UnitDataKeys registration", "animation request event subscriptions" },
            new[]
            {
                "UnitDataKeys register and roundtrip metadata",
                "AvailableAnimations stores runtime animation names as data",
                "Play/Stop/Finished animation events carry unit payloads without requiring visual nodes"
            },
            new[] { $"stdout contains {PassMarker}", "artifact status is pass", "failureReasons is empty" },
            new[] { $"stdout contains {FailMarker}", "any Unit check fails", "artifact standard-answer fields are missing" },
            validation =>
            {
                validation.Check("data_key_registration_metadata_roundtrip", "UnitData", ValidateDataKeyRegistrationMetadataRoundtrip);
                validation.Check("available_animations_roundtrip", "UnitData", ValidateAvailableAnimationsRoundtrip);
                validation.Check("animation_request_events", "UnitEvents", ValidateAnimationRequestEvents);
            });
    }

    private static CheckResult ValidateDataKeyRegistrationMetadataRoundtrip()
    {
        UnitDataKeys.RegisterAll();
        var unit = CapabilityValidationSupport.Spawn("unit-scene-data");
        unit.Data.Set(UnitDataKeys.Name, "Scene Unit");
        unit.Data.Set(UnitDataKeys.EntityType, "Enemy");
        unit.Data.Set(UnitDataKeys.VisualScenePath, "res://visual-path-is-data-only.tscn");
        unit.Data.Set(UnitDataKeys.HealthBarHeight, 24f);
        unit.Data.Set(UnitDataKeys.IsShowHealthBar, false);

        var success = unit.Data.Get(UnitDataKeys.Name, string.Empty) == "Scene Unit"
            && unit.Data.Get(UnitDataKeys.EntityType, string.Empty) == "Enemy"
            && unit.Data.Get(UnitDataKeys.VisualScenePath, string.Empty) == "res://visual-path-is-data-only.tscn"
            && CapabilityValidationSupport.Approximately(unit.Data.Get<float>(UnitDataKeys.HealthBarHeight, 0f), 24f)
            && !unit.Data.Get<bool>(UnitDataKeys.IsShowHealthBar, true);

        return CheckResult.From(success, success ? "Unit metadata roundtrip passed" : "Unit metadata mismatch", CapabilityValidationSupport.Details(
            ("name", unit.Data.Get(UnitDataKeys.Name, string.Empty)),
            ("entityType", unit.Data.Get(UnitDataKeys.EntityType, string.Empty)),
            ("visualScenePath", unit.Data.Get(UnitDataKeys.VisualScenePath, string.Empty)),
            ("healthBarHeight", unit.Data.Get<float>(UnitDataKeys.HealthBarHeight, 0f)),
            ("isShowHealthBar", unit.Data.Get<bool>(UnitDataKeys.IsShowHealthBar, true))));
    }

    private static CheckResult ValidateAvailableAnimationsRoundtrip()
    {
        var unit = CapabilityValidationSupport.Spawn("unit-scene-animations");
        unit.Data.Set(UnitDataKeys.AvailableAnimations, new List<string> { "idle", "attack" });
        var animations = unit.Data.Get(UnitDataKeys.AvailableAnimations, new List<string>());
        var success = animations.Count == 2 && animations[0] == "idle" && animations[1] == "attack";

        return CheckResult.From(success, success ? "available animations stored as data" : "available animations mismatch", CapabilityValidationSupport.Details(
            ("count", animations.Count),
            ("first", animations.Count > 0 ? animations[0] : null),
            ("second", animations.Count > 1 ? animations[1] : null)));
    }

    private static CheckResult ValidateAnimationRequestEvents()
    {
        var unit = CapabilityValidationSupport.Spawn("unit-scene-events");
        var play = 0;
        var stop = 0;
        var finished = 0;
        using var playToken = unit.Events.Subscribe<PlayAnimationRequested>(data => { if (data.AnimationName == "attack") play++; });
        using var stopToken = unit.Events.Subscribe<StopAnimationRequested>(data => { if (data.Entity.EntityId == unit.EntityId) stop++; });
        using var finishedToken = unit.Events.Subscribe<AnimationFinished>(data => { if (data.AnimationName == "attack") finished++; });

        unit.Events.Publish(new PlayAnimationRequested(unit, "attack", ForceRestart: true, Duration: 0.25f));
        unit.Events.Publish(new StopAnimationRequested(unit));
        unit.Events.Publish(new AnimationFinished(unit, "attack"));
        var success = play == 1 && stop == 1 && finished == 1;

        return CheckResult.From(success, success ? "Unit animation event payloads passed" : "Unit animation event mismatch", CapabilityValidationSupport.Details(
            ("play", play),
            ("stop", stop),
            ("finished", finished)));
    }
}
