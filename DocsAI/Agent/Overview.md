# Agent

`Agent` 存放 SkilmeAI 的通用 AI 工作流协议文档。可执行 skill 的唯一源头在仓库根 `.codex/skills/`。

## 子域

- `Protocols/`：AI 开发、Debug、迁移、OpenSpec change、数据生成等协议。
- `Templates/`：Capability、计划、ResearchBrief 等模板。
- `DocsAI/`：Agent 侧索引和说明。

## 计划协议

- `Protocols/OpenSpecChangeProtocol.md`：框架级计划事实源和 change 生命周期协议。

后续长期任务默认进入 `openspec/changes/<change>/`。`plan` / `ralplan` 只作为澄清、评审和风险收敛手段；需要落盘时写入 OpenSpec artifact。

## Skill 迁移状态

框架仓库根 `.codex/skills/` 包含 Runtime、DataOS、Capability、GodotBridge、Validation、OpenSpec 和 Research 入口型 Skill。游戏仓库只通过这些入口和游戏侧 DocsAI 路由，避免普通游戏开发误改框架内部。
