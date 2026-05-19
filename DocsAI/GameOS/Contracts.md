# SlimeAI.GameOS Contracts

> Version: 0.1.0-alpha.0
> Status: runtime-minimum

## 当前契约

当前框架已建立构建边界、发布身份，并迁入 Runtime 最小内核：Runtime Data / Event / Runtime Entity / Relationship / RuntimeSchedule / Resource / Pool / Timer，GodotBridge Adapter 第一版和 Node 池化 / 碰撞隔离扩展第一段，以及 Movement / Collision / Damage / Ability / Feature / AI / Attack / Projectile / Effect Capability 第一批。

术语护栏：

- `Runtime Entity` 是稳定运行时对象身份容器，最小契约为 `EntityId + Runtime Data + entity-scoped Events`；它不是 archetype entity，不承载业务逻辑，也不是继承式 Actor 根类。
- `Runtime Data / DataKey` 是状态契约；不要把运行时数据称为 ECS Component，也不要新增裸 string/object 数据入口。
- `Capability` 是玩法组合和所有权单元；Gameplay 行为优先进入 Capability service、tool、handler、DataKey、Event 或 selector。
- `GodotBridge Adapter` 是 Godot Node 生命周期、输入、物理、资源和表现桥接角色；`IGodotComponent`、`Godot*Component`、`AttackComponent` 等现有符号是 legacy compatibility name，不是传统 ECS data component。
- `Runtime Process` / `Schedule Process` 是 `RuntimeSchedule` 管理的调度执行单元；`IRuntimeSystem`、`SystemConfig`、`SystemRunCondition`、`MovementSystem` 等现有符号是 schedule compatibility name，不是 ECS query system。
- `ECS / Entity / Component / System` 只作为历史迁移、外部对照、拒绝清单或 legacy searchability 词使用。新设计不得默认引入 world query、archetype storage 或 generic query system。

已存在的稳定入口：

