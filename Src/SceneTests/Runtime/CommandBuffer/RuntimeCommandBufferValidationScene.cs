using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using SlimeAI.GameOS.Observation;
using SlimeAI.GameOS.Runtime.CommandBuffer;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Events.Core;
using SlimeAI.GameOS.Runtime.Resource;
using SlimeAI.GameOS.Runtime.Schedule;
using SlimeAI.GameOS.Runtime.World;

namespace SlimeAI.SceneTests.Runtime.CommandBuffer;

/// <summary>
/// Runtime/CommandBuffer 的 Godot headless 验证场景。
/// </summary>
public partial class RuntimeCommandBufferValidationScene : Node
{
    private const string ScenePath = "res://SlimeAI/Scenes/Validation/Runtime/CommandBuffer/RuntimeCommandBufferValidation.tscn";
    private const string ArtifactFileName = "runtime-command-buffer-validation.json";
    private const string LogContext = "RuntimeCommandBufferValidation";

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
            "Runtime/CommandBuffer",
            ArtifactFileName,
            new[]
            {
                "SlimeAI.GameOS.Runtime.CommandBuffer",
                "SlimeAI.GameOS.Runtime.Schedule",
                "SlimeAI.GameOS.Runtime.World",
                "Games/BrotatoLike Godot scene runner"
            },
            new[]
            {
                "GodotNodeInstantiate and GodotNodeFree are validated with a fake Runtime handler.",
                "Manual phase is used only for explicit validation playback."
            },
            expectedInputs: new[]
            {
                "RuntimeWorld.CreateScoped() command buffer with spawn, destroy, attach, detach, event, resource and fake Godot node commands",
                "SchedulePhase.Manual and EndOfFrame playback calls",
                "nested EnterGuard(reason) scopes and world dispose with pending commands"
            },
            expectedObservations: new[]
            {
                "all eight deferred command kinds are accepted and reported",
                "only commands matching the requested phase are played and Manual is not auto-played by frame phases",
                "guarded structural mutations defer until EndOfFrame and pending commands are skipped with WorldDisposing on dispose"
            },
            passCriteria: new[]
            {
                "all command kind, phase, guard and dispose checks pass",
                "stdout contains GameOS Runtime CommandBuffer validation PASS",
                "failureReasons is empty"
            },
            failCriteria: new[]
            {
                "any deferred command, phase playback, guard or discard-report check fails",
                "stdout contains GameOS Runtime CommandBuffer validation FAIL",
                "failureReasons identifies the failed command-buffer invariant"
            });

        validation.Info("validation start");
        validation.Check("all_eight_command_kinds", "CommandKinds", ValidateAllEightCommandKinds);
        validation.Check("phase_targeted_playback", "SchedulePhase", ValidatePhaseTargetedPlayback);
        validation.Check("nested_guard_behavior", "StructuralChangeGuard", ValidateNestedGuardBehavior);
        validation.Check("guarded_structural_mutations", "StructuralChangeGuard", ValidateGuardedStructuralMutations);
        validation.Check("dispose_discard_report", "CommandBufferDispose", ValidateDisposeDiscardReport);

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
        GD.Print(success ? "GameOS Runtime CommandBuffer validation PASS" : "GameOS Runtime CommandBuffer validation FAIL");
        if (!success)
        {
            GD.Print($"GameOS Runtime CommandBuffer validation failures: {string.Join("; ", validation.FailureReasons)}");
        }

        GetTree().Quit(success ? 0 : 1);
    }

    private static CheckResult ValidateAllEightCommandKinds()
    {
        using var world = RuntimeWorld.CreateScoped();
        var bridge = new FakeGodotNodeCommandHandler();
        world.Commands.GodotNodeCommandHandler = bridge;

        var parent = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("command-parent") });
        var child = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("command-child") });
        var destroyTarget = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("command-destroy") });
        var detachParent = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("command-detach-parent") });
        var detachChild = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("command-detach-child") });
        world.Lifecycle.Attach(detachParent.EntityId, detachChild.EntityId);

        var attachEventCount = 0;
        using var sub = world.Events.Subscribe<LifecycleChildAttached>(_ => attachEventCount++);

        world.Commands.Enqueue(DeferredRuntimeCommand.ForSpawn(new EntitySpawnConfig { EntityId = new EntityId("command-spawn") }, new EntityId("command-spawn"), SchedulePhase.Manual, "validation", "spawn"));
        world.Commands.Enqueue(DeferredRuntimeCommand.ForDestroy(destroyTarget.EntityId, SchedulePhase.Manual, "validation", "destroy"));
        world.Commands.Enqueue(DeferredRuntimeCommand.ForAttach(parent.EntityId, child.EntityId, ParentDestroyPolicy.Detach, 7, SchedulePhase.Manual, "validation", "attach"));
        world.Commands.Enqueue(DeferredRuntimeCommand.ForDetach(detachParent.EntityId, detachChild.EntityId, SchedulePhase.Manual, "validation", "detach"));
        world.Commands.Enqueue(DeferredRuntimeCommand.ForQueuedEvent(new LifecycleChildAttached(parent.EntityId, child.EntityId, ParentDestroyPolicy.Detach), SchedulePhase.Manual, "validation", "event"));
        world.Commands.Enqueue(DeferredRuntimeCommand.ForResourceRequest("command-resource", "res://CommandBuffer.tscn", SchedulePhase.Manual, "validation", "resource"));
        world.Commands.Enqueue(DeferredRuntimeCommand.ForGodotInstantiate("res://Node.tscn", parent.EntityId, SchedulePhase.Manual, "validation", "instantiate"));
        world.Commands.Enqueue(DeferredRuntimeCommand.ForGodotFree("/root/Node", SchedulePhase.Manual, "validation", "free"));

        var report = world.Schedule.RunPhase(SchedulePhase.Manual);
        var kinds = string.Join(",", report.Commands.Select(command => command.Kind.ToString()));
        var success = report.PlayedCount == 8
            && report.FailedCount == 0
            && world.Entities.Get(new EntityId("command-spawn")) != null
            && world.Entities.Get(destroyTarget.EntityId) == null
            && world.Lifecycle.IsAttached(parent.EntityId, child.EntityId)
            && !world.Lifecycle.IsAttached(detachParent.EntityId, detachChild.EntityId)
            && attachEventCount == 2
            && world.Resources.TryGet("command-resource", ResourceCategory.Other, out var resource)
            && resource.Path == "res://CommandBuffer.tscn"
            && bridge.InstantiateCount == 1
            && bridge.FreeCount == 1;

        return CheckResult.From(success, success ? "all 8 command kinds played" : "command kind playback mismatch", new Dictionary<string, object?>
        {
            ["playedCount"] = report.PlayedCount,
            ["failedCount"] = report.FailedCount,
            ["kinds"] = kinds,
            ["attachEventCount"] = attachEventCount,
            ["instantiateCount"] = bridge.InstantiateCount,
            ["freeCount"] = bridge.FreeCount
        });
    }

    private static CheckResult ValidatePhaseTargetedPlayback()
    {
        using var world = RuntimeWorld.CreateScoped();
        world.Commands.Enqueue(DeferredRuntimeCommand.ForSpawn(new EntitySpawnConfig { EntityId = new EntityId("phase-before") }, new EntityId("phase-before"), SchedulePhase.BeforeSystemTick, "validation", "before"));
        world.Commands.Enqueue(DeferredRuntimeCommand.ForSpawn(new EntitySpawnConfig { EntityId = new EntityId("phase-manual") }, new EntityId("phase-manual"), SchedulePhase.Manual, "validation", "manual"));

        var before = world.Schedule.RunPhase(SchedulePhase.BeforeSystemTick);
        var begin = world.Schedule.RunPhase(SchedulePhase.BeginTick);
        var afterSystem = world.Schedule.RunPhase(SchedulePhase.AfterSystemTick);
        var afterEvent = world.Schedule.RunPhase(SchedulePhase.AfterEventDispatch);
        var end = world.Schedule.RunPhase(SchedulePhase.EndOfFrame);
        var manualStillPending = world.Entities.Get(new EntityId("phase-manual")) == null && world.Commands.QueuedCount == 1;
        var manual = world.Schedule.RunPhase(SchedulePhase.Manual);

        var success = before.PlayedCount == 1
            && begin.PlayedCount == 0
            && afterSystem.PlayedCount == 0
            && afterEvent.PlayedCount == 0
            && end.PlayedCount == 0
            && manualStillPending
            && manual.PlayedCount == 1
            && world.Entities.Get(new EntityId("phase-before")) != null
            && world.Entities.Get(new EntityId("phase-manual")) != null;

        return CheckResult.From(success, success ? "phase-targeted playback passed" : "phase-targeted playback mismatch", new Dictionary<string, object?>
        {
            ["beforePlayed"] = before.PlayedCount,
            ["manualStillPending"] = manualStillPending,
            ["manualPlayed"] = manual.PlayedCount,
            ["remainingQueued"] = world.Commands.QueuedCount
        });
    }

    private static CheckResult ValidateNestedGuardBehavior()
    {
        using var world = RuntimeWorld.CreateScoped();
        var initiallyGuarded = world.Commands.IsGuarded;
        var outerGuarded = false;
        var innerGuarded = false;
        var afterInnerGuarded = false;

        using (world.Commands.EnterGuard("outer"))
        {
            outerGuarded = world.Commands.IsGuarded;
            using (world.Commands.EnterGuard("inner"))
            {
                innerGuarded = world.Commands.IsGuarded;
            }

            afterInnerGuarded = world.Commands.IsGuarded;
        }

        var success = !initiallyGuarded
            && outerGuarded
            && innerGuarded
            && afterInnerGuarded
            && !world.Commands.IsGuarded;

        return CheckResult.From(success, success ? "nested guard behavior passed" : "nested guard behavior mismatch", new Dictionary<string, object?>
        {
            ["initiallyGuarded"] = initiallyGuarded,
            ["outerGuarded"] = outerGuarded,
            ["innerGuarded"] = innerGuarded,
            ["afterInnerGuarded"] = afterInnerGuarded,
            ["finallyGuarded"] = world.Commands.IsGuarded
        });
    }

    private static CheckResult ValidateGuardedStructuralMutations()
    {
        using var world = RuntimeWorld.CreateScoped();
        var parent = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("guard-parent") });
        var child = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("guard-child") });
        var doomed = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("guard-doomed") });
        RuntimeEntity reserved;

        using (world.Commands.EnterGuard("validation-guard"))
        {
            reserved = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("guard-spawn") });
            var attachQueued = world.Lifecycle.Attach(parent.EntityId, child.EntityId);
            var destroyQueued = world.Entities.Destroy(doomed.EntityId);
            if (!attachQueued || !destroyQueued)
            {
                return CheckResult.Fail("guarded attach or destroy was rejected");
            }
        }

        var beforePlayback = world.Entities.Get(reserved.EntityId) == null
            && !world.Lifecycle.IsAttached(parent.EntityId, child.EntityId)
            && world.Entities.Get(doomed.EntityId) != null
            && world.Commands.QueuedCount == 3;
        var report = world.Schedule.RunPhase(SchedulePhase.EndOfFrame);
        var afterPlayback = world.Entities.Get(reserved.EntityId)?.EntityId == reserved.EntityId
            && world.Lifecycle.IsAttached(parent.EntityId, child.EntityId)
            && world.Entities.Get(doomed.EntityId) == null
            && report.PlayedCount == 3;

        var success = beforePlayback && afterPlayback;
        return CheckResult.From(success, success ? "guarded structural mutations passed" : "guarded structural mutations mismatch", new Dictionary<string, object?>
        {
            ["beforePlayback"] = beforePlayback,
            ["afterPlayback"] = afterPlayback,
            ["playedCount"] = report.PlayedCount,
            ["failedCount"] = report.FailedCount
        });
    }

    private static CheckResult ValidateDisposeDiscardReport()
    {
        var world = RuntimeWorld.CreateScoped();
        var commands = world.Commands;
        commands.Enqueue(DeferredRuntimeCommand.ForSpawn(
            new EntitySpawnConfig { EntityId = new EntityId("command-dispose") },
            new EntityId("command-dispose"),
            SchedulePhase.EndOfFrame,
            "validation",
            "dispose"));

        world.Dispose();
        var report = commands.LastDiscardReport;
        var enqueueAfterDisposeThrows = Throws<ObjectDisposedException>(() => commands.Enqueue(DeferredRuntimeCommand.ForGodotFree("/root/AfterDispose")));
        var success = report.SkippedCount == 1
            && report.Commands[0].Status == DeferredCommandStatus.Skipped
            && report.Commands[0].FailureReason == DeferredCommandFailureReason.WorldDisposing
            && enqueueAfterDisposeThrows;

        return CheckResult.From(success, success ? "dispose discard report passed" : "dispose discard report mismatch", new Dictionary<string, object?>
        {
            ["skippedCount"] = report.SkippedCount,
            ["status"] = report.Commands.Length > 0 ? report.Commands[0].Status.ToString() : string.Empty,
            ["reason"] = report.Commands.Length > 0 ? report.Commands[0].FailureReason.ToString() : string.Empty,
            ["enqueueAfterDisposeThrows"] = enqueueAfterDisposeThrows
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

    private sealed class FakeGodotNodeCommandHandler : IGodotNodeCommandHandler
    {
        public int InstantiateCount { get; private set; }
        public int FreeCount { get; private set; }

        public bool InstantiateNode(GodotNodeInstantiatePayload payload)
        {
            InstantiateCount++;
            return true;
        }

        public bool FreeNode(GodotNodeFreeCommandPayload payload)
        {
            FreeCount++;
            return true;
        }
    }
}
