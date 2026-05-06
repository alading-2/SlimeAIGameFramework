---
name: ecs-component
description: 修改 SkilmeAI.GameOS GodotBridge 组件协议、IGodotComponent 或可挂节点组件时使用。
---

# Component / GodotBridge 入口

## 必读入口

- `GameOS/SkilmeAI.GameOS.Contracts.md`
- `GameOS/SkilmeAI.GameOS.ApiIndex.md`
- `GameOS/SkilmeAI.GameOS.DebugGuide.md`
- `DocsAI/ProjectState.md`

## 源码位置

- `GameOS/GodotBridge/`
- `GameOS/Runtime/Relationship/`
- `GameOS/Capabilities/*/`
- `/home/slime/Code/SkilmeAI/Games/BrotatoLike/Src/Game/`

## 规则

- 框架组件优先实现 `IGodotComponent`，注册时通过 `GameOSGodotBridge.RegisterComponents` 接入实体。
- 组件业务状态写入 `Entity.Data`，不要用私有字段作为长期状态真相。
- 组件间通信走 `Entity.Events` 或 Runtime 服务，不直接互调具体组件方法。
- `_Process` 中避免分配对象和 LINQ。

## 验证

```bash
Tools/run-build.sh
Tools/run-tests.sh
cd /home/slime/Code/SkilmeAI/Games/BrotatoLike
Tools/run-godot-scene.sh run-main-smoke --log-dir .ai-temp/scene-tests/runs
```
