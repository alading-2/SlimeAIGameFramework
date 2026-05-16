# GameOS Godot Scene Validation

> Status: runtime-validation-contract
> First scene: `Runtime/Event/RuntimeEventValidation`
> 标准答案集中索引：[ValidationCatalog.md](ValidationCatalog.md)

## 目标

GameOS 基础层验证采用“框架规格 + 游戏承载”模式：

- `SlimeAI/GameOS/Validation/` 定义验证契约、命名规则、artifact 字段和判定规则。
- `Games/BrotatoLike` 承载实际可运行的 Godot `.tscn` 场景，因为 `res://` 绑定当前 `project.godot` root。

该体系用于补齐纯 C# 测试之外的 Godot headless 运行证据。当前已覆盖：

```text
Runtime/Entity
Runtime/Lifecycle
Runtime/World
Runtime/Data
Runtime/Event
Runtime/CommandBuffer
GameOS/Observation
BrotatoLike Game/Input
```

新增功能如果依赖 GodotBridge、Godot Node 生命周期、Physics、Input、Resource、UI、动画或游戏侧胶水，必须新增独立验证场景。`run-main-smoke` 和普通主场景 playable acceptance 只能作为回归补充，不能替代该功能自己的验证场景。

## 场景命名

- 目录使用层级路径：`Scenes/Validation/<Area>/<Layer>/`。
- 文件名、目录名、C# 类名和 Godot 节点名使用英文 ASCII。
- 场景名使用 `<Area><Layer>Validation.tscn` 或能表达同等层级的 ASCII 名称。
- C# 脚本放在 `Src/SceneTests/<Area>/<Layer>/`。
- README 放在场景同目录，文件名固定为 `README.md`。

当前 Runtime/Data 场景固定为：

```text
Games/BrotatoLike/SlimeAI/Scenes/Validation/Runtime/Data/RuntimeDataValidation.tscn
Games/BrotatoLike/SlimeAI/Scenes/Validation/Runtime/Data/README.md
Games/BrotatoLike/SlimeAI/Src/SceneTests/Runtime/Data/RuntimeDataValidationScene.cs
```

当前 Runtime/Event 场景固定为：

```text
Games/BrotatoLike/Scenes/Validation/Runtime/Event/RuntimeEventValidation.tscn
Games/BrotatoLike/Scenes/Validation/Runtime/Event/README.md
Games/BrotatoLike/Src/SceneTests/Runtime/Event/RuntimeEventValidationScene.cs
```

AI-first Runtime refactor validation scenes 固定为：

```text
Games/BrotatoLike/SlimeAI/Scenes/Validation/Runtime/Entity/RuntimeEntityValidation.tscn
Games/BrotatoLike/SlimeAI/Scenes/Validation/Runtime/Entity/README.md
Games/BrotatoLike/SlimeAI/Src/SceneTests/Runtime/Entity/RuntimeEntityValidationScene.cs

Games/BrotatoLike/SlimeAI/Scenes/Validation/Runtime/Lifecycle/RuntimeLifecycleValidation.tscn
Games/BrotatoLike/SlimeAI/Scenes/Validation/Runtime/Lifecycle/README.md
Games/BrotatoLike/SlimeAI/Src/SceneTests/Runtime/Lifecycle/RuntimeLifecycleValidationScene.cs

Games/BrotatoLike/SlimeAI/Scenes/Validation/Runtime/World/RuntimeWorldValidation.tscn
Games/BrotatoLike/SlimeAI/Scenes/Validation/Runtime/World/README.md
Games/BrotatoLike/SlimeAI/Src/SceneTests/Runtime/World/RuntimeWorldValidationScene.cs

Games/BrotatoLike/SlimeAI/Scenes/Validation/Runtime/CommandBuffer/RuntimeCommandBufferValidation.tscn
Games/BrotatoLike/SlimeAI/Scenes/Validation/Runtime/CommandBuffer/README.md
Games/BrotatoLike/SlimeAI/Src/SceneTests/Runtime/CommandBuffer/RuntimeCommandBufferValidationScene.cs
```

