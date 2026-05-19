# Damage Capability Validation

## 测试目标

验证 Damage Capability owner-scoped 契约：processor order、crit/armor/shield/heal/killed、`Encounter*` stats，以及禁止绕过 Damage service 直接写 HP。

## expectedInputs

- attacker、victim、healer Runtime entities。
- Damage processors、`DamageInfo`、`HealInfo`。
- Crit、Armor、Shield、CurrentHp、MaxHp 和 Encounter DataKeys。
- Damage event subscriptions。

## expectedObservations

- processors 按 priority 从小到大执行。
- crit、shield、armor 会影响最终伤害和 HP。
- `HealService` 发布 Healed，`DamageService` 发布 Killed 并设置 IsDead。
- simulation damage 不写 HP，正式 damage 更新 `Encounter*` stats。

## passCriteria

- stdout 包含 `GameOS Capability Damage validation PASS`。
- artifact `damage-capability-validation.json` 的 `status` 为 `pass`。
- artifact `failureReasons` 为空，四个检查项均为 `pass`。

## failCriteria

- stdout 包含 `GameOS Capability Damage validation FAIL`。
- 任一 pipeline、event、stats 或 HP boundary 检查失败。
- artifact 标准答案字段缺失或 `failureReasons` 非空。

## artifactPath

```text
.ai-temp/scene-tests/runs/<date>/<time>/<scene-attempt>/artifacts/damage-capability-validation.json
```

## 运行命令

```bash
cd /home/slime/Code/SlimeAI/Games/BrotatoLike
Tools/run-build.sh
Tools/run-godot-scene.sh run res://SlimeAI/Src/Validation/GameOS/Capabilities/Damage/DamageCapabilityValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
Tools/analyze-godot-scene-logs.sh
```
