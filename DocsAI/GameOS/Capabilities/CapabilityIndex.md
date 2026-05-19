# GameOS Capability 索引

> 状态：routing-index  
> 更新日期：2026-05-13  
> 范围：AI 任务路由、Capability 所有权、状态规范、验证证据。

## 定位

本索引是 GameOS Capability 工作的第一路由入口。它告诉 AI 或开发者：某个 Capability 归哪个 skill 所有，应该先读哪些 contract / debug 文档，涉及哪些 DataKeys 和 Events，GodotBridge 边界在哪里，以及需要哪些验证证据。

本文件不是 runtime registry，不改变 public API。`capability.json` 仍是本地 metadata 来源；本索引用来补充跨 Capability 的路由和证据规则。

Capability 是 `Capability Composition Runtime` 的玩法组合和所有权单元。新增或修改 gameplay 行为时，优先路由到对应 Capability 的 service、tool、handler、DataKey、Event、selector 和测试，不新增泛型 `Component` 或绕过 owner 的 `System`。`Godot*Component` 是 GodotBridge Adapter 的 legacy compatibility name；`MovementSystem`、`CollisionSystem` 等现有类名是 Runtime Process / Schedule Process 的兼容符号。

## 状态规则

规范状态：

| Status | 含义 |
| --- | --- |
| `Draft` | 只有设计或部分 artifact，尚不能进入默认 profile 路由。 |
| `Experimental` | 框架实现、测试或 smoke 证据存在，但缺少被接受的 profile 或 playable game-slice evidence。 |
| `Supported` | Contract、Debug、Tests，以及至少一个 profile 或 game-slice evidence 全部存在。 |
| `Deprecated` | 保留兼容，不建议新代码使用。 |

`stable`、`migrated`、`bootstrap`、`active`、`experimental` 等 legacy status 只作为来源证据记录，不能自动映射为 `Supported`。

当前首批决策：所有列出的 Capability 均为 `Experimental`，直到 `Survivor2D` profile 和 BrotatoLike playable-slice acceptance 提供明确 game-slice evidence。

## 验证基线

框架验证：

```bash
cd /home/slime/Code/SlimeAI/SlimeAI
Tools/run-build.sh
Tools/run-tests.sh
Tools/run-dataos-validate.sh
```

当 Capability 触及 GodotBridge 时，BrotatoLike smoke 或 scene evidence 的参考命令：

```bash
cd /home/slime/Code/SlimeAI/Games/BrotatoLike
Tools/run-build.sh
Tools/run-godot-scene.sh run res://Scenes/Main.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
Tools/run-godot-scene.sh run-main-smoke --log-dir .ai-temp/scene-tests/runs
Tools/analyze-godot-scene-logs.sh
```

`Tools/run-godot-smoke.sh` 只作为兼容入口；新证据优先使用统一 scene runner 和 analyzer。

## Typed DataKey / Descriptor 路由

- Capability Runtime DataKeys 必须暴露为 `DataKey<T>` handles，例如 `DamageDataKeys.CurrentHp`；业务代码只调用 `Data.Get/Set/TryGet/Has/Remove(DataKey<T>)`。
- stable key string 只允许在 DataOS、snapshot、manifest、observation、debug dump 和 catalog resolve 边界出现，不作为 gameplay write API。
- authoring metadata 属于 DataOS `data_key_descriptor`，包括 display name、description、icon、owner skill、range/options、default mirror 和 modifier/computed 标记。
- 每个 descriptor 必须有明确 `ownerCapability` 和 `ownerSkill`；游戏专属 descriptor 留在游戏 seed/profile，不上提为框架默认。
- DataOS generator / validator / loader 必须验证 descriptor coverage、type/default drift、capability dependencies、disabled capability trimming 和 resource ownership。
- 修改任何 Capability DataKey 后，必须同步 DataOS descriptor mirror 或验证会在 `Tools/run-dataos-validate.sh`、`Tools/run-tests.sh`、BrotatoLike build/smoke 中失败。
- 如果任务使用 `ECS / Entity / Component / System` 词汇，先回到 `GameOS/Overview.md#术语表`：`Component` 默认不是 gameplay/data owner，`System` 默认不是 ECS query processor，`Entity` 默认是 Runtime Entity 身份容器。