- `SlimeAI.GameOS.Runtime.GameOSInfo.FrameworkId`
- `SlimeAI.GameOS.Runtime.GameOSInfo.Version`
- `SlimeAI.GameOS.Runtime.GameOSInfo.Stage`
- `SlimeAI.GameOS.Runtime.Event.IEvent`
- `SlimeAI.GameOS.Runtime.Event.IEntityEvent`
- `SlimeAI.GameOS.Runtime.Event.IGlobalEvent`
- `SlimeAI.GameOS.Runtime.Event.IBroadcastEvent`
- `SlimeAI.GameOS.Runtime.Event.IEventBus`
- `SlimeAI.GameOS.Runtime.Event.EntityEventBus`
- `SlimeAI.GameOS.Runtime.Event.WorldEventBus`
- `SlimeAI.GameOS.Runtime.Event.WorldEvents`
- `SlimeAI.GameOS.Runtime.Event.EventBusObservation`
- `SlimeAI.GameOS.Runtime.World.RuntimeWorld`
- `SlimeAI.GameOS.Runtime.World.IEntityRegistry`
- `SlimeAI.GameOS.Runtime.World.ILifecycleTree`
- `SlimeAI.GameOS.Runtime.World.IWorldEventBus`
- `SlimeAI.GameOS.Runtime.World.IResourceCatalog`
- `SlimeAI.GameOS.Runtime.World.IObjectPoolManager`
- `SlimeAI.GameOS.Runtime.World.IRuntimeSchedule`
- `SlimeAI.GameOS.Runtime.World.IRuntimeCommandBuffer`
- `SlimeAI.GameOS.Runtime.Schedule.SchedulePhase`
- `SlimeAI.GameOS.Runtime.CommandBuffer.DeferredCommandKind`
- `SlimeAI.GameOS.Runtime.CommandBuffer.DeferredCommandStatus`
- `SlimeAI.GameOS.Runtime.CommandBuffer.DeferredCommandFailureReason`
- `SlimeAI.GameOS.Runtime.CommandBuffer.DeferredRuntimeCommand`
- `SlimeAI.GameOS.Runtime.CommandBuffer.CommandPlaybackReport`
- `SlimeAI.GameOS.Runtime.CommandBuffer.CommandPlaybackEntry`
- `SlimeAI.GameOS.Runtime.CommandBuffer.IStructuralChangeGuard`
- `SlimeAI.GameOS.Runtime.CommandBuffer.RuntimeCommandBuffer`
- `SlimeAI.GameOS.Runtime.Data.Data`
- `SlimeAI.GameOS.Runtime.Data.DataKey<T>`
- `SlimeAI.GameOS.Runtime.Data.IDataKey`
- `SlimeAI.GameOS.Runtime.Data.DataSlot<T>`
- `SlimeAI.GameOS.Runtime.Data.DataCatalog`
- `SlimeAI.GameOS.Runtime.Data.DataKeyRegistry`
- `SlimeAI.GameOS.Runtime.Data.FrameworkDataKeys`
- `SlimeAI.GameOS.Runtime.Data.DataModifier`
- `SlimeAI.GameOS.Runtime.Data.IDataChangeSink`
- `SlimeAI.GameOS.Runtime.Data.RuntimeDataSnapshot`
- `SlimeAI.GameOS.Runtime.Event.EventDataChangeSink`
- `SlimeAI.GameOS.Runtime.Entity.IEntity`
- `SlimeAI.GameOS.Runtime.Entity.RuntimeEntity`
- `SlimeAI.GameOS.Runtime.Entity.EntityManager`
- `SlimeAI.GameOS.Runtime.Entity.LifecycleTree`
- `SlimeAI.GameOS.Runtime.Entity.LifecycleLink`
- `SlimeAI.GameOS.Runtime.Entity.ParentDestroyPolicy`
- `SlimeAI.GameOS.Runtime.Entity.EntityIdList`
- `SlimeAI.GameOS.Runtime.Entity.OwnedReferenceDescriptor`
- `SlimeAI.GameOS.Runtime.Entity.RuntimeOwnedReferenceRegistry`
- `SlimeAI.GameOS.Runtime.Entity.IOwnedReferenceCleaner`
- `SlimeAI.GameOS.Runtime.Schedule.RuntimeSchedule`
- `SlimeAI.GameOS.Runtime.Schedule.ProjectStateService`
- `SlimeAI.GameOS.Runtime.Schedule.SystemRunCondition`
- `SlimeAI.GameOS.Runtime.Schedule.ScheduleDataKeys`
- `SlimeAI.GameOS.Runtime.Resource.ResourceCatalog`
- `SlimeAI.GameOS.Runtime.Resource.ResourceManagement`
- `SlimeAI.GameOS.Runtime.Pool.ObjectPool<T>`
- `SlimeAI.GameOS.Runtime.Pool.ObjectPoolManager`
- `SlimeAI.GameOS.Runtime.Pool.IObjectPool`
- `SlimeAI.GameOS.Runtime.Timer.TimerManager`
- `SlimeAI.GameOS.Runtime.Timer.GameTimer`
- `SlimeAI.GameOS.Capabilities.Movement.Vector2Value`
- `SlimeAI.GameOS.Capabilities.Movement.MovementDataKeys`
- `SlimeAI.GameOS.Capabilities.Movement.MoveMode`
- `SlimeAI.GameOS.Capabilities.Movement.MovementParams`
- `SlimeAI.GameOS.Capabilities.Movement.IMovementStrategy`
- `SlimeAI.GameOS.Capabilities.Movement.MovementStrategyRegistry`
- `SlimeAI.GameOS.Capabilities.Movement.MovementSystem`
- `SlimeAI.GameOS.Capabilities.Movement.MovementStopReason`
- `SlimeAI.GameOS.Capabilities.Movement.MovementStopContext`
- `SlimeAI.GameOS.GodotBridge.GodotEntity`
- `SlimeAI.GameOS.GodotBridge.GodotEntity2D`
- `SlimeAI.GameOS.GodotBridge.IGodotComponent`
- `SlimeAI.GameOS.GodotBridge.GameOSGodotBridge`
- `SlimeAI.GameOS.GodotBridge.GodotNodeRegistry`
- `SlimeAI.GameOS.GodotBridge.GameOSTimerDriver`
- `SlimeAI.GameOS.GodotBridge.GodotMovementDriver`
- `SlimeAI.GameOS.GodotBridge.GodotNodePool<T>`
- `SlimeAI.GameOS.GodotBridge.GodotNodePoolConfig`
- `SlimeAI.GameOS.GodotBridge.GodotNodePoolManager`
- `SlimeAI.GameOS.GodotBridge.GodotCollisionIsolation`
- `SlimeAI.GameOS.GodotBridge.GodotAttackComponent`
- `SlimeAI.GameOS.GodotBridge.AttackComponent`
- `SlimeAI.GameOS.GodotBridge.GodotUnitAnimationComponent`
- `SlimeAI.GameOS.GodotBridge.GodotAIComponent`
- `SlimeAI.GameOS.GodotBridge.GodotProjectileEffectSpawner`
- `SlimeAI.GameOS.Capabilities.Damage.DamageService`
- `SlimeAI.GameOS.Capabilities.Damage.IDamageProcessor`
- `SlimeAI.GameOS.Capabilities.Damage.DamageDataKeys`
- `SlimeAI.GameOS.Capabilities.Damage.DamageInfo`
- `SlimeAI.GameOS.Capabilities.Damage.DamageResult`
- `SlimeAI.GameOS.Capabilities.Damage.HealService`
- `SlimeAI.GameOS.Capabilities.Damage.DamageTool`
- `SlimeAI.GameOS.Capabilities.Ability.AbilityService`
- `SlimeAI.GameOS.Capabilities.Ability.AbilityDataKeys`
- `SlimeAI.GameOS.Capabilities.Feature.FeatureService`
- `SlimeAI.GameOS.Capabilities.Feature.FeatureDataKeys`
- `SlimeAI.GameOS.Capabilities.Feature.FeatureDefinition`
- `SlimeAI.GameOS.Capabilities.Feature.IFeatureHandler`
- `SlimeAI.GameOS.Capabilities.Feature.FeatureHandlerRegistry`
- `SlimeAI.GameOS.Capabilities.AI.AIService`
- `SlimeAI.GameOS.Capabilities.AI.AIDataKeys`
- `SlimeAI.GameOS.Capabilities.AI.BehaviorNode`
- `SlimeAI.GameOS.Capabilities.AI.IsTargetInRangeCondition`
- `SlimeAI.GameOS.Capabilities.AI.RequestAttackAction`
- `SlimeAI.GameOS.Capabilities.AI.PatrolAction`
- `SlimeAI.GameOS.Capabilities.AI.EnemyBehaviorBlocks`
- `SlimeAI.GameOS.Capabilities.AI.EnemyBehaviorTreeBuilder`
- `SlimeAI.GameOS.Capabilities.Attack.AttackDataKeys`
- `SlimeAI.GameOS.Capabilities.Attack.AttackService`
- `SlimeAI.GameOS.Capabilities.Attack.AttackState`
- `SlimeAI.GameOS.Capabilities.Attack.AttackCancelReason`
- `SlimeAI.GameOS.Capabilities.Projectile.ProjectileDataKeys`
- `SlimeAI.GameOS.Capabilities.Projectile.ProjectileTool`
- `SlimeAI.GameOS.Capabilities.Projectile.ProjectileMovementOptions`
- `SlimeAI.GameOS.Capabilities.Effect.EffectDataKeys`
- `SlimeAI.GameOS.Capabilities.Effect.EffectTool`
- `SlimeAI.GameOS.Capabilities.Unit.UnitDataKeys`
- `SlimeAI.GameOS.Observation.GameOSLog`
- `SlimeAI.GameOS.Observation.GameOSContextLog`
- `SlimeAI.GameOS.Observation.GameOSLogEntry`
- `SlimeAI.GameOS.Observation.IGameOSLogSink`
- `SlimeAI.GameOS.Observation.GameOSObservationSession`
- `SlimeAI.GameOS.Observation.SceneValidationSession`

## Runtime World 契约

- `RuntimeWorld` 是 GameOS Runtime 的世界容器 facade，固定持有 `Entities / Lifecycle / Events / Resources / Pools / Schedule / Commands` 七个 subsystem 句柄。禁止在 `RuntimeWorld` 上引入 `IServiceProvider`、`Services.Get<T>()` 或反射 service locator；延迟命令和 phase playback 必须通过 `RuntimeWorld.Commands / Schedule`，不能另建并行全局容器。
- `RuntimeWorld.Default` 是进程级 eager singleton；现有 `EntityManager`、`LifecycleTree`、`WorldEvents.World`、`ResourceCatalog`、`ObjectPoolManager` static facade 保留并转发到 `Default`，BrotatoLike 和 Capability 旧调用面不强制改造。
- `RuntimeWorld.CreateScoped()` 每次返回独立 sandbox，实体注册表、LifecycleTree、WorldEventBus、ResourceCatalog、ObjectPoolManager state 与 `Default` 和其他 scoped world 完全隔离。框架测试必须优先使用 `using var world = RuntimeWorld.CreateScoped();`。
- `RuntimeWorld.Default.Dispose()` 必须抛 `InvalidOperationException("RuntimeWorld.Default cannot be disposed")`；scoped world `Dispose()` 后 `Entities / Lifecycle / Events / Resources / Pools / Schedule / Commands` getter 必须抛 `ObjectDisposedException`。
- Scoped world dispose 顺序固定为 `Schedule -> Commands -> Pools -> Resources -> Lifecycle -> Entities -> Events`；后续 change 不得重新定义此顺序。`Commands.Clear()` 在 teardown 中 discard pending commands，报告 `Skipped / WorldDisposing`，不得 drain。
- `IEntityRegistry / ILifecycleTree / IWorldEventBus / IResourceCatalog / IObjectPoolManager / IRuntimeSchedule / IRuntimeCommandBuffer` 是 `RuntimeWorld` 组合用 internal-only abstraction：Capability、游戏仓和测试不支持注册自定义实现或 mock；需要隔离时用真实 `CreateScoped()`。

