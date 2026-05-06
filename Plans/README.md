# SkilmeAI 框架仓库执行计划

> 更新日期：2026-05-05
> 状态：执行中
> 定位：本仓库是 `SkilmeAI.GameOS`、`DataOS`、通用 Agent 协议、框架 Skill 源头和包发布的主仓库。旧 `brotato-my` 只作为迁移输入和控制面，不再作为长期主项目。

## 当前执行到哪一步

当前已完成到 `M27.3 Ability handler 真实执行闭环扩展`。

已验证完成：

- `M1/M2`：`/home/slime/Code/SkilmeAI/` 多仓库工作区骨架已建立。
- `M3`：`SkilmeAI.GameOS` 最小可构建包已建立。
- `M3 Runtime`：Data / Event / Entity / Relationship / Schedule / Resource / Pool / Timer 最小内核已迁入。
- `M4`：`Games/BrotatoLike` 已通过 `ProjectReference` 接入框架并可构建。
- `M5`：GodotBridge 第一版已迁入，覆盖 Godot Entity 生命周期、Component 注册关系和 `_Process` Timer bridge。
- `M5.1`：GodotBridge Node 对象池第一版已迁入，覆盖 Node 池化、延迟激活、回池泊车、`CollisionObject2D` 脱树和碰撞隔离基础工具。
- `M5.1`：BrotatoLike 已建立 `Tools/run-godot-smoke.sh`，使用现成 Godot 4.6.2 mono CLI 完成 headless smoke，断言 `bridge:True pool:True`。
- 旧 `assets/` 已复制到 `Games/BrotatoLike/assets`，旧 `Data/` 和 `Src/Main/` 已复制到 `Games/BrotatoLike/MigrationInput/` 并排除编译。
- Godot 引擎源码入口已固定为 `/home/slime/Code/SkilmeAI/Engine/godot-4.6.2-stable`，构建脚本在 `/home/slime/Code/SkilmeAI/Engine/Tools/build-linux-editor-mono.sh`。
- `M17`：DataOS 已建立 SQLite core schema、migration、generator、validator 和 Runtime JSON snapshot loader；BrotatoLike 已接入第一批 authoring seed、生成 snapshot，并在 Godot smoke 中读取 snapshot 写入 Runtime Data、注册资源映射。
- `M18+`：BrotatoLike DataOS seed 已扩大到 TargetingIndicator、ChainAbility、旧 AbilityData 通用字段、Ability handler-specific 参数第三段、Feature definition / modifier、System config / preset、Spawn config 和更多旧 ResourcePaths；框架补充 Unit / Ability / Feature / Schedule DataMeta，并新增 Movement handler authoring DataKey；游戏侧新增 `BrotatoLikeDataOSBootstrap` 和 `BrotatoLikeAbilityHandlers`，正式代码可从 snapshot 生成 Runtime Entity；SineWave / Boomerang / BezierCurve / CircularArc / Orbit / AttachToHost、Dash、ChainLightning、Slam、TargetPoint、CircleDamage 和 AuraShield 已从 DataOS Runtime Data 进入真实 handler 执行闭环。

最新验证命令：

```bash
cd /home/slime/Code/SkilmeAI/SkilmeAI
Tools/run-build.sh
Tools/run-tests.sh
Tools/run-pack.sh

cd /home/slime/Code/SkilmeAI/Games/BrotatoLike
Tools/run-build.sh
Tools/run-godot-smoke.sh
```

最新结果：框架 build 通过，Runtime 行为测试全部 PASS，本地 NuGet 已生成，BrotatoLike build 通过。

当前阻塞：无代码阻塞。本机缺少 `SCons`，暂时无法从 Godot 源码构建自定义 CLI；但已使用现成 Godot 4.6.2 mono CLI 跑通 BrotatoLike headless smoke。

## 仓库边界

本仓库负责：

- `GameOS/Runtime`：纯 C# 内核，尽量不依赖 Godot。
- `GameOS/GodotBridge`：Godot Node、SceneTree、PackedScene、Physics、Resource、`_Process` 等 Godot 适配层。
- `GameOS/Capabilities`：Movement / Collision / Damage / Ability / Feature / AI 等能力包。
- `GameOS/Validation`：build、测试场景、Capability 回归和验证门禁。
- `GameOS/Observation`：日志、dump、trace、运行快照和报告。
- `DataOS`：SQLite schema、迁移、生成器、runtime snapshot 和数据校验。
- `Agent`：通用 AI 协议、Skill 源头和模板。
- `Packages`：本地 NuGet、构建输出和发布说明。

