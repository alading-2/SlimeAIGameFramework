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
Tools/run-godot-scene.sh run res://SlimeAI/Src/Validation/Runtime/Lifecycle/RuntimeLifecycleValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
```

## PASS/FAIL 判定

- PASS marker: `GameOS Runtime Lifecycle validation PASS`
- FAIL marker: `GameOS Runtime Lifecycle validation FAIL`

## Standard answer

### expectedInputs

- `LifecycleTree` attach/detach、single parent、自循环和 empty id 输入。
- destroy policy recursive/detach 请求。
- `EntityIdList` value semantics。
- owner cleanup registry。

### expectedObservations

- attach/detach 和 single parent 约束正确。
- self-cycle 与 empty id 被拒绝。
- destroy policy 递归清理和 detach 行为正确。
- `EntityIdList` value semantics 与 owner cleanup hook 正确维护。

### passCriteria

- `index.json` 对应 entry `status=passed` 且 `exitCode=0`。
- per-scene `result.json` `status=passed`，`firstError=null`。
- artifact `status=pass`，`failureReasons=[]`，且 `checks[]` 全部为 pass。
- stdout 含 `GameOS Runtime Lifecycle validation PASS`。

### failCriteria

- stdout 含 `GameOS Runtime Lifecycle validation FAIL`。
- 任一 lifecycle tree、destroy policy、EntityIdList 或 owner cleanup check 失败。
- artifact `status=fail`、`failureReasons` 非空或标准答案字段缺失。

### artifactPath

`artifacts/runtime-lifecycle-validation.json`

## Artifact

`artifacts/runtime-lifecycle-validation.json`

## 常见失败排查顺序

1. 打开最新 `index.json`。
2. 打开 per-scene `combined.log`。
3. 打开 `artifacts/runtime-lifecycle-validation.json` 的 `failureReasons` 和 `checks`。
