# Runtime/World Validation

## 测试目标

验证 `RuntimeWorld.Default`、`RuntimeWorld.CreateScoped()`、dispose 顺序和 dispose 后访问语义。

## 允许依赖

- `SlimeAI.GameOS.Runtime.World`
- `SlimeAI.GameOS.Runtime.Entity`
- `SlimeAI.GameOS.Runtime.Events.Core`
- `SlimeAI.GameOS.Runtime.CommandBuffer`
- `Games/BrotatoLike` Godot scene runner

## 不覆盖内容

- Capability tick 调度。
- 游戏主循环 phase 编排。

## 运行命令

```bash
cd /home/slime/Code/SlimeAI/Games/BrotatoLike
Tools/run-godot-scene.sh run res://SlimeAI/Src/Validation/Runtime/World/RuntimeWorldValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
```

## PASS/FAIL 判定

- PASS marker: `GameOS Runtime World validation PASS`
- FAIL marker: `GameOS Runtime World validation FAIL`

## Artifact

`artifacts/runtime-world-validation.json`

## 常见失败排查顺序

1. 打开最新 `index.json`。
2. 打开 per-scene `combined.log`。
3. 打开 `artifacts/runtime-world-validation.json`。
