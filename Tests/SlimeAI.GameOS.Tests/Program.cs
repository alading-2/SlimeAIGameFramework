using SlimeAI.GameOS.Capabilities.Movement;
using SlimeAI.GameOS.Capabilities.Collision;
using SlimeAI.GameOS.Capabilities.Damage;
using SlimeAI.GameOS.Capabilities.Effect;
using SlimeAI.GameOS.Capabilities.Ability;
using SlimeAI.GameOS.Capabilities.Feature;
using SlimeAI.GameOS.Capabilities.AI;
using SlimeAI.GameOS.Capabilities.Attack;
using SlimeAI.GameOS.Capabilities.Projectile;
using SlimeAI.GameOS.Observation;
using SlimeAI.GameOS.Runtime.Data;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;
using SlimeAI.GameOS.Runtime.Events.Core;
using SlimeAI.GameOS.Runtime.Pool;
using SlimeAI.GameOS.Runtime.Resource;
using SlimeAI.GameOS.Runtime.Schedule;
using SlimeAI.GameOS.Runtime.Timer;
using SlimeAI.GameOS.Runtime.World;
using AbilityEvents = SlimeAI.GameOS.Capabilities.Ability.Events;
using AttackEvents = SlimeAI.GameOS.Capabilities.Attack.Events;
using CollisionEvents = SlimeAI.GameOS.Capabilities.Collision.Events;
using DamageEvents = SlimeAI.GameOS.Capabilities.Damage.Events;
using EffectEvents = SlimeAI.GameOS.Capabilities.Effect.Events;
using MovementEvents = SlimeAI.GameOS.Capabilities.Movement.Events;
using ProjectileEvents = SlimeAI.GameOS.Capabilities.Projectile.Events;

var tests = new (string Name, Action Run)[]
{
    ("EventBus subscribe publish and dispose", TestEventBus),
    ("EventBus reentry same type is blocked", EventBusTests.ReentrySameTypeIsBlocked),
    ("EventBus different type cascade allowed", EventBusTests.DifferentTypeCascadeAllowed),
    ("EventBus different bus same type allowed", EventBusTests.DifferentBusSameTypeAllowed),
    ("EventBus handler exception is isolated", EventBusTests.HandlerExceptionIsIsolated),
    ("EventBus dispose unsubscribes", EventBusTests.DisposeUnsubscribes),
    ("EventBus registration order preserved", EventBusTests.RegistrationOrderPreserved),
    ("EventBus broadcast routes to both buses", EventBusTests.BroadcastRoutesToBothBuses),
    ("EventBus entity event on world bus rejected", EventBusTests.EntityEventOnWorldBusRejected),
    ("EventBus global event on entity bus rejected", EventBusTests.GlobalEventOnEntityBusRejected),
    ("RuntimeWorld default singleton", RuntimeWorldTests.DefaultSingleton),
    ("RuntimeWorld default cannot dispose", RuntimeWorldTests.DefaultCannotDispose),
    ("RuntimeWorld scoped subsystem isolation", RuntimeWorldTests.CreateScopedIsolatesSubsystems),
    ("RuntimeWorld dispose order", RuntimeWorldTests.DisposeOrderIsDeterministic),
    ("RuntimeWorld dispose publishes teardown events", RuntimeWorldTests.DisposePublishesTeardownEventsBeforeEventsClear),
    ("RuntimeWorld access after dispose throws", RuntimeWorldTests.DisposeAfterAccessThrows),
    ("RuntimeWorld subsystem handles present", RuntimeWorldTests.SubsystemHandlesArePresent),
    ("Entity Data event bridge", TestEntityDataBridge),
    ("EntityId value equality", EntityIdTests.ValueEqualityWorksAcrossSameKey),
    ("EntityId empty/default/null all equal", EntityIdTests.EmptyDefaultAndFromNullAllEqual),
    ("EntityId IsEmpty for null/empty/whitespace", EntityIdTests.IsEmptyTreatsNullAndEmptyAsEmpty),
    ("EntityId Value preserves underlying string", EntityIdTests.ValuePreservesUnderlyingString),
    ("Entity lifecycle", TestEntityLifecycle),
    ("LifecycleTree attach detach single-parent", TestLifecycleTree),
    ("EntityIdList immutable value-equality", TestEntityIdList),
    ("RuntimeOwnedReferenceRegistry cleanup descriptor", TestRuntimeOwnedReferenceRegistry),
    ("Entity parent destroy policy", TestEntityParentDestroyPolicy),
    ("SystemRunCondition gates", TestSystemRunCondition),
    ("RuntimeSchedule lifecycle and command", TestRuntimeSchedule),
    ("ObjectPool reuse", TestObjectPool),
    ("TimerManager tick", TestTimerManager),
    ("ResourceCatalog mapping", TestResourceCatalog),
    ("Runtime typed Data API", TestRuntimeTypedDataApi),
    ("Runtime typed Data modifier constraints", TestRuntimeTypedDataModifierConstraints),
    ("Runtime typed Data computed cache", TestRuntimeTypedDataComputedCache),
    ("Runtime DataOS snapshot applies data and resources", TestRuntimeDataSnapshotAppliesDataAndResources),
    ("Runtime DataOS snapshot rejects invalid fields", TestRuntimeDataSnapshotRejectsInvalidFields),
    ("Runtime DataOS snapshot trims disabled resources", TestRuntimeDataSnapshotRejectsDisabledResources),
    ("Collision layer mask event", TestCollisionLayerMaskEvent),
    ("Damage service applies health", TestDamageServiceAppliesHealth),
    ("Damage service killed event", TestDamageServiceKilledEvent),
    ("Damage pipeline dodge", TestDamagePipelineDodge),
    ("Damage pipeline critical armor and stats", TestDamagePipelineCriticalArmorAndStats),
    ("Damage pipeline true damage bypasses dodge and armor", TestDamagePipelineTrueDamageBypassesDodgeAndArmor),
    ("Damage pipeline lifesteal", TestDamagePipelineLifesteal),
    ("Damage pipeline shield", TestDamagePipelineShield),
    ("Heal service applies and clamps", TestHealServiceAppliesAndClamps),
    ("DamageTool multi target and periodic", TestDamageToolMultiTargetAndPeriodic),
    ("Ability service instant damage cooldown and charge", TestAbilityServiceInstantDamageCooldownAndCharge),
    ("Ability service requires target", TestAbilityServiceRequiresTarget),
    ("Ability service accepts point target", TestAbilityServiceAcceptsPointTarget),
    ("Ability service accepts entity or point target", TestAbilityServiceAcceptsEntityOrPointTarget),
    ("Ability service periodic damage", TestAbilityServicePeriodicDamage),
    ("Ability service periodic auto trigger", TestAbilityServicePeriodicAutoTrigger),
    ("Ability targeting tool finds nearest entity", TestAbilityTargetingToolFindsNearestEntity),
    ("Projectile tool spawns runtime entity", TestProjectileToolSpawnsRuntimeEntity),
    ("Projectile movement hit damages and destroys", TestProjectileMovementHitDamagesAndDestroys),
    ("Projectile movement pierces and destroys after max hits", TestProjectileMovementPiercesAndDestroysAfterMaxHits),
    ("Projectile movement lifetime destroys", TestProjectileMovementLifetimeDestroys),
    ("Effect tool spawns runtime entity", TestEffectToolSpawnsRuntimeEntity),
    ("Feature service grants modifiers and removes", TestFeatureServiceGrantsModifiersAndRemoves),
    ("Feature service lifecycle handler", TestFeatureServiceLifecycleHandler),
    ("Ability service invokes feature handler", TestAbilityServiceInvokesFeatureHandler),
    ("AI service moves toward target", TestAIServiceMovesTowardTarget),
    ("AI service finds nearest target", TestAIServiceFindsNearestTarget),
    ("AI service patrol action", TestAIServicePatrolAction),
    ("AI behavior tree builder melee attack priority", TestAIBehaviorTreeBuilderMeleeAttackPriority),
    ("AI behavior tree builder patrol fallback", TestAIBehaviorTreeBuilderPatrolFallback),
    ("AI service requests attack in range", TestAIServiceRequestsAttackInRange),
    ("AI service ability auto trigger action", TestAIServiceAbilityAutoTriggerAction),
    ("AI service prepares ability auto target contexts", TestAIServicePreparesAbilityAutoTargetContexts),
    ("Attack service consumes request and damages", TestAttackServiceConsumesRequestAndDamages),
    ("Attack service gates range and cooldown", TestAttackServiceGatesRangeAndCooldown),
    ("Attack service windup and recovery timers", TestAttackServiceWindupAndRecoveryTimers),
    ("Movement charge tick", TestMovementChargeTick),
    ("Movement target stop event", TestMovementTargetStopEvent),
    ("Movement orbit tick", TestMovementOrbitTick),
    ("Movement sine wave tick", TestMovementSineWaveTick),
    ("Movement bezier curve tick", TestMovementBezierCurveTick),
    ("Movement boomerang tick", TestMovementBoomerangTick),
    ("Movement attach to host tick", TestMovementAttachToHostTick),
    ("Movement player input tick", TestMovementPlayerInputTick),
    ("Movement AI controlled tick", TestMovementAIControlledTick),
    ("Movement parabola tick", TestMovementParabolaTick),
    ("Movement circular arc tick", TestMovementCircularArcTick),
    ("Movement orientation params defaults", TestMovementOrientationParamsDefaults),
    ("Movement collision stop tick", TestMovementCollisionStopTick),
    ("Movement collision target query injection", TestMovementCollisionTargetQueryInjection),
    ("Movement collision notify without stop", TestMovementCollisionNotifyWithoutStop),
    ("Observation log level filter and formatting", TestObservationLogLevelFilterAndFormatting),
    ("Observation JSONL serialization", TestObservationJsonlSerialization),
    ("Scene validation session aggregates failures", TestSceneValidationSessionAggregatesFailures)
};

var failed = 0;
for (var i = 0; i < tests.Length; i++)
{
    try
    {
        tests[i].Run();
        Console.WriteLine($"PASS {tests[i].Name}");
    }
    catch (Exception ex)
    {
        failed++;
        Console.Error.WriteLine($"FAIL {tests[i].Name}: {ex.Message}");
    }
}

if (failed > 0)
{
    Environment.Exit(1);
}

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

static void TestRuntimeTypedDataApi()
{
    var health = DataKey.Create<float>(
        "Test.Typed.Health",
        defaultValue: 10f,
        category: RuntimeDataTestCategory.Runtime,
        minValue: 0f,
        maxValue: 100f,
        supportsModifiers: true);
    var name = DataKey.Create<string>(
        "Test.Typed.Name",
        defaultValue: "unset",
        category: RuntimeDataTestCategory.Config);
    var option = DataKey.Create<int>(
        "Test.Typed.Option",
        defaultValue: 0,
        options: ["A", "B"]);
    var catalog = DataCatalog.CreateBuilder("test-runtime-data")
        .Add(health)
        .Add(name)
        .Add(option)
        .Build();
    var sink = new RecordingDataChangeSink();
    var data = new Data(catalog, sink);

    AssertEqual("typed get default", 10f, data.Get(health));
    AssertEqual("typed get caller fallback", 99f, data.Get(health, 99f));
    AssertEqual("typed tryget unset", false, data.TryGet(health, out var healthValue));
    AssertEqual("typed tryget default out", 10f, healthValue);
    AssertEqual("typed has unset", false, data.Has(health));

    AssertEqual("typed set", true, data.Set(health, 25f));
    AssertEqual("typed get set", 25f, data.Get(health));
    AssertEqual("typed has set", true, data.Has(health));
    AssertEqual("typed tryget set", true, data.TryGet(health, out healthValue));
    AssertEqual("typed tryget set out", 25f, healthValue);

    data.Add(health, 5f);
    data.Multiply(health, 2f);
    AssertEqual("typed add multiply", 60f, data.GetBase(health));
    AssertEqual("typed clamp", true, data.Set(health, 200f));
    AssertEqual("typed clamped value", 100f, data.Get(health));

    AssertEqual("typed option invalid", false, data.Set(option, 2));
    AssertEqual("typed option valid", true, data.Set(option, 1));

    data.Set(name, "unit");
    var all = data.GetAll();
    AssertEqual("typed get all count", 3, all.Count);
    AssertEqual("typed get all stable key", "unit", all[name.StableKey]);
    AssertEqual("typed change sink key", name.StableKey, sink.LastChange?.StableKey);

    data.ResetByCategory(RuntimeDataTestCategory.Runtime);
    AssertEqual("typed reset category", 10f, data.Get(health));

    AssertEqual("typed remove", true, data.Remove(health));
    AssertEqual("typed has removed", false, data.Has(health));
    AssertEqual("typed get removed default", 10f, data.Get(health));
}

static void TestRuntimeTypedDataModifierConstraints()
{
    var modifiable = DataKey.Create<float>(
        "Test.Modifier.Modifiable",
        defaultValue: 10f,
        supportsModifiers: true);
    var nonmodifiable = DataKey.Create<float>(
        "Test.Modifier.Nonmodifiable",
        defaultValue: 10f);
    var text = DataKey.Create<string>(
        "Test.Modifier.Text",
        defaultValue: "base",
        supportsModifiers: true);
    var catalog = DataCatalog.CreateBuilder("test-runtime-modifier")
        .Add(modifiable)
        .Add(nonmodifiable)
        .Add(text)
        .Build();
    var data = new Data(catalog);

    data.Set(modifiable, 10f);
    AssertEqual("modifier nonmodifiable rejected", false, data.AddModifier(nonmodifiable, new DataModifier(ModifierType.Additive, 1f, id: "bad-no-support")));
    AssertEqual("modifier nonnumeric rejected", false, data.AddModifier(text, new DataModifier(ModifierType.Additive, 1f, id: "bad-text")));

    AssertEqual("modifier add", true, data.AddModifier(modifiable, new DataModifier(ModifierType.Additive, 5f, priority: 10, id: "add")));
    AssertEqual("modifier multiply", true, data.AddModifier(modifiable, new DataModifier(ModifierType.Multiplicative, 2f, priority: 20, id: "mul")));
    AssertEqual("modifier final", true, data.AddModifier(modifiable, new DataModifier(ModifierType.FinalAdditive, 1f, priority: 30, id: "final")));
    AssertEqual("modifier priority result", 31f, data.Get(modifiable));
    AssertEqual("modifier duplicate rejected", false, data.AddModifier(modifiable, new DataModifier(ModifierType.Additive, 1f, id: "add")));
    AssertEqual("modifier remove", true, data.RemoveModifier(modifiable, "mul"));
    AssertEqual("modifier remove result", 16f, data.Get(modifiable));
}

