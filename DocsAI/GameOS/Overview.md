# GameOS

`GameOS` 是 SlimeAI 的 AI-first GameOS 框架层，运行时架构是 `Capability Composition Runtime`。它借鉴组合式架构，但不把 SlimeAI 定位成传统 ECS 框架；新文档和新 API 设计应优先使用 Runtime Entity、Runtime Data / DataKey、Capability、GodotBridge Adapter 和 Runtime Process 这些术语。

## 子域

- `Runtime/`：Runtime Entity、Runtime Data / DataKey、Event、Relationship、RuntimeSchedule、Pool、Timer、Resource。
- `Capabilities/`：Movement、Collision、Damage、Ability、Feature、AIBehavior、Projectile、Spawn、UIHud 等能力包。
- `Validation/`：构建、场景测试、Capability 测试和回归门禁。
- `Observation/`：日志、dump、trace、snapshot 和报告。
- `GodotBridge/`：Godot Node、SceneTree、Resource、Physics 和表现层 adapter。

## 术语表

| 推荐术语 | 允许的 legacy code symbol / 搜索词 | 禁止解释 | 路由目标 |
| --- | --- | --- | --- |
| `AI-first GameOS` | `GameOS`、历史 `ECS` 搜索 | 传统 ECS framework、通用游戏引擎 | `Framework/Overview.md`、`Framework/Principles.md`、ECS 边界 ADR |
| `Capability Composition Runtime` | `Runtime`、历史 `ECS` 搜索 | global world query、archetype/chunk/sparse-set runtime | `GameOS/Contracts.md`、`CapabilityIndex.md` |
| `Runtime Entity` | `IEntity`、`RuntimeEntity`、`EntityManager`、`Entity` | archetype entity、行为继承根、业务逻辑 owner | `GameOS/Contracts.md` 的 Runtime Entity 契约、`ecs-entity` skill |
| `Runtime Data / DataKey` | `Data`、`DataKey<T>`、`DataCatalog`、`DataSlot`、历史 `Component` 搜索 | ECS component storage、裸 string/object 字段池 | `ecs-data`、`data-authoring`、DataOS snapshot 文档 |
| `Capability` | `Movement`、`Damage`、`Ability` 等 capability 目录 | 泛型 Component、无 owner 的行为片段 | `GameOS/Capabilities/CapabilityIndex.md` 和对应 owner skill |
| `GodotBridge Adapter` | `IGodotComponent`、`Godot*Component`、`AttackComponent`、`Component` | ECS data component、玩法状态真相源 | `GodotBridge/`、`ecs-component` skill；旧符号只是 compatibility name |
| `Runtime Process` / `Schedule Process` | `IRuntimeSystem`、`SystemConfig`、`SystemRunCondition`、`MovementSystem`、`System` | ECS query system、绕过 Capability owner 的全局处理器 | `RuntimeSchedule`、`GameOS/Contracts.md` 的 Runtime Schedule 契约 |

`ECS / Entity / Component / System` 作为搜索词必须能导向本表。`ECS` 只允许出现在历史迁移、外部框架对照、拒绝清单或 ADR 解释语境；新实现不得以传统 ECS 心智模型新增 world query、纯数据 component storage 或 query system。

## 当前状态

已创建 `SlimeAI.GameOS` 最小可构建包，当前包含 Data / Event / Entity / Relationship / Schedule / Resource / Pool / Timer 纯 C# Runtime 最小内核，并已迁入 GodotBridge 第一版、Movement / Collision / Damage / Ability / Feature / AI / Attack 第一批，以及 Projectile / Effect 纯 Runtime 生成、Projectile 命中生命周期、穿透 / 生命周期扩展和 Effect 动画播放第一段。

GodotBridge 当前包含：

- `GodotEntity`：可挂场景的 Runtime Entity 基类，进入 / 离开 SceneTree 时接入 `EntityManager`。
- `IGodotComponent`：GodotBridge Adapter 的 legacy compatibility name，不是传统 ECS data component。
- `GameOSGodotBridge`：Node Entity、GodotBridge Adapter 注册 / 注销和 legacy Entity-Component 关系绑定入口。
- `GodotNodeRegistry`：Godot Node 到稳定运行时 Id 的注册表。
- `GameOSTimerDriver`：用 `_Process` 驱动 `TimerManager.Instance.Tick`。

## 构建

```bash
dotnet build GameOS/SlimeAI.GameOS.csproj
Tools/run-build.sh
```

## 契约

- `Contracts.md`
- `ApiIndex.md`
- `DebugGuide.md`
- `SlimeAI.GameOS.Migration.md`
