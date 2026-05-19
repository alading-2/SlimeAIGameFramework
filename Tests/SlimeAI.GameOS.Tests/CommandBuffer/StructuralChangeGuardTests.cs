using SlimeAI.GameOS.Capabilities.Damage;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Schedule;
using SlimeAI.GameOS.Runtime.World;
using static TestAssert;

internal static class StructuralChangeGuardTests
{
    public static void EnterGuardTracksNestedScopes()
    {
        using var world = RuntimeWorld.CreateScoped();

        AssertEqual("not guarded initially", false, world.Commands.IsGuarded);
        using (world.Commands.EnterGuard("outer"))
        {
            AssertEqual("outer guarded", true, world.Commands.IsGuarded);
            using (world.Commands.EnterGuard("inner"))
            {
                AssertEqual("inner guarded", true, world.Commands.IsGuarded);
            }

            AssertEqual("outer remains guarded", true, world.Commands.IsGuarded);
        }

        AssertEqual("guard released", false, world.Commands.IsGuarded);
    }

    public static void GuardedSpawnQueuesReservedEntity()
    {
        using var world = RuntimeWorld.CreateScoped();
        RuntimeEntity entity;

        using (world.Commands.EnterGuard("test-guard"))
        {
            entity = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("guard-spawn") });
            entity.Data.Set(DamageDataKeys.CurrentHp, 12f);

            AssertEqual("reserved id", new EntityId("guard-spawn"), entity.EntityId);
            AssertEqual("not registered in guard", null, world.Entities.Get(entity.EntityId));
            AssertEqual("queued in guard", 1, world.Commands.QueuedCount);
        }

        AssertEqual("still pending after guard", 1, world.Commands.QueuedCount);

        var report = world.Schedule.RunPhase(SchedulePhase.EndOfFrame);

        AssertEqual("spawn played", 1, report.PlayedCount);
        AssertEqual("registered after playback", entity.EntityId, world.Entities.Get(entity.EntityId)?.EntityId);
        AssertEqual("guard data retained", 12f, world.Entities.Get(entity.EntityId)?.Data.Get(DamageDataKeys.CurrentHp));
    }

    public static void GuardedDestroyAndLifecycleMutationsQueue()
    {
        using var world = RuntimeWorld.CreateScoped();
        var parent = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("guard-parent") });
        var child = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("guard-child") });
        var doomed = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("guard-doomed") });

        using (world.Commands.EnterGuard("test-guard"))
        {
            AssertEqual("attach accepted", true, world.Lifecycle.Attach(parent.EntityId, child.EntityId));
            AssertEqual("attach not immediate", false, world.Lifecycle.IsAttached(parent.EntityId, child.EntityId));
            AssertEqual("destroy accepted", true, world.Entities.Destroy(doomed.EntityId));
            AssertEqual("destroy not immediate", doomed.EntityId, world.Entities.Get(doomed.EntityId)?.EntityId);
        }

        world.Schedule.RunPhase(SchedulePhase.EndOfFrame);

        AssertEqual("attach applied", true, world.Lifecycle.IsAttached(parent.EntityId, child.EntityId));
        AssertEqual("destroy applied", null, world.Entities.Get(doomed.EntityId));
    }

    public static void GuardDisposesAfterException()
    {
        using var world = RuntimeWorld.CreateScoped();

        try
        {
            using var guard = world.Commands.EnterGuard("throws");
            throw new InvalidOperationException("expected");
        }
        catch (InvalidOperationException)
        {
        }

        AssertEqual("guard released after exception", false, world.Commands.IsGuarded);
    }

    public static void GuardScopeCommandLimitThrows()
    {
        using var world = RuntimeWorld.CreateScoped();

        using var guard = world.Commands.EnterGuard("limit");
        for (var i = 0; i < 1000; i++)
        {
            world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId($"guard-limit-{i}") });
        }

        AssertThrows<InvalidOperationException>(
            "guard command limit",
            () => world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("guard-limit-overflow") }));
    }

}
