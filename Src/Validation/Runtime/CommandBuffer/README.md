# Runtime/CommandBuffer Validation

## 测试目标

验证 `RuntimeCommandBuffer`、`SchedulePhase` 和 structural change guard 的 Godot headless 场景证据。

## 允许依赖

- `SlimeAI.GameOS.Runtime.CommandBuffer`
- `SlimeAI.GameOS.Runtime.Schedule`
- `SlimeAI.GameOS.Runtime.World`
- `SlimeAI.GameOS.Runtime.Entity`
- `Games/BrotatoLike` Godot scene runner

## 不覆盖内容

- BrotatoLike 主循环 phase tick 顺序；该项由主场景 smoke 覆盖。
- 真实 Godot Node 实例化；本场景使用 fake `IGodotNodeCommandHandler` 验证 Runtime command playback。

## 运行命令

```bash
cd /home/slime/Code/SlimeAI/Games/BrotatoLike
Tools/run-godot-scene.sh run res://SlimeAI/Src/Validation/Runtime/CommandBuffer/RuntimeCommandBufferValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
```

## PASS/FAIL 判定

- PASS marker: `GameOS Runtime CommandBuffer validation PASS`
- FAIL marker: `GameOS Runtime CommandBuffer validation FAIL`

## Standard answer

### expectedInputs

- `DeferredRuntimeCommand` 各 kind。
- `SchedulePhase` targeted playback 参数。
- structural change guard 嵌套与受保护结构变更请求。
- dispose 时未播放命令 discard 报告。

### expectedObservations

- 8 类 deferred command 均可入队并按预期播放。
- phase-targeted playback 只播放目标 phase。
- guard 内结构变更被拒绝并记录。
- dispose discard report 能说明未播放命令。

### passCriteria

- `index.json` 对应 entry `status=passed` 且 `exitCode=0`。
- per-scene `result.json` `status=passed`，`firstError=null`。
- artifact `status=pass`，`failureReasons=[]`，且 `checks[]` 覆盖 command kind、phase、guard 和 dispose checks。
- stdout 含 `GameOS Runtime CommandBuffer validation PASS`。

### failCriteria

- stdout 含 `GameOS Runtime CommandBuffer validation FAIL`。
- 任一 command playback、phase、guard 或 dispose check 失败。
- artifact `status=fail`、`failureReasons` 非空或标准答案字段缺失。

### artifactPath

`artifacts/runtime-command-buffer-validation.json`

## Artifact

`artifacts/runtime-command-buffer-validation.json`

## 常见失败排查顺序

1. 打开最新 `index.json`。
2. 打开 per-scene `combined.log`。
3. 打开 `artifacts/runtime-command-buffer-validation.json`。
