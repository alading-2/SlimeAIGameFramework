using SlimeAI.GameOS.Runtime.CommandBuffer;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Events.Core;
using SlimeAI.GameOS.Runtime.Event;
using SlimeAI.GameOS.Runtime.Resource;
using SlimeAI.GameOS.Runtime.Schedule;
using SlimeAI.GameOS.Runtime.World;

internal static class RuntimeCommandBufferTests
{
    public static void EnqueueSnapshotAndClear()
    {
        using var world = RuntimeWorld.CreateScoped();
        var command = DeferredRuntimeCommand.ForSpawn(
            new EntitySpawnConfig { EntityId = new EntityId("buffer-snapshot") },
            new EntityId("buffer-snapshot"),
            SchedulePhase.Manual,
            "test",
            "snapshot");

        var queued = world.Commands.Enqueue(command);

        AssertEqual("queued count", 1, world.Commands.QueuedCount);
        AssertEqual("snapshot count", 1, world.Commands.Snapshot().Length);
        AssertEqual("assigned command id", true, queued.CommandId > 0);
        AssertEqual("assigned sequence", true, queued.Sequence > 0);

        var report = world.Commands.Clear();

        AssertEqual("clear skipped count", 1, report.SkippedCount);
        AssertEqual("clear reason", DeferredCommandFailureReason.WorldDisposing, report.Commands[0].FailureReason);
        AssertEqual("clear empties queue", 0, world.Commands.QueuedCount);
    }

