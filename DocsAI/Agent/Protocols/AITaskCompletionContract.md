# AI 任务完成契约

> 更新日期：2026-05-09  
> 状态：生效候选  
> 适用范围：每轮 AI 修改、排查、计划推进或验证完成后的最终汇报。

## 什么时候用

每轮任务结束前都使用本契约。无论任务是文档、OpenSpec、代码、测试、DataOS、GodotBridge 还是游戏侧迁移，都必须给出可检查的完成说明。

## 必须读什么

结束前至少检查：

- 当前 OpenSpec change 的 `tasks.md`。
- 本轮实际修改过的文件。
- 本轮运行过的验证命令输出。
- `git status --short`。
- 如果涉及 Capability，检查 `CapabilityIndex.md` 是否需要同步。
- 如果涉及计划状态，检查 roadmap 或对应状态文档是否需要同步。
- 如果是新功能，检查是否已有专项 Runtime test 或独立 Godot 验证场景；没有时必须说明原因。

## 汇报字段

最终汇报必须包含：

- 本轮处理阶段或 change id。
- primary category 和 secondary categories。
- 修改文件列表。
- 已完成的 `tasks.md` checkbox。
- 运行的验证命令和结果。
- 未运行验证的原因。
- 剩余风险。
- 下一步建议。

如果用户明确要求某种输出格式，优先满足用户格式，但不能省略验证结果和未验证原因。

## 禁止什么

- 禁止只说“已完成”而不列出改了什么。
- 禁止把未运行的验证说成通过。
- 禁止隐藏 build/test 失败。
- 禁止把 smoke 通过写成 playable slice 完成。
- 禁止在未明确要求时 commit 或 push。
- 禁止回滚、覆盖或删除用户已有无关改动。

## 验证什么

按影响面选择：

文档 / OpenSpec：

```bash
openspec validate <change-id> --strict
```

框架代码：

```bash
cd /home/slime/Code/SkilmeAI/SkilmeAI
Tools/run-build.sh
Tools/run-tests.sh
```

DataOS：

```bash
cd /home/slime/Code/SkilmeAI/SkilmeAI
Tools/run-dataos-validate.sh
```

Godot 场景：

```bash
cd /home/slime/Code/SkilmeAI/Games/BrotatoLike
Tools/run-godot-scene.sh run res://Scenes/Validation/<Area>/<Layer>/<Scene>.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
Tools/analyze-godot-scene-logs.sh
```

如果本轮只需要回归 smoke：

```bash
cd /home/slime/Code/SkilmeAI/Games/BrotatoLike
Tools/run-godot-scene.sh run-main-smoke --log-dir .ai-temp/scene-tests/runs
Tools/analyze-godot-scene-logs.sh
```

`run-main-smoke` 不能替代新功能专项验证场景。Godot 环境不可用、耗时过长或用户要求跳过时，最终汇报必须写明未运行的具体命令和原因。

## 多轮失败时如何复盘

如果本轮没有完成，应说明：

- 卡在哪个任务。
- 是需求不清、设计冲突、构建失败、测试失败、环境不可用还是证据不足。
- 已尝试命令。
- 下轮继续前必须读哪些文件。
- 是否需要新 OpenSpec proposal 或拆分任务。

连续失败两次时，必须写出错误摘要和备选方案，不能只继续尝试同一种修复。
