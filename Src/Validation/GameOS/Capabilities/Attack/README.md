# Attack Capability Validation

## 测试目标

验证 Attack Capability owner-scoped 契约：request consumption、range gate、cooldown、windup/recovery 和注入 damage path。

## expectedInputs

- attacker / target Runtime entities。
- `Attack.Events.Requested` 事件。
- `TimerManager` 推进前摇、后摇和冷却。
- 注入的 `DamageService`。

## expectedObservations

- `AttackService.Register` 后能消费局部攻击请求事件并结算伤害。
- 超出范围时返回 `FailOutOfRange` 并发布 `Cancelled(TargetOutOfRange)`。
- windup、recovery、cooldown 状态按 timer 推进。
- 注入 `DamageService` 的 processor 能控制 Attack 伤害路径。

## passCriteria

- stdout 包含 `GameOS Capability Attack validation PASS`。
- artifact `attack-capability-validation.json` 的 `status` 为 `pass`。
- artifact `failureReasons` 为空，四个检查项均为 `pass`。

## failCriteria

- stdout 包含 `GameOS Capability Attack validation FAIL`。
- 任一 request、gate、timing 或 damage path 检查失败。
- artifact 标准答案字段缺失或 `failureReasons` 非空。

## artifactPath

```text
.ai-temp/scene-tests/runs/<date>/<time>/<scene-attempt>/artifacts/attack-capability-validation.json
```

## 运行命令

```bash
cd /home/slime/Code/SlimeAI/Games/BrotatoLike
Tools/run-build.sh
Tools/run-godot-scene.sh run res://SlimeAI/Src/Validation/GameOS/Capabilities/Attack/AttackCapabilityValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
Tools/analyze-godot-scene-logs.sh
```
