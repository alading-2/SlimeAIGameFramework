# GameOS Observation Contract

> 更新日期：2026-05-10  
> 状态：implemented draft  
> 适用范围：GameOS Runtime / Capability / DataOS / GodotBridge / BrotatoLike scene acceptance。

## 定位

Observation 是调试和验收证据层，用来回答“为什么系统没有跑、事件没有到、目标没有选中、资源路径不可用、命令没有回放、场景没有通过”。它不是 gameplay hot path，不替代 Runtime API，不引入通用 world query DSL。

第一版已实现通用日志 / session / validation helper 和 scene runner 结构。后续 Schedule trace、selector dump、DataOS trace 和 command playback 仍按本 contract 分阶段补齐。

## 通用日志 API

框架侧入口位于 `SkilmeAI.GameOS.Observation`：

- `GameOSLog.For(string context)` 创建 context logger。
- `GameOSLog.Configure(GameOSLogOptions options)` 配置最低等级、stdout 和 JSONL。
- `GameOSLog.AddSink(IGameOSLogSink sink)` / `RemoveSink` 注册输出。
- `GameOSObservationSession.FromEnvironment(scenePath, mode)` 读取 `GODOT_SCENE_TEST_*` 环境变量。
- `SceneValidationSession` 负责检查项日志、失败聚合和 validation artifact。

源码按 AI 读取路径分组，但 public namespace 保持不变：

```text
SkilmeAI/GameOS/Observation/
  Logging/      # 通用 log、entry、options、sink
  Session/      # scene runner 环境路径和 JSONL session
  Validation/   # Godot validation check 与 artifact helper
```

`.codex/skills/godot-scene-test/scripts/` 只保存 runner/analyzer；不要把 runner 脚本混入框架 Observation。

日志文本格式固定为：

```text
[LEVEL][Context] message key=value
```

等级：`Trace`、`Debug`、`Info`、`Pass`、`Warn`、`Error`、`Fail`。Godot / stdout sink 可以带颜色，但原始文本必须保留上述格式。启用 JSONL 时，默认写入：

```text
GODOT_SCENE_TEST_ARTIFACT_DIR/logs/scene-log.jsonl
```

## 输出位置

BrotatoLike Godot scene runner 使用新结构：

```text
.ai-temp/scene-tests/runs/<date>/<time>/
  index.json
  001_<scene>_attempt1/
    stdout.log
    stderr.log
    combined.log
    result.json
    screenshots/
    artifacts/
      logs/scene-log.jsonl
```

结构化 Observation JSON 默认写入：

```text
.ai-temp/scene-tests/runs/<date>/<time>/<scene-attempt>/artifacts/
```

框架纯 runtime tests 可以把同形状 JSON 写到测试输出目录，但不得写入源码目录。

通用日志功能的独立 Godot 验证场景：

```bash
cd /home/slime/Code/SkilmeAI/Games/BrotatoLike
Tools/run-godot-scene.sh run res://Scenes/Validation/GameOS/Observation/ObservationLogValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
Tools/analyze-godot-scene-logs.sh
```

打印记录看最新 run 的 `combined.log` / `stdout.log`；结构化记录看同目录 `artifacts/logs/scene-log.jsonl` 和 `artifacts/observation-log-validation.json`。

## 通用 Artifact 字段

所有 Observation JSON SHOULD 包含：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `schemaVersion` | string | artifact schema 版本。 |
| `artifactType` | string | 例如 `schedule-trace`、`eventbus-dump`、`selector-dump`、`scene-acceptance`。 |
| `generatedAtUtc` | string | UTC ISO-8601；测试可使用稳定时间。 |
| `runId` | string | scene run 或 test run id。 |
| `source` | string | 生成者，例如 `RuntimeSchedule`、`BrotatoLike.MainSceneAcceptance`。 |
| `status` | string | `pass`、`fail`、`warning`、`info`。 |
| `summary` | object | 人类可读摘要和计数。 |
| `artifacts` | object | 相关 stdout、stderr、screenshots、trace 文件路径。 |

