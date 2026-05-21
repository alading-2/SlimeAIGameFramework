# Observation Log Validation

## 测试目标

验证 GameOS Observation 日志与 `SceneValidationSession` artifact 路径：日志级别格式、最小等级过滤、JSONL sink，以及 scene artifact 输出位置。

## expectedInputs

- `GameOSLog` 配置为 Trace minimum、stdout enabled、JSONL enabled。
- memory sink 与 JSONL sink 的全等级样例日志。
- `GameOSObservationSession` 从 runner 环境读取或回退生成的 artifact 路径。

## expectedObservations

- 日志级别按稳定文本格式输出 context 和结构化 values。
- minimum level filtering 能过滤低等级日志。
- JSONL 和 observation session 路径创建在 scene artifact 目录下。

## passCriteria

- stdout 包含 `GameOS Observation Log validation PASS`。
- artifact `observation-log-validation.json` 的 `status` 为 `pass`。
- artifact `failureReasons` 为空，四个检查项均为 `pass`。

## failCriteria

- stdout 包含 `GameOS Observation Log validation FAIL`。
- formatting、filtering、JSONL 或 observation path 任一检查失败。
- artifact 标准答案字段缺失或 `failureReasons` 非空。

## artifactPath

```text
.ai-temp/scene-tests/runs/<date>/<time>/<scene-attempt>/artifacts/observation-log-validation.json
```

## 运行命令

```bash
cd /home/slime/Code/SlimeAI/Games/BrotatoLike
Tools/run-build.sh
Tools/run-godot-scene.sh run res://SlimeAI/Src/Validation/GameOS/Observation/ObservationLogValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
Tools/analyze-godot-scene-logs.sh
```
