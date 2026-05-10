# 从 brotato-my 迁移到 SkilmeAI

> 日期：2026-05-06
> 状态：Phase 01-09 已完成，Phase 10-11 待开始
> 来源仓库：`/home/slime/Code/Godot/Games/MyGames/brotato-my`
> 目标工作区：`/home/slime/Code/SkilmeAI/`

---

## 1. 为什么迁移

旧仓库 `brotato-my` 是一个 Godot 4.6 C# Brotato-like 游戏项目，积累了完整的 ECS 框架、DataNew 配置体系、Skill 文档和测试基础设施。随着项目从"单个游戏"向"AI-First 游戏框架"演进，出现以下结构性问题：

| 问题 | 影响 |
|------|------|
| 框架代码和游戏代码混在一个仓库 | 框架无法独立演进，游戏升级牵一发动全身 |
| 数据配置用 `.tres` / `DataNew` | AI 无法理解、修改和验证；运行时与 authoring 混在一起 |
| Skill 文档与代码严重脱节 | 维护成本高，AI 执行时频繁需要人工纠偏 |
| 没有清晰的 Capability 边界 | 新增功能容易破坏已有结构，无法按能力包独立验证 |
| 没有 DataOS 层 | 数据 authoring、runtime snapshot、验证门禁全部缺失 |
| 没有对象池和碰撞隔离 | 高频对象 `new`/`QueueFree` 导致性能问题和物理时序 bug |

迁移目标：**建立 SkilmeAI GameOS 框架**，把通用 Runtime、Capabilities、DataOS、Validation 和 Observation 从游戏中拆分出来，让游戏仓库只保留游戏特定内容。

---

## 2. 迁移了什么

### 2.1 新仓库工作区骨架

```text
/home/slime/Code/SkilmeAI/
  SkilmeAI/                    # AI 框架主仓库（repo）
    GameOS/Runtime/            # 纯 C# 内核
    GameOS/Capabilities/       # 能力包
    GameOS/GodotBridge/        # Godot 适配层
    GameOS/Validation/         # 测试与回归门禁
    DataOS/                    # SQLite authoring + snapshot
    Agent/                     # AI 协议文档
    DocsAI/                    # 架构文档

  Engine/                      # Godot 源码目录
    godot-4.6.2-stable/

  Games/
    BrotatoLike/               # 第一个正式游戏（repo）
      Src/Game/
      DataOS/
      assets/
      Scenes/
```

### 2.2 Runtime 内核（8 个系统）

从旧仓库 `Src/ECS/Base/` 迁入 `SkilmeAI/GameOS/Runtime/`：

| 系统 | 旧路径 | 新路径 | 说明 |
|------|--------|--------|------|
| Entity | `Entity/Core` | `Runtime/Entity` | 生命周期、注册、销毁、关系绑定 |
| Event | `Event` | `Runtime/Event` | EventBus、全局事件、基础事件协议 |
| Data | `Data` | `Runtime/Data` | 运行时状态容器 `Data` / `DataRegistry` |
| Relationship | `Entity/Relationship` | `Runtime/Relationship` | 父子关系、归属策略、级联销毁 |
| Schedule | `System/Schedule` | `Runtime/Schedule` | RuntimeSchedule、系统配置、运行门禁 |
| Pool | `Tools/ObjectPool` | `Runtime/Pool` | 通用对象池接口 |
| Timer | `Tools/TimerManager` | `Runtime/Timer` | TimerManager、定时器调度 |
| Resource | `Data/ResourceManagement` | `Runtime/Resource` | 运行时资源加载、ResourceCatalog |

### 2.3 Capability（10 个能力包）

从旧仓库 `Src/ECS/Base/System/` 和 `Component/` 迁入 `SkilmeAI/GameOS/Capabilities/`，每个 Capability 建立标准模板：