`status=pass` 必须绑定明确条件；不能因为没有 `ERROR:` 就自动通过。

## Schedule Trace

Schedule artifact 文件名建议：

```text
artifacts/schedule-trace.json
```

每个系统条目 MUST 包含当前 `RuntimeSchedule.GetRuntimeInfo()` 可提供的字段：

| 字段 | 来源 | 说明 |
| --- | --- | --- |
| `systemId` | `SystemRuntimeInfo.SystemId` | 系统 Id。 |
| `isAdded` | `SystemRuntimeInfo.IsAdded` | 是否已加载。 |
| `isEnabled` | `SystemRuntimeInfo.IsEnabled` | 人工开关。 |
| `isRunning` | `SystemRuntimeInfo.IsRunning` | 生命周期运行态。 |
| `isStateAllowed` | `SystemRuntimeInfo.IsStateAllowed` | 是否满足 `ProjectStateSnapshot`。 |
| `blockedReason` | `SystemRuntimeInfo.BlockedReason` | `SystemRunCondition.GetBlockedReason()` 输出。 |
| `group` | `SystemRuntimeInfo.Group` | Schedule group。 |
| `tags` | `SystemRuntimeInfo.Tags` | Schedule tags。 |

Trace 根节点 MUST 记录 `ProjectStateSnapshot`：

```json
{
  "flowState": "Gameplay",
  "overlays": "None",
  "simulationState": "Running"
}
```

Phase trace SHOULD 记录：

| 字段 | 说明 |
| --- | --- |
| `phase` | `bootstrap`、`state-change`、`execute-command`、`tick`、`shutdown`。 |
| `systemId` | 相关系统。 |
| `before` / `after` | 关键状态快照。 |
| `decision` | `started`、`stopped`、`blocked`、`executed`、`skipped`。 |
| `reason` | 阻断或跳过原因。 |

## EventBus Dump

EventBus artifact 文件名建议：

```text
artifacts/eventbus-dump.json
```

Runtime EventBus 暴露的唯一导出入口是 `IEventBus.ExportObservation(path)`：`EntityEventBus` 与 `WorldEventBus` 都在内部维护 `EventBusObservation`，记录订阅、发布计数、同类型嵌套阻断和 handler 异常。

Event dump MUST 能区分：

- 没有订阅。
- 订阅被 `IDisposable.Dispose()` 退订。
- 同类型嵌套被 reentry guard 跳过（per-bus）。
- handler 抛异常被 `EventBusObservation` 捕获。

字段：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `schemaVersion` | string | dump schema 版本。 |
| `busName` | string | `world` 或 `entity:<id>`。 |
| `generatedAtUtc` | string | ISO UTC 时间戳。 |
| `subscriptions` | object | `eventTypeFullName -> { handlerCount, handlers:[{handlerLabel, registrationOrder}] }`。 |
| `emittedCounts` | object | `eventTypeFullName -> publishCount`。 |
| `sameTypeReentryBlockedCounts` | object | `eventTypeFullName -> blockedCount`。 |
| `handlerExceptions` | array | `eventType`、`handlerLabel`、`exceptionType`、`message`。 |
| `handlerRegistrationOrder` | string[] | `registrationOrder:handlerLabel`，按注册顺序排列。 |

## Lifecycle Tree 与 Typed References

Lifecycle artifact 文件名建议：

```text
artifacts/lifecycle-tree.json
```

Relationship tree MUST 使用 `RelationshipManager` 快照查询，不持有内部索引引用。每条关系记录：

| 字段 | 来源 | 说明 |
| --- | --- | --- |
| `parentEntityId` | `RelationshipRecord.ParentEntityId` | 父级或来源实体。 |
| `childEntityId` | `RelationshipRecord.ChildEntityId` | 子级或目标实体。 |
| `relationType` | `RelationshipRecord.RelationType` | `PARENT`、`SOURCE`、`TARGET` 等。 |
| `priority` | `RelationshipRecord.Priority` | 排序优先级。 |
| `data` | `RelationshipRecord.Data` | 低频附加数据。 |

