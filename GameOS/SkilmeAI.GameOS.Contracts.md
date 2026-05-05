# SkilmeAI.GameOS Contracts

> Version: 0.1.0-alpha.0
> Status: runtime-minimum

## 当前契约

当前包已建立构建边界、发布身份，并迁入 Runtime 最小内核：Data / Event / Entity / Relationship / Schedule / Resource / Pool / Timer，GodotBridge 第一版和 Node 池化 / 碰撞隔离扩展第一段，以及 Movement / Collision / Damage / Ability / Feature / AI / Attack / Projectile / Effect Capability 第一批。

已存在的稳定入口：

- `SkilmeAI.GameOS.Runtime.GameOSInfo.PackageId`
- `SkilmeAI.GameOS.Runtime.GameOSInfo.Version`
- `SkilmeAI.GameOS.Runtime.GameOSInfo.Stage`
- `SkilmeAI.GameOS.Runtime.Event.EventBus`
- `SkilmeAI.GameOS.Runtime.Event.EventContext`
- `SkilmeAI.GameOS.Runtime.Event.GlobalEventBus`
- `SkilmeAI.GameOS.Runtime.Event.GameEventType`
- `SkilmeAI.GameOS.Runtime.Data.Data`
- `SkilmeAI.GameOS.Runtime.Data.DataMeta`
- `SkilmeAI.GameOS.Runtime.Data.DataRegistry`
- `SkilmeAI.GameOS.Runtime.Data.DataModifier`
- `SkilmeAI.GameOS.Runtime.Data.IDataChangeSink`
- `SkilmeAI.GameOS.Runtime.Data.RuntimeDataSnapshot`
- `SkilmeAI.GameOS.Runtime.Event.EventDataChangeSink`
- `SkilmeAI.GameOS.Runtime.Entity.IEntity`
- `SkilmeAI.GameOS.Runtime.Entity.RuntimeEntity`
- `SkilmeAI.GameOS.Runtime.Entity.EntityManager`
- `SkilmeAI.GameOS.Runtime.Relationship.RelationshipManager`
- `SkilmeAI.GameOS.Runtime.Relationship.RelationshipType`
- `SkilmeAI.GameOS.Runtime.Relationship.RelationshipLifecycle`
- `SkilmeAI.GameOS.Runtime.Schedule.RuntimeSchedule`
- `SkilmeAI.GameOS.Runtime.Schedule.ProjectStateService`
- `SkilmeAI.GameOS.Runtime.Schedule.SystemRunCondition`
- `SkilmeAI.GameOS.Runtime.Resource.ResourceCatalog`
- `SkilmeAI.GameOS.Runtime.Resource.ResourceManagement`
- `SkilmeAI.GameOS.Runtime.Pool.ObjectPool<T>`
- `SkilmeAI.GameOS.Runtime.Pool.ObjectPoolManager`
- `SkilmeAI.GameOS.Runtime.Pool.IObjectPool`
- `SkilmeAI.GameOS.Runtime.Timer.TimerManager`
- `SkilmeAI.GameOS.Runtime.Timer.GameTimer`
- `SkilmeAI.GameOS.Capabilities.Movement.Vector2Value`
- `SkilmeAI.GameOS.Capabilities.Movement.MovementDataKeys`
- `SkilmeAI.GameOS.Capabilities.Movement.MoveMode`
- `SkilmeAI.GameOS.Capabilities.Movement.MovementParams`
- `SkilmeAI.GameOS.Capabilities.Movement.IMovementStrategy`
- `SkilmeAI.GameOS.Capabilities.Movement.MovementStrategyRegistry`
- `SkilmeAI.GameOS.Capabilities.Movement.MovementSystem`
- `SkilmeAI.GameOS.Capabilities.Movement.MovementStopReason`
- `SkilmeAI.GameOS.Capabilities.Movement.MovementStopContext`
- `SkilmeAI.GameOS.GodotBridge.GodotEntity`
- `SkilmeAI.GameOS.GodotBridge.GodotEntity2D`
- `SkilmeAI.GameOS.GodotBridge.IGodotComponent`
- `SkilmeAI.GameOS.GodotBridge.GameOSGodotBridge`
- `SkilmeAI.GameOS.GodotBridge.GodotNodeRegistry`
- `SkilmeAI.GameOS.GodotBridge.GameOSTimerDriver`
- `SkilmeAI.GameOS.GodotBridge.GodotMovementDriver`
- `SkilmeAI.GameOS.GodotBridge.GodotNodePool<T>`
- `SkilmeAI.GameOS.GodotBridge.GodotNodePoolConfig`
- `SkilmeAI.GameOS.GodotBridge.GodotNodePoolManager`
- `SkilmeAI.GameOS.GodotBridge.GodotCollisionIsolation`
- `SkilmeAI.GameOS.GodotBridge.GodotAttackComponent`
- `SkilmeAI.GameOS.GodotBridge.AttackComponent`
- `SkilmeAI.GameOS.GodotBridge.GodotUnitAnimationComponent`
- `SkilmeAI.GameOS.GodotBridge.GodotAIComponent`
- `SkilmeAI.GameOS.GodotBridge.GodotProjectileEffectSpawner`
- `SkilmeAI.GameOS.Capabilities.Damage.DamageService`
- `SkilmeAI.GameOS.Capabilities.Damage.IDamageProcessor`
- `SkilmeAI.GameOS.Capabilities.Damage.DamageDataKeys`
- `SkilmeAI.GameOS.Capabilities.Damage.DamageInfo`
- `SkilmeAI.GameOS.Capabilities.Damage.DamageResult`
- `SkilmeAI.GameOS.Capabilities.Damage.HealService`
- `SkilmeAI.GameOS.Capabilities.Damage.DamageTool`
- `SkilmeAI.GameOS.Capabilities.Ability.AbilityService`
- `SkilmeAI.GameOS.Capabilities.Ability.AbilityDataKeys`
- `SkilmeAI.GameOS.Capabilities.Feature.FeatureService`
- `SkilmeAI.GameOS.Capabilities.Feature.FeatureDataKeys`
- `SkilmeAI.GameOS.Capabilities.Feature.FeatureDefinition`
- `SkilmeAI.GameOS.Capabilities.Feature.IFeatureHandler`
- `SkilmeAI.GameOS.Capabilities.Feature.FeatureHandlerRegistry`
- `SkilmeAI.GameOS.Capabilities.AI.AIService`
- `SkilmeAI.GameOS.Capabilities.AI.AIDataKeys`
- `SkilmeAI.GameOS.Capabilities.AI.BehaviorNode`
- `SkilmeAI.GameOS.Capabilities.AI.IsTargetInRangeCondition`
- `SkilmeAI.GameOS.Capabilities.AI.RequestAttackAction`
- `SkilmeAI.GameOS.Capabilities.AI.PatrolAction`
- `SkilmeAI.GameOS.Capabilities.AI.EnemyBehaviorBlocks`
- `SkilmeAI.GameOS.Capabilities.AI.EnemyBehaviorTreeBuilder`
- `SkilmeAI.GameOS.Capabilities.Attack.AttackDataKeys`
- `SkilmeAI.GameOS.Capabilities.Attack.AttackService`
- `SkilmeAI.GameOS.Capabilities.Attack.AttackState`
- `SkilmeAI.GameOS.Capabilities.Attack.AttackCancelReason`
- `SkilmeAI.GameOS.Capabilities.Projectile.ProjectileDataKeys`
- `SkilmeAI.GameOS.Capabilities.Projectile.ProjectileTool`
- `SkilmeAI.GameOS.Capabilities.Projectile.ProjectileMovementOptions`
- `SkilmeAI.GameOS.Capabilities.Effect.EffectDataKeys`
- `SkilmeAI.GameOS.Capabilities.Effect.EffectTool`
- `SkilmeAI.GameOS.Capabilities.Unit.UnitDataKeys`

