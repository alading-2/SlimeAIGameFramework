# Godot Scene Testing

## 事实源

Godot 场景测试脚本事实源：

```text
/home/slime/Code/SlimeAI/.codex/skills/godot-scene-test/scripts/
  godot-scene-runner.mjs
  analyze-logs.sh
  run-test.sh
```

BrotatoLike 保留薄封装：

```text
/home/slime/Code/SlimeAI/Games/BrotatoLike/Tools/run-godot-scene.sh
/home/slime/Code/SlimeAI/Games/BrotatoLike/Tools/analyze-godot-scene-logs.sh
```

游戏仓里的 `SlimeAI/` 是框架仓 git submodule 镜像。当前初始开发阶段，框架仓新增或修改 `Scenes/Validation/...`、`Src/SceneTests/...` 后，默认选 BrotatoLike 作为承载游戏，并直接同步到 `Games/BrotatoLike/SlimeAI/` 工作树以跑通验证。后续多游戏 / 成品阶段不默认同步所有游戏，改按每个游戏的框架版本策略更新 submodule 指针，再选择对应承载游戏跑验证。

承载游戏 wrapper 的 scan roots 必须包含游戏自身 `Scenes,Src` 和框架镜像 `SlimeAI/Scenes,SlimeAI/Src`；承载游戏 `.csproj` 需要排除框架源码但重新包含 `SlimeAI/Src/SceneTests/**/*.cs`。

## 常用命令

```bash
cd /home/slime/Code/SlimeAI/Games/BrotatoLike
Tools/run-build.sh
Tools/run-godot-scene.sh list
Tools/run-godot-scene.sh run res://SlimeAI/Scenes/Validation/Runtime/Data/RuntimeDataValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
Tools/run-godot-scene.sh run res://SlimeAI/Scenes/Validation/Runtime/Event/RuntimeEventValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
Tools/run-godot-scene.sh run res://Scenes/Validation/GameOS/Observation/ObservationLogValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
Tools/run-godot-scene.sh run-main-smoke --log-dir .ai-temp/scene-tests/runs
Tools/run-godot-scene.sh run res://Scenes/Main.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
Tools/analyze-godot-scene-logs.sh
```

## 日志结构

使用 `--log-dir .ai-temp/scene-tests/runs` 时，runner 输出：

```text
.ai-temp/scene-tests/runs/<date>/<time>/
  index.json
  001_<scene>_attempt1/
    stdout.log
    stderr.log
    combined.log
    result.json
    screenshots/
    artifacts/
      logs/scene-log.jsonl
```

优先读取：

- `index.json`
- per-scene `result.json`
- `combined.log`
- `artifacts/logs/scene-log.jsonl`

需要看 Godot 打印记录时，优先打开 analyzer 输出的 `combinedLog`；原始 stdout 在同一 attempt 目录的 `stdout.log`。Runtime/Data 场景写 `artifacts/runtime-data-validation.json`，Runtime/Event 场景写 `artifacts/runtime-event-validation.json`，Observation 日志验证场景还会写 `artifacts/observation-log-validation.json`。

## 判定规则

- PASS 需要 Godot 进程 exit code 为 `0`，并且场景 artifact 或日志 marker 明确 PASS。
- FAIL marker、非零 exit code、artifact `status=fail` 都是失败。
- 只有“没有 `ERROR:`”不能视为通过。

## 规格入口

底层验证场景命名、README、artifact 字段和 PASS/FAIL marker 规则见 `Tests/GodotSceneValidation.md`。
