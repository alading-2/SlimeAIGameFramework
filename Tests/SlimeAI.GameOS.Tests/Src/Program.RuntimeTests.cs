using SlimeAI.GameOS.Capabilities.Damage;
using SlimeAI.GameOS.Capabilities.Projectile;
using SlimeAI.GameOS.Runtime.Data;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;
using SlimeAI.GameOS.Runtime.Events.Core;
using SlimeAI.GameOS.Runtime.Pool;
using SlimeAI.GameOS.Runtime.Resource;
using SlimeAI.GameOS.Runtime.Schedule;
using SlimeAI.GameOS.Runtime.Timer;
using SlimeAI.GameOS.Runtime.World;
using DamageEvents = SlimeAI.GameOS.Capabilities.Damage.Events;
using static TestAssert;

internal partial class Program
{
    static void TestEventBus()
    {
        var bus = new EntityEventBus("entity:test");
        var order = new List<int>();

        bus.Subscribe<RuntimeTestEvent>(_ => order.Add(1));
        bus.Subscribe<RuntimeTestEvent>(_ => order.Add(2));
        IDisposable? third = null;
        third = bus.Subscribe<RuntimeTestEvent>(_ =>
        {
            order.Add(3);
            third?.Dispose();
        });

        bus.Publish(new RuntimeTestEvent(7));
        bus.Publish(new RuntimeTestEvent(8));

        AssertEqual("registration order first emit", "1,2,3", string.Join(",", order.Take(3)));
        AssertEqual("dispose unsubscribes", "1,2,3,1,2", string.Join(",", order));
    }

    static void TestEntityDataBridge()
    {
        var entity = new RuntimeEntity(new EntityId("entity-data"));
        DataChangedEventData? received = null;
        entity.Events.Subscribe<DataPropertyChanged>(data => received = data.Change);

        entity.Data.Set(DamageDataKeys.CurrentHp, 10f);

        AssertEqual("data event key", "Damage.CurrentHp", received?.StableKey);
        AssertEqual("data event new value", 10f, received?.NewValue);
    }

