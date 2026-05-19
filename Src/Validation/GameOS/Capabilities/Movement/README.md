# Movement Capability Validation

## 测试目标

验证 Movement Capability owner-scoped 契约：deterministic tick、active movement selector、collision target query injection 和 stop event。

## expectedInputs

- moving / inactive Runtime entities。
- `MovementParams`，包含 Charge、Speed、Direction 和 CollisionParams。
- 注入的 `IMovementCollisionTargetQuery`。
- Movement Started / Collision / Stopped 事件订阅。

## expectedObservations

- deterministic Charge tick 按 speed 和 delta 写入位置。
- `MovementSystem.Tick` 只推进已 Start 的 active movement。
- 注入 collision query 被调用并触发 Movement collision event。
- `Stop` 发布 Stopped，payload 包含 stop reason。

## passCriteria

- stdout 包含 `GameOS Capability Movement validation PASS`。
- artifact `movement-capability-validation.json` 的 `status` 为 `pass`。
- artifact `failureReasons` 为空，四个检查项均为 `pass`。

## failCriteria

- stdout 包含 `GameOS Capability Movement validation FAIL`。
- 任一 tick、selector、collision query 或 stop event 检查失败。
- artifact 标准答案字段缺失或 `failureReasons` 非空。

## artifactPath

```text
.ai-temp/scene-tests/runs/<date>/<time>/<scene-attempt>/artifacts/movement-capability-validation.json
```

## 运行命令

```bash
cd /home/slime/Code/SlimeAI/Games/BrotatoLike
Tools/run-build.sh
Tools/run-godot-scene.sh run res://SlimeAI/Src/Validation/GameOS/Capabilities/Movement/MovementCapabilityValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
Tools/analyze-godot-scene-logs.sh
```
