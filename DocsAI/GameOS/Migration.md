# SlimeAI.GameOS Migration

## 输入仓库

```text
/home/slime/Code/Godot/Games/MyGames/brotato-my
```

## 当前迁移批次

M17：DataOS 正式适配最小闭环。

## 已迁入

- `GameOS/SlimeAI.GameOS.csproj`
- `GameOS/Runtime/GameOSInfo.cs`
- 包契约和 API 文档占位。
- Runtime Data 最小内核：
  - `Data`
  - `DataMeta`
  - `DataRegistry`
  - `DataModifier`
  - `DataKeyAttribute`
  - `IDataChangeSink`
  - `DataChangedEventData`
  - `RuntimeDataSnapshot`
  - `RuntimeDataRecord`
  - `RuntimeDataField`
  - `RuntimeResourceEntry`
- Runtime Event 最小内核：
  - `IEvent` / `IEntityEvent` / `IGlobalEvent` / `IBroadcastEvent` marker interface
  - `IEventBus`
  - `EntityEventBus`
  - `WorldEventBus` / `WorldEvents.World`（替换旧 `GlobalEventBus`）
  - `EventBusObservation`
  - `EventDataChangeSink`
  - Runtime 事件按目录组织：`Runtime/Events/Core/`（Entity 生命周期、Data 变化、Lifecycle Tree），`Runtime/Events/Global/` 当前无 framework-owned payload；Capability 事件在 `Capabilities/<Cap>/Events/`
- Runtime Entity 最小内核：
  - `EntityId`（P2a typed value）
  - `IEntity`（typed `EntityId`）
  - `RuntimeEntity`
  - `EntitySpawnConfig`（typed `EntityId / ParentEntityId / ParentDestroyPolicy`）
  - `EntityManager`（typed `Spawn / Get / Destroy / AttachLifecycleParent`，内部 `Dictionary<EntityId, IEntity>`）
  - `EntityIdList`（typed entity-id 多引用不可变值）
  - `OwnedReferenceDescriptor`、`IOwnedReferenceCleaner`、`RuntimeOwnedReferenceRegistry`
- RuntimeWorld facade：
  - `RuntimeWorld.Default`（进程级默认 world）
  - `RuntimeWorld.CreateScoped()`（测试 / 沙箱隔离 world）
  - `IEntityRegistry / ILifecycleTree / IWorldEventBus / IResourceCatalog / IObjectPoolManager / IRuntimeSchedule / IRuntimeCommandBuffer`（internal-only subsystem 句柄）
  - `EntityManager / LifecycleTree / WorldEvents.World / ResourceCatalog / ObjectPoolManager` static facade 转发到 `RuntimeWorld.Default`
- Runtime CommandBuffer + Phase：
  - `SchedulePhase`
  - `DeferredCommandKind / DeferredCommandStatus / DeferredCommandFailureReason`
  - 8 个 typed payload record：`Spawn / Destroy / Attach / Detach / QueuedEvent / ResourceRequest / GodotNodeInstantiate / GodotNodeFree`
  - `DeferredRuntimeCommand`
  - `CommandPlaybackReport / CommandPlaybackEntry`
  - `IStructuralChangeGuard`
  - `RuntimeCommandBuffer`
  - `RuntimeWorld.Commands / RuntimeWorld.Schedule`
- Runtime LifecycleTree 最小内核：
  - `LifecycleTree`（单父树 facade，typed API）
  - `LifecycleLink`（`(ParentEntityId, ChildEntityId, DestroyPolicy, Priority)`）
  - `ParentDestroyPolicy`（`DestroyRecursively / Detach`，迁入 `Runtime.Entity` namespace）
- Runtime Schedule 最小内核：
  - `RuntimeSchedule`
  - `RuntimeSchedule.RunPhase(SchedulePhase)`
  - `ProjectStateService`
  - `ProjectStateSnapshot`
  - `ProjectStateChangedEventArgs`
  - `SystemRunCondition`
  - `SystemDescriptor`
  - `SystemConfig`
  - `IRuntimeSystem`
  - `IRuntimeCommandHandler<TRequest,TResult>`
  - `SystemExecuteResult<TResult>`
  - `SystemRuntimeInfo`
  - `GameFlowState / OverlayFlags / SimulationState / SystemGroup / SystemTag`
