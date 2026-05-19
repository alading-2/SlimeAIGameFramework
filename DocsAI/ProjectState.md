# SlimeAI ProjectState

> 更新日期：2026-05-15（新增 AI-first Runtime refactor Godot validation scenes）

## 当前阶段

M3 Runtime 最小内核已完成，M4 BrotatoLike 最小接入已完成，M5-M17 的 GodotBridge、Movement、Collision、Damage、Ability、Projectile、Effect、Feature、AI、Attack 和 DataOS 最小闭环已完成。M18 DataOS 扩大迁移与正式生成入口切片已完成：框架新增 `ScheduleDataKeys`，Unit / Ability / Feature / Movement / Collision / Damage / AI / Attack / Projectile / Effect 已迁到 typed `DataKey<T>`；后续已补 `MovementDataKeys` 中 SineWave / Orbit / Boomerang / Bezier / Parabola / CircularArc / AttachToHost 的 handler authoring 参数；BrotatoLike seed 覆盖 TargetingIndicator、ChainAbility、Feature definition / modifier、System config / preset、Spawn config、更多旧 ResourcePaths 和 Ability handler-specific 参数第三段；游戏侧新增 `BrotatoLikeDataOSBootstrap` 和 `BrotatoLikeAbilityHandlers`，正式代码可从 generated typed snapshot 生成 Runtime Entity 并注册 ResourceCatalog，且 SineWave / Boomerang / BezierCurve / CircularArc / Orbit / AttachToHost、Dash、ChainLightning、Slam、TargetPoint、CircleDamage 和 AuraShield 已通过游戏侧 Feature handler 执行闭环。

Runtime 事件系统已作为 OpenSpec 基线完成：事件 API 换成 type-keyed `Publish<T>(in T) / Subscribe<T>(Action<T>) → IDisposable / ExportObservation(path)` 三件套；事件 payload 是 `readonly record struct` 并自声明 scope（`IEntityEvent / IGlobalEvent / IBroadcastEvent`）；`EventBus / GlobalEventBus / EventContext / EventPriority / Once / Off` 和整个 `GameEventTypes/` 目录已删除；事件按 Capability 组织在 `SlimeAI/GameOS/Capabilities/<Cap>/Events/`，Runtime 级事件在 `SlimeAI/GameOS/Runtime/Events/{Core,Global}/`；`WorldEvents.World` 替换旧 `GlobalEventBus.Global`；同类型嵌套 Publish 被 per-bus reentry guard 阻断；内建 `EventBusObservation.ExportTo` 导出 `eventbus-dump.json`（`schemaVersion / busName / generatedAtUtc / subscriptions / emittedCounts / sameTypeReentryBlockedCounts / handlerExceptions / handlerRegistrationOrder`）。P3 `refactor-runtime-events-purge-game-leakage` 已完成并 archived：Bucket A 旧事件 `MouseSelection* / Wave* / GameStart / GameOver / GamePause / GameResume` 直接删除不迁移；Bucket B `InputUseSkill / InputPreviousSkill / InputNextSkill` 与输入桥接迁到 BrotatoLike；`Runtime/Events/Global/` 当前无 framework-owned payload。

计划系统已切换到 OpenSpec 口径：框架级功能、重构、架构调整、迁移账本和长期实施任务默认进入 `openspec/changes/<change>/`。第一轮 AI-first Runtime 大重构的总体纲领和当前进度维护在工作区根 `openspec/Plan/2026-05-15-ai-first-runtime-refactor-plan.md`；完成后的要求合入 `openspec/specs/` 作为当前规范基线；旧执行 checklist 和归档历史不再作为 AI 入口保留。