    public static void PlaysAllCommandKinds()
    {
        using var world = RuntimeWorld.CreateScoped();
        var bridge = new FakeGodotNodeCommandHandler();
        world.Commands.GodotNodeCommandHandler = bridge;

        var parent = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("buffer-parent") });
        var child = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("buffer-child") });
        var destroyTarget = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("buffer-destroy") });
        var detachParent = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("buffer-detach-parent") });
        var detachChild = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("buffer-detach-child") });
        world.Lifecycle.Attach(detachParent.EntityId, detachChild.EntityId);

        var queuedEventCount = 0;
        using var sub = world.Events.Subscribe<LifecycleChildAttached>(_ => queuedEventCount++);

        world.Commands.Enqueue(DeferredRuntimeCommand.ForSpawn(
            new EntitySpawnConfig { EntityId = new EntityId("buffer-spawn") },
            new EntityId("buffer-spawn"),
            SchedulePhase.Manual,
            "test",
            "spawn"));
        world.Commands.Enqueue(DeferredRuntimeCommand.ForDestroy(destroyTarget.EntityId, SchedulePhase.Manual, "test", "destroy"));
        world.Commands.Enqueue(DeferredRuntimeCommand.ForAttach(parent.EntityId, child.EntityId, ParentDestroyPolicy.Detach, 7, SchedulePhase.Manual, "test", "attach"));
        world.Commands.Enqueue(DeferredRuntimeCommand.ForDetach(detachParent.EntityId, detachChild.EntityId, SchedulePhase.Manual, "test", "detach"));
        world.Commands.Enqueue(DeferredRuntimeCommand.ForQueuedEvent(
            new LifecycleChildAttached(parent.EntityId, child.EntityId, ParentDestroyPolicy.Detach),
            SchedulePhase.Manual,
            "test",
            "event"));
        world.Commands.Enqueue(DeferredRuntimeCommand.ForResourceRequest("buffer-resource", "res://Buffer.tscn", SchedulePhase.Manual, "test", "resource"));
        world.Commands.Enqueue(DeferredRuntimeCommand.ForGodotInstantiate("res://Node.tscn", parent.EntityId, SchedulePhase.Manual, "test", "instantiate"));
        world.Commands.Enqueue(DeferredRuntimeCommand.ForGodotFree("/root/Node", SchedulePhase.Manual, "test", "free"));

        var report = world.Schedule.RunPhase(SchedulePhase.Manual);

        AssertEqual("all commands played", 8, report.PlayedCount);
        AssertEqual("no failures", 0, report.FailedCount);
        AssertEqual("spawn registered", new EntityId("buffer-spawn"), world.Entities.Get(new EntityId("buffer-spawn"))?.EntityId);
        AssertEqual("destroy removed", null, world.Entities.Get(destroyTarget.EntityId));
        AssertEqual("attach applied", true, world.Lifecycle.IsAttached(parent.EntityId, child.EntityId));
        AssertEqual("detach applied", false, world.Lifecycle.IsAttached(detachParent.EntityId, detachChild.EntityId));
        AssertEqual("queued event published", 2, queuedEventCount);
        AssertEqual("resource registered", true, world.Resources.TryGet("buffer-resource", ResourceCategory.Other, out var resource));
        AssertEqual("resource path", "res://Buffer.tscn", resource.Path);
        AssertEqual("instantiate handler", 1, bridge.InstantiateCount);
        AssertEqual("free handler", 1, bridge.FreeCount);
    }

    public static void ReportsTypicalFailuresForAllCommandKinds()
    {
        using var world = RuntimeWorld.CreateScoped();
        var existing = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("buffer-existing") });

        world.Commands.Enqueue(DeferredRuntimeCommand.ForSpawn(
            new EntitySpawnConfig { EntityId = existing.EntityId },
            existing.EntityId,
            SchedulePhase.Manual,
            "test",
            "duplicate"));
        world.Commands.Enqueue(DeferredRuntimeCommand.ForDestroy(new EntityId("buffer-missing"), SchedulePhase.Manual, "test", "missing-destroy"));
        world.Commands.Enqueue(DeferredRuntimeCommand.ForAttach(new EntityId("missing-parent"), existing.EntityId, ParentDestroyPolicy.Detach, 0, SchedulePhase.Manual, "test", "missing-parent"));
        world.Commands.Enqueue(DeferredRuntimeCommand.ForDetach(existing.EntityId, new EntityId("missing-child"), SchedulePhase.Manual, "test", "missing-child"));
        world.Commands.Enqueue(DeferredRuntimeCommand.ForQueuedEvent(new ExternalTestEvent(7), SchedulePhase.Manual, "test", "external-event"));
        world.Commands.Enqueue(DeferredRuntimeCommand.ForResourceRequest("bad-resource", "", SchedulePhase.Manual, "test", "bad-resource"));
        world.Commands.Enqueue(DeferredRuntimeCommand.ForGodotInstantiate("res://Missing.tscn", null, SchedulePhase.Manual, "test", "missing-bridge-instantiate"));
        world.Commands.Enqueue(DeferredRuntimeCommand.ForGodotFree("/root/Missing", SchedulePhase.Manual, "test", "missing-bridge-free"));

        var report = world.Schedule.RunPhase(SchedulePhase.Manual);

        AssertEqual("all commands failed", 8, report.FailedCount);
        AssertEqual("spawn duplicate", DeferredCommandFailureReason.DuplicateEntityId, report.Commands[0].FailureReason);
        AssertEqual("destroy missing", DeferredCommandFailureReason.MissingTargetEntity, report.Commands[1].FailureReason);
        AssertEqual("attach missing parent", DeferredCommandFailureReason.MissingParent, report.Commands[2].FailureReason);
        AssertEqual("detach missing child", DeferredCommandFailureReason.MissingChild, report.Commands[3].FailureReason);
        AssertEqual("external event rejected", DeferredCommandFailureReason.UnsupportedKind, report.Commands[4].FailureReason);
        AssertEqual("resource invalid", DeferredCommandFailureReason.InvalidPayload, report.Commands[5].FailureReason);
        AssertEqual("instantiate bridge missing", DeferredCommandFailureReason.BridgeTargetUnavailable, report.Commands[6].FailureReason);
        AssertEqual("free bridge missing", DeferredCommandFailureReason.BridgeTargetUnavailable, report.Commands[7].FailureReason);
    }

    public static void PayloadInvariantAndSequenceOrdering()
    {
        using var world = RuntimeWorld.CreateScoped();
        var first = world.Commands.Enqueue(DeferredRuntimeCommand.ForSpawn(
            new EntitySpawnConfig { EntityId = new EntityId("buffer-seq-first") },
            new EntityId("buffer-seq-first"),
            SchedulePhase.Manual,
            "test",
            "first"));
        var second = world.Commands.Enqueue(DeferredRuntimeCommand.ForSpawn(
            new EntitySpawnConfig { EntityId = new EntityId("buffer-seq-second") },
            new EntityId("buffer-seq-second"),
            SchedulePhase.Manual,
            "test",
            "second"));

        AssertEqual("spawn payload present", true, first.Spawn.HasValue);
        AssertEqual("other payload absent", false, first.Destroy.HasValue);
        AssertEqual("sequence monotonic", true, second.Sequence > first.Sequence);

        var report = world.Schedule.RunPhase(SchedulePhase.Manual);

        AssertEqual("first played first", first.CommandId, report.Commands[0].CommandId);
        AssertEqual("second played second", second.CommandId, report.Commands[1].CommandId);
        AssertEqual("created id reported", new EntityId("buffer-seq-first"), report.Commands[0].CreatedEntityId);
    }

    public static void DisposeSkipsPendingCommands()
    {
        var world = RuntimeWorld.CreateScoped();
        var commands = world.Commands;
        commands.Enqueue(DeferredRuntimeCommand.ForSpawn(
            new EntitySpawnConfig { EntityId = new EntityId("buffer-dispose") },
            new EntityId("buffer-dispose"),
            SchedulePhase.EndOfFrame,
            "test",
            "dispose"));

        world.Dispose();

        AssertEqual("discard report count", 1, commands.LastDiscardReport.SkippedCount);
        AssertEqual("discard status", DeferredCommandStatus.Skipped, commands.LastDiscardReport.Commands[0].Status);
        AssertEqual("discard reason", DeferredCommandFailureReason.WorldDisposing, commands.LastDiscardReport.Commands[0].FailureReason);
        AssertThrows<ObjectDisposedException>("enqueue after clear", () => commands.Enqueue(DeferredRuntimeCommand.ForGodotFree("/root/AfterDispose")));
    }

    private static void AssertThrows<TException>(string name, Action action)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            return;
        }

        throw new InvalidOperationException($"{name}: expected {typeof(TException).Name}");
    }

    private static void AssertEqual<T>(string name, T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"{name}: expected {expected}, actual {actual}");
        }
    }

    private readonly record struct ExternalTestEvent(int Value) : IGlobalEvent;

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
