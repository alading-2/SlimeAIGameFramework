# SlimeAI DocsAI 索引

## 入口路径

SlimeAI 是 AI-first GameOS 框架仓库。AI 执行框架任务时按固定路径进入：

```text
/home/slime/Code/SlimeAI/AGENTS.md
  -> SlimeAI/DocsAI/INDEX.md
  -> domain 文档
  -> owner skill / 项目脚本
  -> 验证 artifact
```

## 当前状态

- 当前事实状态：`ProjectState.md`
- 框架总览：`Framework/Overview.md`
- 框架原则：`Framework/Principles.md`
- 架构决策记录：`ArchitectureDecisionRecords)/深度分析：AI-first GameOS 与 ECS 概念边界.md`
- Godot C# 约束：`Framework/GodotCSharpConstraints.md`
- 多游戏架构：`Framework/MultiGameLayout.md`
- GameOS 总览：`GameOS/Overview.md`
- GameOS 契约：`GameOS/Contracts.md`
- GameOS API：`GameOS/ApiIndex.md`
- GameOS 术语护栏：`GameOS/Overview.md#术语表`、`ArchitectureDecisionRecords)/深度分析：AI-first GameOS 与 ECS 概念边界.md`
- GameOS Debug：`GameOS/DebugGuide.md`
- Capability 索引：`GameOS/Capabilities/CapabilityIndex.md`
- DataOS：`DataOS/Overview.md`
- Agent 协议：`Workspace/SystemAgent/Protocols/`
- 工作区 AI agent 流程总纲：`Workspace/SystemAgent/INDEX.md`
- Runtime 测试入口：`Tests/RuntimeTests.md`
- Godot 场景测试入口：`Tests/GodotSceneTesting.md`
- Godot 场景验证：`Tests/GodotSceneValidation.md`
- 验证场景标准答案索引：`Tests/ValidationCatalog.md`
- 新功能开发入口：`.ai-config/skills/ai/ai-feature-development/SKILL.md`（统一源）、`.codex/skills/ai-feature-development`（工具副本）、`Workspace/SystemAgent/Protocols/AIFeatureDevelopmentProtocol.md`

## 按任务路由

| 任务 | 先读 | 再读 / 使用 |
| --- | --- | --- |
| 新功能、功能扩展、迁移旧逻辑、框架重构 | `Workspace/SystemAgent/INDEX.md`、`Workspace/SystemAgent/Protocols/AIFeatureDevelopmentProtocol.md`、`Framework/Principles.md` | `.ai-config/skills/ai/ai-feature-development/SKILL.md` / `.codex/skills/ai-feature-development`、对应 owner skill、必要时 OpenSpec；计划阶段先写验收标准，结束前补测试场景 / artifact / DocsAI / retrospective |
| 框架方向、边界、是否该做 | `Framework/Overview.md`、`Framework/Principles.md` | `Workspace/SystemAgent/Protocols/FrameworkVsGameBoundary.md` |
| Godot C# 场景/脚本加载约束 | `Framework/GodotCSharpConstraints.md` | 引擎源码 `Resources/Engine/Engine/godot-4.6.2-stable/modules/mono/` |
| 多游戏架构、submodule 工作流 | `Framework/MultiGameLayout.md` | 游戏仓 `.gitmodules`、`BrotatoLike.csproj` |
| Runtime / Data / Event / Entity / Schedule | `GameOS/Contracts.md`、`GameOS/ApiIndex.md` | `project-index`、`ecs-*`、`tools` skills |
| ECS / Entity / Component / System / Capability 命名敏感任务 | `GameOS/Overview.md#术语表`、`ArchitectureDecisionRecords)/深度分析：AI-first GameOS 与 ECS 概念边界.md` | 使用 AI-first GameOS / Capability Composition Runtime 术语；legacy code symbol 只按兼容名处理，不直接做 public API rename |
| Capability 修改 | `GameOS/Capabilities/CapabilityIndex.md` | 对应 `GameOS/Capabilities/<Name>/Contract.md`、`Debug.md` 和 owner skill |
| Runtime 纯逻辑测试 | `Tests/RuntimeTests.md` | `Tests/SlimeAI.GameOS.Tests/`、`Tools/run-tests.sh`；不启动 Godot |
| GodotBridge / 场景验证 | `Tests/GodotSceneTesting.md` | `godot-scene-test` skill、BrotatoLike Tools wrapper |
| DataOS schema / snapshot / validator | `DataOS/Overview.md`、`DataOS/SnapshotManifest.md` | `data-authoring` skill |
| Agent 协议 / 长任务 / 完成汇报 | `Workspace/SystemAgent/INDEX.md` | `Workspace/SystemAgent/Protocols/*.md` |
| OpenSpec change | `Workspace/SystemAgent/Protocols/OpenSpecChangeProtocol.md` | `openspec/changes/<change>/` |

## 当前计划源

框架级可执行计划默认使用仓库根目录的 OpenSpec：

```text
openspec/changes/<change>/
```

当前可执行计划只使用 OpenSpec active change；完成后的要求合入 `openspec/specs/` 作为基线，不保留旧执行 checklist 或 archive 历史作为 AI 入口。

## ai-feature-development references

`ai-feature-development` skill 统一源在 `.ai-config/skills/ai/ai-feature-development/`，references 当前固定为 12 个文件：

- `references/workflow-governance.md`
- `references/validation-closure.md`
- `references/framework-research-filter.md`
- `references/refactor-decision-tree.md`
- `references/typed-value-design.md`
- `references/rename-pipeline.md`
- `references/framework-game-boundary.md`
- `references/structural-change-guard.md`
- `references/runtime-world-facade.md`
- `references/lifecycle-vs-business-reference.md`
- `references/spec-code-alignment-check.md`
- `references/skill-sync-discipline.md`

## 文档事实源

- 框架长期知识只写入 `SlimeAI/DocsAI/`；Agent 工作流不再放在框架仓 DocsAI 子目录。
- AI agent 工作流、协议、角色、hooks/subagent/MCP/git 策略长期知识写入 `Workspace/SystemAgent/`。
- 源码目录中的 `.md` 只允许是 `AGENTS.md`、skill 文档、包/工具操作指针，或 OpenSpec/Resources/Engine/游戏侧文档。
- `.ai-config/skills` 是唯一可维护 skill 源；`.codex/skills`、`.claude/skills`、`.windsurf/skills` 是同步副本，只保存技能路由和命令入口，长期说明链接回 DocsAI。
