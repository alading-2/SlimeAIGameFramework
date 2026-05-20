# Unit Composition Validation

## 测试目标

验证框架 `GodotUnitComposer` 可以按 profile 组合通用 GodotBridge Adapter，并保持单位动画在 Runtime Data / Damage events 驱动下工作；不依赖 BrotatoLike 代码命名空间。

## expectedInputs

- `GodotEntity2D`。
- `Unit.VisualScenePath`、`Collision.Radius`、`Movement.Velocity`、Damage events。
- `GodotUnitCompositionProfile` 启用 visual、animation、orientation、attack、hurtbox、contact damage。

## expectedObservations

- visual scene 加载为 `VisualRoot`。
- profile 启用的 Adapter 被挂载并可在已进树 entity 上注册。
- Hurtbox 使用 `CircleShape2D`，半径来自 `Collision.Radius`。
- `GodotUnitAnimationComponent` 可从 idle 切 run，并响应 damaged / killed 动画。
- 组合出的框架 Adapter 类型不属于 `BrotatoLike.*` namespace。

## passCriteria

- stdout 包含 `GameOS GodotBridge UnitComposition validation PASS`。
- artifact `unit-composition-validation.json` 的 `status` 为 `pass`。
- artifact `failureReasons` 为空，全部检查项均为 `pass`。

## failCriteria

- stdout 包含 `GameOS GodotBridge UnitComposition validation FAIL`。
- visual、adapter registration、hurtbox、animation 或 namespace 任一检查失败。
- artifact 标准答案字段缺失或 `failureReasons` 非空。

## artifactPath

```text
.ai-temp/scene-tests/runs/<date>/<time>/<scene-attempt>/artifacts/unit-composition-validation.json
```

## 运行命令

```bash
cd /home/slime/Code/SlimeAI/Games/BrotatoLike
Tools/run-build.sh
Tools/run-godot-scene.sh run res://SlimeAI/Src/Validation/GameOS/GodotBridge/UnitComposition/UnitCompositionValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
Tools/analyze-godot-scene-logs.sh
```