BrotatoLike game-owned input validation scene 固定为：

```text
Games/BrotatoLike/Scenes/Validation/Game/Input/BrotatoLikeInputEventValidation.tscn
Games/BrotatoLike/Scenes/Validation/Game/Input/README.md
Games/BrotatoLike/Src/SceneTests/Game/Input/BrotatoLikeInputEventValidationScene.cs
```

## README 必填项

每个场景旁的 README 必须包含：

- 测试目标。
- 允许依赖。
- 不覆盖内容。
- 运行命令。
- 标准答案：expected inputs、expected observations、pass criteria、fail criteria。
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
  "expectedInputs": [],
  "expectedObservations": [],
  "passCriteria": [],
  "failCriteria": [],
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
- `expectedInputs`: 场景预设输入、模拟输入、初始 DataKey / Event / Resource 条件。
- `expectedObservations`: AI 用来判断方向是否正确的标准答案，例如期望实体状态、事件顺序、目标选择结果、UI/Bridge 状态或 artifact 字段。
- `passCriteria`: 通过条件，必须能映射到 `checks` 或总 PASS marker。
- `failCriteria`: 失败条件，必须能映射到 `failureReasons` 或 `[FAIL]` 日志。
- `checks`: 每个检查项的结构化证据，至少包含 `name`、`status`、`category`。
- `logs`: 与 stdout 同源的关键日志条目，至少包含 `level`、`context`、`message`。
- `failureReasons`: 失败原因数组；PASS 时必须为空。
- `dependencies`: 运行场景允许依赖的框架和游戏侧承载项。
- `notes`: 非核心断言、跨层桥接或排查提示。

## 日志约定

场景验证必须同时产出人类可读日志和机器可读 artifact。日志不是 artifact 的替代品；它用于快速定位“跑到哪一步、哪一步失败、关键观测值是什么”。artifact 是 AI / CI 复验事实源。默认日志面向 AI 分析，不面向人工实时阅读；不要打印大量每帧噪声，除非写入单独 trace 文件并在 artifact 中索引。

框架侧 `SceneValidationSession` 是默认 helper；场景脚本只声明检查项，helper 负责 check start、PASS/FAIL、失败聚合、JSONL 和 validation artifact。

每个检查项必须至少输出：

- 开始：`[INFO][<SceneName>] check <name> start`
- 通过：`[PASS][<SceneName>] check <name> <summary>`
- 失败：`[FAIL][<SceneName>] check <name> <reason>`

日志规则：

- stdout 日志使用 ASCII level 和 scene context，便于 `rg` 检索。
- Godot 承载场景可以使用 `GD.PrintRich` 做颜色输出，但原始文本必须包含 `[INFO]`、`[PASS]`、`[FAIL]`、`[WARN]` 或 `[ERROR]`。
- 失败日志必须和 artifact `failureReasons` 对应。
- 每个检查日志的 `check <name>` 必须能对应 artifact `checks[].name`。
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

新增 AI-first Runtime refactor 场景 marker：

