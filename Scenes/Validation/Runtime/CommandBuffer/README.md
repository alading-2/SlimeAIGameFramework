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
cd /home/slime/Code/SkilmeAI/Games/BrotatoLike
Tools/run-godot-scene.sh run res://SlimeAI/Scenes/Validation/Runtime/CommandBuffer/RuntimeCommandBufferValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
```

## PASS/FAIL 判定

- PASS marker: `GameOS Runtime CommandBuffer validation PASS`
- FAIL marker: `GameOS Runtime CommandBuffer validation FAIL`

## Artifact

`artifacts/runtime-command-buffer-validation.json`

## 常见失败排查顺序

1. 打开最新 `index.json`。
2. 打开 per-scene `combined.log`。
3. 打开 `artifacts/runtime-command-buffer-validation.json`。