static void TestRuntimeTypedDataComputedCache()
{
    var baseKey = DataKey.Create<float>(
        "Test.Computed.Base",
        defaultValue: 2f);
    var calls = 0;
    var computedKey = DataKey.Create<float>(
        "Test.Computed.Double",
        defaultValue: 0f,
        dependencies: [baseKey],
        compute: data =>
        {
            calls++;
            return data.Get(baseKey) * 2f;
        });
    var catalog = DataCatalog.CreateBuilder("test-runtime-computed")
        .Add(baseKey)
        .Add(computedKey)
        .Build();
    var data = new Data(catalog);

    AssertEqual("computed first read", 4f, data.Get(computedKey));
    AssertEqual("computed first calls", 1, calls);
    AssertEqual("computed cached read", 4f, data.Get(computedKey));
    AssertEqual("computed cached calls", 1, calls);

    data.Set(baseKey, 3f);
    AssertEqual("computed dirty read", 6f, data.Get(computedKey));
    AssertEqual("computed dirty calls", 2, calls);

    data.Remove(baseKey);
    AssertEqual("computed remove clears cache", 4f, data.Get(computedKey));
    AssertEqual("computed remove calls", 3, calls);
    AssertEqual("computed has", true, data.Has(computedKey));
}

static void TestRuntimeDataSnapshotAppliesDataAndResources()
{
    const string json = """
    {
      "schemaVersion": 2,
      "generatedAtUtc": "2026-05-05T00:00:00Z",
      "manifest": {
        "schemaVersion": 2,
        "generatedAtUtc": "2026-05-05T00:00:00Z",
        "profile": "test",
        "catalogId": "framework",
        "enabledCapabilities": ["Damage", "Movement", "Attack", "Ability"],
        "descriptorCount": 5,
        "recordCount": 1,
        "resourceCount": 1,
        "validation": { "warningCount": 0, "errorCount": 0 }
      },
      "descriptors": [
        { "stableKey": "Damage.MaxHp", "ownerCapability": "Damage", "ownerSkill": "damage-system", "valueType": "float", "defaultValue": "0", "displayName": "Max HP", "category": "Damage.Config", "supportsModifiers": true },
        { "stableKey": "Damage.CurrentHp", "ownerCapability": "Damage", "ownerSkill": "damage-system", "valueType": "float", "defaultValue": "0", "displayName": "Current HP", "category": "Damage.Runtime" },
        { "stableKey": "Movement.MoveSpeed", "ownerCapability": "Movement", "ownerSkill": "movement-system", "valueType": "float", "defaultValue": "0", "displayName": "Move Speed", "category": "Movement.Config", "supportsModifiers": true },
        { "stableKey": "Attack.Damage", "ownerCapability": "Attack", "ownerSkill": "attack-system", "valueType": "float", "defaultValue": "0", "displayName": "Attack Damage", "category": "Attack.Basic", "supportsModifiers": true },
        { "stableKey": "Ability.TriggerMode", "ownerCapability": "Ability", "ownerSkill": "ability-system", "valueType": "string", "defaultValue": "None", "displayName": "Trigger Mode", "category": "Ability.Basic" }
      ],
      "records": [
        {
          "table": "unit.enemy",
          "id": "yuren",
          "name": "鱼人",
          "fields": {
            "Damage.MaxHp": { "type": "float", "value": 150 },
            "Damage.CurrentHp": { "type": "float", "value": 150 },
            "Movement.MoveSpeed": { "type": "float", "value": 150 },
            "Attack.Damage": { "type": "float", "value": 6 },
            "Ability.TriggerMode": { "type": "string", "value": "Manual" }
          }
        }
      ],
      "resources": [
        { "category": "Entity", "key": "EnemyEntity", "path": "res://Scenes/Enemy.tscn" }
      ]
    }
    """;

    ResourceCatalog.Clear();
    var snapshot = RuntimeDataSnapshot.FromJson(json);
    AssertEqual("snapshot schema", 2, snapshot.SchemaVersion);
    AssertEqual("snapshot descriptor count", 5, snapshot.Descriptors.Count);
    AssertEqual("snapshot find record", true, snapshot.TryFindRecord("unit.enemy", "鱼人", out var record));

    var data = new Data();
    AssertEqual("snapshot applied count", 5, snapshot.ApplyRecord(data, record));
    AssertEqual("snapshot max hp", 150f, data.Get<float>(DamageDataKeys.MaxHp));
    AssertEqual("snapshot move speed", 150f, data.Get<float>(MovementDataKeys.MoveSpeed));
    AssertEqual("snapshot attack damage", 6f, data.Get<float>(AttackDataKeys.Damage));
    AssertEqual("snapshot enum string", AbilityTriggerMode.Manual, data.Get<AbilityTriggerMode>(AbilityDataKeys.TriggerMode));

    AssertEqual("snapshot resources", 1, snapshot.RegisterResources());
    AssertEqual("snapshot resource path", "res://Scenes/Enemy.tscn", ResourceManagement.GetPath("EnemyEntity", ResourceCategory.Entity));
    ResourceCatalog.Clear();
}

static void TestRuntimeDataSnapshotRejectsInvalidFields()
{
    var wrongType = RuntimeDataSnapshot.FromJson(BuildSingleFieldSnapshot(
        "Damage.MaxHp",
        "float",
        "\"not-a-number\"",
        "Damage.MaxHp",
        "float",
        "0"));
    AssertEqual(
        "snapshot wrong type code",
        "snapshot.wrong_type",
        wrongType.ApplyRecordWithReport(new Data(), wrongType.Records[0]).Errors[0].Code);

    var unknownKey = RuntimeDataSnapshot.FromJson(BuildSingleFieldSnapshot(
        "Unknown.Key",
        "float",
        "1",
        "Unknown.Key",
        "float",
        "0"));
    AssertEqual(
        "snapshot unknown key code",
        "snapshot.unknown_key",
        unknownKey.ApplyRecordWithReport(new Data(), unknownKey.Records[0]).Errors[0].Code);

    var defaultDrift = RuntimeDataSnapshot.FromJson(BuildSingleFieldSnapshot(
        "Damage.MaxHp",
        "float",
        "1",
        "Damage.MaxHp",
        "float",
        "1"));
    AssertEqual(
        "snapshot default drift code",
        "snapshot.default_drift",
        defaultDrift.ApplyRecordWithReport(new Data(), defaultDrift.Records[0]).Errors[0].Code);

    var missingDescriptor = RuntimeDataSnapshot.FromJson(BuildMissingDescriptorSnapshot());
    AssertEqual(
        "snapshot missing descriptor code",
        "snapshot.missing_descriptor",
        missingDescriptor.ApplyRecordWithReport(new Data(), missingDescriptor.Records[0]).Errors[0].Code);
}

static void TestRuntimeDataSnapshotRejectsDisabledResources()
{
    const string json = """
    {
      "schemaVersion": 2,
      "generatedAtUtc": "2026-05-05T00:00:00Z",
      "manifest": {
        "schemaVersion": 2,
        "generatedAtUtc": "2026-05-05T00:00:00Z",
        "profile": "test",
        "catalogId": "framework",
        "enabledCapabilities": ["Damage"],
        "descriptorCount": 0,
        "recordCount": 0,
        "resourceCount": 1,
        "validation": { "warningCount": 0, "errorCount": 0 }
      },
      "descriptors": [],
      "records": [],
      "resources": [
        { "category": "Entity", "key": "DisabledProjectile", "path": "res://Projectiles/Disabled.tscn", "ownerCapability": "Projectile", "legacyStatus": "active" }
      ]
    }
    """;

    var snapshot = RuntimeDataSnapshot.FromJson(json);
    AssertEqual(
        "snapshot disabled resource code",
        "snapshot.resource_disabled_capability",
        snapshot.RegisterResourcesWithReport().Errors[0].Code);
}

static string BuildSingleFieldSnapshot(
    string fieldKey,
    string fieldType,
    string fieldValueJson,
    string descriptorKey,
    string descriptorType,
    string descriptorDefault)
{
    return $$"""
    {
      "schemaVersion": 2,
      "generatedAtUtc": "2026-05-05T00:00:00Z",
      "manifest": {
        "schemaVersion": 2,
        "generatedAtUtc": "2026-05-05T00:00:00Z",
        "profile": "test",
        "catalogId": "framework",
        "enabledCapabilities": ["Damage"],
        "descriptorCount": 1,
        "recordCount": 1,
        "resourceCount": 0,
        "validation": { "warningCount": 0, "errorCount": 0 }
      },
      "descriptors": [
        { "stableKey": "{{descriptorKey}}", "ownerCapability": "Damage", "ownerSkill": "damage-system", "valueType": "{{descriptorType}}", "defaultValue": "{{descriptorDefault}}", "displayName": "{{descriptorKey}}", "category": "Damage.Config" }
      ],
      "records": [
        {
          "table": "unit.enemy",
          "id": "probe",
          "name": "Probe",
          "fields": {
            "{{fieldKey}}": { "type": "{{fieldType}}", "value": {{fieldValueJson}} }
          }
        }
      ],
      "resources": []
    }
    """;
}

static string BuildMissingDescriptorSnapshot()
{
    return """
    {
      "schemaVersion": 2,
      "generatedAtUtc": "2026-05-05T00:00:00Z",
      "manifest": {
        "schemaVersion": 2,
        "generatedAtUtc": "2026-05-05T00:00:00Z",
        "profile": "test",
        "catalogId": "framework",
        "enabledCapabilities": ["Damage"],
        "descriptorCount": 0,
        "recordCount": 1,
        "resourceCount": 0,
        "validation": { "warningCount": 0, "errorCount": 0 }
      },
      "descriptors": [],
      "records": [
        {
          "table": "unit.enemy",
          "id": "probe",
          "name": "Probe",
          "fields": {
            "Damage.MaxHp": { "type": "float", "value": 1 }
          }
        }
      ],
      "resources": []
    }
    """;
}

static void TestCollisionLayerMaskEvent()
{
    using var world = RuntimeWorld.CreateScoped();
    var source = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("collision-source") });
    source.Data.Set(CollisionDataKeys.CollisionLayer, CollisionLayers.Projectile);
    source.Data.Set(CollisionDataKeys.CollisionMask, CollisionLayers.EnemyHurtbox);
    source.Data.Set(CollisionDataKeys.Team, 1);

    var target = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("collision-target") });
    target.Data.Set(CollisionDataKeys.CollisionLayer, CollisionLayers.EnemyHurtbox);
    target.Data.Set(CollisionDataKeys.Team, 2);

    var sameTeam = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("collision-same-team") });
    sameTeam.Data.Set(CollisionDataKeys.CollisionLayer, CollisionLayers.EnemyHurtbox);
    sameTeam.Data.Set(CollisionDataKeys.Team, 1);

    var entered = 0;
    var exited = 0;
    CollisionContact? received = null;
    source.Events.Subscribe<CollisionEvents.Entered>(data =>
    {
        entered++;
        received = data.Contact;
    });
    source.Events.Subscribe<CollisionEvents.Exited>(_ => exited++);

    var collision = new CollisionSystem();
    AssertEqual("collision can collide", true, collision.CanCollide(source, target));
    AssertEqual("collision same team blocked", false, collision.CanCollide(source, sameTeam, new CollisionFilterPolicy(IgnoreSameTeam: true)));
    AssertEqual("collision entered emitted", true, collision.EmitEntered(source, target));
    AssertEqual("collision exited emitted", true, collision.EmitExited(source, target));
    AssertEqual("collision entered count", 1, entered);
    AssertEqual("collision exited count", 1, exited);
    AssertEqual("collision payload source", source.EntityId, received?.Source.EntityId);
    AssertEqual("collision payload target", target.EntityId, received?.Target.EntityId);
    AssertEqual("collision payload layer", CollisionLayers.EnemyHurtbox, received?.TargetLayer);

}

static void TestDamageServiceAppliesHealth()
{
    using var world = RuntimeWorld.CreateScoped();
    var attacker = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("damage-attacker") });
    var victim = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("damage-victim") });
    victim.Data.Set(DamageDataKeys.MaxHp, 20f);
    victim.Data.Set(DamageDataKeys.CurrentHp, 20f);

    var damagedEvents = 0;
    var healthEvents = 0;
    victim.Events.Subscribe<DamageEvents.Damaged>(data =>
    {
        damagedEvents++;
        AssertEqual("damaged attacker", attacker.EntityId, data.Info.Attacker?.EntityId);
    });
    victim.Events.Subscribe<DamageEvents.HealthChanged>(data =>
    {
        healthEvents++;
        AssertNear("health old", 20f, data.OldHp);
        AssertNear("health new", 13f, data.NewHp);
    });

    var result = DamageService.Instance.Process(new DamageInfo
    {
        Attacker = attacker,
        Victim = victim,
        Damage = 7f,
        Type = DamageType.Physical,
        Tags = DamageTags.Attack
    });

    AssertEqual("damage applied", true, result.Applied);
    AssertNear("damage final", 7f, result.Info.FinalDamage);
    AssertNear("damage hp", 13f, victim.Data.Get<float>(DamageDataKeys.CurrentHp));
    AssertNear("damage dealt stat", 7f, attacker.Data.Get<float>(DamageDataKeys.TotalDamageDealt));
    AssertNear("damage taken stat", 7f, victim.Data.Get<float>(DamageDataKeys.TotalDamageTaken));
    AssertEqual("damage hit stat", 1, attacker.Data.Get<int>(DamageDataKeys.TotalHits));
    AssertEqual("damaged events", 1, damagedEvents);
    AssertEqual("health events", 1, healthEvents);

}

static void TestDamageServiceKilledEvent()
{
    using var world = RuntimeWorld.CreateScoped();
    var attacker = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("damage-killer") });
    var victim = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("damage-killed-victim") });
    victim.Data.Set(DamageDataKeys.CurrentHp, 5f);

    var killedEvents = 0;
    victim.Events.Subscribe<DamageEvents.Killed>(data =>
    {
        killedEvents++;
        AssertEqual("killed killer", attacker.EntityId, data.Killer?.EntityId);
    });

    var result = DamageService.Instance.Process(new DamageInfo
    {
        Attacker = attacker,
        Victim = victim,
        Damage = 10f,
        Tags = DamageTags.Contact
    });

    AssertEqual("kill applied", true, result.Applied);
    AssertEqual("kill fatal", true, result.Info.IsFatal);
    AssertEqual("kill is dead", true, victim.Data.Get<bool>(DamageDataKeys.IsDead));
    AssertNear("kill hp", 0f, victim.Data.Get<float>(DamageDataKeys.CurrentHp));
    AssertEqual("kill stat", 1, attacker.Data.Get<int>(DamageDataKeys.TotalKills));
    AssertEqual("killed events", 1, killedEvents);

}