AI-first Runtime 重构第一轮**已完成**：P2a `refactor-runtime-entity-id-typed-value` 已完成并 archived，typed `EntityId` baseline 已合入 `openspec/specs/runtime-entity-identity/spec.md`；P1 `refactor-runtime-relationship-as-lifecycle-tree` 已完成并 archived，`RelationshipManager / RelationshipType / RelationshipRecord` 已删除，LifecycleTree 与 typed business reference baseline 已合入 `openspec/specs/runtime-relationship-lifecycle/spec.md` 和 `openspec/specs/runtime-business-entity-references/spec.md`；P2b `refactor-runtime-world-facade` 已完成并 archived，`RuntimeWorld.Default / CreateScoped()` 与 world-scoped Entity / Lifecycle / Events / Resources / Pools baseline 已合入 `openspec/specs/runtime-world-container/spec.md`；P3 `refactor-runtime-events-purge-game-leakage` 已完成并 archived，game-leakage cleanup baseline 已合入 `openspec/specs/runtime-event-game-leakage-cleanup/spec.md`，`runtime-event-system` baseline 已强化事件归属边界；P4 `refactor-runtime-command-buffer-with-phases` 已完成并 archived，`SchedulePhase`、`RuntimeCommandBuffer` 8 种 typed command kind（包含 `GodotNodeInstantiate / GodotNodeFree`）、`EnterGuard(reason)` 保护域、`RuntimeWorld.Commands / Schedule` 与 `CommandPlaybackReport` typed observation 已合入 `openspec/specs/runtime-command-buffer/spec.md` 和 `openspec/specs/runtime-schedule-phase/spec.md`；P5 `enhance-ai-feature-development-skill` 已完成并 archived，`ai-feature-development-skill` baseline 已合入 `openspec/specs/ai-feature-development-skill/spec.md`，`.ai-config/skills/ai/ai-feature-development/` 现在固定 9 段 SKILL.md 与 12 个 references。P4 **未做** `IRuntimeSystem -> IRuntimeProcess` rename，也未合并 `SystemConfig + SystemDescriptor + SystemRuntimeInfo`，这两项列入 future backlog，后续必须新建 OpenSpec change。

DocsAI 已成为框架长期知识事实源。GameOS、Capabilities、DataOS、Observation 和 Godot 场景测试文档集中在 `SlimeAI/DocsAI/`；AI agent 工作流、协议、角色、hooks/subagent/MCP/git 策略集中在 `Workspace/SystemAgent/`，旧的框架仓 Agent 文档入口已删除，不再作为兼容入口；源码目录只保留允许例外和操作指针。

AI 新功能开发入口已补齐并开始进入统一 workflow governance：统一源为 `.ai-config/skills/ai/ai-feature-development/`，同步副本为 `.codex/skills/ai-feature-development` / `.claude/skills/ai-feature-development` / `.windsurf/skills/ai-feature-development`，协议入口为 `Workspace/SystemAgent/Protocols/AIFeatureDevelopmentProtocol.md`，工作区总纲入口为 `Workspace/SystemAgent/INDEX.md`。旧的框架仓 Agent 文档入口已删除，不再保存长期流程正文或兼容指针。SKILL.md 固定为 9 段：目标、入口顺序、设计规则、文件修改前、实现优先级、功能收尾、验证、Skill 同步、完成检查；references 固定为 12 个文件，覆盖 workflow governance、validation closure、framework research filter、refactor decision tree、typed value、rename pipeline、framework/game boundary、structural guard、RuntimeWorld facade、lifecycle/business reference、spec/code alignment、skill sync discipline。该入口强调 AI 可路由、可验证、可观察、可回顾优先，不为旧框架兼容保留新入口；新功能必须先写验收标准，补专项 Runtime test 或独立 Godot 验证场景，主场景 smoke 只作回归补充，并在结束前同步相关 DocsAI、Contract、Debug、ApiIndex、ProjectState、游戏侧状态、owner skill 统一源和 retrospective 结论。

当前正在执行 `unify-ai-agent-development-workflow` OpenSpec change：新增 `Workspace/SystemAgent/` 总纲、角色提示词和研究采纳记录；更新 `AIFeatureDevelopmentProtocol`、Godot Scene Validation / Testing 文档和 `ai-feature-development` skill，明确 SDD / OpenSpec、TDD / 标准答案、AI retrospective、hooks / subagent / MCP / git checkpoint 的统一流程。该 change 不修改 GameOS runtime、DataOS schema、GodotBridge 或 BrotatoLike gameplay。

GameOS Observation 已建立第一版通用日志和场景验证 helper：`GameOSLog`、`GameOSObservationSession`、`SceneValidationSession`、memory sink 和 JSONL sink 已进入框架侧；BrotatoLike scene runner 委托 `.codex/skills/godot-scene-test/scripts/godot-scene-runner.mjs`，新日志结构为 `index.json + per-scene result/combined/artifacts/logs/scene-log.jsonl`。Runtime/Data 已补独立 Godot validation scene：`res://SlimeAI/Src/Validation/Runtime/Data/RuntimeDataValidation.tscn`，覆盖 typed `DataKey<T>` lifecycle、`DataCatalog` resolve、modifier/computed dirty、category reset 和 Data-to-Event bridge artifact。

