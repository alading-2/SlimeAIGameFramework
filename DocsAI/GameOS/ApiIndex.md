# SlimeAI.GameOS API Index

> Version: 0.1.0-alpha.0

## AI-facing 术语映射

| Preferred term | Legacy public symbols / search terms | 解释 |
| --- | --- | --- |
| `Runtime Entity` | `IEntity`、`RuntimeEntity`、`EntityManager`、`Entity` | 运行时对象身份容器，暴露 `EntityId + Data + Events`；不是 archetype entity 或行为 owner。 |
| `Runtime World` | `RuntimeWorld`、`RuntimeWorld.Default`、`RuntimeWorld.CreateScoped`、`WorldEvents.World` | GameOS Runtime 状态容器；`Default` 承载旧 static facade，`CreateScoped()` 用于测试沙箱。 |
| `Runtime Data / DataKey` | `Data`、`DataKey<T>`、`DataCatalog`、`DataSlot`、历史 `Component` 搜索 | 状态契约和 typed 读写入口；不是 ECS component storage。 |
| `Capability` | `Movement`、`Damage`、`Ability`、`Feature` 等 Capability API | 玩法组合和所有权单元；行为进入 service、tool、handler、DataKey、Event 或 selector。 |
| `GodotBridge Adapter` | `IGodotComponent`、`Godot*Component`、`AttackComponent`、`Component` | Godot Node 生命周期/输入/物理/表现桥接；旧符号是 compatibility name，不是传统 ECS data component。 |
| `Runtime Process` / `Schedule Process` | `IRuntimeSystem`、`SystemConfig`、`SystemRunCondition`、`SystemGroup`、`MovementSystem`、`System` | `RuntimeSchedule` 调度执行单元；旧符号是 schedule compatibility name，不是 ECS query system。 |

本索引保留 legacy symbol 以便搜索和迁移。Phase 1 不做 C# public API rename；任何 `IEntity`、`RuntimeEntity`、`EntityManager`、`IGodotComponent`、`IRuntimeSystem`、`SystemConfig` 或 `MovementSystem` 改名都必须进入单独 OpenSpec code migration RFC。

## Runtime

