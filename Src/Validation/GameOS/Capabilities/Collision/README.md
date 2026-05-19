# Collision Capability Validation

## 测试目标

验证 Collision Capability owner-scoped 契约：layer/mask/team 过滤、entered/exited/contact event payload，以及 contact damage boundary 不直接写 HP。

## expectedInputs

- source / target Runtime entities。
- `Collision.Layer`、`Collision.Mask`、`Collision.Team` 和 radius DataKeys。
- `CollisionSystem.EmitEntered / EmitExited`。
- target 的 `Damage.CurrentHp` 用于边界确认。

## expectedObservations

- layer/mask 命中时允许碰撞，mask 不命中或同队过滤时拒绝。
- Entered / Exited 事件携带 `CollisionContact` 的 source、target、layer。
- `CollisionSystem` 只发布碰撞事件，不直接修改 HP。

## passCriteria

- stdout 包含 `GameOS Capability Collision validation PASS`。
- artifact `collision-capability-validation.json` 的 `status` 为 `pass`。
- artifact `failureReasons` 为空，三个检查项均为 `pass`。

## failCriteria

- stdout 包含 `GameOS Capability Collision validation FAIL`。
- 任一 filter、payload 或 damage boundary 检查失败。
- artifact 标准答案字段缺失或 `failureReasons` 非空。

## artifactPath

```text
.ai-temp/scene-tests/runs/<date>/<time>/<scene-attempt>/artifacts/collision-capability-validation.json
```

## 运行命令

```bash
cd /home/slime/Code/SlimeAI/Games/BrotatoLike
Tools/run-build.sh
Tools/run-godot-scene.sh run res://SlimeAI/Src/Validation/GameOS/Capabilities/Collision/CollisionCapabilityValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
Tools/analyze-godot-scene-logs.sh
```
