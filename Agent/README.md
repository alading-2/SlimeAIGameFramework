# Agent

`Agent` 存放 SkilmeAI 的通用 AI 工作流资产。

## 子域

- `SkillsSource/`：通用 Skill 源头和模板。
- `Protocols/`：AI 开发、Debug、迁移、数据生成等协议。
- `Templates/`：Capability、计划、ResearchBrief 等模板。
- `DocsAI/`：Agent 侧索引和说明。

## Skill 迁移状态

旧 `brotato-my/.codex/skills` 已一次性迁入：

- 可触发入口：`.codex/skills/`
- 可维护源头：`Agent/SkillsSource/`

框架仓库包含 Runtime、DataOS、Capability、GodotBridge、Validation 和 Research 入口型 Skill。游戏仓库只保留游戏任务入口型 Skill，避免普通游戏开发误改框架内部。
