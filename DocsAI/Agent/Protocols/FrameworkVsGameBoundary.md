# Framework vs Game 边界协议

> 更新日期：2026-05-09  
> 状态：生效候选  
> 适用范围：所有可能同时触及 `SlimeAI/` 框架路径和 `Games/BrotatoLike/` 游戏路径的任务。

## 什么时候用

收到任务后，只要涉及以下任一内容，就先使用本协议：

- Runtime kernel、Capability、DataOS、GodotBridge、Validation、Observation、Agent protocol。
- GenreProfile、GameAdapter、GameContent。
- BrotatoLike 主场景、UI、技能、波次、资源路径、迁移账本或旧项目行为。
- 任何“是否应该上提到框架”的判断。

任务开始前必须选择 primary category：

```text
Kernel / Capability / GenreProfile / GameAdapter / GameContent / DataOS / GodotBridge / Validation / Observation
```

可以有 secondary categories，但必须先写清 primary category。

## 必须读什么

通用必读：

- `SlimeAI/DocsAI/Agent/Protocols/OpenSpecChangeProtocol.md`
- `SlimeAI/DocsAI/GameOS/Capabilities/CapabilityIndex.md`
- 当前 OpenSpec change 的 `proposal.md`、`design.md`、`specs/**/*.md`、`tasks.md`

框架任务再读：

- `SlimeAI/DocsAI/GameOS/Contracts.md`
- `SlimeAI/DocsAI/GameOS/ApiIndex.md`
- 相关 Capability 的 `Contract.md` / `Debug.md`
- 相关 `.codex/skills/*/SKILL.md`

游戏任务再读：

- `Games/BrotatoLike/DocsAI/GameProjectState.md`
- `Games/BrotatoLike/DocsAI/MigrationLedger.md` 或对应 OpenSpec change
- 旧行为来源：`Resources/Else/brotato-my/` 或已复制到游戏仓库的 migration input

## 所有权判断

框架路径负责：

- `GameOS/Runtime`：Entity、Data、Event、Schedule、Timer、Pool、Resource 等通用运行时内核。
- `GameOS/Capabilities`：通用能力契约、服务、DataKeys、Events、Debug、测试。
- `DataOS`：schema、migration、generator、validator、runtime snapshot 和 traceability。
- `GameOS/GodotBridge`：通用 Node / SceneTree / Physics / Input / Resource 适配。
- `DocsAI/Agent/Protocols`：AI 执行协议、边界规则、完成契约。
- `Validation` / `Observation`：通用验证命令、artifact 形状、日志分析协议。
- `GenreProfile`：可复用能力组合和 profile preset，不包含具体游戏资产。

游戏路径负责：

- `Games/BrotatoLike` 的场景、UI、资产、技能 handler、波次规则、game adapter。
- BrotatoLike 旧行为迁移账本的新目标路径和验收证据。
- 游戏专属 DataOS seed、resource catalog、acceptance runner。

## 禁止什么

- 禁止把 BrotatoLike 专属技能、波次数值、UI 样式、场景布局或资产路径写成 GameOS framework default。
- 禁止在框架仓 `SlimeAI/` 中加入 `using BrotatoLike`、`BrotatoLike.Game.*` 或其它游戏命名空间反向依赖。
- 禁止把游戏专属玩法事件放进 `SlimeAI/GameOS/Runtime/Events/`；已知反例是 P3 删除 / 迁移的 `MouseSelection* / Wave* / GameStart / GameOver / GamePause / GameResume / InputUseSkill / InputPreviousSkill / InputNextSkill`。
- 禁止为了一个游戏特例扩大 public API。
- 禁止把 `Tools/run-godot-smoke.sh` 当作完整可玩切片验收。
- 禁止 runtime hot path 查询 DataOS authoring SQLite。
- 禁止引入外部 ECS runtime dependency、通用 world query DSL、pair graph public API 或 registry-like public API。
- 禁止复制 Unity DOTS baking、Unreal GAS、QFramework static architecture 等 public API 形态。

## 事件归属决策树

新增或迁移事件时按以下顺序判断：

1. payload 是否对多个 game profile 都有意义，且不依赖 Godot `Vector2 / Rect2 / Node` 等引擎类型？如果是，放在 framework Runtime/Events 或 framework Capability `Events/`，按 entity/world scope 选择目录。
2. payload 是否依赖具体游戏玩法、UI、输入动作、资产或波次 / 主动技能 / 卡牌 / 天赋等术语？如果是，放在 `Games/<Game>/Src/Game/Event/`，namespace 使用 `<Game>.Game.Events`。
3. payload 是否依赖 Godot 引擎类型？如果是，默认放在游戏侧；framework Runtime payload 使用 primitives、`EntityId`、`Vector2Value` 或其它 framework-owned value type。
4. 只有一个游戏使用、但未来可能通用时，先留在游戏侧；第二个 game profile 验证出共同抽象后再提 OpenSpec 上提。

对旧 framework event 做迁移时，先 grep `SlimeAI/` 与 `Games/<Game>/Src/` 并分桶：

| Bucket | 判定 | 动作 |
| --- | --- | --- |
| Bucket A：死代码 | framework / game 均无 Publish / Subscribe | 删除，不创建空替换 API |
| Bucket B：真实调用链 | 有 producer 或 consumer，语义属于具体游戏 | 迁到 `Games/<Game>/Src/Game/Event/`，同一变更删除 framework 声明 |
| Bucket C：框架使用但名称错误 | framework 自身 Publish / Subscribe，但名称带游戏术语 | 留在 framework，改成中性名称；不得让 framework 引用 game namespace |

参考案例：P3 `refactor-runtime-events-purge-game-leakage` 中，`MouseSelection* / Wave* / GameStart / GameOver / GamePause / GameResume` 属于 Bucket A，直接删除且不迁移；`InputUseSkill / InputPreviousSkill / InputNextSkill` 与 `GodotPlayerInputComponent` 属于 Bucket B，迁到 BrotatoLike；本轮未发现 Bucket C。

## 验证什么

文档和 OpenSpec：

```bash
cd /home/slime/Code/SlimeAI
openspec validate <change-id> --strict
```

核心框架：

```bash
cd /home/slime/Code/SlimeAI/SlimeAI
Tools/run-build.sh
Tools/run-tests.sh
```

DataOS：

```bash
cd /home/slime/Code/SlimeAI/SlimeAI
Tools/run-dataos-validate.sh
```

Godot 场景验证当前不作为本阶段门禁。后续用户确认环境可运行后，再按游戏侧 scene runner 协议恢复。

## 完成时怎么汇报

汇报必须包含：

- primary category 和 secondary categories。
- 修改文件列表。
- 完成的 OpenSpec `tasks.md` checkbox。
- 运行的验证命令和结果。
- 未运行验证的明确原因。
- 框架 / 游戏边界是否被改变。
- 剩余风险和下一步。

## 多轮失败时如何复盘

同一问题连续失败两次时，停止重复尝试，记录：

- 失败命令和关键错误。
- 已读文件和已排除假设。
- 当前 primary category 是否选错。
- 是否需要拆成新的 OpenSpec change。
- 是否需要先补迁移账本、Observation 或 DataOS trace。

复盘结果写入当前 OpenSpec change 的 `tasks.md` 或相关状态文档。