`verify-ai-first-runtime-refactor-scenes` 正在补齐第一轮 AI-first Runtime 重构的独立 Godot headless 验证场景：框架侧新增 `res://SlimeAI/Src/Validation/Runtime/Entity/RuntimeEntityValidation.tscn`、`Runtime/Lifecycle/RuntimeLifecycleValidation.tscn`、`Runtime/World/RuntimeWorldValidation.tscn`、`Runtime/CommandBuffer/RuntimeCommandBufferValidation.tscn`，并继续把既有 `Runtime/Event/RuntimeEventValidation.tscn` 纳入 P3 验证门禁；BrotatoLike 游戏侧新增 `res://Src/Validation/Game/Input/BrotatoLikeInputEventValidation.tscn` 验证 game-side input events。每个场景必须写 `SceneValidationSession` artifact、固定 PASS/FAIL marker、逐项 `checks` 和 `failureReasons`。P5 skill/docs 变更不新增 Godot 场景，使用 OpenSpec strict validate 和 DocsAI consistency 覆盖。

## 下一步

1. 第一轮 AI-first Runtime 重构已收尾；后续 `IRuntimeSystem -> IRuntimeProcess`、`SystemConfig + SystemDescriptor + SystemRuntimeInfo` 合并等 backlog 必须新建 OpenSpec change，不得混入普通功能任务。
2. 继续扩大 DataOS 迁移范围：旧 DataNew 剩余字段、剩余被动 Feature actions，以及尚未接线的具体 Ability / Feature handler 参数。
3. 继续把 BrotatoLike 真实 UI、SpawnSystem 专项场景和更细的输入专项测试接入统一 Godot scene runner 和 Observation artifact；普通主场景可玩切片和 smoke 已有 PASS artifact。
4. 推进 DataOS snapshot 到真实 UI 和更多游戏场景内容；真实主场景 / 生成系统入口已有第一批 DataOS 驱动证据。
5. 建立 Godot 引擎 trace 计划，源码入口是 `/home/slime/Code/SlimeAI/Resources/Engine/Engine/godot-4.6.2-stable`。
6. 将后续框架级计划按 `Workspace/SystemAgent/Protocols/OpenSpecChangeProtocol.md` 创建 OpenSpec change，再进入实现。

## 风险