## Runtime Data 契约

- `Data` 是运行时动态状态容器，支持基础值、计算键、数值修改器、分类重置和 `LoadFromConfig`。
- `DataMeta` 描述运行时键的类型、默认值、数值边界、选项、分类、迁移许可和计算函数。
- `DataRegistry` 只注册运行时元数据，不承担 Authoring 数据表职责。
- `IDataChangeSink` 是 Data 到外部通知的解耦接口。
- `EventDataChangeSink` 将 `DataChangedEventData` 转成 `GameEventType.Data.PropertyChanged` 并发布到目标 `EventBus`。
- `RuntimeEntity` 默认创建局部 `EventBus`，并让 `Data` 通过 `EventDataChangeSink` 接入 `Entity.Events`。
- `DataKeyAttribute` 只负责 authoring config property 到 runtime key 的映射。
- `RuntimeDataSnapshot` 只消费 DataOS 生成的 JSON snapshot，不在运行时热路径访问 SQLite；它可以按 `table + id/name` 找记录、把字段写入 `Data`，并把资源映射注册到 `ResourceCatalog`。

## Runtime Event 契约

- `EventBus` 支持 typed / parameterless handler、优先级、Once、Off、同事件重入保护和 `EventContext.StopPropagation()`。
- `GlobalEventBus.Global` 只承载跨实体 / 系统级低频事件，不作为状态容器。
- 内置事件名先覆盖 Data、Entity 生命周期和 Relationship 增删；Capability 事件后续随 Capability 迁移扩展。

