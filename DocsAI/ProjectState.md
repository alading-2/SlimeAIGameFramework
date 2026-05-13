# SlimeAI ProjectState

> 更新日期：2026-05-13

## 当前阶段

M3 Runtime 最小内核已完成，M4 BrotatoLike 最小接入已完成，M5-M17 的 GodotBridge、Movement、Collision、Damage、Ability、Projectile、Effect、Feature、AI、Attack 和 DataOS 最小闭环已完成。M18 DataOS 扩大迁移与正式生成入口切片已完成：框架新增 `ScheduleDataKeys`，Unit / Ability / Feature / Movement / Collision / Damage / AI / Attack / Projectile / Effect 已迁到 typed `DataKey<T>`；后续已补 `MovementDataKeys` 中 SineWave / Orbit / Boomerang / Bezier / Parabola / CircularArc / AttachToHost 的 handler authoring 参数；BrotatoLike seed 覆盖 TargetingIndicator、ChainAbility、Feature definition / modifier、System config / preset、Spawn config、更多旧 ResourcePaths 和 Ability handler-specific 参数第三段；游戏侧新增 `BrotatoLikeDataOSBootstrap` 和 `BrotatoLikeAbilityHandlers`，正式代码可从 generated typed snapshot 生成 Runtime Entity 并注册 ResourceCatalog，且 SineWave / Boomerang / BezierCurve / CircularArc / Orbit / AttachToHost、Dash、ChainLightning、Slam、TargetPoint、CircleDamage 和 AuraShield 已通过游戏侧 Feature handler 执行闭环。

Runtime 事件系统已作为 OpenSpec 基线完成：事件 API 换成 type-keyed `Publish<T>(in T) / Subscribe<T>(Action<T>) → IDisposable / ExportObservation(path)` 三件套；事件 payload 是 `readonly record struct` 并自声明 scope（`IEntityEvent / IGlobalEvent / IBroadcastEvent`）；`EventBus / GlobalEventBus / EventContext / EventPriority / Once / Off` 和整个 `GameEventTypes/` 目录已删除；事件按 Capability 组织在 `SlimeAI/GameOS/Capabilities/<Cap>/Events/`，Runtime 级事件在 `SlimeAI/GameOS/Runtime/Events/{Core,Global}/`；`WorldEvents.World` 替换旧 `GlobalEventBus.Global`；同类型嵌套 Publish 被 per-bus reentry guard 阻断；内建 `EventBusObservation.ExportTo` 导出 `eventbus-dump.json`（`schemaVersion / busName / generatedAtUtc / subscriptions / emittedCounts / sameTypeReentryBlockedCounts / handlerExceptions / handlerRegistrationOrder`）。

计划系统已切换到 OpenSpec 口径：框架级功能、重构、架构调整、迁移账本和长期实施任务默认进入 `openspec/changes/<change>/`。完成后的要求合入 `openspec/specs/` 作为当前规范基线；旧执行 checklist 和归档历史不再作为 AI 入口保留。

DocsAI 已成为框架长期知识事实源。GameOS、Capabilities、DataOS、Agent Protocol、Observation 和 Godot 场景测试文档集中在 `SlimeAI/DocsAI/`；源码目录只保留允许例外和操作指针。

AI 新功能开发入口已补齐：新增 `.codex/skills/ai-feature-development` 和 `DocsAI/Agent/Protocols/AIFeatureDevelopmentProtocol.md`，作为每次写新功能、扩展能力、迁移旧逻辑或重构框架行为前的前置入口；该入口强调 AI 可路由、可验证、可观察优先，不为旧框架兼容保留新入口，并把纯 Runtime / DataOS tooling 的普通数据处理收束到 C# 标准库和框架 API。该入口现已补功能收尾闸门：新功能必须补专项 Runtime test 或独立 Godot 验证场景，主场景 smoke 只作回归补充，并在结束前同步相关 DocsAI、Contract、Debug、ApiIndex、ProjectState、游戏侧状态和 owner skill 统一源。