- P1 `refactor-runtime-relationship-as-lifecycle-tree` 已完成并 archived：删除 `Runtime/Relationship/` 目录，新增 `Runtime/Entity/LifecycleTree.cs`、`LifecycleLink.cs`、`EntityIdList.cs`、`OwnedReferenceDescriptor.cs`、`IOwnedReferenceCleaner.cs`、`RuntimeOwnedReferenceRegistry.cs`；`EntitySpawnConfig` 简化为 `EntityId / DataCatalog / ParentEntityId / ParentDestroyPolicy`；`Projectile / Effect / Ability` capability 在 `Initialize` 处注册 owner cleanup hook，`SpawnedProjectileIds / SpawnedEffectIds / OwnedAbilityIds` 改为 `DataKey<EntityIdList>`；`GodotNodeRegistry` 内部 entity→adapter 索引替代旧 `RelationshipType.EntityToComponent`。框架仓、BrotatoLike 游戏仓、Godot smoke、旧输入仓 build 和 OpenSpec spec validate 均已完成验证；baseline 已合入 `openspec/specs/runtime-relationship-lifecycle/`、`runtime-business-entity-references/` 与 `observation-contract/`。
- P2b `refactor-runtime-world-facade` 已完成并 archived：新增 `RuntimeWorld.Default` eager singleton 和 `RuntimeWorld.CreateScoped()` sandbox；`EntityManager / LifecycleTree / WorldEvents.World / ResourceCatalog / ObjectPoolManager` static facade 继续保留并转发到 `Default`；P4 后 scoped world dispose 顺序补齐为 `Schedule -> Commands -> Pools -> Resources -> Lifecycle -> Entities -> Events`，`Default.Dispose()` 明确抛错，disposed 后 subsystem getter 抛 `ObjectDisposedException`。框架仓、BrotatoLike build、Godot smoke 和 OpenSpec specs strict validate 均已完成验证；baseline 已合入 `openspec/specs/runtime-world-container/spec.md`，P4 继续扩展该基线。
- P3 `refactor-runtime-events-purge-game-leakage` 已完成并 archived：框架 `Runtime/Events` 删除 BrotatoLike-specific Bucket A 死代码事件 `MouseSelection* / Wave* / GameStart / GameOver / GamePause / GameResume`；Bucket B 主动技能输入事件迁到 `Games/BrotatoLike/Src/Game/Event/BrotatoLikeInputEvents.cs`，输入桥接迁为 `Games/BrotatoLike/Src/Game/Bridge/BrotatoLikePlayerInputComponent.cs`；框架仓 `rg -n "using BrotatoLike" --type cs SlimeAI/` 输出为空；baseline 已合入 `openspec/specs/runtime-event-game-leakage-cleanup/spec.md` 和 `openspec/specs/runtime-event-system/spec.md`。
- Godot Node 对象池碰撞隔离已有第一版 API，并已通过 BrotatoLike Godot headless smoke；PhysicsServer2D trace 尚未接入。
- 当前碰撞隔离已覆盖 `CollisionObject2D` 根节点脱树、泊车、layer/mask 清零、Area2D monitoring/monitorable 和 CollisionShape/Polygon 禁用；PhysicsServer2D trace 尚未接入。
- LifecycleTree / Schedule 已迁入纯 C# 最小内核；ScheduleDataKeys 和 BrotatoLike 第一批系统配置 / 预设 / Spawn config 已进入 DataOS snapshot，但尚未接完整 RuntimeSchedule 自动装载。
- Movement Capability 已覆盖旧项目 `None / Charge / Orbit / SineWave / BezierCurve / Boomerang / AttachToHost / PlayerInput / AIControlled / Parabola / CircularArc` 纯运行时策略，并完成运行时圆形扫描版运动碰撞、同帧多命中派发、目标查询注入、Godot Physics broadphase、Godot 朝向输出第一批和 DataOS 可写入的 handler authoring DataKey；BrotatoLike 已接入 SineWave / Orbit / Boomerang / Bezier / CircularArc / AttachToHost / Charge 从 DataOS 组装 `MovementParams` 的执行路径，真实输入 UI 和更多游戏侧宿主内容仍未迁入。
- Collision Capability 已迁入旧碰撞层常量、`CollisionDataKeys`、`CollisionFilterPolicy`、`CollisionContact`、`CollisionSystem`、`Capabilities.Collision.Events`、`GodotAreaEntity2D`、`GodotCollisionBridge`、`GodotCollisionComponent` 和 `GodotHurtboxComponent` 第一批；ContactDamage bridge 已接入 DamageService。
- Damage Capability 已迁入 `DamageDataKeys`、`DamageType`、`DamageTags`、`DamageInfo`、`DamageService`、`HealService`、`DamageTool`、`IDamageProcessor`、默认处理器优先级、基础检查、闪避、暴击、护盾、护甲、受伤倍率、生命值结算、吸血、统计处理器、`Capabilities.Damage.Events` 和 Godot `GodotContactDamageComponent` 第一批；SystemCore 门禁尚未迁入。
- Ability Capability 已迁入 `AbilityDataKeys`、`AbilityType`、`AbilityTriggerMode`、`AbilityTargetSelection`、`AbilityTriggerResult`、`AbilityCastContext`、`AbilityExecutedResult`、`AbilityAutoTargetOptions`、`AbilityTargetingTool`、`AbilityService` 和 `Capabilities.Ability.Events` 最小切片；当前覆盖显式目标正式触发、点选目标正式触发、启用 / 执行中 / 冷却 / 充能检查、冷却 Tick、`AbilityTriggerMode.Periodic` 自动触发 Tick、显式自动索敌上下文准备、通过 `DamageTool` 造成即时或周期伤害，并可通过 `AbilityDataKeys.FeatureHandlerId` 调用 Feature handler。
- Feature Capability 已迁入 `FeatureDataKeys`、`FeatureDefinition`、`FeatureModifierEntry`、`FeatureContext`、`FeatureEndReason`、`IFeatureHandler`、`FeatureHandlerRegistry`、`FeatureService` 和 `Capabilities.Feature.Events` 最小生命周期；当前覆盖授予 / 移除 Modifier、启用 / 禁用、激活 / 执行 / 结束、handler 生命周期和 AbilityService 可选接入；Feature actions、自动触发、点选、Projectile / Effect、AI 和 DataOS authoring 适配尚未迁入。
- Attack Capability 已迁入 `AttackDataKeys`、`AttackState`、`AttackTriggerResult`、`AttackTriggerReport`、`AttackCancelReason`、`AttackService` 最小 Runtime、`GodotAttackComponent` bridge 第一段、`AttackComponent` 旧类名兼容包装、`UnitDataKeys`、`Capabilities.Unit.Events` 和 `GodotUnitAnimationComponent` 动画事件桥第二段，以及旧 Attack 动画选择兼容第一段；当前覆盖消费 `Capabilities.Attack.Events.Requested`、前摇 / 后摇 / 冷却 Timer、距离 / 死亡 / 可攻击门禁、通过 `DamageService` 造成 `DamageTags.Attack` 伤害，并发布 Started / Finished / Cancelled 事件；Godot bridge 可注册服务、写入导出参数、保留注册前已有 Attack Data、把节点目标映射为 Runtime 攻击请求，把 Attack Started / Cancelled 转为 AnimatedSprite2D 播放 / 停止请求，缓存可用动画，在配置动画不存在时从 `attack*` 可用动画中选择，并在一次性动画完成后发布 `Capabilities.Unit.Events.AnimationFinished` 回 idle。
- AI Capability 已迁入 `AIDataKeys`、`AIContext`、`AIState`、`AIService`、`BehaviorNode`、`SequenceNode`、`SelectorNode`、`FindNearestTargetAction`、`MoveToTargetAction`、`IsTargetInRangeCondition`、`RequestAttackAction`、`PrepareAbilityAutoTargetContextsAction`、`TickAbilityAutoTriggersAction`、`PatrolAction`、`EnemyBehaviorBlocks`、`EnemyBehaviorTreeBuilder`、`GodotAIComponent` 和 `GodotAIBehaviorTreeKind`；当前覆盖启用 / 死亡门禁、Runtime Entity 快照最近目标查询、同队 / 死亡 / 距离过滤、向目标写入 `MovementDataKeys.AIMoveDirection / AIMoveSpeedMultiplier`、范围内发出 `Capabilities.Attack.Events.Requested` 并停步面向目标、确定性左右巡逻和等待倒计时、标准近战树攻击优先 / 追逐 / 巡逻回退、Godot 组件导出参数写入和手动 / `_Process` Tick，通过 AbilityTargetingTool 准备自动索敌施法上下文，以及通过 AbilityService 推进 Periodic 自动施法。
- Runtime Data 已切换到 typed contract：业务访问入口是 `DataKey<T>`，`Data` 绑定 frozen `DataCatalog` 并使用 typed slot 存储，旧 string/object/DataMeta/DataRegistry 正式入口已移除；DataOS descriptor 持有 authoring metadata 和 default mirror，snapshot loader 必须先校验 manifest/descriptors/catalog 后再按 resolved `DataKey<T>` typed apply。DataOS schema / migration / generator / validator / Runtime snapshot loader 已创建 typed snapshot 闭环，validator 会阻断 descriptor missing/extra、type/default drift、disabled capability 未裁剪、wrong snapshot type 和 unknown key；当前覆盖 BrotatoLike 玩家 / 敌人 / TargetingIndicator / Ability / ChainAbility / Ability handler-specific 参数第三段 / Feature definition / modifier / 系统配置 / 预设 / Spawn config / 资源路径第一批，尚未覆盖旧 DataNew 全量字段和编辑器 authoring UI。
- 玩家输入系统已迁入游戏侧：`BrotatoLikePlayerInputComponent`（`Games/BrotatoLike/Src/Game/Bridge/`）读取 Godot Input Map 的 MoveLeft/Right/Up/Down、UseSkill、PreviousSkill、NextSkill，将移动方向写入 `MovementDataKeys.InputDirection`，并发布 game-side `InputUseSkill / InputPreviousSkill / InputNextSkill` 到 Entity EventBus；支持 `CanMoveInput` 门禁和 `Movement.Acceleration` 平滑移动插值。`GodotActiveSkillInputComponent`（游戏侧）订阅技能输入事件，管理 `AbilityDataKeys.OwnedAbilityIds` 和 `CurrentAbilityIndex`，通过 `AbilityTargetingTool` 自动索敌后调用 `AbilityService.TryTrigger`。BrotatoLike `SpawnPlayer` 已接入：生成玩家时从 DataOS 创建 slam / chain_lightning 初始技能实体，挂载双输入组件，启动 PlayerInput 移动策略；框架 GodotBridge 不再持有 BrotatoLike-specific 输入组件。