static void TestDamagePipelineDodge()
{
    using var world = RuntimeWorld.CreateScoped();
    var attacker = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("damage-dodge-attacker") });
    var victim = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("damage-dodge-victim") });
    victim.Data.Set(DamageDataKeys.CurrentHp, 20f);
    victim.Data.Set(DamageDataKeys.DodgeChance, 100f);

    var damagedEvents = 0;
    var dodgedEvents = 0;
    victim.Events.Subscribe<DamageEvents.Damaged>(_ => damagedEvents++);
    victim.Events.Subscribe<DamageEvents.Dodged>(data =>
    {
        dodgedEvents++;
        AssertEqual("dodged attacker", attacker.EntityId, data.Attacker?.EntityId);
    });

    var result = DamageService.Instance.Process(new DamageInfo
    {
        Attacker = attacker,
        Victim = victim,
        Damage = 10f,
        Type = DamageType.Physical
    });

    AssertEqual("dodge not applied", false, result.Applied);
    AssertEqual("dodge flag", true, result.Info.IsDodged);
    AssertNear("dodge final", 0f, result.Info.FinalDamage);
    AssertNear("dodge hp unchanged", 20f, victim.Data.Get<float>(DamageDataKeys.CurrentHp));
    AssertEqual("dodge damaged events", 0, damagedEvents);
    AssertEqual("dodge event", 1, dodgedEvents);

}

static void TestDamagePipelineCriticalArmorAndStats()
{
    using var world = RuntimeWorld.CreateScoped();
    var attacker = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("damage-crit-attacker") });
    attacker.Data.Set(DamageDataKeys.CritRate, 100f);
    attacker.Data.Set(DamageDataKeys.CritDamage, 200f);

    var victim = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("damage-crit-victim") });
    victim.Data.Set(DamageDataKeys.CurrentHp, 100f);
    victim.Data.Set(DamageDataKeys.Armor, 15f);

    var result = DamageService.Instance.Process(new DamageInfo
    {
        Attacker = attacker,
        Victim = victim,
        Damage = 20f,
        Type = DamageType.Physical
    });

    AssertEqual("crit armor applied", true, result.Applied);
    AssertEqual("crit flag", true, result.Info.IsCritical);
    AssertNear("armor multiplier", 0.5f, result.Info.ArmorMultiplier);
    AssertNear("crit armor final", 20f, result.Info.FinalDamage);
    AssertNear("crit armor hp", 80f, victim.Data.Get<float>(DamageDataKeys.CurrentHp));
    AssertNear("crit highest", 20f, attacker.Data.Get<float>(DamageDataKeys.HighestSingleDamage));
    AssertEqual("crit count", 1, attacker.Data.Get<int>(DamageDataKeys.TotalCriticalHits));
    AssertEqual("crit wave count", 1, attacker.Data.Get<int>(DamageDataKeys.WaveCriticalHits));
    AssertEqual("hit count", 1, attacker.Data.Get<int>(DamageDataKeys.TotalHits));
    AssertNear("wave damage dealt", 20f, attacker.Data.Get<float>(DamageDataKeys.WaveDamageDealt));
    AssertNear("wave damage taken", 20f, victim.Data.Get<float>(DamageDataKeys.WaveDamageTaken));

}

static void TestDamagePipelineTrueDamageBypassesDodgeAndArmor()
{
    using var world = RuntimeWorld.CreateScoped();
    var attacker = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("damage-true-attacker") });
    var victim = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("damage-true-victim") });
    victim.Data.Set(DamageDataKeys.CurrentHp, 50f);
    victim.Data.Set(DamageDataKeys.DodgeChance, 100f);
    victim.Data.Set(DamageDataKeys.Armor, 100f);

    var result = DamageService.Instance.Process(new DamageInfo
    {
        Attacker = attacker,
        Victim = victim,
        Damage = 12f,
        Type = DamageType.True
    });

    AssertEqual("true damage applied", true, result.Applied);
    AssertEqual("true damage not dodged", false, result.Info.IsDodged);
    AssertNear("true damage armor multiplier unchanged", 1f, result.Info.ArmorMultiplier);
    AssertNear("true damage final", 12f, result.Info.FinalDamage);
    AssertNear("true damage hp", 38f, victim.Data.Get<float>(DamageDataKeys.CurrentHp));

}

static void TestDamagePipelineLifesteal()
{
    using var world = RuntimeWorld.CreateScoped();
    var attacker = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("damage-lifesteal-attacker") });
    attacker.Data.Set(DamageDataKeys.MaxHp, 30f);
    attacker.Data.Set(DamageDataKeys.CurrentHp, 10f);
    attacker.Data.Set(DamageDataKeys.LifeSteal, 100f);

    var victim = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("damage-lifesteal-victim") });
    victim.Data.Set(DamageDataKeys.CurrentHp, 40f);

    var healedEvents = 0;
    attacker.Events.Subscribe<DamageEvents.Healed>(data =>
    {
        healedEvents++;
        AssertNear("lifesteal event amount", 10f, data.Amount);
    });

    var result = DamageService.Instance.Process(new DamageInfo
    {
        Attacker = attacker,
        Victim = victim,
        Damage = 10f,
        Type = DamageType.Physical
    });

    AssertEqual("lifesteal applied", true, result.Applied);
    AssertNear("lifesteal victim hp", 30f, victim.Data.Get<float>(DamageDataKeys.CurrentHp));
    AssertNear("lifesteal attacker hp", 20f, attacker.Data.Get<float>(DamageDataKeys.CurrentHp));
    AssertNear("lifesteal amount", 10f, result.Info.LifestealAmount);
    AssertNear("lifesteal stat", 10f, attacker.Data.Get<float>(DamageDataKeys.TotalHealingDone));
    AssertEqual("lifesteal event", 1, healedEvents);

}

static void TestDamagePipelineShield()
{
    using var world = RuntimeWorld.CreateScoped();
    var attacker = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("damage-shield-attacker") });
    var victim = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("damage-shield-victim") });
    victim.Data.Set(DamageDataKeys.CurrentHp, 30f);
    victim.Data.Set(DamageDataKeys.Shield, 8f);

    var result = DamageService.Instance.Process(new DamageInfo
    {
        Attacker = attacker,
        Victim = victim,
        Damage = 10f,
        Type = DamageType.Physical
    });

    AssertEqual("shield partial applied", true, result.Applied);
    AssertNear("shield partial final", 2f, result.Info.FinalDamage);
    AssertNear("shield absorbed", 8f, result.Info.ShieldDamageAbsorbed);
    AssertNear("shield removed", 0f, victim.Data.Get<float>(DamageDataKeys.Shield));
    AssertNear("shield hp reduced by remaining", 28f, victim.Data.Get<float>(DamageDataKeys.CurrentHp));
    AssertNear("shield stat", 8f, victim.Data.Get<float>(DamageDataKeys.TotalShieldAbsorbed));

    victim.Data.Set(DamageDataKeys.CurrentHp, 30f);
    victim.Data.Set(DamageDataKeys.Shield, 20f);
    var blocked = DamageService.Instance.Process(new DamageInfo
    {
        Attacker = attacker,
        Victim = victim,
        Damage = 5f,
        Type = DamageType.Physical
    });

    AssertEqual("shield full blocked", false, blocked.Applied);
    AssertEqual("shield full blocked flag", true, blocked.Info.IsBlocked);
    AssertNear("shield full final", 0f, blocked.Info.FinalDamage);
    AssertNear("shield full hp unchanged", 30f, victim.Data.Get<float>(DamageDataKeys.CurrentHp));
    AssertNear("shield full remaining", 15f, victim.Data.Get<float>(DamageDataKeys.Shield));

}

static void TestHealServiceAppliesAndClamps()
{
    using var world = RuntimeWorld.CreateScoped();
    var healer = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("heal-service-healer") });
    var target = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("heal-service-target") });
    target.Data.Set(DamageDataKeys.MaxHp, 20f);
    target.Data.Set(DamageDataKeys.CurrentHp, 10f);

    var healedEvents = 0;
    target.Events.Subscribe<DamageEvents.Healed>(data =>
    {
        healedEvents++;
        AssertEqual("heal event source", HealSource.Direct, data.Info.Source);
        AssertNear("heal event amount", 10f, data.Amount);
    });

    var result = HealService.Instance.Process(new HealInfo
    {
        Healer = healer,
        Target = target,
        Amount = 15f,
        Source = HealSource.Direct
    });

    AssertEqual("heal applied", true, result.Applied);
    AssertNear("heal final", 10f, result.Info.FinalAmount);
    AssertNear("heal hp clamped", 20f, target.Data.Get<float>(DamageDataKeys.CurrentHp));
    AssertNear("heal done stat", 10f, healer.Data.Get<float>(DamageDataKeys.TotalHealingDone));
    AssertNear("heal received stat", 10f, target.Data.Get<float>(DamageDataKeys.TotalHealingReceived));
    AssertEqual("heal events", 1, healedEvents);

}

static void TestDamageToolMultiTargetAndPeriodic()
{
    using var world = RuntimeWorld.CreateScoped();
    var attacker = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("damage-tool-attacker") });
    var first = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("damage-tool-first") });
    var second = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("damage-tool-second") });
    first.Data.Set(DamageDataKeys.CurrentHp, 20f);
    second.Data.Set(DamageDataKeys.CurrentHp, 20f);

    var targets = new[] { first, second };
    var once = DamageTool.Apply(targets, new DamageApplyOptions(3f)
    {
        Attacker = attacker,
        Type = DamageType.Physical,
        Tags = DamageTags.Ability
    });

    AssertEqual("tool once target count", 2, once.TargetCount);
    AssertEqual("tool once applied count", 2, once.AppliedCount);
    AssertNear("tool once first hp", 17f, first.Data.Get<float>(DamageDataKeys.CurrentHp));
    AssertNear("tool once second hp", 17f, second.Data.Get<float>(DamageDataKeys.CurrentHp));

    var timerManager = new TimerManager("damage-tool-test-timers");
    var periodic = DamageTool.ApplyPeriodic(
        targets,
        new DamageApplyOptions(2f)
        {
            Attacker = attacker,
            Type = DamageType.Magical,
            Tags = DamageTags.Persistent
        },
        new DamageRepeatOptions(0.5f)
        {
            RepeatCount = 3,
            ApplyImmediately = true,
            TimerTag = "damage-tool-periodic"
        },
        timerManager);

    AssertEqual("tool periodic initial applied", 2, periodic.AppliedCount);
    AssertEqual("tool periodic has timer", true, periodic.Timer != null);
    AssertNear("tool periodic first initial", 15f, first.Data.Get<float>(DamageDataKeys.CurrentHp));

    timerManager.Tick(0.5f);
    timerManager.Tick(0.5f);

    AssertNear("tool periodic first final", 11f, first.Data.Get<float>(DamageDataKeys.CurrentHp));
    AssertNear("tool periodic second final", 11f, second.Data.Get<float>(DamageDataKeys.CurrentHp));
    AssertNear("tool periodic dealt stat", 18f, attacker.Data.Get<float>(DamageDataKeys.TotalDamageDealt));
    timerManager.Clear();
}

static void TestAbilityServiceInstantDamageCooldownAndCharge()
{
    using var world = RuntimeWorld.CreateScoped();
    var caster = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-caster") });
    var ability = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-firebolt") });
    ability.Data.Set(AbilityDataKeys.IsEnabled, true);
    ability.Data.Set(AbilityDataKeys.TargetSelection, AbilityTargetSelection.Entity);
    ability.Data.Set(AbilityDataKeys.Damage, 5f);
    ability.Data.Set(AbilityDataKeys.Cooldown, 1f);
    ability.Data.Set(AbilityDataKeys.UsesCharges, true);
    ability.Data.Set(AbilityDataKeys.CurrentCharges, 2);

    var target = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-target") });
    target.Data.Set(DamageDataKeys.CurrentHp, 20f);

    var executedEvents = 0;
    ability.Events.Subscribe<AbilityEvents.Executed>(data =>
    {
        executedEvents++;
        AssertEqual("ability event targets", 1, data.Result.TargetsHit);
    });

    var service = new AbilityService(new TimerManager("ability-test-timers"));
    var first = service.TryTrigger(new AbilityCastContext
    {
        Caster = caster,
        Ability = ability,
        Targets = new[] { target }
    });

    AssertEqual("ability first success", AbilityTriggerResult.Success, first.Result);
    AssertNear("ability damage hp", 15f, target.Data.Get<float>(DamageDataKeys.CurrentHp));
    AssertEqual("ability charge consumed", 1, ability.Data.Get<int>(AbilityDataKeys.CurrentCharges));
    AssertNear("ability cooldown started", 1f, ability.Data.Get<float>(AbilityDataKeys.CooldownRemaining));
    AssertEqual("ability executed events", 1, executedEvents);

    var blocked = service.TryTrigger(new AbilityCastContext
    {
        Caster = caster,
        Ability = ability,
        Targets = new[] { target }
    });
    AssertEqual("ability cooldown blocked", AbilityTriggerResult.FailCooldown, blocked.Result);

    service.TickCooldowns(new[] { ability }, 1f);
    var second = service.TryTrigger(new AbilityCastContext
    {
        Caster = caster,
        Ability = ability,
        Targets = new[] { target }
    });

    AssertEqual("ability second success", AbilityTriggerResult.Success, second.Result);
    AssertNear("ability second damage hp", 10f, target.Data.Get<float>(DamageDataKeys.CurrentHp));
    AssertEqual("ability second charge consumed", 0, ability.Data.Get<int>(AbilityDataKeys.CurrentCharges));

}

static void TestAbilityServiceRequiresTarget()
{
    using var world = RuntimeWorld.CreateScoped();
    var caster = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-no-target-caster") });
    var ability = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-no-target") });
    ability.Data.Set(AbilityDataKeys.IsEnabled, true);
    ability.Data.Set(AbilityDataKeys.TargetSelection, AbilityTargetSelection.Entity);
    ability.Data.Set(AbilityDataKeys.Damage, 5f);

    var failedEvents = 0;
    ability.Events.Subscribe<AbilityEvents.Failed>(data =>
    {
        failedEvents++;
        AssertEqual("ability fail reason", AbilityTriggerResult.FailNoTarget, data.Result);
    });

    var report = AbilityService.Instance.TryTrigger(new AbilityCastContext
    {
        Caster = caster,
        Ability = ability
    });

    AssertEqual("ability no target failed", AbilityTriggerResult.FailNoTarget, report.Result);
    AssertEqual("ability failed events", 1, failedEvents);

}