GameOS Observation 已建立第一版通用日志和场景验证 helper：`GameOSLog`、`GameOSObservationSession`、`SceneValidationSession`、memory sink 和 JSONL sink 已进入框架侧；BrotatoLike scene runner 委托 `.codex/skills/godot-scene-test/scripts/godot-scene-runner.mjs`，新日志结构为 `index.json + per-scene result/combined/artifacts/logs/scene-log.jsonl`。Runtime/Data 已补独立 Godot validation scene：`res://SlimeAI/Scenes/Validation/Runtime/Data/RuntimeDataValidation.tscn`，覆盖 typed `DataKey<T>` lifecycle、`DataCatalog` resolve、modifier/computed dirty、category reset 和 Data-to-Event bridge artifact。

## 下一步

1. 继续扩大 DataOS 迁移范围：旧 DataNew 剩余字段、剩余被动 Feature actions，以及尚未接线的具体 Ability / Feature handler 参数。
2. 继续把 BrotatoLike 真实 UI、SpawnSystem 专项场景和更细的输入专项测试接入统一 Godot scene runner 和 Observation artifact；普通主场景可玩切片和 smoke 已有 PASS artifact。
3. 推进 DataOS snapshot 到真实 UI 和更多游戏场景内容；真实主场景 / 生成系统入口已有第一批 DataOS 驱动证据。
4. 建立 Godot 引擎 trace 计划，源码入口是 `/home/slime/Code/SlimeAI/Resources/Engine/Engine/godot-4.6.2-stable`。
5. 将后续框架级计划按 `DocsAI/Agent/Protocols/OpenSpecChangeProtocol.md` 创建 OpenSpec change，再进入实现。

## 风险

