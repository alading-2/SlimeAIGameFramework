---
name: test-system
description: 修改 SkilmeAI.GameOS 测试、Validation、Observation、日志分析或测试包装脚本时使用。
---

# Validation / Test 入口

## 必读入口

- `DocsAI/ProjectState.md`
- `GameOS/SkilmeAI.GameOS.DebugGuide.md`
- `Tests/SkilmeAI.GameOS.Tests/`
- `/home/slime/Code/SkilmeAI/Games/BrotatoLike/DocsAI/GodotSceneTesting.md`

## 源码位置

- `Tests/SkilmeAI.GameOS.Tests/`
- `GameOS/Validation/`
- `GameOS/Observation/`
- `Tools/run-build.sh`
- `Tools/run-tests.sh`
- `/home/slime/Code/SkilmeAI/Games/BrotatoLike/Tools/run-godot-scene.sh`

## 规则

- 框架纯逻辑优先补 Runtime tests。
- Godot 场景行为用 BrotatoLike 统一 runner 验证。
- 日志和 artifacts 写到 `.ai-temp/scene-tests/runs/<date>/<time>/`，不要污染源码目录。
- 新能力必须有最小 build / tests / smoke 验证路径。

## 验证

```bash
Tools/run-build.sh
Tools/run-tests.sh
cd /home/slime/Code/SkilmeAI/Games/BrotatoLike
Tools/run-godot-scene.sh run-main-smoke --log-dir .ai-temp/scene-tests/runs
Tools/analyze-godot-scene-logs.sh
```