- Runtime Resource 最小内核：
  - `ResourceCategory`
  - `ResourceData`
  - `ResourceCatalog`
  - `ResourceManagement`
- Runtime Pool 最小内核：
  - `IPoolable`
  - `ObjectPoolConfig`
  - `PoolStats`
  - `ObjectPool<T>`
  - `ObjectPoolManager`
- Runtime Timer 最小内核：
  - `GameTimer`
  - `TimerManager`
- Damage Capability：
  - `DamageDataKeys`
  - `DamageType`
  - `DamageTags`
  - `DamageInfo`
  - `DamageResult`
  - `IDamageProcessor`
  - `DamageService`
  - 默认处理器：Base / Dodge / Critical / Shield / Armor / DamageTakenAmplification / HealthExecution / Lifesteal / Statistics
  - `HealService`
  - `DamageTool`
- Ability Capability：
  - `AbilityDataKeys`
  - `AbilityType`
  - `AbilityTriggerMode`
  - `AbilityTargetSelection`
  - `AbilityTriggerResult`
  - `AbilityCastContext`
  - `AbilityExecutedResult`
  - `AbilityService`
  - 事件目录：`Capabilities/Ability/Events/`（`Activated / Executed / Failed`）
- Feature Capability：
  - `FeatureDataKeys`
  - `FeatureDefinition`
  - `FeatureModifierEntry`
  - `FeatureContext`
  - `FeatureEndReason`
  - `IFeatureHandler`
  - `FeatureHandlerRegistry`
  - `FeatureService`
  - 事件目录：`Capabilities/Feature/Events/`（`Granted / Removed / Enabled / Disabled / Activated / Executed / Ended`）
- Ability / Feature 接入：
  - `AbilityDataKeys.FeatureHandlerId`
  - `AbilityService` 可选调用 `FeatureService.Activate / End`
- Ability 自动触发最小 Runtime：
  - `AbilityService.TickAutoTriggers(...)`
  - 支持 `AbilityTriggerMode.Periodic`
  - 外部系统负责准备 `AbilityCastContext` 和目标列表
- Ability 点选目标语义：
  - `AbilityCastContext.TargetPosition`
  - `AbilityTargetSelection.Point`
  - `AbilityTargetSelection.EntityOrPoint`
- Projectile Capability：
  - `ProjectileDataKeys`
  - `ProjectileSpawnOptions`
  - `ProjectileSpawnResult`
  - `ProjectileMovementOptions`
  - `ProjectileTool`
  - 事件目录：`Capabilities/Projectile/Events/`（`Spawned / Hit`）
- Effect Capability：
  - `EffectDataKeys`
  - `EffectSpawnOptions`
  - `EffectSpawnResult`
  - `EffectTool`
  - 事件目录：`Capabilities/Effect/Events/`（`Spawned`）
- AI Capability：
  - `AIDataKeys`
  - `AIContext`
  - `AIState`
  - `AIService`
  - `BehaviorNode`
  - `SequenceNode`
  - `SelectorNode`
  - `FindNearestTargetAction`
  - `MoveToTargetAction`
  - `IsTargetInRangeCondition`
  - `RequestAttackAction`
  - `TickAbilityAutoTriggersAction`
  - `PatrolAction`
  - `EnemyBehaviorBlocks`
  - `EnemyBehaviorTreeBuilder`
- Attack Runtime 事件：
  - 事件目录：`Capabilities/Attack/Events/`（`Requested / Started / Finished / Cancelled / CancelRequested`）
  - `AttackCancelReason`
- Attack Capability：
  - `AttackDataKeys`
  - `AttackState`
  - `AttackTriggerResult`
  - `AttackTriggerReport`
  - `AttackService`
- Attack GodotBridge：
  - `GodotAttackComponent`
  - `AttackComponent`
- Unit Animation GodotBridge：
  - `UnitDataKeys`
  - `GodotUnitAnimationComponent`
- AI GodotBridge：
  - `GodotAIBehaviorTreeKind`
  - `GodotAIComponent`
- Projectile / Effect GodotBridge：
  - `GodotProjectileEffectSpawner`