## Runtime CommandBuffer 契约

- `RuntimeCommandBuffer` 是 Runtime、Capability、GodotBridge 和游戏 adapter 唯一的 deferred structural request 队列。禁止为某个 subsystem 另建并行 deferred queue。
- `DeferredCommandKind` 第一阶段固定为 8 种：`Spawn / Destroy / Attach / Detach / QueuedEvent / ResourceRequest / GodotNodeInstantiate / GodotNodeFree`。新增 kind 必须进入独立 OpenSpec change。
- `DeferredRuntimeCommand` 是 `readonly record struct`，payload 必须使用 8 个 nullable typed payload field：`SpawnCommandPayload / DestroyCommandPayload / AttachCommandPayload / DetachCommandPayload / QueuedEventCommandPayload / ResourceRequestCommandPayload / GodotNodeInstantiatePayload / GodotNodeFreeCommandPayload`。不得使用 `object Payload`、`Dictionary<string, object>` 或 stringly-typed `PayloadKey / PayloadValue`。
- `RuntimeCommandBuffer.EnterGuard(reason)` 返回 `IStructuralChangeGuard`。当前 framework 自动进入 guard 的上下文为：world event handler dispatch（`event-dispatch:<EventName>`）、lifecycle callback publish（`lifecycle-callback`）、GodotBridge component registered/unregistered callback（`godot-bridge-callback`）。
- Guard 内调用 `EntityManager.Spawn / world.Entities.Spawn` 会返回 reserved `RuntimeEntity`，但不立即注册到 `EntityRegistry`；同一 guard 内 `world.Entities.Get(capturedId)` 返回 `null`。返回的 entity handle 可正常 `Data.Set` 和 `Events.Publish`，playback 时会注册同一个 reserved entity，保留 guard 内 data writes。
- Guard 内 `Destroy / Attach / Detach` 自动入队。单个 outer guard scope 内超过 1000 条 enqueue 会抛 `InvalidOperationException("Guard scope command limit exceeded")`。
- `RuntimeCommandBuffer.Playback(phase)` 只播放 `TargetPhase == phase` 的命令，并按 `Sequence` 排序；其它 phase 留在队列。失败或跳过会写入 `CommandPlaybackReport`。
- `QueuedEventCommandPayload` 第一阶段只支持 framework-known `IGlobalEvent` record，使用 `typeof(T).FullName` 与 `JsonSerializer.SerializeToUtf8Bytes`；playback 通过 framework event type registry 反序列化并 `Publish<T>`。不支持 game-specific dynamic event replay。
- `ResourceRequestCommandPayload(ResourceKey, ResourcePath)` playback 时默认以 `ResourceCategory.Other` 注册。
- `GodotNodeInstantiate / GodotNodeFree` 通过可注入 `IGodotNodeCommandHandler` 执行；默认未注入时报告 `BridgeTargetUnavailable`。

## Runtime Data 契约

- `Data` 是运行时 typed 状态容器，构造时绑定 frozen `DataCatalog`，主存储使用 catalog key id + typed slot。
- `DataKey<T>` 是业务读写唯一正式入口，承载 stable key、runtime default、分类、数值边界、modifier/computed 标记和 typed dependencies。
- `DataSlot<T>` 是内部存储单元，负责 base value、default fallback、modifier list、computed dirty/cache 和 typed set/get。
- `DataCatalog` 是 framework/profile/test scoped contract，负责 stable key resolve、key id lookup、enabled capability metadata 和 snapshot loader 边界。
- `Get / Set / TryGet / Has / Remove / GetBase / Add / Multiply / AddModifier / RemoveModifier / ResetByCategory / GetAll` 的正式业务入口均使用 `DataKey<T>`；stable string 只允许出现在 DB、snapshot、manifest、observation、debug dump 和 catalog resolve 边界。
- `IDataChangeSink` 是 Data 到外部通知的解耦接口。
- `EventDataChangeSink` 将 `DataChangedEventData` 包装成 `Runtime.Events.Core.DataPropertyChanged` 事件并通过目标 `IEventBus.Publish` 派发。
- `RuntimeEntity` 默认创建 `EntityEventBus`，并让 `Data` 通过 `EventDataChangeSink` 接入 `Entity.Events`；`EntitySpawnConfig.DataCatalog` 可指定 profile catalog。
- `RuntimeDataSnapshot` 只消费 DataOS 生成的 typed JSON snapshot，不在运行时热路径访问 SQLite；它先校验 manifest / descriptors / catalog，再按 resolved `DataKey<T>` typed apply records，并把 profile 允许的资源映射注册到 `ResourceCatalog`。

## Runtime Event 契约

- 事件总线只暴露 `Publish<T>(in T) / Subscribe<T>(Action<T>) → IDisposable / ExportObservation(path)` 三个入口；没有 `On / Emit / Once / Off / priority / EventContext`。
- 事件 payload 是 `readonly record struct` 并实现 `IEntityEvent / IGlobalEvent / IBroadcastEvent` 之一；scope 完全由 marker interface 决定，调用方永远只 `Publish` 一次。
- `EntityEventBus` 承载实体级事件；`IBroadcastEvent` 会在 Publish 后自动转发到注入的 `WorldEventBus`。反向违规（`IEntityEvent` 到 world bus、`IGlobalEvent` 到 entity bus）会 `log Error` 并 return，不派发。
- 同类型嵌套 Publish 被阻断：per-bus 检测到正在派发的 `T` 时记录 reentry、log Error、return；跨类型级联和不同 bus 上的同类型 Publish 仍允许。
- 订阅顺序等于注册顺序；退订唯一路径是 Dispose 由 `Subscribe` 返回的 token。handler 异常被 `EventBusObservation` 捕获，不会中断其他 handler。
- `WorldEvents.World` 是 `RuntimeWorld.Default.Events` 的静态访问点；Capability 事件按目录组织在 `SlimeAI/GameOS/Capabilities/<Cap>/Events/`，Runtime 级事件在 `SlimeAI/GameOS/Runtime/Events/{Core,Global}/`。
- Framework Runtime/Events 只持有跨游戏 profile 有意义、且 payload 不直接依赖 Godot 引擎类型的事件。带具体游戏玩法术语或 Godot `Vector2 / Rect2 / Node` 等字段的事件必须放在 `Games/<Game>/Src/Game/Event/`；完整决策树见 `Workspace/SystemAgent/Protocols/FrameworkVsGameBoundary.md#事件归属决策树`。
- P3 `refactor-runtime-events-purge-game-leakage` 已删除 Bucket A 死代码事件 `MouseSelectionCompleted / MouseSelectionMissed / MouseSelectionPreviewUpdated / WaveStarted / WaveCompleted / GameStart / GameOver / GamePause / GameResume`，未创建游戏侧替换；Bucket B `InputUseSkill / InputPreviousSkill / InputNextSkill` 已迁到 `Games/BrotatoLike/Src/Game/Event/BrotatoLikeInputEvents.cs`。
- `ExportObservation` 写 `eventbus-dump.json`，字段覆盖 `schemaVersion`、`busName`、`generatedAtUtc`、`subscriptions`、`emittedCounts`、`sameTypeReentryBlockedCounts`、`handlerExceptions`、`handlerRegistrationOrder`。