Entity snapshot SHOULD 包含：

- `entityId`
- `dataKeys`
- `parentChain`
- `children`
- `relationshipsByType`
- `localEventBusSummary`

Typed reference dump SHOULD 记录业务引用来源，不把 Relationship 扩展成通用 pair graph API：

| 引用 | 常见来源 |
| --- | --- |
| `sourceEntity` | `ProjectileDataKeys.SourceEntity`、`EffectDataKeys.SourceEntity`、`DamageInfo.Attacker`。 |
| `targetEntity` | `ProjectileDataKeys.TargetEntity`、`AbilityCastContext.Targets`、`AIDataKeys.TargetEntity`。 |
| `abilityEntity` | `ProjectileDataKeys.AbilityEntity`、`EffectDataKeys.AbilityEntity`。 |
| `projectileEntity` | `RelationshipType.EntityToProjectile`。 |
| `effectEntity` | `RelationshipType.EntityToEffect`。 |
| `godotNodeId` | `GodotNodeRegistry` 或 scene adapter 输出。 |

## Selector Dump

Selector evidence MUST 归属于 Capability owner，不新增通用 world query DSL。

### AI Targeting

文件名建议：

```text
artifacts/selectors/ai-targeting.json
```

字段：

- `sourceEntityId`
- `range`
- `excludeSameTeam`
- `candidateCount`
- `filteredSelfCount`
- `filteredDeadCount`
- `filteredSameTeamCount`
- `filteredOutOfRangeCount`
- `selectedEntityId`
- `selectedDistance`
- `ordering`: `nearest-distance`

### Ability Targeting

文件名建议：

```text
artifacts/selectors/ability-targeting.json
```

字段：

- `casterEntityId`
- `abilityEntityId`
- `targetSelection`
- `range`
- `maxTargets`
- `ignoreSameTeam`
- `requiresDamageable`
- `includeDeadTargets`
- `candidateCount`
- `filteredByReason`
- `selectedTargets`
- `targetPosition`
- `failureReason`

### Movement Collision Query

文件名建议：

```text
artifacts/selectors/movement-collision.json
```

字段：

- `sourceEntityId`
- `fromPosition`
- `intendedPosition`
- `mode`
- `queryOwner`: `runtime` 或 `godot-physics`
- `targetMatchMode`
- `specificTargetEntityId`
- `sourceRadius`
- `targetRadius`
- `candidateCount`
- `filteredByLayerMask`
- `filteredSelf`
- `filteredSameTeam`
- `selectedCollisions`
- `stopAfterCollisionCount`
- `willStop`

### Projectile Hit Query

Projectile dump MUST reference Movement collision dump because `ProjectileTool.StartMovement` uses `MovementCollisionParams.OnCollision` to turn movement collision into projectile hit.

字段：

- `projectileEntityId`
- `sourceEntityId`
- `abilityEntityId`
- `targetEntityId`
- `scenePath`
- `maxHitCount`
- `hitCountBefore`
- `hitCountAfter`
- `applyDamageOnHit`
- `damageResult`
- `destroyOnStop`
- `movementCollisionRef`

## DataOS Trace

DataOS trace artifact 文件名建议：

```text
artifacts/dataos-trace.json
```

Observation MUST NOT query authoring SQLite during gameplay. It MAY reference sidecars defined by `SkilmeAI/DataOS/SnapshotManifest.md`:

- `runtime_snapshot.json`
- `runtime_snapshot.manifest.json`
- `dataos_validation_report.json`

每个 DataOS evidence 条目 SHOULD 包含：

| 字段 | 说明 |
| --- | --- |
| `snapshotPath` | Runtime snapshot 路径。 |
| `manifestPath` | manifest 路径；没有则为空字符串。 |
| `validationReportPath` | structured validation report 路径；没有则为空字符串。 |
| `tableId` | DataOS table。 |
| `recordId` | DataOS record。 |
| `fieldKey` | Runtime DataKey。 |
| `valueType` | authoring value type。 |
| `resourceKey` | 资源键，可为空。 |
| `resourcePath` | `res://` 路径，可为空。 |
| `resourceStatus` | `migrated`、`legacy`、`missing`、`intentionally dropped`。 |
| `ledgerRef` | 迁移台账引用，可为空。 |