| Capability | 旧路径 | 状态 | 说明 |
|------------|--------|------|------|
| Movement | `System/Movement` + `Component/Movement` | 已完成 | 12 种移动策略：Charge/SineWave/Orbit/Boomerang/BezierCurve/CircularArc/Parabola/AttachToHost/PlayerInput/AIControlled + 碰撞检测 + 朝向输出 |
| Collision | `Component/Collision` | 已完成 | CollisionLayers、CollisionComponent、HurtboxComponent、ContactDamage、碰撞隔离 |
| Damage | `System/DamageSystem` | 已完成 | DamageService、暴击/闪避/护甲/护盾/吸血、DamageTool、伤害统计 |
| Effect | `System/EffectSystem` | 已完成 | Effect 实体生成、AnimatedSprite2D 播放、生命周期 |
| Unit | `Component/Unit` | 已完成 | UnitDataKeys、动画事件桥、死亡类型 |
| Attack | `System/AttackSystem` | 已完成 | AttackService、前摇/后摇/冷却、攻击动画桥 |
| Projectile | `Entity/Projectile` + `System/ProjectileSystem` | 已完成 | Projectile 实体生成、命中生命周期、穿透、MaxLifeTime、Godot 实例化桥 |
| Feature | `System/FeatureSystem` | 已完成 | FeatureDefinition、IFeatureHandler、Modifier 授予/移除、激活/执行/结束生命周期 |
| Ability | `System/AbilitySystem` + `Component/Ability` | 已完成 | AbilityService、冷却/充能/触发、自动索敌、通过 FeatureHandlerId 调用 handler |
| AI | `System/AISystem` + AI 组件 | 已完成 | 行为树、AIContext、FindNearestTarget、MoveToTarget、RequestAttack、PrepareAbilityAutoTarget、TickAbilityAutoTriggers |

每个 Capability 包含：
- `capability.json`（manifest，声明 DataKey、Event、Component、System）
- `Contract.md`（契约文档）
- `Debug.md`（调试指南）
- `Tests/`（最小行为测试目录）

### 2.4 DataOS（数据 authoring 层）

旧 `Data/DataNew` / `.tres` / `DataKey` / `EventType` → 新 `DataOS/`：

| 组件 | 说明 |
|------|------|
| SQLite schema | `DataOS/Schema/core.sql`，覆盖 table / record / field / resource_entry |
| migrations | `DataOS/Migrations/001_initial.sql` |
| generator | `DataOS/Generators/generate-runtime-snapshot.sh`，从 SQLite 生成 JSON runtime snapshot |
| validator | `DataOS/Validation/validate-dataos.sh` |
| runtime loader | `RuntimeDataSnapshot.FromJson()`，游戏启动时读取 snapshot |
| BrotatoLike seed | `Games/BrotatoLike/DataOS/Authoring/BrotatoLike.seed.sql`，覆盖玩家/敌人/技能/Feature/系统配置/Spawn 规则 |

**关键决策**：运行时热路径不直接查询 SQL；游戏通过 snapshot 消费数据。

### 2.5 GodotBridge（Godot 适配层）

旧仓库中散落在 `Src/ECS/Base/Entity/` 和 `Component/` 中的 Godot 耦合代码 → 新 `SkilmeAI/GameOS/GodotBridge/`：

| 组件 | 说明 |
|------|------|
| GodotEntity2D | 继承 `CharacterBody2D`，自动注册子节点 Component |
| GodotAreaEntity2D | 用于碰撞/伤害区域的 Entity 节点 |
| IGodotComponent | 组件生命周期：`OnComponentRegistered` / `OnComponentUnregistered` |
| GodotMovementDriver | `_Process` 驱动 MovementSystem.Tick |
| GodotTimerDriver | `_Process` 驱动 TimerManager.Tick |
| GodotNodePool<T> | 对象池：延迟激活、回池泊车、CollisionObject2D 脱树、碰撞隔离 |
| GodotPlayerInputComponent | Input Map → `MovementDataKeys.InputDirection` 桥接 |
| GodotCollisionBridge | Area2D 碰撞事件 → `GameEventType.Collision` |
| GodotProjectileEffectSpawner | 监听 Projectile/Effect 生成事件，实例化 Godot 节点 |
| GodotContactDamageComponent | 碰撞接触 → DamageService |
| GodotAttackComponent | 攻击请求 → AttackService + 动画桥 |
| GodotAIComponent | AI 行为树 Tick + 导出参数写入 |
| GodotUnitAnimationComponent | 动画播放/停止请求桥 |
| GodotOrientationComponent | 朝向同步到 Node2D.RotationDegrees |

### 2.6 BrotatoLike 游戏仓库