## Runtime Entity 契约

- `IEntity` 只暴露 `EntityId`、`Data`、`Events`，不承载业务逻辑。
- `EntityManager` 当前是纯 C# 最小生命周期注册表，支持 `Spawn / Register / Destroy / Get / GetAll / Clear / BindParentRelationships`。
- `EntitySpawnConfig` 支持 `ParentEntityId / AutoAddParentRelation / ParentDestroyPolicy / ParentRelationTypes`，生成阶段可自动绑定归属关系。
- Godot Node Entity 和 Component 生命周期第一版已进入 GodotBridge；对象池、碰撞隔离和复杂组件缓存后续继续迁。

## Runtime Relationship 契约

- `RelationshipManager` 是运行时关系图入口，维护父索引、子索引和类型索引。
- `RelationshipType.Parent` 是归属主链，`ParentDestroyPolicy` 只写入 PARENT 关系。
- 父实体销毁时，`DestroyRecursively` 子实体递归销毁，`Detach` 子实体继续存活并断开 PARENT。
- 关系增删会通过 `GameEventType.Relationship.Added / Removed` 发布低频全局事件。
- 关系查询返回快照集合，避免调用方持有内部索引。

## Runtime Schedule 契约

- `RuntimeSchedule` 是纯 C# 调度器，负责系统描述符、配置、依赖、人工启用状态、运行条件和生命周期。
- `ProjectStateService` 维护 `GameFlowState / OverlayFlags / SimulationState` 三域状态。
- `SystemRunCondition` 把流程、覆盖层、模拟状态门禁前置到调度层。
- `IRuntimeSystem` 是生命周期协议；`IRuntimeCommandHandler<TRequest,TResult>` 是命令入口协议。
- 当前 Schedule 不挂 Godot Node，不接 DataOS 生成配置；GodotBridge / DataOS 后续接入。

## Runtime Resource 契约

- `ResourceCatalog` 保存稳定资源键到 Godot `res://` 路径的映射。
- `ResourceManagement` 是统一加载入口，游戏和 Capability 不直接调用 `GD.Load`。
- DataOS snapshot 可批量注册资源映射；资源扫描仍不在 Runtime 热路径中执行。

## Runtime Pool 契约