## 最新验证

```bash
Tools/run-build.sh
Tools/run-tests.sh
Tools/run-dataos-validate.sh
cd /home/slime/Code/SlimeAI/Games/BrotatoLike
Tools/run-build.sh
Tools/run-godot-scene.sh run res://SlimeAI/Src/Validation/Runtime/Entity/RuntimeEntityValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
Tools/run-godot-scene.sh run res://SlimeAI/Src/Validation/Runtime/Lifecycle/RuntimeLifecycleValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
Tools/run-godot-scene.sh run res://SlimeAI/Src/Validation/Runtime/World/RuntimeWorldValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
Tools/run-godot-scene.sh run res://SlimeAI/Src/Validation/Runtime/Event/RuntimeEventValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
Tools/run-godot-scene.sh run res://SlimeAI/Src/Validation/Runtime/CommandBuffer/RuntimeCommandBufferValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
Tools/run-godot-scene.sh run res://Src/Validation/Game/Input/BrotatoLikeInputEventValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
Tools/run-godot-scene.sh run-main-smoke --log-dir .ai-temp/scene-tests/runs
Tools/analyze-godot-scene-logs.sh
```

### P2a typed EntityId 实施验证

### AI-first Runtime refactor 场景验证（2026-05-15）

```bash
cd /home/slime/Code/SlimeAI
openspec validate verify-ai-first-runtime-refactor-scenes --strict
openspec validate --specs --strict
```

