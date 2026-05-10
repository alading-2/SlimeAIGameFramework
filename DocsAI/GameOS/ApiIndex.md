# SkilmeAI.GameOS API Index

> Version: 0.1.0-alpha.0

## Runtime

| API | 类型 | 状态 | 说明 |
| --- | --- | --- | --- |
| `SkilmeAI.GameOS.Runtime.GameOSInfo` | static class | bootstrap | 包身份和迁移阶段标记。 |
| `GameOSInfo.PackageId` | const string | stable | `SkilmeAI.GameOS`。 |
| `GameOSInfo.Version` | const string | bootstrap | 当前包版本。 |
| `GameOSInfo.Stage` | const string | bootstrap | 当前迁移阶段。 |
| `SkilmeAI.GameOS.Runtime.Data.Data` | class | migrated | 运行时动态数据容器。 |
| `SkilmeAI.GameOS.Runtime.Data.DataMeta` | class | migrated | Data key 元数据和约束。 |
| `SkilmeAI.GameOS.Runtime.Data.DataRegistry` | static class | migrated | DataMeta 注册和查询。 |
| `SkilmeAI.GameOS.Runtime.Data.DataModifier` | class | migrated | 数值修改器。 |
| `SkilmeAI.GameOS.Runtime.Data.ModifierType` | enum | migrated | 修改器类型。 |
| `SkilmeAI.GameOS.Runtime.Data.DataKeyAttribute` | attribute | migrated | Config 属性到 Data key 的映射。 |
| `SkilmeAI.GameOS.Runtime.Data.IDataChangeSink` | interface | bootstrap | Event Runtime 迁入前的数据变更通知桥。 |
| `SkilmeAI.GameOS.Runtime.Data.DataChangedEventData` | record struct | bootstrap | 数据变更通知 payload。 |
| `SkilmeAI.GameOS.Runtime.Data.RuntimeDataSnapshot` | class | bootstrap | DataOS 生成的 Runtime JSON snapshot 读取入口，可应用记录到 `Data` 并注册资源。 |
| `SkilmeAI.GameOS.Runtime.Data.RuntimeDataRecord` | class | bootstrap | DataOS snapshot 中的一条数据记录。 |
| `SkilmeAI.GameOS.Runtime.Data.RuntimeDataField` | class | bootstrap | DataOS snapshot 字段值，支持 string / int / float / double / bool 转 CLR 值。 |
| `SkilmeAI.GameOS.Runtime.Data.RuntimeResourceEntry` | class | bootstrap | DataOS snapshot 资源映射条目。 |
| `SkilmeAI.GameOS.Runtime.Event.EventPriority` | enum | migrated | EventBus 订阅优先级。 |
| `SkilmeAI.GameOS.Runtime.Event.EventBus` | class | migrated | 局部或全局事件总线。 |
| `SkilmeAI.GameOS.Runtime.Event.EventContext` | class | migrated | 请求-响应事件上下文。 |
| `SkilmeAI.GameOS.Runtime.Event.GlobalEventBus` | static class | migrated | 全局事件总线入口。 |
| `SkilmeAI.GameOS.Runtime.Event.GameEventType` | static class | migrated | GameOS 内置事件名和 payload。 |
| `SkilmeAI.GameOS.Runtime.Event.EventDataChangeSink` | class | migrated | Data 变更到 EventBus 的桥。 |
| `SkilmeAI.GameOS.Runtime.Entity.IEntity` | interface | migrated | 最小 Entity 契约。 |
| `SkilmeAI.GameOS.Runtime.Entity.RuntimeEntity` | class | migrated | 纯 C# Runtime Entity。 |
| `SkilmeAI.GameOS.Runtime.Entity.EntitySpawnConfig` | record struct | migrated | Runtime Entity 生成参数。 |
| `SkilmeAI.GameOS.Runtime.Entity.EntityManager` | static class | migrated | 最小 Entity 生命周期注册表。 |
| `SkilmeAI.GameOS.Runtime.Relationship.RelationshipConstraint` | enum | migrated | 关系写入约束。 |
| `SkilmeAI.GameOS.Runtime.Relationship.ParentDestroyPolicy` | enum | migrated | 父实体销毁时对子实体的处理策略。 |
| `SkilmeAI.GameOS.Runtime.Relationship.RelationshipType` | static class | migrated | GameOS 内置关系类型。 |
| `SkilmeAI.GameOS.Runtime.Relationship.RelationshipRecord` | class | migrated | 关系记录。 |
| `SkilmeAI.GameOS.Runtime.Relationship.RelationshipLifecycle` | static class | migrated | PARENT 关系生命周期数据工具。 |
| `SkilmeAI.GameOS.Runtime.Relationship.RelationshipManager` | static class | migrated | 三索引 Runtime 关系图。 |
| `SkilmeAI.GameOS.Runtime.Schedule.GameFlowState` | enum | migrated | 游戏流程状态。 |
| `SkilmeAI.GameOS.Runtime.Schedule.OverlayFlags` | enum | migrated | 项目覆盖层标记。 |
| `SkilmeAI.GameOS.Runtime.Schedule.SimulationState` | enum | migrated | 项目模拟状态。 |
| `SkilmeAI.GameOS.Runtime.Schedule.SystemGroup` | enum | migrated | 系统分组。 |
| `SkilmeAI.GameOS.Runtime.Schedule.SystemTag` | enum | migrated | 系统标签。 |
| `SkilmeAI.GameOS.Runtime.Schedule.ProjectStateService` | class | migrated | 项目状态服务。 |
| `SkilmeAI.GameOS.Runtime.Schedule.SystemRunCondition` | class | migrated | 系统运行条件。 |
| `SkilmeAI.GameOS.Runtime.Schedule.ScheduleCategory` | enum | bootstrap | Schedule DataKey 分类：System / Preset / Spawn。 |
| `SkilmeAI.GameOS.Runtime.Schedule.ScheduleDataKeys` | static class | bootstrap | 系统配置、系统预设和 Spawn config 使用的 Runtime DataKey。 |
| `SkilmeAI.GameOS.Runtime.Schedule.IRuntimeSystem` | interface | migrated | 系统生命周期协议。 |
| `SkilmeAI.GameOS.Runtime.Schedule.IRuntimeCommandHandler<TRequest,TResult>` | interface | migrated | 系统命令处理协议。 |
| `SkilmeAI.GameOS.Runtime.Schedule.RuntimeSchedule` | class | migrated | 纯 C# Runtime 调度器。 |
| `SkilmeAI.GameOS.Runtime.Resource.ResourceCategory` | enum | migrated | 资源分类。 |
| `SkilmeAI.GameOS.Runtime.Resource.ResourceData` | record struct | migrated | 资源映射条目。 |
| `SkilmeAI.GameOS.Runtime.Resource.ResourceCatalog` | static class | migrated | 资源键到路径映射表。 |
| `SkilmeAI.GameOS.Runtime.Resource.ResourceManagement` | static class | migrated | 统一资源加载 facade。 |
| `SkilmeAI.GameOS.Runtime.Pool.IPoolable` | interface | migrated | 对象池生命周期接口。 |
| `SkilmeAI.GameOS.Runtime.Pool.IObjectPool` | interface | migrated | 全局池注册表使用的非泛型句柄。 |
| `SkilmeAI.GameOS.Runtime.Pool.ObjectPoolConfig` | record struct | migrated | 对象池配置。 |
| `SkilmeAI.GameOS.Runtime.Pool.PoolStats` | record struct | migrated | 对象池统计。 |
| `SkilmeAI.GameOS.Runtime.Pool.ObjectPool<T>` | class | migrated | 类型安全对象池。 |
| `SkilmeAI.GameOS.Runtime.Pool.ObjectPoolManager` | static class | migrated | 全局对象池注册和归还入口。 |
| `SkilmeAI.GameOS.Runtime.Timer.GameTimer` | class | migrated | 可池化运行时计时器。 |
| `SkilmeAI.GameOS.Runtime.Timer.TimerManager` | class | migrated | 由外部 Tick 驱动的计时器管理器。 |
| `SkilmeAI.GameOS.Runtime.Event.GameEventType.Movement` | static class | migrated | Movement Capability 事件名和 payload。 |
| `SkilmeAI.GameOS.Runtime.Event.GameEventType.Collision` | static class | bootstrap | Collision Capability 事件名和 payload。 |
| `SkilmeAI.GameOS.Runtime.Event.GameEventType.Attack` | static class | bootstrap | Attack Runtime 请求 / 开始 / 完成 / 取消事件名和 payload，payload 使用纯 Runtime `IEntity` 与 `Vector2Value`。 |
| `SkilmeAI.GameOS.Runtime.Event.GameEventType.Unit` | static class | bootstrap | Unit 表现层动画请求 / 停止 / 完成事件名和 payload。 |
| `SkilmeAI.GameOS.Runtime.Event.GameEventType.Feature` | static class | bootstrap | Feature Capability 生命周期事件名和 payload。 |