    static void TestEntityLifecycle()
    {
        using var world = RuntimeWorld.CreateScoped();
        var spawned = 0;
        var destroyed = 0;
        var spawnSub = world.Events.Subscribe<EntitySpawned>(_ => spawned++);
        var destroySub = world.Events.Subscribe<EntityDestroyed>(_ => destroyed++);

        var entity = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("entity-life") });

        AssertEqual("entity found", new EntityId("entity-life"), world.Entities.Get(new EntityId("entity-life"))?.EntityId);
        AssertEqual("spawned event", 1, spawned);
        AssertEqual("destroy result", true, world.Entities.Destroy(entity));
        AssertEqual("destroyed event", 1, destroyed);

        spawnSub.Dispose();
        destroySub.Dispose();
    }

    static void TestLifecycleTree()
    {
        using var world = RuntimeWorld.CreateScoped();

        var attached = 0;
        var detached = 0;
        var attachSub = world.Events.Subscribe<LifecycleChildAttached>(_ => attached++);
        var detachSub = world.Events.Subscribe<LifecycleChildDetached>(_ => detached++);

        var parent1 = new EntityId("lifecycle-parent-1");
        var parent2 = new EntityId("lifecycle-parent-2");
        var child = new EntityId("lifecycle-child");

        // Attach 成功并发布事件。
        AssertEqual("attach first parent", true, world.Lifecycle.Attach(parent1, child, ParentDestroyPolicy.DestroyRecursively, priority: 7));
        AssertEqual("attach event count", 1, attached);
        AssertEqual("isAttached after attach", true, world.Lifecycle.IsAttached(parent1, child));
        AssertEqual("getParentEntityId", parent1, world.Lifecycle.GetParentEntityId(child));
        AssertEqual("getChildEntityIds count", 1, world.Lifecycle.GetChildEntityIds(parent1).Count);
        AssertEqual("getChildEntityIds entry", child, world.Lifecycle.GetChildEntityIds(parent1)[0]);

        var links = world.Lifecycle.GetChildren(parent1);
        AssertEqual("link parent", parent1, links[0].ParentEntityId);
        AssertEqual("link policy", ParentDestroyPolicy.DestroyRecursively, links[0].DestroyPolicy);
        AssertEqual("link priority", 7, links[0].Priority);

        // 单 parent 假设：第二个 parent attach 失败。
        AssertEqual("attach second parent blocked", false, world.Lifecycle.Attach(parent2, child));
        AssertEqual("attach event still 1", 1, attached);

        // 自挂、Empty、cycle 拒绝。
        AssertEqual("self attach rejected", false, world.Lifecycle.Attach(parent1, parent1));
        AssertEqual("empty parent rejected", false, world.Lifecycle.Attach(EntityId.Empty, child));
        AssertEqual("empty child rejected", false, world.Lifecycle.Attach(parent1, EntityId.Empty));

        var grandchild = new EntityId("lifecycle-grandchild");
        world.Lifecycle.Attach(child, grandchild);
        AssertEqual("cycle attach rejected", false, world.Lifecycle.Attach(grandchild, parent1));

        // Detach 触发事件并清状态。
        AssertEqual("detach success", true, world.Lifecycle.Detach(parent1, child));
        AssertEqual("detach event count", 1, detached);
        AssertEqual("getParentEntityId after detach", EntityId.Empty, world.Lifecycle.GetParentEntityId(child));
        AssertEqual("isAttached after detach", false, world.Lifecycle.IsAttached(parent1, child));

        // Detach 不存在 link 返回 false。
        AssertEqual("detach absent link", false, world.Lifecycle.Detach(parent1, child));

        // 重 attach 到新 parent 成功。
        AssertEqual("re-attach to new parent", true, world.Lifecycle.Attach(parent2, child));

        // DetachAll 清掉 entity 所有参与（child 与 parent 两侧）。
        world.Lifecycle.DetachAll(child);
        AssertEqual("detachAll removes child link", false, world.Lifecycle.IsAttached(parent2, child));
        AssertEqual("detachAll removes parent links", 0, world.Lifecycle.GetChildEntityIds(child).Count);

        attachSub.Dispose();
        detachSub.Dispose();
    }

    static void TestEntityIdList()
    {
        AssertEqual("empty count", 0, EntityIdList.Empty.Count);
        AssertEqual("default count", 0, default(EntityIdList).Count);

        var a = new EntityId("a");
        var b = new EntityId("b");
        var listAb = EntityIdList.Empty.Add(a).Add(b);
        AssertEqual("add count", 2, listAb.Count);
        AssertEqual("add ordering preserved", a, listAb[0]);
        AssertEqual("contains true", true, listAb.Contains(a));
        AssertEqual("contains false", false, listAb.Contains(new EntityId("c")));

        // Add 已存在不会重复加。
        var listAbAgain = listAb.Add(a);
        AssertEqual("dedup add count", 2, listAbAgain.Count);

        // value-equality：不同底层 list，内容相同也相等。
        var listAb2 = EntityIdList.Empty.Add(a).Add(b);
        AssertEqual("value equality", true, listAb.Equals(listAb2));
        AssertEqual("hash equality", listAb.GetHashCode(), listAb2.GetHashCode());

        // Remove 返回新值。
        var listOnlyB = listAb.Remove(a);
        AssertEqual("remove count", 1, listOnlyB.Count);
        AssertEqual("remove leftover entry", b, listOnlyB[0]);
        AssertEqual("immutability original count", 2, listAb.Count);

        // 移除所有元素回到 Empty。
        var listEmpty = listOnlyB.Remove(b);
        AssertEqual("empty after remove all", true, listEmpty.Equals(EntityIdList.Empty));

        // EntityId.Empty 作为元素能正常处理。
        var listWithEmpty = EntityIdList.Empty.Add(EntityId.Empty).Add(a);
        AssertEqual("empty element contains", true, listWithEmpty.Contains(EntityId.Empty));
        var listAfterRemoveEmpty = listWithEmpty.Remove(EntityId.Empty);
        AssertEqual("empty element removed", false, listAfterRemoveEmpty.Contains(EntityId.Empty));
    }

    static void TestRuntimeOwnedReferenceRegistry()
    {
        using var world = RuntimeWorld.CreateScoped();
        RuntimeOwnedReferenceRegistry.Clear();
        ProjectileDataKeys.RegisterAll();

        // 注册 typed descriptor：source -> SpawnedProjectileIds。
        RuntimeOwnedReferenceRegistry.Register(new OwnedReferenceDescriptor(
            ProjectileDataKeys.SourceEntity,
            ProjectileDataKeys.SpawnedProjectileIds));

        var source = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("owner-source") });
        var child1 = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("owner-child-1") });
        var child2 = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("owner-child-2") });

        // capability 手动维护 spawn 一致性。
        child1.Data.Set(ProjectileDataKeys.SourceEntity, source.EntityId);
        child2.Data.Set(ProjectileDataKeys.SourceEntity, source.EntityId);
        source.Data.Set(ProjectileDataKeys.SpawnedProjectileIds,
            EntityIdList.Empty.Add(child1.EntityId).Add(child2.EntityId));

        AssertEqual("owner list contains child1", true,
            source.Data.Get(ProjectileDataKeys.SpawnedProjectileIds).Contains(child1.EntityId));

        // destroy child1：framework 自动从 owner list 移除。
        world.Entities.Destroy(child1);
        var listAfterDestroy = source.Data.Get(ProjectileDataKeys.SpawnedProjectileIds);
        AssertEqual("owner list child1 removed", false, listAfterDestroy.Contains(child1.EntityId));
        AssertEqual("owner list child2 retained", true, listAfterDestroy.Contains(child2.EntityId));

        // 自定义 cleaner 也会被回调。
        var cleaner = new TestOwnedReferenceCleaner();
        RuntimeOwnedReferenceRegistry.Register(cleaner);
        world.Entities.Destroy(child2);
        AssertEqual("cleaner invoked once", 1, cleaner.CallCount);
        AssertEqual("cleaner sees destroyed entity", child2.EntityId, cleaner.LastEntityId);

        // Clear 后 descriptor 不再生效。
        RuntimeOwnedReferenceRegistry.Clear();
        var child3 = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("owner-child-3") });
        child3.Data.Set(ProjectileDataKeys.SourceEntity, source.EntityId);
        source.Data.Set(ProjectileDataKeys.SpawnedProjectileIds,
            EntityIdList.Empty.Add(child3.EntityId));
        world.Entities.Destroy(child3);
        AssertEqual("after Clear descriptor inert",
            true,
            source.Data.Get(ProjectileDataKeys.SpawnedProjectileIds).Contains(child3.EntityId));

        RuntimeOwnedReferenceRegistry.Clear();
    }

    static void TestEntityParentDestroyPolicy()
    {
        using var world = RuntimeWorld.CreateScoped();

        var parent = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("parent") });
        var recursiveChild = world.Entities.Spawn(new EntitySpawnConfig
        {
            EntityId = new EntityId("child-recursive"),
            ParentEntityId = parent.EntityId,
            ParentDestroyPolicy = ParentDestroyPolicy.DestroyRecursively
        });
        var detachedChild = world.Entities.Spawn(new EntitySpawnConfig
        {
            EntityId = new EntityId("child-detached"),
            ParentEntityId = parent.EntityId,
            ParentDestroyPolicy = ParentDestroyPolicy.Detach
        });

        AssertEqual("recursive child registered", recursiveChild.EntityId, world.Entities.Get(recursiveChild.EntityId)?.EntityId);
        AssertEqual("detached child policy", ParentDestroyPolicy.Detach, world.Lifecycle.GetChildren(parent.EntityId)[1].DestroyPolicy);

        world.Entities.Destroy(parent);

        AssertEqual("recursive child destroyed", null, world.Entities.Get(recursiveChild.EntityId));
        AssertEqual("detached child alive", detachedChild.EntityId, world.Entities.Get(detachedChild.EntityId)?.EntityId);
        AssertEqual("detached parent link removed", false, world.Lifecycle.IsAttached(parent.EntityId, detachedChild.EntityId));
    }

    static void TestSystemRunCondition()
    {
        var condition = SystemRunCondition.GameplayRunning();
        var running = new ProjectStateSnapshot(GameFlowState.SessionPlaying, OverlayFlags.None, SimulationState.Running);
        var paused = new ProjectStateSnapshot(GameFlowState.SessionPlaying, OverlayFlags.PauseMenu, SimulationState.Running);

        AssertEqual("condition running", true, condition.Evaluate(running));
        AssertEqual("condition paused", true, condition.GetBlockedReason(paused).IsBlocked);
    }

    static void TestRuntimeSchedule()
    {
        var schedule = new RuntimeSchedule();
        schedule.Register(
            new SystemDescriptor("probe", static () => new ScheduleProbeSystem()),
            new SystemConfig
            {
                SystemId = "probe",
                Group = SystemGroup.Gameplay,
                Tags = SystemTag.Gameplay | SystemTag.Runtime,
                RunCondition = SystemRunCondition.GameplayRunning()
            });

        schedule.Bootstrap();
        var probe = schedule.Resolve<ScheduleProbeSystem>() ?? throw new InvalidOperationException("probe missing");

        AssertEqual("schedule registered", 1, probe.Registered);
        AssertEqual("schedule blocked on boot", false, schedule.GetRuntimeInfo()[0].IsRunning);

        schedule.ProjectState.BeginGameplaySession();
        AssertEqual("schedule started", 1, probe.Started);
        AssertEqual("schedule command success", true, schedule.Execute<ScheduleProbeSystem, int, int>(2).Success);
        AssertEqual("schedule command value", 3, schedule.Execute<ScheduleProbeSystem, int, int>(2).Value);

        schedule.ProjectState.OpenPauseMenu();
        AssertEqual("schedule stopped", 1, probe.Stopped);
        AssertEqual("schedule command blocked", false, schedule.Execute<ScheduleProbeSystem, int, int>(2).Success);

        schedule.Clear();
    }

    static void TestObjectPool()
    {
        ObjectPoolManager.DestroyAll();
        var created = 0;
        var pool = new ObjectPool<PooledProbe>(
            () =>
            {
                created++;
                return new PooledProbe();
            },
            new ObjectPoolConfig
            {
                Name = "probe",
                InitialSize = 1,
                MaxSize = 2
            });

        var first = pool.Get();
        ObjectPoolManager.ReturnToPool(first);
        var second = pool.Get();

        AssertEqual("pooled reused instance", true, ReferenceEquals(first, second));
        AssertEqual("created count", 1, created);
        AssertEqual("acquire lifecycle", 2, second.Acquired);

        pool.Destroy();
    }

    static void TestTimerManager()
    {
        var manager = new TimerManager("test-timers");
        var completed = false;
        var loopCount = 0;

        manager.Delay(0.5f).OnComplete(() => completed = true);
        manager.Repeat(0.25f, 2).OnLoop(() => loopCount++);

        manager.Tick(0.25f);
        AssertEqual("delay before complete", false, completed);
        manager.Tick(0.25f);
        AssertEqual("delay complete", true, completed);
        AssertEqual("repeat loops", 2, loopCount);

        manager.Clear();
    }

    static void TestResourceCatalog()
    {
        using var world = RuntimeWorld.CreateScoped();
        world.Resources.Register("Main", ResourceCategory.Entity, "res://Scenes/Main.tscn");

        AssertEqual("resource found", true, world.Resources.TryGet("Main", ResourceCategory.Entity, out var data));
        AssertEqual("resource path", "res://Scenes/Main.tscn", data.Path);
        AssertEqual("resource key count", 1, world.Resources.GetKeys(ResourceCategory.Entity).Count);
    }

    sealed class PooledProbe : IPoolable
    {
        public int Acquired { get; private set; }

        public void OnPoolAcquire()
        {
            Acquired++;
        }
    }
}