## Runtime Entity 契约

- `EntityId` 是 typed value：`readonly record struct EntityId(string Value)`，定义在 `SlimeAI.GameOS.Runtime.Entity`；不允许 implicit `string ↔ EntityId` 转换，调用方必须 `new EntityId(value)` 或 `EntityId.From(string?)` 显式构造。`EntityId.Empty` 是唯一"无引用"值，`IsEmpty` 同时把 `null` 与 `""` 视为 empty；`default(EntityId) == Empty == From(null) == From("")`。
- `IEntity` 是 `Runtime Entity` 的 legacy public symbol，只暴露 typed `EntityId`、`Data`、`Events`，不承载业务逻辑。
- `RuntimeEntity` 是运行时对象身份容器，由 Capability、Relationship、RuntimeSchedule 和 GodotBridge Adapter 操作；不要通过子类继承或在 `IEntity` 上增加行为方法来实现玩法。
- `EntityManager.Spawn / Register / Destroy / Get / GetAll / Clear / AttachLifecycleParent` 全部使用 typed `EntityId`；static facade 转发到 `RuntimeWorld.Default.Entities`，instance state 由 `EntityRegistry` 持有。`EntityManager.Spawn(EntitySpawnConfig)` 在 `EntityId.Empty` 时分配 `EntityId.From(Guid.NewGuid().ToString("N"))`。
- `EntitySpawnConfig` 仅含 `EntityId / DataCatalog / ParentEntityId / ParentDestroyPolicy` 四字段；不再支持 `AutoAddParentRelation` 与 `ParentRelationTypes`（业务引用走 typed DataKey）。
- Runtime Events `EntitySpawned / EntityDestroyed` payload 携带 `IEntity Entity`（typed `EntityId` 通过 `Entity.EntityId` 暴露）；`LifecycleChildAttached / LifecycleChildDetached` payload 字段 `ParentEntityId / ChildEntityId / DestroyPolicy` 全部 typed，没有 string `RelationType`。
- 业务 entity 引用 DataKey 必须是 `DataKey<EntityId?>` 或 `DataKey<EntityIdList>`：`Projectile.SourceEntity / AbilityEntity / TargetEntity / SpawnedProjectileIds`、`Effect.SourceEntity / AbilityEntity / TargetEntity / SpawnedEffectIds`、`Ability.OwnerEntity / OwnedAbilityIds`、`AI.TargetEntity`。`DataKey<IEntity?>` 与 `DataKey<List<string>>` 不再作为业务 entity-id 引用类型。
- Owner cleanup hook：Capability 在 `Initialize` 时通过 `RuntimeOwnedReferenceRegistry.Register(new OwnedReferenceDescriptor(ChildToOwnerKey, OwnerListKey))` 注册一次；framework 在 `EntityManager.Destroy` 销毁完 lifecycle children、发布 `EntityDestroyed` 之前回调每个 descriptor，把被销毁 entity id 从 owner `EntityIdList` 里移除。
- Godot Node Entity 和 GodotBridge Adapter 生命周期第一版已进入 GodotBridge；对象池、碰撞隔离和复杂 adapter 缓存后续继续迁。`Component` 字样只作为现有类名兼容。

## Runtime LifecycleTree 契约

- `LifecycleTree` 是运行时唯一公开 Relationship 入口，假设每个 entity 至多一个 lifecycle parent；业务多对多引用走 typed `DataKey<EntityIdList>`，不进入 `LifecycleTree`。
- `LifecycleLink(ParentEntityId, ChildEntityId, DestroyPolicy, Priority)` 是不可变 typed record；策略和优先级是字段，不是 `Dictionary<string, object>`。
- 父实体销毁时，`DestroyPolicy = DestroyRecursively` 子实体递归销毁，`Detach` 子实体继续存活并断开 lifecycle link；`EntityManager.Destroy` 在销毁路径调用 `RuntimeOwnedReferenceRegistry.NotifyDestroying`，再发布 `EntityDestroyed`。
- `LifecycleTree.Attach / Detach` static facade 转发到 `RuntimeWorld.Default.Lifecycle`；成功时通过所属 world bus 发布 `LifecycleChildAttached / LifecycleChildDetached`（`IGlobalEvent`）；`EntityId.Empty` 作为 parent 或 child 被拒绝。
- `LifecycleTree.GetChildren / GetChildEntityIds / GetParentEntityId` 返回快照，避免调用方持有内部索引；`GetParentEntityId` 在无 parent 时返回 `EntityId.Empty`。
- `LifecycleTree.Clear` 与 `EntityManager.Clear` 配套调用，仅用于运行域重置；新测试使用 `RuntimeWorld.CreateScoped()`，不直接 mutate `RuntimeWorld.Default`。

## Runtime Schedule 契约

- `RuntimeSchedule` 是纯 C# 调度器，负责 Runtime Process / Schedule Process 描述符、配置、依赖、人工启用状态、运行条件和生命周期。
- `RuntimeSchedule.RunPhase(SchedulePhase phase)` 是 CommandBuffer playback 入口，只调用 `RuntimeCommandBuffer.Playback(phase)`，不 tick capability service，不引入 system graph 或 query DSL。
- `ProjectStateService` 维护 `GameFlowState / OverlayFlags / SimulationState` 三域状态。
- `SystemRunCondition` 是 schedule compatibility symbol，把流程、覆盖层、模拟状态门禁前置到调度层。
- `IRuntimeSystem` 是 Runtime Process 生命周期协议的 legacy compatibility name；`IRuntimeCommandHandler<TRequest,TResult>` 是命令入口协议。
- `ScheduleDataKeys` 覆盖 process/system 配置、preset 和 Spawn config 的第一批 DataOS authoring 字段；当前不自动装载 RuntimeSchedule，游戏侧仍需显式消费 snapshot。

