---
name: movement-system
description: 修改 SkilmeAI.GameOS Movement Capability、MovementDataKeys、MovementSystem、运动策略、运动碰撞或 Godot 位移桥时使用。
---

# Movement Capability 入口

## 必读入口

- `GameOS/SkilmeAI.GameOS.Contracts.md`
- `GameOS/SkilmeAI.GameOS.ApiIndex.md`
- `DocsAI/ProjectState.md`

## 源码位置

- `GameOS/Capabilities/Movement/`
- `GameOS/GodotBridge/GodotMovementDriver.cs`
- `GameOS/GodotBridge/GodotOrientationComponent.cs`
- `Tests/SkilmeAI.GameOS.Tests/`

## 规则

- 对外角度输入使用度，语义为 `0=右、90=下、180=左、正值顺时针`。
- 数值型“不限制”统一用 `-1`，例如最大距离 / 最大时长。
- 纯 Movement 不依赖 `Godot.Vector2` 或 `Node2D`；Godot 同步写在 bridge。
- 新策略通过 `IMovementStrategy` / `MovementStrategyRegistry` 接入，并补 Runtime 测试和 BrotatoLike smoke。

## 验证

```bash
Tools/run-build.sh
Tools/run-tests.sh
cd /home/slime/Code/SkilmeAI/Games/BrotatoLike
Tools/run-godot-scene.sh run-main-smoke --log-dir .ai-temp/scene-tests/runs
```