```text
Games/BrotatoLike/
  Src/Game/
    BrotatoLikeGameRuntime.cs          # 主运行时：DataOS 初始化 + RuntimeSchedule + Spawn Tick
    BrotatoLikeDataOSBootstrap.cs      # DataOS snapshot → Runtime Entity
    BrotatoLikeEnemySpawnSystem.cs     # 波次敌人生成
    BrotatoLikeSpawnCatalog.cs         # 生成规则目录
    BrotatoLikeAbilityHandlers.cs      # 游戏侧 Feature handler 注册
    BrotatoLikeGameEventType.cs        # 游戏特定事件
    GodotActiveSkillInputComponent.cs  # 玩家技能输入组件
    Main.cs                             # 启动场景 + smoke probe
  DataOS/
    Authoring/BrotatoLike.seed.sql     # 全量数据 authoring
    Snapshots/runtime_snapshot.json    # 运行时 snapshot（生成）
  assets/                              # 游戏资产（sprite、场景、特效）
  Scenes/
    Main.tscn                          # 主场景，挂载 GameRuntime
```

**已验证闭环**：
- 从 DataOS `unit.player/deluyi` 生成玩家 Entity
- 从 DataOS `unit.enemy/yuren` / `chailangren` 生成敌人 Entity
- 从 DataOS `ability/slam` / `chain_lightning` 生成技能 Entity
- SineWave / Boomerang / BezierCurve / CircularArc / Orbit / AttachToHost / Dash / ChainLightning / Slam / TargetPoint / CircleDamage / AuraShield 从 DataOS 参数进入真实 handler 执行
- Godot headless smoke PASS：`bridge:True pool:True dataos:True main:True`

### 2.7 Skill 体系

旧 `.codex/skills`（长文档、与代码脱节）→ 新**入口型 Skill**：

**框架仓库 `.codex/skills`**（约 15 个）：
- `ecs-entity`、`ecs-component`、`ecs-data`、`ecs-event`
- `movement-system`、`collision-system`、`damage-system`、`ability-system`、`feature-system`、`ai-system`
- `data-authoring`、`project-index`、`tools`、`test-system`
- `research-reference-framework`

**游戏仓库 `.codex/skills`**（约 5 个）：
- `project-index`、`game-development`、`gameos-reference`、`data-authoring`、`godot-scene-test`

**框架仓库 `.codex/skills/`**：唯一 skill 源头和脚本入口，供 AI 路由、验证和维护。

---

## 3. 怎么迁移的

### 3.1 核心策略

| 策略 | 说明 |
|------|------|
| **不做旧兼容** | 旧代码只作为迁移输入，迁完后在新结构重新建立入口 |
| **先迁后删** | 先在新仓库建立最小闭环并验证通过，再删除旧仓库对应内容 |
| **批量入口型 Skill** | 不再逐个迁移 Skill，一次性建立短入口型 Skill 体系 |
| **Capability 标准模板** | 每个 Capability 必须有 manifest + Contract + Debug + Tests |
| **DataOS 接管数据** | 旧 DataNew / .tres 逐步迁入 SQLite authoring，生成 runtime snapshot |
| **Smoke 驱动验证** | 每次迁移必须有 headless Godot smoke 断言 |

### 3.2 迁移顺序

```
Phase 00：冻结旧仓库定位
Phase 01：资产盘点（00_Inventory.md）
Phase 02：建立新工作区骨架（/home/slime/Code/SkilmeAI/）
Phase 03：迁移 Runtime 内核（Entity/Event/Data/Relationship/Schedule/Pool/Timer/Resource）
Phase 04：迁移第一批 Capability（Movement/Collision/Damage/Feature/Ability）
Phase 05：迁移第二批 Capability（AI/Projectile/Attack/Effect）
Phase 06：迁移 DataOS（SQLite schema/migration/generator/validator/snapshot）
Phase 07：建立 Validation + Observation（Runtime tests/Godot scene runner）
Phase 08：重写 DocsAI + Skill（入口型 Skill 体系）
Phase 09：游戏侧接入（BrotatoLike DataOS seed/GameRuntime/Smoke）
Phase 10：删除旧结构（待开始）
Phase 11：AI 生成 Demo 验证（待开始）
```

### 3.3 关键决策

