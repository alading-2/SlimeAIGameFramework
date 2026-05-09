# SkilmeAI OpenSpec Change Protocol

> 更新日期：2026-05-08  
> 状态：生效  
> 适用范围：框架级功能、重构、架构调整、迁移账本、长期计划和跨目录文档治理。

## 定位

OpenSpec 是 SkilmeAI 框架仓库新的计划事实源。后续可执行计划默认沉淀到 `openspec/changes/<change>/`，而不是新增孤立的 `Plans/*.md` 或 `.omx/plans/*.md`。

OMX `plan` / `ralplan` 的价值保留为思考、澄清、评审和风险收敛；它们不再作为长期计划文件格式。需要落盘时，应把结论写入 OpenSpec artifact。

## Artifact 映射

```text
openspec/changes/<change>/
  proposal.md   # what / why / scope / non-goals
  design.md     # technical approach / tradeoffs / risks
  specs/        # requirements, scenarios, capability deltas
  tasks.md      # implementation checklist and verification work
```

旧计划文件的职责迁移如下：

| 旧职责 | 新位置 |
| --- | --- |
| PRD / 背景 / 为什么做 | `proposal.md` |
| 技术方案 / 架构取舍 | `design.md` |
| 测试规格 / 验收场景 | `specs/` 或 `tasks.md` 的验证项 |
| 执行清单 | `tasks.md` |
| 迁移账本 | 优先放在 change 内；长期基线再同步到 `DocsAI` 或专门账本文档 |
| 历史研究与方向沉淀 | 保留在 `Plans/`，必要时被新 change 引用 |

## 默认流程

1. 探索问题：使用 `openspec-explore` 或普通分析，允许读取代码和文档，不直接改实现。
2. 创建计划：使用 `openspec-propose` 建立 change，并补齐 schema 要求的 artifact。
3. 实施任务：使用 `openspec-apply-change`，实施前读取 CLI 返回的 `contextFiles`。
4. 同步状态：每完成一批任务，更新 `tasks.md` checkbox，并同步受影响的 `DocsAI/ProjectState.md`、Capability Contract、Debug Guide 或游戏侧状态文档。
5. 归档：完成实现、验证和必要 spec 同步后，使用 `openspec-archive-change` 归档到 `openspec/changes/archive/YYYY-MM-DD-<change>/`。

## 实施门禁

框架级实现开始前，应满足：

- `openspec list --json` 能定位到目标 active change，或用户明确指定 change。
- `openspec status --change "<change>" --json` 显示 schema 的 `applyRequires` artifact 已完成。
- `openspec instructions apply --change "<change>" --json` 返回的 `contextFiles` 已读完。
- `tasks.md` 中存在可执行任务和对应验证项。

例外：

- 极小修复、拼写、链接、注释和同步性文档修正可以直接执行，但最终仍要更新相关状态文档。
- 紧急回滚、构建修复或测试修复可以先修，再把原因补入当前 change 或 `DocsAI/ProjectState.md`。

## Plans 目录的新角色

`SkilmeAI/Plans/` 保留为历史研究、方向背景和长期参考，不再默认承载新的执行清单。新增计划优先进入 OpenSpec change。

允许继续放在 `Plans/` 的内容：

- 已完成或正在沉淀的研究简报。
- 不直接触发实现的战略判断。
- 需要长期人工阅读的背景文档。
- 从旧项目迁移来的历史上下文。

不再新增到 `Plans/` 的内容：

- 新功能执行步骤。
- 目录拆分 checklist。
- Capability 实施计划。
- 需要跨多轮持续打勾的任务清单。

## 与现有协议的关系

- `DocsAI/INDEX.md` 是入口索引，不是计划源。
- `DocsAI/ProjectState.md` 记录当前事实状态，不承载未来任务分解。
- Capability 的 Contract / Debug Guide 记录稳定接口和验证方式，不替代 change artifact。
- 游戏侧 BrotatoLike 文档只记录游戏接入状态；框架级设计仍以 SkilmeAI 的 OpenSpec change 为准。

## 验证记录

文档类 change 至少执行：

```bash
cd /home/slime/Code/SkilmeAI
openspec list --json
find openspec -maxdepth 4 -type f | sort
```

代码类 change 按影响面执行：

```bash
cd /home/slime/Code/SkilmeAI/SkilmeAI
Tools/run-build.sh
Tools/run-tests.sh
```

DataOS 或游戏侧改动还要执行对应 validator / Godot scene runner。