| API | 类型 | 状态 | 说明 |
| --- | --- | --- | --- |
| `SlimeAI.GameOS.Runtime.GameOSInfo` | static class | bootstrap | 框架身份和迁移阶段标记。 |
| `GameOSInfo.FrameworkId` | const string | stable | `SlimeAI.GameOS`。 |
| `GameOSInfo.Version` | const string | bootstrap | 当前框架版本。 |
| `GameOSInfo.Stage` | const string | bootstrap | 当前迁移阶段。 |
| `SlimeAI.GameOS.Runtime.World.RuntimeWorld` | class | runtime-world | GameOS Runtime 世界容器；公开 `Default / CreateScoped / Entities / Lifecycle / Events / Resources / Pools / IsDefault / IsDisposed`。 |
| `RuntimeWorld.Default` | static property | runtime-world | 进程级 eager singleton；现有 static facade 全部转发到此实例。 |
| `RuntimeWorld.CreateScoped()` | static method | runtime-world | 创建独立 sandbox world；测试和局部运行域优先使用。 |
| `RuntimeWorld.Entities` | property | runtime-world | 当前 world 的 `IEntityRegistry` 句柄。 |
| `RuntimeWorld.Lifecycle` | property | runtime-world | 当前 world 的 `ILifecycleTree` 句柄。 |
| `RuntimeWorld.Events` | property | runtime-world | 当前 world 的 `IWorldEventBus` 句柄。 |
| `RuntimeWorld.Resources` | property | runtime-world | 当前 world 的 `IResourceCatalog` 句柄。 |
| `RuntimeWorld.Pools` | property | runtime-world | 当前 world 的 `IObjectPoolManager` 句柄。 |
| `SlimeAI.GameOS.Runtime.World.IEntityRegistry` | interface | internal-only abstraction | RuntimeWorld 组合用实体注册表句柄；不支持外部自定义实现或 mock。 |
| `SlimeAI.GameOS.Runtime.World.ILifecycleTree` | interface | internal-only abstraction | RuntimeWorld 组合用 lifecycle 树句柄；不支持外部自定义实现或 mock。 |
| `SlimeAI.GameOS.Runtime.World.IWorldEventBus` | interface | internal-only abstraction | RuntimeWorld 组合用 world event bus 句柄；不支持外部自定义实现或 mock。 |
| `SlimeAI.GameOS.Runtime.World.IResourceCatalog` | interface | internal-only abstraction | RuntimeWorld 组合用资源目录句柄；不支持外部自定义实现或 mock。 |
| `SlimeAI.GameOS.Runtime.World.IObjectPoolManager` | interface | internal-only abstraction | RuntimeWorld 组合用对象池管理句柄；不支持外部自定义实现或 mock。 |
| `SlimeAI.GameOS.Runtime.Data.Data` | class | typed-contract | 运行时 typed 数据容器，绑定 `DataCatalog`，公开 `DataKey<T>` 读写 API。 |
| `SlimeAI.GameOS.Runtime.Data.DataKey<T>` | class | typed-contract | 业务 Data 访问入口，定义 stable key、runtime default、类型、分类、modifier/computed 规则。 |
| `SlimeAI.GameOS.Runtime.Data.IDataKey` | interface | typed-contract | Catalog、snapshot、debug 边界使用的非泛型只读 DataKey 视图。 |
| `SlimeAI.GameOS.Runtime.Data.DataSlot<T>` | class | typed-contract | 内部 typed slot，管理 base value、modifier、computed dirty/cache 和 default fallback。 |
| `SlimeAI.GameOS.Runtime.Data.DataCatalog` | class | typed-contract | frozen framework/profile/test DataKey contract，负责 stable key resolve、key id lookup 和 capability metadata。 |
| `SlimeAI.GameOS.Runtime.Data.DataKeyRegistry` | static class | internal-contract | 进程内 typed DataKey 注册辅助，业务代码不直接按字符串写 Data。 |
| `SlimeAI.GameOS.Runtime.Data.FrameworkDataKeys` | static class | typed-contract | 显式注册框架 Runtime / Capability DataKeys。 |
| `SlimeAI.GameOS.Runtime.Data.DataModifier` | class | migrated | 数值修改器。 |
| `SlimeAI.GameOS.Runtime.Data.ModifierType` | enum | migrated | 修改器类型。 |
| `SlimeAI.GameOS.Runtime.Data.IDataChangeSink` | interface | bootstrap | Event Runtime 迁入前的数据变更通知桥。 |
| `SlimeAI.GameOS.Runtime.Data.DataChangedEventData` | record struct | typed-contract | 数据变更通知 payload，可从 `IDataKey` 派生 stable key。 |
| `SlimeAI.GameOS.Runtime.Data.RuntimeDataSnapshot` | class | typed-loader | DataOS typed snapshot 读取入口，先校验 manifest/descriptors/catalog，再 typed apply records 并注册资源。 |
| `SlimeAI.GameOS.Runtime.Data.RuntimeSnapshotManifest` | class | typed-loader | Snapshot manifest：schema、profile、catalog、enabled capabilities、counts、validation summary。 |
| `SlimeAI.GameOS.Runtime.Data.RuntimeDataDescriptor` | class | typed-loader | Snapshot descriptor mirror：stable key、owner capability、authoring metadata、type/default mirror。 |
| `SlimeAI.GameOS.Runtime.Data.RuntimeDataRecord` | class | typed-loader | DataOS snapshot 中的一条数据记录。 |
| `SlimeAI.GameOS.Runtime.Data.RuntimeDataField` | class | typed-loader | DataOS snapshot 字段值，按 resolved `DataKey<T>` 严格转换，不静默替换默认值。 |
| `SlimeAI.GameOS.Runtime.Data.RuntimeResourceEntry` | class | typed-loader | DataOS snapshot 资源映射条目，受 profile/capability trimming 约束。 |
| `SlimeAI.GameOS.Runtime.Data.RuntimeSnapshotApplyReport` | class | typed-loader | typed apply / resource registration 结构化报告。 |
| `SlimeAI.GameOS.Runtime.Event.IEvent` | marker interface | migrated | 所有事件 payload 的基础标记接口。 |
| `SlimeAI.GameOS.Runtime.Event.IEntityEvent` | marker interface | migrated | 仅派发到目标 entity bus 的事件。 |
| `SlimeAI.GameOS.Runtime.Event.IGlobalEvent` | marker interface | migrated | 仅派发到 world bus 的事件。 |
| `SlimeAI.GameOS.Runtime.Event.IBroadcastEvent` | marker interface | migrated | entity bus Publish 后自动转发到 world bus。 |
| `SlimeAI.GameOS.Runtime.Event.IEventBus` | interface | migrated | `Publish<T>(in T) / Subscribe<T>(Action<T>) → IDisposable / ExportObservation(path)`。 |
| `SlimeAI.GameOS.Runtime.Event.EntityEventBus` | class | migrated | 实体级事件总线，支持 IBroadcastEvent 自动转发。 |
| `SlimeAI.GameOS.Runtime.Event.WorldEventBus` | class | migrated | 进程级事件总线，替换旧 `GlobalEventBus`。 |
| `SlimeAI.GameOS.Runtime.Event.WorldEvents` | static class | runtime-world | `WorldEvents.World` 转发到 `RuntimeWorld.Default.Events`。 |
| `SlimeAI.GameOS.Runtime.Event.EventBusObservation` | class | migrated | 订阅、发布计数、reentry 阻断、handler 异常和 dump 导出。 |
| `SlimeAI.GameOS.Runtime.Event.EventDataChangeSink` | class | migrated | Data 变更到 IEventBus 的桥，发布 `Runtime.Events.Core.DataPropertyChanged`。 |
| `SlimeAI.GameOS.Runtime.Entity.EntityId` | record struct | migrated | Typed entity 身份 wrapper；`Empty / From(string?) / IsEmpty / Value`，禁止 implicit `string` 转换。 |
| `SlimeAI.GameOS.Runtime.Entity.IEntity` | interface | migrated | Runtime Entity legacy interface；只暴露 typed `EntityId`、Runtime Data 和事件，不承载业务逻辑。 |
| `SlimeAI.GameOS.Runtime.Entity.RuntimeEntity` | class | migrated | 纯 C# Runtime Entity 身份容器，构造接受 typed `EntityId`，不是 archetype entity。 |
| `SlimeAI.GameOS.Runtime.Entity.EntitySpawnConfig` | record struct | migrated | Runtime Entity 生成参数；`EntityId / ParentEntityId` 为 typed `EntityId`，以 `EntityId.Empty` 表达"未指定"；只含 `EntityId / DataCatalog / ParentEntityId / ParentDestroyPolicy`。 |
| `SlimeAI.GameOS.Runtime.Entity.EntityManager` | static class | runtime-world | Runtime Entity static facade，转发到 `RuntimeWorld.Default.Entities`；`Spawn / Register / Destroy / Get / GetAll / Clear / AttachLifecycleParent` 全部使用 typed `EntityId`。 |
| `SlimeAI.GameOS.Runtime.Entity.LifecycleTree` | static class | runtime-world | Lifecycle parent 单父树 static facade，转发到 `RuntimeWorld.Default.Lifecycle`；typed API，发布 `LifecycleChildAttached / LifecycleChildDetached`。 |
| `SlimeAI.GameOS.Runtime.Entity.LifecycleLink` | record struct | migrated | `(ParentEntityId, ChildEntityId, DestroyPolicy, Priority)` 不可变 lifecycle 边。 |
| `SlimeAI.GameOS.Runtime.Entity.ParentDestroyPolicy` | enum | migrated | `DestroyRecursively / Detach`，lifecycle 销毁策略；原位于 `Runtime.Relationship`，现迁入 `Runtime.Entity`。 |
| `SlimeAI.GameOS.Runtime.Entity.EntityIdList` | record struct | migrated | typed entity-id 多引用不可变值；`Empty / Add / Remove / Contains / Count / [int]`，按内容与顺序 value-equality。 |
| `SlimeAI.GameOS.Runtime.Entity.OwnedReferenceDescriptor` | record struct | migrated | Owner cleanup descriptor：`(DataKey<EntityId?> ChildToOwnerKey, DataKey<EntityIdList> OwnerListKey)`。 |
| `SlimeAI.GameOS.Runtime.Entity.IOwnedReferenceCleaner` | interface | migrated | Capability 可注册的销毁回调接口，`OnEntityDestroying(IEntity destroyed)`。 |
| `SlimeAI.GameOS.Runtime.Entity.RuntimeOwnedReferenceRegistry` | static class | migrated | `Register(OwnedReferenceDescriptor) / Register(IOwnedReferenceCleaner) / Clear()`；`EntityManager.Destroy` 销毁路径调用 `NotifyDestroying`。 |
| `SlimeAI.GameOS.Runtime.Schedule.GameFlowState` | enum | migrated | 游戏流程状态。 |
| `SlimeAI.GameOS.Runtime.Schedule.OverlayFlags` | enum | migrated | 项目覆盖层标记。 |
| `SlimeAI.GameOS.Runtime.Schedule.SimulationState` | enum | migrated | 项目模拟状态。 |
| `SlimeAI.GameOS.Runtime.Schedule.SystemGroup` | enum | migrated | Schedule Process 分组；`System` 为 legacy compatibility name。 |
| `SlimeAI.GameOS.Runtime.Schedule.SystemTag` | enum | migrated | Schedule Process 标签；`System` 为 legacy compatibility name。 |
| `SlimeAI.GameOS.Runtime.Schedule.ProjectStateService` | class | migrated | 项目状态服务。 |
| `SlimeAI.GameOS.Runtime.Schedule.SystemRunCondition` | class | migrated | Runtime Process 运行条件；`System` 为 schedule compatibility name。 |
| `SlimeAI.GameOS.Runtime.Schedule.ScheduleCategory` | enum | bootstrap | Schedule DataKey 分类：System / Preset / Spawn；`System` 分类名暂为 legacy authoring category。 |
| `SlimeAI.GameOS.Runtime.Schedule.ScheduleDataKeys` | static class | bootstrap | Runtime Process 配置、preset 和 Spawn config 使用的 Runtime DataKey。 |
| `SlimeAI.GameOS.Runtime.Schedule.IRuntimeSystem` | interface | migrated | Runtime Process 生命周期协议的 legacy compatibility name。 |
| `SlimeAI.GameOS.Runtime.Schedule.IRuntimeCommandHandler<TRequest,TResult>` | interface | migrated | Runtime Process 命令处理协议。 |
| `SlimeAI.GameOS.Runtime.Schedule.RuntimeSchedule` | class | migrated | 纯 C# Runtime 调度器。 |
| `SlimeAI.GameOS.Runtime.Resource.ResourceCategory` | enum | migrated | 资源分类。 |
| `SlimeAI.GameOS.Runtime.Resource.ResourceData` | record struct | migrated | 资源映射条目。 |
| `SlimeAI.GameOS.Runtime.Resource.ResourceCatalog` | static class | runtime-world | 资源键到路径映射 static facade，转发到 `RuntimeWorld.Default.Resources`。 |
| `SlimeAI.GameOS.Runtime.Resource.ResourceManagement` | static class | migrated | 统一资源加载 facade。 |
| `SlimeAI.GameOS.Runtime.Pool.IPoolable` | interface | migrated | 对象池生命周期接口。 |
| `SlimeAI.GameOS.Runtime.Pool.IObjectPool` | interface | migrated | 全局池注册表使用的非泛型句柄。 |
| `SlimeAI.GameOS.Runtime.Pool.ObjectPoolConfig` | record struct | migrated | 对象池配置。 |
| `SlimeAI.GameOS.Runtime.Pool.PoolStats` | record struct | migrated | 对象池统计。 |
| `SlimeAI.GameOS.Runtime.Pool.ObjectPool<T>` | class | migrated | 类型安全对象池。 |
| `SlimeAI.GameOS.Runtime.Pool.ObjectPoolManager` | static class | runtime-world | 对象池注册和归还 static facade，转发到 `RuntimeWorld.Default.Pools`。 |
| `SlimeAI.GameOS.Runtime.Timer.GameTimer` | class | migrated | 可池化运行时计时器。 |
| `SlimeAI.GameOS.Runtime.Timer.TimerManager` | class | migrated | 由外部 Tick 驱动的计时器管理器。 |
| `SlimeAI.GameOS.Runtime.Events.Core` | namespace | migrated | Runtime 基础事件 payload：`EntitySpawned / EntityDestroyed / DataPropertyChanged / LifecycleChildAttached / LifecycleChildDetached`。BrotatoLike 主动技能输入事件已迁出框架，见 `Games/BrotatoLike/Src/Game/Event/BrotatoLikeInputEvents.cs`。 |
| `SlimeAI.GameOS.Runtime.Events.Global` | namespace | reserved-empty | 当前无 framework-owned 全局 payload。Bucket A 旧事件 `MouseSelectionCompleted / MouseSelectionMissed / MouseSelectionPreviewUpdated / WaveStarted / WaveCompleted / GameStart / GameOver / GamePause / GameResume` 已删除，未在游戏侧创建替换。 |
| `SlimeAI.GameOS.Capabilities.Movement.Events` | namespace | migrated | Movement Capability 事件 payload：`Started / Stopped / Collision`。 |
| `SlimeAI.GameOS.Capabilities.Collision.Events` | namespace | bootstrap | Collision Capability 事件 payload：`Entered / Exited / HurtboxEntered / HurtboxExited`。 |
| `SlimeAI.GameOS.Capabilities.Attack.Events` | namespace | bootstrap | Attack Capability 事件 payload：`Requested / Started / Finished / Cancelled / CancelRequested`。 |
| `SlimeAI.GameOS.Capabilities.Unit.Events` | namespace | bootstrap | Unit 表现层动画事件 payload：`PlayAnimationRequested / StopAnimationRequested / AnimationFinished`。 |
| `SlimeAI.GameOS.Capabilities.Feature.Events` | namespace | bootstrap | Feature Capability 生命周期事件 payload：`Granted / Removed / Enabled / Disabled / Activated / Executed / Ended`。 |