- Godot Node 对象池碰撞隔离已有第一版 API，并已通过 BrotatoLike Godot headless smoke；PhysicsServer2D trace 尚未接入。
- 当前碰撞隔离已覆盖 `CollisionObject2D` 根节点脱树、泊车、layer/mask 清零、Area2D monitoring/monitorable 和 CollisionShape/Polygon 禁用；PhysicsServer2D trace 尚未接入。
- Relationship / Schedule 已迁入纯 C# 最小内核；ScheduleDataKeys 和 BrotatoLike 第一批系统配置 / 预设 / Spawn config 已进入 DataOS snapshot，但尚未接完整 RuntimeSchedule 自动装载。
- Movement Capability 已覆盖旧项目 `None / Charge / Orbit / SineWave / BezierCurve / Boomerang / AttachToHost / PlayerInput / AIControlled / Parabola / CircularArc` 纯运行时策略，并完成运行时圆形扫描版运动碰撞、同帧多命中派发、目标查询注入、Godot Physics broadphase、Godot 朝向输出第一批和 DataOS 可写入的 handler authoring DataKey；BrotatoLike 已接入 SineWave / Orbit / Boomerang / Bezier / CircularArc / AttachToHost / Charge 从 DataOS 组装 `MovementParams` 的执行路径，真实输入 UI 和更多游戏侧宿主内容仍未迁入。
- Collision Capability 已迁入旧碰撞层常量、`CollisionDataKeys`、`CollisionFilterPolicy`、`CollisionContact`、`CollisionSystem`、`Capabilities.Collision.Events`、`GodotAreaEntity2D`、`GodotCollisionBridge`、`GodotCollisionComponent` 和 `GodotHurtboxComponent` 第一批；ContactDamage bridge 已接入 DamageService。
- Damage Capability 已迁入 `DamageDataKeys`、`DamageType`、`DamageTags`、`DamageInfo`、`DamageService`、`HealService`、`DamageTool`、`IDamageProcessor`、默认处理器优先级、基础检查、闪避、暴击、护盾、护甲、受伤倍率、生命值结算、吸血、统计处理器、`Capabilities.Damage.Events` 和 Godot `GodotContactDamageComponent` 第一批；SystemCore 门禁尚未迁入。
- Ability Capability 已迁入 `AbilityDataKeys`、`AbilityType`、`AbilityTriggerMode`、`AbilityTargetSelection`、`AbilityTriggerResult`、`AbilityCastContext`、`AbilityExecutedResult`、`AbilityAutoTargetOptions`、`AbilityTargetingTool`、`AbilityService` 和 `Capabilities.Ability.Events` 最小切片；当前覆盖显式目标正式触发、点选目标正式触发、启用 / 执行中 / 冷却 / 充能检查、冷却 Tick、`AbilityTriggerMode.Periodic` 自动触发 Tick、显式自动索敌上下文准备、通过 `DamageTool` 造成即时或周期伤害，并可通过 `AbilityDataKeys.FeatureHandlerId` 调用 Feature handler。
- Feature Capability 已迁入 `FeatureDataKeys`、`FeatureDefinition`、`FeatureModifierEntry`、`FeatureContext`、`FeatureEndReason`、`IFeatureHandler`、`FeatureHandlerRegistry`、`FeatureService` 和 `Capabilities.Feature.Events` 最小生命周期；当前覆盖授予 / 移除 Modifier、启用 / 禁用、激活 / 执行 / 结束、handler 生命周期和 AbilityService 可选接入；Feature actions、自动触发、点选、Projectile / Effect、AI 和 DataOS authoring 适配尚未迁入。
- Attack Capability 已迁入 `AttackDataKeys`、`AttackState`、`AttackTriggerResult`、`AttackTriggerReport`、`AttackCancelReason`、`AttackService` 最小 Runtime、`GodotAttackComponent` bridge 第一段、`AttackComponent` 旧类名兼容包装、`UnitDataKeys`、`Capabilities.Unit.Events` 和 `GodotUnitAnimationComponent` 动画事件桥第二段，以及旧 Attack 动画选择兼容第一段；当前覆盖消费 `Capabilities.Attack.Events.Requested`、前摇 / 后摇 / 冷却 Timer、距离 / 死亡 / 可攻击门禁、通过 `DamageService` 造成 `DamageTags.Attack` 伤害，并发布 Started / Finished / Cancelled 事件；Godot bridge 可注册服务、写入导出参数、保留注册前已有 Attack Data、把节点目标映射为 Runtime 攻击请求，把 Attack Started / Cancelled 转为 AnimatedSprite2D 播放 / 停止请求，缓存可用动画，在配置动画不存在时从 `attack*` 可用动画中选择，并在一次性动画完成后发布 `Capabilities.Unit.Events.AnimationFinished` 回 idle。
- AI Capability 已迁入 `AIDataKeys`、`AIContext`、`AIState`、`AIService`、`BehaviorNode`、`SequenceNode`、`SelectorNode`、`FindNearestTargetAction`、`MoveToTargetAction`、`IsTargetInRangeCondition`、`RequestAttackAction`、`PrepareAbilityAutoTargetContextsAction`、`TickAbilityAutoTriggersAction`、`PatrolAction`、`EnemyBehaviorBlocks`、`EnemyBehaviorTreeBuilder`、`GodotAIComponent` 和 `GodotAIBehaviorTreeKind`；当前覆盖启用 / 死亡门禁、Runtime Entity 快照最近目标查询、同队 / 死亡 / 距离过滤、向目标写入 `MovementDataKeys.AIMoveDirection / AIMoveSpeedMultiplier`、范围内发出 `Capabilities.Attack.Events.Requested` 并停步面向目标、确定性左右巡逻和等待倒计时、标准近战树攻击优先 / 追逐 / 巡逻回退、Godot 组件导出参数写入和手动 / `_Process` Tick，通过 AbilityTargetingTool 准备自动索敌施法上下文，以及通过 AbilityService 推进 Periodic 自动施法。
- Runtime Data 已切换到 typed contract：业务访问入口是 `DataKey<T>`，`Data` 绑定 frozen `DataCatalog` 并使用 typed slot 存储，旧 string/object/DataMeta/DataRegistry 正式入口已移除；DataOS descriptor 持有 authoring metadata 和 default mirror，snapshot loader 必须先校验 manifest/descriptors/catalog 后再按 resolved `DataKey<T>` typed apply。DataOS schema / migration / generator / validator / Runtime snapshot loader 已创建 typed snapshot 闭环，validator 会阻断 descriptor missing/extra、type/default drift、disabled capability 未裁剪、wrong snapshot type 和 unknown key；当前覆盖 BrotatoLike 玩家 / 敌人 / TargetingIndicator / Ability / ChainAbility / Ability handler-specific 参数第三段 / Feature definition / modifier / 系统配置 / 预设 / Spawn config / 资源路径第一批，尚未覆盖旧 DataNew 全量字段和编辑器 authoring UI。
- 玩家输入系统已迁入：`GodotPlayerInputComponent`（框架 GodotBridge）读取 Godot Input Map 的 MoveLeft/Right/Up/Down、UseSkill、PreviousSkill、NextSkill，将移动方向写入 `MovementDataKeys.InputDirection`，并将技能按钮事件发射到 Entity EventBus；支持 `CanMoveInput` 门禁和 `Movement.Acceleration` 平滑移动插值。`GodotActiveSkillInputComponent`（游戏侧）订阅技能输入事件，管理 `AbilityDataKeys.OwnedAbilityIds` 和 `CurrentAbilityIndex`，通过 `AbilityTargetingTool` 自动索敌后调用 `AbilityService.TryTrigger`。BrotatoLike `SpawnPlayer` 已接入：生成玩家时从 DataOS 创建 slam / chain_lightning 初始技能实体，挂载双输入组件，启动 PlayerInput 移动策略。