static void TestAbilityServiceAcceptsPointTarget()
{
    using var world = RuntimeWorld.CreateScoped();
    var caster = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-point-caster") });
    var ability = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-point") });
    ability.Data.Set(AbilityDataKeys.IsEnabled, true);
    ability.Data.Set(AbilityDataKeys.TargetSelection, AbilityTargetSelection.Point);

    var missingPoint = AbilityService.Instance.TryTrigger(new AbilityCastContext
    {
        Caster = caster,
        Ability = ability
    });
    AssertEqual("ability point missing failed", AbilityTriggerResult.FailNoTarget, missingPoint.Result);

    var report = AbilityService.Instance.TryTrigger(new AbilityCastContext
    {
        Caster = caster,
        Ability = ability,
        TargetPosition = new Vector2Value(3f, 4f)
    });
    AssertEqual("ability point success", AbilityTriggerResult.Success, report.Result);

}

static void TestAbilityServiceAcceptsEntityOrPointTarget()
{
    using var world = RuntimeWorld.CreateScoped();
    var caster = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-either-caster") });
    var ability = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-either") });
    ability.Data.Set(AbilityDataKeys.IsEnabled, true);
    ability.Data.Set(AbilityDataKeys.TargetSelection, AbilityTargetSelection.EntityOrPoint);

    var missing = AbilityService.Instance.TryTrigger(new AbilityCastContext
    {
        Caster = caster,
        Ability = ability
    });
    AssertEqual("ability either missing failed", AbilityTriggerResult.FailNoTarget, missing.Result);

    var pointReport = AbilityService.Instance.TryTrigger(new AbilityCastContext
    {
        Caster = caster,
        Ability = ability,
        TargetPosition = new Vector2Value(5f, 0f)
    });
    AssertEqual("ability either point success", AbilityTriggerResult.Success, pointReport.Result);

    var target = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-either-target") });
    var entityReport = AbilityService.Instance.TryTrigger(new AbilityCastContext
    {
        Caster = caster,
        Ability = ability,
        Targets = new[] { target }
    });
    AssertEqual("ability either entity success", AbilityTriggerResult.Success, entityReport.Result);

}

static void TestAbilityServicePeriodicDamage()
{
    using var world = RuntimeWorld.CreateScoped();
    var timerManager = new TimerManager("ability-periodic-test-timers");
    var service = new AbilityService(timerManager);
    var caster = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-periodic-caster") });
    var ability = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-periodic") });
    ability.Data.Set(AbilityDataKeys.IsEnabled, true);
    ability.Data.Set(AbilityDataKeys.TargetSelection, AbilityTargetSelection.Entity);
    ability.Data.Set(AbilityDataKeys.Damage, 2f);
    ability.Data.Set(AbilityDataKeys.DamageInterval, 0.5f);
    ability.Data.Set(AbilityDataKeys.DamageRepeatCount, 3);
    ability.Data.Set(AbilityDataKeys.ApplyImmediateDamage, true);

    var target = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-periodic-target") });
    target.Data.Set(DamageDataKeys.CurrentHp, 20f);

    var report = service.TryTrigger(new AbilityCastContext
    {
        Caster = caster,
        Ability = ability,
        Targets = new[] { target },
        DamageType = DamageType.Magical,
        DamageTags = DamageTags.Persistent
    });

    AssertEqual("ability periodic success", AbilityTriggerResult.Success, report.Result);
    AssertEqual("ability periodic has timer", true, report.Executed?.DamageResult?.Timer != null);
    AssertNear("ability periodic immediate", 18f, target.Data.Get<float>(DamageDataKeys.CurrentHp));

    timerManager.Tick(0.5f);
    timerManager.Tick(0.5f);

    AssertNear("ability periodic final hp", 14f, target.Data.Get<float>(DamageDataKeys.CurrentHp));
    timerManager.Clear();
}

static void TestAbilityServicePeriodicAutoTrigger()
{
    using var world = RuntimeWorld.CreateScoped();
    var service = new AbilityService(new TimerManager("ability-auto-trigger-test-timers"));
    var caster = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-auto-caster") });
    var ability = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-auto") });
    ability.Data.Set(AbilityDataKeys.IsEnabled, true);
    ability.Data.Set(AbilityDataKeys.TriggerMode, AbilityTriggerMode.Periodic);
    ability.Data.Set(AbilityDataKeys.TargetSelection, AbilityTargetSelection.Entity);
    ability.Data.Set(AbilityDataKeys.Cooldown, 1f);
    ability.Data.Set(AbilityDataKeys.Damage, 3f);

    var manualAbility = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-auto-manual") });
    manualAbility.Data.Set(AbilityDataKeys.IsEnabled, true);
    manualAbility.Data.Set(AbilityDataKeys.TriggerMode, AbilityTriggerMode.Manual);
    manualAbility.Data.Set(AbilityDataKeys.TargetSelection, AbilityTargetSelection.Entity);
    manualAbility.Data.Set(AbilityDataKeys.Damage, 100f);

    var target = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-auto-target") });
    target.Data.Set(DamageDataKeys.CurrentHp, 20f);

    var context = new AbilityCastContext
    {
        Caster = caster,
        Ability = ability,
        Targets = new[] { target }
    };
    var manualContext = new AbilityCastContext
    {
        Caster = caster,
        Ability = manualAbility,
        Targets = new[] { target }
    };

    var first = service.TickAutoTriggers(new[] { context, manualContext }, 0f);
    AssertEqual("ability auto first count", 1, first.Count);
    AssertEqual("ability auto first success", AbilityTriggerResult.Success, first[0].Result);
    AssertNear("ability auto first damage", 17f, target.Data.Get<float>(DamageDataKeys.CurrentHp));
    AssertNear("ability auto cooldown started", 1f, ability.Data.Get<float>(AbilityDataKeys.CooldownRemaining));

    var blocked = service.TickAutoTriggers(new[] { context }, 0.5f);
    AssertEqual("ability auto blocked count", 0, blocked.Count);
    AssertNear("ability auto cooldown ticked", 0.5f, ability.Data.Get<float>(AbilityDataKeys.CooldownRemaining));
    AssertNear("ability auto blocked damage", 17f, target.Data.Get<float>(DamageDataKeys.CurrentHp));

    var second = service.TickAutoTriggers(new[] { context }, 0.5f);
    AssertEqual("ability auto second count", 1, second.Count);
    AssertNear("ability auto second damage", 14f, target.Data.Get<float>(DamageDataKeys.CurrentHp));

}

static void TestAbilityTargetingToolFindsNearestEntity()
{
    EntityManager.Clear();
    var caster = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-targeting-caster") });
    caster.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
    caster.Data.Set(CollisionDataKeys.Team, 1);

    var ability = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-targeting-ability") });
    ability.Data.Set(AbilityDataKeys.TargetSelection, AbilityTargetSelection.Entity);
    ability.Data.Set(AbilityDataKeys.AutoTargetRange, 5f);
    ability.Data.Set(AbilityDataKeys.AutoTargetMaxTargets, 2);

    var sameTeam = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-targeting-same-team") });
    sameTeam.Data.Set(MovementDataKeys.Position, new Vector2Value(1f, 0f));
    sameTeam.Data.Set(CollisionDataKeys.Team, 1);
    sameTeam.Data.Set(DamageDataKeys.CurrentHp, 20f);

    var deadEnemy = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-targeting-dead-enemy") });
    deadEnemy.Data.Set(MovementDataKeys.Position, new Vector2Value(2f, 0f));
    deadEnemy.Data.Set(CollisionDataKeys.Team, 2);
    deadEnemy.Data.Set(DamageDataKeys.CurrentHp, 20f);
    deadEnemy.Data.Set(DamageDataKeys.IsDead, true);

    var nearEnemy = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-targeting-near-enemy") });
    nearEnemy.Data.Set(MovementDataKeys.Position, new Vector2Value(3f, 0f));
    nearEnemy.Data.Set(CollisionDataKeys.Team, 2);
    nearEnemy.Data.Set(DamageDataKeys.CurrentHp, 20f);

    var farEnemy = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-targeting-far-enemy") });
    farEnemy.Data.Set(MovementDataKeys.Position, new Vector2Value(4f, 0f));
    farEnemy.Data.Set(CollisionDataKeys.Team, 2);
    farEnemy.Data.Set(DamageDataKeys.CurrentHp, 20f);

    var outOfRangeEnemy = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-targeting-out-of-range") });
    outOfRangeEnemy.Data.Set(MovementDataKeys.Position, new Vector2Value(8f, 0f));
    outOfRangeEnemy.Data.Set(CollisionDataKeys.Team, 2);
    outOfRangeEnemy.Data.Set(DamageDataKeys.CurrentHp, 20f);

    var built = AbilityTargetingTool.TryBuildContext(caster, ability, out var context);
    AssertEqual("ability targeting built", true, built);
    AssertEqual("ability targeting target count", 2, context?.Targets?.Count);
    AssertEqual("ability targeting nearest", nearEnemy, context?.Targets?[0]);
    AssertEqual("ability targeting second", farEnemy, context?.Targets?[1]);
    AssertEqual("ability targeting target position", new Vector2Value(3f, 0f), context?.TargetPosition);

    ability.Data.Set(AbilityDataKeys.AutoTargetRange, 2f);
    var failed = AbilityTargetingTool.TryBuildContext(caster, ability, out var missing);
    AssertEqual("ability targeting no target", false, failed);
    AssertEqual("ability targeting missing context", null, missing);

    EntityManager.Clear();
}

static void TestProjectileToolSpawnsRuntimeEntity()
{
    EntityManager.Clear();
    WorldEvents.World.Clear();
    var source = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("projectile-source") });
    var ability = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("projectile-ability") });
    var target = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("projectile-target") });
    target.Data.Set(MovementDataKeys.Position, new Vector2Value(10f, 0f));

    var spawnedEvents = 0;
    var spawnSub = WorldEvents.World.Subscribe<ProjectileEvents.Spawned>(data =>
    {
        spawnedEvents++;
        AssertEqual("projectile event source", source.EntityId, data.Source.EntityId);
        AssertEqual("projectile event target", target.EntityId, data.Target?.EntityId);
    });

    var result = ProjectileTool.Spawn(new ProjectileSpawnOptions
    {
        Source = source,
        Ability = ability,
        Target = target,
        EntityId = new EntityId("projectile-runtime"),
        ScenePath = "res://Projectiles/Fireball.tscn",
        SpawnPosition = Vector2Value.Zero,
        Speed = 12f,
        MaxHitCount = 3,
        MaxLifeTime = 2f,
        Damage = 4f,
        DamageType = DamageType.Magical,
        DamageTags = DamageTags.Projectile | DamageTags.Ability
    });

    AssertEqual("projectile created", true, result.Created);
    AssertEqual("projectile registered", result.Projectile.EntityId, EntityManager.Get(new EntityId("projectile-runtime"))?.EntityId);
    AssertEqual("projectile source typed", source.EntityId, result.Projectile.Data.Get<EntityId?>(ProjectileDataKeys.SourceEntity, null) ?? EntityId.Empty);
    AssertEqual("projectile target typed", target.EntityId, result.Projectile.Data.Get<EntityId?>(ProjectileDataKeys.TargetEntity, null) ?? EntityId.Empty);
    AssertEqual("spawner spawned-projectile-id present", true, source.Data.Get(ProjectileDataKeys.SpawnedProjectileIds).Contains(result.Projectile.EntityId));
    AssertEqual("projectile lifecycle parent attached", true, LifecycleTree.IsAttached(source.EntityId, result.Projectile.EntityId));
    AssertEqual("projectile scene path", "res://Projectiles/Fireball.tscn", result.Projectile.Data.Get<string>(ProjectileDataKeys.ScenePath));
    AssertEqual("projectile direction", new Vector2Value(1f, 0f), result.Projectile.Data.Get<Vector2Value>(ProjectileDataKeys.Direction));
    AssertNear("projectile speed", 12f, result.Projectile.Data.Get<float>(ProjectileDataKeys.Speed));
    AssertEqual("projectile max hit count", 3, result.Projectile.Data.Get<int>(ProjectileDataKeys.MaxHitCount));
    AssertEqual("projectile hit count", 0, result.Projectile.Data.Get<int>(ProjectileDataKeys.HitCount));
    AssertNear("projectile max lifetime", 2f, result.Projectile.Data.Get<float>(ProjectileDataKeys.MaxLifeTime));
    AssertNear("projectile damage", 4f, result.Projectile.Data.Get<float>(ProjectileDataKeys.Damage));
    AssertEqual("projectile events", 1, spawnedEvents);

    spawnSub.Dispose();
    WorldEvents.World.Clear();
    EntityManager.Clear();
}

static void TestProjectileMovementPiercesAndDestroysAfterMaxHits()
{
    EntityManager.Clear();
    WorldEvents.World.Clear();
    var source = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("projectile-pierce-source") });
    source.Data.Set(CollisionDataKeys.Team, 1);

    var firstTarget = CreateProjectileTarget("projectile-pierce-target-1", new Vector2Value(5f, 0f));
    var secondTarget = CreateProjectileTarget("projectile-pierce-target-2", new Vector2Value(10f, 0f));
    var thirdTarget = CreateProjectileTarget("projectile-pierce-target-3", new Vector2Value(15f, 0f));

    var projectile = ProjectileTool.Spawn(new ProjectileSpawnOptions
    {
        Source = source,
        EntityId = new EntityId("projectile-pierce-runtime"),
        SpawnPosition = Vector2Value.Zero,
        Direction = new Vector2Value(1f, 0f),
        Speed = 20f,
        MaxHitCount = 2,
        Damage = 3f,
        DamageTags = DamageTags.Projectile
    });
    projectile.Projectile.Data.Set(CollisionDataKeys.CollisionLayer, CollisionLayers.Projectile);
    projectile.Projectile.Data.Set(CollisionDataKeys.CollisionMask, CollisionLayers.EnemyHurtbox);
    projectile.Projectile.Data.Set(CollisionDataKeys.CollisionRadius, 1f);
    projectile.Projectile.Data.Set(CollisionDataKeys.Team, 1);

    var hitTargets = new List<EntityId>();
    var hitSub = WorldEvents.World.Subscribe<ProjectileEvents.Hit>(data =>
    {
        if (data.Projectile.EntityId == projectile.Projectile.EntityId)
        {
            hitTargets.Add(data.Target.EntityId);
        }
    });

    var movement = new MovementSystem();
    var started = ProjectileTool.StartMovement(projectile.Projectile, movement);
    AssertEqual("projectile pierce movement started", true, started);

    movement.Tick(1f);

    AssertEqual("projectile pierce hit count", 2, hitTargets.Count);
    AssertEqual("projectile pierce first target", firstTarget.EntityId, hitTargets[0]);
    AssertEqual("projectile pierce second target", secondTarget.EntityId, hitTargets[1]);
    AssertNear("projectile pierce first damaged", 17f, firstTarget.Data.Get<float>(DamageDataKeys.CurrentHp));
    AssertNear("projectile pierce second damaged", 17f, secondTarget.Data.Get<float>(DamageDataKeys.CurrentHp));
    AssertNear("projectile pierce third untouched", 20f, thirdTarget.Data.Get<float>(DamageDataKeys.CurrentHp));
    AssertEqual("projectile pierce destroyed", null, EntityManager.Get(projectile.Projectile.EntityId));
    AssertEqual("projectile pierce movement stopped", false, movement.IsMoving(projectile.Projectile));

    hitSub.Dispose();
    WorldEvents.World.Clear();
    EntityManager.Clear();
}