## Capabilities

| API | 类型 | 状态 | 说明 |
| --- | --- | --- | --- |
| `SkilmeAI.GameOS.Capabilities.Movement.Vector2Value` | record struct | migrated | 纯 C# 二维向量值，避免 Capability 内核直接依赖 Godot 类型。 |
| `SkilmeAI.GameOS.Capabilities.Movement.MovementDataKeys` | static class | migrated | Movement 运行时与 authoring DataKey：Position / Velocity / FacingDirection / IsMoving / MoveSpeed / InputDirection / AIMoveDirection / AIMoveSpeedMultiplier / CanMoveInput / LastMoveDirection，以及 handler 可映射到 MovementParams 的 SineWave / Orbit / Boomerang / Bezier / Parabola / CircularArc 参数。 |
| `SkilmeAI.GameOS.Capabilities.Movement.MoveMode` | enum | migrated | 当前迁入 `None / Charge / Orbit / SineWave / BezierCurve / Boomerang / AttachToHost / PlayerInput / AIControlled / Parabola / CircularArc`。 |
| `SkilmeAI.GameOS.Capabilities.Movement.MovementParams` | record struct | migrated | 全部旧 `MoveMode` 纯运行时策略使用的方向、目标点、目标实体、附着偏移、速度、轨道、波形、Bezier、Boomerang、曲线、时长、距离、可选运动碰撞参数和朝向参数。 |
| `SkilmeAI.GameOS.Capabilities.Movement.OrientationMode` | enum | bootstrap | Movement 视觉朝向模式：FollowMovement / SpinOnly / FollowMovementAndSpin。 |
| `SkilmeAI.GameOS.Capabilities.Movement.OrientationSink` | enum | bootstrap | Movement 朝向输出目标：RootRotation / VisualFlipX。 |
| `SkilmeAI.GameOS.Capabilities.Movement.OrientationParams` | record struct | bootstrap | Movement 期间可选的视觉朝向控制参数。 |
| `SkilmeAI.GameOS.Capabilities.Movement.MovementCollisionParams` | record struct | bootstrap | 单次移动的碰撞策略：Collision 过滤、目标匹配、半径覆盖、碰撞次数停止、销毁和回调。 |
| `SkilmeAI.GameOS.Capabilities.Movement.MovementCollisionPolicy` | class | bootstrap | 纯运行时 Movement 碰撞过滤、去重、计数和线段/圆形扫描。 |
| `SkilmeAI.GameOS.Capabilities.Movement.IMovementCollisionTargetQuery` | interface | bootstrap | Movement 碰撞候选目标查询接口，允许 Runtime 扫描或 Godot Physics broadphase 注入。 |
| `SkilmeAI.GameOS.Capabilities.Movement.MovementCollisionRuntimeTargetQuery` | class | bootstrap | 默认 EntityManager 快照碰撞候选查询。 |
| `SkilmeAI.GameOS.Capabilities.Movement.MovementCollisionContext` | record struct | bootstrap | 有效 Movement 碰撞上下文。 |
| `SkilmeAI.GameOS.Capabilities.Movement.MovementCollisionTargetMatchMode` | enum | bootstrap | Movement 碰撞目标匹配模式。 |
| `SkilmeAI.GameOS.Capabilities.Movement.IMovementStrategy` | interface | migrated | Movement Strategy 生命周期接口。 |
| `SkilmeAI.GameOS.Capabilities.Movement.MovementStrategyRegistry` | static class | migrated | `MoveMode -> IMovementStrategy` 注册表。 |
| `SkilmeAI.GameOS.Capabilities.Movement.ChargeMovementStrategy` | class | migrated | 纯 C# Charge 直线 / 到点移动策略。 |
| `SkilmeAI.GameOS.Capabilities.Movement.OrbitMovementStrategy` | class | migrated | 纯 C# 固定点环绕策略。 |
| `SkilmeAI.GameOS.Capabilities.Movement.SineWaveMovementStrategy` | class | migrated | 纯 C# 正弦波前进策略。 |
| `SkilmeAI.GameOS.Capabilities.Movement.BezierCurveMovementStrategy` | class | migrated | 纯 C# 固定控制点 Bezier 曲线策略。 |
| `SkilmeAI.GameOS.Capabilities.Movement.BoomerangMovementStrategy` | class | migrated | 纯 C# 去程、停顿、返程回旋镖策略。 |
| `SkilmeAI.GameOS.Capabilities.Movement.AttachToHostMovementStrategy` | class | migrated | 纯 C# 目标实体 Position 跟随策略。 |
| `SkilmeAI.GameOS.Capabilities.Movement.PlayerInputMovementStrategy` | class | migrated | 消费 `MovementDataKeys.InputDirection` 的玩家输入策略。 |
| `SkilmeAI.GameOS.Capabilities.Movement.AIControlledMovementStrategy` | class | migrated | 消费 AI 方向和速度倍率 DataKey 的 AI 常驻移动策略。 |
| `SkilmeAI.GameOS.Capabilities.Movement.ParabolaMovementStrategy` | class | migrated | 纯 C# 抛物线轨迹策略。 |
| `SkilmeAI.GameOS.Capabilities.Movement.CircularArcMovementStrategy` | class | migrated | 纯 C# 圆弧轨迹策略。 |
| `SkilmeAI.GameOS.Capabilities.Movement.MovementSystem` | class | migrated | 纯 C# Movement 调度系统，负责 Start / Stop / Tick / 位置积分 / 停止事件。 |
| `SkilmeAI.GameOS.Capabilities.Movement.MovementStopReason` | enum | migrated | Movement 停止原因。 |
| `SkilmeAI.GameOS.Capabilities.Movement.MovementStopContext` | record struct | migrated | Movement 停止事件上下文。 |
| `SkilmeAI.GameOS.Capabilities.Collision.CollisionLayers` | static class | migrated | 旧项目 2D 碰撞层常量。 |
| `SkilmeAI.GameOS.Capabilities.Collision.CollisionDataKeys` | static class | bootstrap | Collision 运行时 DataKey：CollisionLayer / CollisionMask / Team / CollisionRadius。 |
| `SkilmeAI.GameOS.Capabilities.Collision.CollisionFilterPolicy` | record struct | bootstrap | layer/mask、自身和同队过滤策略。 |
| `SkilmeAI.GameOS.Capabilities.Collision.CollisionContact` | record struct | bootstrap | 纯运行时碰撞接触 payload。 |
| `SkilmeAI.GameOS.Capabilities.Collision.CollisionSystem` | class | bootstrap | 纯 C# 碰撞过滤和 Entered / Exited 事件发布。 |
| `SkilmeAI.GameOS.Capabilities.Damage.DamageDataKeys` | static class | bootstrap | Damage 运行时 DataKey：CurrentHp / MaxHp / IsDead / IsInvulnerable / Armor / Shield / DodgeChance / CritRate / CritDamage / DamageTakenMultiplier / LifeSteal / ContactDamage / ContactDamageInterval / Damage 统计键。 |
| `SkilmeAI.GameOS.Capabilities.Damage.DamageType` | enum | bootstrap | 伤害类型：Physical / Magical / True。 |
| `SkilmeAI.GameOS.Capabilities.Damage.DamageTags` | enum flags | bootstrap | 伤害标签：Attack / Ability / Contact / Persistent / Area / Projectile。 |
| `SkilmeAI.GameOS.Capabilities.Damage.DamageInfo` | class | bootstrap | 单次伤害上下文。 |
| `SkilmeAI.GameOS.Capabilities.Damage.DamageResult` | record struct | bootstrap | 单次伤害处理结果。 |
| `SkilmeAI.GameOS.Capabilities.Damage.IDamageProcessor` | interface | bootstrap | Damage 处理器协议。 |
| `SkilmeAI.GameOS.Capabilities.Damage.DamageProcessorPriority` | static class | bootstrap | 默认 Damage 处理器优先级常量。 |
| `SkilmeAI.GameOS.Capabilities.Damage.BaseDamageProcessor` | class | bootstrap | 伤害前置检查和基础伤害初始化。 |
| `SkilmeAI.GameOS.Capabilities.Damage.DodgeProcessor` | class | bootstrap | 非真实伤害闪避判定和 `damage:dodged` 事件。 |
| `SkilmeAI.GameOS.Capabilities.Damage.CriticalDamageProcessor` | class | bootstrap | 暴击概率和暴击倍率结算。 |
| `SkilmeAI.GameOS.Capabilities.Damage.ShieldDamageProcessor` | class | bootstrap | 护盾吸收，优先于护甲结算。 |
| `SkilmeAI.GameOS.Capabilities.Damage.ArmorDamageProcessor` | class | bootstrap | 护甲倍率结算，真实伤害跳过。 |
| `SkilmeAI.GameOS.Capabilities.Damage.DamageTakenAmplificationProcessor` | class | bootstrap | 目标受伤倍率结算。 |
| `SkilmeAI.GameOS.Capabilities.Damage.HealthExecutionProcessor` | class | bootstrap | 生命值扣减、HealthChanged / Damaged / Killed 事件。 |
| `SkilmeAI.GameOS.Capabilities.Damage.LifestealProcessor` | class | bootstrap | 基于最终伤害和 LifeSteal 触发吸血恢复。 |
| `SkilmeAI.GameOS.Capabilities.Damage.DamageStatisticsProcessor` | class | bootstrap | 伤害、命中、暴击、击杀、波次统计写入。 |
| `SkilmeAI.GameOS.Capabilities.Damage.DamageService` | class | bootstrap | 统一伤害入口，管理默认处理器管线并返回 DamageResult。 |
| `SkilmeAI.GameOS.Capabilities.Damage.HealSource` | enum | bootstrap | 治疗来源：Direct / Lifesteal / Regeneration。 |
| `SkilmeAI.GameOS.Capabilities.Damage.HealInfo` | class | bootstrap | 单次治疗上下文。 |
| `SkilmeAI.GameOS.Capabilities.Damage.HealResult` | record struct | bootstrap | 单次治疗处理结果。 |
| `SkilmeAI.GameOS.Capabilities.Damage.HealService` | class | bootstrap | 统一治疗入口，负责 HP 夹取、统计和 Healed / HealthChanged 事件。 |
| `SkilmeAI.GameOS.Capabilities.Damage.DamageApplyOptions` | record struct | bootstrap | DamageTool 单次伤害参数。 |
| `SkilmeAI.GameOS.Capabilities.Damage.DamageRepeatOptions` | record struct | bootstrap | DamageTool 周期伤害参数。 |
| `SkilmeAI.GameOS.Capabilities.Damage.DamageApplyResult` | class | bootstrap | DamageTool 多目标 / 周期伤害结果。 |
| `SkilmeAI.GameOS.Capabilities.Damage.DamageTool` | static class | bootstrap | 多目标和周期伤害工具，统一调用 DamageService 与 TimerManager。 |
| `SkilmeAI.GameOS.Capabilities.Ability.AbilityDataKeys` | static class | bootstrap | Ability 运行时 DataKey：类型、触发模式、目标语义、自动索敌、描述 / 图标 / 分组 / 消耗、冷却、充能、伤害、施法范围、效果半径和链式参数。 |
| `SkilmeAI.GameOS.Capabilities.Ability.AbilityType` | enum | bootstrap | 技能类型：Active / Passive / Weapon。 |
| `SkilmeAI.GameOS.Capabilities.Ability.AbilityTriggerMode` | enum flags | bootstrap | 技能触发模式：Manual / OnEvent / Periodic / Permanent。 |
| `SkilmeAI.GameOS.Capabilities.Ability.AbilityTargetSelection` | enum | bootstrap | 技能目标输入语义：None / Entity / Point / EntityOrPoint。 |
| `SkilmeAI.GameOS.Capabilities.Ability.AbilityTriggerResult` | enum | bootstrap | 技能触发结果和失败原因。 |
| `SkilmeAI.GameOS.Capabilities.Ability.AbilityCastContext` | class | bootstrap | 单次技能触发上下文，支持显式实体目标和点选目标位置。 |
| `SkilmeAI.GameOS.Capabilities.Ability.AbilityExecutedResult` | class | bootstrap | 技能执行结果。 |
| `SkilmeAI.GameOS.Capabilities.Ability.AbilityTriggerReport` | record struct | bootstrap | 技能触发报告。 |
| `SkilmeAI.GameOS.Capabilities.Ability.AbilityService` | class | bootstrap | Ability 最小运行时服务，负责实体 / 点选目标正式触发、冷却、充能、Periodic 自动触发 Tick、DamageTool 命中和可选 Feature handler 调用。 |
| `SkilmeAI.GameOS.Capabilities.Ability.AbilityAutoTargetOptions` | class | bootstrap | Ability 自动索敌参数：Range / MaxTargets / IgnoreSameTeam / RequiresDamageable / IncludeDeadTargets。 |
| `SkilmeAI.GameOS.Capabilities.Ability.AbilityTargetingTool` | static class | bootstrap | Ability 显式目标准备工具，按 Ability 自动索敌 DataKey 从 Runtime Entity 快照构造 `AbilityCastContext`。 |
| `SkilmeAI.GameOS.Capabilities.Projectile.ProjectileCategory` | enum | bootstrap | Projectile DataKey 分类。 |
| `SkilmeAI.GameOS.Capabilities.Projectile.ProjectileDataKeys` | static class | bootstrap | Projectile 运行时 DataKey：ScenePath / SourceEntity / AbilityEntity / TargetEntity / SpawnPosition / TargetPosition / Direction / Speed / MaxHitCount / HitCount / MaxLifeTime / Damage / DamageType / DamageTags。 |
| `SkilmeAI.GameOS.Capabilities.Projectile.ProjectileSpawnOptions` | class | bootstrap | 投射物生成参数。 |
| `SkilmeAI.GameOS.Capabilities.Projectile.ProjectileSpawnResult` | record struct | bootstrap | 投射物生成结果。 |
| `SkilmeAI.GameOS.Capabilities.Projectile.ProjectileMovementOptions` | class | bootstrap | 投射物飞行、穿透和生命周期参数，配置移动、命中伤害、最大命中数、最大存活时间、停止和销毁语义。 |
| `SkilmeAI.GameOS.Capabilities.Projectile.ProjectileTool` | static class | bootstrap | 纯 Runtime 投射物工具，支持生成、启动 Movement 飞行、穿透多目标、命中伤害结算、停止销毁并发布 `projectile:spawned / projectile:hit`。 |
| `SkilmeAI.GameOS.Capabilities.Effect.EffectCategory` | enum | bootstrap | Effect DataKey 分类。 |
| `SkilmeAI.GameOS.Capabilities.Effect.EffectDataKeys` | static class | bootstrap | Effect 运行时 DataKey：ScenePath / Name / AnimationName / SourceEntity / AbilityEntity / TargetEntity / Position / Duration。 |
| `SkilmeAI.GameOS.Capabilities.Effect.EffectSpawnOptions` | class | bootstrap | 效果生成参数。 |
| `SkilmeAI.GameOS.Capabilities.Effect.EffectSpawnResult` | record struct | bootstrap | 效果生成结果。 |
| `SkilmeAI.GameOS.Capabilities.Effect.EffectTool` | static class | bootstrap | 纯 Runtime 效果生成工具，写入 Data、位置、关系并发布 `effect:spawned`。 |
| `SkilmeAI.GameOS.Capabilities.Feature.FeatureDataKeys` | static class | bootstrap | Feature 运行时 DataKey：FeatureId / HandlerId / Description / Category / modifier authoring 字段 / IsEnabled / IsActive / ActivationCount。 |
| `SkilmeAI.GameOS.Capabilities.Feature.FeatureDefinition` | class | bootstrap | 可授予能力定义，包含 FeatureId、HandlerId 和授予时 Modifier 列表。 |
| `SkilmeAI.GameOS.Capabilities.Feature.FeatureModifierEntry` | record struct | bootstrap | Feature 授予时应用到 Owner Data 的 Modifier 配置。 |
| `SkilmeAI.GameOS.Capabilities.Feature.FeatureContext` | class | bootstrap | Feature 生命周期上下文，承载 Owner / Feature / Definition / ActivationData / ExecuteResult。 |
| `SkilmeAI.GameOS.Capabilities.Feature.FeatureEndReason` | enum | bootstrap | Feature 单次运行结束原因：Completed / Cancelled / Interrupted / Failed。 |
| `SkilmeAI.GameOS.Capabilities.Feature.IFeatureHandler` | interface | bootstrap | Feature 处理器协议：OnGranted / OnRemoved / OnEnabled / OnDisabled / OnActivated / OnExecute / OnEnded。 |
| `SkilmeAI.GameOS.Capabilities.Feature.FeatureHandlerRegistry` | static class | bootstrap | Feature handler 注册表，以完整 HandlerId 查询。 |
| `SkilmeAI.GameOS.Capabilities.Feature.FeatureService` | class | bootstrap | Feature 最小生命周期服务，负责授予 / 移除 Modifier、生命周期事件和 handler 调用。 |
| `SkilmeAI.GameOS.Capabilities.Attack.AttackDataKeys` | static class | bootstrap | Attack 运行时 DataKey：Damage / Range / Interval / WindUpTime / RecoveryTime / CanAttack / IsAttacking / State / CooldownRemaining。 |
| `SkilmeAI.GameOS.Capabilities.Attack.AttackState` | enum | bootstrap | 普通攻击状态：Idle / WindUp / Recovery。 |
| `SkilmeAI.GameOS.Capabilities.Attack.AttackCancelReason` | enum | bootstrap | 攻击流程取消原因。 |
| `SkilmeAI.GameOS.Capabilities.Attack.AttackTriggerResult` | enum | bootstrap | 普通攻击请求结果和失败原因。 |
| `SkilmeAI.GameOS.Capabilities.Attack.AttackTriggerReport` | record struct | bootstrap | 普通攻击请求报告。 |
| `SkilmeAI.GameOS.Capabilities.Attack.AttackService` | class | bootstrap | 普通攻击最小 Runtime 服务，消费 `GameEventType.Attack.Requested`，处理前摇 / 后摇 / 冷却并通过 `DamageService` 结算 `DamageTags.Attack` 伤害。 |
| `SkilmeAI.GameOS.Capabilities.Unit.UnitDataKeys` | static class | bootstrap | Unit 运行时 DataKey：Name / EntityType / DeathType / VisualScenePath / HealthBarHeight / IsShowHealthBar / PickupRange / ExpReward / DetectionRange / AvailableAnimations。 |
| `SkilmeAI.GameOS.Capabilities.AI.AIDataKeys` | static class | bootstrap | AI 运行时 DataKey：IsEnabled / TargetEntity / TargetPosition / HasTargetPosition / IsAttackRequested / AttackRange / PatrolCenter / PatrolRadius / PatrolWaitTime / PatrolTargetPosition / HasPatrolTargetPosition / PatrolWaitRemaining / PatrolDirectionSign。 |
| `SkilmeAI.GameOS.Capabilities.AI.AIContext` | class | bootstrap | AI 行为树 Tick 上下文，承载实体、delta、AbilityService、自动施法上下文和可选自动索敌技能集合。 |
| `SkilmeAI.GameOS.Capabilities.AI.AIState` | enum | bootstrap | AI 节点返回状态：Failure / Success / Running。 |
| `SkilmeAI.GameOS.Capabilities.AI.BehaviorNode` | abstract class | bootstrap | AI 行为树节点基类。 |
| `SkilmeAI.GameOS.Capabilities.AI.SequenceNode` | class | bootstrap | 顺序组合节点。 |
| `SkilmeAI.GameOS.Capabilities.AI.SelectorNode` | class | bootstrap | 选择组合节点。 |
| `SkilmeAI.GameOS.Capabilities.AI.AIService` | class | bootstrap | AI 最小 Tick 服务，负责启用 / 死亡门禁并执行行为树。 |
| `SkilmeAI.GameOS.Capabilities.AI.FindNearestTargetAction` | class | bootstrap | 从 Runtime Entity 快照中查找最近目标并写入 AI 目标 Data。 |
| `SkilmeAI.GameOS.Capabilities.AI.MoveToTargetAction` | class | bootstrap | 向目标写入 Movement AI 移动意图。 |
| `SkilmeAI.GameOS.Capabilities.AI.IsTargetInRangeCondition` | class | bootstrap | 检查当前目标实体或目标点是否在固定范围或 DataKey 范围内。 |
| `SkilmeAI.GameOS.Capabilities.AI.RequestAttackAction` | class | bootstrap | 行为树中发出 `GameEventType.Attack.Requested`，并写入停步和面向目标意图。 |
| `SkilmeAI.GameOS.Capabilities.AI.PrepareAbilityAutoTargetContextsAction` | class | bootstrap | 行为树中把 `AutoTargetAbilities` 转换为带自动索敌结果的 `AbilityContexts`。 |
| `SkilmeAI.GameOS.Capabilities.AI.TickAbilityAutoTriggersAction` | class | bootstrap | 行为树中调用 AbilityService 推进 Periodic 自动施法。 |
| `SkilmeAI.GameOS.Capabilities.AI.PatrolAction` | class | bootstrap | 确定性巡逻动作，写入 Movement AI 移动意图并维护巡逻等待倒计时。 |
| `SkilmeAI.GameOS.Capabilities.AI.EnemyBehaviorBlocks` | static class | bootstrap | AI 可复用行为积木块：AttackBranch / ChaseBranch / PatrolBranch / AbilityAutoTriggerBranch。 |
| `SkilmeAI.GameOS.Capabilities.AI.EnemyBehaviorTreeBuilder` | static class | bootstrap | AI 预制树工厂：BuildMeleeEnemyTree / BuildAbilityMeleeEnemyTree / BuildPatrolOnlyTree / BuildChaserTree。 |

