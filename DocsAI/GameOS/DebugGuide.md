# SlimeAI.GameOS Debug Guide

## 当前阶段

当前验证框架能独立构建、Runtime 行为测试通过，并且 GodotBridge 可被游戏仓库编译引用。GodotBridge Node 对象池 / 碰撞隔离已通过 BrotatoLike `Tools/run-godot-smoke.sh` headless smoke。Movement Capability 第一段纯 C# Charge / Orbit / SineWave / Parabola / CircularArc 和 Godot 2D 位移桥已迁入，并被框架测试和 BrotatoLike smoke 覆盖。

## 命令

```bash
Tools/run-build.sh
Tools/run-tests.sh
dotnet build GameOS/SlimeAI.GameOS.csproj
cd /home/slime/Code/SlimeAI/Games/BrotatoLike && Tools/run-godot-smoke.sh
```

## Observation

- Observation artifact contract：`DocsAI/GameOS/Observation.md`。
- BrotatoLike scene runner 日志目录：`.ai-temp/scene-tests/runs/<date>/<time>/index.json` 加 per-scene attempt 目录。
- 结构化验收证据默认写入 `<scene-attempt>/artifacts/`，JSONL 写入 `artifacts/logs/scene-log.jsonl`，截图写入 `screenshots/`。
- 可玩切片必须看明确 PASS/FAIL marker 或 `scene-acceptance.json`，不能只看没有 `ERROR:`。

## 常见问题

- 找不到 `Godot.NET.Sdk/4.6.2`：检查 Godot .NET SDK 安装。
- 游戏仓库找不到框架项目：检查游戏仓库 `DocsAI/ExternalFrameworkMap.md` 中的 `framework_project`。
- GodotBridge 场景无法运行：先确认 `GODOT_BIN`，默认使用 `/home/slime/Code/Godot/GodotResources/Engine/4.x/Godot_v4.6.2-stable_mono_linux_x86_64/Godot_v4.6.2-stable_mono_linux.x86_64`；`--build-solutions` 必须配合 `--quit`，否则 editor 初始化后会常驻。
- 池化物理节点回收后仍触发碰撞：确认对象使用 `GodotNodePool<T>`，根节点是 `CollisionObject2D` 时默认会移动到泊车位、递归隔离碰撞并脱树；出池前需要设置好 `GodotNodePoolConfig.ActiveParent`。
- Movement 目标点没有停止：确认使用 `MoveMode.Charge`、`TargetPosition` 非空、`StopAtTarget = true`，并且外部循环调用了 `MovementSystem.Tick(delta)`。
- Godot 2D 节点没有跟随 Movement：确认实体是 `GodotEntity2D` 或其它 `Node2D + IEntity`，场景中存在 `GodotMovementDriver`，并且 `AutoTick` 开启或手动调用了 `TickMovement(delta)`。