static void TestProjectileMovementLifetimeDestroys()
{
    EntityManager.Clear();
    var source = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("projectile-lifetime-source") });
    var projectile = ProjectileTool.Spawn(new ProjectileSpawnOptions
    {
        Source = source,
        EntityId = new EntityId("projectile-lifetime-runtime"),
        SpawnPosition = Vector2Value.Zero,
        Direction = new Vector2Value(1f, 0f),
        Speed = 5f,
        MaxLifeTime = 0.5f,
        Damage = 0f
    });

    var movement = new MovementSystem();
    var started = ProjectileTool.StartMovement(projectile.Projectile, movement);
    AssertEqual("projectile lifetime movement started", true, started);

    movement.Tick(0.5f);

    AssertEqual("projectile lifetime destroyed", null, EntityManager.Get(projectile.Projectile.EntityId));
    AssertEqual("projectile lifetime movement stopped", false, movement.IsMoving(projectile.Projectile));

    EntityManager.Clear();
}

static IEntity CreateProjectileTarget(string entityId, Vector2Value position)
{
    var target = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId(entityId) });
    target.Data.Set(MovementDataKeys.Position, position);
    target.Data.Set(CollisionDataKeys.CollisionLayer, CollisionLayers.EnemyHurtbox);
    target.Data.Set(CollisionDataKeys.CollisionRadius, 1f);
    target.Data.Set(CollisionDataKeys.Team, 2);
    target.Data.Set(DamageDataKeys.MaxHp, 20f);
    target.Data.Set(DamageDataKeys.CurrentHp, 20f);
    return target;
}

static void TestProjectileMovementHitDamagesAndDestroys()
{
    EntityManager.Clear();
    WorldEvents.World.Clear();
    var source = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("projectile-hit-source") });
    source.Data.Set(CollisionDataKeys.Team, 1);

    var target = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("projectile-hit-target") });
    target.Data.Set(MovementDataKeys.Position, new Vector2Value(10f, 0f));
    target.Data.Set(CollisionDataKeys.CollisionLayer, CollisionLayers.EnemyHurtbox);
    target.Data.Set(CollisionDataKeys.CollisionRadius, 1f);
    target.Data.Set(CollisionDataKeys.Team, 2);
    target.Data.Set(DamageDataKeys.MaxHp, 20f);
    target.Data.Set(DamageDataKeys.CurrentHp, 20f);

    var projectile = ProjectileTool.Spawn(new ProjectileSpawnOptions
    {
        Source = source,
        Target = target,
        EntityId = new EntityId("projectile-hit-runtime"),
        SpawnPosition = Vector2Value.Zero,
        Speed = 20f,
        Damage = 6f,
        DamageTags = DamageTags.Projectile | DamageTags.Ability
    });
    projectile.Projectile.Data.Set(CollisionDataKeys.CollisionLayer, CollisionLayers.Projectile);
    projectile.Projectile.Data.Set(CollisionDataKeys.CollisionMask, CollisionLayers.EnemyHurtbox);
    projectile.Projectile.Data.Set(CollisionDataKeys.CollisionRadius, 1f);
    projectile.Projectile.Data.Set(CollisionDataKeys.Team, 1);

    var hitEvents = 0;
    var hitSub = WorldEvents.World.Subscribe<ProjectileEvents.Hit>(data =>
    {
        hitEvents++;
        AssertEqual("projectile hit source", source.EntityId, data.Source.EntityId);
        AssertEqual("projectile hit target", target.EntityId, data.Target.EntityId);
        AssertEqual("projectile hit damage applied", true, data.Damage.Applied);
    });

    var movement = new MovementSystem();
    var started = ProjectileTool.StartMovement(projectile.Projectile, movement);
    AssertEqual("projectile movement started", true, started);

    movement.Tick(1f);

    AssertEqual("projectile hit event count", 1, hitEvents);
    AssertNear("projectile target damaged", 14f, target.Data.Get<float>(DamageDataKeys.CurrentHp));
    AssertEqual("projectile destroyed", null, EntityManager.Get(projectile.Projectile.EntityId));
    AssertEqual("projectile movement stopped after destroy", false, movement.IsMoving(projectile.Projectile));

    hitSub.Dispose();
    WorldEvents.World.Clear();
    EntityManager.Clear();
}

static void TestEffectToolSpawnsRuntimeEntity()
{
    EntityManager.Clear();
    WorldEvents.World.Clear();
    var source = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("effect-source") });
    var ability = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("effect-ability") });
    var target = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("effect-target") });
    target.Data.Set(MovementDataKeys.Position, new Vector2Value(2f, 3f));

    var spawnedEvents = 0;
    var spawnSub = WorldEvents.World.Subscribe<EffectEvents.Spawned>(data =>
    {
        spawnedEvents++;
        AssertEqual("effect event source", source.EntityId, data.Source.EntityId);
        AssertEqual("effect event target", target.EntityId, data.Target?.EntityId);
    });

    var result = EffectTool.Spawn(new EffectSpawnOptions
    {
        Source = source,
        Ability = ability,
        Target = target,
        EntityId = new EntityId("effect-runtime"),
        ScenePath = "res://Effects/Impact.tscn",
        Name = "Impact",
        AnimationName = "Effect",
        Position = Vector2Value.Zero,
        Duration = 0.75f
    });

    AssertEqual("effect created", true, result.Created);
    AssertEqual("effect registered", result.Effect.EntityId, EntityManager.Get(new EntityId("effect-runtime"))?.EntityId);
    AssertEqual("effect source typed", source.EntityId, result.Effect.Data.Get<EntityId?>(EffectDataKeys.SourceEntity, null) ?? EntityId.Empty);
    AssertEqual("effect target typed", target.EntityId, result.Effect.Data.Get<EntityId?>(EffectDataKeys.TargetEntity, null) ?? EntityId.Empty);
    AssertEqual("spawner spawned-effect-id present", true, source.Data.Get(EffectDataKeys.SpawnedEffectIds).Contains(result.Effect.EntityId));
    AssertEqual("effect lifecycle parent attached", true, LifecycleTree.IsAttached(source.EntityId, result.Effect.EntityId));
    AssertEqual("effect scene path", "res://Effects/Impact.tscn", result.Effect.Data.Get<string>(EffectDataKeys.ScenePath));
    AssertEqual("effect name", "Impact", result.Effect.Data.Get<string>(EffectDataKeys.Name));
    AssertEqual("effect animation name", "Effect", result.Effect.Data.Get<string>(EffectDataKeys.AnimationName));
    AssertEqual("effect target position", new Vector2Value(2f, 3f), result.Effect.Data.Get<Vector2Value>(EffectDataKeys.Position));
    AssertNear("effect duration", 0.75f, result.Effect.Data.Get<float>(EffectDataKeys.Duration));
    AssertEqual("effect events", 1, spawnedEvents);

    spawnSub.Dispose();
    WorldEvents.World.Clear();
    EntityManager.Clear();
}

static void TestFeatureServiceGrantsModifiersAndRemoves()
{
    using var world = RuntimeWorld.CreateScoped();
    var owner = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("feature-owner") });
    var feature = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("feature-damage-bonus") });
    owner.Data.Set(DamageDataKeys.ContactDamage, 5f);

    var service = new FeatureService();
    var definition = new FeatureDefinition
    {
        FeatureId = "feature.damage_bonus",
        Modifiers =
        [
            new FeatureModifierEntry(
                DamageDataKeys.ContactDamage,
                ModifierType.Additive,
                3f)
        ]
    };
    service.Grant(owner, feature, definition);

    AssertNear("feature modifier applied", 8f, owner.Data.Get<float>(DamageDataKeys.ContactDamage));

    service.Remove(owner, feature);
    AssertNear("feature modifier removed", 5f, owner.Data.Get<float>(DamageDataKeys.ContactDamage));

}

static void TestFeatureServiceLifecycleHandler()
{
    using var world = RuntimeWorld.CreateScoped();
    FeatureHandlerRegistry.Clear();
    var handler = new CountingFeatureHandler("feature.lifecycle");
    FeatureHandlerRegistry.Register(handler);

    var owner = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("feature-lifecycle-owner") });
    var feature = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("feature-lifecycle") });
    var service = new FeatureService();
    var definition = new FeatureDefinition
    {
        FeatureId = "feature.lifecycle",
        HandlerId = "feature.lifecycle"
    };

    service.Grant(owner, feature, definition);
    service.Disable(owner, feature);
    service.Enable(owner, feature);
    var context = new FeatureContext { Owner = owner, Feature = feature, Definition = definition, ActivationData = "payload" };
    service.Activate(context);
    service.End(context);
    service.Remove(owner, feature);

    AssertEqual("feature granted", 1, handler.Granted);
    AssertEqual("feature disabled", 1, handler.Disabled);
    AssertEqual("feature enabled", 1, handler.Enabled);
    AssertEqual("feature activated", 1, handler.Activated);
    AssertEqual("feature executed", 1, handler.Executed);
    AssertEqual("feature ended", 1, handler.Ended);
    AssertEqual("feature removed", 1, handler.Removed);
    AssertEqual("feature execute result", "executed:payload", context.ExecuteResult);
    AssertEqual("feature active reset", false, feature.Data.Get<bool>(FeatureDataKeys.IsActive));
    AssertEqual("feature activation count", 1, feature.Data.Get<int>(FeatureDataKeys.ActivationCount));

    FeatureHandlerRegistry.Clear();
}

static void TestAbilityServiceInvokesFeatureHandler()
{
    using var world = RuntimeWorld.CreateScoped();
    FeatureHandlerRegistry.Clear();
    var handler = new AbilityFeatureProbeHandler();
    FeatureHandlerRegistry.Register(handler);

    var caster = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-feature-caster") });
    var ability = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-feature") });
    ability.Data.Set(AbilityDataKeys.IsEnabled, true);
    ability.Data.Set(AbilityDataKeys.FeatureHandlerId, handler.FeatureId);
    ability.Data.Set(AbilityDataKeys.Damage, 99f);

    var target = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-feature-target") });
    target.Data.Set(DamageDataKeys.CurrentHp, 20f);

    var report = AbilityService.Instance.TryTrigger(new AbilityCastContext
    {
        Caster = caster,
        Ability = ability,
        Targets = new[] { target }
    });

    AssertEqual("ability feature success", AbilityTriggerResult.Success, report.Result);
    AssertEqual("ability feature executed", 1, handler.Executed);
    AssertEqual("ability feature targets", 1, report.Executed?.TargetsHit);
    AssertNear("ability feature avoids fallback damage", 20f, target.Data.Get<float>(DamageDataKeys.CurrentHp));
    AssertEqual("ability feature active reset", false, ability.Data.Get<bool>(FeatureDataKeys.IsActive));

    FeatureHandlerRegistry.Clear();
}

static void TestAIServiceMovesTowardTarget()
{
    EntityManager.Clear();
    var agent = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-agent") });
    var target = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-target") });
    agent.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
    target.Data.Set(MovementDataKeys.Position, new Vector2Value(3f, 4f));
    agent.Data.Set<EntityId?>(AIDataKeys.TargetEntity, target.EntityId);

    var context = new AIContext { Entity = agent, Delta = 0.1f };
    var action = new MoveToTargetAction(0.75f);
    var result = AIService.Instance.Tick(context, action);

    AssertEqual("ai move success", AIState.Success, result);
    AssertEqual("ai move direction", new Vector2Value(0.6f, 0.8f), agent.Data.Get<Vector2Value>(MovementDataKeys.AIMoveDirection));
    AssertNear("ai move speed multiplier", 0.75f, agent.Data.Get<float>(MovementDataKeys.AIMoveSpeedMultiplier));

    agent.Data.Set(AIDataKeys.IsEnabled, false);
    agent.Data.Set(MovementDataKeys.AIMoveDirection, new Vector2Value(1f, 0f));
    var disabled = AIService.Instance.Tick(context, action);
    AssertEqual("ai disabled fails", AIState.Failure, disabled);
    AssertEqual("ai disabled clears movement", Vector2Value.Zero, agent.Data.Get<Vector2Value>(MovementDataKeys.AIMoveDirection));

    EntityManager.Clear();
}

static void TestAIServiceFindsNearestTarget()
{
    EntityManager.Clear();
    var agent = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-query-agent") });
    agent.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
    agent.Data.Set(CollisionDataKeys.Team, 1);

    var sameTeam = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-query-same-team") });
    sameTeam.Data.Set(MovementDataKeys.Position, new Vector2Value(1f, 0f));
    sameTeam.Data.Set(CollisionDataKeys.Team, 1);

    var farEnemy = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-query-far-enemy") });
    farEnemy.Data.Set(MovementDataKeys.Position, new Vector2Value(8f, 0f));
    farEnemy.Data.Set(CollisionDataKeys.Team, 2);

    var nearEnemy = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-query-near-enemy") });
    nearEnemy.Data.Set(MovementDataKeys.Position, new Vector2Value(3f, 0f));
    nearEnemy.Data.Set(CollisionDataKeys.Team, 2);

    var deadEnemy = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-query-dead-enemy") });
    deadEnemy.Data.Set(MovementDataKeys.Position, new Vector2Value(2f, 0f));
    deadEnemy.Data.Set(CollisionDataKeys.Team, 2);
    deadEnemy.Data.Set(DamageDataKeys.IsDead, true);

    var context = new AIContext { Entity = agent, Delta = 0.1f };
    var root = new SequenceNode("Find And Move")
        .Add(new FindNearestTargetAction(5f))
        .Add(new MoveToTargetAction());

    var state = AIService.Instance.Tick(context, root);

    AssertEqual("ai query success", AIState.Success, state);
    AssertEqual("ai query target", (EntityId?)nearEnemy.EntityId, agent.Data.Get<EntityId?>(AIDataKeys.TargetEntity));
    AssertEqual("ai query target position", new Vector2Value(3f, 0f), agent.Data.Get<Vector2Value>(AIDataKeys.TargetPosition));
    AssertEqual("ai query has target position", true, agent.Data.Get<bool>(AIDataKeys.HasTargetPosition));
    AssertEqual("ai query move direction", new Vector2Value(1f, 0f), agent.Data.Get<Vector2Value>(MovementDataKeys.AIMoveDirection));

    nearEnemy.Data.Set(DamageDataKeys.IsDead, true);
    var failed = AIService.Instance.Tick(context, new FindNearestTargetAction(5f));
    AssertEqual("ai query no target", AIState.Failure, failed);
    AssertEqual("ai query cleared target", false, agent.Data.Has(AIDataKeys.TargetEntity));
    AssertEqual("ai query cleared target position flag", false, agent.Data.Get<bool>(AIDataKeys.HasTargetPosition));

    EntityManager.Clear();
}