- `IObjectPool` 是全局池注册表的非泛型句柄，允许 Runtime 对象池和 Godot Node 池共用 `ObjectPoolManager.ReturnToPool`。
- `ObjectPool<T>` 支持预热、复用、统计、`IPoolable` 生命周期和 `ObjectPoolManager.ReturnToPool`。
- `ObjectPoolConfig.MaxSize = -1` 表示无限制，遵守项目数值语义。
- Runtime `ObjectPool<T>` 保持纯 C#；Godot Node 泊车、脱树和碰撞隔离逻辑进入 GodotBridge。

## Runtime Timer 契约

- `TimerManager` 是纯 C# 计时器调度器，由外部 `Tick(delta, unscaledDelta)` 驱动。
- `GameTimer` 支持 `Delay / Loop / Repeat / Countdown`、Tag、Pause、Cancel、Complete。
- Godot 项目通过 `GameOSTimerDriver._Process` 桥接 `TimerManager.Instance.Tick`。

## Movement Capability 契约

- `MovementSystem` 是纯 C# 调度系统，当前负责 `Start / Stop / Tick`、速度积分、最大时长 / 最大距离停止和 `GameEventType.Movement.Started / Stopped`。
- Movement 内核不依赖 `Godot.Vector2` 或 `Node2D`；位置、速度、朝向统一用 `Vector2Value` 写入 `MovementDataKeys`。
- `MovementDataKeys.Position / Velocity / FacingDirection / IsMoving / MoveSpeed` 是当前稳定 Data 入口；`MoveSpeed` 支持数值 Modifier。
- 当前已迁入 `MoveMode.Charge / Orbit / SineWave / Parabola / CircularArc`。Charge 支持方向移动、目标点移动、速度、`MaxDuration = -1` 不限制、`MaxDistance = -1` 不限制、`ReachDistance` 到点停止；Orbit 支持固定圆心、半径、初始角、角速度、总角度；SineWave 支持基础方向、速度、振幅、频率和相位；Parabola 支持固定终点、总时长和顶点高度；CircularArc 支持固定终点、半径、方向和总时长。
- `IMovementStrategy` 只输出速度和朝向意图，不处理伤害、AI、动画或 Godot 物理体。
- Godot 场景中的 2D 位移桥接第一段已迁入：`GodotEntity2D` 注册时把初始 `Node2D.Position` 写入 Movement Data，`GodotMovementDriver` 推进 `MovementSystem.Tick` 并把 `MovementDataKeys.Position` 同步回 `Node2D.Position`。
- Godot 场景中的 2D 朝向输出第一段已迁入：`GodotOrientationComponent` 消费 `MovementDataKeys.FacingDirection` 和 `MovementParams.Orientation`，写 root `RotationDegrees` 或 `AnimatedSprite2D.FlipH`。

## GodotBridge 契约