## Observation

| API | 类型 | 状态 | 说明 |
| --- | --- | --- | --- |
| `SkilmeAI.GameOS.Observation.GameOSLogLevel` | enum | bootstrap | 通用日志等级：Trace / Debug / Info / Pass / Warn / Error / Fail。 |
| `SkilmeAI.GameOS.Observation.GameOSLogEntry` | class | bootstrap | 单条结构化日志，支持 `[LEVEL][Context] message key=value` 格式化。 |
| `SkilmeAI.GameOS.Observation.IGameOSLogSink` | interface | bootstrap | 日志 sink 协议。 |
| `SkilmeAI.GameOS.Observation.GameOSLog` | static class | bootstrap | 全局日志配置、context logger 创建和 sink 注册入口。 |
| `SkilmeAI.GameOS.Observation.GameOSContextLog` | class | bootstrap | context-bound logger，提供 Trace / Debug / Info / Pass / Warn / Error / Fail。 |
| `SkilmeAI.GameOS.Observation.GameOSMemoryLogSink` | class | bootstrap | 测试和 artifact 聚合用内存 sink。 |
| `SkilmeAI.GameOS.Observation.GameOSJsonlLogSink` | class | bootstrap | JSONL sink，默认用于 `artifacts/logs/scene-log.jsonl`。 |
| `SkilmeAI.GameOS.Observation.GameOSObservationSession` | class | bootstrap | 读取 scene runner 环境变量并准备 artifact / JSONL 路径。 |
| `SkilmeAI.GameOS.Observation.CheckResult` | record | bootstrap | validation check 返回值。 |
| `SkilmeAI.GameOS.Observation.SceneValidationSession` | class | bootstrap | Godot validation scene 检查项日志、失败聚合和 artifact 写入 helper。 |