## Capabilities

| API | 类型 | 状态 | 说明 |
| --- | --- | --- | --- |
| `SlimeAI.GameOS.Capabilities.Movement.Vector2Value` | record struct | migrated | 纯 C# 二维向量值，避免 Capability 内核直接依赖 Godot 类型。 |
| `SlimeAI.GameOS.Capabilities.Movement.MovementDataKeys` | static class | migrated | Movement 运行时与 authoring DataKey：Position / Velocity / FacingDirection / IsMoving / MoveSpeed / InputDirection / AIMoveDirection / AIMoveSpeedMultiplier / CanMoveInput / LastMoveDirection，以及 handler 可映射到 MovementParams 的 SineWave / Orbit / Boomerang / Bezier / Parabola / CircularArc 参数。 |
| `SlimeAI.GameOS.Capabilities.Movement.MoveMode` | enum | migrated | 当前迁入 `None / Charge / Orbit / SineWave / BezierCurve / Boomerang / AttachToHost / PlayerInput / AIControlled / Parabola / CircularArc`。 |
| `SlimeAI.GameOS.Capabilities.Movement.MovementParams` | record struct | migrated | 全部旧 `MoveMode` 纯运行时策略使用的方向、目标点、目标实体、附着偏移、速度、轨道、波形、Bezier、Boomerang、曲线、时长、距离、可选运动碰撞参数和朝向参数。 |
| `SlimeAI.GameOS.Capabilities.Movement.OrientationMode` | enum | bootstrap | Movement 视觉朝向模式：FollowMovement / SpinOnly / FollowMovementAndSpin。 |
| `SlimeAI.GameOS.Capabilities.Movement.OrientationSink` | enum | bootstrap | Movement 朝向输出目标：RootRotation / VisualFlipX。 |
| `SlimeAI.GameOS.Capabilities.Movement.OrientationParams` | record struct | bootstrap | Movement 期间可选的视觉朝向控制参数。 |
| `SlimeAI.GameOS.Capabilities.Movement.MovementCollisionParams` | record struct | bootstrap | 单次移动的碰撞策略：Collision 过滤、目标匹配、半径覆盖、碰撞次数停止、销毁和回调。 |
| `SlimeAI.GameOS.Capabilities.Movement.MovementCollisionPolicy` | class | bootstrap | 纯运行时 Movement 碰撞过滤、去重、计数和线段/圆形扫描。 |
| `SlimeAI.GameOS.Capabilities.Movement.IMovementCollisionTargetQuery` | interface | bootstrap | Movement 碰撞候选目标查询接口，允许 Runtime 扫描或 Godot Physics broadphase 注入。 |
| `SlimeAI.GameOS.Capabilities.Movement.MovementCollisionRuntimeTargetQuery` | class | bootstrap | 默认 EntityManager 快照碰撞候选查询。 |
| `SlimeAI.GameOS.Capabilities.Movement.MovementCollisionContext` | record struct | bootstrap | 有效 Movement 碰撞上下文。 |
| `SlimeAI.GameOS.Capabilities.Movement.MovementCollisionTargetMatchMode` | enum | bootstrap | Movement 碰撞目标匹配模式。 |
| `SlimeAI.GameOS.Capabilities.Movement.IMovementStrategy` | interface | migrated | Movement Strategy 生命周期接口。 |
| `SlimeAI.GameOS.Capabilities.Movement.MovementStrategyRegistry` | static class | migrated | `MoveMode -> IMovementStrategy` 注册表。 |
| `SlimeAI.GameOS.Capabilities.Movement.ChargeMovementStrategy` | class | migrated | 纯 C# Charge 直线 / 到点移动策略。 |
| `SlimeAI.GameOS.Capabilities.Movement.OrbitMovementStrategy` | class | migrated | 纯 C# 固定点环绕策略。 |
| `SlimeAI.GameOS.Capabilities.Movement.SineWaveMovementStrategy` | class | migrated | 纯 C# 正弦波前进策略。 |
| `SlimeAI.GameOS.Capabilities.Movement.BezierCurveMovementStrategy` | class | migrated | 纯 C# 固定控制点 Bezier 曲线策略。 |
| `SlimeAI.GameOS.Capabilities.Movement.BoomerangMovementStrategy` | class | migrated | 纯 C# 去程、停顿、返程回旋镖策略。 |
| `SlimeAI.GameOS.Capabilities.Movement.AttachToHostMovementStrategy` | class | migrated | 纯 C# 目标实体 Position 跟随策略。 |
| `SlimeAI.GameOS.Capabilities.Movement.PlayerInputMovementStrategy` | class | migrated | 消费 `MovementDataKeys.InputDirection` 的玩家输入策略。 |
| `SlimeAI.GameOS.Capabilities.Movement.AIControlledMovementStrategy` | class | migrated | 消费 AI 方向和速度倍率 DataKey 的 AI 常驻移动策略。 |
| `SlimeAI.GameOS.Capabilities.Movement.ParabolaMovementStrategy` | class | migrated | 纯 C# 抛物线轨迹策略。 |
| `SlimeAI.GameOS.Capabilities.Movement.CircularArcMovementStrategy` | class | migrated | 纯 C# 圆弧轨迹策略。 |
| `SlimeAI.GameOS.Capabilities.Movement.MovementSystem` | class | migrated | 纯 C# Movement Runtime Process，负责 Start / Stop / Tick / 位置积分 / 停止事件；类名保留 `System` 兼容。 |
| `SlimeAI.GameOS.Capabilities.Movement.MovementStopReason` | enum | migrated | Movement 停止原因。 |
| `SlimeAI.GameOS.Capabilities.Movement.MovementStopContext` | record struct | migrated | Movement 停止事件上下文。 |
| `SlimeAI.GameOS.Capabilities.Collision.CollisionLayers` | static class | migrated | 旧项目 2D 碰撞层常量。 |
| `SlimeAI.GameOS.Capabilities.Collision.CollisionDataKeys` | static class | bootstrap | Collision 运行时 DataKey：CollisionLayer / CollisionMask / Team / CollisionRadius。 |
| `SlimeAI.GameOS.Capabilities.Collision.CollisionFilterPolicy` | record struct | bootstrap | layer/mask、自身和同队过滤策略。 |
| `SlimeAI.GameOS.Capabilities.Collision.CollisionContact` | record struct | bootstrap | 纯运行时碰撞接触 payload。 |
| `SlimeAI.GameOS.Capabilities.Collision.CollisionSystem` | class | bootstrap | 纯 C# Collision Runtime Process / service-like executor，负责过滤和事件发布；类名保留 `System` 兼容。 |
| `SlimeAI.GameOS.Capabilities.Damage.DamageDataKeys` | static class | bootstrap | Damage 运行时 DataKey：CurrentHp / MaxHp / IsDead / IsInvulnerable / Armor / Shield / DodgeChance / CritRate / CritDamage / DamageTakenMultiplier / LifeSteal / ContactDamage / ContactDamageInterval / Damage 统计键。 |
| `SlimeAI.GameOS.Capabilities.Damage.DamageType` | enum | bootstrap | 伤害类型：Physical / Magical / True。 |
| `SlimeAI.GameOS.Capabilities.Damage.DamageTags` | enum flags | bootstrap | 伤害标签：Attack / Ability / Contact / Persistent / Area / Projectile。 |
| `SlimeAI.GameOS.Capabilities.Damage.DamageInfo` | class | bootstrap | 单次伤害上下文。 |
| `SlimeAI.GameOS.Capabilities.Damage.DamageResult` | record struct | bootstrap | 单次伤害处理结果。 |
| `SlimeAI.GameOS.Capabilities.Damage.IDamageProcessor` | interface | bootstrap | Damage 处理器协议。 |
| `SlimeAI.GameOS.Capabilities.Damage.DamageProcessorPriority` | static class | bootstrap | 默认 Damage 处理器优先级常量。 |
| `SlimeAI.GameOS.Capabilities.Damage.BaseDamageProcessor` | class | bootstrap | 伤害前置检查和基础伤害初始化。 |
| `SlimeAI.GameOS.Capabilities.Damage.DodgeProcessor` | class | bootstrap | 非真实伤害闪避判定和 `damage:dodged` 事件。 |
| `SlimeAI.GameOS.Capabilities.Damage.CriticalDamageProcessor` | class | bootstrap | 暴击概率和暴击倍率结算。 |
| `SlimeAI.GameOS.Capabilities.Damage.ShieldDamageProcessor` | class | bootstrap | 护盾吸收，优先于护甲结算。 |
| `SlimeAI.GameOS.Capabilities.Damage.ArmorDamageProcessor` | class | bootstrap | 护甲倍率结算，真实伤害跳过。 |
| `SlimeAI.GameOS.Capabilities.Damage.DamageTakenAmplificationProcessor` | class | bootstrap | 目标受伤倍率结算。 |
| `SlimeAI.GameOS.Capabilities.Damage.HealthExecutionProcessor` | class | bootstrap | 生命值扣减、HealthChanged / Damaged / Killed 事件。 |
| `SlimeAI.GameOS.Capabilities.Damage.LifestealProcessor` | class | bootstrap | 基于最终伤害和 LifeSteal 触发吸血恢复。 |
| `SlimeAI.GameOS.Capabilities.Damage.DamageStatisticsProcessor` | class | bootstrap | 伤害、命中、暴击、击杀、波次统计写入。 |
| `SlimeAI.GameOS.Capabilities.Damage.DamageService` | class | bootstrap | 统一伤害入口，管理默认处理器管线并返回 DamageResult。 |
| `SlimeAI.GameOS.Capabilities.Damage.HealSource` | enum | bootstrap | 治疗来源：Direct / Lifesteal / Regeneration。 |
| `SlimeAI.GameOS.Capabilities.Damage.HealInfo` | class | bootstrap | 单次治疗上下文。 |
| `SlimeAI.GameOS.Capabilities.Damage.HealResult` | record struct | bootstrap | 单次治疗处理结果。 |
| `SlimeAI.GameOS.Capabilities.Damage.HealService` | class | bootstrap | 统一治疗入口，负责 HP 夹取、统计和 Healed / HealthChanged 事件。 |
| `SlimeAI.GameOS.Capabilities.Damage.DamageApplyOptions` | record struct | bootstrap | DamageTool 单次伤害参数。 |
| `SlimeAI.GameOS.Capabilities.Damage.DamageRepeatOptions` | record struct | bootstrap | DamageTool 周期伤害参数。 |
| `SlimeAI.GameOS.Capabilities.Damage.DamageApplyResult` | class | bootstrap | DamageTool 多目标 / 周期伤害结果。 |
| `SlimeAI.GameOS.Capabilities.Damage.DamageTool` | static class | bootstrap | 多目标和周期伤害工具，统一调用 DamageService 与 TimerManager。 |
| `SlimeAI.GameOS.Capabilities.Ability.AbilityDataKeys` | static class | bootstrap | Ability 运行时 DataKey：类型、触发模式、目标语义、自动索敌、描述 / 图标 / 分组 / 消耗、冷却、充能、伤害、施法范围、效果半径和链式参数。 |
| `SlimeAI.GameOS.Capabilities.Ability.AbilityType` | enum | bootstrap | 技能类型：Active / Passive / Weapon。 |
| `SlimeAI.GameOS.Capabilities.Ability.AbilityTriggerMode` | enum flags | bootstrap | 技能触发模式：Manual / OnEvent / Periodic / Permanent。 |
| `SlimeAI.GameOS.Capabilities.Ability.AbilityTargetSelection` | enum | bootstrap | 技能目标输入语义：None / Entity / Point / EntityOrPoint。 |
| `SlimeAI.GameOS.Capabilities.Ability.AbilityTriggerResult` | enum | bootstrap | 技能触发结果和失败原因。 |
| `SlimeAI.GameOS.Capabilities.Ability.AbilityCastContext` | class | bootstrap | 单次技能触发上下文，支持显式实体目标和点选目标位置。 |
| `SlimeAI.GameOS.Capabilities.Ability.AbilityExecutedResult` | class | bootstrap | 技能执行结果。 |
| `SlimeAI.GameOS.Capabilities.Ability.AbilityTriggerReport` | record struct | bootstrap | 技能触发报告。 |
| `SlimeAI.GameOS.Capabilities.Ability.AbilityService` | class | bootstrap | Ability 最小运行时服务，负责实体 / 点选目标正式触发、冷却、充能、Periodic 自动触发 Tick、DamageTool 命中和可选 Feature handler 调用。 |
| `SlimeAI.GameOS.Capabilities.Ability.AbilityAutoTargetOptions` | class | bootstrap | Ability 自动索敌参数：Range / MaxTargets / IgnoreSameTeam / RequiresDamageable / IncludeDeadTargets。 |
| `SlimeAI.GameOS.Capabilities.Ability.AbilityTargetingTool` | static class | bootstrap | Ability 显式目标准备工具，按 Ability 自动索敌 DataKey 从 Runtime Entity 快照构造 `AbilityCastContext`。 |
| `SlimeAI.GameOS.Capabilities.Projectile.ProjectileCategory` | enum | bootstrap | Projectile DataKey 分类。 |
| `SlimeAI.GameOS.Capabilities.Projectile.ProjectileDataKeys` | static class | bootstrap | Projectile 运行时 DataKey：ScenePath / SourceEntity / AbilityEntity / TargetEntity / SpawnPosition / TargetPosition / Direction / Speed / MaxHitCount / HitCount / MaxLifeTime / Damage / DamageType / DamageTags。 |
| `SlimeAI.GameOS.Capabilities.Projectile.ProjectileSpawnOptions` | class | bootstrap | 投射物生成参数。 |
| `SlimeAI.GameOS.Capabilities.Projectile.ProjectileSpawnResult` | record struct | bootstrap | 投射物生成结果。 |
| `SlimeAI.GameOS.Capabilities.Projectile.ProjectileMovementOptions` | class | bootstrap | 投射物飞行、穿透和生命周期参数，配置移动、命中伤害、最大命中数、最大存活时间、停止和销毁语义。 |
| `SlimeAI.GameOS.Capabilities.Projectile.ProjectileTool` | static class | bootstrap | 纯 Runtime 投射物工具，支持生成、启动 Movement 飞行、穿透多目标、命中伤害结算、停止销毁并发布 `projectile:spawned / projectile:hit`。 |
| `SlimeAI.GameOS.Capabilities.Effect.EffectCategory` | enum | bootstrap | Effect DataKey 分类。 |
| `SlimeAI.GameOS.Capabilities.Effect.EffectDataKeys` | static class | bootstrap | Effect 运行时 DataKey：ScenePath / Name / AnimationName / SourceEntity / AbilityEntity / TargetEntity / Position / Duration。 |
| `SlimeAI.GameOS.Capabilities.Effect.EffectSpawnOptions` | class | bootstrap | 效果生成参数。 |
| `SlimeAI.GameOS.Capabilities.Effect.EffectSpawnResult` | record struct | bootstrap | 效果生成结果。 |
| `SlimeAI.GameOS.Capabilities.Effect.EffectTool` | static class | bootstrap | 纯 Runtime 效果生成工具，写入 Data、位置、关系并发布 `effect:spawned`。 |
| `SlimeAI.GameOS.Capabilities.Feature.FeatureDataKeys` | static class | bootstrap | Feature 运行时 DataKey：FeatureId / HandlerId / Description / Category / modifier authoring 字段 / IsEnabled / IsActive / ActivationCount。 |
| `SlimeAI.GameOS.Capabilities.Feature.FeatureDefinition` | class | bootstrap | 可授予能力定义，包含 FeatureId、HandlerId 和授予时 Modifier 列表。 |
| `SlimeAI.GameOS.Capabilities.Feature.FeatureModifierEntry` | record struct | bootstrap | Feature 授予时应用到 Owner Data 的 Modifier 配置。 |
| `SlimeAI.GameOS.Capabilities.Feature.FeatureContext` | class | bootstrap | Feature 生命周期上下文，承载 Owner / Feature / Definition / ActivationData / ExecuteResult。 |
| `SlimeAI.GameOS.Capabilities.Feature.FeatureEndReason` | enum | bootstrap | Feature 单次运行结束原因：Completed / Cancelled / Interrupted / Failed。 |
| `SlimeAI.GameOS.Capabilities.Feature.IFeatureHandler` | interface | bootstrap | Feature 处理器协议：OnGranted / OnRemoved / OnEnabled / OnDisabled / OnActivated / OnExecute / OnEnded。 |
| `SlimeAI.GameOS.Capabilities.Feature.FeatureHandlerRegistry` | static class | bootstrap | Feature handler 注册表，以完整 HandlerId 查询。 |
| `SlimeAI.GameOS.Capabilities.Feature.FeatureService` | class | bootstrap | Feature 最小生命周期服务，负责授予 / 移除 Modifier、生命周期事件和 handler 调用。 |
| `SlimeAI.GameOS.Capabilities.Attack.AttackDataKeys` | static class | bootstrap | Attack 运行时 DataKey：Damage / Range / Interval / WindUpTime / RecoveryTime / CanAttack / IsAttacking / State / CooldownRemaining。 |
| `SlimeAI.GameOS.Capabilities.Attack.AttackState` | enum | bootstrap | 普通攻击状态：Idle / WindUp / Recovery。 |
| `SlimeAI.GameOS.Capabilities.Attack.AttackCancelReason` | enum | bootstrap | 攻击流程取消原因。 |
| `SlimeAI.GameOS.Capabilities.Attack.AttackTriggerResult` | enum | bootstrap | 普通攻击请求结果和失败原因。 |
| `SlimeAI.GameOS.Capabilities.Attack.AttackTriggerReport` | record struct | bootstrap | 普通攻击请求报告。 |
| `SlimeAI.GameOS.Capabilities.Attack.AttackService` | class | bootstrap | 普通攻击最小 Runtime 服务，消费 `Capabilities.Attack.Events.Requested`，处理前摇 / 后摇 / 冷却并通过 `DamageService` 结算 `DamageTags.Attack` 伤害。 |
| `SlimeAI.GameOS.Capabilities.Unit.UnitDataKeys` | static class | bootstrap | Unit 运行时 DataKey：Name / EntityType / DeathType / VisualScenePath / HealthBarHeight / IsShowHealthBar / PickupRange / ExpReward / DetectionRange / AvailableAnimations。 |
| `SlimeAI.GameOS.Capabilities.AI.AIDataKeys` | static class | bootstrap | AI 运行时 DataKey：IsEnabled / TargetEntity / TargetPosition / HasTargetPosition / IsAttackRequested / AttackRange / PatrolCenter / PatrolRadius / PatrolWaitTime / PatrolTargetPosition / HasPatrolTargetPosition / PatrolWaitRemaining / PatrolDirectionSign。 |
| `SlimeAI.GameOS.Capabilities.AI.AIContext` | class | bootstrap | AI 行为树 Tick 上下文，承载实体、delta、AbilityService、自动施法上下文和可选自动索敌技能集合。 |
| `SlimeAI.GameOS.Capabilities.AI.AIState` | enum | bootstrap | AI 节点返回状态：Failure / Success / Running。 |
| `SlimeAI.GameOS.Capabilities.AI.BehaviorNode` | abstract class | bootstrap | AI 行为树节点基类。 |
| `SlimeAI.GameOS.Capabilities.AI.SequenceNode` | class | bootstrap | 顺序组合节点。 |
| `SlimeAI.GameOS.Capabilities.AI.SelectorNode` | class | bootstrap | 选择组合节点。 |
| `SlimeAI.GameOS.Capabilities.AI.AIService` | class | bootstrap | AI 最小 Tick 服务，负责启用 / 死亡门禁并执行行为树。 |
| `SlimeAI.GameOS.Capabilities.AI.FindNearestTargetAction` | class | bootstrap | 从 Runtime Entity 快照中查找最近目标并写入 AI 目标 Data。 |
| `SlimeAI.GameOS.Capabilities.AI.MoveToTargetAction` | class | bootstrap | 向目标写入 Movement AI 移动意图。 |
| `SlimeAI.GameOS.Capabilities.AI.IsTargetInRangeCondition` | class | bootstrap | 检查当前目标实体或目标点是否在固定范围或 DataKey 范围内。 |
| `SlimeAI.GameOS.Capabilities.AI.RequestAttackAction` | class | bootstrap | 行为树中发出 `Capabilities.Attack.Events.Requested`，并写入停步和面向目标意图。 |
| `SlimeAI.GameOS.Capabilities.AI.PrepareAbilityAutoTargetContextsAction` | class | bootstrap | 行为树中把 `AutoTargetAbilities` 转换为带自动索敌结果的 `AbilityContexts`。 |
| `SlimeAI.GameOS.Capabilities.AI.TickAbilityAutoTriggersAction` | class | bootstrap | 行为树中调用 AbilityService 推进 Periodic 自动施法。 |
| `SlimeAI.GameOS.Capabilities.AI.PatrolAction` | class | bootstrap | 确定性巡逻动作，写入 Movement AI 移动意图并维护巡逻等待倒计时。 |
| `SlimeAI.GameOS.Capabilities.AI.EnemyBehaviorBlocks` | static class | bootstrap | AI 可复用行为积木块：AttackBranch / ChaseBranch / PatrolBranch / AbilityAutoTriggerBranch。 |
| `SlimeAI.GameOS.Capabilities.AI.EnemyBehaviorTreeBuilder` | static class | bootstrap | AI 预制树工厂：BuildMeleeEnemyTree / BuildAbilityMeleeEnemyTree / BuildPatrolOnlyTree / BuildChaserTree。 |