## Runtime Resource 契约

- `ResourceCatalog` 保存稳定资源键到 Godot `res://` 路径的映射；static facade 转发到 `RuntimeWorld.Default.Resources`，scoped 测试可通过 `world.Resources` 隔离资源目录。
- `ResourceManagement` 是统一加载入口，游戏和 Capability 不直接调用 `GD.Load`。
- DataOS snapshot 可批量注册资源映射；资源扫描仍不在 Runtime 热路径中执行。

## Runtime Pool 契约

- `IObjectPool` 是全局池注册表的非泛型句柄，允许 Runtime 对象池和 Godot Node 池共用 `ObjectPoolManager.ReturnToPool`。
- `ObjectPool<T>` 支持预热、复用、统计、`IPoolable` 生命周期和 `ObjectPoolManager.ReturnToPool`；static manager 转发到 `RuntimeWorld.Default.Pools`。
- `ObjectPoolConfig.MaxSize = -1` 表示无限制，遵守项目数值语义。
- Runtime `ObjectPool<T>` 保持纯 C#；Godot Node 泊车、脱树和碰撞隔离逻辑进入 GodotBridge。

## Runtime Timer 契约

- `TimerManager` 是纯 C# 计时器调度器，由外部 `Tick(delta, unscaledDelta)` 驱动。
- `GameTimer` 支持 `Delay / Loop / Repeat / Countdown`、Tag、Pause、Cancel、Complete。
- Godot 项目通过 `GameOSTimerDriver._Process` 桥接 `TimerManager.Instance.Tick`。

## Observation 契约

- `GameOSLog.For(context)` 是通用日志入口，输出格式固定为 `[LEVEL][Context] message key=value`。
- `GameOSLogLevel` 覆盖 `Trace / Debug / Info / Pass / Warn / Error / Fail`，`GameOSLogOptions.MinimumLevel` 控制全局过滤。
- `IGameOSLogSink` 是日志输出协议，当前提供默认 Godot/stdout sink、memory sink 和 JSONL sink。
- `GameOSObservationSession.FromEnvironment(scenePath, mode)` 读取 `GODOT_SCENE_TEST_RUN_DIR / SCENE_DIR / SCREENSHOT_DIR / ARTIFACT_DIR` 及 `_REL` 变量，并在 artifact 目录下准备 `logs/scene-log.jsonl`。
- `SceneValidationSession` 负责 Godot validation scene 的 check start、PASS/FAIL、失败原因聚合和 validation artifact 写入；scene script 只声明检查项和最终 stdout marker。

## Movement Capability 契约

- `MovementSystem` 是纯 C# 调度系统，当前负责 `Start / Stop / Tick`、速度积分、最大时长 / 最大距离停止和 `Capabilities.Movement.Events.Started / Stopped`。
- Movement 内核不依赖 `Godot.Vector2` 或 `Node2D`；位置、速度、朝向统一用 `Vector2Value` 写入 `MovementDataKeys`。
- `MovementDataKeys.Position / Velocity / FacingDirection / IsMoving / MoveSpeed` 是当前稳定 Runtime Data 入口；`MoveSpeed` 支持数值 Modifier。`MovementDataKeys` 也提供 SineWave / Orbit / Boomerang / Bezier / Parabola / CircularArc 的 handler authoring 参数，供 Ability / Feature handler 从 DataOS 读取后显式映射到 `MovementParams`。
- 当前已迁入 `MoveMode.Charge / Orbit / SineWave / Parabola / CircularArc`。Charge 支持方向移动、目标点移动、速度、`MaxDuration = -1` 不限制、`MaxDistance = -1` 不限制、`ReachDistance` 到点停止；Orbit 支持固定圆心、半径、初始角、角速度、总角度；SineWave 支持基础方向、速度、振幅、频率和相位；Parabola 支持固定终点、总时长和顶点高度；CircularArc 支持固定终点、半径、方向和总时长。
- `IMovementStrategy` 只输出速度和朝向意图，不处理伤害、AI、动画或 Godot 物理体。
- Godot 场景中的 2D 位移桥接第一段已迁入：`GodotEntity2D` 注册时把初始 `Node2D.Position` 写入 Movement Data，`GodotMovementDriver` 推进 `MovementSystem.Tick` 并把 `MovementDataKeys.Position` 同步回 `Node2D.Position`。
- Godot 场景中的 2D 朝向输出第一段已迁入：`GodotOrientationComponent` 消费 `MovementDataKeys.FacingDirection` 和 `MovementParams.Orientation`，写 root `RotationDegrees` 或 `AnimatedSprite2D.FlipH`。

## GodotBridge 契约

