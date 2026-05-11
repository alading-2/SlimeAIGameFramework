# GameOS Godot Scene Validation

> Status: runtime-validation-contract
> First scene: `Runtime/Event/RuntimeEventValidation`

## 目标

GameOS 基础层验证采用“框架规格 + 游戏承载”模式：

- `SkilmeAI/GameOS/Validation/` 定义验证契约、命名规则、artifact 字段和判定规则。
- `Games/BrotatoLike` 承载实际可运行的 Godot `.tscn` 场景，因为 `res://` 绑定当前 `project.godot` root。

该体系用于补齐纯 C# 测试之外的 Godot headless 运行证据。第一轮只覆盖 `Runtime/Event`，后续按基础层顺序扩展：

```text
Data -> Entity -> Pool -> Timer -> Resource -> Schedule -> Relationship -> Observation
```

## 场景命名

- 目录使用层级路径：`Scenes/Validation/<Area>/<Layer>/`。
- 文件名、目录名、C# 类名和 Godot 节点名使用英文 ASCII。
- 场景名使用 `<Area><Layer>Validation.tscn` 或能表达同等层级的 ASCII 名称。
- C# 脚本放在 `Src/SceneTests/<Area>/<Layer>/`。
- README 放在场景同目录，文件名固定为 `README.md`。

当前 Event 场景固定为：

```text
Games/BrotatoLike/Scenes/Validation/Runtime/Event/RuntimeEventValidation.tscn
Games/BrotatoLike/Scenes/Validation/Runtime/Event/README.md
Games/BrotatoLike/Src/SceneTests/Runtime/Event/RuntimeEventValidationScene.cs
```

## README 必填项

每个场景旁的 README 必须包含：

- 测试目标。
- 允许依赖。
- 不覆盖内容。
- 运行命令。
- PASS/FAIL 判定。
- artifact 路径和字段。
- 常见失败排查顺序。

README 的人类说明可以使用中文；命令、路径、marker 和字段名保持原文。

## Runner 输出结构

统一 runner 由 skill `.codex/skills/godot-scene-test/scripts/godot-scene-runner.mjs` 提供，BrotatoLike `Tools/run-godot-scene.sh` 只是薄封装。使用 `--log-dir .ai-temp/scene-tests/runs` 时输出：

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

每个场景进程会收到 `GODOT_SCENE_TEST_RUN_DIR`、`GODOT_SCENE_TEST_SCENE_DIR`、`GODOT_SCENE_TEST_SCREENSHOT_DIR`、`GODOT_SCENE_TEST_ARTIFACT_DIR` 及对应 `_REL` 环境变量。

## Artifact 约定

使用 scene runner 的 `GODOT_SCENE_TEST_ARTIFACT_DIR` 写入 JSON。缺失该环境变量时，场景可以退化写入当前目录下的 `.ai-temp/scene-tests/manual/artifacts`，但 CI 和验收命令必须使用 `--log-dir`。

每个 artifact 必须至少包含：

```json
{
  "status": "pass",
  "scene": "res://Scenes/Validation/Runtime/Event/RuntimeEventValidation.tscn",
  "layer": "Runtime/Event",
  "checks": [],
  "logs": [],
  "failureReasons": [],
  "dependencies": [],
  "notes": []
}
```

字段规则：

- `status`: `pass` 或 `fail`。
- `scene`: 当前运行的 `res://` 场景路径。
- `layer`: 被验证的 GameOS 基础层。
- `checks`: 每个检查项的结构化证据，至少包含 `name`、`status`、`category`。
- `logs`: 与 stdout 同源的关键日志条目，至少包含 `level`、`context`、`message`。
- `failureReasons`: 失败原因数组；PASS 时必须为空。
- `dependencies`: 运行场景允许依赖的框架和游戏侧承载项。
- `notes`: 非核心断言、跨层桥接或排查提示。

## 日志约定

场景验证必须同时产出人类可读日志和机器可读 artifact。日志不是 artifact 的替代品；它用于快速定位“跑到哪一步、哪一步失败、关键观测值是什么”。artifact 是 AI / CI 复验事实源。

框架侧 `SceneValidationSession` 是默认 helper；场景脚本只声明检查项，helper 负责 check start、PASS/FAIL、失败聚合、JSONL 和 validation artifact。

每个检查项必须至少输出：

- 开始：`[INFO][<SceneName>] check <name> start`
- 通过：`[PASS][<SceneName>] check <name> <summary>`
- 失败：`[FAIL][<SceneName>] check <name> <reason>`

日志规则：

- stdout 日志使用 ASCII level 和 scene context，便于 `rg` 检索。
- Godot 承载场景可以使用 `GD.PrintRich` 做颜色输出，但原始文本必须包含 `[INFO]`、`[PASS]`、`[FAIL]`、`[WARN]` 或 `[ERROR]`。
- 失败日志必须和 artifact `failureReasons` 对应。
- PASS/FAIL 总 marker 仍必须保留，不能只依赖逐项日志。
- 低层验证不要把大量每帧日志作为默认输出；需要长 trace 时写入 artifact 或单独 trace 文件。

## PASS/FAIL Marker

每个验证场景必须输出稳定 stdout marker：

- PASS: `GameOS <Area> <Layer> validation PASS`
- FAIL: `GameOS <Area> <Layer> validation FAIL`

`Runtime/Event` 第一条场景固定使用：

```text
GameOS Runtime Event validation PASS
GameOS Runtime Event validation FAIL
```

验收标准必须同时满足：

- Godot 进程 exit code 为 `0`。
- stdout 包含 PASS marker。
- artifact `status` 为 `pass`。
- artifact `failureReasons` 为空。

失败时必须：

- Godot 进程 exit code 非 `0`。
- stdout 包含 FAIL marker。
- artifact `status` 为 `fail`。
- artifact `failureReasons` 至少包含一条原因。

## 依赖标注规则

基础层场景允许最小必要依赖，但必须在 README 和 artifact 中标注。

- EventBus 核心断言只依赖 `SkilmeAI.GameOS.Runtime.Event`。
- 使用 `RuntimeEntity` 或 `Data.Set` 的检查必须标注为跨层桥接检查。
- `WorldEvents.World` 使用前后必须 Clear，避免污染同进程后续场景。
- 游戏侧代码只能作为 Godot 承载和 runner 接入，不应引入 BrotatoLike 玩法行为作为基础层通过条件。

## Runtime/Event 第一轮覆盖

`RuntimeEventValidation` 必须验证：

- 注册顺序即派发顺序。
- `IDisposable.Dispose()` 退订后不再派发。
- handler exception 被 `EventBusObservation` 捕获，后续 handler 继续执行。
- 同类型嵌套 Publish 被 per-bus reentry guard 阻断，并记录尝试次数和实际执行次数。
- `IEntityEvent` 被 `WorldEventBus.Publish` 时 log Error + return，不派发。
- `IBroadcastEvent` 在 entity bus 一次 Publish MUST 同时派发到 entity bus 和 world bus。
- `RuntimeEntity + Data.Set` 触发 `Runtime.Events.Core.DataPropertyChanged`，并标注为 Data-to-Event 跨层桥接检查。