## Capability 必含文件

每个 Capability 目录（`SlimeAI/GameOS/Capabilities/<Cap>/`）MUST 包含：

- `capability.json`：Capability metadata。
- `Contract.md`（放在 `DocsAI/GameOS/Capabilities/<Cap>/`）：Capability 契约，含事件目录指针。
- `Debug.md`、`Tests.md`：调试和测试入口。
- `Events/`：事件 payload 目录（一文件一 `readonly record struct`，实现 `IEntityEvent / IGlobalEvent / IBroadcastEvent`）。Capability 无事件时允许不建。

## Capability 条目

### Movement

| 字段 | 内容 |
| --- | --- |
| Capability id | `movement` |
| Metadata | `SlimeAI/GameOS/Capabilities/Movement/capability.json` |
| Legacy status source | `capability.json` 中为 `experimental`；`ApiIndex.md` 中存在 `migrated/bootstrap` 行 |
| Normalized status | `Experimental` |
| Owner skill | `movement-system` |
| Contract / Debug | `GameOS/Capabilities/Movement/Contract.md`；`GameOS/Capabilities/Movement/Debug.md` |
| Primary APIs | `MovementSystem`、`MovementDataKeys`、`MovementParams`、`IMovementStrategy`、`MovementStrategyRegistry`、`MovementCollisionPolicy` |
| DataKeys | Position、Velocity、FacingDirection、IsMoving、MoveSpeed、Acceleration、InputDirection、AIMoveDirection、AIMoveSpeedMultiplier、CanMoveInput、LastMoveDirection、handler movement params、wave/orbit/boomerang/bezier/parabola/circular-arc params |
| Events | `Movement.Started`、`Movement.Stopped`、`Movement.Collided` |
| Selector owner | Movement 持有 `activeMovements` 集合（`Tick()` 只遍历活跃移动实体，不调 `EntityManager.GetAll()`）；碰撞候选通过 `IMovementCollisionTargetQuery` 注入；玩法目标选择仍归 AI 或 Ability。 |
| RuntimeSchedule boundary | 每帧 movement tick；调用方负责 schedule 注册和 delta 来源。 |
| GodotBridge boundary | `GodotMovementDriver`、`GodotOrientationComponent`、`GodotPlayerInputComponent`、`GodotPhysicsMovementCollisionTargetQuery` 把 Node2D / input / physics 同步到 Runtime data。 |
| Dependencies | Runtime Entity/Data/Event；Collision 用于过滤；GodotBridge 用于场景同步。 |
| Validation commands | `Tools/run-build.sh`；`Tools/run-tests.sh`；`Tools/run-dataos-validate.sh`；Godot bridge 触及时运行 BrotatoLike `Tools/run-build.sh`、`Tools/run-godot-scene.sh run-main-smoke --log-dir .ai-temp/scene-tests/runs`、`Tools/analyze-godot-scene-logs.sh`。 |
| Game-slice evidence | BrotatoLike smoke 已覆盖 PlayerInput、AIControlled、轨迹、朝向和 physics broadphase；普通 playable-slice PASS/FAIL evidence 仍待补。 |
| Gaps | 在普通 `Scenes/Main.tscn` 下记录 Survivor2D/BrotatoLike playable-slice acceptance 前，不标记为 `Supported`。 |

### Collision

