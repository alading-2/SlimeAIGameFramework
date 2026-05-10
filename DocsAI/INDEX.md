# SkilmeAI DocsAI 索引

## 入口路径

SkilmeAI 是 AI-first GameOS 框架仓库。AI 执行框架任务时按固定路径进入：

```text
/home/slime/Code/SkilmeAI/AGENTS.md
  -> SkilmeAI/DocsAI/INDEX.md
  -> domain 文档
  -> owner skill / 项目脚本
  -> 验证 artifact
```

## 当前状态

- 当前事实状态：`ProjectState.md`
- 框架总览：`Framework/Overview.md`
- 框架原则：`Framework/Principles.md`
- GameOS 总览：`GameOS/Overview.md`
- GameOS 契约：`GameOS/Contracts.md`
- GameOS API：`GameOS/ApiIndex.md`
- GameOS Debug：`GameOS/DebugGuide.md`
- Capability 索引：`GameOS/Capabilities/CapabilityIndex.md`
- DataOS：`DataOS/Overview.md`
- Agent 协议：`Agent/Overview.md`
- 测试入口：`Tests/GodotSceneTesting.md`

## 按任务路由

| 任务 | 先读 | 再读 / 使用 |
| --- | --- | --- |
| 框架方向、边界、是否该做 | `Framework/Overview.md`、`Framework/Principles.md` | `DocsAI/Agent/Protocols/FrameworkVsGameBoundary.md` |
| Runtime / Data / Event / Entity / Schedule | `GameOS/Contracts.md`、`GameOS/ApiIndex.md` | `project-index`、`ecs-*`、`tools` skills |
| Capability 修改 | `GameOS/Capabilities/CapabilityIndex.md` | 对应 `GameOS/Capabilities/<Name>/Contract.md`、`Debug.md` 和 owner skill |
| GodotBridge / 场景验证 | `Tests/GodotSceneTesting.md` | `godot-scene-test` skill、BrotatoLike Tools wrapper |
| DataOS schema / snapshot / validator | `DataOS/Overview.md`、`DataOS/SnapshotManifest.md` | `data-authoring` skill |
| Agent 协议 / 长任务 / 完成汇报 | `Agent/Overview.md` | `DocsAI/Agent/Protocols/*.md` |
| OpenSpec change | `DocsAI/Agent/Protocols/OpenSpecChangeProtocol.md` | `openspec/changes/<change>/` |

## 当前计划源

框架级可执行计划默认使用仓库根目录的 OpenSpec：

```text
openspec/changes/<change>/
```

`Plans/` 保留为历史研究、方向背景和长期参考，不再作为新任务 checklist 的默认位置。OpenSpec archive 保留历史事实，不要求随当前路径治理反写。

## 文档事实源

- 框架长期知识只写入 `SkilmeAI/DocsAI/`。
- 源码目录中的 `.md` 只允许是 `AGENTS.md`、skill 文档、包/工具操作指针，或 OpenSpec/Plans/Engine/游戏侧文档。
- `.codex/skills` 和 `SkilmeAI/Agent/SkillsSource` 只保存技能路由和命令入口，长期说明链接回 DocsAI。
