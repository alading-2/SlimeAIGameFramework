using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;
using SlimeAI.GameOS.Runtime.Events.Core;
using SlimeAI.GameOS.Runtime.Pool;
using SlimeAI.GameOS.Runtime.Resource;
using SlimeAI.GameOS.Runtime.World;

internal static class RuntimeWorldTests
{
    public static void DefaultSingleton()
    {
        AssertEqual("default singleton", true, ReferenceEquals(RuntimeWorld.Default, RuntimeWorld.Default));
        AssertEqual("default flag", true, RuntimeWorld.Default.IsDefault);
        AssertEqual("default not disposed", false, RuntimeWorld.Default.IsDisposed);
    }

    public static void DefaultCannotDispose()
    {
        try
        {
            RuntimeWorld.Default.Dispose();
        }
        catch (InvalidOperationException ex)
        {
            AssertEqual("default dispose message", "RuntimeWorld.Default cannot be disposed", ex.Message);
            return;
        }

        throw new InvalidOperationException("RuntimeWorld.Default.Dispose should throw InvalidOperationException");
    }

    public static void CreateScopedIsolatesSubsystems()
    {
        using var w1 = RuntimeWorld.CreateScoped();
        using var w2 = RuntimeWorld.CreateScoped();

        var parent = w1.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("runtime-world-parent") });
        var child = w1.Entities.Spawn(new EntitySpawnConfig
        {
            EntityId = new EntityId("runtime-world-child"),
            ParentEntityId = parent.EntityId
        });
        w1.Resources.Register("runtime-world-resource", ResourceCategory.Entity, "res://RuntimeWorld.tscn");
        w1.Pools.RegisterPool(new TestPool("runtime-world-pool"));

        var w1Events = 0;
        var w2Events = 0;
        using var w1Sub = w1.Events.Subscribe<TestWorldEvent>(_ => w1Events++);
        using var w2Sub = w2.Events.Subscribe<TestWorldEvent>(_ => w2Events++);

        w1.Events.Publish(new TestWorldEvent(1));

        AssertEqual("w1 entity visible", child.EntityId, w1.Entities.Get(child.EntityId)?.EntityId);
        AssertEqual("w2 entity isolated", null, w2.Entities.Get(child.EntityId));
        AssertEqual("default entity isolated", null, RuntimeWorld.Default.Entities.Get(child.EntityId));
        AssertEqual("w1 lifecycle visible", true, w1.Lifecycle.IsAttached(parent.EntityId, child.EntityId));
        AssertEqual("w2 lifecycle isolated", false, w2.Lifecycle.IsAttached(parent.EntityId, child.EntityId));
        AssertEqual("w1 resource visible", true, w1.Resources.TryGet("runtime-world-resource", ResourceCategory.Entity, out _));
        AssertEqual("w2 resource isolated", false, w2.Resources.TryGet("runtime-world-resource", ResourceCategory.Entity, out _));
        AssertEqual("default resource isolated", false, RuntimeWorld.Default.Resources.TryGet("runtime-world-resource", ResourceCategory.Entity, out _));
        AssertEqual("w1 pool visible", true, w1.Pools.GetNamedPool("runtime-world-pool") != null);
        AssertEqual("w2 pool isolated", null, w2.Pools.GetNamedPool("runtime-world-pool"));
        AssertEqual("default pool isolated", null, RuntimeWorld.Default.Pools.GetNamedPool("runtime-world-pool"));
        AssertEqual("w1 event delivered", 1, w1Events);
        AssertEqual("w2 event isolated", 0, w2Events);
    }

    public static void DisposeOrderIsDeterministic()
    {
        var order = new List<string>();
        var world = RuntimeWorld.CreateScoped(order.Add);

        world.Dispose();

        AssertEqual("dispose order", "Schedule,Commands,Pools,Resources,Lifecycle,Entities,Events", string.Join(",", order));
    }

    public static void DisposePublishesTeardownEventsBeforeEventsClear()
    {
        var world = RuntimeWorld.CreateScoped();
        var detached = 0;
        var destroyed = 0;
        using var detachSub = world.Events.Subscribe<LifecycleChildDetached>(_ => detached++);
        using var destroySub = world.Events.Subscribe<EntityDestroyed>(_ => destroyed++);

        var parent = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("runtime-world-dispose-parent") });
        world.Entities.Spawn(new EntitySpawnConfig
        {
            EntityId = new EntityId("runtime-world-dispose-child"),
            ParentEntityId = parent.EntityId
        });

        world.Dispose();

        AssertEqual("lifecycle detach during dispose", 1, detached);
        AssertEqual("entity destroy during dispose", 2, destroyed);
    }

    public static void DisposeAfterAccessThrows()
    {
        var world = RuntimeWorld.CreateScoped();
        world.Dispose();

        AssertThrows<ObjectDisposedException>("entities getter after dispose", () => _ = world.Entities);
        AssertThrows<ObjectDisposedException>("lifecycle getter after dispose", () => _ = world.Lifecycle);
        AssertThrows<ObjectDisposedException>("events getter after dispose", () => _ = world.Events);
        AssertThrows<ObjectDisposedException>("resources getter after dispose", () => _ = world.Resources);
        AssertThrows<ObjectDisposedException>("pools getter after dispose", () => _ = world.Pools);
        AssertThrows<ObjectDisposedException>("schedule getter after dispose", () => _ = world.Schedule);
        AssertThrows<ObjectDisposedException>("commands getter after dispose", () => _ = world.Commands);
    }

    public static void SubsystemHandlesArePresent()
    {
        using var world = RuntimeWorld.CreateScoped();

        AssertEqual("entities handle", true, world.Entities != null);
        AssertEqual("lifecycle handle", true, world.Lifecycle != null);
        AssertEqual("events handle", true, world.Events != null);
        AssertEqual("resources handle", true, world.Resources != null);
        AssertEqual("pools handle", true, world.Pools != null);
        AssertEqual("schedule handle", true, world.Schedule != null);
        AssertEqual("commands handle", true, world.Commands != null);
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

    private readonly record struct TestWorldEvent(int Value) : IGlobalEvent;

    private sealed class TestPool : IObjectPool
    {
        public TestPool(string poolName)
        {
            PoolName = poolName;
        }

        public string PoolName { get; }

        public bool ReleaseObject(object instance)
        {
            return false;
        }

        public PoolStats GetStats()
        {
            return new PoolStats(PoolName, 0, 0, 0, 0, 0, 0, 0, 0);
        }

        public void Destroy()
        {
        }
    }
}
