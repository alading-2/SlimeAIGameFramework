---
name: project-index
description: SkilmeAI 框架仓库导航入口。用于查找 GameOS、DataOS、Agent、Packages、测试、计划和迁移输入位置。
---

# SkilmeAI 项目导航

## 必读入口

- `DocsAI/INDEX.md`
- `DocsAI/ProjectState.md`
- `Plans/README.md`
- `GameOS/SkilmeAI.GameOS.Contracts.md`
- `GameOS/SkilmeAI.GameOS.ApiIndex.md`
- `GameOS/SkilmeAI.GameOS.DebugGuide.md`

## 查找规则

- 找 Runtime / Capability API：先看 `GameOS/SkilmeAI.GameOS.Contracts.md` 和 `GameOS/SkilmeAI.GameOS.ApiIndex.md`。
- 找源码：查 `GameOS/Runtime/`、`GameOS/Capabilities/`、`GameOS/GodotBridge/`。
- 找 DataOS：查 `DataOS/README.md`、`DataOS/Schema/`、`DataOS/Migrations/`、`DataOS/Generators/`、`DataOS/Validation/`。
- 找测试：查 `Tests/SkilmeAI.GameOS.Tests/` 和 `Tools/run-tests.sh`。
- 找迁移来源：旧输入仓库是 `/home/slime/Code/Godot/Games/MyGames/brotato-my`。
- 找第一个游戏接入验证：游戏仓库是 `/home/slime/Code/SkilmeAI/Games/BrotatoLike`。

## 常用命令

```bash
rg -n "<关键词>" DocsAI GameOS DataOS Tests Tools Plans
find GameOS -maxdepth 4 -type f | sort
Tools/run-build.sh
Tools/run-tests.sh
```

## 边界

- 框架仓库只改通用 Runtime、Capability、DataOS、Validation、Observation、Agent 和包发布逻辑。
- 游戏资产、游戏特定场景和 BrotatoLike 玩法胶水默认放到 `Games/BrotatoLike`。
- 不从旧仓库复制长期架构入口；旧仓库只作为迁移输入和历史对照。