## GodotBridge

| API | 类型 | 状态 | 说明 |
| --- | --- | --- | --- |
| `SkilmeAI.GameOS.GodotBridge.GodotEntity` | Node | migrated | 可挂场景的 GameOS Entity 基类。 |
| `SkilmeAI.GameOS.GodotBridge.GodotEntity2D` | Node2D | migrated | 可挂 2D 场景的 GameOS Entity 基类，会把初始 Node2D Position 写入 Movement Data。 |
| `SkilmeAI.GameOS.GodotBridge.GodotAreaEntity2D` | Area2D | bootstrap | 可挂 2D 物理场景的 GameOS Entity 基类，会同步 Position 和 Collision layer/mask 到 Runtime Data。 |
| `SkilmeAI.GameOS.GodotBridge.IGodotComponent` | interface | migrated | Godot Node Component 生命周期协议。 |
| `SkilmeAI.GameOS.GodotBridge.GameOSGodotBridge` | static class | migrated | SceneTree 与 Runtime 生命周期桥接入口。 |
| `SkilmeAI.GameOS.GodotBridge.GodotNodeRegistry` | static class | migrated | Godot Node 到稳定运行时 Id 的注册表。 |
| `SkilmeAI.GameOS.GodotBridge.GameOSTimerDriver` | Node | migrated | 用 `_Process` 驱动 `TimerManager.Instance.Tick`。 |
| `SkilmeAI.GameOS.GodotBridge.GodotMovementDriver` | Node | migrated | 用 `_Process` 驱动 `MovementSystem.Tick` 并同步 Runtime Position 到 Node2D。 |
| `SkilmeAI.GameOS.GodotBridge.GodotOrientationComponent` | Node | bootstrap | 消费 `MovementDataKeys.FacingDirection` / `MovementParams.Orientation`，输出 `RotationDegrees` 或 `AnimatedSprite2D.FlipH`。 |
| `SkilmeAI.GameOS.GodotBridge.GodotUnitAnimationComponent` | Node | bootstrap | 消费 `GameEventType.Unit` 动画事件并驱动 `AnimatedSprite2D`，缓存可用动画，支持一次性动画完成事件和待机回退。 |
| `SkilmeAI.GameOS.GodotBridge.GodotPhysicsMovementCollisionTargetQuery` | class | bootstrap | 用 Godot Physics 2D `IntersectShape` 收集 Movement 碰撞 broadphase 候选，并回退 Runtime Entity 扫描。 |
| `SkilmeAI.GameOS.GodotBridge.GodotCollisionBridge` | static class | bootstrap | Godot `CollisionObject2D` / Node 到 Collision Runtime 的实体解析、Data 同步和事件桥。 |
| `SkilmeAI.GameOS.GodotBridge.GodotCollisionComponent` | Node | bootstrap | 桥接 Entity 根 `Area2D` 进入 / 离开信号到 `GameEventType.Collision`。 |
| `SkilmeAI.GameOS.GodotBridge.GodotHurtboxComponent` | Area2D | bootstrap | 桥接 Hurtbox `Area2D` 进入 / 离开信号到 `GameEventType.Collision.Hurtbox*`。 |
| `SkilmeAI.GameOS.GodotBridge.GodotContactDamageComponent` | Node | bootstrap | 消费 Hurtbox 事件并通过 `DamageService` 结算接触伤害。 |
| `SkilmeAI.GameOS.GodotBridge.GodotAttackComponent` | Node | bootstrap | 注册默认 `AttackService`，把导出攻击参数写入 Runtime Data，把 Godot 节点目标解析为 Runtime 攻击请求，并可选把 Attack 事件转为 Unit 动画请求；配置动画不存在时可从 `attack*` 可用动画中回退选择，可选择保留注册前已有 Attack Data。 |
| `SkilmeAI.GameOS.GodotBridge.AttackComponent` | Node | bootstrap | 旧项目 `AttackComponent` 类名 / 场景名兼容包装，默认保留已有 Attack Data。 |
| `SkilmeAI.GameOS.GodotBridge.GodotAIBehaviorTreeKind` | enum | bootstrap | Godot AI bridge 内置行为树类型：Melee / AbilityMelee / PatrolOnly / Chaser。 |
| `SkilmeAI.GameOS.GodotBridge.GodotAIComponent` | Node | bootstrap | 驱动 `AIService.Tick` 的 Godot AI 组件，写入导出 AI 参数和 Movement AI 意图，不直接移动节点。 |
| `SkilmeAI.GameOS.GodotBridge.GodotProjectileEffectSpawner` | Node | bootstrap | 监听 Projectile / Effect Runtime 生成事件，通过 `ScenePath` 加载 `PackedScene`，实例化视觉节点并按 Runtime EntityId 注册到 `GodotNodeRegistry`，并自动播放 Effect `AnimatedSprite2D`。 |
| `SkilmeAI.GameOS.GodotBridge.GodotBridgeStats` | record struct | migrated | 桥接注册状态快照。 |
| `SkilmeAI.GameOS.GodotBridge.GodotNodePoolConfig` | record struct | migrated | Godot Node 池配置。 |
| `SkilmeAI.GameOS.GodotBridge.GodotNodePool<T>` | class | migrated | 带泊车、脱树和碰撞隔离的 Godot Node 对象池。 |
| `SkilmeAI.GameOS.GodotBridge.GodotNodePoolManager` | static class | migrated | Godot Node 池查询和归还入口。 |
| `SkilmeAI.GameOS.GodotBridge.GodotCollisionIsolation` | static class | migrated | 2D 碰撞隔离工具。 |

