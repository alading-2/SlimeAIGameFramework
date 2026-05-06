---
name: ecs-event
description: 修改 SkilmeAI.GameOS EventBus、GlobalEventBus、GameEventType 或事件通信协议时使用。
---

# Runtime Event 入口

## 必读入口

- `GameOS/SkilmeAI.GameOS.Contracts.md`
- `GameOS/SkilmeAI.GameOS.ApiIndex.md`
- `DocsAI/ProjectState.md`

## 源码位置

- `GameOS/Runtime/Event/`
- `GameOS/Runtime/Data/EventDataChangeSink.cs`
- `Tests/SkilmeAI.GameOS.Tests/`

## 规则

- 实体内通信优先 `RuntimeEntity.Events`。
- 系统级低频广播才用 `GlobalEventBus.Global`。
- 新事件名优先挂到对应 Capability 的 `GameEventType` 分组。
- 订阅方必须可清理，不把事件当状态存储。

## 验证

```bash
Tools/run-build.sh
Tools/run-tests.sh
```
