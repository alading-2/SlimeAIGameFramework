# Runtime Event Validation

## 测试目标

验证 GameOS `Runtime/Event` typed kernel 在 Godot headless 场景中的可运行行为，并产出稳定分级日志、PASS/FAIL marker 与 JSON artifact。

核心断言覆盖：

- `EntityEventBus` 的 registration order 与 `IDisposable` 退订生命周期。
- handler exception 隔离与后续 handler 继续执行。
- 同类型 nested Publish 的 per-bus reentry guard。
- `IEntityEvent`、`IGlobalEvent`、`IBroadcastEvent` 的 scope 路由。
- `RuntimeEntity.Data.Set` 到 `DataPropertyChanged` 的跨层桥接。

## 允许依赖

- Godot scene runner: `Tools/run-godot-scene.sh`
- GameOS Runtime Event: `EntityEventBus`、`WorldEvents.World`、`DataPropertyChanged`
- GameOS Runtime Entity/Data bridge: `RuntimeEntity`、`Data.Set`、`EventDataChangeSink`
- BrotatoLike 仅作为 `res://` 场景承载工程，不引入玩法代码作为通过条件。

## 不覆盖内容

- 不验证 Data 层完整行为。
- 不验证 Entity 生命周期管理器。
- 不验证 Capability、GodotBridge 组件、BrotatoLike 玩法或 UI。
- 不回归旧 string-key EventBus、priority、Once、Off 或 EventContext API。

## 运行命令

框架侧：

```bash
cd /home/slime/Code/SlimeAI/SlimeAI
Tools/run-build.sh
Tools/run-tests.sh
```

游戏侧：

```bash
cd /home/slime/Code/SlimeAI/Games/BrotatoLike
Tools/run-build.sh
Tools/run-godot-scene.sh run res://SlimeAI/Src/Validation/Runtime/Event/RuntimeEventValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
Tools/analyze-godot-scene-logs.sh
```

## PASS/FAIL 判定

PASS 必须同时满足：

- Godot 进程 exit code 为 `0`。
- stdout 至少包含每个检查项的 `[INFO]` / `[PASS]` 分级日志。
- stdout 包含 `GameOS Runtime Event validation PASS`。
- artifact `runtime-event-validation.json` 的 `status` 为 `pass`。
- artifact `failureReasons` 为空。

FAIL 判定：

- stdout 包含 `GameOS Runtime Event validation FAIL`，或 Godot 进程 exit code 非 `0`。
- stdout 包含对应检查项的 `[FAIL]` 日志。
- artifact `runtime-event-validation.json` 的 `status` 为 `fail`。
- artifact `failureReasons` 至少包含一条失败原因。

## Standard answer

### expectedInputs

- `EntityEventBus` typed subscriptions、`WorldEvents.World` subscriptions。
- handler exception、same-type reentry 和 invalid scope publish attempts。
- `RuntimeEntity.Data.Set` 到 `DataPropertyChanged` bridge。

### expectedObservations

- registration order、unsubscribe 和 handler exception isolation deterministic。
- same-type reentry 被阻断，invalid scope publish 不派发。
- broadcast event 同时到达 entity bus 和 world bus，data bridge emits `DataPropertyChanged`。

### passCriteria

- `index.json` 对应 entry `status=passed` 且 `exitCode=0`。
- per-scene `result.json` `status=passed`，`firstError=null`。
- artifact `status=pass`，`failureReasons=[]`。
- `checks[]` 覆盖 registration、dispose、exception、reentry、entity rejection、broadcast 和 data bridge。

### failCriteria

- stdout 含 `GameOS Runtime Event validation FAIL`。
- 任一 event ordering、subscription、guard、scope 或 bridge check 失败。
- artifact `status=fail`、`failureReasons` 非空或标准答案字段缺失。

### artifactPath

`artifacts/runtime-event-validation.json`

## 日志

场景使用框架侧 `SceneValidationSession` 输出固定文本格式，便于 Godot Output、人类终端和 AI 检索：

```text
[INFO][RuntimeEventValidation] check registration_order start
[PASS][RuntimeEventValidation] check registration_order registration order preserved
[FAIL][RuntimeEventValidation] check <name> <reason>
```

日志不是 artifact 的替代品。stdout 用于快速判断执行路径，artifact 用于保留完整结构化证据。

## Artifact

scene runner 会把 artifact 写入：

```text
.ai-temp/scene-tests/runs/<date>/<time>/<scene-attempt>/artifacts/runtime-event-validation.json
```

同一场景还会写入：

```text
.ai-temp/scene-tests/runs/<date>/<time>/<scene-attempt>/artifacts/logs/scene-log.jsonl
```

字段：

- `status`: `pass` 或 `fail`。
- `scene`: `res://SlimeAI/Src/Validation/Runtime/Event/RuntimeEventValidation.tscn`。
- `layer`: `Runtime/Event`。
- `checks`: 检查项数组，每项包含 `name`、`status`、`category`、`details`。
- `logs`: 与 stdout 同源的关键日志条目，每项包含 `timestamp`、`level`、`context`、`message`、`values`。
- `failureReasons`: 失败原因数组，PASS 时为空。
- `dependencies`: 本场景允许依赖。
- `notes`: 跨层桥接说明和后续扩展说明。

## 常见失败排查顺序

1. 先看 Godot 进程 exit code 和 stdout 的 PASS/FAIL marker。
2. 打开最新 run 目录中的 `artifacts/runtime-event-validation.json`，查看 `failureReasons`。
3. 如果 `handler_exception_capture` 失败，确认异常 handler 后续订阅仍执行。
4. 如果 `same_type_reentry_blocked` 失败，确认重入调用是否被同 bus 阻断。
5. 如果 `entity_event_rejected_on_world_bus` 失败，确认 `IEntityEvent` 是否被错误地路由到 world bus。
6. 如果 `data_to_event_bridge` 失败，按跨层桥接处理，检查 `RuntimeEntity`、`Data.Set` 和 `EventDataChangeSink`，不要先修改 EventBus。