- `GodotEntity` 是可挂场景的 `Node + IEntity` 基类，进入 SceneTree 时注册到 `EntityManager` 和 `GodotNodeRegistry`，离开 SceneTree 时注销并清理 Runtime Entity。
- `GodotEntity2D` 是可挂 2D 场景的 `Node2D + IEntity` 基类，额外接入 Movement `Position` 初始同步。
- `IGodotComponent` 是 GodotBridge Adapter 生命周期协议的 legacy compatibility name；`GameOSGodotBridge.RegisterComponents` 会递归扫描子节点，识别实现 `IGodotComponent` 或类型名以 `Component` 结尾的节点。
- Entity-Adapter 绑定记录在 GodotBridge 内部 typed registry：`GodotNodeRegistry.RegisterAdapter / UnregisterAdapter / IsAdapterRegistered / GetAdaptersByEntity`；不进入框架 `LifecycleTree`，也不通过 typed DataKey 暴露给 Capability。
- `GodotNodeRegistry` 同时保存 Node 注册表和 entity→adapter ids 映射，不替代 SceneTree，也不持有业务状态。
- `GameOSTimerDriver._Process` 不分配对象，不使用 LINQ，只把 Godot delta 传给 `TimerManager.Tick`。
- `GodotMovementDriver._Process` 推进 `MovementSystem.Tick`，并同步已注册 `Node2D + IEntity` 的运行时位置；需要固定步长或测试时可关闭 `AutoTick` 并手动调用 `TickMovement(delta)`。
- `GodotNodePool<T>` 管理 Godot Node 池化，支持 `Get(false)` 延迟激活、`Activate`、`Release`、`ReleaseAll`、`Destroy` 和 `PoolStats`。
- `GodotCollisionIsolation` 负责回池时递归隔离 2D 碰撞：`CollisionObject2D` layer/mask 清零并缓存、`Area2D` 关闭 monitoring/monitorable、`CollisionShape2D / CollisionPolygon2D` 禁用、`CharacterBody2D` 清零速度。
- `GodotNodePool<T>` 对 `CollisionObject2D` 根节点默认执行泊车位移动和脱树；重新出池时先挂回 `ActiveParent` 并同步禁用碰撞，再由 `Activate` 恢复处理、可见性和碰撞。
- `GodotContactDamageComponent` 消费 `Capabilities.Collision.Events.HurtboxEntered / HurtboxExited`，只把接触转换为 `DamageService` 请求，不直接修改 HP。
- `GodotUnitAnimationComponent` 消费 `Capabilities.Unit.Events.PlayAnimationRequested / StopAnimationRequested`，驱动 `VisualRoot` 或子节点中的 `AnimatedSprite2D`，缓存 `UnitDataKeys.AvailableAnimations`，并在非循环动画结束后发布 `Capabilities.Unit.Events.AnimationFinished` 回退到 idle。
- `GodotAttackComponent` 是普通攻击 Godot bridge 第一段，注册默认 `AttackService`，把导出攻击参数写入 `AttackDataKeys`，并把 Godot 节点目标解析为 Runtime `IEntity` 后交给 `AttackService.TryRequest`。
- `GodotAttackComponent` 可选把 `Attack.Started / Cancelled` 转成 Unit 动画请求；默认优先使用 `AttackAnimation`，当该动画不在 `UnitDataKeys.AvailableAnimations` 中时，会选择第一个 `attack*` 可用动画作为旧资源兼容回退；`PreferExistingDataOnRegister=true` 时会保留注册前已有 Attack Data。
- `AttackComponent` 是旧项目类名 / 场景名兼容包装，继承 `GodotAttackComponent`，默认启用 `PreferExistingDataOnRegister`，用于旧场景迁移时保留 DataNew / 初始化流程写入的攻击参数。
- `GodotAIComponent` 是 AI Godot bridge 第一段，注册时把导出 AI 参数写入 `AIDataKeys`，按 `GodotAIBehaviorTreeKind` 构建 Runtime 行为树，并通过 `_Process` 或 `TickAI(delta)` 调用 `AIService.Tick`；它只写 AI / Movement 意图，不直接移动 Godot 节点。
- `GodotProjectileEffectSpawner` 监听 `Capabilities.Projectile.Events.Spawned / Capabilities.Effect.Events.Spawned`，从 `ScenePath` 读取 `res://` 路径并通过 `ResourceManagement.LoadPath<PackedScene>` 实例化视觉节点，按 Runtime EntityId 注册到 `GodotNodeRegistry`，并在对应 Runtime Entity 销毁时清理自己生成的视觉节点。

## Capability Service 规范

- `XxxService.Default` 是进程级默认入口；`XxxService.Instance` 是其别名，仅为向后兼容保留。新代码优先使用 `.Default`。
- **测试代码必须用 `new XxxService(...)` 独立实例**，禁止在 `CreateScoped` 沙箱中使用 `Default / Instance`；两个 scoped world 的测试共享同一 `XxxService.Default` 会导致状态污染。
- `LifestealProcessor` 通过 `DamageService` 构造时注入 `HealService`（`DamageService(HealService? healService = null)`），不再是进程级全局依赖；默认构造时自动创建 `new HealService()`。
- `AIContext.AbilityService` 无默认值，调用方必须显式传入（或保留 `null`）；`AIService.Tick` 若 `AbilityService == null` 则 log Warning 并跳过 auto-trigger，不抛异常。
- GodotBridge 使用进程级实例时，直接传入 `AbilityService.Instance`（语义：BrotatoLike 游戏内共享全局 Ability 计时器）。

## Damage Capability 契约

- `DamageService` 是当前伤害和生命值扣减的唯一入口，按 `IDamageProcessor.Priority` 执行默认处理器管线。
- 默认处理器顺序是 Base、Dodge、Critical、Shield、Armor、DamageTakenAmplification、HealthExecution、Lifesteal、Statistics。
- `DamageDataKeys.CurrentHp / MaxHp / IsDead / IsInvulnerable / Armor / Shield / DodgeChance / CritRate / CritDamage / DamageTakenMultiplier / LifeSteal / ContactDamage / ContactDamageInterval` 是当前稳定 Data 入口。
- 概率值统一使用 0-100；`CritDamage = 100` 表示 1 倍暴击伤害，`LifeSteal` 当前同时表示触发概率和恢复比例。
- `DamageType.True` 跳过闪避和护甲；非真实伤害会进入闪避、暴击、护甲和受伤倍率。
- `ShieldDamageProcessor` 优先于护甲消耗 `DamageDataKeys.Shield`，完全吸收时阻断后续 HP 扣减。
- `HealthExecutionProcessor` 写入 `CurrentHp / IsDead` 并发布 `HealthChanged / Damaged / Killed`；`DodgeProcessor` 发布 `Dodged`；`LifestealProcessor` 通过 `HealService` 恢复攻击者 HP。
- `HealService` 是当前恢复生命值的唯一入口，负责 MaxHp 夹取、`TotalHealingDone / TotalHealingReceived` 统计和 `HealthChanged / Healed` 事件。
- `DamageTool` 是多目标和周期伤害入口，内部统一调用 `DamageService` 和 `TimerManager`。
- `DamageStatisticsProcessor` 写入 `TotalDamageDealt / Taken`、`WaveDamageDealt / Taken`、命中、暴击、击杀和最高单次伤害统计。
- 接触伤害由 `GodotContactDamageComponent` 监听 Hurtbox 事件触发，攻击者伤害读取 `DamageDataKeys.ContactDamage`，同队过滤读取 `CollisionDataKeys.Team`。

## Ability Capability 契约

- `AbilityService` 是当前最小正式施法入口，接收 `AbilityCastContext`，不做统一自动索敌或点选决策。
- `AbilityDataKeys` 当前覆盖启用、执行中、冷却、剩余冷却、充能、触发模式、目标输入语义、自动索敌参数、伤害参数、描述 / 图标 / 分组 / 消耗 / 施法距离 / 效果半径 / 链式参数和可选 `FeatureHandlerId`。
- `TryTrigger` 顺序是：施法者死亡检查、启用检查、执行中检查、冷却检查、充能检查、目标检查、消耗充能、启动冷却、执行伤害或 Feature handler、发布事件。
- `TickAutoTriggers` 只消费外部系统准备好的 `AbilityCastContext`，当前支持 `AbilityTriggerMode.Periodic`，先推进冷却，再对冷却归零的周期技能调用 `TryTrigger`。
- `AbilityCastContext.TargetPosition` 承载输入层完成点选后的目标点；`AbilityTargetSelection.Entity` 要求 `Targets`，`Point` 要求 `TargetPosition`，`EntityOrPoint` 接受二者任一。
- `AbilityTargetingTool` 是当前显式自动索敌辅助入口，会读取 `AbilityDataKeys.AutoTargetRange / AutoTargetMaxTargets / AutoTargetIgnoreSameTeam / AutoTargetRequiresDamageable`，从 Runtime Entity 快照中按距离准备 `AbilityCastContext`；`AutoTargetRange = -1` 表示不限距离，`AutoTargetMaxTargets = -1` 表示不限数量。
- 当前不做隐式统一自动索敌和异步点选会话；输入层、AI 行为节点或具体 handler 仍负责显式准备目标。
- 技能伤害统一走 `DamageTool`，周期伤害使用 `AbilityDataKeys.DamageInterval / DamageRepeatCount / ApplyImmediateDamage`。
- 当 `AbilityDataKeys.FeatureHandlerId` 非空时，`AbilityService` 会把 `AbilityCastContext` 放入 `FeatureContext.ActivationData`，调用 `FeatureService.Activate / End`；若 handler 返回 `AbilityExecutedResult` 则作为施法结果，否则回退到默认 `DamageTool` 伤害逻辑。