| 决策 | 原因 |
|------|------|
| 多仓库而非 monorepo | 框架、引擎、游戏独立演进，游戏锁定框架版本 |
| 本地项目引用 → 未来本地 NuGet | 迁移期方便 Debug，稳定后锁定版本 |
| Godot C# 不抽象跨引擎 | 第一阶段只做 Godot，真实边界比 premature abstraction 更重要 |
| 角度输入用"度"而非弧度 | 符合游戏设计者直觉；内部计算再转弧度 |
| 概率统一 0-100 | 避免小数点误差和沟通歧义 |
| "不限制"用 -1 | 0 有歧义，-1 明确表示无限制 |
| `_Process` 中禁止 new + LINQ | 性能红线，高频帧中避免 GC |
| 不用 Godot Signal 处理核心逻辑 | 用 EventBus 替代，AI 可理解和追踪 |
| 不用字符串字面量访问 Data | 用 `static readonly DataMeta` + `DataRegistry.Register` |
| Entity 必须走 `EntityManager.Spawn/Destroy` | 统一生命周期，避免内存泄漏 |

---

## 4. 旧仓库现状

旧仓库 `brotato-my` 已降级为**迁移输入仓库**：

```text
brotato-my/
  HistoricalArchitectureNotes/ # 旧迁移计划材料（本文档的原始来源）
  Src/ECS/                     # 旧代码（已大部分迁移完成）
  Data/                        # 旧数据（已迁入 DataOS）
  Docs/                        # 旧文档
  legacy-agent-skills/         # 旧 Skill 来源（已迁入根 .codex/skills）
```

**不再做的事情**：
- 不再新增长期架构能力
- 不再修改旧 ECS 框架代码
- 不再把旧 Skill 作为执行入口

**保留的内容**：
- 旧代码、旧数据、旧场景作为参考和审计痕迹
- 迁移计划文档作为历史记录
- `MigrationInput/` 中的旧文件（游戏仓库中）供后续适配使用

---

## 5. 下一步

### 5.1 近期（Phase 10）

- 删除 SkilmeAI 仓库中的 `MigrationInput/` 旧文件
- 建立旧结构归档清单和最终验证门禁
- 旧仓库 `brotato-my` 只作为迁移输入和历史对照

### 5.2 中期（Phase 11 + M6-M9）

- **AI 生成 Demo 验证**：让 AI 从零生成一个最小 Demo，验证框架独立可用
- **真实主场景接入**：把 BrotatoLike 主场景从 smoke probe 扩展到完整 gameplay
- **UI 接入**：HealthBar、SkillBar、DamageNumber 等 HUD 接入新框架
- **SpawnSystem 专项 smoke**：波次生成、难度曲线、敌人类型组合验证
- **Capability 测试 runner**：`Tools/run-capability-test.sh <capability-id>`
- **DataOS 扩大覆盖**：旧 DataNew 剩余字段、编辑器 authoring UI

### 5.3 远期

- **本地 NuGet 发布**：框架包从项目引用迁移到本地 NuGet 流程
- **Godot 引擎 trace**：PhysicsServer2D trace fork，解决对象池碰撞时序问题
- **第二个游戏验证**：用 NextGameA 验证框架的通用性
- **MCP 接口**：让外部 AI Agent 通过 MCP 调用 GameOS API

---

## 6. 验证命令

```bash
# 框架验证
cd /home/slime/Code/SkilmeAI/SkilmeAI
Tools/run-build.sh      # dotnet build
Tools/run-tests.sh      # Runtime 行为测试
Tools/run-pack.sh       # 本地 NuGet 包

# 游戏验证
cd /home/slime/Code/SkilmeAI/Games/BrotatoLike
Tools/run-build.sh              # dotnet build
Tools/run-godot-smoke.sh        # Godot headless smoke（PASS 条件：bridge + pool + dataos + main = True）
Tools/run-dataos-snapshot.sh    # 生成 runtime snapshot
```

---

## 7. 参考

- 旧仓库迁移计划材料：旧 `brotato-my` 历史架构记录。
- 框架当前计划入口：`/home/slime/Code/SkilmeAI/openspec/changes/<change>/`
- 框架当前规范基线：`/home/slime/Code/SkilmeAI/openspec/specs/`
- 框架项目状态：`/home/slime/Code/SkilmeAI/SkilmeAI/DocsAI/ProjectState.md`
