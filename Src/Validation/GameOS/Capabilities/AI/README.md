# AI Capability Validation

## 测试目标

验证 AI Capability owner-scoped 契约：`IAITargetQuery` 注入、最近目标、巡逻、攻击请求和 Ability auto-target context。

## expectedInputs

- `RuntimeWorld.Default` 的隔离实体集合。
- 注入的 `IAITargetQuery` 候选实体。
- AI、Movement、Collision、Damage、Ability DataKeys。

## expectedObservations

- `FindNearestTargetAction` 只从注入 query 读取候选，并选择最近合法敌方目标。
- `PatrolAction` 写入确定性移动意图。
- `RequestAttackAction` 发布 `Attack.Events.Requested` 并停步面向目标。
- `PrepareAbilityAutoTargetContextsAction` 为自动索敌技能生成 `AbilityCastContext`。

## passCriteria

- stdout 包含 `GameOS Capability AI validation PASS`。
- artifact `ai-capability-validation.json` 的 `status` 为 `pass`。
- artifact `failureReasons` 为空，且四个检查项均为 `pass`。

## failCriteria

- stdout 包含 `GameOS Capability AI validation FAIL`。
- 任一 AI selector、patrol、attack request 或 auto-target context 检查失败。
- artifact 缺少标准答案字段或 `failureReasons` 非空。

## artifactPath

```text
.ai-temp/scene-tests/runs/<date>/<time>/<scene-attempt>/artifacts/ai-capability-validation.json
```

## 运行命令

```bash
cd /home/slime/Code/SlimeAI/Games/BrotatoLike
Tools/run-build.sh
Tools/run-godot-scene.sh run res://SlimeAI/Src/Validation/GameOS/Capabilities/AI/AICapabilityValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
Tools/analyze-godot-scene-logs.sh
```
