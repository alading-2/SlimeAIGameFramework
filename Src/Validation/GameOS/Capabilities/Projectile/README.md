# Projectile Capability Validation

## 测试目标

验证 Projectile Capability owner-scoped 契约：spawn data、movement hit、pierce/max hits、lifetime destroy 和 damage integration。

## expectedInputs

- source、projectile、target Runtime entities。
- `ProjectileSpawnOptions` 和 `ProjectileMovementOptions`。
- `MovementSystem`、注入 collision target query 和 `DamageService`。
- Projectile Spawned / Hit event subscriptions。

## expectedObservations

- Spawn 写入 source、target、ScenePath、speed、max hits 和 owner list。
- Movement collision 转换为 Projectile Hit。
- MaxHitCount 控制穿透次数并在停止后销毁 projectile。
- MaxLifeTime 到期销毁 projectile。
- Hit damage 通过 `DamageService` 扣减 target HP。

## passCriteria

- stdout 包含 `GameOS Capability Projectile validation PASS`。
- artifact `projectile-capability-validation.json` 的 `status` 为 `pass`。
- artifact `failureReasons` 为空，四个检查项均为 `pass`。

## failCriteria

- stdout 包含 `GameOS Capability Projectile validation FAIL`。
- 任一 spawn、hit、pierce、lifetime 或 damage integration 检查失败。
- artifact 标准答案字段缺失或 `failureReasons` 非空。

## artifactPath

```text
.ai-temp/scene-tests/runs/<date>/<time>/<scene-attempt>/artifacts/projectile-capability-validation.json
```

## 运行命令

```bash
cd /home/slime/Code/SlimeAI/Games/BrotatoLike
Tools/run-build.sh
Tools/run-godot-scene.sh run res://SlimeAI/Src/Validation/GameOS/Capabilities/Projectile/ProjectileCapabilityValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
Tools/analyze-godot-scene-logs.sh
```