## Projectile Capability 契约

- `ProjectileTool.Spawn` 是当前纯 Runtime 投射物生成入口，生成 `RuntimeEntity`，写入 `ProjectileDataKeys` 和 Movement `Position / FacingDirection`，并发布 `Capabilities.Projectile.Events.Spawned`。
- `ProjectileTool.StartMovement` 是当前投射物飞行和命中生命周期入口，使用外部传入的 `MovementSystem` 启动移动，并通过 `MovementCollisionParams.OnCollision` 把命中转换为 `DamageService` 伤害结算和 `Capabilities.Projectile.Events.Hit`。
- Movement 碰撞在一次位移段内会连续派发多个有效目标，支持 Projectile 同帧穿透；同一次移动内已命中的目标会去重，且会显式跳过移动实体自身。
- `ProjectileMovementOptions` 当前覆盖 `Mode / Speed / MaxDuration / MaxDistance / ReachDistance / StopAtTarget / ApplyDamageOnHit / StopAfterHitCount / DestroyOnStop / IgnoreSameTeam / TargetMatchMode / SourceRadiusOverride / TargetRadiusOverride / Damage / DamageType / DamageTags`；`StopAfterHitCount` 默认读取 `ProjectileDataKeys.MaxHitCount`，`StopAfterHitCount = -1` 表示只命中通知和伤害但不停止；`MaxDuration` 默认读取 `ProjectileDataKeys.MaxLifeTime`，`-1` 表示不限制。
- `DestroyOnStop` 对 Projectile 的碰撞停止、到时、到距离和完成停止都生效；默认开启，用于停止后销毁 Runtime 投射物并触发 GodotBridge 清理视觉节点。
- `ProjectileDataKeys` 当前覆盖 `ScenePath / SourceEntity / AbilityEntity / TargetEntity / SpawnPosition / TargetPosition / Direction / Speed / MaxHitCount / HitCount / MaxLifeTime / Damage / DamageType / DamageTags / SpawnedProjectileIds`；其中 `SpawnedProjectileIds: DataKey<EntityIdList>` 位于 source 实体上，表达 spawner -> projectile 多引用。
- `ScenePath` 只保存 `res://` 字符串路径；Runtime 层不加载 Godot 场景，Godot 项目可挂 `GodotProjectileEffectSpawner` 监听生成事件并实例化视觉节点。
- 生成时 `ProjectileTool.Spawn` 以 source 为 `EntitySpawnConfig.ParentEntityId` 建立 lifecycle parent（默认 `DestroyRecursively`），同时写 typed `ProjectileDataKeys.SourceEntity / TargetEntity`、并把 projectile id append 到 source 的 `SpawnedProjectileIds`；`ProjectileTool.Initialize` 在 capability 启动处调用 `RuntimeOwnedReferenceRegistry.Register(new OwnedReferenceDescriptor(SourceEntity, SpawnedProjectileIds))` 以 framework 销毁路径自动同步 owner-list。
- 飞行 Tick 由调用方持有的 `MovementSystem` 推进；Godot 项目可用 `GodotMovementDriver` 同步 Runtime 位置到视觉节点。

## Effect Capability 契约

- `EffectTool.Spawn` 是当前纯 Runtime 效果生成入口，生成 `RuntimeEntity`，写入 `EffectDataKeys` 和 Movement `Position`，并发布 `Capabilities.Effect.Events.Spawned`。
- `EffectDataKeys` 当前覆盖 `ScenePath / Name / AnimationName / SourceEntity / AbilityEntity / TargetEntity / Position / Duration / SpawnedEffectIds`；`SpawnedEffectIds: DataKey<EntityIdList>` 位于 source 实体上。
- `Duration = -1` 表示不自动结束，遵守数值型“不限制”语义。
- 生成时 `EffectTool.Spawn` 以 source 为 `EntitySpawnConfig.ParentEntityId` 建立 lifecycle parent，同时写 typed `EffectDataKeys.SourceEntity / TargetEntity`、并把 effect id append 到 source 的 `SpawnedEffectIds`；`EffectTool.Initialize` 在 capability 启动处调用 `RuntimeOwnedReferenceRegistry.Register(new OwnedReferenceDescriptor(SourceEntity, SpawnedEffectIds))`。
- Godot 项目可挂 `GodotProjectileEffectSpawner` 监听生成事件并实例化视觉节点；Bridge 会解析 `AnimatedSprite2D` 并播放 `AnimationName`，为空时回退到场景当前动画或第一个可用动画，`Duration > 0` 时按动画帧数和 fps 调整 `SpeedScale`。
- 当前 Effect 仍不负责附着跟随和生命周期计时销毁，这些后续由 Effect runtime 扩展接入。

## Feature Capability 契约

- `FeatureService` 是当前 Feature 最小生命周期入口，负责 `Grant / Remove / Enable / Disable / Activate / End`。
- Feature 核心不引用 Ability 专有类型；子系统上下文统一通过 `FeatureContext.ActivationData` 传入，执行结果统一从 `FeatureContext.ExecuteResult` 取出。
- `FeatureDefinition` 当前包含 `FeatureId / HandlerId / Modifiers`；纯属性 Feature 可以只配置 `FeatureModifierEntry`，复杂逻辑通过 `IFeatureHandler` 扩展。
- `FeatureService.Grant` 会把 `FeatureModifierEntry` 转成 `DataModifier` 写入 Owner Data，Modifier source 标记为 Feature 实体；`Remove` 通过 `Data.RemoveModifiersBySource(feature)` 回滚。
- `FeatureHandlerRegistry` 以完整 `HandlerId` 查询 handler，不使用分组作为运行时查找键。
- `GameEventType.Feature` 已删除；Feature 事件 payload 位于 `SlimeAI/GameOS/Capabilities/Feature/Events/`，覆盖 `Granted / Removed / Enabled / Disabled / Activated / Executed / Ended`。
- `FeatureDataKeys` 当前覆盖 Feature Id / HandlerId / 描述 / 分类 / 启用状态和 modifier authoring 字段；复杂 Feature action 仍通过 handler 扩展。