| 字段 | 内容 |
| --- | --- |
| Capability id | `collision` |
| Metadata | `SlimeAI/GameOS/Capabilities/Collision/capability.json` |
| Legacy status source | `capability.json` 中为 `experimental`；API index 中存在 `bootstrap/migrated` 行 |
| Normalized status | `Experimental` |
| Owner skill | `collision-system` |
| Contract / Debug | `GameOS/Capabilities/Collision/Contract.md`；`GameOS/Capabilities/Collision/Debug.md` |
| Primary APIs | `CollisionDataKeys`、`CollisionLayers`、`CollisionFilterPolicy`、`CollisionSystem`、`CollisionContact` |
| DataKeys | CollisionLayer、CollisionMask、Team、CollisionRadius |
| Events | `Collision.Entered`、`Collision.Exited`、`Collision.Contact` |
| Selector owner | Collision 拥有 layer/mask/team 过滤；目标意图仍归 AI、Ability 或 Projectile。 |
| RuntimeSchedule boundary | Collision event 由 bridge/system 驱动；消费者不能绕过 Damage 或 Attack services。 |
| GodotBridge boundary | `GodotAreaEntity2D`、`GodotCollisionBridge`、`GodotCollisionComponent`、`GodotHurtboxComponent`、`GodotContactDamageComponent`、collision isolation。 |
| Dependencies | Runtime Entity/Data/Event；Movement 用于 movement-collision scans；Damage 用于 contact damage conversion。 |
| Validation commands | `Tools/run-build.sh`；`Tools/run-tests.sh`；`Tools/run-dataos-validate.sh`；Godot bridge 触及时运行 BrotatoLike build / main smoke / analyzer。 |
| Game-slice evidence | Smoke 曾覆盖 runtime collision events 和 Godot hurtbox/contact bridge；main-scene playable collision acceptance 待补。 |
| Gaps | 需要 Observation selector/collision dump 和 playable-slice collision evidence 后才能考虑 `Supported`。 |

### Damage

| 字段 | 内容 |
| --- | --- |
| Capability id | `damage` |
| Metadata | `SlimeAI/GameOS/Capabilities/Damage/capability.json` |
| Legacy status source | `capability.json` 中为 `experimental`；API index 中存在 `bootstrap` 行 |
| Normalized status | `Experimental` |
| Owner skill | `damage-system` |
| Contract / Debug | `GameOS/Capabilities/Damage/Contract.md`；`GameOS/Capabilities/Damage/Debug.md` |
| Primary APIs | `DamageService`、`HealService`、`DamageTool`、`DamageInfo`、`DamageResult`、`IDamageProcessor`、`DamageDataKeys`；`DamageService.Default`（进程级默认入口）和 `DamageService.Instance`（向后兼容别名）语义相同；测试必须用 `new DamageService()` 独立实例 |
| DataKeys | CurrentHp、MaxHp、IsDead、IsInvulnerable、Armor、DodgeChance、CritRate、CritDamage、DamageTakenMultiplier、LifeSteal、Shield、ContactDamage、ContactDamageInterval、total/wave damage、hit、crit、kill、healing、shield statistics |
| Events | 事件目录 `SlimeAI/GameOS/Capabilities/Damage/Events/`：`Damaged / Dodged / Healed / HealthChanged / Killed`（全部 `IBroadcastEvent`）。 |
| Selector owner | `None`；从 Attack、Ability、Projectile 或 contact bridge 接收明确 attacker/target 集合。 |
| RuntimeSchedule boundary | Service call 和 timer-driven periodic damage；调用方负责调度。 |
| GodotBridge boundary | `GodotContactDamageComponent` 把 hurtbox contacts 转为 `DamageService` 请求；bridge 不直接写 HP。 |
| Dependencies | Runtime Entity/Data/Event/Timer；Collision 用于 contact filtering。 |
| Validation commands | `Tools/run-build.sh`；`Tools/run-tests.sh`；`Tools/run-dataos-validate.sh`；contact / projectile bridge 触及时运行 BrotatoLike build / main smoke / analyzer。 |
| Game-slice evidence | Runtime tests 覆盖 processor pipeline、healing、statistics、periodic/multi-target damage；smoke 曾覆盖 contact 和 projectile damage；main-scene death/cleanup evidence 待补。 |
| Gaps | 需要 playable-slice evidence 覆盖 damage、death、cleanup、damage logs 或 damage numbers。 |

### Unit

