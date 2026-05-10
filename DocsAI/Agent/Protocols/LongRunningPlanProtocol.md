# 长任务计划维护协议

> 更新日期：2026-05-09  
> 状态：生效候选  
> 适用范围：跨多轮推进的 OpenSpec roadmap、child change、迁移账本、框架重构和可玩切片准备工作。

## 什么时候用

任务满足任一条件时使用本协议：

- 需要跨多轮完成。
- 涉及多个 OpenSpec child changes。
- 涉及框架和游戏仓库边界。
- 涉及迁移账本、Observation、Profile、DataOS manifest 或 Runtime RFC。
- 当前轮只能完成一部分，需要把状态留给下一轮。

## 必须读什么

开始前读取：

- `SkilmeAI/DocsAI/Agent/Protocols/OpenSpecChangeProtocol.md`
- 当前 roadmap change 的 `README.md`、`proposal.md`、`design.md`、`specs/**/*.md`、`tasks.md`
- 当前 child change 的全部 OpenSpec artifacts
- `Plans/README.md`
- 受影响模块的协议文档，例如 `CapabilityChangeProtocol.md` 或 `FrameworkVsGameBoundary.md`

如果涉及游戏迁移，再读：

- BrotatoLike game state 文档。
- 当前迁移 ledger。
- 旧行为来源路径。

## 计划维护规则

- OpenSpec 是事实源；不要新增孤立执行 checklist 到 `Plans/`。
- 每完成一个明确任务，立即更新对应 `tasks.md` checkbox。
- child change 创建后，roadmap tasks 应记录对应阶段进度。
- 如果任务被阻塞，写明 blocker、证据、失败命令和下一步。
- 如果发现路线需要调整，修改 roadmap OpenSpec artifact，不散写新计划。
- 不把目录治理和行为修改混在同一批。

## 禁止什么

- 禁止跳过前置 child change 直接推进依赖任务，除非在 child proposal 中写明 waiver 和原因。
- 禁止把不完整 smoke 当作完成证据。
- 禁止在没有迁移账本的情况下标记旧行为 migrated / complete。
- 禁止为了赶进度引入外部 ECS dependency 或复制外部框架 public API。
- 禁止默认运行破坏性 git 命令、commit 或 push。

## 验证什么

每轮至少验证当前 OpenSpec change：

```bash
cd /home/slime/Code/SkilmeAI
openspec validate <change-id> --strict
```

框架代码或核心文档变更后：

```bash
cd /home/slime/Code/SkilmeAI/SkilmeAI
Tools/run-build.sh
Tools/run-tests.sh
```

本阶段暂不运行 Godot 场景。后续恢复 Godot 时，必须把命令、log dir 和 PASS/FAIL artifact 写回对应 change。

## 完成时怎么汇报

长任务每轮结束必须汇报：

- 本轮推进的 roadmap 阶段。
- 创建或修改的 child change id。
- 修改文件。
- 完成的 checkbox。
- 验证命令和结果。
- 未验证原因。
- 剩余风险。
- 下一轮第一个未完成任务。

## 多轮失败时如何复盘

连续失败两次时，必须创建复盘记录，写入当前 child change `tasks.md` 或设计文档：

- 失败目标。
- 失败命令和错误摘要。
- 已排除原因。
- 仍不确定的假设。
- 是否需要拆分任务、调整设计或创建新 proposal。
- 暂停还是继续的条件。

复盘完成前，不继续扩大修改范围。
