---
name: projectile-effect-system
description: 修改 SkilmeAI.GameOS ProjectileTool、EffectTool、投射物命中生命周期、穿透、视觉实例化或 Effect 动画播放时使用。
---

# Projectile / Effect 入口

## 必读入口

- `GameOS/SkilmeAI.GameOS.Contracts.md`
- `GameOS/SkilmeAI.GameOS.ApiIndex.md`
- `DocsAI/ProjectState.md`

## 源码位置

- `GameOS/Capabilities/Projectile/`
- `GameOS/Capabilities/Effect/`
- `GameOS/Capabilities/Movement/`
- `GameOS/Capabilities/Damage/`
- `GameOS/GodotBridge/GodotProjectileEffectSpawner.cs`
- `Tests/SkilmeAI.GameOS.Tests/`

## 规则

- Runtime 只保存 `res://` 场景路径，不在纯 Runtime 加载 Godot 资源。
- 投射物命中通过 MovementCollision 转 `DamageService` 请求。
- 穿透、最大命中数、生命周期统一读 `ProjectileDataKeys` / `ProjectileMovementOptions`。
- 视觉实例化和动画播放放 GodotBridge 或游戏侧，不写进纯 Runtime。

## 验证

```bash
Tools/run-build.sh
Tools/run-tests.sh
cd /home/slime/Code/SkilmeAI/Games/BrotatoLike
Tools/run-godot-scene.sh run-main-smoke --log-dir .ai-temp/scene-tests/runs
```