static void TestAIServicePatrolAction()
{
    EntityManager.Clear();
    var agent = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-patrol-agent") });
    agent.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
    agent.Data.Set(AIDataKeys.PatrolCenter, Vector2Value.Zero);
    agent.Data.Set(AIDataKeys.PatrolRadius, 10f);
    agent.Data.Set(AIDataKeys.PatrolWaitTime, 0.5f);

    var context = new AIContext { Entity = agent, Delta = 0.1f };
    var action = new PatrolAction(reachDistance: 0.25f, speedMultiplier: 0.5f);
    var first = AIService.Instance.Tick(context, action);

    AssertEqual("ai patrol running", AIState.Running, first);
    AssertEqual("ai patrol target right", new Vector2Value(10f, 0f), agent.Data.Get<Vector2Value>(AIDataKeys.PatrolTargetPosition));
    AssertEqual("ai patrol has target", true, agent.Data.Get<bool>(AIDataKeys.HasPatrolTargetPosition));
    AssertEqual("ai patrol move right", new Vector2Value(1f, 0f), agent.Data.Get<Vector2Value>(MovementDataKeys.AIMoveDirection));
    AssertNear("ai patrol speed", 0.5f, agent.Data.Get<float>(MovementDataKeys.AIMoveSpeedMultiplier));

    agent.Data.Set(MovementDataKeys.Position, new Vector2Value(10f, 0f));
    var arrived = AIService.Instance.Tick(context, action);
    AssertEqual("ai patrol arrived running", AIState.Running, arrived);
    AssertEqual("ai patrol stops at target", Vector2Value.Zero, agent.Data.Get<Vector2Value>(MovementDataKeys.AIMoveDirection));
    AssertNear("ai patrol wait set", 0.5f, agent.Data.Get<float>(AIDataKeys.PatrolWaitRemaining));
    AssertEqual("ai patrol target cleared", false, agent.Data.Get<bool>(AIDataKeys.HasPatrolTargetPosition));

    context.Delta = 0.25f;
    var waiting = AIService.Instance.Tick(context, action);
    AssertEqual("ai patrol waiting", AIState.Running, waiting);
    AssertNear("ai patrol wait tick", 0.25f, agent.Data.Get<float>(AIDataKeys.PatrolWaitRemaining));
    AssertEqual("ai patrol waiting stopped", Vector2Value.Zero, agent.Data.Get<Vector2Value>(MovementDataKeys.AIMoveDirection));

    context.Delta = 0.25f;
    var next = AIService.Instance.Tick(context, action);
    AssertEqual("ai patrol next running", AIState.Running, next);
    AssertNear("ai patrol wait complete", 0f, agent.Data.Get<float>(AIDataKeys.PatrolWaitRemaining));
    AssertEqual("ai patrol target left", new Vector2Value(-10f, 0f), agent.Data.Get<Vector2Value>(AIDataKeys.PatrolTargetPosition));
    AssertEqual("ai patrol move left", new Vector2Value(-1f, 0f), agent.Data.Get<Vector2Value>(MovementDataKeys.AIMoveDirection));
    AssertNear("ai patrol speed restored", 0.5f, agent.Data.Get<float>(MovementDataKeys.AIMoveSpeedMultiplier));

    EntityManager.Clear();
}

static void TestAIBehaviorTreeBuilderMeleeAttackPriority()
{
    EntityManager.Clear();
    var agent = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-builder-agent") });
    var target = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-builder-target") });
    agent.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
    agent.Data.Set(CollisionDataKeys.Team, 1);
    agent.Data.Set(AIDataKeys.AttackRange, 5f);
    target.Data.Set(MovementDataKeys.Position, new Vector2Value(3f, 4f));
    target.Data.Set(CollisionDataKeys.Team, 2);

    var requested = 0;
    agent.Events.Subscribe<AttackEvents.Requested>(_ => requested++);

    var context = new AIContext { Entity = agent, Delta = 0.1f };
    var root = EnemyBehaviorTreeBuilder.BuildMeleeEnemyTree(targetSearchRange: 20f, defaultAttackRange: 1f);
    var state = AIService.Instance.Tick(context, root);

    AssertEqual("ai builder attack running", AIState.Running, state);
    AssertEqual("ai builder target selected", (EntityId?)target.EntityId, agent.Data.Get<EntityId?>(AIDataKeys.TargetEntity));
    AssertEqual("ai builder attack requested", 1, requested);
    AssertEqual("ai builder attack flag", true, agent.Data.Get<bool>(AIDataKeys.IsAttackRequested));
    AssertEqual("ai builder attack face", new Vector2Value(0.6f, 0.8f), agent.Data.Get<Vector2Value>(MovementDataKeys.AIMoveDirection));
    AssertNear("ai builder attack stopped", 0f, agent.Data.Get<float>(MovementDataKeys.AIMoveSpeedMultiplier));

    EntityManager.Clear();
}

static void TestAIBehaviorTreeBuilderPatrolFallback()
{
    EntityManager.Clear();
    var agent = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-builder-patrol-agent") });
    agent.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
    agent.Data.Set(AIDataKeys.PatrolCenter, Vector2Value.Zero);
    agent.Data.Set(AIDataKeys.PatrolRadius, 6f);
    agent.Data.Set(AIDataKeys.PatrolWaitTime, 0.1f);

    var context = new AIContext { Entity = agent, Delta = 0.1f };
    var root = EnemyBehaviorTreeBuilder.BuildMeleeEnemyTree(
        targetSearchRange: 4f,
        defaultAttackRange: 2f,
        patrolReachDistance: 0.25f,
        patrolSpeedMultiplier: 0.4f);
    var state = AIService.Instance.Tick(context, root);

    AssertEqual("ai builder patrol running", AIState.Running, state);
    AssertEqual("ai builder no attack flag", false, agent.Data.Get<bool>(AIDataKeys.IsAttackRequested));
    AssertEqual("ai builder patrol target", new Vector2Value(6f, 0f), agent.Data.Get<Vector2Value>(AIDataKeys.PatrolTargetPosition));
    AssertEqual("ai builder patrol direction", new Vector2Value(1f, 0f), agent.Data.Get<Vector2Value>(MovementDataKeys.AIMoveDirection));
    AssertNear("ai builder patrol speed", 0.4f, agent.Data.Get<float>(MovementDataKeys.AIMoveSpeedMultiplier));

    EntityManager.Clear();
}

static void TestAIServiceRequestsAttackInRange()
{
    EntityManager.Clear();
    var agent = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-attack-agent") });
    var target = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-attack-target") });
    agent.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
    agent.Data.Set(AIDataKeys.AttackRange, 5f);
    target.Data.Set(MovementDataKeys.Position, new Vector2Value(3f, 4f));
    agent.Data.Set<EntityId?>(AIDataKeys.TargetEntity, target.EntityId);

    var requested = 0;
    AttackEvents.Requested? payload = null;
    agent.Events.Subscribe<AttackEvents.Requested>(data =>
    {
        requested++;
        payload = data;
    });

    var context = new AIContext { Entity = agent, Delta = 0.1f };
    var attackBranch = new SequenceNode("AI Attack")
        .Add(new IsTargetInRangeCondition(AIDataKeys.AttackRange, 0f))
        .Add(new RequestAttackAction());

    var running = AIService.Instance.Tick(context, attackBranch);

    AssertEqual("ai attack running", AIState.Running, running);
    AssertEqual("ai attack event count", 1, requested);
    AssertEqual("ai attack payload attacker", agent, payload?.Attacker);
    AssertEqual("ai attack payload target", target, payload?.Target);
    AssertEqual("ai attack payload position", new Vector2Value(3f, 4f), payload?.TargetPosition);
    AssertEqual("ai attack requested flag", true, agent.Data.Get<bool>(AIDataKeys.IsAttackRequested));
    AssertEqual("ai attack face direction", new Vector2Value(0.6f, 0.8f), agent.Data.Get<Vector2Value>(MovementDataKeys.AIMoveDirection));
    AssertNear("ai attack stops movement", 0f, agent.Data.Get<float>(MovementDataKeys.AIMoveSpeedMultiplier));

    target.Data.Set(MovementDataKeys.Position, new Vector2Value(10f, 0f));
    var outOfRange = AIService.Instance.Tick(context, attackBranch);
    AssertEqual("ai attack out of range fails", AIState.Failure, outOfRange);
    AssertEqual("ai attack out of range no extra event", 1, requested);
    AssertEqual("ai attack out of range clears flag", false, agent.Data.Get<bool>(AIDataKeys.IsAttackRequested));

    EntityManager.Clear();
}

static void TestAIServiceAbilityAutoTriggerAction()
{
    EntityManager.Clear();
    var abilityTimer = new TimerManager("ai-ability-auto-trigger-test-timers");
    var abilityService = new AbilityService(abilityTimer);
    var agent = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-ability-agent") });
    var ability = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-ability") });
    ability.Data.Set(AbilityDataKeys.IsEnabled, true);
    ability.Data.Set(AbilityDataKeys.TriggerMode, AbilityTriggerMode.Periodic);
    ability.Data.Set(AbilityDataKeys.TargetSelection, AbilityTargetSelection.Entity);
    ability.Data.Set(AbilityDataKeys.Cooldown, 1f);
    ability.Data.Set(AbilityDataKeys.Damage, 4f);

    var target = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-ability-target") });
    target.Data.Set(DamageDataKeys.CurrentHp, 20f);

    var context = new AIContext
    {
        Entity = agent,
        Delta = 0f,
        AbilityService = abilityService,
        AbilityContexts =
        [
            new AbilityCastContext
            {
                Caster = agent,
                Ability = ability,
                Targets = new[] { target }
            }
        ]
    };
    var root = new SequenceNode("AI Auto Cast").Add(new TickAbilityAutoTriggersAction());

    var first = AIService.Instance.Tick(context, root);
    AssertEqual("ai ability action success", AIState.Success, first);
    AssertEqual("ai ability reports", 1, context.LastAbilityReports?.Count);
    AssertNear("ai ability damage", 16f, target.Data.Get<float>(DamageDataKeys.CurrentHp));

    context.Delta = 0.5f;
    var blocked = AIService.Instance.Tick(context, root);
    AssertEqual("ai ability action blocked", AIState.Failure, blocked);
    AssertEqual("ai ability blocked reports", 0, context.LastAbilityReports?.Count);
    AssertNear("ai ability cooldown tick", 0.5f, ability.Data.Get<float>(AbilityDataKeys.CooldownRemaining));

    abilityTimer.Clear();
    EntityManager.Clear();
}

static void TestAIServicePreparesAbilityAutoTargetContexts()
{
    EntityManager.Clear();
    var abilityTimer = new TimerManager("ai-ability-auto-target-test-timers");
    var abilityService = new AbilityService(abilityTimer);
    var agent = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-ability-auto-target-agent") });
    agent.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
    agent.Data.Set(CollisionDataKeys.Team, 1);

    var ability = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-ability-auto-target-ability") });
    ability.Data.Set(AbilityDataKeys.IsEnabled, true);
    ability.Data.Set(AbilityDataKeys.TriggerMode, AbilityTriggerMode.Periodic);
    ability.Data.Set(AbilityDataKeys.TargetSelection, AbilityTargetSelection.Entity);
    ability.Data.Set(AbilityDataKeys.AutoTargetRange, 8f);
    ability.Data.Set(AbilityDataKeys.Damage, 4f);

    var sameTeam = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-ability-auto-target-same-team") });
    sameTeam.Data.Set(MovementDataKeys.Position, new Vector2Value(1f, 0f));
    sameTeam.Data.Set(CollisionDataKeys.Team, 1);
    sameTeam.Data.Set(DamageDataKeys.CurrentHp, 20f);

    var target = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-ability-auto-target-enemy") });
    target.Data.Set(MovementDataKeys.Position, new Vector2Value(4f, 0f));
    target.Data.Set(CollisionDataKeys.Team, 2);
    target.Data.Set(DamageDataKeys.CurrentHp, 20f);

    var context = new AIContext
    {
        Entity = agent,
        Delta = 0f,
        AbilityService = abilityService,
        AutoTargetAbilities = [ability]
    };
    var root = new SequenceNode("AI Auto Target Cast")
        .Add(new PrepareAbilityAutoTargetContextsAction())
        .Add(new TickAbilityAutoTriggersAction());

    var first = AIService.Instance.Tick(context, root);
    AssertEqual("ai ability auto target success", AIState.Success, first);
    AssertEqual("ai ability auto target contexts", 1, context.AbilityContexts?.Count);
    AssertEqual("ai ability auto target selected", target, context.AbilityContexts?[0].Targets?[0]);
    AssertEqual("ai ability auto target reports", 1, context.LastAbilityReports?.Count);
    AssertNear("ai ability auto target damage", 16f, target.Data.Get<float>(DamageDataKeys.CurrentHp));
    AssertNear("ai ability auto target same team hp", 20f, sameTeam.Data.Get<float>(DamageDataKeys.CurrentHp));

    target.Data.Set(MovementDataKeys.Position, new Vector2Value(20f, 0f));
    var missing = AIService.Instance.Tick(context, root);
    AssertEqual("ai ability auto target missing fails", AIState.Failure, missing);
    AssertEqual("ai ability auto target missing contexts", 0, context.AbilityContexts?.Count);

    abilityTimer.Clear();
    EntityManager.Clear();
}

static void TestAttackServiceConsumesRequestAndDamages()
{
    using var world = RuntimeWorld.CreateScoped();
    var timer = new TimerManager("attack-service-instant-test-timers");
    var service = new AttackService(timer, DamageService.Instance);
    var attacker = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("attack-instant-attacker") });
    var target = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("attack-instant-target") });
    attacker.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
    attacker.Data.Set(AttackDataKeys.Damage, 6f);
    attacker.Data.Set(AttackDataKeys.Range, 20f);
    attacker.Data.Set(AttackDataKeys.Interval, 1f);
    target.Data.Set(MovementDataKeys.Position, new Vector2Value(5f, 0f));
    target.Data.Set(DamageDataKeys.CurrentHp, 20f);

    var started = 0;
    var finished = 0;
    var didHit = false;
    attacker.Events.Subscribe<AttackEvents.Started>(_ => started++);
    attacker.Events.Subscribe<AttackEvents.Finished>(data =>
    {
        finished++;
        didHit = data.DidHit;
    });
    service.Register(attacker);

    attacker.Events.Publish(new AttackEvents.Requested(
        attacker,
        target,
        target.Data.Get<Vector2Value>(MovementDataKeys.Position)));

    AssertEqual("attack service started", 1, started);
    AssertEqual("attack service finished", 1, finished);
    AssertEqual("attack service hit", true, didHit);
    AssertNear("attack service damage", 14f, target.Data.Get<float>(DamageDataKeys.CurrentHp));
    AssertEqual("attack service attack tag stats", 1, attacker.Data.Get<int>(DamageDataKeys.TotalHits));
    AssertEqual("attack service state idle", AttackState.Idle, attacker.Data.Get<AttackState>(AttackDataKeys.State));
    AssertNear("attack service cooldown set", 1f, attacker.Data.Get<float>(AttackDataKeys.CooldownRemaining));

    service.Clear();
    timer.Clear();
}

