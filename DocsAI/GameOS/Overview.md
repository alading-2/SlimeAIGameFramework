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

## AI 路由：GodotBridge 三条路径

GodotBridge 是 AI 的边界护栏，不是必须通过的强制层。AI 创建 Godot 实体时按需选择：

| 路径 | 做法 | 适用场景 |
| --- | --- | --- |
| 直接继承 | 继承 `GodotEntity2D`，在子类里直接写 Godot API（`Sprite2D`、`AnimationPlayer` 等） | 简单实体、一次性原型、不需要框架通用 Adapter |
| 使用 Composer | 调用 `GodotUnitComposer.Compose(entity, profile)`，按 profile 自动挂载 animation/orientation/AI/attack/hurtbox/contact damage adapter | 标准敌人/玩家，需要框架通用表现层 Adapter |
| 自定义 Adapter | 自己写 `IGodotComponent` 实现，通过 `GameOSGodotBridge.RegisterComponents` 注册 | 框架 Adapter 不满足需求，需要自定义生命周期桥接 |

关键原则：三条路径的 Entity 都通过 `Entity.Data` 持有运行时状态，通过 `Entity.Events` 收发事件。Godot 场景节点只负责表现，不做玩法真相源。

## 功能开关总览

AI 可通过三层开关控制功能的启用/禁用：

| 层 | 机制 | 生效时机 | 适用场景 |
| --- | --- | --- | --- |
| DataOS | `capability_manifest.enabled = 0/1`（seed SQL） | snapshot 生成时 | 永久关闭某个 Capability，其 DataKey 不进 DataCatalog |
| RuntimeSchedule | `schedule.SetSystemEnabled("SpawnSystem", false)` | 运行时即时 | 动态启停某个 System 的 Tick |
| ProjectState | `schedule.ProjectState.OpenPauseMenu()` / `BeginGameplaySession()` | 运行时即时 | 整个游戏状态切换（暂停/恢复/菜单） |

优先级：DataOS 是编译/生成期开关（不可运行时改变），RuntimeSchedule 和 ProjectState 是运行期开关（可动态切换）。

游戏侧可通过 Godot `[Export]` 变量或 `GameConfig` 资源暴露开关给人类玩家；DataOS 开关改 seed SQL 后需重新生成 snapshot。

## 契约

- `Contracts.md`
- `ApiIndex.md`
- `DebugGuide.md`
- `GodotPitfalls.md`：AI 写 Godot 表现层代码前必读的陷阱知识库（坐标系、Camera、生命周期、输入门控等）
- `SlimeAI.GameOS.Migration.md`
