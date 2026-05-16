# Runtime/Entity Validation

## 测试目标

验证 AI-first Runtime 重构后的 typed `EntityId` 与 Entity lifecycle 运行时证据。

## 允许依赖

- `SlimeAI.GameOS.Runtime.Entity`
- `SlimeAI.GameOS.Runtime.World`
- `SlimeAI.GameOS.Runtime.Events.Core`
- `Games/BrotatoLike` Godot scene runner

## 不覆盖内容

- DataOS authoring / snapshot。
- Capability 业务引用清理；该项由 Runtime/Lifecycle 场景覆盖。
- GodotBridge 节点注册。

## 运行命令

```bash
cd /home/slime/Code/SkilmeAI/Games/BrotatoLike
Tools/run-godot-scene.sh run res://SlimeAI/Scenes/Validation/Runtime/Entity/RuntimeEntityValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
```

## PASS/FAIL 判定

- PASS marker: `GameOS Runtime Entity validation PASS`
- FAIL marker: `GameOS Runtime Entity validation FAIL`
- Godot 进程 exit code 必须与 marker 一致。

## Artifact

`artifacts/runtime-entity-validation.json`

关键字段：

- `status`
- `checks`
- `failureReasons`
- `dependencies`
- `notes`

## 常见失败排查顺序

1. 打开最新 `index.json`。
2. 打开 per-scene `result.json` 和 `combined.log`。
3. 打开 `artifacts/runtime-entity-validation.json`，先看 `failureReasons`，再看对应 `checks`。