enum RuntimeDataTestCategory
{
    Runtime,
    Config
}

internal partial class Program
{
    sealed class RecordingDataChangeSink : IDataChangeSink
    {
        public DataChangedEventData? LastChange { get; private set; }

        public void OnDataChanged(DataChangedEventData change)
        {
            LastChange = change;
        }
    }

    sealed class ScheduleProbeSystem : IRuntimeSystem, IRuntimeCommandHandler<int, int>
    {
        public int Registered { get; private set; }
        public int Started { get; private set; }
        public int Stopped { get; private set; }

        public void OnRegistered(SystemRegistrationContext context)
        {
            Registered++;
        }

        public void OnStarted(ProjectStateSnapshot snapshot)
        {
            Started++;
        }

        public void OnStopped(ProjectStateSnapshot snapshot)
        {
            Stopped++;
        }

        public int Execute(int request)
        {
            return request + 1;
        }
    }

    readonly record struct RuntimeTestEvent(int Value) : IEntityEvent;

    sealed class TestOwnedReferenceCleaner : IOwnedReferenceCleaner
    {
        public int CallCount { get; private set; }
        public EntityId LastEntityId { get; private set; }

        public void OnEntityDestroying(IEntity destroyed)
        {
            CallCount++;
            LastEntityId = destroyed.EntityId;
        }
    }
}