## Observation

| API | 类型 | 状态 | 说明 |
| --- | --- | --- | --- |
| `SlimeAI.GameOS.Observation.GameOSLogLevel` | enum | bootstrap | 通用日志等级：Trace / Debug / Info / Pass / Warn / Error / Fail。 |
| `SlimeAI.GameOS.Observation.GameOSLogEntry` | class | bootstrap | 单条结构化日志，支持 `[LEVEL][Context] message key=value` 格式化。 |
| `SlimeAI.GameOS.Observation.IGameOSLogSink` | interface | bootstrap | 日志 sink 协议。 |
| `SlimeAI.GameOS.Observation.GameOSLog` | static class | bootstrap | 全局日志配置、context logger 创建和 sink 注册入口。 |
| `SlimeAI.GameOS.Observation.GameOSContextLog` | class | bootstrap | context-bound logger，提供 Trace / Debug / Info / Pass / Warn / Error / Fail。 |
| `SlimeAI.GameOS.Observation.GameOSMemoryLogSink` | class | bootstrap | 测试和 artifact 聚合用内存 sink。 |
| `SlimeAI.GameOS.Observation.GameOSJsonlLogSink` | class | bootstrap | JSONL sink，默认用于 `artifacts/logs/scene-log.jsonl`。 |
| `SlimeAI.GameOS.Observation.GameOSObservationSession` | class | bootstrap | 读取 scene runner 环境变量并准备 artifact / JSONL 路径。 |
| `SlimeAI.GameOS.Observation.CheckResult` | record | bootstrap | validation check 返回值。 |
| `SlimeAI.GameOS.Observation.SceneValidationSession` | class | bootstrap | Godot validation scene 检查项日志、失败聚合和 artifact 写入 helper。 |

