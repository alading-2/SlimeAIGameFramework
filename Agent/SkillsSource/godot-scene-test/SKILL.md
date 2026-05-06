---
name: godot-scene-test
description: 需要通过 BrotatoLike 运行 Godot headless 场景、主场景 smoke 或分析 Godot 日志时使用。
---

# Godot 场景测试入口

## 运行位置

Godot 场景测试在第一个游戏仓库运行：

```bash
cd /home/slime/Code/SkilmeAI/Games/BrotatoLike
```

## 必读入口

- `DocsAI/GodotSceneTesting.md`
- `DocsAI/GameProjectState.md`
- `Tools/run-godot-scene.sh`
- `Tools/analyze-godot-scene-logs.sh`

## 常用命令

```bash
Tools/run-build.sh
Tools/run-godot-scene.sh list
Tools/run-godot-scene.sh run-main-smoke --log-dir .ai-temp/scene-tests/runs
Tools/run-godot-scene.sh run res://Scenes/Main.tscn --timeout 3 --log-dir .ai-temp/scene-tests/runs
Tools/analyze-godot-scene-logs.sh
```

## 已迁 bundled scripts

- `scripts/run-test.sh`
- `scripts/analyze-logs.sh`
- `scripts/godot-scene-runner.mjs`

## 规则

- 框架修改后如影响 GodotBridge / Capability bridge，必须回到 BrotatoLike 跑 headless smoke。
- `Tools/run-godot-smoke.sh` 只是兼容入口，优先用统一 runner。
- 日志和截图 artifacts 保持在 `.ai-temp/scene-tests/runs`。
