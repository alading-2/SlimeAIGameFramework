# GameOS Capability Convergence Validation

> 跨 Capability 回归场景。它不计作任何单个 Capability 的 owner-scoped 验收证据；单个 Capability 必须使用相邻 owner 目录下的 `<Capability>CapabilityValidation` 场景。

## 测试目标

验证 7 个 GameOS OpenSpec 未完成项收敛后，Capability 层关键契约在 Godot headless 场景中可复验。

覆盖范围：

- Feature action、grant action、Periodic / OnEvent auto trigger。
- typed `FeatureContext` activation/result 与 Ability -> Feature handler flow。
- `AbilityService` 注入 `FeatureService / DamageService`，并使用注入 damage path。
- Damage neutral `Encounter*` 统计。
- AI / Ability capability-owned target query。

## expectedInputs

- Godot scene runner 设置 `GODOT_SCENE_TEST_ARTIFACT_DIR`。
- `RuntimeWorld.CreateScoped()` 提供隔离 Runtime world。
- Feature / Ability / Damage / AI / Collision / Movement Capability API 来自主框架仓。

## expectedObservations

- Feature action 与 auto-trigger 都能调用 handler，并能在 Dispose 后停止。
- Ability handler 通过 typed payload/result 返回 `AbilityExecutedResult`，不依赖 raw object bag。
- 注入 `DamageService` 的 processor 能阻断默认 damage path。
- Damage 统计写入 `EncounterDamageDealt / EncounterDamageTaken / EncounterCriticalHits`。
- AI / Ability selector 只消费注入 query 的候选实体。

## passCriteria

- Godot 进程 exit code 为 `0`。
- stdout 包含 `GameOS Capability convergence validation PASS`。
- artifact `capability-convergence-validation.json` 的 `status` 为 `pass`。
- artifact `failureReasons` 为空。

## failCriteria

- stdout 包含 `GameOS Capability convergence validation FAIL`。
- 任一 Feature / Ability / Damage / selector 检查失败。
- artifact `failureReasons` 非空或缺少标准答案字段。

## artifactPath

```text
.ai-temp/scene-tests/runs/<date>/<time>/<scene-attempt>/artifacts/capability-convergence-validation.json
```

## 运行命令

```bash
cd /home/slime/Code/SlimeAI/Games/BrotatoLike
Tools/run-build.sh
Tools/run-godot-scene.sh run res://SlimeAI/Src/Validation/GameOS/Capabilities/Convergence/CapabilityConvergenceValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
Tools/analyze-godot-scene-logs.sh
```