| 字段 | 内容 |
| --- | --- |
| Capability id | `unit` |
| Metadata | `SlimeAI/GameOS/Capabilities/Unit/capability.json` |
| Legacy status source | `capability.json` 中为 `experimental`；API index 中存在 `bootstrap` 行 |
| Normalized status | `Experimental` |
| Owner skill | DataKeys 和 runtime metadata 归 `ecs-data`；HUD/UI-facing 后续工作使用 `ui-bind`。 |
| Contract / Debug | `GameOS/Capabilities/Unit/Contract.md`；`GameOS/Capabilities/Unit/Debug.md` |
| Primary APIs | `UnitDataKeys` |
| DataKeys | Name、EntityType、DeathType、VisualScenePath、HealthBarHeight、IsShowHealthBar、PickupRange、ExpReward、DetectionRange、AvailableAnimations |
| Events | Unit 自身无主动事件；动画请求/完成使用 `Capabilities.Unit.Events.PlayAnimationRequested / StopAnimationRequested / AnimationFinished`（`IEntityEvent`）。 |
| Selector owner | `None`；Unit metadata 被其他系统消费。 |
| RuntimeSchedule boundary | 首批没有独立 RuntimeSchedule system。 |
| GodotBridge boundary | `GodotUnitAnimationComponent` 消费 Unit animation event requests 和 available animation metadata。 |
| Dependencies | Runtime Data；GodotBridge 用于视觉动画；DataOS 用于 unit authoring records。 |
| Validation commands | `Tools/run-build.sh`；`Tools/run-tests.sh`；`Tools/run-dataos-validate.sh`；DataOS unit 或 animation bridge 触及时运行 BrotatoLike build / main smoke / analyzer。 |
| Game-slice evidence | BrotatoLike smoke 曾覆盖 DataOS player/enemy unit records 和 animation bridge；HUD / health bar playable evidence 待补。 |
| Gaps | Unit 暂无独立 owner skill；不要把游戏专属 unit visuals 或 HUD layout 当成框架默认。 |

### Attack

| 字段 | 内容 |
| --- | --- |
| Capability id | `attack` |
| Metadata | `SlimeAI/GameOS/Capabilities/Attack/capability.json` |
| Legacy status source | `capability.json` 中为 `experimental`；API index 中存在 `bootstrap` 行 |
| Normalized status | `Experimental` |
| Owner skill | `attack-system` |
| Contract / Debug | `GameOS/Capabilities/Attack/Contract.md`；`GameOS/Capabilities/Attack/Debug.md` |
| Primary APIs | `AttackService`、`AttackDataKeys`、`AttackState`、`AttackTriggerReport`、`AttackTriggerResult` |
| DataKeys | Damage、Range、Interval、WindUpTime、RecoveryTime、CanAttack、IsAttacking、State、CooldownRemaining |
| Events | `Attack.Requested`、`Attack.CancelRequested`、`Attack.Started`、`Attack.Completed`、`Attack.Cancelled` |
| Selector owner | `None`；target entity 或 point 必须先由 AI、input 或 game adapter 准备好。 |
| RuntimeSchedule boundary | Event/service driven；windup、recovery 和 cooldown 使用 `TimerManager`。 |
| GodotBridge boundary | `GodotAttackComponent`、legacy `AttackComponent`、`GodotUnitAnimationComponent` animation requests。 |
| Dependencies | Damage 用于伤害结算；Timer；Unit animation events；AI/input 负责请求。 |
| Validation commands | `Tools/run-build.sh`；`Tools/run-tests.sh`；`Tools/run-dataos-validate.sh`；GodotAttackComponent 或 legacy wrapper 触及时运行 BrotatoLike build / main smoke / analyzer。 |
| Game-slice evidence | Runtime tests 覆盖 request consumption、range、cooldown、windup、recovery 和 damage；smoke 曾覆盖 Godot bridge 和 animation selection；普通 main-scene combat acceptance 待补。 |
| Gaps | 需要 playable-slice evidence 覆盖敌人 / 玩家攻击循环和 cleanup。 |

### AI

