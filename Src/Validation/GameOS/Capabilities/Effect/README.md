# Effect Capability Validation

## 测试目标

验证 Effect Capability owner-scoped 契约：`EffectTool.Spawn` data/event、source/target/position/duration，以及 visual path 仅作为 data，不加载游戏资产。

## expectedInputs

- source、ability、target Runtime entities。
- `EffectSpawnOptions`，包含 ScenePath、Name、AnimationName、Position、Duration。
- Effect spawned event subscriber。

## expectedObservations

- spawned effect 写入 source、ability、target、position、duration、name 和 path。
- source 的 `SpawnedEffectIds` 包含新 effect id。
- `Effect.Events.Spawned` payload 指向 effect/source/target。
- ScenePath 和 AnimationName 保持字符串 data，不触发资源加载。

## passCriteria

- stdout 包含 `GameOS Capability Effect validation PASS`。
- artifact `effect-capability-validation.json` 的 `status` 为 `pass`。
- artifact `failureReasons` 为空，三个检查项均为 `pass`。

## failCriteria

- stdout 包含 `GameOS Capability Effect validation FAIL`。
- 任一 spawn data、event payload 或 visual boundary 检查失败。
- artifact 标准答案字段缺失或 `failureReasons` 非空。

## artifactPath

```text
.ai-temp/scene-tests/runs/<date>/<time>/<scene-attempt>/artifacts/effect-capability-validation.json
```

## 运行命令

```bash
cd /home/slime/Code/SlimeAI/Games/BrotatoLike
Tools/run-build.sh
Tools/run-godot-scene.sh run res://SlimeAI/Src/Validation/GameOS/Capabilities/Effect/EffectCapabilityValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
Tools/analyze-godot-scene-logs.sh
```
