using System;
using System.Collections.Generic;
using System.IO;
using Godot;
using SlimeAI.GameOS.Observation;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;
using SlimeAI.GameOS.Runtime.Events.Core;
using RuntimeDataKey = SlimeAI.GameOS.Runtime.Data.DataKey;
using RuntimeDataKeyInt = SlimeAI.GameOS.Runtime.Data.DataKey<int>;

namespace SlimeAI.SceneTests.Runtime.Event;

/// <summary>
/// Godot headless scene validation for GameOS Runtime/Event.
/// </summary>
public partial class RuntimeEventValidationScene : Node
{
    private const string ScenePath = "res://SlimeAI/Scenes/Validation/Runtime/Event/RuntimeEventValidation.tscn";
    private const string ArtifactFileName = "runtime-event-validation.json";
    private const string LogContext = "RuntimeEventValidation";
    private static readonly RuntimeDataKeyInt BridgeValue = RuntimeDataKey.Create("RuntimeEventValidation.Value", 0);

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
            "Runtime/Event",
            ArtifactFileName,
            new[]
            {
                "SlimeAI.GameOS.Runtime.Event",
                "SlimeAI.GameOS.Runtime.Entity for Data-to-Event bridge check",
                "Games/BrotatoLike Godot scene runner"
            },
            new[]
            {
                "data_to_event_bridge is a labelled cross-layer bridge check, not a pure EventBus core assertion.",
                "WorldEventBus is cleared before and after validation."
            },
            expectedInputs: new[]
            {
                "local entity event bus and WorldEvents.World subscriptions",
                "handler exception, same-type reentry and invalid scope publish attempts",
                "RuntimeEntity.Data.Set bridge for DataPropertyChanged event"
            },
            expectedObservations: new[]
            {
                "registration order, unsubscribe and handler exception isolation are deterministic",
                "same-type reentry is blocked and invalid entity/world event scope publishes do not dispatch",
                "broadcast event reaches both entity and world buses and data bridge emits DataPropertyChanged"
            },
            passCriteria: new[]
            {
                "all Runtime/Event checks pass",
                "stdout contains GameOS Runtime Event validation PASS",
                "failureReasons is empty"
            },
            failCriteria: new[]
            {
                "any event ordering, subscription, guard, scope or bridge check fails",
                "stdout contains GameOS Runtime Event validation FAIL",
                "failureReasons identifies the failed event invariant"
            });

        validation.Info("validation start");
        WorldEvents.World.Clear();
        validation.Info("world event bus cleared before checks");

        validation.Check("registration_order", "EventBusCore", ValidateRegistrationOrder);
        validation.Check("dispose_unsubscribes", "EventBusCore", ValidateDisposeUnsubscribes);
        validation.Check("handler_exception_capture", "EventBusCore", ValidateHandlerExceptionCapture);
        validation.Check("same_type_reentry_blocked", "EventBusCore", ValidateSameTypeReentryBlocked);
        validation.Check("entity_event_rejected_on_world_bus", "EventBusCore", ValidateEntityEventRejectedOnWorldBus);
        validation.Check("broadcast_routes_to_both_buses", "EventBusCore", ValidateBroadcastRoutesToBothBuses);
        validation.Check("data_to_event_bridge", "DataToEventBridge", ValidateDataToEventBridge);

        var success = validation.Success;
        if (success)
        {
            validation.Pass("all checks passed");
        }
        else
        {
            validation.Fail($"{validation.FailureReasons.Count} checks failed");
        }

        WorldEvents.World.Clear();
        validation.Info("world event bus cleared after checks");
        validation.WriteArtifact();

        GD.Print(success ? "GameOS Runtime Event validation PASS" : "GameOS Runtime Event validation FAIL");
        if (!success)
        {
            GD.Print($"GameOS Runtime Event validation failures: {string.Join("; ", validation.FailureReasons)}");
        }

        GetTree().Quit(success ? 0 : 1);
    }

    private static CheckResult ValidateRegistrationOrder()
    {
        var bus = new EntityEventBus("entity:validation-registration");
        var order = new List<string>();
        bus.Subscribe<ValidationLocalEvent>(_ => order.Add("first"));
        bus.Subscribe<ValidationLocalEvent>(_ => order.Add("second"));
        bus.Subscribe<ValidationLocalEvent>(_ => order.Add("third"));

        bus.Publish(new ValidationLocalEvent(1));

        var joined = string.Join(",", order);
        var success = joined == "first,second,third";
        return CheckResult.From(success, success ? "registration order preserved" : "registration order mismatch", new Dictionary<string, object?>
        {
            ["observedOrder"] = joined,
            ["expectedOrder"] = "first,second,third"
        });
    }

    private static CheckResult ValidateDisposeUnsubscribes()
    {
        var bus = new EntityEventBus("entity:validation-dispose");
        var count = 0;
        var token = bus.Subscribe<ValidationLocalEvent>(_ => count++);

        bus.Publish(new ValidationLocalEvent(1));
        token.Dispose();
        bus.Publish(new ValidationLocalEvent(2));

        var success = count == 1;
        return CheckResult.From(success, success ? "dispose unsubscribes" : "disposed handler still fired", new Dictionary<string, object?>
        {
            ["count"] = count
        });
    }

    private static CheckResult ValidateHandlerExceptionCapture()
    {
        return RunWithStdoutMuted(() =>
        {
            var bus = new EntityEventBus("entity:validation-exception");
            var continuedCount = 0;

            bus.Subscribe<ValidationLocalEvent>(_ => throw new InvalidOperationException("expected validation exception"));
            bus.Subscribe<ValidationLocalEvent>(_ => continuedCount++);

            bus.Publish(new ValidationLocalEvent(1));

            var success = continuedCount == 1;
            return CheckResult.From(success, success ? "exception isolated and dispatch continued" : "exception interrupted dispatch", new Dictionary<string, object?>
            {
                ["continuedCount"] = continuedCount
            });
        });
    }

    private static CheckResult ValidateSameTypeReentryBlocked()
    {
        return RunWithStdoutMuted(() =>
        {
            var bus = new EntityEventBus("entity:validation-reentry");
            var executions = 0;

            bus.Subscribe<ValidationLocalEvent>(_ =>
            {
                executions++;
                if (executions == 1)
                {
                    bus.Publish(new ValidationLocalEvent(2));
                }
            });

            bus.Publish(new ValidationLocalEvent(1));

            var success = executions == 1;
            return CheckResult.From(success, success ? "same-type reentry was blocked" : "same-type reentry executed unexpectedly", new Dictionary<string, object?>
            {
                ["executions"] = executions
            });
        });
    }

    private static CheckResult ValidateEntityEventRejectedOnWorldBus()
    {
        return RunWithStdoutMuted(() =>
        {
            var count = 0;
            var token = WorldEvents.World.Subscribe<ValidationLocalEvent>(_ => count++);
            try
            {
                WorldEvents.World.Publish(new ValidationLocalEvent(1));
            }
            finally
            {
                token.Dispose();
            }

            var success = count == 0;
            return CheckResult.From(success, success ? "world bus rejected IEntityEvent" : "world bus accepted IEntityEvent", new Dictionary<string, object?>
            {
                ["count"] = count
            });
        });
    }

    private static CheckResult ValidateBroadcastRoutesToBothBuses()
    {
        WorldEvents.World.Clear();
        var entity = new RuntimeEntity(new EntityId("runtime-event-validation-broadcast"));
        var localCount = 0;
        var globalCount = 0;

        var localToken = entity.Events.Subscribe<ValidationBroadcastEvent>(_ => localCount++);
        var globalToken = WorldEvents.World.Subscribe<ValidationBroadcastEvent>(_ => globalCount++);

        entity.Events.Publish(new ValidationBroadcastEvent(1));

        localToken.Dispose();
        globalToken.Dispose();
        WorldEvents.World.Clear();

        var success = localCount == 1 && globalCount == 1;
        return CheckResult.From(success, success ? "broadcast routed to entity and world buses" : "broadcast routing mismatch", new Dictionary<string, object?>
        {
            ["localCount"] = localCount,
            ["globalCount"] = globalCount
        });
    }

    private static CheckResult ValidateDataToEventBridge()
    {
        var entity = new RuntimeEntity(new EntityId("runtime-event-validation-data"));
        var receivedCount = 0;
        string? receivedKey = null;
        object? receivedValue = null;

        entity.Events.Subscribe<DataPropertyChanged>(data =>
        {
            receivedCount++;
            receivedKey = data.Change.StableKey;
            receivedValue = data.Change.NewValue;
        });

        entity.Data.Set(BridgeValue, 42);

        var success = receivedCount == 1
            && receivedKey == BridgeValue.StableKey
            && receivedValue is int intValue
            && intValue == 42;
        return CheckResult.From(success, success ? "Data.Set emitted PropertyChanged" : "Data-to-Event bridge did not emit expected payload", new Dictionary<string, object?>
        {
            ["bridgeLabel"] = "Data-to-Event bridge check",
            ["receivedCount"] = receivedCount,
            ["receivedKey"] = receivedKey,
            ["receivedValue"] = receivedValue
        });
    }

    private static CheckResult RunWithStdoutMuted(Func<CheckResult> validate)
    {
        GameOSLog.Configure(new GameOSLogOptions
        {
            MinimumLevel = GameOSLogLevel.Trace,
            EnableStdout = false,
            EnableJsonl = true,
            EnableGodotRichText = true
        });

        try
        {
            return validate();
        }
        finally
        {
            GameOSLog.Configure(new GameOSLogOptions
            {
                MinimumLevel = GameOSLogLevel.Trace,
                EnableStdout = true,
                EnableJsonl = true,
                EnableGodotRichText = true
            });
        }
    }

    private readonly record struct ValidationLocalEvent(int Value) : IEntityEvent;

    private readonly record struct ValidationBroadcastEvent(int Value) : IBroadcastEvent;
}