## 最新验证

```bash
Tools/run-build.sh
Tools/run-tests.sh
Tools/run-dataos-validate.sh
cd /home/slime/Code/SlimeAI/Games/BrotatoLike
Tools/run-build.sh
Tools/run-godot-scene.sh run-main-smoke --log-dir .ai-temp/scene-tests/runs
Tools/analyze-godot-scene-logs.sh
```

结果：Runtime 行为测试全部 PASS，DataOS schema / descriptor / manifest 验证 PASS；BrotatoLike build PASS，`run-main-smoke` 输出 `BrotatoLike GameOS smoke PASS`，analyzer 输出 `status: pass`、`firstError: none`。typed loader 曾捕获 `Movement.OrbitTotalAngle` descriptor default drift，修正 seed mirror 后通过。

事件系统基线补充验证：`Tests/SlimeAI.GameOS.Tests/Events/EventBusTests.cs` 覆盖同类型 reentry 阻断、跨类型级联、跨 bus 同类型事件、handler 异常隔离、Dispose 退订、注册顺序、Broadcast 路由和 scope 越界拒绝；`Tools/run-tests.sh` 全部 PASS。BrotatoLike `run-main-smoke` 在 `.ai-temp/scene-tests/runs/2026-05-13/09-23-37/.../artifacts/eventbus-dump.json` 导出 world bus dump，`jq '.SameTypeReentryBlockedCounts, .HandlerExceptions'` 输出 `{}` 和 `[]`；普通 `Scenes/Main.tscn` acceptance 最新通过 artifact 为 `.ai-temp/scene-tests/runs/2026-05-13/09-24-15/index.json`。

Runtime/Data 专项 Godot 验证补充：当前初始开发阶段默认选 BrotatoLike 作为承载游戏，已临时同步其 `SlimeAI/` submodule 工作树中的 `Scenes/Validation/Runtime/Data` 和 `Src/SceneTests/Runtime/Data`，并通过 `BrotatoLike.csproj` 重新包含 `SlimeAI/Src/SceneTests/**/*.cs`。后续多游戏 / 成品阶段不默认同步所有游戏，改按每个游戏的框架版本策略更新 submodule 指针。`Tools/run-godot-scene.sh list` 已能列出 `res://SlimeAI/Scenes/Validation/Runtime/Data/RuntimeDataValidation.tscn`；`Tools/run-godot-scene.sh run res://SlimeAI/Scenes/Validation/Runtime/Data/RuntimeDataValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs && Tools/analyze-godot-scene-logs.sh` 输出 `GameOS Runtime Data validation PASS`，artifact 位于 `.ai-temp/scene-tests/runs/2026-05-13/15-47-30/index.json`。
