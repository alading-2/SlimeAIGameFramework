# SkilmeAI ProjectState

> 更新日期：2026-05-10

## 当前阶段

M3 Runtime 最小内核已完成，M4 BrotatoLike 最小接入已完成，M5-M17 的 GodotBridge、Movement、Collision、Damage、Ability、Projectile、Effect、Feature、AI、Attack 和 DataOS 最小闭环已完成。M18 DataOS 扩大迁移与正式生成入口切片已完成：框架新增 `ScheduleDataKeys`，扩展 Unit / Ability / Feature DataMeta；后续已补 `MovementDataKeys` 中 SineWave / Orbit / Boomerang / Bezier / Parabola / CircularArc / AttachToHost 的 handler authoring 参数；BrotatoLike seed 覆盖 TargetingIndicator、ChainAbility、Feature definition / modifier、System config / preset、Spawn config、更多旧 ResourcePaths 和 Ability handler-specific 参数第三段；游戏侧新增 `BrotatoLikeDataOSBootstrap` 和 `BrotatoLikeAbilityHandlers`，正式代码可从 generated snapshot 生成 Runtime Entity 并注册 ResourceCatalog，且 SineWave / Boomerang / BezierCurve / CircularArc / Orbit / AttachToHost、Dash、ChainLightning、Slam、TargetPoint、CircleDamage 和 AuraShield 已通过游戏侧 Feature handler 执行闭环。

计划系统已切换到 OpenSpec 口径：框架级功能、重构、架构调整、迁移账本和长期实施任务默认进入 `openspec/changes/<change>/`。`Plans/` 保留为历史研究、方向背景和长期参考；新的执行 checklist 不再默认写入 `Plans/` 或 `.omx/plans/`。

DocsAI 已成为框架长期知识事实源。GameOS、Capabilities、DataOS、Agent Protocol、Observation 和 Godot 场景测试文档集中在 `SkilmeAI/DocsAI/`；源码目录只保留允许例外和操作指针。

GameOS Observation 已建立第一版通用日志和场景验证 helper：`GameOSLog`、`GameOSObservationSession`、`SceneValidationSession`、memory sink 和 JSONL sink 已进入框架侧；BrotatoLike scene runner 委托 `.codex/skills/godot-scene-test/scripts/godot-scene-runner.mjs`，新日志结构为 `index.json + per-scene result/combined/artifacts/logs/scene-log.jsonl`。

## 下一步

1. 继续扩大 DataOS 迁移范围：旧 DataNew 剩余字段、剩余 Feature actions，以及把尚未接线的 Ability / Movement handler authoring 参数接入真实 handler 执行路径。
2. 继续把 BrotatoLike 真实主场景、UI 和 SpawnSystem 专项场景接入统一 Godot scene runner 和 Observation artifact。
3. 推进 DataOS snapshot 到真实主场景 / UI / 生成系统入口。
4. 建立 Godot 引擎 trace 计划，源码入口是 `/home/slime/Code/SkilmeAI/Engine/Engine/godot-4.6.2-stable`。
5. 将后续框架级计划按 `DocsAI/Agent/Protocols/OpenSpecChangeProtocol.md` 创建 OpenSpec change，再进入实现。

## 风险

