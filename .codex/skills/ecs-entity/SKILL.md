---
name: ecs-entity
description: 修改 SkilmeAI.GameOS RuntimeEntity、EntityManager、Relationship 或实体生命周期时使用。
---

# Runtime Entity 入口

## 必读入口

- `GameOS/SkilmeAI.GameOS.Contracts.md`
- `GameOS/SkilmeAI.GameOS.ApiIndex.md`
- `DocsAI/ProjectState.md`

## 源码位置

- `GameOS/Runtime/Entity/`
- `GameOS/Runtime/Relationship/`
- `GameOS/GodotBridge/GodotEntity.cs`
- `GameOS/GodotBridge/GodotEntity2D.cs`
- `Tests/SkilmeAI.GameOS.Tests/`

## 规则

- 创建实体走 `EntityManager.Spawn/Register`。
- 销毁实体走 `EntityManager.Destroy`，需要关系级联时用 Relationship 配置。
- `IEntity` 只暴露 `EntityId`、`Data`、`Events`，业务逻辑放 Capability / 系统 / bridge。
- Godot Node 生命周期适配放 `GameOS/GodotBridge/`，不要污染纯 Runtime。

## 验证

```bash
Tools/run-build.sh
Tools/run-tests.sh
```
