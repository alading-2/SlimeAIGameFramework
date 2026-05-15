# SlimeAI OpenSpec Change Protocol

> 更新日期：2026-05-08  
> 状态：生效  
> 适用范围：框架级功能、重构、架构调整、迁移账本、长期计划和跨目录文档治理。

## 定位

OpenSpec 是 SlimeAI 框架仓库新的计划事实源。后续可执行计划默认沉淀到 `openspec/changes/<change>/`，完成后把规范要求合入 `openspec/specs/`。

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
| 历史研究与方向沉淀 | 提炼进 `SlimeAI/DocsAI/` 或 `openspec/specs/`，不保留执行 checklist |

## 默认流程

0. 任务入口：AI 接到框架功能、重构、迁移或 bugfix 任务后，先读 `ai-feature-development` skill（统一源 `.ai-config/skills/ai/ai-feature-development/SKILL.md`，工具副本 `.codex/skills/ai-feature-development`），再判断是否进入 OpenSpec。
1. 探索问题：使用 `openspec-explore` 或普通分析，允许读取代码和文档，不直接改实现。
2. 创建计划：使用 `openspec-propose` 建立 change，并补齐 schema 要求的 artifact。
3. 实施任务：使用 `openspec-apply-change`，实施前读取 CLI 返回的 `contextFiles`。
4. 同步状态：每完成一批任务，更新 `tasks.md` checkbox，并同步受影响的 `DocsAI/ProjectState.md`、Capability Contract、Debug Guide 或游戏侧状态文档。
5. 完成：完成实现、验证和必要 spec 同步后，将要求合入 `openspec/specs/`，并移除已完成 change 的执行历史。

## 实施门禁

框架级实现开始前，应满足：

- `openspec list --json` 能定位到目标 active change，或用户明确指定 change。
- `openspec status --change "<change>" --json` 显示 schema 的 `applyRequires` artifact 已完成。
- `openspec instructions apply --change "<change>" --json` 返回的 `contextFiles` 已读完。
- `tasks.md` 中存在可执行任务和对应验证项。

例外：

- 极小修复、拼写、链接、注释和同步性文档修正可以直接执行，但最终仍要更新相关状态文档。
- 紧急回滚、构建修复或测试修复可以先修，再把原因补入当前 change 或 `DocsAI/ProjectState.md`。

## 历史计划材料的新角色

旧计划材料只在有长期价值时提炼为 `SlimeAI/DocsAI/` 文档或 `openspec/specs/` 规范。不要新增孤立执行 checklist，也不要把完成后的 OpenSpec change 历史作为当前 AI 入口保留。

## 与现有协议的关系

- `DocsAI/INDEX.md` 是入口索引，不是计划源。
- `DocsAI/ProjectState.md` 记录当前事实状态，不承载未来任务分解。
- Capability 的 Contract / Debug Guide 记录稳定接口和验证方式，不替代 change artifact。
- 游戏侧 BrotatoLike 文档只记录游戏接入状态；框架级设计仍以 SlimeAI 的 OpenSpec change 为准。

## 验证记录

文档类 change 至少执行：

```bash
cd /home/slime/Code/SlimeAI
openspec list --json
find openspec -maxdepth 4 -type f | sort
```

代码类 change 按影响面执行：

```bash
cd /home/slime/Code/SlimeAI/SlimeAI
Tools/run-build.sh
Tools/run-tests.sh
```

DataOS 或游戏侧改动还要执行对应 validator / Godot scene runner。
