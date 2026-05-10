# GameOS

`GameOS` 是 Godot AI Game OS 的框架层。

## 子域

- `Runtime/`：Entity、Component、Data、Event、Relationship、Schedule、Pool、Timer、Resource。
- `Capabilities/`：Movement、Collision、Damage、Ability、Feature、AIBehavior、Projectile、Spawn、UIHud 等能力包。
- `Validation/`：构建、场景测试、Capability 测试和回归门禁。
- `Observation/`：日志、dump、trace、snapshot 和报告。
- `GodotBridge/`：Godot Node、SceneTree、Resource、Physics bridge。

## 当前状态

已创建 `SkilmeAI.GameOS` 最小可构建包，当前包含 Data / Event / Entity / Relationship / Schedule / Resource / Pool / Timer 纯 C# Runtime 最小内核，并已迁入 GodotBridge 第一版、Movement / Collision / Damage / Ability / Feature / AI / Attack 第一批，以及 Projectile / Effect 纯 Runtime 生成、Projectile 命中生命周期、穿透 / 生命周期扩展和 Effect 动画播放第一段。

GodotBridge 当前包含：

- `GodotEntity`：可挂场景的 Entity 基类，进入 / 离开 SceneTree 时接入 `EntityManager`。
- `IGodotComponent`：Godot Component 生命周期协议。
- `GameOSGodotBridge`：Node Entity、Component 注册 / 注销和 Entity-Component 关系绑定入口。
- `GodotNodeRegistry`：Godot Node 到稳定运行时 Id 的注册表。
- `GameOSTimerDriver`：用 `_Process` 驱动 `TimerManager.Instance.Tick`。

## 构建

```bash
dotnet build GameOS/SkilmeAI.GameOS.csproj
Tools/run-build.sh
```

## 契约

- `Contracts.md`
- `ApiIndex.md`
- `DebugGuide.md`
- `SkilmeAI.GameOS.Migration.md`