本仓库不放：

- BrotatoLike 专属资产。
- 单个游戏的关卡和数值内容。
- Godot 引擎源码。

## 阶段计划

### M5.1：GodotBridge 扩展

目标：把旧 `EntityManager` 中 Godot 强绑定的生命周期能力继续拆到 `GodotBridge`。

输出：

- Node 场景实例化入口。
- Component path cache / 预热扫描。
- Node 对象池泊车、脱树、启用/禁用流程。`GodotNodePool<T>` 第一版已完成。
- 碰撞隔离基础接口，为 PhysicsServer2D trace 做准备。`GodotCollisionIsolation` 第一版已完成。
- GodotBridge headless 场景验证入口。BrotatoLike `Tools/run-godot-smoke.sh` 已完成第一版。

验收：

```bash
Tools/run-build.sh
Tools/run-tests.sh
cd /home/slime/Code/SkilmeAI/Games/BrotatoLike && Tools/run-build.sh
```

Godot CLI 验证：

```bash
cd /home/slime/Code/SkilmeAI/Games/BrotatoLike && Tools/run-godot-smoke.sh
```

### M6：Capabilities 第一批迁移

迁移顺序：

1. Movement
2. Collision
3. Damage
4. Ability
5. Feature
6. AI

每个 Capability 必须包含：

- `Contract.md`
- manifest / 注册入口
- Runtime API
- GodotBridge 或 Godot 实现边界
- 最小行为测试
- DebugGuide
- 对应游戏接入说明

策略：能从旧仓库直接复制的代码先复制，再删除旧命名空间、旧 Godot 耦合和过量注释；不要为了“重写得干净”丢失旧项目里已经验证过的行为。

### M7：DataOS 建立

目标：把旧 DataNew / `.tres` / ResourcePaths 迁移到 AI 原生数据层。

输出：

- SQLite authoring schema。已完成第一版 `DataOS/Schema/core.sql`。
- migrations。已完成 `DataOS/Migrations/001_initial.sql`。
- generator。已完成 `DataOS/Generators/generate-runtime-snapshot.sh`。
- runtime snapshot。已完成 `RuntimeDataSnapshot` 读取入口和 BrotatoLike `DataOS/Snapshots/runtime_snapshot.json`。
- 数据校验命令。已完成 `DataOS/Validation/validate-dataos.sh` 和 `Tools/run-dataos-validate.sh`。
- BrotatoLike 数据迁移入口。已完成第一批 `DataOS/Authoring/BrotatoLike.seed.sql`。

原则：运行时热路径不直接依赖任意 SQL 查询；游戏数据用 snapshot 消费。

### M8：Validation / Observation

输出：

- Capability test runner。
- Godot scene test runner 接入新引擎 CLI。
- 日志落盘和摘要。
- Runtime dump / relationship dump / schedule dump。
- Physics trace 读取协议。

### M9：包发布和游戏升级

输出：

- 本地 NuGet 发布流程。
- `SkilmeAI.GameOS.Contracts.md`
- `SkilmeAI.GameOS.ApiIndex.md`
- `SkilmeAI.GameOS.DebugGuide.md`
- `SkilmeAI.GameOS.Migration.md`
- BrotatoLike 框架升级记录。

## 当前下一步执行顺序

1. 继续扩大 DataOS 旧 DataNew 剩余字段和真实生成入口，推进剩余 Ability / Projectile / Effect / Feature handler 数据驱动。
2. 继续把真实主场景、UI 和 SpawnSystem 专项 smoke 接入 BrotatoLike 统一场景测试 runner。
3. 继续按 Capability 迁移缺口补 Feature actions 和正式数据适配。
4. Godot 引擎 trace 等自定义 Godot CLI 构建完成后再接入。

## 状态同步规则

每轮迁移结束必须同步：

- 本文件。
- `DocsAI/ProjectState.md`
- `GameOS/SkilmeAI.GameOS.Contracts.md`
- `GameOS/SkilmeAI.GameOS.ApiIndex.md`
- 相关 `GameOS/*/DebugGuide.md` 或迁移说明。
- 如果影响 BrotatoLike，还要同步 `/home/slime/Code/SkilmeAI/Games/BrotatoLike/DocsAI/GameProjectState.md` 和 `Plans/README.md`。
- 如果影响旧控制面，还要同步 `/home/slime/Code/Godot/Games/MyGames/brotato-my/Plans/Architecture/Godot_AI_Game_OS_Migration/Progress.md`。