- GodotBridge 第一版：
  - `GodotEntity`
  - `IGodotComponent`
  - `GameOSGodotBridge`
  - `GodotNodeRegistry`
  - `GameOSTimerDriver`
  - `GodotBridgeStats`
- GodotBridge 扩展第一段：
  - `GodotNodePool<T>`
  - `GodotNodePoolConfig`
  - `GodotNodePoolManager`
  - `GodotCollisionIsolation`
- Runtime Pool 全局注册扩展：
  - `IObjectPool`
- DataOS 最小闭环：
  - `DataOS/Schema/core.sql`
  - `DataOS/Migrations/001_initial.sql`
  - `DataOS/Generators/generate-runtime-snapshot.sh`
  - `DataOS/Validation/validate-dataos.sh`
  - `Tools/run-dataos-validate.sh`
- 验证入口：
  - `Tests/SlimeAI.GameOS.Tests`
  - `Tools/run-tests.sh`

## 下一批

Runtime 最小内核、GodotBridge 第一版、Node 池化 / 碰撞隔离第一段、Movement、Collision、Damage 处理器管线、DamageTool、Heal、Shield、Ability Runtime 最小切片、Feature Runtime 最小生命周期、Ability 自动触发最小 Runtime、Ability 点选目标语义、Ability 自动索敌第一段、Projectile / Effect Runtime 生成第一段、Projectile / Effect Godot 实例化第一段、Projectile 命中生命周期第一段、Projectile 穿透 / 生命周期扩展、Effect 动画播放第一段、AI Runtime 最小行为树、AI 攻击请求事件、AI 巡逻最小 Runtime、AI 行为树预制块、AI Ability 自动索敌上下文准备、Godot AIComponent bridge 第一段、Attack Runtime 最小结算、GodotAttackComponent bridge、Attack 动画事件桥第二段、旧 Attack 动画选择兼容第一段、旧 AttackComponent 场景兼容第一段和 DataOS schema / migration / generator / validator / Runtime snapshot loader 已进入可验证边界。下一批迁移重点：

1. DataOS：扩大旧 DataNew 表迁移范围，并把 snapshot 驱动接入正式游戏生成入口。
2. GodotBridge headless smoke 接入统一场景测试 runner。
3. Godot 引擎 trace：以 `/home/slime/Code/SlimeAI/Resources/Engine/godot-4.6.2-stable` 为源码入口。

每批迁移都必须更新 Contracts、ApiIndex 和验证命令。

## P2a：EntityId typed boundary 完成

- 范围：`string EntityId` 升级为 `readonly record struct EntityId(string Value)`，禁止 implicit `string` 转换；`EntityId.Empty` 是唯一"无引用"值，`default(EntityId) == Empty == From(null) == From("")`。
- 受影响公开 API：`IEntity.EntityId`、`RuntimeEntity` 构造、`EntitySpawnConfig.EntityId / ParentEntityId`、`EntityManager.Spawn / Register / Destroy / Get / GetAll / Clear / AttachLifecycleParent`、`Runtime.Events.Core.LifecycleChildAttached / LifecycleChildDetached` payload、`GameOSGodotBridge.GetEntityNode`。
- 受影响 Capability DataKey：`Projectile.SourceEntity / AbilityEntity / TargetEntity`、`Effect.SourceEntity / AbilityEntity / TargetEntity`、`AI.TargetEntity` 全部由 `DataKey<IEntity?>` 改为 `DataKey<EntityId?>`；`MovementParams.TargetEntityId / MovementCollisionParams.SpecificTargetEntityId` 改为 `EntityId?`。
- 后续 P1（refactor-runtime-relationship-as-lifecycle-tree）已把旧 `RelationshipManager / RelationshipType / RelationshipRecord / RelationshipConstraint / RelationshipLifecycle` 替换为 typed `LifecycleTree / LifecycleLink / ParentDestroyPolicy`，并新增 `EntityIdList / OwnedReferenceDescriptor / RuntimeOwnedReferenceRegistry`；业务多引用走 typed `DataKey<EntityIdList>`。
- 验证：`Tools/run-build.sh` PASS；`Tools/run-tests.sh` 83/83 PASS（含 4 个新增 `EntityIdTests`）；`Tools/run-dataos-validate.sh` PASS。

## P2b：RuntimeWorld facade + dispose invariant 完成

