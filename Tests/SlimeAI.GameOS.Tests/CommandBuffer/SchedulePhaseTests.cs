using SlimeAI.GameOS.Runtime.CommandBuffer;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Schedule;
using SlimeAI.GameOS.Runtime.World;

internal static class SchedulePhaseTests
{
    public static void RunPhaseCoversFixedPhases()
    {
        using var world = RuntimeWorld.CreateScoped();

        AssertEqual("BeginTick phase", SchedulePhase.BeginTick, world.Schedule.RunPhase(SchedulePhase.BeginTick).Phase);
        AssertEqual("BeforeSystemTick phase", SchedulePhase.BeforeSystemTick, world.Schedule.RunPhase(SchedulePhase.BeforeSystemTick).Phase);
        AssertEqual("AfterSystemTick phase", SchedulePhase.AfterSystemTick, world.Schedule.RunPhase(SchedulePhase.AfterSystemTick).Phase);
        AssertEqual("AfterEventDispatch phase", SchedulePhase.AfterEventDispatch, world.Schedule.RunPhase(SchedulePhase.AfterEventDispatch).Phase);
        AssertEqual("EndOfFrame phase", SchedulePhase.EndOfFrame, world.Schedule.RunPhase(SchedulePhase.EndOfFrame).Phase);
        AssertEqual("Manual phase", SchedulePhase.Manual, world.Schedule.RunPhase(SchedulePhase.Manual).Phase);
    }

    public static void RunPhaseOnlyPlaysMatchingPhase()
    {
        using var world = RuntimeWorld.CreateScoped();
        world.Commands.Enqueue(DeferredRuntimeCommand.ForSpawn(
            new EntitySpawnConfig { EntityId = new EntityId("phase-before") },
            new EntityId("phase-before"),
            SchedulePhase.BeforeSystemTick,
            "test",
            "before"));
        world.Commands.Enqueue(DeferredRuntimeCommand.ForSpawn(
            new EntitySpawnConfig { EntityId = new EntityId("phase-manual") },
            new EntityId("phase-manual"),
            SchedulePhase.Manual,
            "test",
            "manual"));

        var before = world.Schedule.RunPhase(SchedulePhase.BeforeSystemTick);

        AssertEqual("before played count", 1, before.PlayedCount);
        AssertEqual("before entity registered", new EntityId("phase-before"), world.Entities.Get(new EntityId("phase-before"))?.EntityId);
        AssertEqual("manual still pending", null, world.Entities.Get(new EntityId("phase-manual")));
        AssertEqual("pending count", 1, world.Commands.QueuedCount);

        var manual = world.Schedule.RunPhase(SchedulePhase.Manual);

        AssertEqual("manual played count", 1, manual.PlayedCount);
        AssertEqual("manual entity registered", new EntityId("phase-manual"), world.Entities.Get(new EntityId("phase-manual"))?.EntityId);
    }

    public static void ManualPhaseIsNotPlayedByFramePhases()
    {
        using var world = RuntimeWorld.CreateScoped();
        world.Commands.Enqueue(DeferredRuntimeCommand.ForSpawn(
            new EntitySpawnConfig { EntityId = new EntityId("phase-manual-only") },
            new EntityId("phase-manual-only"),
            SchedulePhase.Manual,
            "test",
            "manual-only"));

        world.Schedule.RunPhase(SchedulePhase.BeginTick);
        world.Schedule.RunPhase(SchedulePhase.BeforeSystemTick);
        world.Schedule.RunPhase(SchedulePhase.AfterSystemTick);
        world.Schedule.RunPhase(SchedulePhase.AfterEventDispatch);
        world.Schedule.RunPhase(SchedulePhase.EndOfFrame);

        AssertEqual("manual not auto-played", null, world.Entities.Get(new EntityId("phase-manual-only")));
        AssertEqual("manual still queued", 1, world.Commands.QueuedCount);
    }

    public static void RunPhaseAfterScheduleClearThrows()
    {
        using var world = RuntimeWorld.CreateScoped();
        var schedule = world.Schedule;

        schedule.Clear();

        AssertThrows<ObjectDisposedException>("run phase after clear", () => schedule.RunPhase(SchedulePhase.Manual));
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
}
