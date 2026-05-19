# Validation Catalog

> 集中索引所有 Godot 验证场景的标准答案。新增场景时必须同步更新此表。
> 此表是只读索引；场景级事实源仍是各场景目录下的 `README.md`。

## 场景索引

| 场景路径 | 能力 | expectedInputs | expectedObservations | passCriteria | failCriteria | artifactPath |
|---|---|---|---|---|---|---|
| `res://SlimeAI/Src/Validation/Runtime/Entity/RuntimeEntityValidation.tscn` | Runtime/Entity | EntityId.Empty、自定义 EntityId、spawn 请求 | EntityId 边界检查通过、自定义和生成 ID 区分、typed lifecycle event payload | stdout 含 `GameOS Runtime Entity validation PASS`、artifact `status: pass`、`failureReasons` 为空 | stdout 含 `GameOS Runtime Entity validation FAIL` 或 exit code ≠ 0 | `artifacts/runtime-entity-validation.json` |
| `res://SlimeAI/Src/Validation/Runtime/CommandBuffer/RuntimeCommandBufferValidation.tscn` | Runtime/CommandBuffer | DeferredRuntimeCommand 各 kind、SchedulePhase 参数 | 命令入队计数、phase playback 执行、guard 内拒绝结构变更、discard 报告 | stdout 含 `GameOS Runtime CommandBuffer validation PASS`、artifact `status: pass` | stdout 含 `GameOS Runtime CommandBuffer validation FAIL` 或 exit code ≠ 0 | `artifacts/runtime-commandbuffer-validation.json` |
| `res://SlimeAI/Src/Validation/Runtime/Data/RuntimeDataValidation.tscn` | Runtime/Data | DataKey<T> 读写、DataCatalog、typed property changed event | typed 读写正确、变更事件携带正确新旧值、modifier 排序和 dirty 标记 | stdout 含 `GameOS Runtime Data validation PASS`、artifact `status: pass` | stdout 含 `GameOS Runtime Data validation FAIL` 或 exit code ≠ 0 | `artifacts/runtime-data-validation.json` |
| `res://SlimeAI/Src/Validation/Runtime/Event/RuntimeEventValidation.tscn` | Runtime/Event | typed/parameterless handler、priority、Once、Off、exception、reentrancy、StopPropagation | handler 按 priority 触发、Once 执行一次、Off 移除、exception 被捕获后续继续、重入被阻止、StopPropagation 阻断低优先级、local/global 隔离 | stdout 含 `GameOS Runtime Event validation PASS`、artifact `status: pass`、`failureReasons` 为空 | stdout 含 `GameOS Runtime Event validation FAIL` 或 exit code ≠ 0 | `artifacts/runtime-event-validation.json` |
| `res://SlimeAI/Src/Validation/Runtime/Lifecycle/RuntimeLifecycleValidation.tscn` | Runtime/Lifecycle | LifecycleTree、EntityIdList、owner cleanup hook | 父子 attach/detach、owner destroy 级联清理、EntityIdList 正确维护 | stdout 含 `GameOS Runtime Lifecycle validation PASS`、artifact `status: pass` | stdout 含 `GameOS Runtime Lifecycle validation FAIL` 或 exit code ≠ 0 | `artifacts/runtime-lifecycle-validation.json` |
| `res://SlimeAI/Src/Validation/Runtime/World/RuntimeWorldValidation.tscn` | Runtime/World | RuntimeWorld.Default、CreateScoped()、dispose | Default 单例可用、Scoped 隔离、dispose 顺序正确、dispose 后访问抛异常 | stdout 含 `GameOS Runtime World validation PASS`、artifact `status: pass` | stdout 含 `GameOS Runtime World validation FAIL` 或 exit code ≠ 0 | `artifacts/runtime-world-validation.json` |
| `res://SlimeAI/Src/Validation/GameOS/Observation/ObservationLogValidation.tscn` | GameOS/Observation | SceneValidationSession、ObservationLog | 分级日志输出（INFO/PASS/FAIL）、artifact 写入、session 生命周期 | stdout 含 PASS marker、artifact `status: pass` | stdout 含 FAIL marker 或 artifact `status: fail` | `artifacts/observation-log-validation.json` |
| `res://Src/Validation/Game/Input/BrotatoLikeInputEventValidation.tscn` | Game/Input | 玩家输入事件、BrotatoLike InputAdapter | 输入事件正确桥接到 GameOS EventBus、输入状态可查询 | stdout 含 PASS marker、artifact `status: pass` | stdout 含 FAIL marker 或 artifact `status: fail` | `artifacts/brotatolike-input-validation.json` |

## 维护规则

- 新增验证场景 → 必须在此表追加一行
- 修改场景标准答案 → 必须同步更新此表对应行
- `scene-gate` skill 检查此表是否滞后于 README
- 此表不替代 README；README 是场景级事实源
