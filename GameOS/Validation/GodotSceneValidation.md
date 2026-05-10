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

## Artifact 约定

使用 scene runner 的 `GODOT_SCENE_TEST_ARTIFACT_DIR` 写入 JSON。缺失该环境变量时，场景可以退化写入当前目录下的 `.ai-temp/scene-tests/manual/artifacts`，但 CI 和验收命令必须使用 `--log-dir`。

每个 artifact 必须至少包含：

```json
{
  "status": "pass",
  "scene": "res://Scenes/Validation/Runtime/Event/RuntimeEventValidation.tscn",
  "layer": "Runtime/Event",
  "checks": [],
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
- `failureReasons`: 失败原因数组；PASS 时必须为空。
- `dependencies`: 运行场景允许依赖的框架和游戏侧承载项。
- `notes`: 非核心断言、跨层桥接或排查提示。

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
- `GlobalEventBus.Global` 使用前后必须清理，避免污染同进程后续场景。
- 游戏侧代码只能作为 Godot 承载和 runner 接入，不应引入 BrotatoLike 玩法行为作为基础层通过条件。

## Runtime/Event 第一轮覆盖

`RuntimeEventValidation` 必须验证：

- typed handler 和 parameterless handler 触发。
- priority 顺序。
- `Once` 只执行一次。
- `Off` 移除 handler。
- handler exception 被 `HandlerException` 捕获，后续 handler 继续执行。
- 同事件重入被阻止，并记录尝试次数和实际执行次数。
- `EventContext.StopPropagation()` 阻断低优先级 handler。
- entity-local `EventBus` 与 `GlobalEventBus.Global` 不混淆。
- `RuntimeEntity + Data.Set` 触发 `GameEventType.Data.PropertyChanged`，并标注为 Data-to-Event 跨层桥接检查。