- `GodotEntity` 是可挂场景的 `Node + IEntity` 基类，进入 SceneTree 时注册到 `EntityManager` 和 `GodotNodeRegistry`，离开 SceneTree 时注销并清理 Runtime Entity。
- `GodotEntity2D` 是可挂 2D 场景的 `Node2D + IEntity` 基类，额外接入 Movement `Position` 初始同步。
- `IGodotComponent` 是 Godot Component 生命周期协议；`GameOSGodotBridge.RegisterComponents` 会递归扫描子节点，识别实现 `IGodotComponent` 或类型名以 `Component` 结尾的节点。
- Entity-Component 组合关系统一写入 `RelationshipType.EntityToComponent`。
- `GodotNodeRegistry` 只保存运行时注册表，不替代 SceneTree，也不持有业务状态。
- `GameOSTimerDriver._Process` 不分配对象，不使用 LINQ，只把 Godot delta 传给 `TimerManager.Tick`。
- `GodotMovementDriver._Process` 推进 `MovementSystem.Tick`，并同步已注册 `Node2D + IEntity` 的运行时位置；需要固定步长或测试时可关闭 `AutoTick` 并手动调用 `TickMovement(delta)`。
- `GodotNodePool<T>` 管理 Godot Node 池化，支持 `Get(false)` 延迟激活、`Activate`、`Release`、`ReleaseAll`、`Destroy` 和 `PoolStats`。
- `GodotCollisionIsolation` 负责回池时递归隔离 2D 碰撞：`CollisionObject2D` layer/mask 清零并缓存、`Area2D` 关闭 monitoring/monitorable、`CollisionShape2D / CollisionPolygon2D` 禁用、`CharacterBody2D` 清零速度。
- `GodotNodePool<T>` 对 `CollisionObject2D` 根节点默认执行泊车位移动和脱树；重新出池时先挂回 `ActiveParent` 并同步禁用碰撞，再由 `Activate` 恢复处理、可见性和碰撞。
- `GodotContactDamageComponent` 消费 `GameEventType.Collision.HurtboxEntered / HurtboxExited`，只把接触转换为 `DamageService` 请求，不直接修改 HP。
- `GodotUnitAnimationComponent` 消费 `GameEventType.Unit.PlayAnimationRequested / StopAnimationRequested`，驱动 `VisualRoot` 或子节点中的 `AnimatedSprite2D`，缓存 `UnitDataKeys.AvailableAnimations`，并在非循环动画结束后发布 `GameEventType.Unit.AnimationFinished` 回退到 idle。
- `GodotAttackComponent` 是普通攻击 Godot bridge 第一段，注册默认 `AttackService`，把导出攻击参数写入 `AttackDataKeys`，并把 Godot 节点目标解析为 Runtime `IEntity` 后交给 `AttackService.TryRequest`。
- `GodotAttackComponent` 可选把 `Attack.Started / Cancelled` 转成 Unit 动画请求；默认优先使用 `AttackAnimation`，当该动画不在 `UnitDataKeys.AvailableAnimations` 中时，会选择第一个 `attack*` 可用动画作为旧资源兼容回退；`PreferExistingDataOnRegister=true` 时会保留注册前已有 Attack Data。
- `AttackComponent` 是旧项目类名 / 场景名兼容包装，继承 `GodotAttackComponent`，默认启用 `PreferExistingDataOnRegister`，用于旧场景迁移时保留 DataNew / 初始化流程写入的攻击参数。
- `GodotAIComponent` 是 AI Godot bridge 第一段，注册时把导出 AI 参数写入 `AIDataKeys`，按 `GodotAIBehaviorTreeKind` 构建 Runtime 行为树，并通过 `_Process` 或 `TickAI(delta)` 调用 `AIService.Tick`；它只写 AI / Movement 意图，不直接移动 Godot 节点。
- `GodotProjectileEffectSpawner` 监听 `GameEventType.Projectile.Spawned / Effect.Spawned`，从 `ScenePath` 读取 `res://` 路径并通过 `ResourceManagement.LoadPath<PackedScene>` 实例化视觉节点，按 Runtime EntityId 注册到 `GodotNodeRegistry`，并在对应 Runtime Entity 销毁时清理自己生成的视觉节点。

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
- `AbilityDataKeys` 当前覆盖启用、执行中、冷却、剩余冷却、充能、触发模式、目标输入语义、自动索敌参数、伤害参数和可选 `FeatureHandlerId`。
- `TryTrigger` 顺序是：施法者死亡检查、启用检查、执行中检查、冷却检查、充能检查、目标检查、消耗充能、启动冷却、执行伤害或 Feature handler、发布事件。
- `TickAutoTriggers` 只消费外部系统准备好的 `AbilityCastContext`，当前支持 `AbilityTriggerMode.Periodic`，先推进冷却，再对冷却归零的周期技能调用 `TryTrigger`。
- `AbilityCastContext.TargetPosition` 承载输入层完成点选后的目标点；`AbilityTargetSelection.Entity` 要求 `Targets`，`Point` 要求 `TargetPosition`，`EntityOrPoint` 接受二者任一。
- `AbilityTargetingTool` 是当前显式自动索敌辅助入口，会读取 `AbilityDataKeys.AutoTargetRange / AutoTargetMaxTargets / AutoTargetIgnoreSameTeam / AutoTargetRequiresDamageable`，从 Runtime Entity 快照中按距离准备 `AbilityCastContext`；`AutoTargetRange = -1` 表示不限距离，`AutoTargetMaxTargets = -1` 表示不限数量。
- 当前不做隐式统一自动索敌和异步点选会话；输入层、AI 行为节点或具体 handler 仍负责显式准备目标。
- 技能伤害统一走 `DamageTool`，周期伤害使用 `AbilityDataKeys.DamageInterval / DamageRepeatCount / ApplyImmediateDamage`。
- 当 `AbilityDataKeys.FeatureHandlerId` 非空时，`AbilityService` 会把 `AbilityCastContext` 放入 `FeatureContext.ActivationData`，调用 `FeatureService.Activate / End`；若 handler 返回 `AbilityExecutedResult` 则作为施法结果，否则回退到默认 `DamageTool` 伤害逻辑。

