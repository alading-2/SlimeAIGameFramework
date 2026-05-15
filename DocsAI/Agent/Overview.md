# Agent

`Agent` 存放 SlimeAI 的通用 AI 工作流协议文档。可执行 skill 的唯一源头在仓库根 `.codex/skills/`。

## 子域

- `Protocols/`：AI 开发、Debug、迁移、OpenSpec change、数据生成等协议。
- `Templates/`：Capability、计划、ResearchBrief 等模板。
- `DocsAI/`：Agent 侧索引和说明。

## 计划协议

- `Protocols/AIFeatureDevelopmentProtocol.md`：每次写新功能、扩展能力、迁移旧逻辑或重构框架行为前的 AI-first 开发入口；先调用 `.codex/skills/ai-feature-development`，再进入 owner skill，结束时按功能收尾闸门补测试、验证 artifact、DocsAI 和 owner skill 同步。
- `Protocols/OpenSpecChangeProtocol.md`：框架级计划事实源和 change 生命周期协议。

后续长期任务默认进入 `openspec/changes/<change>/`。`plan` / `ralplan` 只作为澄清、评审和风险收敛手段；需要落盘时写入 OpenSpec artifact。

## Runtime World 入口

Runtime 状态容器入口是 `SlimeAI.GameOS.Runtime.World.RuntimeWorld`。生产和旧调用面通过 `RuntimeWorld.Default` 以及 `EntityManager / LifecycleTree / WorldEvents.World / ResourceCatalog / ObjectPoolManager` static facade 访问；测试和沙箱用 `using var world = RuntimeWorld.CreateScoped();`。规范事实源是 OpenSpec `runtime-world-container`，长期契约同步在 `DocsAI/GameOS/Contracts.md` 与 `DocsAI/GameOS/ApiIndex.md`。

## Runtime CommandBuffer 入口

延迟结构变更入口是 `RuntimeWorld.Commands` 与 `RuntimeWorld.Schedule.RunPhase(SchedulePhase)`。AI 写 Runtime / Capability / GodotBridge callback 时，如果处于 event dispatch、lifecycle callback 或 GodotBridge component callback guard 内，`Spawn / Destroy / Attach / Detach` 会自动入队；不要新增 subsystem 专属 deferred queue，也不要引入 `object Payload` 或 stringly-typed payload。

测试 deferred 路径使用 `using var world = RuntimeWorld.CreateScoped();`，显式 `using var guard = world.Commands.EnterGuard("test")` 后调用结构变更，再用 `world.Schedule.RunPhase(SchedulePhase.Manual)` 或目标 phase 触发 playback。事实源见 `DocsAI/GameOS/Contracts.md#runtime-commandbuffer-契约` 和 `DocsAI/GameOS/ApiIndex.md`。

## Skill 迁移状态

框架仓库根 `.codex/skills/` 包含 Runtime、DataOS、Capability、GodotBridge、Validation、OpenSpec 和 Research 入口型 Skill。游戏仓库只通过这些入口和游戏侧 DocsAI 路由，避免普通游戏开发误改框架内部。
