# Feature Capability Validation

## 测试目标

验证 Feature Capability owner-scoped 契约：modifier grant/remove、handler lifecycle、`IFeatureAction`、Periodic/OnEvent、typed payload/result。

## expectedInputs

- owner / feature Runtime entities。
- `FeatureDefinition`、`FeatureModifierEntry`、`IFeatureAction`。
- `FeatureHandlerRegistry` 中注册的测试 handler。
- `FeatureAutoTriggerService` 和 entity event bus。

## expectedObservations

- Grant 应用 modifier，Remove 清理同 source modifier。
- handler 的 grant/enable/disable/activate/execute/end/remove lifecycle 被调用。
- Periodic timer 和 OnEvent 触发能激活 handler。
- `FeatureContext` typed payload/result 可读取。

## passCriteria

- stdout 包含 `GameOS Capability Feature validation PASS`。
- artifact `feature-capability-validation.json` 的 `status` 为 `pass`。
- artifact `failureReasons` 为空，四个检查项均为 `pass`。

## failCriteria

- stdout 包含 `GameOS Capability Feature validation FAIL`。
- 任一 modifier、lifecycle、auto trigger 或 typed context 检查失败。
- artifact 标准答案字段缺失或 `failureReasons` 非空。

## artifactPath

```text
.ai-temp/scene-tests/runs/<date>/<time>/<scene-attempt>/artifacts/feature-capability-validation.json
```

## 运行命令

```bash
cd /home/slime/Code/SlimeAI/Games/BrotatoLike
Tools/run-build.sh
Tools/run-godot-scene.sh run res://SlimeAI/Src/Validation/GameOS/Capabilities/Feature/FeatureCapabilityValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
Tools/analyze-godot-scene-logs.sh
```