- 范围：新增 `RuntimeWorld.Default` 与 `RuntimeWorld.CreateScoped()`，把 Entity / Lifecycle / Events / Resources / Pools 状态收束到 world-scoped subsystem；static facade 保留并转发到 `Default`。
- Dispose invariant：本轮实现 `Pools -> Resources -> Lifecycle -> Entities -> Events`；P4 扩展 Schedule / Commands 时必须使用完整 `Schedule -> Commands -> Pools -> Resources -> Lifecycle -> Entities -> Events` 顺序。
- 新增测试：`Tests/SlimeAI.GameOS.Tests/World/RuntimeWorldTests.cs` 覆盖 Default singleton、Default 不可 dispose、scoped subsystem 隔离、dispose 顺序、dispose teardown event 和 dispose 后 getter 抛 `ObjectDisposedException`。
- 已迁到 `RuntimeWorld.CreateScoped()` 的 Program 用例：`TestEntityLifecycle`、`TestLifecycleTree`、`TestRuntimeOwnedReferenceRegistry`、`TestEntityParentDestroyPolicy`、`TestResourceCatalog`、`TestCollisionLayerMaskEvent`、`TestDamageServiceAppliesHealth`、`TestDamageServiceKilledEvent`、`TestDamagePipelineDodge`、`TestDamagePipelineCriticalArmorAndStats`、`TestDamagePipelineTrueDamageBypassesDodgeAndArmor`、`TestDamagePipelineLifesteal`、`TestDamagePipelineShield`、`TestHealServiceAppliesAndClamps`、`TestDamageToolMultiTargetAndPeriodic`、`TestAbilityServiceInstantDamageCooldownAndCharge`、`TestAbilityServiceRequiresTarget`、`TestAbilityServiceAcceptsPointTarget`、`TestAbilityServiceAcceptsEntityOrPointTarget`、`TestAbilityServicePeriodicDamage`、`TestAbilityServicePeriodicAutoTrigger`、`TestFeatureServiceGrantsModifiersAndRemoves`、`TestFeatureServiceLifecycleHandler`、`TestAbilityServiceInvokesFeatureHandler`、`TestAttackServiceConsumesRequestAndDamages`、`TestAttackServiceGatesRangeAndCooldown`、`TestAttackServiceWindupAndRecoveryTimers`。
- 剩余 backlog：依赖静态全局扫描或 static Capability 工具的 Projectile / AI / Movement / DataSnapshot 资源注册测试仍保留 `EntityManager.Clear()` / `WorldEvents.World.Clear()` 序列，后续在 Capability 支持显式 `RuntimeWorld` 注入后继续迁移。
- 验证：`Tools/run-build.sh` PASS；`Tools/run-tests.sh` PASS（新增 RuntimeWorld tests 后 92/92 PASS）。

## P3：Runtime events game leakage cleanup 完成

- 范围：按三桶证据清理 framework Runtime/Events 中 BrotatoLike 玩法泄漏命名。
- Bucket A 死代码已删除且不迁移：`MouseSelectionCompleted / MouseSelectionMissed / MouseSelectionPreviewUpdated`（含 `MouseSelectionInteractionKind / MouseSelectionHitKind`）、`WaveStarted / WaveCompleted`、`GameStart / GameOver / GamePause / GameResume`。
- Bucket B 真实调用链已迁到游戏侧：`InputUseSkill / InputPreviousSkill / InputNextSkill` 位于 `Games/BrotatoLike/Src/Game/Event/BrotatoLikeInputEvents.cs`，`GodotPlayerInputComponent` 已迁为 `Games/BrotatoLike/Src/Game/Bridge/BrotatoLikePlayerInputComponent.cs`。
- Bucket C 本轮未发现；`SlimeAI/GameOS/GodotBridge/` 删除输入组件后无 BrotatoLike skill / wave hardcode 命中。
- 边界：框架仓禁止 `using BrotatoLike` 或 `BrotatoLike.Game.*` 反向依赖；游戏专属事件默认放入 `Games/<Game>/Src/Game/Event/`。

## OpenSpec 未完成项收敛（2026-05-19）