## Validation

| API | 类型 | 状态 | 说明 |
| --- | --- | --- | --- |
| `Tools/run-build.sh` | script | active | 构建 `SkilmeAI.slnx`。 |
| `Tools/run-dataos-validate.sh` | script | active | 建立临时 SQLite DB，应用 DataOS migration 并运行 schema 验证。 |
| `Tools/run-tests.sh` | script | active | 运行 DataOS schema 验证和 `SkilmeAI.GameOS.Tests` 行为测试。 |
| `DataOS/Generators/generate-runtime-snapshot.sh` | script | active | 从 SQLite authoring DB 生成 Runtime JSON snapshot。 |
| `DataOS/Validation/validate-dataos.sh` | script | active | 校验 DataOS authoring DB 的外键、空键、bool、资源分类和路径。 |
| `DocsAI/GameOS/Observation.md` | document | draft | 定义 Schedule、EventBus、Relationship lifecycle、Capability selector、DataOS trace、Command playback 和 BrotatoLike scene PASS/FAIL artifact contract。 |
| `Tests/SkilmeAI.GameOS.Tests` | console project | active | 覆盖 Event/Data/DataOS snapshot/Entity/Relationship/Schedule/Pool/Timer/Resource/Movement/MovementCollision、DamageService、HealService、DamageTool、AbilityService 点选目标语义、Ability 自动索敌、ProjectileTool 生成与命中生命周期、EffectTool 动画名写入、FeatureService、AttackService、AI 行为树、AI 巡逻、AI 行为树预制块、AI 攻击请求、AI 自动索敌施法上下文准备、Damage 处理器管线、目标查询注入和 OrientationParams 最小行为。 |
| `Games/BrotatoLike/Src/Game/BrotatoLikeDataOSBootstrap.cs` | class | active | 游戏侧正式 DataOS snapshot 入口，可读取 `res://DataOS/Snapshots/runtime_snapshot.json`、注册资源并按记录生成 Runtime Entity。 |
| `Games/BrotatoLike/Src/Game/Main.cs` | Godot scene script | active | 当前 headless smoke 覆盖 Movement、MovementCollision、Damage / ContactDamage / Attack、旧 AttackComponent 兼容包装、Attack 动画播放、Godot AI bridge、Ability 点选目标、Ability 自动索敌、Projectile / Effect Runtime 与 Godot 实例化、DataOS bootstrap 生成 Runtime Entity、Godot Physics broadphase、GodotMovementDriver、GodotOrientationComponent、GodotBridge 和 GodotNodePool 接入。 |
| `Games/BrotatoLike/Tools/run-godot-smoke.sh` | script | active | 使用 Godot 4.6.2 mono CLI 运行 BrotatoLike Runtime / Movement / MovementCollision / Damage / ContactDamage / Attack / Attack Animation lifecycle / Godot AI bridge / Ability 点选目标 / Ability 自动索敌 / Projectile / Effect Runtime 与 Godot 实例化 / Effect 动画播放 / Projectile 命中生命周期 / 穿透 / MaxLifeTime 销毁 / Godot Physics broadphase / Godot Orientation / GodotBridge smoke。 |