## GodotBridge

| API | 类型 | 状态 | 说明 |
| --- | --- | --- | --- |
| `SlimeAI.GameOS.GodotBridge.GodotEntity` | Node | migrated | 可挂场景的 GameOS Entity 基类。 |
| `SlimeAI.GameOS.GodotBridge.GodotEntity2D` | Node2D | migrated | 可挂 2D 场景的 GameOS Entity 基类，会把初始 Node2D Position 写入 Movement Data。 |
| `SlimeAI.GameOS.GodotBridge.GodotAreaEntity2D` | Area2D | bootstrap | 可挂 2D 物理场景的 GameOS Entity 基类，会同步 Position 和 Collision layer/mask 到 Runtime Data。 |
| `SlimeAI.GameOS.GodotBridge.IGodotComponent` | interface | migrated | GodotBridge Adapter 生命周期协议的 legacy compatibility name；不是 ECS data component。 |
| `SlimeAI.GameOS.GodotBridge.GameOSGodotBridge` | static class | migrated | SceneTree 与 Runtime 生命周期桥接入口。 |
| `SlimeAI.GameOS.GodotBridge.GodotNodeRegistry` | static class | migrated | Godot Node 到稳定运行时 Id 的注册表。 |
| `SlimeAI.GameOS.GodotBridge.GameOSTimerDriver` | Node | migrated | 用 `_Process` 驱动 `TimerManager.Instance.Tick`。 |
| `SlimeAI.GameOS.GodotBridge.GodotMovementDriver` | Node | migrated | 用 `_Process` 驱动 `MovementSystem.Tick` 并同步 Runtime Position 到 Node2D。 |
| `SlimeAI.GameOS.GodotBridge.GodotOrientationComponent` | Node | bootstrap | 消费 `MovementDataKeys.FacingDirection` / `MovementParams.Orientation`，输出 `RotationDegrees` 或 `AnimatedSprite2D.FlipH`。 |
| `SlimeAI.GameOS.GodotBridge.GodotUnitAnimationComponent` | Node | bootstrap | 消费 `Capabilities.Unit.Events` 动画事件并驱动 `AnimatedSprite2D`，缓存可用动画，支持一次性动画完成事件和待机回退。 |
| `SlimeAI.GameOS.GodotBridge.GodotPhysicsMovementCollisionTargetQuery` | class | bootstrap | 用 Godot Physics 2D `IntersectShape` 收集 Movement 碰撞 broadphase 候选，并回退 Runtime Entity 扫描。 |
| `SlimeAI.GameOS.GodotBridge.GodotCollisionBridge` | static class | bootstrap | Godot `CollisionObject2D` / Node 到 Collision Runtime 的实体解析、Data 同步和事件桥。 |
| `SlimeAI.GameOS.GodotBridge.GodotCollisionComponent` | Node | bootstrap | 桥接 Entity 根 `Area2D` 进入 / 离开信号到 `Capabilities.Collision.Events.Entered / Exited`。 |
| `SlimeAI.GameOS.GodotBridge.GodotHurtboxComponent` | Area2D | bootstrap | 桥接 Hurtbox `Area2D` 进入 / 离开信号到 `Capabilities.Collision.Events.HurtboxEntered / HurtboxExited`。 |
| `SlimeAI.GameOS.GodotBridge.GodotContactDamageComponent` | Node | bootstrap | 消费 Hurtbox 事件并通过 `DamageService` 结算接触伤害。 |
| `SlimeAI.GameOS.GodotBridge.GodotAttackComponent` | Node | bootstrap | 注册默认 `AttackService`，把导出攻击参数写入 Runtime Data，把 Godot 节点目标解析为 Runtime 攻击请求，并可选把 Attack 事件转为 Unit 动画请求；配置动画不存在时可从 `attack*` 可用动画中回退选择，可选择保留注册前已有 Attack Data。 |
| `SlimeAI.GameOS.GodotBridge.AttackComponent` | Node | bootstrap | 旧项目 `AttackComponent` 类名 / 场景名兼容包装，默认保留已有 Attack Data。 |
| `SlimeAI.GameOS.GodotBridge.GodotAIBehaviorTreeKind` | enum | bootstrap | Godot AI bridge 内置行为树类型：Melee / AbilityMelee / PatrolOnly / Chaser。 |
| `SlimeAI.GameOS.GodotBridge.GodotAIComponent` | Node | bootstrap | 驱动 `AIService.Tick` 的 Godot AI 组件，写入导出 AI 参数和 Movement AI 意图，不直接移动节点。 |
| `SlimeAI.GameOS.GodotBridge.GodotProjectileEffectSpawner` | Node | bootstrap | 监听 Projectile / Effect Runtime 生成事件，通过 `ScenePath` 加载 `PackedScene`，实例化视觉节点并按 Runtime EntityId 注册到 `GodotNodeRegistry`，并自动播放 Effect `AnimatedSprite2D`。 |
| `SlimeAI.GameOS.GodotBridge.GodotBridgeStats` | record struct | migrated | 桥接注册状态快照。 |
| `SlimeAI.GameOS.GodotBridge.GodotNodePoolConfig` | record struct | migrated | Godot Node 池配置。 |
| `SlimeAI.GameOS.GodotBridge.GodotNodePool<T>` | class | migrated | 带泊车、脱树和碰撞隔离的 Godot Node 对象池。 |
| `SlimeAI.GameOS.GodotBridge.GodotNodePoolManager` | static class | migrated | Godot Node 池查询和归还入口。 |
| `SlimeAI.GameOS.GodotBridge.GodotCollisionIsolation` | static class | migrated | 2D 碰撞隔离工具。 |