static void TestAttackServiceGatesRangeAndCooldown()
{
    using var world = RuntimeWorld.CreateScoped();
    var timer = new TimerManager("attack-service-gates-test-timers");
    var service = new AttackService(timer, DamageService.Instance);
    var attacker = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("attack-gate-attacker") });
    var target = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("attack-gate-target") });
    attacker.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
    attacker.Data.Set(AttackDataKeys.Damage, 6f);
    attacker.Data.Set(AttackDataKeys.Range, 4f);
    attacker.Data.Set(AttackDataKeys.Interval, 0.5f);
    target.Data.Set(MovementDataKeys.Position, new Vector2Value(5f, 0f));
    target.Data.Set(DamageDataKeys.CurrentHp, 20f);

    var cancelled = 0;
    AttackCancelReason? reason = null;
    attacker.Events.Subscribe<AttackEvents.Cancelled>(data =>
    {
        cancelled++;
        reason = data.Reason;
    });

    var request = new AttackEvents.Requested(attacker, target, target.Data.Get<Vector2Value>(MovementDataKeys.Position));
    var outOfRange = service.TryRequest(request);
    AssertEqual("attack out of range result", AttackTriggerResult.FailOutOfRange, outOfRange.Result);
    AssertEqual("attack out of range cancel event", 1, cancelled);
    AssertEqual("attack out of range reason", AttackCancelReason.TargetOutOfRange, reason);
    AssertNear("attack out of range hp unchanged", 20f, target.Data.Get<float>(DamageDataKeys.CurrentHp));

    attacker.Data.Set(AttackDataKeys.Range, 10f);
    var accepted = service.TryRequest(request);
    AssertEqual("attack accepted", AttackTriggerResult.Success, accepted.Result);
    AssertNear("attack accepted damage", 14f, target.Data.Get<float>(DamageDataKeys.CurrentHp));

    var cooldown = service.TryRequest(request);
    AssertEqual("attack cooldown blocked", AttackTriggerResult.FailCooldown, cooldown.Result);
    timer.Tick(0.5f);
    var afterCooldown = service.TryRequest(request);
    AssertEqual("attack after cooldown accepted", AttackTriggerResult.Success, afterCooldown.Result);

    timer.Clear();
}

static void TestAttackServiceWindupAndRecoveryTimers()
{
    using var world = RuntimeWorld.CreateScoped();
    var timer = new TimerManager("attack-service-windup-test-timers");
    var service = new AttackService(timer, DamageService.Instance);
    var attacker = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("attack-windup-attacker") });
    var target = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("attack-windup-target") });
    attacker.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
    attacker.Data.Set(AttackDataKeys.Damage, 5f);
    attacker.Data.Set(AttackDataKeys.Range, 10f);
    attacker.Data.Set(AttackDataKeys.Interval, 1f);
    attacker.Data.Set(AttackDataKeys.WindUpTime, 0.25f);
    attacker.Data.Set(AttackDataKeys.RecoveryTime, 0.25f);
    target.Data.Set(MovementDataKeys.Position, new Vector2Value(3f, 0f));
    target.Data.Set(DamageDataKeys.CurrentHp, 20f);

    var finished = 0;
    attacker.Events.Subscribe<AttackEvents.Finished>(_ => finished++);
    var request = new AttackEvents.Requested(attacker, target, target.Data.Get<Vector2Value>(MovementDataKeys.Position));
    var accepted = service.TryRequest(request);

    AssertEqual("attack windup accepted", AttackTriggerResult.Success, accepted.Result);
    AssertEqual("attack windup state", AttackState.WindUp, attacker.Data.Get<AttackState>(AttackDataKeys.State));
    AssertEqual("attack windup active", true, attacker.Data.Get<bool>(AttackDataKeys.IsAttacking));
    AssertNear("attack windup before damage", 20f, target.Data.Get<float>(DamageDataKeys.CurrentHp));

    timer.Tick(0.25f);
    AssertEqual("attack windup finished count", 1, finished);
    AssertNear("attack windup damage", 15f, target.Data.Get<float>(DamageDataKeys.CurrentHp));
    AssertEqual("attack recovery state", AttackState.Recovery, attacker.Data.Get<AttackState>(AttackDataKeys.State));

    timer.Tick(0.25f);
    AssertEqual("attack recovery idle", AttackState.Idle, attacker.Data.Get<AttackState>(AttackDataKeys.State));
    AssertEqual("attack recovery inactive", false, attacker.Data.Get<bool>(AttackDataKeys.IsAttacking));
    AssertEqual("attack cooldown still blocks", AttackTriggerResult.FailCooldown, service.TryRequest(request).Result);

    timer.Tick(0.5f);
    AssertEqual("attack cooldown complete", AttackTriggerResult.Success, service.TryRequest(request).Result);

    timer.Clear();
}

static void TestMovementChargeTick()
{
    EntityManager.Clear();
    var entity = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-charge") });
    entity.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);

    var movement = new MovementSystem();
    var started = movement.Start(entity, new MovementParams
    {
        Mode = MoveMode.Charge,
        Direction = new Vector2Value(1f, 0f),
        Speed = 100f,
        MaxDistance = 50f
    });

    movement.Tick(0.25f);
    AssertEqual("movement started", true, started);
    AssertEqual("movement still active", true, movement.IsMoving(entity));
    AssertNear("movement x after first tick", 25f, entity.Data.Get<Vector2Value>(MovementDataKeys.Position).X);
    AssertNear("movement facing x", 1f, entity.Data.Get<Vector2Value>(MovementDataKeys.FacingDirection).X);

    movement.Tick(0.5f);
    AssertEqual("movement stopped by distance", false, movement.IsMoving(entity));
    AssertNear("movement clamped x", 50f, entity.Data.Get<Vector2Value>(MovementDataKeys.Position).X);
    AssertEqual("movement velocity reset", Vector2Value.Zero, entity.Data.Get<Vector2Value>(MovementDataKeys.Velocity));

    EntityManager.Clear();
}

static void TestMovementTargetStopEvent()
{
    EntityManager.Clear();
    var entity = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-target") });
    entity.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);

    var stopped = 0;
    MovementStopReason? reason = null;
    entity.Events.Subscribe<MovementEvents.Stopped>(data =>
    {
        stopped++;
        reason = data.Context.Reason;
    });

    var movement = new MovementSystem();
    movement.Start(entity, new MovementParams
    {
        Mode = MoveMode.Charge,
        TargetPosition = new Vector2Value(30f, 0f),
        Speed = 100f,
        ReachDistance = 0f
    });

    movement.Tick(1f);

    AssertEqual("target stopped once", 1, stopped);
    AssertEqual("target stop reason", MovementStopReason.Completed, reason);
    AssertNear("target final x", 30f, entity.Data.Get<Vector2Value>(MovementDataKeys.Position).X);
    AssertEqual("target moving flag", false, entity.Data.Get<bool>(MovementDataKeys.IsMoving));

    EntityManager.Clear();
}

static void TestMovementOrbitTick()
{
    EntityManager.Clear();
    var entity = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-orbit") });
    entity.Data.Set(MovementDataKeys.Position, new Vector2Value(10f, 0f));

    var movement = new MovementSystem();
    movement.Start(entity, new MovementParams
    {
        Mode = MoveMode.Orbit,
        OrbitCenter = Vector2Value.Zero,
        OrbitRadius = 10f,
        OrbitInitAngle = 0f,
        OrbitAngularSpeed = 90f,
        OrbitTotalAngle = 90f,
        IsOrbitClockwise = true
    });

    movement.Tick(1f);
    var position = entity.Data.Get<Vector2Value>(MovementDataKeys.Position);

    AssertEqual("orbit stopped", false, movement.IsMoving(entity));
    AssertNear("orbit x", 0f, position.X);
    AssertNear("orbit y", 10f, position.Y);

    EntityManager.Clear();
}

static void TestMovementSineWaveTick()
{
    EntityManager.Clear();
    var entity = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-sine") });
    entity.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);

    var movement = new MovementSystem();
    movement.Start(entity, new MovementParams
    {
        Mode = MoveMode.SineWave,
        Angle = 0f,
        Speed = 10f,
        WaveAmplitude = 5f,
        WaveFrequency = 0.25f,
        WavePhase = 0f,
        MaxDuration = 1f
    });

    movement.Tick(1f);
    var position = entity.Data.Get<Vector2Value>(MovementDataKeys.Position);

    AssertEqual("sine stopped", false, movement.IsMoving(entity));
    AssertNear("sine x", 10f, position.X);
    AssertNear("sine y", 5f, position.Y);

    EntityManager.Clear();
}

static void TestMovementBezierCurveTick()
{
    EntityManager.Clear();
    var entity = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-bezier") });
    entity.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);

    var movement = new MovementSystem();
    movement.Start(entity, new MovementParams
    {
        Mode = MoveMode.BezierCurve,
        TargetPosition = new Vector2Value(10f, 0f),
        BezierPoints = [new Vector2Value(5f, -10f), new Vector2Value(10f, 0f)],
        MaxDuration = 1f
    });

    movement.Tick(0.5f);
    var midpoint = entity.Data.Get<Vector2Value>(MovementDataKeys.Position);
    AssertEqual("bezier still moving", true, movement.IsMoving(entity));
    AssertNear("bezier mid x", 5f, midpoint.X);
    AssertNear("bezier mid y", -5f, midpoint.Y);

    movement.Tick(0.5f);
    var endpoint = entity.Data.Get<Vector2Value>(MovementDataKeys.Position);
    AssertEqual("bezier stopped", false, movement.IsMoving(entity));
    AssertNear("bezier end x", 10f, endpoint.X);
    AssertNear("bezier end y", 0f, endpoint.Y);

    EntityManager.Clear();
}

static void TestMovementBoomerangTick()
{
    EntityManager.Clear();
    var entity = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-boomerang") });
    entity.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);

    var movement = new MovementSystem();
    movement.Start(entity, new MovementParams
    {
        Mode = MoveMode.Boomerang,
        TargetPosition = new Vector2Value(10f, 0f),
        Speed = 10f,
        BoomerangArcHeight = 0f,
        ReachDistance = 0.001f
    });

    movement.Tick(1f);
    AssertEqual("boomerang returning", true, movement.IsMoving(entity));

    movement.Tick(1f);
    var endpoint = entity.Data.Get<Vector2Value>(MovementDataKeys.Position);
    AssertEqual("boomerang stopped", false, movement.IsMoving(entity));
    AssertNear("boomerang return x", 0f, endpoint.X);
    AssertNear("boomerang return y", 0f, endpoint.Y);

    EntityManager.Clear();
}

static void TestMovementAttachToHostTick()
{
    EntityManager.Clear();
    var host = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-host") });
    host.Data.Set(MovementDataKeys.Position, new Vector2Value(20f, 5f));
    var entity = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-attach") });
    entity.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);

    var movement = new MovementSystem();
    movement.Start(entity, new MovementParams
    {
        Mode = MoveMode.AttachToHost,
        TargetEntityId = host.EntityId,
        AttachOffset = new Vector2Value(2f, -1f),
        MaxDuration = 1f
    });

    movement.Tick(0.25f);
    var position = entity.Data.Get<Vector2Value>(MovementDataKeys.Position);
    AssertEqual("attach still moving", true, movement.IsMoving(entity));
    AssertNear("attach x", 22f, position.X);
    AssertNear("attach y", 4f, position.Y);

    EntityManager.Clear();
}

static void TestMovementPlayerInputTick()
{
    EntityManager.Clear();
    var entity = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-player-input") });
    entity.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
    entity.Data.Set(MovementDataKeys.MoveSpeed, 12f);
    entity.Data.Set(MovementDataKeys.InputDirection, new Vector2Value(1f, 0f));

    var movement = new MovementSystem();
    movement.Start(entity, new MovementParams
    {
        Mode = MoveMode.PlayerInput,
        MaxDuration = 1f
    });

    movement.Tick(0.5f);
    AssertNear("player input x", 6f, entity.Data.Get<Vector2Value>(MovementDataKeys.Position).X);
    AssertEqual("player input last direction", new Vector2Value(1f, 0f), entity.Data.Get<Vector2Value>(MovementDataKeys.LastMoveDirection));

    EntityManager.Clear();
}

static void TestMovementAIControlledTick()
{
    EntityManager.Clear();
    var entity = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-ai") });
    entity.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
    entity.Data.Set(MovementDataKeys.MoveSpeed, 20f);
    entity.Data.Set(MovementDataKeys.AIMoveDirection, new Vector2Value(0f, 1f));
    entity.Data.Set(MovementDataKeys.AIMoveSpeedMultiplier, 0.5f);

    var movement = new MovementSystem();
    movement.Start(entity, new MovementParams
    {
        Mode = MoveMode.AIControlled,
        MaxDuration = 1f
    });

    movement.Tick(0.5f);
    AssertNear("ai input y", 5f, entity.Data.Get<Vector2Value>(MovementDataKeys.Position).Y);

    EntityManager.Clear();
}

static void TestMovementParabolaTick()
{
    EntityManager.Clear();
    var entity = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-parabola") });
    entity.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);

    var movement = new MovementSystem();
    movement.Start(entity, new MovementParams
    {
        Mode = MoveMode.Parabola,
        TargetPosition = new Vector2Value(10f, 0f),
        MaxDuration = 1f,
        ParabolaApexHeight = -5f
    });

    movement.Tick(0.5f);
    var midpoint = entity.Data.Get<Vector2Value>(MovementDataKeys.Position);
    AssertEqual("parabola still moving", true, movement.IsMoving(entity));
    AssertNear("parabola mid x", 5f, midpoint.X);
    AssertNear("parabola mid y", -5f, midpoint.Y);

    movement.Tick(0.5f);
    var endpoint = entity.Data.Get<Vector2Value>(MovementDataKeys.Position);
    AssertEqual("parabola stopped", false, movement.IsMoving(entity));
    AssertNear("parabola end x", 10f, endpoint.X);
    AssertNear("parabola end y", 0f, endpoint.Y);

    EntityManager.Clear();
}

