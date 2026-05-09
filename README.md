# SkilmeAI

`SkilmeAI` 是面向 AI 长任务开发游戏的框架主仓库。第一阶段只服务 Godot C# 游戏，不做跨引擎抽象。

## 目录

- `GameOS/`：Godot AI Game OS，包含 Runtime、Capabilities、Validation、Observation 和 GodotBridge。
- `DataOS/`：AI 原生数据层，包含 schema、迁移、生成器、snapshot 和分析。
- `Agent/`：通用 AI 协议、Skill 源头和模板。
- `Packages/`：本地 NuGet、构建输出和发布说明。
- `DocsAI/`：AI 执行入口。
- `Plans/`：历史研究、方向背景和长期参考。
- `../openspec/`：框架级 change 的计划事实源，承载 proposal、design、specs 和 tasks。

## 计划与执行

框架级功能、重构、架构调整和迁移账本默认使用 OpenSpec：

```bash
openspec list --json
openspec status --change "<change>" --json
openspec instructions apply --change "<change>" --json
```

`Plans/` 不再默认新增执行清单；需要长期实施的工作应创建 `openspec/changes/<change>/`。详细规则见 `Agent/Protocols/OpenSpecChangeProtocol.md`。

## 构建

```bash
Tools/run-build.sh
Tools/run-pack.sh
```

当前构建目标是 `GameOS/SkilmeAI.GameOS.csproj`。