## Projectile Capability 契约

- `ProjectileTool.Spawn` 是当前纯 Runtime 投射物生成入口，生成 `RuntimeEntity`，写入 `ProjectileDataKeys` 和 Movement `Position / FacingDirection`，并发布 `GameEventType.Projectile.Spawned`。
- `ProjectileTool.StartMovement` 是当前投射物飞行和命中生命周期入口，使用外部传入的 `MovementSystem` 启动移动，并通过 `MovementCollisionParams.OnCollision` 把命中转换为 `DamageService` 伤害结算和 `GameEventType.Projectile.Hit`。
- Movement 碰撞在一次位移段内会连续派发多个有效目标，支持 Projectile 同帧穿透；同一次移动内已命中的目标会去重，且会显式跳过移动实体自身。
- `ProjectileMovementOptions` 当前覆盖 `Mode / Speed / MaxDuration / MaxDistance / ReachDistance / StopAtTarget / ApplyDamageOnHit / StopAfterHitCount / DestroyOnStop / IgnoreSameTeam / TargetMatchMode / SourceRadiusOverride / TargetRadiusOverride / Damage / DamageType / DamageTags`；`StopAfterHitCount` 默认读取 `ProjectileDataKeys.MaxHitCount`，`StopAfterHitCount = -1` 表示只命中通知和伤害但不停止；`MaxDuration` 默认读取 `ProjectileDataKeys.MaxLifeTime`，`-1` 表示不限制。
- `DestroyOnStop` 对 Projectile 的碰撞停止、到时、到距离和完成停止都生效；默认开启，用于停止后销毁 Runtime 投射物并触发 GodotBridge 清理视觉节点。
- `ProjectileDataKeys` 当前覆盖 `ScenePath / SourceEntity / AbilityEntity / TargetEntity / SpawnPosition / TargetPosition / Direction / Speed / MaxHitCount / HitCount / MaxLifeTime / Damage / DamageType / DamageTags`。
- `ScenePath` 只保存 `res://` 字符串路径；Runtime 层不加载 Godot 场景，Godot 项目可挂 `GodotProjectileEffectSpawner` 监听生成事件并实例化视觉节点。
- 生成时会绑定 `RelationshipType.EntityToProjectile / Source`，有目标实体时额外绑定 `RelationshipType.Target`。
- 飞行 Tick 由调用方持有的 `MovementSystem` 推进；Godot 项目可用 `GodotMovementDriver` 同步 Runtime 位置到视觉节点。

## Effect Capability 契约