static void TestMovementCircularArcTick()
{
    EntityManager.Clear();
    var entity = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-circular-arc") });
    entity.Data.Set(MovementDataKeys.Position, new Vector2Value(10f, 0f));

    var movement = new MovementSystem();
    movement.Start(entity, new MovementParams
    {
        Mode = MoveMode.CircularArc,
        TargetPosition = new Vector2Value(-10f, 0f),
        CircularArcRadius = 10f,
        CircularArcClockwise = true,
        MaxDuration = 1f
    });

    movement.Tick(0.5f);
    var midpoint = entity.Data.Get<Vector2Value>(MovementDataKeys.Position);
    AssertEqual("arc still moving", true, movement.IsMoving(entity));
    AssertNear("arc mid x", 0f, midpoint.X);
    AssertNear("arc mid y", 10f, midpoint.Y);

    movement.Tick(0.5f);
    var endpoint = entity.Data.Get<Vector2Value>(MovementDataKeys.Position);
    AssertEqual("arc stopped", false, movement.IsMoving(entity));
    AssertNear("arc end x", -10f, endpoint.X);
    AssertNear("arc end y", 0f, endpoint.Y);

    EntityManager.Clear();
}

static void TestMovementCollisionStopTick()
{
    EntityManager.Clear();
    var projectile = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-collision-projectile") });
    projectile.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
    projectile.Data.Set(CollisionDataKeys.CollisionLayer, CollisionLayers.Projectile);
    projectile.Data.Set(CollisionDataKeys.CollisionMask, CollisionLayers.EnemyHurtbox);
    projectile.Data.Set(CollisionDataKeys.CollisionRadius, 1f);
    projectile.Data.Set(CollisionDataKeys.Team, 1);

    var enemy = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-collision-enemy") });
    enemy.Data.Set(MovementDataKeys.Position, new Vector2Value(10f, 0f));
    enemy.Data.Set(CollisionDataKeys.CollisionLayer, CollisionLayers.EnemyHurtbox);
    enemy.Data.Set(CollisionDataKeys.CollisionRadius, 1f);
    enemy.Data.Set(CollisionDataKeys.Team, 2);

    var sameTeam = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-collision-same-team") });
    sameTeam.Data.Set(MovementDataKeys.Position, new Vector2Value(4f, 0f));
    sameTeam.Data.Set(CollisionDataKeys.CollisionLayer, CollisionLayers.EnemyHurtbox);
    sameTeam.Data.Set(CollisionDataKeys.CollisionRadius, 1f);
    sameTeam.Data.Set(CollisionDataKeys.Team, 1);

    var collisionEvents = 0;
    var stoppedEvents = 0;
    MovementCollisionContext? receivedCollision = null;
    MovementStopReason? stopReason = null;
    projectile.Events.Subscribe<MovementEvents.Collision>(data =>
    {
        collisionEvents++;
        receivedCollision = data.Context;
    });
    projectile.Events.Subscribe<MovementEvents.Stopped>(data =>
    {
        stoppedEvents++;
        stopReason = data.Context.Reason;
    });

    var movement = new MovementSystem();
    movement.Start(projectile, new MovementParams
    {
        Mode = MoveMode.Charge,
        Direction = new Vector2Value(1f, 0f),
        Speed = 20f,
        CollisionParams = new MovementCollisionParams
        {
            FilterPolicy = new CollisionFilterPolicy(IgnoreSameTeam: true),
            StopAfterCollisionCount = 1
        }
    });

    movement.Tick(1f);

    AssertEqual("collision stopped", false, movement.IsMoving(projectile));
    AssertEqual("collision event count", 1, collisionEvents);
    AssertEqual("collision stopped count", 1, stoppedEvents);
    AssertEqual("collision stop reason", MovementStopReason.Collision, stopReason);
    AssertEqual("collision target enemy", enemy.EntityId, receivedCollision?.Target.EntityId);
    AssertNear("collision contact x", 8f, projectile.Data.Get<Vector2Value>(MovementDataKeys.Position).X);

    EntityManager.Clear();
}

static void TestMovementOrientationParamsDefaults()
{
    var orientation = new OrientationParams();
    AssertEqual("orientation mode default", OrientationMode.FollowMovement, orientation.Mode);
    AssertNear("orientation angular speed default", 0f, orientation.AngularSpeed);
    AssertNear("orientation total angle default", -1f, orientation.TotalAngle);
    AssertEqual("orientation clockwise default", true, orientation.IsClockwise);

    var movementParams = new MovementParams();
    AssertEqual("movement orientation default", null, movementParams.Orientation);
}

static void TestMovementCollisionNotifyWithoutStop()
{
    EntityManager.Clear();
    var projectile = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-collision-notify") });
    projectile.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
    projectile.Data.Set(CollisionDataKeys.CollisionLayer, CollisionLayers.Projectile);
    projectile.Data.Set(CollisionDataKeys.CollisionMask, CollisionLayers.EnemyHurtbox);
    projectile.Data.Set(CollisionDataKeys.CollisionRadius, 1f);

    var target = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-collision-notify-target") });
    target.Data.Set(MovementDataKeys.Position, new Vector2Value(5f, 0f));
    target.Data.Set(CollisionDataKeys.CollisionLayer, CollisionLayers.EnemyHurtbox);
    target.Data.Set(CollisionDataKeys.CollisionRadius, 1f);

    var collisionEvents = 0;
    projectile.Events.Subscribe<MovementEvents.Collision>(_ => collisionEvents++);

    var movement = new MovementSystem();
    movement.Start(projectile, new MovementParams
    {
        Mode = MoveMode.Charge,
        Direction = new Vector2Value(1f, 0f),
        Speed = 10f,
        CollisionParams = new MovementCollisionParams
        {
            StopAfterCollisionCount = -1
        }
    });

    movement.Tick(1f);
    movement.Tick(1f);

    AssertEqual("notify still moving", true, movement.IsMoving(projectile));
    AssertEqual("notify dedup count", 1, collisionEvents);
    AssertNear("notify reaches through target", 20f, projectile.Data.Get<Vector2Value>(MovementDataKeys.Position).X);

    EntityManager.Clear();
}

static void TestMovementCollisionTargetQueryInjection()
{
    EntityManager.Clear();
    var projectile = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-collision-query-projectile") });
    projectile.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
    projectile.Data.Set(CollisionDataKeys.CollisionLayer, CollisionLayers.Projectile);
    projectile.Data.Set(CollisionDataKeys.CollisionMask, CollisionLayers.EnemyHurtbox);
    projectile.Data.Set(CollisionDataKeys.CollisionRadius, 1f);

    var ignoredNearTarget = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-collision-query-ignored") });
    ignoredNearTarget.Data.Set(MovementDataKeys.Position, new Vector2Value(4f, 0f));
    ignoredNearTarget.Data.Set(CollisionDataKeys.CollisionLayer, CollisionLayers.EnemyHurtbox);
    ignoredNearTarget.Data.Set(CollisionDataKeys.CollisionRadius, 1f);

    var queryTarget = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("movement-collision-query-target") });
    queryTarget.Data.Set(MovementDataKeys.Position, new Vector2Value(10f, 0f));
    queryTarget.Data.Set(CollisionDataKeys.CollisionLayer, CollisionLayers.EnemyHurtbox);
    queryTarget.Data.Set(CollisionDataKeys.CollisionRadius, 1f);

    var query = new FixedMovementCollisionTargetQuery(queryTarget);
    var movement = new MovementSystem(query);
    movement.Start(projectile, new MovementParams
    {
        Mode = MoveMode.Charge,
        Direction = new Vector2Value(1f, 0f),
        Speed = 20f,
        CollisionParams = new MovementCollisionParams
        {
            StopAfterCollisionCount = 1
        }
    });

    movement.Tick(1f);

    AssertEqual("query called", 1, query.Calls);
    AssertEqual("query collision stopped", false, movement.IsMoving(projectile));
    AssertNear("query selected target contact", 8f, projectile.Data.Get<Vector2Value>(MovementDataKeys.Position).X);

    EntityManager.Clear();
}

static void TestObservationLogLevelFilterAndFormatting()
{
    GameOSLog.Reset(new GameOSLogOptions { EnableStdout = false, MinimumLevel = GameOSLogLevel.Info, EnableJsonl = false });
    var memory = new GameOSMemoryLogSink();
    GameOSLog.AddSink(memory);

    var log = GameOSLog.For("ObservationTest");
    log.Debug("filtered");
    log.Info("hello world", new Dictionary<string, object?>
    {
        ["count"] = 2,
        ["name"] = "alpha beta"
    });
    log.Pass("check ok");

    AssertEqual("filtered count", 2, memory.Entries.Count);
    AssertEqual("first level", GameOSLogLevel.Info, memory.Entries[0].Level);
    AssertEqual("formatted text", "[INFO][ObservationTest] hello world count=2 name=\"alpha beta\"", memory.Entries[0].FormatText());

    GameOSLog.Reset(new GameOSLogOptions { EnableStdout = false, EnableJsonl = false });
}

static void TestObservationJsonlSerialization()
{
    var tempDir = Path.Combine(Path.GetTempPath(), $"gameos-observation-{Guid.NewGuid():N}");
    Directory.CreateDirectory(tempDir);
    try
    {
        var path = Path.Combine(tempDir, "scene-log.jsonl");
        using (var sink = new GameOSJsonlLogSink(path))
        {
            sink.Emit(new GameOSLogEntry(GameOSLogLevel.Pass, "JsonlTest", "json ok", new Dictionary<string, object?>
            {
                ["value"] = 7
            }));
        }

        var line = File.ReadAllLines(path).Single();
        AssertEqual("jsonl has pass", true, line.Contains("\"level\":\"PASS\"", StringComparison.Ordinal));
        AssertEqual("jsonl has context", true, line.Contains("\"context\":\"JsonlTest\"", StringComparison.Ordinal));
        AssertEqual("jsonl has formatted text", true, line.Contains("[PASS][JsonlTest] json ok value=7", StringComparison.Ordinal));
    }
    finally
    {
        Directory.Delete(tempDir, recursive: true);
    }
}

static void TestSceneValidationSessionAggregatesFailures()
{
    var tempDir = Path.Combine(Path.GetTempPath(), $"gameos-scene-validation-{Guid.NewGuid():N}");
    Directory.CreateDirectory(tempDir);
    GameOSLog.Reset(new GameOSLogOptions { EnableStdout = false, EnableJsonl = true });
    try
    {
        using var observation = GameOSObservationSession.FromEnvironment(
            "res://Scenes/Validation/Test.tscn",
            "validation",
            tempDir);
        using var validation = new SceneValidationSession(
            observation,
            "ValidationTest",
            "Runtime/Test",
            "validation.json",
            new[] { "DependencyA" },
            new[] { "NoteA" });

        validation.Check("passes", "Core", () => CheckResult.Pass("ok", new Dictionary<string, object?> { ["seen"] = true }));
        validation.Check("fails", "Core", () => CheckResult.Fail("not ok"));
        var artifactPath = validation.WriteArtifact();

        AssertEqual("validation success", false, validation.Success);
        AssertEqual("failure count", 1, validation.FailureReasons.Count);
        AssertEqual("check count", 2, validation.Checks.Count);
        AssertEqual("artifact written", true, File.Exists(artifactPath));
        AssertEqual("jsonl written", true, File.Exists(Path.Combine(tempDir, "logs", "scene-log.jsonl")));

        var artifact = File.ReadAllText(artifactPath!);
        AssertEqual("artifact status lower", true, artifact.Contains("\"status\": \"fail\"", StringComparison.Ordinal));
        AssertEqual("artifact includes fail reason", true, artifact.Contains("fails: not ok", StringComparison.Ordinal));
    }
    finally
    {
        GameOSLog.Reset(new GameOSLogOptions { EnableStdout = false, EnableJsonl = false });
        Directory.Delete(tempDir, recursive: true);
    }
}

static void AssertEqual<T>(string name, T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"{name}: expected {expected}, actual {actual}");
    }
}

static void AssertNear(string name, float expected, float actual, float tolerance = 0.001f)
{
    if (Math.Abs(expected - actual) > tolerance)
    {
        throw new InvalidOperationException($"{name}: expected {expected}, actual {actual}");
    }
}

sealed class PooledProbe : IPoolable
{
    public int Acquired { get; private set; }

    public void OnPoolAcquire()
    {
        Acquired++;
    }
}

enum RuntimeDataTestCategory
{
    Runtime,
    Config
}

sealed class RecordingDataChangeSink : IDataChangeSink
{
    public DataChangedEventData? LastChange { get; private set; }

    public void OnDataChanged(DataChangedEventData change)
    {
        LastChange = change;
    }
}

sealed class FixedMovementCollisionTargetQuery : IMovementCollisionTargetQuery
{
    private readonly IReadOnlyList<IEntity> candidates;

    public FixedMovementCollisionTargetQuery(params IEntity[] candidates)
    {
        this.candidates = candidates;
    }

    public int Calls { get; private set; }

    public IReadOnlyList<IEntity> GetCandidates(
        IEntity source,
        in MovementParams movementParams,
        Vector2Value from,
        Vector2Value intended)
    {
        Calls++;
        return candidates;
    }
}

sealed class CountingFeatureHandler : IFeatureHandler
{
    public CountingFeatureHandler(string featureId)
    {
        FeatureId = featureId;
    }

    public string FeatureId { get; }
    public int Granted { get; private set; }
    public int Removed { get; private set; }
    public int Enabled { get; private set; }
    public int Disabled { get; private set; }
    public int Activated { get; private set; }
    public int Executed { get; private set; }
    public int Ended { get; private set; }

    public void OnGranted(FeatureContext context) => Granted++;
    public void OnRemoved(FeatureContext context) => Removed++;
    public void OnEnabled(FeatureContext context) => Enabled++;
    public void OnDisabled(FeatureContext context) => Disabled++;
    public void OnActivated(FeatureContext context) => Activated++;

    public object? OnExecute(FeatureContext context)
    {
        Executed++;
        return $"executed:{context.ActivationData}";
    }

    public void OnEnded(FeatureContext context, FeatureEndReason reason) => Ended++;
}

sealed class AbilityFeatureProbeHandler : IFeatureHandler
{
    public string FeatureId => "ability.probe";
    public int Executed { get; private set; }

    public object? OnExecute(FeatureContext context)
    {
        Executed++;
        var cast = context.ActivationData as AbilityCastContext;
        return new AbilityExecutedResult
        {
            TargetsHit = cast?.Targets?.Count ?? 0,
            TotalDamage = 0f
        };
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
