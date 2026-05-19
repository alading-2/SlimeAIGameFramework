using Godot;
using SlimeAI.GameOS.Capabilities.Feature;
using SlimeAI.GameOS.Capabilities.Movement;
using SlimeAI.GameOS.Observation;
using SlimeAI.GameOS.Runtime.Data;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;
using SlimeAI.GameOS.Runtime.Timer;

namespace SlimeAI.SceneTests.GameOS.Capabilities.Feature;

public partial class FeatureCapabilityValidationScene : Node
{
    private const string ScenePath = "res://SlimeAI/Src/Validation/GameOS/Capabilities/Feature/FeatureCapabilityValidation.tscn";
    private const string PassMarker = "GameOS Capability Feature validation PASS";
    private const string FailMarker = "GameOS Capability Feature validation FAIL";

    public override void _Ready()
    {
        CapabilityValidationSupport.Run(
            this,
            ScenePath,
            "FeatureCapabilityValidation",
            "GameOS/Capabilities/Feature",
            "feature-capability-validation.json",
            PassMarker,
            FailMarker,
            new[] { "SlimeAI.GameOS.Capabilities.Feature", "SlimeAI.GameOS.Runtime.Timer" },
            new[] { "Feature handlers are test doubles registered in FeatureHandlerRegistry." },
            new[] { "owner and feature entities", "modifier entries", "IFeatureAction", "FeatureAutoTriggerService", "typed payload/result" },
            new[]
            {
                "grant/remove applies and clears modifiers",
                "handler lifecycle callbacks and IFeatureAction run",
                "Periodic and OnEvent triggers activate handlers",
                "typed payload/result APIs are used instead of raw object bag"
            },
            new[] { $"stdout contains {PassMarker}", "artifact status is pass", "failureReasons is empty" },
            new[] { $"stdout contains {FailMarker}", "any Feature check fails", "artifact standard-answer fields are missing" },
            validation =>
            {
                validation.Check("modifier_grant_remove", "FeatureModifiers", ValidateModifierGrantRemove);
                validation.Check("handler_lifecycle_and_action", "FeatureLifecycle", ValidateHandlerLifecycleAndAction);
                validation.Check("periodic_and_onevent_triggers", "FeatureAutoTrigger", ValidatePeriodicAndOnEventTriggers);
                validation.Check("typed_payload_and_result", "FeatureTypedContext", ValidateTypedPayloadAndResult);
            });
    }

    private static CheckResult ValidateModifierGrantRemove()
    {
        FeatureHandlerRegistry.Clear();
        var service = new FeatureService();
        var owner = CapabilityValidationSupport.Spawn("feature-scene-mod-owner");
        var feature = CapabilityValidationSupport.Spawn("feature-scene-mod-feature");
        owner.Data.Set(MovementDataKeys.MoveSpeed, 10f);

        service.Grant(owner, feature, new FeatureDefinition
        {
            FeatureId = "scene.feature.modifier",
            Modifiers = new[] { new FeatureModifierEntry(MovementDataKeys.MoveSpeed, ModifierType.Additive, 5f) }
        });
        var grantedSpeed = owner.Data.Get<float>(MovementDataKeys.MoveSpeed, 0f);
        service.Remove(owner, feature);
        var removedSpeed = owner.Data.Get<float>(MovementDataKeys.MoveSpeed, 0f);
        var success = CapabilityValidationSupport.Approximately(grantedSpeed, 15f)
            && CapabilityValidationSupport.Approximately(removedSpeed, 10f);

        return CheckResult.From(success, success ? "modifier grant/remove passed" : "modifier grant/remove mismatch", CapabilityValidationSupport.Details(
            ("grantedSpeed", grantedSpeed),
            ("removedSpeed", removedSpeed)));
    }

    private static CheckResult ValidateHandlerLifecycleAndAction()
    {
        FeatureHandlerRegistry.Clear();
        var service = new FeatureService();
        var handler = new ProbeFeatureHandler("scene.feature.lifecycle");
        var action = new CountingFeatureAction();
        FeatureHandlerRegistry.Register(handler);
        var owner = CapabilityValidationSupport.Spawn("feature-scene-life-owner");
        var feature = CapabilityValidationSupport.Spawn("feature-scene-life-feature");
        var definition = new FeatureDefinition { FeatureId = "scene.feature.lifecycle", HandlerId = handler.FeatureId, Actions = new[] { action } };

        var context = service.Grant(owner, feature, definition);
        service.Enable(owner, feature);
        service.Disable(owner, feature);
        service.Activate(context);
        service.End(context);
        service.Remove(owner, feature);
        var success = action.Count == 1
            && handler.Granted == 1
            && handler.Enabled == 1
            && handler.Disabled == 1
            && handler.Activated == 1
            && handler.Executed == 1
            && handler.Ended == 1
            && handler.Removed == 1;

        return CheckResult.From(success, success ? "handler lifecycle and action passed" : "handler lifecycle/action mismatch", CapabilityValidationSupport.Details(
            ("actionCount", action.Count),
            ("granted", handler.Granted),
            ("enabled", handler.Enabled),
            ("disabled", handler.Disabled),
            ("activated", handler.Activated),
            ("executed", handler.Executed),
            ("ended", handler.Ended),
            ("removed", handler.Removed)));
    }

