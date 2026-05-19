# Unit Capability Validation

## 测试目标

验证 Unit Capability owner-scoped 契约：Unit DataKeys registration、metadata roundtrip、animation request events；不引入游戏 HUD/visual 默认契约。

## expectedInputs

- unit Runtime entity。
- `UnitDataKeys.RegisterAll()`。
- Name、EntityType、VisualScenePath、HealthBarHeight、IsShowHealthBar、AvailableAnimations。
- PlayAnimationRequested、StopAnimationRequested、AnimationFinished event payloads。

## expectedObservations

- Unit metadata 通过 DataKeys 正确往返。
- AvailableAnimations 作为 runtime data 保存动画名列表。
- 动画 request/stop/finished 事件 payload 指向同一 unit，不要求存在 Godot visual node。

## passCriteria

- stdout 包含 `GameOS Capability Unit validation PASS`。
- artifact `unit-capability-validation.json` 的 `status` 为 `pass`。
- artifact `failureReasons` 为空，三个检查项均为 `pass`。

## failCriteria

- stdout 包含 `GameOS Capability Unit validation FAIL`。
- 任一 DataKey、metadata 或 animation event 检查失败。
- artifact 标准答案字段缺失或 `failureReasons` 非空。

## artifactPath

```text
.ai-temp/scene-tests/runs/<date>/<time>/<scene-attempt>/artifacts/unit-capability-validation.json
```

## 运行命令

```bash
cd /home/slime/Code/SlimeAI/Games/BrotatoLike
Tools/run-build.sh
Tools/run-godot-scene.sh run res://SlimeAI/Src/Validation/GameOS/Capabilities/Unit/UnitCapabilityValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
Tools/analyze-godot-scene-logs.sh
```