- `EffectTool.Spawn` 是当前纯 Runtime 效果生成入口，生成 `RuntimeEntity`，写入 `EffectDataKeys` 和 Movement `Position`，并发布 `GameEventType.Effect.Spawned`。
- `EffectDataKeys` 当前覆盖 `ScenePath / Name / AnimationName / SourceEntity / AbilityEntity / TargetEntity / Position / Duration`。
- `Duration = -1` 表示不自动结束，遵守数值型“不限制”语义。
- 生成时会绑定 `RelationshipType.EntityToEffect / Source`，有目标实体时额外绑定 `RelationshipType.Target`。
- Godot 项目可挂 `GodotProjectileEffectSpawner` 监听生成事件并实例化视觉节点；Bridge 会解析 `AnimatedSprite2D` 并播放 `AnimationName`，为空时回退到场景当前动画或第一个可用动画，`Duration > 0` 时按动画帧数和 fps 调整 `SpeedScale`。
- 当前 Effect 仍不负责附着跟随和生命周期计时销毁，这些后续由 Effect runtime 扩展接入。

## Feature Capability 契约

- `FeatureService` 是当前 Feature 最小生命周期入口，负责 `Grant / Remove / Enable / Disable / Activate / End`。
- Feature 核心不引用 Ability 专有类型；子系统上下文统一通过 `FeatureContext.ActivationData` 传入，执行结果统一从 `FeatureContext.ExecuteResult` 取出。
- `FeatureDefinition` 当前包含 `FeatureId / HandlerId / Modifiers`；纯属性 Feature 可以只配置 `FeatureModifierEntry`，复杂逻辑通过 `IFeatureHandler` 扩展。
- `FeatureService.Grant` 会把 `FeatureModifierEntry` 转成 `DataModifier` 写入 Owner Data，Modifier source 标记为 Feature 实体；`Remove` 通过 `Data.RemoveModifiersBySource(feature)` 回滚。
- `FeatureHandlerRegistry` 以完整 `HandlerId` 查询 handler，不使用分组作为运行时查找键。
- `GameEventType.Feature` 当前覆盖 Granted / Removed / Enabled / Disabled / Activated / Executed / Ended。

## AI Capability 契约

- `AIService` 是当前 AI 最小 Tick 入口，接收复用式 `AIContext` 和行为树根节点。
- `AIContext` 只持有运行时 Entity、delta、AbilityService、外部准备好的 AbilityCastContext 和可选自动索敌技能集合，不持有 Godot Node 或物理体引用。
- `BehaviorNode.Evaluate(AIContext)` 只返回 `AIState`，通过 Entity Data 或系统入口表达意图。
- `AIService.Tick` 在 `AIDataKeys.IsEnabled=false` 或实体 `DamageDataKeys.IsDead=true` 时返回 Failure，并清空 `MovementDataKeys.AIMoveDirection`。
- `MoveToTargetAction` 只写 `MovementDataKeys.AIMoveDirection / AIMoveSpeedMultiplier`，由 `AIControlledMovementStrategy` 实际移动。
- `FindNearestTargetAction` 当前从 `EntityManager.GetAll()` 快照查询最近目标，支持 `range = -1` 不限距离、同队过滤、死亡过滤，并写入 `AIDataKeys.TargetEntity / TargetPosition / HasTargetPosition`。
- `IsTargetInRangeCondition` 使用当前目标实体或目标点检查距离，`range = -1` 表示不限距离；范围可从固定值或 DataKey 读取。
- `RequestAttackAction` 发出 `GameEventType.Attack.Requested`，payload 使用纯 Runtime `IEntity` 目标和 `Vector2Value` 目标点；动作会写入 `AIMoveDirection` 面向目标、`AIMoveSpeedMultiplier = 0` 停步，并把 `AIDataKeys.IsAttackRequested` 标记为本 Tick 已请求。
- `PatrolAction` 是当前确定性巡逻最小动作，读取 `AIDataKeys.PatrolCenter / PatrolRadius / PatrolWaitTime`，写入 `PatrolTargetPosition / PatrolWaitRemaining` 和 `MovementDataKeys.AIMoveDirection / AIMoveSpeedMultiplier`；不直接移动 Godot 节点。
- `EnemyBehaviorBlocks` 当前提供 `AttackBranch / ChaseBranch / PatrolBranch / AbilityAutoTriggerBranch`，只组合现有纯 Runtime 节点。
- `EnemyBehaviorTreeBuilder` 当前提供 `BuildMeleeEnemyTree / BuildAbilityMeleeEnemyTree / BuildPatrolOnlyTree / BuildChaserTree` 预制树，标准近战树顺序是攻击优先、追逐其次、巡逻兜底。
- `AIService.Tick` 每次 Tick 开始会清空 `AIDataKeys.IsAttackRequested`，只有当前 Tick 真的触发 `RequestAttackAction` 才置 true。
- `PrepareAbilityAutoTargetContextsAction` 会把 `AIContext.AutoTargetAbilities` 通过 `AbilityTargetingTool` 转成 `AIContext.AbilityContexts`，只准备上下文，不直接结算技能效果。
- `TickAbilityAutoTriggersAction` 只调用 `AbilityService.TickAutoTriggers`，不直接执行技能效果。

