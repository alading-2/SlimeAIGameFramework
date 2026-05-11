# SkilmeAI.GameOS Migration

## 输入仓库

```text
/home/slime/Code/Godot/Games/MyGames/brotato-my
```

## 当前迁移批次

M17：DataOS 正式适配最小闭环。

## 已迁入

- `GameOS/SkilmeAI.GameOS.csproj`
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
  - Runtime 事件按目录组织：`Runtime/Events/Core/`（Entity 生命周期、Data 变化、Relationship、Input）、`Runtime/Events/Global/`（Wave、GameState、MouseSelection），Capability 事件在 `Capabilities/<Cap>/Events/`
- Runtime Entity 最小内核：
  - `IEntity`
  - `RuntimeEntity`
  - `EntitySpawnConfig`
  - `EntityManager`
- Runtime Relationship 最小内核：
  - `RelationshipManager`
  - `RelationshipType`
  - `RelationshipRecord`
  - `RelationshipConstraint`
  - `RelationshipLifecycle`
  - `ParentDestroyPolicy`
- Runtime Schedule 最小内核：
  - `RuntimeSchedule`
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
  - `Tests/SkilmeAI.GameOS.Tests`
  - `Tools/run-tests.sh`

## 下一批

Runtime 最小内核、GodotBridge 第一版、Node 池化 / 碰撞隔离第一段、Movement、Collision、Damage 处理器管线、DamageTool、Heal、Shield、Ability Runtime 最小切片、Feature Runtime 最小生命周期、Ability 自动触发最小 Runtime、Ability 点选目标语义、Ability 自动索敌第一段、Projectile / Effect Runtime 生成第一段、Projectile / Effect Godot 实例化第一段、Projectile 命中生命周期第一段、Projectile 穿透 / 生命周期扩展、Effect 动画播放第一段、AI Runtime 最小行为树、AI 攻击请求事件、AI 巡逻最小 Runtime、AI 行为树预制块、AI Ability 自动索敌上下文准备、Godot AIComponent bridge 第一段、Attack Runtime 最小结算、GodotAttackComponent bridge、Attack 动画事件桥第二段、旧 Attack 动画选择兼容第一段、旧 AttackComponent 场景兼容第一段和 DataOS schema / migration / generator / validator / Runtime snapshot loader 已进入可验证边界。下一批迁移重点：

1. DataOS：扩大旧 DataNew 表迁移范围，并把 snapshot 驱动接入正式游戏生成入口。
2. GodotBridge headless smoke 接入统一场景测试 runner。
3. Godot 引擎 trace：以 `/home/slime/Code/SkilmeAI/Engine/godot-4.6.2-stable` 为源码入口。

每批迁移都必须更新 Contracts、ApiIndex 和验证命令。