| 字段 | 内容 |
| --- | --- |
| Capability id | `ai` |
| Metadata | `SlimeAI/GameOS/Capabilities/AI/capability.json` |
| Legacy status source | `capability.json` 中为 `experimental`；API index 中存在 `bootstrap` 行 |
| Normalized status | `Experimental` |
| Owner skill | `ai-system` |
| Contract / Debug | `GameOS/Capabilities/AI/Contract.md`；`GameOS/Capabilities/AI/Debug.md` |
| Primary APIs | `AIService`、`AIContext`、`BehaviorNode`、`EnemyBehaviorBlocks`、`EnemyBehaviorTreeBuilder`、target/action nodes；`AIService.Default`（进程级默认入口）；`AIContext.AbilityService` 无默认值，调用方必须显式注入 |
| DataKeys | IsEnabled、TargetEntity、TargetPosition、HasTargetPosition、IsAttackRequested、AttackRange、PatrolCenter、PatrolRadius、PatrolWaitTime、PatrolTargetPosition、HasPatrolTargetPosition、PatrolWaitRemaining、PatrolDirectionSign |
| Events | `AI.TargetAcquired`、`AI.TargetLost`、`AI.PatrolStarted` |
| Selector owner | AI 持有 `IAITargetQuery`，`FindNearestTargetAction` 通过注入 query 获取候选目标（默认 `RuntimeAITargetQuery`）；Ability targeting 仍归 Ability。 |
| RuntimeSchedule boundary | AI tick 由调用方调度；AI 只写 intent/request DataKeys 和 events，不直接移动或造成伤害。 |
| GodotBridge boundary | `GodotAIComponent` 导出参数并 tick `AIService`；不直接移动 Godot nodes。 |
| Dependencies | Movement 用于移动意图；Attack 用于攻击请求；Ability 用于 auto-trigger contexts；Damage 用于死亡门禁。 |
| Validation commands | `Tools/run-build.sh`；`Tools/run-tests.sh`；`Tools/run-dataos-validate.sh`；Godot AI bridge 触及时运行 BrotatoLike build / main smoke / analyzer。 |
| Game-slice evidence | Runtime tests 覆盖 target finding、patrol、melee behavior、attack request、ability auto-target context；smoke 曾覆盖 Godot AI bridge；main-scene enemy chase evidence 待补。 |
| Gaps | 需要 playable-slice evidence 覆盖 wave enemy chase 和 attack behavior。 |

### Ability

| 字段 | 内容 |
| --- | --- |
| Capability id | `ability` |
| Metadata | `SlimeAI/GameOS/Capabilities/Ability/capability.json` |
| Legacy status source | `capability.json` 中为 `experimental`；API index 中存在 `bootstrap` 行 |
| Normalized status | `Experimental` |
| Owner skill | `ability-system` |
| Contract / Debug | `GameOS/Capabilities/Ability/Contract.md`；`GameOS/Capabilities/Ability/Debug.md` |
| Primary APIs | `AbilityService`、`AbilityDataKeys`、`AbilityCastContext`、`AbilityTriggerReport`、`AbilityTargetingTool` |
| DataKeys | Name、Type、TriggerMode、TargetSelection、AutoTargetRange、AutoTargetMaxTargets、AutoTargetIgnoreSameTeam、AutoTargetRequiresDamageable、FeatureHandlerId、FeatureGroupId、Description、IconPath、Level、MaxLevel、CostType、CostAmount、ChargeTime、CastRange、EffectRadius、chain fields、LineEffectScenePath、IsEnabled、IsActive、Cooldown、CooldownRemaining、charge fields、Damage、DamageInterval、DamageRepeatCount、ApplyImmediateDamage |
| Events | `Ability.Executed`、`Ability.Failed`、`Ability.CooldownStarted`、`Ability.CooldownFinished` |
| Selector owner | Ability 持有 `IAbilityTargetQuery`，`AbilityTargetingTool` 使用注入 query 获取候选目标（默认 `RuntimeAbilityTargetQuery`）。 |
| RuntimeSchedule boundary | Manual/event/periodic trigger boundary；periodic auto-trigger tick 由调用方调度。 |
| GodotBridge boundary | 暂无通用 Ability bridge；BrotatoLike game handlers 通过 `BrotatoLikeAbilityHandlers` 适配游戏侧技能。 |
| Dependencies | Damage 用于 ability damage；Feature 用于 handler execution；Projectile/Effect/Movement 用于 game-side handler outputs；AI 可准备 auto-trigger contexts。 |
| Validation commands | `Tools/run-build.sh`；`Tools/run-tests.sh`；`Tools/run-dataos-validate.sh`；selected game-side ability handler paths 触及时运行 BrotatoLike build / main smoke / analyzer。 |
| Game-slice evidence | Runtime tests 覆盖 target semantics、cooldown、charges、periodic damage、periodic auto-trigger 和 auto-targeting；smoke 曾覆盖多个 BrotatoLike handlers，但至少两个旧技能的 playable-slice evidence 待补。 |
| Gaps | 不把 BrotatoLike 专属 skill names 或 handler behavior 上提为 framework defaults。 |