    private static CheckResult ValidatePeriodicAndOnEventTriggers()
    {
        FeatureHandlerRegistry.Clear();
        var timer = new TimerManager("feature-scene-auto");
        var service = new FeatureService();
        var autoTrigger = new FeatureAutoTriggerService(service, timer, () => 0f);
        var periodicHandler = new ProbeFeatureHandler("scene.feature.periodic");
        var eventHandler = new ProbeFeatureHandler("scene.feature.event");
        FeatureHandlerRegistry.Register(periodicHandler);
        FeatureHandlerRegistry.Register(eventHandler);
        var owner = CapabilityValidationSupport.Spawn("feature-scene-auto-owner");
        var periodic = CapabilityValidationSupport.Spawn("feature-scene-periodic");
        var onEvent = CapabilityValidationSupport.Spawn("feature-scene-event");
        periodic.Data.Set(FeatureDataKeys.IsEnabled, true);
        periodic.Data.Set(FeatureDataKeys.Cooldown, 0.2f);
        onEvent.Data.Set(FeatureDataKeys.IsEnabled, true);
        onEvent.Data.Set(FeatureDataKeys.TriggerChance, 100f);

        using var periodicRegistration = autoTrigger.RegisterPeriodic(owner, periodic, new FeatureDefinition { FeatureId = periodicHandler.FeatureId, HandlerId = periodicHandler.FeatureId });
        using var eventRegistration = autoTrigger.RegisterOnEvent<SceneFeatureEvent>(owner, onEvent, new FeatureDefinition { FeatureId = eventHandler.FeatureId, HandlerId = eventHandler.FeatureId }, owner.Events);
        timer.Tick(0.2f);
        owner.Events.Publish(new SceneFeatureEvent(3));
        var success = periodicHandler.Activated == 1
            && eventHandler.Activated == 1
            && eventHandler.SawTypedPayload;

        timer.Clear();
        return CheckResult.From(success, success ? "periodic and on-event triggers passed" : "feature auto trigger mismatch", CapabilityValidationSupport.Details(
            ("periodicActivated", periodicHandler.Activated),
            ("eventActivated", eventHandler.Activated),
            ("eventSawTypedPayload", eventHandler.SawTypedPayload)));
    }

    private static CheckResult ValidateTypedPayloadAndResult()
    {
        FeatureHandlerRegistry.Clear();
        var handler = new ProbeFeatureHandler("scene.feature.typed");
        FeatureHandlerRegistry.Register(handler);
        var service = new FeatureService();
        var owner = CapabilityValidationSupport.Spawn("feature-scene-typed-owner");
        var feature = CapabilityValidationSupport.Spawn("feature-scene-typed-feature");
        var context = new FeatureContext
        {
            Owner = owner,
            Feature = feature,
            Definition = new FeatureDefinition { FeatureId = handler.FeatureId, HandlerId = handler.FeatureId },
            ActivationPayload = new SceneActivationPayload("typed")
        };

        service.Activate(context);
        var hasPayload = context.TryGetActivation<SceneActivationPayload>(out var payload);
        var hasResult = context.TryGetExecutionResult<ProbeFeatureResult>(out var result);
        var success = hasPayload && payload.Value == "typed" && hasResult && result.Value == 1;

        return CheckResult.From(success, success ? "typed payload/result passed" : "typed payload/result mismatch", CapabilityValidationSupport.Details(
            ("hasPayload", hasPayload),
            ("payload", hasPayload ? payload.Value : null),
            ("hasResult", hasResult),
            ("resultValue", hasResult ? result.Value : null)));
    }

    private readonly record struct SceneFeatureEvent(int Value) : IEntityEvent;

    private sealed class SceneActivationPayload : IFeatureActivationPayload
    {
        public SceneActivationPayload(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }
}