Movement 已完成旧 `MoveMode` 纯 C# Capability 策略迁入、Godot 2D 位移桥、运行时圆形扫描版运动碰撞、同帧多命中派发、Godot Physics broadphase 查询和 Godot 朝向输出第一批。Collision 已完成层常量、DataKey、过滤策略、运行时事件和 Godot `Area2D` / Hurtbox bridge 第一批。Damage 已完成 DamageService / HealService / DamageTool / DamageDataKeys / ContactDamage bridge / 处理器管线第一批。Ability 已完成最小 Runtime 切片、点选目标语义、自动索敌第一段、Periodic 自动触发 Tick 并可选调用 Feature handler；Projectile / Effect 已完成纯 Runtime 生成入口和 Godot 实例化第一段，Projectile 已完成命中生命周期、穿透多目标和 MaxLifeTime 停止销毁，Effect 已完成动画播放第一段；Feature 已完成最小生命周期；AI 已完成最小纯 Runtime 行为树、最近目标查询、巡逻、行为树预制块、Ability 自动索敌上下文准备、Godot AI bridge 和攻击请求事件；Attack 已完成最小 Runtime 结算、GodotAttackComponent bridge、旧 AttackComponent 兼容包装、Attack 动画事件桥第二段和旧 Attack 动画选择兼容第一段。DataOS 已建立 SQLite schema / migration / generator / validator / Runtime snapshot loader 最小闭环。
