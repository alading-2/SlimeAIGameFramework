---
name: tools
description: 修改 SkilmeAI.GameOS Timer、Pool、ResourceManagement、Target 查询或通用 Runtime 工具时使用。
---

# Runtime Tools 入口

## 必读入口

- `GameOS/SkilmeAI.GameOS.Contracts.md`
- `GameOS/SkilmeAI.GameOS.ApiIndex.md`
- `DocsAI/ProjectState.md`

## 源码位置

- `GameOS/Runtime/Timer/`
- `GameOS/Runtime/Pool/`
- `GameOS/Runtime/Resource/`
- `GameOS/GodotBridge/GodotNodePool*.cs`
- `GameOS/GodotBridge/GodotCollisionIsolation.cs`
- `GameOS/Capabilities/AI/`
- `GameOS/Capabilities/Ability/AbilityTargetingTool.cs`

## 规则

- 计时统一用 `TimerManager`，由外部 Tick 驱动。
- Runtime 池保持纯 C#；Godot Node 池化和碰撞隔离放 GodotBridge。
- 资源加载统一通过 `ResourceManagement` / `ResourceCatalog`，不要在 Capability 中直接 `GD.Load`。
- 目标查询优先复用 AI / Ability targeting 工具，不在热路径手写全局扫描。

## 验证

```bash
Tools/run-build.sh
Tools/run-tests.sh
```
