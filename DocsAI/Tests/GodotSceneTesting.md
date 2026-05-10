# Godot Scene Testing

## 事实源

Godot 场景测试脚本事实源：

```text
/home/slime/Code/SkilmeAI/.codex/skills/godot-scene-test/scripts/
  godot-scene-runner.mjs
  analyze-logs.sh
  run-test.sh
```

BrotatoLike 保留薄封装：

```text
/home/slime/Code/SkilmeAI/Games/BrotatoLike/Tools/run-godot-scene.sh
/home/slime/Code/SkilmeAI/Games/BrotatoLike/Tools/analyze-godot-scene-logs.sh
```

## 常用命令

```bash
cd /home/slime/Code/SkilmeAI/Games/BrotatoLike
Tools/run-build.sh
Tools/run-godot-scene.sh list
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

需要看 Godot 打印记录时，优先打开 analyzer 输出的 `combinedLog`；原始 stdout 在同一 attempt 目录的 `stdout.log`。Observation 日志验证场景还会写 `artifacts/observation-log-validation.json`。

## 判定规则

- PASS 需要 Godot 进程 exit code 为 `0`，并且场景 artifact 或日志 marker 明确 PASS。
- FAIL marker、非零 exit code、artifact `status=fail` 都是失败。
- 只有“没有 `ERROR:`”不能视为通过。

## 规格入口

底层验证场景命名、README、artifact 字段和 PASS/FAIL marker 规则见 `Tests/GodotSceneValidation.md`。