## Attack Capability 契约

- `AttackService` 是当前普通攻击最小 Runtime 入口，可直接调用 `TryRequest(...)`，也可通过 `Register(entity)` 订阅实体局部 `GameEventType.Attack.Requested`。
- `AttackDataKeys.Damage / Range / Interval / WindUpTime / RecoveryTime / CanAttack / IsAttacking / State / CooldownRemaining` 是当前稳定 Data 入口。
- `TryRequest` 会检查目标、攻击者死亡、目标死亡、`CanAttack`、攻击占用状态、冷却和距离；通过后发布 `Attack.Started`，进入前摇或立即结算。
- 命中结算统一调用 `DamageService`，伤害标签固定带 `DamageTags.Attack`，不直接修改 HP。
- 前摇、后摇和冷却由注入的 `TimerManager` 驱动；`Cancel` 会取消当前攻击相关 Timer、回到 Idle，并发布 `Attack.Cancelled`。
- `AttackService` 不播放动画、不持有 Godot Node；Godot 层通过 `GodotAttackComponent` 完成服务注册、导出参数写入、节点目标映射和动画请求转发，动画实际播放由 `GodotUnitAnimationComponent` 执行。
- `UnitDataKeys.AvailableAnimations` 是 Godot 动画桥写入的表现层运行态列表，供攻击、预览或工具层选择可播放动画名。

当前不包含：

- 旧 `IEntity` 直接依赖。
- 旧全局命名空间 `GameEventType` 直接依赖。
- Godot headless 场景断言。
- PhysicsServer2D trace 读取协议。
- Capability 层：Ability 点选目标上下文、自动索敌第一段、Projectile / Effect 纯 Runtime 生成入口、Projectile 命中生命周期、Projectile 穿透 / 生命周期扩展、Godot 实例化第一段和 Effect 动画播放第一段已迁入；DataOS authoring 已建立 schema / migration / generator / validator / Runtime snapshot loader 最小闭环；AI 巡逻最小 Runtime、行为树预制块、Ability 自动索敌上下文准备和 Godot AIComponent bridge 第一段已迁入；GodotAttackComponent、Attack 动画事件桥、旧 Attack 动画选择兼容和旧 AttackComponent 类名兼容包装已迁入；Movement 已完成旧 `MoveMode` 纯 C# 策略、Godot 2D 位移桥、运动碰撞、同帧多命中、Godot Physics broadphase 和 Godot Orientation 第一段；Collision / Damage / Attack 已完成第一段。

## 后续契约分层

- Runtime 契约：Entity / Component / Event / Relationship / Schedule / Pool / Timer / Resource。
- Capability 契约：Movement / Collision / Damage / Ability / Feature / AIBehavior / Projectile / Spawn / UIHud。
- Validation 契约：build、scene test、capability test、regression gate。
- Observation 契约：logs、dump、trace、snapshot、reports。

## 修改门禁

- Runtime API 改动必须同步本文件和 `SkilmeAI.GameOS.ApiIndex.md`。
- Capability 新增必须带 manifest、`Contract.md`、测试和 Debug 文档。
- 发布包前必须通过 `Tools/run-build.sh` 和 `Tools/run-tests.sh`。