- Godot Node 对象池碰撞隔离已有第一版 API，并已通过 BrotatoLike Godot headless smoke；PhysicsServer2D trace 尚未接入。
- 当前碰撞隔离已覆盖 `CollisionObject2D` 根节点脱树、泊车、layer/mask 清零、Area2D monitoring/monitorable 和 CollisionShape/Polygon 禁用；PhysicsServer2D trace 尚未接入。
- Relationship / Schedule 已迁入纯 C# 最小内核；ScheduleDataKeys 和 BrotatoLike 第一批系统配置 / 预设 / Spawn config 已进入 DataOS snapshot，但尚未接完整 RuntimeSchedule 自动装载。
- Movement Capability 已覆盖旧项目 `None / Charge / Orbit / SineWave / BezierCurve / Boomerang / AttachToHost / PlayerInput / AIControlled / Parabola / CircularArc` 纯运行时策略，并完成运行时圆形扫描版运动碰撞、同帧多命中派发、目标查询注入、Godot Physics broadphase、Godot 朝向输出第一批和 DataOS 可写入的 handler authoring DataKey；BrotatoLike 已接入 SineWave / Orbit / Boomerang / Bezier / CircularArc / AttachToHost / Charge 从 DataOS 组装 `MovementParams` 的执行路径，真实输入 UI 和更多游戏侧宿主内容仍未迁入。
- Collision Capability 已迁入旧碰撞层常量、`CollisionDataKeys`、`CollisionFilterPolicy`、`CollisionContact`、`CollisionSystem`、`GameEventType.Collision`、`GodotAreaEntity2D`、`GodotCollisionBridge`、`GodotCollisionComponent` 和 `GodotHurtboxComponent` 第一批；ContactDamage bridge 已接入 DamageService。
- Damage Capability 已迁入 `DamageDataKeys`、`DamageType`、`DamageTags`、`DamageInfo`、`DamageService`、`HealService`、`DamageTool`、`IDamageProcessor`、默认处理器优先级、基础检查、闪避、暴击、护盾、护甲、受伤倍率、生命值结算、吸血、统计处理器、`GameEventType.Damage` 和 Godot `GodotContactDamageComponent` 第一批；SystemCore 门禁尚未迁入。
- Ability Capability 已迁入 `AbilityDataKeys`、`AbilityType`、`AbilityTriggerMode`、`AbilityTargetSelection`、`AbilityTriggerResult`、`AbilityCastContext`、`AbilityExecutedResult`、`AbilityAutoTargetOptions`、`AbilityTargetingTool`、`AbilityService` 和 `GameEventType.Ability` 最小切片；当前覆盖显式目标正式触发、点选目标正式触发、启用 / 执行中 / 冷却 / 充能检查、冷却 Tick、`AbilityTriggerMode.Periodic` 自动触发 Tick、显式自动索敌上下文准备、通过 `DamageTool` 造成即时或周期伤害，并可通过 `AbilityDataKeys.FeatureHandlerId` 调用 Feature handler。
- Feature Capability 已迁入 `FeatureDataKeys`、`FeatureDefinition`、`FeatureModifierEntry`、`FeatureContext`、`FeatureEndReason`、`IFeatureHandler`、`FeatureHandlerRegistry`、`FeatureService` 和 `GameEventType.Feature` 最小生命周期；当前覆盖授予 / 移除 Modifier、启用 / 禁用、激活 / 执行 / 结束、handler 生命周期和 AbilityService 可选接入；Feature actions、自动触发、点选、Projectile / Effect、AI 和 DataOS authoring 适配尚未迁入。
- Attack Capability 已迁入 `AttackDataKeys`、`AttackState`、`AttackTriggerResult`、`AttackTriggerReport`、`AttackCancelReason`、`AttackService` 最小 Runtime、`GodotAttackComponent` bridge 第一段、`AttackComponent` 旧类名兼容包装、`UnitDataKeys`、`GameEventType.Unit` 和 `GodotUnitAnimationComponent` 动画事件桥第二段，以及旧 Attack 动画选择兼容第一段；当前覆盖消费 `GameEventType.Attack.Requested`、前摇 / 后摇 / 冷却 Timer、距离 / 死亡 / 可攻击门禁、通过 `DamageService` 造成 `DamageTags.Attack` 伤害，并发布 Started / Finished / Cancelled 事件；Godot bridge 可注册服务、写入导出参数、保留注册前已有 Attack Data、把节点目标映射为 Runtime 攻击请求，把 Attack Started / Cancelled 转为 AnimatedSprite2D 播放 / 停止请求，缓存可用动画，在配置动画不存在时从 `attack*` 可用动画中选择，并在一次性动画完成后发布 `unit:animation_finished` 回 idle。
- AI Capability 已迁入 `AIDataKeys`、`AIContext`、`AIState`、`AIService`、`BehaviorNode`、`SequenceNode`、`SelectorNode`、`FindNearestTargetAction`、`MoveToTargetAction`、`IsTargetInRangeCondition`、`RequestAttackAction`、`PrepareAbilityAutoTargetContextsAction`、`TickAbilityAutoTriggersAction`、`PatrolAction`、`EnemyBehaviorBlocks`、`EnemyBehaviorTreeBuilder`、`GodotAIComponent` 和 `GodotAIBehaviorTreeKind`；当前覆盖启用 / 死亡门禁、Runtime Entity 快照最近目标查询、同队 / 死亡 / 距离过滤、向目标写入 `MovementDataKeys.AIMoveDirection / AIMoveSpeedMultiplier`、范围内发出 `GameEventType.Attack.Requested` 并停步面向目标、确定性左右巡逻和等待倒计时、标准近战树攻击优先 / 追逐 / 巡逻回退、Godot 组件导出参数写入和手动 / `_Process` Tick，通过 AbilityTargetingTool 准备自动索敌施法上下文，以及通过 AbilityService 推进 Periodic 自动施法。
- DataOS schema / migration / generator / validator / Runtime snapshot loader 已创建；当前覆盖 BrotatoLike 玩家 / 敌人 / TargetingIndicator / Ability / ChainAbility / Ability handler-specific 参数第三段 / Feature definition / modifier / 系统配置 / 预设 / Spawn config / 资源路径第一批，尚未覆盖旧 DataNew 全量字段、正式生成器和编辑器 authoring UI。
- 玩家输入系统已迁入：`GodotPlayerInputComponent`（框架 GodotBridge）读取 Godot Input Map 的 MoveLeft/Right/Up/Down、UseSkill、PreviousSkill、NextSkill，将移动方向写入 `MovementDataKeys.InputDirection`，并将技能按钮事件发射到 Entity EventBus；支持 `CanMoveInput` 门禁和 `Movement.Acceleration` 平滑移动插值。`GodotActiveSkillInputComponent`（游戏侧）订阅技能输入事件，管理 `AbilityDataKeys.OwnedAbilityIds` 和 `CurrentAbilityIndex`，通过 `AbilityTargetingTool` 自动索敌后调用 `AbilityService.TryTrigger`。BrotatoLike `SpawnPlayer` 已接入：生成玩家时从 DataOS 创建 slam / chain_lightning 初始技能实体，挂载双输入组件，启动 PlayerInput 移动策略。

## 最新验证

```bash
Tools/run-build.sh
Tools/run-tests.sh
```

结果：Runtime 行为测试全部 PASS，DataOS schema 验证 PASS；新增 Observation level filter、JSONL serialization、validation failure aggregation 测试通过。BrotatoLike Runtime Event validation、普通 `Scenes/Main.tscn` playable acceptance、`run-main-smoke` 均已通过新结构 runner 和 analyzer。