## AI Capability 契约

- `AIService` 是当前 AI 最小 Tick 入口，接收复用式 `AIContext` 和行为树根节点。
- `AIContext` 只持有运行时 Entity、delta、AbilityService、外部准备好的 AbilityCastContext 和可选自动索敌技能集合，不持有 Godot Node 或物理体引用。
- `BehaviorNode.Evaluate(AIContext)` 只返回 `AIState`，通过 Entity Data 或系统入口表达意图。
- `AIService.Tick` 在 `AIDataKeys.IsEnabled=false` 或实体 `DamageDataKeys.IsDead=true` 时返回 Failure，并清空 `MovementDataKeys.AIMoveDirection`。
- `MoveToTargetAction` 只写 `MovementDataKeys.AIMoveDirection / AIMoveSpeedMultiplier`，由 `AIControlledMovementStrategy` 实际移动。
- `FindNearestTargetAction` 通过注入的 `IAITargetQuery`（默认 `RuntimeAITargetQuery`）获取候选目标，支持 `range = -1` 不限距离、同队过滤、死亡过滤，并写入 `AIDataKeys.TargetEntity / TargetPosition / HasTargetPosition`。
- `IsTargetInRangeCondition` 使用当前目标实体或目标点检查距离，`range = -1` 表示不限距离；范围可从固定值或 DataKey 读取。
- `RequestAttackAction` 发出 `Capabilities.Attack.Events.Requested`，payload 使用纯 Runtime `IEntity` 目标和 `Vector2Value` 目标点；动作会写入 `AIMoveDirection` 面向目标、`AIMoveSpeedMultiplier = 0` 停步，并把 `AIDataKeys.IsAttackRequested` 标记为本 Tick 已请求。
- `PatrolAction` 是当前确定性巡逻最小动作，读取 `AIDataKeys.PatrolCenter / PatrolRadius / PatrolWaitTime`，写入 `PatrolTargetPosition / PatrolWaitRemaining` 和 `MovementDataKeys.AIMoveDirection / AIMoveSpeedMultiplier`；不直接移动 Godot 节点。
- `EnemyBehaviorBlocks` 当前提供 `AttackBranch / ChaseBranch / PatrolBranch / AbilityAutoTriggerBranch`，只组合现有纯 Runtime 节点。
- `EnemyBehaviorTreeBuilder` 当前提供 `BuildMeleeEnemyTree / BuildAbilityMeleeEnemyTree / BuildPatrolOnlyTree / BuildChaserTree` 预制树，标准近战树顺序是攻击优先、追逐其次、巡逻兜底。
- `AIService.Tick` 每次 Tick 开始会清空 `AIDataKeys.IsAttackRequested`，只有当前 Tick 真的触发 `RequestAttackAction` 才置 true。
- `PrepareAbilityAutoTargetContextsAction` 会把 `AIContext.AutoTargetAbilities` 通过 `AbilityTargetingTool` 转成 `AIContext.AbilityContexts`，只准备上下文，不直接结算技能效果。
- `TickAbilityAutoTriggersAction` 只调用 `AbilityService.TickAutoTriggers`，不直接执行技能效果。

## Attack Capability 契约

- `AttackService` 是当前普通攻击最小 Runtime 入口，可直接调用 `TryRequest(...)`，也可通过 `Register(entity)` 订阅实体局部 `Capabilities.Attack.Events.Requested`。
- `AttackDataKeys.Damage / Range / Interval / WindUpTime / RecoveryTime / CanAttack / IsAttacking / State / CooldownRemaining` 是当前稳定 Data 入口。
- `TryRequest` 会检查目标、攻击者死亡、目标死亡、`CanAttack`、攻击占用状态、冷却和距离；通过后发布 `Attack.Started`，进入前摇或立即结算。
- 命中结算统一调用 `DamageService`，伤害标签固定带 `DamageTags.Attack`，不直接修改 HP。
- 前摇、后摇和冷却由注入的 `TimerManager` 驱动；`Cancel` 会取消当前攻击相关 Timer、回到 Idle，并发布 `Attack.Cancelled`。
- `AttackService` 不播放动画、不持有 Godot Node；Godot 层通过 `GodotAttackComponent` 完成服务注册、导出参数写入、节点目标映射和动画请求转发，动画实际播放由 `GodotUnitAnimationComponent` 执行。
- `UnitDataKeys` 当前覆盖单位名称、实体类型、死亡类型、视觉场景路径、血条配置、拾取范围、经验奖励、检测范围和 `AvailableAnimations`；视觉路径只保存 `res://` 字符串。

当前不包含：

- 旧 `IEntity` 直接依赖。
- 旧全局命名空间 `GameEventType`、`GlobalEventBus`、`EventContext`、`EventPriority` 或 `On / Emit / Once / Off` API。
- Godot headless 场景断言。
- PhysicsServer2D trace 读取协议。
- Capability 层：Ability 点选目标上下文、自动索敌第一段、Projectile / Effect 纯 Runtime 生成入口、Projectile 命中生命周期、Projectile 穿透 / 生命周期扩展、Godot 实例化第一段和 Effect 动画播放第一段已迁入；DataOS authoring 已建立 schema / migration / generator / validator / Runtime snapshot loader 最小闭环；AI 巡逻最小 Runtime、行为树预制块、Ability 自动索敌上下文准备和 Godot AIComponent bridge 第一段已迁入；GodotAttackComponent、Attack 动画事件桥、旧 Attack 动画选择兼容和旧 AttackComponent 类名兼容包装已迁入；Movement 已完成旧 `MoveMode` 纯 C# 策略、Godot 2D 位移桥、运动碰撞、同帧多命中、Godot Physics broadphase 和 Godot Orientation 第一段；Collision / Damage / Attack 已完成第一段。

## 后续契约分层

- Runtime 契约：Runtime Entity / Runtime Data / Event / Relationship / RuntimeSchedule / Pool / Timer / Resource；`Component` 和 `System` 只作为 GodotBridge / Schedule 兼容符号解释。
- Capability 契约：Movement / Collision / Damage / Ability / Feature / AIBehavior / Projectile / Spawn / UIHud。
- Validation 契约：build、scene test、capability test、regression gate。
- Observation 契约：logs、dump、trace、snapshot、reports。

## 修改门禁

- Runtime API 改动必须同步本文件和 `ApiIndex.md`。
- Capability 新增必须带 manifest、`Contract.md`、测试和 Debug 文档。
- 发布前必须通过 `Tools/run-build.sh` 和 `Tools/run-tests.sh`。