### Feature

| 字段 | 内容 |
| --- | --- |
| Capability id | `feature` |
| Metadata | `SlimeAI/GameOS/Capabilities/Feature/capability.json` |
| Legacy status source | `capability.json` 中为 `experimental`；API index 中存在 `bootstrap` 行 |
| Normalized status | `Experimental` |
| Owner skill | `feature-system` |
| Contract / Debug | `GameOS/Capabilities/Feature/Contract.md`；`GameOS/Capabilities/Feature/Debug.md` |
| Primary APIs | `FeatureService`、`FeatureDataKeys`、`FeatureDefinition`、`FeatureModifierEntry`、`IFeatureHandler`、`FeatureHandlerRegistry` |
| DataKeys | FeatureId、HandlerId、Description、Category、ModifierTargetKey、ModifierType、ModifierValue、ModifierPriority、IsEnabled、IsActive、ActivationCount |
| Events | `Feature.Activated`、`Feature.Deactivated`、`Feature.ModifierApplied`、`Feature.ModifierRemoved` |
| Selector owner | 默认 `None`；具体 handler 可以消费 Ability 或 game adapter 提供的 target context。 |
| RuntimeSchedule boundary | Service-driven lifecycle；handler 不应把长期状态隐藏在 Runtime data 或 services 之外。 |
| GodotBridge boundary | 暂无通用 Feature bridge；game-specific handlers 先留在 game adapter code，直到可复用。 |
| Dependencies | Runtime Data modifiers；Ability 可提供 activation context。 |
| Validation commands | Framework build/tests；game-side handlers 的 Godot 证据暂不运行。 |
| Game-slice evidence | Runtime tests 覆盖 grant/remove、handler lifecycle 和 AbilityService handler invocation；BrotatoLike smoke 曾覆盖 selected game-side handlers；playable-slice evidence 待补。 |
| Gaps | 需要更严格的 source rollback evidence 后才能考虑 `Supported`。 |

### Projectile

| 字段 | 内容 |
| --- | --- |
| Capability id | `projectile` |
| Metadata | `SlimeAI/GameOS/Capabilities/Projectile/capability.json` |
| Legacy status source | `capability.json` 中为 `experimental`；API index 中存在 `bootstrap` 行 |
| Normalized status | `Experimental` |
| Owner skill | `projectile-effect-system` |
| Contract / Debug | `GameOS/Capabilities/Projectile/Contract.md`；`GameOS/Capabilities/Projectile/Debug.md` |
| Primary APIs | `ProjectileTool`、`ProjectileDataKeys`、`ProjectileSpawnOptions`、`ProjectileMovementOptions` |
| DataKeys | ScenePath、SourceEntity、AbilityEntity、TargetEntity、SpawnPosition、TargetPosition、Direction、Speed、MaxHitCount、HitCount、MaxLifeTime、Damage、DamageType、DamageTags |
| Events | `Projectile.Spawned`、`Projectile.Hit`、`Projectile.LifeTimeExpired`、`Projectile.Destroyed` |
| Selector owner | Projectile 拥有 movement-collision result 上的 hit lifecycle；初始目标选择归 Ability、AI、input 或 game adapter。 |
| RuntimeSchedule boundary | Spawn 由 service/tool 驱动；movement 和 lifetime progression 依赖调用方持有的 MovementSystem / Timer ticks。 |
| GodotBridge boundary | `GodotProjectileEffectSpawner` 根据 `ScenePath` 实例化视觉节点，并在 Runtime entity 销毁时清理；Runtime 不直接加载 Godot 资源。 |
| Dependencies | Movement 用于飞行；Collision 用于过滤；Damage 用于命中伤害；handler 需要时可配合 Effect。 |
| Validation commands | Framework build/tests；visual instantiation 和 hit lifecycle 的 Godot 证据暂不运行。 |
| Game-slice evidence | Runtime tests 覆盖 spawn、movement hit、pierce、max hit count、lifetime destroy 和 damage；smoke 曾覆盖 visual spawn 和 cleanup；playable-slice old-skill projectile evidence 待补。 |
| Gaps | 需要普通 main-scene skill evidence 覆盖 visual、hit behavior、target selection 和 cleanup。 |

