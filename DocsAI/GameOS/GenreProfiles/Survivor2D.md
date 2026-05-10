# Survivor2D Profile

> 更新日期：2026-05-09  
> 状态：contract draft  
> 范围：2D top-down survivor / arena survival 类游戏的能力组合边界。

## 定位

`Survivor2D` 是 SkilmeAI 的第一个 GenreProfile。它定义一个幸存者 / 类 Brotato 2D 俯视游戏通常需要哪些 GameOS Capability、DataOS preset、activation actions 和验证命令。

本文件不是 runtime loader，不改变 public API。第一版只定义 profile 边界，后续是否实现 C# profile loader 需要单独 OpenSpec change。

BrotatoLike 是第一验证游戏，不是 profile 默认内容来源。

## 默认能力候选

根据 `SkilmeAI/DocsAI/GameOS/Capabilities/CapabilityIndex.md`，首批能力当前均为 `Experimental`。它们可以作为 `Survivor2D` 默认能力候选，但不能标记为 `Supported`。

| Capability | Profile role | 当前状态 | 说明 |
| --- | --- | --- | --- |
| Movement | 默认能力候选 | `Experimental` | 玩家输入移动、AI 追逐、投射物轨迹和运动碰撞。 |
| Collision | 默认能力候选 | `Experimental` | layer/mask/team 过滤、hurtbox/contact、movement collision。 |
| Damage | 默认能力候选 | `Experimental` | HP、伤害、治疗、死亡、统计、周期伤害。 |
| Unit | 默认能力候选 | `Experimental` | unit metadata、visual scene path、动画可用列表、生命条元数据。 |
| Attack | 默认能力候选 | `Experimental` | 普通攻击、前摇、后摇、冷却、攻击请求。 |
| AI | 默认能力候选 | `Experimental` | 敌人目标选择、追逐、巡逻、攻击请求、自动施法上下文。 |
| Ability | 默认能力候选 | `Experimental` | 主动 / 被动 / 武器技能、冷却、充能、目标语义、自动索敌。 |
| Feature | 默认能力候选 | `Experimental` | 可授予 modifier、handler lifecycle、Ability handler 接入。 |
| Projectile | 默认能力候选 | `Experimental` | 投射物生成、飞行、穿透、命中、生命周期。 |
| Effect | 默认能力候选 | `Experimental` | Runtime effect 生成、动画名、视觉实例化边界。 |

## 候选扩展

以下内容暂不作为 profile 默认框架能力：

| Candidate | 当前位置 | 进入默认的条件 |
| --- | --- | --- |
| Spawn | GameAdapter 或后续 Capability | 至少一个可复用 spawn contract 和 game slice evidence。 |
| HUD | GameAdapter / GameContent | 至少形成通用 UI binding contract，且不含游戏样式。 |
| Drop | GameAdapter | 有复用的掉落事件、资源和验证协议。 |
| LevelUp | GameAdapter | 有通用经验、升级、选择协议。 |
| Inventory | GameAdapter 或独立 Capability | 有多个游戏复用证据。 |
| economy-like systems | GameAdapter | 明确资源、货币或商店协议后再提升。 |

## DataOS Preset Expectations

`Survivor2D` profile 期望 DataOS 至少能描述：

- `unit.player`：玩家单位记录，包含 Movement、Damage、Unit、Attack 基础字段。
- `unit.enemy`：敌人单位记录，包含 Movement、Damage、Collision、AI、Attack 基础字段。
- `ability.*`：技能记录，包含 Ability、Feature、Projectile、Effect、Movement handler 参数。
- `resource_entry`：稳定 resource key 到 `res://` path 的映射。
- `system.config` / `system.preset`：可选系统配置和 schedule preset。
- `spawn.config`：候选扩展；第一版可由 game adapter 消费。

DataOS record 存在不等于玩法迁移完成。资源路径状态和行为完成状态必须由 snapshot manifest、validation report、migration ledger 和可玩切片 evidence 共同证明。

## Activation Actions

第一版 `Survivor2D` profile 建议的 activation actions 是文档约束，不是 runtime loader API：

1. 初始化 Runtime kernel：Entity、Data、Event、Timer、Resource、Schedule。
2. 注册默认 Capability services：DamageService、HealService、AbilityService、FeatureService、AttackService、AIService。
3. 注册 Movement strategies：PlayerInput、AIControlled、Charge、Orbit、SineWave、BezierCurve、Boomerang、Parabola、CircularArc、AttachToHost。
4. 注册 ResourceCatalog，并从 DataOS snapshot 加载资源映射。
5. 由 game adapter 生成玩家、敌人、技能实体和场景节点。
6. 由 game adapter 挂载 GodotBridge components。
7. 运行 validation / Observation contract，输出 PASS/FAIL artifact。

## 排除项

以下内容不得写入 `Survivor2D` profile defaults：

- BrotatoLike 专属技能名和 handler 逻辑。
- BrotatoLike 波次数值、敌人组合、掉落数值。
- BrotatoLike UI 样式、布局、字体、颜色。
- BrotatoLike 资产路径，例如具体 `res://assets/...` 或旧 `res://Src/...`。
- 只为单个游戏存在的 game balance 字段。

## 验证命令

核心框架：

```bash
cd /home/slime/Code/SkilmeAI/SkilmeAI
Tools/run-build.sh
Tools/run-tests.sh
Tools/run-dataos-validate.sh
```

Godot 场景验证当前暂不运行。后续用户确认环境可运行后，`Survivor2D` 在 BrotatoLike 中的验收必须使用普通主场景和明确 PASS/FAIL marker，而不是只看 smoke。

## 支持状态门禁

`Survivor2D` profile 本身升为可支持状态前，至少需要：

- CapabilityIndex 中默认能力的 status 和证据更新。
- DataOS snapshot manifest 和 validation report 可引用。
- BrotatoLike migration ledger 覆盖第一批旧路径。
- Observation contract 定义 PASS/FAIL artifact。
- 普通 `Scenes/Main.tscn` 可玩切片通过并输出明确 PASS/FAIL marker。