旧 `res://Src/...` 或 `res://Data/...` MUST 标记为 `legacy`、`missing` 或 `intentionally dropped`，不能因为出现在 snapshot 中就标记为已迁移行为。

## Command Playback Report

`RuntimeCommandBuffer` 尚未实现。Observation 预留 report 形状，后续 R09 实现时使用：

```text
artifacts/command-playback.json
```

字段：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `queuedCount` | int | 入队命令数量。 |
| `playedCount` | int | 成功回放数量。 |
| `failedCount` | int | 失败数量。 |
| `skippedCount` | int | 跳过数量。 |
| `durationMs` | float | 回放耗时。 |
| `commands` | array | 单条命令记录。 |

单条命令：

- `commandId`
- `commandType`
- `phase`
- `status`
- `failureReason`
- `requestedBy`
- `capturedEntityId`
- `createdEntityId`
- `targetEntityId`
- `eventName`
- `resourceKey`
- `godotNodePath`

## Scene PASS/FAIL Artifact

BrotatoLike playable acceptance SHOULD 输出：

```text
artifacts/scene-acceptance.json
```

最低字段：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `scene` | string | `res://Scenes/Main.tscn` 等。 |
| `mode` | string | `smoke`、`playable-slice`、`debug`。 |
| `status` | string | `pass` 或 `fail`。 |
| `passMarker` | string | stdout 中对应 marker。 |
| `criteria` | array | 每个验收项状态。 |
| `failureReasons` | array | 失败原因。 |
| `observedEntities` | array | 关键 entity id。 |
| `artifacts` | object | 关联 schedule/event/selector/dataos/screenshot 文件。 |

Playable slice criteria 至少覆盖：

- player spawned
- movement input observed
- wave enemy spawned
- enemy chase observed
- collision observed
- attack observed
- damage observed
- death or cleanup observed
- two old skills observed
- HUD health or current skill observed
- damage number or structured damage log observed

stdout marker 建议：

```text
[PASS] BrotatoLike playable-slice acceptance
[FAIL] BrotatoLike playable-slice acceptance: <reason>
```

`BrotatoLike GameOS smoke PASS` 只代表 smoke，不等价于 playable slice PASS。

## Log Analysis Contract

`Tools/analyze-godot-scene-logs.sh` 当前检查：

- 新结构 `index.json` 和每个场景的 `result.json`。
- Artifact `status`，包含 `scene-acceptance.json` 和 validation artifact。
- stdout / combined log 明确 PASS/FAIL marker。
- `ERROR:`、`[ERROR]`、`[FAIL]`、`FAIL:`、`Exception`、`Cannot instantiate`、`Failed to load`、`scene not found` 等复查信号。

判定顺序 MUST 是：

1. Godot 进程 exit code。
2. artifact 的 `status`。
3. stdout 明确 `[PASS]` 或 `[FAIL]` marker。
4. error markers 作为复查信号。

没有 error marker 不能单独视为通过；有 generic `ERROR:` 也必须结合 exit code、结构化 artifact 和明确 PASS/FAIL 判断。

旧 flat run directory（`runs/<date>/<time>/stdout.log`）只作为 analyzer 迁移期兼容格式，新运行不再生成该结构。

## R07 使用要求

R07 `migrate-brotatolike-playable-slice` 开始前，应引用本 contract 并至少产出：

- `scene-acceptance.json`
- `schedule-trace.json`
- 与目标选择或技能命中相关的 selector dump
- 如使用 DataOS 记录作为证据，则产出 `dataos-trace.json`
- 若主场景仍无法输出这些 artifact，必须在 R07 proposal 中明确 waiver 和补齐计划
