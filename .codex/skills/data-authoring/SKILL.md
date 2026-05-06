---
name: data-authoring
description: 修改 SkilmeAI DataOS schema、migration、snapshot generator、validator、DataKey authoring 映射或游戏 seed 数据时使用。
---

# DataOS / Authoring 入口

## 必读入口

- `DataOS/README.md`
- `GameOS/SkilmeAI.GameOS.Contracts.md`
- `GameOS/SkilmeAI.GameOS.ApiIndex.md`
- `DocsAI/ProjectState.md`
- `/home/slime/Code/SkilmeAI/Games/BrotatoLike/DocsAI/GameProjectState.md`

## 源码位置

- `DataOS/Schema/`
- `DataOS/Migrations/`
- `DataOS/Generators/`
- `DataOS/Validation/`
- `GameOS/Runtime/Data/`
- `GameOS/Capabilities/*/*DataKeys.cs`
- `/home/slime/Code/SkilmeAI/Games/BrotatoLike/DataOS/Authoring/BrotatoLike.seed.sql`

## 规则

- Authoring 数据写 SQLite seed / migration，运行时消费 generated snapshot。
- Runtime DataKey 用 `DataMeta` 分域注册，不新增裸字符串访问。
- 新字段必须同步 generator / validator / contracts / 游戏 smoke 断言。
- 游戏专有 seed 留在游戏仓库，不写入框架通用 DataOS seed。

## 验证

```bash
Tools/run-build.sh
Tools/run-tests.sh
cd /home/slime/Code/SkilmeAI/Games/BrotatoLike
Tools/run-build.sh
Tools/run-godot-scene.sh run-main-smoke --log-dir .ai-temp/scene-tests/runs
```
