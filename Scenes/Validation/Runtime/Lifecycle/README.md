# Runtime/Lifecycle Validation

## 测试目标

验证 LifecycleTree、`EntityIdList` 和 owner cleanup hook 的运行时证据。

## 允许依赖

- `SlimeAI.GameOS.Runtime.Entity`
- `SlimeAI.GameOS.Runtime.World`
- `SlimeAI.GameOS.Runtime.Data`
- `SlimeAI.GameOS.Runtime.Events.Core`
- `Games/BrotatoLike` Godot scene runner

## 不覆盖内容

- Capability 专属业务语义。
- Godot Node 生命周期。

## 运行命令

```bash
cd /home/slime/Code/SlimeAI/Games/BrotatoLike
Tools/run-godot-scene.sh run res://SlimeAI/Scenes/Validation/Runtime/Lifecycle/RuntimeLifecycleValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
```

## PASS/FAIL 判定

- PASS marker: `GameOS Runtime Lifecycle validation PASS`
- FAIL marker: `GameOS Runtime Lifecycle validation FAIL`

## Artifact

`artifacts/runtime-lifecycle-validation.json`

## 常见失败排查顺序

1. 打开最新 `index.json`。
2. 打开 per-scene `combined.log`。
3. 打开 `artifacts/runtime-lifecycle-validation.json` 的 `failureReasons` 和 `checks`。