```text
GameOS Runtime Entity validation PASS
GameOS Runtime Entity validation FAIL
GameOS Runtime Lifecycle validation PASS
GameOS Runtime Lifecycle validation FAIL
GameOS Runtime World validation PASS
GameOS Runtime World validation FAIL
GameOS Runtime CommandBuffer validation PASS
GameOS Runtime CommandBuffer validation FAIL
BrotatoLike Game Input validation PASS
BrotatoLike Game Input validation FAIL
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

- Runtime/Data 核心断言只依赖 `SlimeAI.GameOS.Runtime.Data`。
- 使用 `RuntimeEntity` 或 `DataPropertyChanged` 的检查必须标注为 Data-to-Event bridge 检查。
- EventBus 核心断言只依赖 `SlimeAI.GameOS.Runtime.Event`。
- 使用 `RuntimeEntity` 或 `Data.Set` 的检查必须标注为跨层桥接检查。
- `WorldEvents.World` 使用前后必须 Clear，避免污染同进程后续场景。
- 游戏侧代码只能作为 Godot 承载和 runner 接入，不应引入 BrotatoLike 玩法行为作为基础层通过条件。

## Runtime/Data 第一轮覆盖

`RuntimeDataValidation` 必须验证：

- `DataCatalog` 能 typed resolve `DataKey<T>` 并保留 capability enabled/disabled metadata。
- `DataKey<T>` default、`Set`、`Get`、`TryGet`、`Has`、`Remove` 和 `GetAll` typed lifecycle。
- min/max clamp 和 options 非法值拒绝。
- numeric modifier 叠加、重复 modifier 拒绝、移除 modifier，以及 computed key dependency dirty。
- `ResetByCategory` 只重置目标分类中的显式值。
- `RuntimeEntity + Data.Set(DataKey<T>)` 触发 `Runtime.Events.Core.DataPropertyChanged`，并保留 stable key、old value、new value。

## Runtime/Event 第一轮覆盖

`RuntimeEventValidation` 必须验证：

- 注册顺序即派发顺序。
- `IDisposable.Dispose()` 退订后不再派发。
- handler exception 被 `EventBusObservation` 捕获，后续 handler 继续执行。
- 同类型嵌套 Publish 被 per-bus reentry guard 阻断，并记录尝试次数和实际执行次数。
- `IEntityEvent` 被 `WorldEventBus.Publish` 时 log Error + return，不派发。
- `IBroadcastEvent` 在 entity bus 一次 Publish MUST 同时派发到 entity bus 和 world bus。
- `RuntimeEntity + Data.Set` 触发 `Runtime.Events.Core.DataPropertyChanged`，并标注为 Data-to-Event 跨层桥接检查。

## AI-first Runtime refactor 场景覆盖

`RuntimeEntityValidation` 必须验证：

- `EntityId.Empty`、`default(EntityId)`、`EntityId.From(null)` 和 `EntityId.From("")` 的 empty 语义。
- `new EntityId(value)` 和 `.Value / ToString()` 显式取值边界。
- `RuntimeWorld.CreateScoped().Entities.Spawn` 的 custom id 和 generated id。
- `EntitySpawned / EntityDestroyed` payload 使用 typed `EntityId`。

`RuntimeLifecycleValidation` 必须验证：

- attach / detach 事件与 parent 查询。
- single-parent、self、empty 和 cycle attach 拒绝。
- parent destroy 时 `DestroyRecursively` 子实体被销毁，`Detach` 子实体保留并断链。
- `EntityIdList` 不重复、值相等、移除后保持不可变。
- `RuntimeOwnedReferenceRegistry` descriptor 能从 owner list 清理被销毁 child。

`RuntimeWorldValidation` 必须验证：

- `CreateScoped()` 隔离 entity、resource 和 event bus。
- `RuntimeWorld.Default.Dispose()` 抛 `InvalidOperationException` 且不进入 disposed 状态。
- scoped world dispose 顺序为 `Schedule -> Commands -> Pools -> Resources -> Lifecycle -> Entities -> Events`。
- dispose 期间 lifecycle detach 和 entity destroy event 可送达。
- dispose 后 subsystem getter 抛 `ObjectDisposedException`，pending commands 生成 discard report。

`RuntimeCommandBufferValidation` 必须验证：

- 8 种 command kind：`Spawn / Destroy / Attach / Detach / QueuedEvent / ResourceRequest / GodotNodeInstantiate / GodotNodeFree`。
- `SchedulePhase` 只播放目标 phase，`Manual` 不被 frame phases 自动播放。
- `EnterGuard(reason)` 支持嵌套并在 outer dispose 后释放。
- guard 内 spawn / destroy / attach / detach 延迟到 `EndOfFrame` playback。
- world dispose 时 pending commands 标记为 `Skipped / WorldDisposing`。

`BrotatoLikeInputEventValidation` 必须验证：

- `BrotatoLikePlayerInputComponent` 写入 `MovementDataKeys.InputDirection`。
- 技能输入事件类型归属 `BrotatoLike.Game.Events`。
- `InputNextSkill / InputPreviousSkill / InputUseSkill` 能驱动 `GodotActiveSkillInputComponent` 切换并触发当前技能。
