using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Godot;
using SlimeAI.GameOS.Observation;
using SlimeAI.GameOS.Runtime.CommandBuffer;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Events.Core;
using SlimeAI.GameOS.Runtime.Resource;
using SlimeAI.GameOS.Runtime.Schedule;
using SlimeAI.GameOS.Runtime.World;

namespace SlimeAI.SceneTests.Runtime.World;

/// <summary>
/// Runtime/World 的 Godot headless 验证场景。
/// </summary>
public partial class RuntimeWorldValidationScene : Node
{
    private const string ScenePath = "res://SlimeAI/Scenes/Validation/Runtime/World/RuntimeWorldValidation.tscn";
    private const string ArtifactFileName = "runtime-world-validation.json";
    private const string LogContext = "RuntimeWorldValidation";

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
            "Runtime/World",
            ArtifactFileName,
            new[]
            {
                "SlimeAI.GameOS.Runtime.World",
                "SlimeAI.GameOS.Runtime.Entity",
                "SlimeAI.GameOS.Runtime.CommandBuffer",
                "Games/BrotatoLike Godot scene runner"
            },
            new[]
            {
                "The dispose order check uses the existing internal test hook through reflection because the hook is not public API.",
                "No Runtime public API is added for this validation."
            },
            expectedInputs: new[]
            {
                "two RuntimeWorld.CreateScoped() instances with isolated entity/resource/event state",
                "RuntimeWorld.Default dispose attempt",
                "scoped world dispose with pending command and teardown event subscriptions"
            },
            expectedObservations: new[]
            {
                "scoped worlds do not share entities, resources or event bus state",
                "RuntimeWorld.Default.Dispose() throws InvalidOperationException and remains usable",
                "scoped world dispose order is Schedule,Commands,Pools,Resources,Lifecycle,Entities,Events and post-dispose access throws ObjectDisposedException"
            },
            passCriteria: new[]
            {
                "all world isolation and dispose checks pass",
                "stdout contains GameOS Runtime World validation PASS",
                "failureReasons is empty"
            },
            failCriteria: new[]
            {
                "isolation, default disposal, dispose order, teardown event or post-dispose access check fails",
                "stdout contains GameOS Runtime World validation FAIL",
                "failureReasons identifies the failed world invariant"
            });

        validation.Info("validation start");
        validation.Check("scoped_world_isolation", "RuntimeWorld", ValidateScopedWorldIsolation);
        validation.Check("default_cannot_dispose", "RuntimeWorld", ValidateDefaultCannotDispose);
        validation.Check("dispose_order_seven_steps", "RuntimeWorldDispose", ValidateDisposeOrderSevenSteps);
        validation.Check("teardown_events_delivered", "RuntimeWorldDispose", ValidateTeardownEventsDelivered);
        validation.Check("access_after_dispose_throws", "RuntimeWorldDispose", ValidateAccessAfterDisposeThrows);

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
        GD.Print(success ? "GameOS Runtime World validation PASS" : "GameOS Runtime World validation FAIL");
        if (!success)
        {
            GD.Print($"GameOS Runtime World validation failures: {string.Join("; ", validation.FailureReasons)}");
        }

        GetTree().Quit(success ? 0 : 1);
    }

    private static CheckResult ValidateScopedWorldIsolation()
    {
        using var w1 = RuntimeWorld.CreateScoped();
        using var w2 = RuntimeWorld.CreateScoped();
        var entity = w1.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("runtime-world-isolated") });
        w1.Resources.Register("runtime-world-resource", ResourceCategory.Entity, "res://RuntimeWorld.tscn");

        var w1Events = 0;
        var w2Events = 0;
        using var w1Sub = w1.Events.Subscribe<LifecycleChildAttached>(_ => w1Events++);
        using var w2Sub = w2.Events.Subscribe<LifecycleChildAttached>(_ => w2Events++);
        w1.Events.Publish(new LifecycleChildAttached(entity.EntityId, new EntityId("runtime-world-child"), ParentDestroyPolicy.Detach));

        var success = w1.Entities.Get(entity.EntityId)?.EntityId == entity.EntityId
            && w2.Entities.Get(entity.EntityId) == null
            && RuntimeWorld.Default.Entities.Get(entity.EntityId) == null
            && w1.Resources.TryGet("runtime-world-resource", ResourceCategory.Entity, out _)
            && !w2.Resources.TryGet("runtime-world-resource", ResourceCategory.Entity, out _)
            && w1Events == 1
            && w2Events == 0;

        return CheckResult.From(success, success ? "CreateScoped isolation passed" : "CreateScoped isolation mismatch", new Dictionary<string, object?>
        {
            ["w1Entity"] = w1.Entities.Get(entity.EntityId)?.EntityId.Value,
            ["w2Entity"] = w2.Entities.Get(entity.EntityId)?.EntityId.Value,
            ["defaultEntity"] = RuntimeWorld.Default.Entities.Get(entity.EntityId)?.EntityId.Value,
            ["w1Events"] = w1Events,
            ["w2Events"] = w2Events
        });
    }

    private static CheckResult ValidateDefaultCannotDispose()
    {
        var threw = false;
        string? message = null;
        try
        {
            RuntimeWorld.Default.Dispose();
        }
        catch (InvalidOperationException ex)
        {
            threw = true;
            message = ex.Message;
        }

        var success = threw
            && message == "RuntimeWorld.Default cannot be disposed"
            && !RuntimeWorld.Default.IsDisposed;
        return CheckResult.From(success, success ? "Default dispose rejection passed" : "Default dispose rejection mismatch", new Dictionary<string, object?>
        {
            ["threw"] = threw,
            ["message"] = message,
            ["defaultDisposed"] = RuntimeWorld.Default.IsDisposed
        });
    }

    private static CheckResult ValidateDisposeOrderSevenSteps()
    {
        var order = new List<string>();
        var method = typeof(RuntimeWorld).GetMethod(
            "CreateScoped",
            BindingFlags.Static | BindingFlags.NonPublic,
            binder: null,
            types: new[] { typeof(Action<string>) },
            modifiers: null);

        if (method == null)
        {
            return CheckResult.Fail("internal dispose observer hook not found");
        }

        var world = (RuntimeWorld?)method.Invoke(null, new object[] { (Action<string>)order.Add });
        if (world == null)
        {
            return CheckResult.Fail("internal dispose observer hook returned null");
        }

        world.Dispose();
        var observed = string.Join(",", order);
        const string expected = "Schedule,Commands,Pools,Resources,Lifecycle,Entities,Events";
        var success = observed == expected;
        return CheckResult.From(success, success ? "7-step dispose order passed" : "7-step dispose order mismatch", new Dictionary<string, object?>
        {
            ["observed"] = observed,
            ["expected"] = expected
        });
    }

    private static CheckResult ValidateTeardownEventsDelivered()
    {
        var world = RuntimeWorld.CreateScoped();
        var detached = 0;
        var destroyed = 0;
        using var detachSub = world.Events.Subscribe<LifecycleChildDetached>(_ => detached++);
        using var destroySub = world.Events.Subscribe<EntityDestroyed>(_ => destroyed++);

        var parent = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("runtime-world-teardown-parent") });
        world.Entities.Spawn(new EntitySpawnConfig
        {
            EntityId = new EntityId("runtime-world-teardown-child"),
            ParentEntityId = parent.EntityId
        });

        world.Dispose();
        var success = detached == 1 && destroyed == 2;
        return CheckResult.From(success, success ? "teardown events delivered" : "teardown event counts mismatch", new Dictionary<string, object?>
        {
            ["detached"] = detached,
            ["destroyed"] = destroyed
        });
    }

    private static CheckResult ValidateAccessAfterDisposeThrows()
    {
        var world = RuntimeWorld.CreateScoped();
        var commands = world.Commands;
        commands.Enqueue(DeferredRuntimeCommand.ForSpawn(
            new EntitySpawnConfig { EntityId = new EntityId("runtime-world-discard") },
            new EntityId("runtime-world-discard"),
            SchedulePhase.EndOfFrame,
            "runtime-world-validation",
            "discard"));

        world.Dispose();

        var entitiesThrow = Throws<ObjectDisposedException>(() => _ = world.Entities);
        var lifecycleThrow = Throws<ObjectDisposedException>(() => _ = world.Lifecycle);
        var eventsThrow = Throws<ObjectDisposedException>(() => _ = world.Events);
        var resourcesThrow = Throws<ObjectDisposedException>(() => _ = world.Resources);
        var poolsThrow = Throws<ObjectDisposedException>(() => _ = world.Pools);
        var scheduleThrow = Throws<ObjectDisposedException>(() => _ = world.Schedule);
        var commandsThrow = Throws<ObjectDisposedException>(() => _ = world.Commands);
        var discardReported = commands.LastDiscardReport.SkippedCount == 1
            && commands.LastDiscardReport.Commands[0].FailureReason == DeferredCommandFailureReason.WorldDisposing;

        var success = entitiesThrow
            && lifecycleThrow
            && eventsThrow
            && resourcesThrow
            && poolsThrow
            && scheduleThrow
            && commandsThrow
            && discardReported;

        return CheckResult.From(success, success ? "post-dispose access and discard report passed" : "post-dispose access or discard report mismatch", new Dictionary<string, object?>
        {
            ["entitiesThrow"] = entitiesThrow,
            ["lifecycleThrow"] = lifecycleThrow,
            ["eventsThrow"] = eventsThrow,
            ["resourcesThrow"] = resourcesThrow,
            ["poolsThrow"] = poolsThrow,
            ["scheduleThrow"] = scheduleThrow,
            ["commandsThrow"] = commandsThrow,
            ["discardSkipped"] = commands.LastDiscardReport.SkippedCount
        });
    }

    private static bool Throws<TException>(Action action)
        where TException : Exception
    {
        try
        {
            action();
            return false;
        }
        catch (TException)
        {
            return true;
        }
    }
}