## Validation

| API | 类型 | 状态 | 说明 |
| --- | --- | --- | --- |
| `Tools/run-build.sh` | script | active | 构建 `SlimeAI.slnx`。 |
| `Tools/run-dataos-validate.sh` | script | active | 建立临时 SQLite DB，应用 DataOS migration 并运行 schema 验证。 |
| `Tools/run-tests.sh` | script | active | 运行 DataOS schema 验证和 `SlimeAI.GameOS.Tests` 行为测试。 |
| `DataOS/Generators/generate-runtime-snapshot.sh` | script | active | 从 SQLite authoring DB 生成 Runtime JSON snapshot。 |
| `DataOS/Validation/validate-dataos.sh` | script | active | 校验 DataOS authoring DB 的外键、空键、bool、资源分类和路径。 |
| `DocsAI/GameOS/Observation.md` | document | draft | 定义 Schedule、EventBus、Lifecycle Tree 与 Typed References、Capability selector、DataOS trace、Command playback 和 BrotatoLike scene PASS/FAIL artifact contract。 |
| `Tests/SlimeAI.GameOS.Tests` | console project | active | 覆盖 Event/Data/DataOS snapshot/Entity/Relationship/Schedule/Pool/Timer/Resource/Movement/MovementCollision、DamageService、HealService、DamageTool、AbilityService 点选目标语义、Ability 自动索敌、ProjectileTool 生成与命中生命周期、EffectTool 动画名写入、FeatureService、AttackService、AI 行为树、AI 巡逻、AI 行为树预制块、AI 攻击请求、AI 自动索敌施法上下文准备、Damage 处理器管线、目标查询注入和 OrientationParams 最小行为。 |
| `Games/BrotatoLike/Src/Game/BrotatoLikeDataOSBootstrap.cs` | class | active | 游戏侧正式 DataOS snapshot 入口，可读取 `res://DataOS/Snapshots/runtime_snapshot.json`、注册资源并按记录生成 Runtime Entity。 |
| `Games/BrotatoLike/Src/Game/Main.cs` | Godot scene script | active | 当前 headless smoke 覆盖 Movement、MovementCollision、Damage / ContactDamage / Attack、旧 AttackComponent 兼容包装、Attack 动画播放、Godot AI bridge、Ability 点选目标、Ability 自动索敌、Projectile / Effect Runtime 与 Godot 实例化、DataOS bootstrap 生成 Runtime Entity、Godot Physics broadphase、GodotMovementDriver、GodotOrientationComponent、GodotBridge 和 GodotNodePool 接入。 |
| `Games/BrotatoLike/Tools/run-godot-smoke.sh` | script | active | 使用 Godot 4.6.2 mono CLI 运行 BrotatoLike Runtime / Movement / MovementCollision / Damage / ContactDamage / Attack / Attack Animation lifecycle / Godot AI bridge / Ability 点选目标 / Ability 自动索敌 / Projectile / Effect Runtime 与 Godot 实例化 / Effect 动画播放 / Projectile 命中生命周期 / 穿透 / MaxLifeTime 销毁 / Godot Physics broadphase / Godot Orientation / GodotBridge smoke。 |