### Effect

| 字段 | 内容 |
| --- | --- |
| Capability id | `effect` |
| Metadata | `SlimeAI/GameOS/Capabilities/Effect/capability.json` |
| Legacy status source | `capability.json` 中为 `experimental`；API index 中存在 `bootstrap` 行 |
| Normalized status | `Experimental` |
| Owner skill | `projectile-effect-system` |
| Contract / Debug | `GameOS/Capabilities/Effect/Contract.md`；`GameOS/Capabilities/Effect/Debug.md` |
| Primary APIs | `EffectTool`、`EffectDataKeys`、`EffectSpawnOptions`、`EffectSpawnResult` |
| DataKeys | ScenePath、Name、AnimationName、SourceEntity、AbilityEntity、TargetEntity、Position、Duration |
| Events | `Effect.Spawned`、`Effect.Completed`、`Effect.Cancelled` |
| Selector owner | `None`；effects 消费调用方提供的 source/target/position。 |
| RuntimeSchedule boundary | Spawn 由 service/tool 驱动；duration 和 visual lifecycle 第一批由 bridge 或调用方管理。 |
| GodotBridge boundary | `GodotProjectileEffectSpawner` 实例化 effect scenes 并播放 `AnimatedSprite2D` animation names；Runtime 只保存 paths 和 data。 |
| Dependencies | Runtime Entity/Data/Event；Movement 用于 position values；Projectile/Ability/Feature handlers 可生成 effects。 |
| Validation commands | Framework build/tests；effect instantiation 和 animation playback 的 Godot 证据暂不运行。 |
| Game-slice evidence | Runtime tests 覆盖 spawn data 和 animation name 写入；smoke 曾覆盖 Godot visual instantiation 和 animation playback；playable-slice skill visual evidence 待补。 |
| Gaps | 需要 Observation 和 playable-slice artifacts 证明 visual lifecycle 和 cleanup。 |

## 跨 Capability 路由备注

- Ability trigger、cooldown、charge、target validation 和 `AbilityTriggerReport` 使用 `ability-system`。
- 可复用 modifier 和 handler lifecycle 使用 `feature-system`。游戏专属 handler body 先放在 `Games/BrotatoLike`，除非单独 proposal 证明可复用。
- `ProjectileTool`、`EffectTool`、projectile hit lifecycle、visual spawn bridge 和 effect animation playback 使用 `projectile-effect-system`。
- 修改 GodotBridge Adapter protocol 或 legacy `IGodotComponent` 行为时使用 `ecs-component`；skill ID 暂不重命名。
- 修改 DataOS schema、migration、generator、validator 或 DataKey authoring mapping 时使用 `data-authoring`。
- 修改 validation、Observation、runner、artifact 或 log-analysis 时使用 `test-system`。

## Supported 门禁清单

Capability 只有在全部条件满足时，才能提议升级为 `Supported`：

- Contract doc 存在且匹配 public API。
- Debug doc 或 debug guide entry 存在。
- Framework tests 覆盖核心 runtime 行为。
- 至少一个已接受 Profile 或 game-slice artifact 引用该 Capability。
- Validation commands 和最新证据已记录。
- 游戏专属行为仍留在 framework contract 外，除非通过 OpenSpec 单独提升。