结果：active change strict validate PASS；baseline specs strict validate 输出 `28 passed, 0 failed`。

```bash
cd /home/slime/Code/SlimeAI/SlimeAI
Tools/run-build.sh
Tools/run-tests.sh
```

结果：框架 build PASS（0 errors；既有 XML comment warnings），Runtime tests 全部 PASS。

```bash
cd /home/slime/Code/SlimeAI/Games/BrotatoLike
Tools/run-build.sh
Tools/run-godot-scene.sh run res://SlimeAI/Src/Validation/Runtime/Entity/RuntimeEntityValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
Tools/run-godot-scene.sh run res://SlimeAI/Src/Validation/Runtime/Lifecycle/RuntimeLifecycleValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
Tools/run-godot-scene.sh run res://SlimeAI/Src/Validation/Runtime/World/RuntimeWorldValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
Tools/run-godot-scene.sh run res://SlimeAI/Src/Validation/Runtime/Event/RuntimeEventValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
Tools/run-godot-scene.sh run res://SlimeAI/Src/Validation/Runtime/CommandBuffer/RuntimeCommandBufferValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
Tools/run-godot-scene.sh run res://Src/Validation/Game/Input/BrotatoLikeInputEventValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
Tools/run-godot-scene.sh run-main-smoke --log-dir .ai-temp/scene-tests/runs
Tools/analyze-godot-scene-logs.sh
```

结果：BrotatoLike build PASS（0 warnings / 0 errors）；新增和既有专项场景均输出 PASS marker，`run-main-smoke` 输出 `BrotatoLike GameOS smoke PASS`，analyzer 输出 `status: pass`、`firstError: none`。artifact 入口：

- Runtime/Entity：`.ai-temp/scene-tests/runs/2026-05-15/18-32-43/index.json`
- Runtime/Lifecycle：`.ai-temp/scene-tests/runs/2026-05-15/18-32-52/index.json`
- Runtime/World：`.ai-temp/scene-tests/runs/2026-05-15/18-32-59/index.json`
- Runtime/Event：`.ai-temp/scene-tests/runs/2026-05-15/18-33-06/index.json`
- Runtime/CommandBuffer：`.ai-temp/scene-tests/runs/2026-05-15/18-33-18/index.json`
- BrotatoLike Game/Input：`.ai-temp/scene-tests/runs/2026-05-15/18-36-50/index.json`
- Main smoke：`.ai-temp/scene-tests/runs/2026-05-15/18-36-58/index.json`

静态边界验证：`rg -n "using BrotatoLike" --type cs SlimeAI/` 无匹配；`rg -n "InputUseSkill|InputPreviousSkill|InputNextSkill|MouseSelection|WaveStarted|GameStart" --type cs SlimeAI/GameOS/Runtime/Events SlimeAI/GameOS/GodotBridge` 无匹配。

P5 skill/docs 变更未新增 Godot 场景，已由 OpenSpec strict validate 和 DocsAI consistency 覆盖。

### P2a typed EntityId 实施验证

