# Runtime Data Validation

## 测试目标

验证 GameOS `Runtime/Data` typed contract 在 Godot headless 场景中的可运行行为，并产出稳定分级日志、PASS/FAIL marker 与 JSON artifact。

核心 Data 断言覆盖：

- `DataKey<T>` 作为唯一业务访问入口。
- `DataCatalog` typed resolve 和 capability metadata。
- default、`Set`、`Get`、`TryGet`、`Has`、`Remove`、`GetAll`。
- min/max clamp、options 拒绝非法值。
- numeric modifier 叠加、移除和 computed key dependency dirty。
- `ResetByCategory` 只重置目标分类。
- `RuntimeEntity + Data.Set(DataKey<T>)` 触发 `DataPropertyChanged`，payload 保留 stable key、old value 和 new value。

## 允许依赖

- Godot scene runner: `Tools/run-godot-scene.sh`
- GameOS Runtime Data: `Data`、`DataKey<T>`、`DataCatalog`、`DataModifier`
- GameOS Runtime Entity/Event bridge: `RuntimeEntity`、`EventDataChangeSink`、`DataPropertyChanged`
- GameOS Observation: `SceneValidationSession`
- BrotatoLike 仅作为 `res://` 场景承载工程，不引入玩法代码作为通过条件。

## 不覆盖内容

- 不验证 DataOS SQLite schema、snapshot generator 或 validator。
- 不验证 Capability 专属 DataKeys。
- 不验证 GodotBridge 组件、BrotatoLike 玩法、UI、Physics 或资源加载。
- 不改变 Runtime Data API。

## 运行命令

框架侧：

```bash
cd /home/slime/Code/SkilmeAI/SkilmeAI
Tools/run-build.sh
Tools/run-tests.sh
```

游戏侧：

```bash
cd /home/slime/Code/SkilmeAI/Games/BrotatoLike
Tools/run-build.sh
Tools/run-godot-scene.sh run res://SkilmeAI/Scenes/Validation/Runtime/Data/RuntimeDataValidation.tscn --timeout 10 --log-dir .ai-temp/scene-tests/runs
Tools/analyze-godot-scene-logs.sh
```

## PASS/FAIL 判定

PASS 必须同时满足：

- Godot 进程 exit code 为 `0`。
- stdout 至少包含每个检查项的 `[INFO]` / `[PASS]` 分级日志。
- stdout 包含 `GameOS Runtime Data validation PASS`。
- artifact `runtime-data-validation.json` 的 `status` 为 `pass`。
- artifact `failureReasons` 为空。

FAIL 判定：

- stdout 包含 `GameOS Runtime Data validation FAIL`，或 Godot 进程 exit code 非 `0`。
- stdout 包含对应检查项的 `[FAIL]` 日志。
- artifact `runtime-data-validation.json` 的 `status` 为 `fail`。
- artifact `failureReasons` 至少包含一条失败原因。

## Artifact

scene runner 会把 artifact 写入：

```text
.ai-temp/scene-tests/runs/<date>/<time>/<scene-attempt>/artifacts/runtime-data-validation.json
```

同一场景还会写入：

```text
.ai-temp/scene-tests/runs/<date>/<time>/<scene-attempt>/artifacts/logs/scene-log.jsonl
```

字段：

- `status`: `pass` 或 `fail`。
- `scene`: `res://SkilmeAI/Scenes/Validation/Runtime/Data/RuntimeDataValidation.tscn`。
- `layer`: `Runtime/Data`。
- `checks`: 检查项数组，每项包含 `name`、`status`、`category`、`details`。
- `logs`: 与 stdout 同源的关键日志条目。
- `failureReasons`: 失败原因数组，PASS 时为空。
- `dependencies`: 本场景允许依赖。
- `notes`: Data-to-Event bridge 和非覆盖范围说明。

## 常见失败排查顺序

1. 先看 Godot 进程 exit code 和 stdout 的 PASS/FAIL marker。
2. 打开最新 run 目录中的 `artifacts/runtime-data-validation.json`，查看 `failureReasons`。
3. 如果 `catalog_resolve_and_capability_metadata` 失败，检查 `DataCatalogBuilder` 是否包含测试 key 和 capability metadata。
4. 如果 `typed_value_lifecycle` 失败，检查 `Data.Set/Get/TryGet/Has/Remove/GetAll` 的 typed slot 逻辑。
5. 如果 `modifier_and_computed_dirty` 失败，检查 modifier 排序、计算键依赖和 dirty 标记。
6. 如果 `entity_data_change_event` 失败，检查 `RuntimeEntity`、`EventDataChangeSink` 和 `DataPropertyChanged`。
