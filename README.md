# SkilmeAI

`SkilmeAI` 是面向 AI 长任务开发游戏的框架主仓库。第一阶段只服务 Godot C# 游戏，不做跨引擎抽象。

## 目录

- `GameOS/`：Godot AI Game OS，包含 Runtime、Capabilities、Validation、Observation 和 GodotBridge。
- `DataOS/`：AI 原生数据层，包含 schema、迁移、生成器、snapshot 和分析。
- `Agent/`：通用 AI 协议文档。
- `Packages/`：本地 NuGet、构建输出和发布说明。
- `DocsAI/`：AI 执行入口。
- `../openspec/`：框架级 change 的计划事实源和规范基线，承载 active change 与 `specs/`。

## 计划与执行

框架级功能、重构、架构调整和迁移账本默认使用 OpenSpec：

```bash
openspec list --json
openspec status --change "<change>" --json
openspec instructions apply --change "<change>" --json
```

需要长期实施的工作应创建 `openspec/changes/<change>/`；完成后把要求合入 `openspec/specs/`。详细规则见 `DocsAI/Agent/Protocols/OpenSpecChangeProtocol.md`。

## 构建

```bash
Tools/run-build.sh
Tools/run-pack.sh
```

当前构建目标是 `GameOS/SkilmeAI.GameOS.csproj`。