```bash
cd /home/slime/Code/SlimeAI/SlimeAI
Tools/run-build.sh             # PASS
Tools/run-tests.sh             # 83/83 PASS（含 4 个新增 EntityIdTests）
Tools/run-dataos-validate.sh   # PASS
```

### P1 LifecycleTree 框架仓实施验证（2026-05-15）

```bash
cd /home/slime/Code/SlimeAI/SlimeAI
Tools/run-build.sh             # PASS（0 errors / 0 warnings）
Tools/run-tests.sh             # 85/85 PASS（含 LifecycleTree、EntityIdList、RuntimeOwnedReferenceRegistry 新测试）
```

```bash
cd /home/slime/Code/SlimeAI/Games/BrotatoLike
Tools/run-build.sh             # PASS（0 errors / 0 warnings）
Tools/run-godot-scene.sh run res://Scenes/Main.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
Tools/run-godot-scene.sh run-main-smoke --log-dir .ai-temp/scene-tests/runs
Tools/analyze-godot-scene-logs.sh
```

结果：普通主场景 playable slice 输出 `BrotatoLike playable slice PASS`，artifact 位于 `.ai-temp/scene-tests/runs/2026-05-15/09-40-30/index.json`；`run-main-smoke` 输出 `BrotatoLike GameOS smoke PASS`，analyzer 输出 `status: pass`、`firstError: none`，artifact 位于 `.ai-temp/scene-tests/runs/2026-05-15/09-47-27/index.json`。

```bash
cd /home/slime/Code/SlimeAI
openspec archive refactor-runtime-relationship-as-lifecycle-tree -y
openspec validate --specs --strict
```

结果：P1 delta specs 已合入基线；`openspec validate --specs --strict` 输出 `24 passed, 0 failed`。

### P2b RuntimeWorld facade 实施验证（2026-05-15）

```bash
cd /home/slime/Code/SlimeAI/SlimeAI
Tools/run-build.sh             # PASS（0 errors / 0 warnings）
Tools/run-tests.sh             # PASS（含 RuntimeWorld default / scoped isolation / dispose 顺序测试）
```

```bash
cd /home/slime/Code/SlimeAI/Games/BrotatoLike
Tools/run-build.sh             # PASS（0 errors / 0 warnings）
Tools/run-godot-scene.sh run-main-smoke --log-dir .ai-temp/scene-tests/runs
Tools/analyze-godot-scene-logs.sh
```

结果：`run-main-smoke` 输出 `BrotatoLike GameOS smoke PASS`，`bridge:True pool:True dataos:True main:True`；analyzer 输出 `status: pass`、`firstError: none`，artifact 位于 `.ai-temp/scene-tests/runs/2026-05-15/13-55-38/index.json`。

```bash
cd /home/slime/Code/SlimeAI
openspec archive refactor-runtime-world-facade -y
openspec validate --specs --strict
```

结果：P2b delta spec 已合入 `openspec/specs/runtime-world-container/spec.md`；`openspec validate --specs --strict` 输出 `25 passed, 0 failed`。

### P3 Runtime events leakage cleanup 实施验证（2026-05-15）

```bash
cd /home/slime/Code/SlimeAI/SlimeAI
Tools/run-build.sh             # PASS（0 errors；既有 XML 注释 warnings）
Tools/run-tests.sh             # PASS
```

```bash
cd /home/slime/Code/SlimeAI/Games/BrotatoLike
Tools/run-build.sh             # PASS（0 errors；既有 XML 注释 warnings）
Tools/run-godot-scene.sh run-main-smoke --log-dir .ai-temp/scene-tests/runs
Tools/analyze-godot-scene-logs.sh
```

结果：`run-main-smoke` 输出 `BrotatoLike GameOS smoke PASS`，`bridge:True pool:True dataos:True main:True`；analyzer 输出 `status: pass`、`firstError: none`，artifact 位于 `.ai-temp/scene-tests/runs/2026-05-15/14-39-45/index.json`。

```bash
cd /home/slime/Code/SlimeAI
openspec validate refactor-runtime-events-purge-game-leakage --strict
openspec archive refactor-runtime-events-purge-game-leakage -y
openspec validate --specs --strict
```

结果：P3 delta specs 已合入 `openspec/specs/runtime-event-game-leakage-cleanup/spec.md` 和 `openspec/specs/runtime-event-system/spec.md`；`openspec validate --specs --strict` 输出 `26 passed, 0 failed`。

### P4 Runtime CommandBuffer + Phase 实施验证（2026-05-15）