- `gameos-feature-actions-and-auto-trigger`：Feature Capability 补齐 `IFeatureAction`、`FeatureDefinition.Actions`、`FeatureService.ExecuteActions` 和 `FeatureAutoTriggerService`；新增 `Feature.TriggerMode / Cooldown / TriggerEventType / TriggerChance` DataKey authoring mirror；BrotatoLike seed 和 generated snapshot 已包含 Feature trigger 字段。OnEvent 第一版由调用方显式注册泛型事件，`TriggerEventType` 保留为 authoring stable key。
- `refactor-feature-context-typed-execution`：Feature execution 新增 `IFeatureActivationPayload` / `IFeatureExecutionResult` typed contract；`FeatureContext.ActivationPayload / ExecutionResult / SourceEventPayload` 和 `TryGetActivation<T> / TryGetExecutionResult<T>` 是新首选入口；旧 `ActivationData / ExecuteResult / SourceEventData / ExtraData` 标记 `[Obsolete]`，仅作为迁移兼容。
- `refactor-capability-services-scoped-dependencies`：`AbilityService` 可注入 `TimerManager / FeatureService / DamageService`；`DamageTool`、`ProjectileTool` 和 GodotBridge adapters 的 process default 只作为兼容边界，测试使用独立实例。
- `refactor-godotbridge-adapter-runtime-context`：新增 `GodotBridgeContext` 和 `GodotBridgeNodeRegistry`，static `GameOSGodotBridge` / `GodotNodeRegistry` 转发默认 context；adapter callback guard 使用目标 context 的 `RuntimeWorld.Commands`。
- `purge-game-specific-capability-leakage`：Damage `Wave*` 统计按 Bucket C 改为 neutral `Encounter*` DataKeys；`SineWave` 是 Movement 轨迹 false positive；BrotatoLike handler 名称仅作为游戏侧验证证据，不作为框架默认契约。
- `rename-legacy-component-system-apis`：本轮建立 migration table，保留 `IGodotComponent`、`Godot*Component`、`AttackComponent`、`MovementSystem`、`CollisionSystem`、`IRuntimeSystem` 等 public compatibility symbols；新文档和 skill 改用 GodotBridge Adapter、Runtime Process、Runtime Data / DataKey 术语。
- `gameos-capability-owned-selectors`：AI/Ability target selection 收敛到 `IAITargetQuery` / `IAbilityTargetQuery`；Movement tick 遍历自身 active movement set；`Runtime*TargetQuery` 和 `MovementCollisionRuntimeTargetQuery` 是显式 runtime fallback。

## P4：Runtime CommandBuffer + Phase 完成

- 范围：新增 `Runtime/CommandBuffer/` typed deferred queue、`SchedulePhase`、`IStructuralChangeGuard`、`RuntimeWorld.Commands / Schedule`，并把 scoped world dispose 顺序补齐为 `Schedule -> Commands -> Pools -> Resources -> Lifecycle -> Entities -> Events`。
- Guard 接入点：world event handler dispatch、lifecycle attached/detached publish、GodotBridge component registered/unregistered callback。`GameOSGodotBridge` 保持 static class，不新增 `_Process / _EnterTree / _ExitTree` override。
- Deferred 语义：guard 内 `Spawn` 返回 reserved `RuntimeEntity`，同一 guard 内 registry `Get` 返回 `null`，但 `entity.Data.Set` 保留到 playback；`Destroy / Attach / Detach` 自动入队。
- Phase playback：`RuntimeSchedule.RunPhase(SchedulePhase)` 只播放 `RuntimeCommandBuffer`，不 tick capability service，不引入 system graph / query DSL。
- QueuedEvent 限制：只支持 framework-known `IGlobalEvent` record；不支持 game-specific dynamic event replay。
- Godot node command：`GodotNodeInstantiate / GodotNodeFree` 通过可注入 bridge handler；默认未注入时报告 `BridgeTargetUnavailable`。
- 明确未做：`IRuntimeSystem -> IRuntimeProcess` rename；`SystemConfig + SystemDescriptor + SystemRuntimeInfo` 合并。这两项没有被本变更需要，作为 future backlog 保留。
- 验证：`Tools/run-build.sh` PASS；`Tools/run-tests.sh` PASS（新增 CommandBuffer / SchedulePhase / StructuralChangeGuard tests）。BrotatoLike build 与 Godot smoke 见 ProjectState 最新验证。