Movement 已完成旧 `MoveMode` 纯 C# Capability 策略迁入、Godot 2D 位移桥、运行时圆形扫描版运动碰撞、同帧多命中派发、Godot Physics broadphase 查询和 Godot 朝向输出第一批。Collision 已完成层常量、DataKey、过滤策略、运行时事件和 Godot `Area2D` / Hurtbox bridge 第一批。Damage 已完成 DamageService / HealService / DamageTool / DamageDataKeys / ContactDamage bridge / 处理器管线第一批。Ability 已完成最小 Runtime 切片、点选目标语义、自动索敌第一段、Periodic 自动触发 Tick 并可选调用 Feature handler；Projectile / Effect 已完成纯 Runtime 生成入口和 Godot 实例化第一段，Projectile 已完成命中生命周期、穿透多目标和 MaxLifeTime 停止销毁，Effect 已完成动画播放第一段；Feature 已完成最小生命周期；AI 已完成最小纯 Runtime 行为树、最近目标查询、巡逻、行为树预制块、Ability 自动索敌上下文准备、Godot AI bridge 和攻击请求事件；Attack 已完成最小 Runtime 结算、GodotAttackComponent bridge、旧 AttackComponent 兼容包装、Attack 动画事件桥第二段和旧 Attack 动画选择兼容第一段。DataOS 已建立 SQLite schema / migration / generator / validator / Runtime snapshot loader 最小闭环。
