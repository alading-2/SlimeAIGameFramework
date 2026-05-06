---
name: collision-system
description: 修改 SkilmeAI.GameOS Collision Capability、碰撞层、Hurtbox、ContactDamage、MovementCollision 或对象池碰撞隔离时使用。
---

# Collision Capability 入口

## 必读入口

- `GameOS/SkilmeAI.GameOS.Contracts.md`
- `GameOS/SkilmeAI.GameOS.DebugGuide.md`
- `DocsAI/ProjectState.md`

## 源码位置

- `GameOS/Capabilities/Collision/`
- `GameOS/Capabilities/Movement/`
- `GameOS/GodotBridge/GodotCollision*.cs`
- `GameOS/GodotBridge/GodotHurtboxComponent.cs`
- `GameOS/GodotBridge/GodotContactDamageComponent.cs`
- `GameOS/GodotBridge/GodotCollisionIsolation.cs`

## 规则

- 碰撞过滤统一读取 Collision DataKeys / layer / mask / team 规则。
- 接触伤害只把碰撞转成 DamageService 请求，不直接改 HP。
- 对象池回收 Godot Node 时必须执行碰撞隔离，避免 PhysicsServer2D 旧帧残留。
- MovementCollision 负责命中检测和派发，伤害结算仍走 Damage Capability。

## 验证

```bash
Tools/run-build.sh
Tools/run-tests.sh
cd /home/slime/Code/SkilmeAI/Games/BrotatoLike
Tools/run-godot-scene.sh run-main-smoke --log-dir .ai-temp/scene-tests/runs
```
