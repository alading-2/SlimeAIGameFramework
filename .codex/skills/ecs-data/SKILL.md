---
name: ecs-data
description: 修改 SkilmeAI.GameOS Runtime Data、DataMeta、DataRegistry、RuntimeDataSnapshot 或数据变更事件时使用。
---

# Runtime Data 入口

## 必读入口

- `GameOS/SkilmeAI.GameOS.Contracts.md`
- `GameOS/SkilmeAI.GameOS.ApiIndex.md`
- `DocsAI/ProjectState.md`
- `DataOS/README.md`

## 源码位置

- `GameOS/Runtime/Data/`
- `GameOS/Runtime/Event/EventDataChangeSink.cs`
- `GameOS/Runtime/Data/RuntimeDataSnapshot.cs`
- `Tests/SkilmeAI.GameOS.Tests/`

## 规则

- `Data` 只存运行时状态，不承担 authoring 表职责。
- 新 DataKey 用 `DataMeta` 注册到对应 Runtime / Capability DataKeys，不新增字符串字面量访问。
- Data 变更通知通过 `IDataChangeSink` 和 `Entity.Events`，不要把业务监听写进 `Data` 容器。
- DataOS SQLite 只在生成 / 校验 / snapshot 阶段使用，运行时热路径读取 `RuntimeDataSnapshot`。

## 验证

```bash
Tools/run-build.sh
Tools/run-tests.sh
```