```bash
cd /home/slime/Code/SlimeAI/SlimeAI
Tools/run-build.sh             # PASS（0 errors / 0 warnings）
Tools/run-tests.sh             # PASS（含 RuntimeCommandBuffer / SchedulePhase / StructuralChangeGuard tests）
```

```bash
cd /home/slime/Code/SlimeAI/Games/BrotatoLike
Tools/run-build.sh             # PASS（0 errors；XML 注释 warnings 仍存在）
Tools/run-godot-scene.sh run-main-smoke --log-dir .ai-temp/scene-tests/runs
Tools/analyze-godot-scene-logs.sh
```

结果：`run-main-smoke` 输出 `BrotatoLike GameOS smoke PASS`，`bridge:True pool:True dataos:True main:True`；analyzer 输出 `status: pass`、`firstError: none`，artifact 位于 `.ai-temp/scene-tests/runs/2026-05-15/16-45-27/index.json`。

```bash
cd /home/slime/Code/SlimeAI
openspec validate refactor-runtime-command-buffer-with-phases --strict
openspec validate --specs --strict
```

结果：P4 delta specs 已合入 `openspec/specs/runtime-command-buffer/spec.md` 和 `openspec/specs/runtime-schedule-phase/spec.md`；`openspec validate --specs --strict` 输出 `27 passed, 0 failed`。

### P5 ai-feature-development skill enhancement 验证（2026-05-15）

```bash
cd /home/slime/Code/SlimeAI
openspec validate enhance-ai-feature-development-skill --strict
openspec list --json
find .ai-config/skills/ai/ai-feature-development/references -type f -name '*.md' | sort
bash Workspace/Tools/ai-config-sync/sync-ai-config.sh
openspec archive enhance-ai-feature-development-skill -y
openspec validate --specs --strict
```

结果：P5 delta spec 已合入 `openspec/specs/ai-feature-development-skill/spec.md`；`openspec validate --specs --strict` 输出 `28 passed, 0 failed`。`ai-feature-development` 统一源和 `.codex/.claude/.windsurf` 副本均包含 11 个 reference 文件；本变更为 skill / DocsAI / OpenSpec 文档改动，未触及框架代码，未运行框架 build/tests。

结果：Runtime 行为测试全部 PASS，DataOS schema / descriptor / manifest 验证 PASS；BrotatoLike build PASS，`run-main-smoke` 输出 `BrotatoLike GameOS smoke PASS`，analyzer 输出 `status: pass`、`firstError: none`。typed loader 曾捕获 `Movement.OrbitTotalAngle` descriptor default drift，修正 seed mirror 后通过。

事件系统基线补充验证：`Tests/SlimeAI.GameOS.Tests/Events/EventBusTests.cs` 覆盖同类型 reentry 阻断、跨类型级联、跨 bus 同类型事件、handler 异常隔离、Dispose 退订、注册顺序、Broadcast 路由和 scope 越界拒绝；`Tools/run-tests.sh` 全部 PASS。BrotatoLike `run-main-smoke` 在 `.ai-temp/scene-tests/runs/2026-05-13/09-23-37/.../artifacts/eventbus-dump.json` 导出 world bus dump，`jq '.SameTypeReentryBlockedCounts, .HandlerExceptions'` 输出 `{}` 和 `[]`；普通 `Scenes/Main.tscn` acceptance 最新通过 artifact 为 `.ai-temp/scene-tests/runs/2026-05-13/09-24-15/index.json`。

Runtime/Data 专项 Godot 验证补充：当前初始开发阶段默认选 BrotatoLike 作为承载游戏，已临时同步其 `SlimeAI/` submodule 工作树中的 `Src/Validation/Runtime/Data` 和 `Src/Validation/Runtime/Data`，并通过 `BrotatoLike.csproj` 重新包含 `SlimeAI/Src/Validation/**/*.cs`。后续多游戏 / 成品阶段不默认同步所有游戏，改按每个游戏的框架版本策略更新 submodule 指针。`Tools/run-godot-scene.sh list` 已能列出 `res://SlimeAI/Src/Validation/Runtime/Data/RuntimeDataValidation.tscn`；`Tools/run-godot-scene.sh run res://SlimeAI/Src/Validation/Runtime/Data/RuntimeDataValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs && Tools/analyze-godot-scene-logs.sh` 输出 `GameOS Runtime Data validation PASS`，artifact 位于 `.ai-temp/scene-tests/runs/2026-05-13/15-47-30/index.json`。
