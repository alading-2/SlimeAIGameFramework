# Ability Capability Validation

## 测试目标

验证 Ability Capability owner-scoped 契约：显式目标、点目标、cooldown/charge、Periodic auto trigger 和 `IAbilityTargetQuery` 注入。

## expectedInputs

- caster、ability、entity target 和 point target。
- `AbilityDataKeys` 的 TargetSelection、Damage、Cooldown、Charges、TriggerMode。
- 注入的 `IAbilityTargetQuery` 候选列表。

## expectedObservations

- 显式实体目标通过 `AbilityService` 结算伤害。
- 点目标无需实体目标也能成功触发。
- 冷却优先阻断，冷却结束后无 charge 阻断。
- Periodic auto trigger 能推进一次正式触发。
- Ability selector 使用注入 query 并按距离排序。

## passCriteria

- stdout 包含 `GameOS Capability Ability validation PASS`。
- artifact `ability-capability-validation.json` 的 `status` 为 `pass`。
- artifact `failureReasons` 为空，五个检查项均为 `pass`。

## failCriteria

- stdout 包含 `GameOS Capability Ability validation FAIL`。
- 任一 Ability trigger、gate、auto trigger 或 selector 检查失败。
- artifact 标准答案字段缺失或 `failureReasons` 非空。

## artifactPath

```text
.ai-temp/scene-tests/runs/<date>/<time>/<scene-attempt>/artifacts/ability-capability-validation.json
```

## 运行命令

```bash
cd /home/slime/Code/SlimeAI/Games/BrotatoLike
Tools/run-build.sh
Tools/run-godot-scene.sh run res://SlimeAI/Src/Validation/GameOS/Capabilities/Ability/AbilityCapabilityValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
Tools/analyze-godot-scene-logs.sh
```
