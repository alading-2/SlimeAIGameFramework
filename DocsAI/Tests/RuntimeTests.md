# Runtime Tests

## 定位

`Tests/SkilmeAI.GameOS.Tests/` 是 SkilmeAI.GameOS 的纯 C# Runtime 行为测试入口。它不是打包目录，也不是发布产物目录。

这个测试项目用于在不启动 Godot 的情况下验证 GameOS 核心运行时契约。可以把它理解为框架层的最小运行沙盘：直接创建 Runtime Entity、写入 DataKey、触发 EventBus、Tick Runtime service，然后用断言检查结果。

它的目标不是证明游戏完整可玩，而是在 Godot 场景测试之前快速发现框架纯逻辑回归。

## 它模拟什么

Runtime 测试会手动搭建最小对象和服务调用链，例如：

- 创建 `RuntimeEntity`，读写 Runtime data。
- 订阅和派发 `EntityEventBus` / `WorldEventBus`（通过 `Publish<T> / Subscribe<T> → IDisposable`）。
- 调用 `MovementSystem.Tick(delta)` 推进位置和碰撞事件。
- 调用 `DamageService`、`HealService`、`DamageTool` 检查伤害结算。
- 调用 `AbilityService.TryTrigger` 检查目标、冷却、充能和 Feature handler 接入。
- 创建投射物 / effect Runtime entity，检查命中、穿透、生命周期。
- 调用 AI 行为节点，检查索敌、移动、攻击请求和自动技能上下文。
- 写入 Observation / Validation artifact，检查可验证输出。

## 它不模拟什么

Runtime 测试不启动 Godot，所以不覆盖：

- Godot Node 生命周期、场景树和信号连接。
- PhysicsServer2D、Area2D、CollisionShape2D 的真实物理行为。
- C# 脚本挂载、导出字段、资源路径和场景资源加载。
- 输入、UI、渲染、动画播放和真实游戏主循环。
- BrotatoLike 专属玩法胶水和真实主场景接入。

这些内容必须使用 Godot scene runner 或 BrotatoLike smoke 验证。

## 覆盖范围

当前测试入口是：

```text
Tests/SkilmeAI.GameOS.Tests/Program.cs
```

它覆盖：

- Runtime 基础设施：EventBus、Entity、Relationship、Schedule、ObjectPool、Timer、ResourceCatalog。
- Runtime DataOS：snapshot 写入 Runtime data 和 ResourceCatalog。
- Capability 纯逻辑：Collision、Damage、Ability、Projectile、Effect、Feature、AI、Attack、Movement。
- Observation / Validation：日志等级过滤、JSONL 序列化、SceneValidationSession 聚合失败 artifact。

## 命令

```bash
cd /home/slime/Code/SkilmeAI/SkilmeAI
Tools/run-build.sh
Tools/run-tests.sh
```

`Tools/run-tests.sh` 会先执行 DataOS schema / seed 验证，再运行 Runtime 测试项目：

```bash
Tools/run-dataos-validate.sh
dotnet run --project Tests/SkilmeAI.GameOS.Tests/SkilmeAI.GameOS.Tests.csproj
```

## 与构建和打包的关系

- `SkilmeAI.slnx` 包含 `Tests/SkilmeAI.GameOS.Tests/`，所以 `Tools/run-build.sh` 会构建测试项目。
- `Tools/run-tests.sh` 会执行测试项目。
- `Tools/run-pack.sh` 只打包 `GameOS/SkilmeAI.GameOS.csproj` 到 `Packages/LocalNuGet`，不会打包 `Tests/`。

## 什么时候补测试

修改以下内容时，优先在 Runtime 测试中补最小行为断言：

- Runtime Entity / Data / Event / Relationship / Schedule。
- Capability 的纯 C# 逻辑和 DataKey 行为。
- DataOS snapshot 到 Runtime 的装载结果。
- Observation、Validation 和可检查 artifact 的生成逻辑。

如果行为依赖 Godot Node、PhysicsServer、场景树、输入或渲染，使用 Godot scene runner 验证，入口见 `DocsAI/Tests/GodotSceneTesting.md`。

## 新增测试规则

- 测试名必须描述行为，不只描述方法名。
- 每个测试只验证一个主要行为，必要时拆成多个测试。
- 优先断言最终可观察结果，例如 data、event、entity 状态、artifact 内容。
- 不要把游戏侧资源路径、场景路径、BrotatoLike 专属玩法写进 Runtime 测试。
- 需要临时文件时写入系统临时目录或 `.ai-temp`，不要污染源码目录。
- 新 Capability 的纯 C# 最小闭环应先有 Runtime 测试，再补 Godot scene 验证。

## 与 Godot 场景测试的分工

| 验证问题 | Runtime Tests | Godot Scene Tests |
| --- | --- | --- |
| DataKey 写入后服务逻辑是否正确 | 是 | 可选 |
| Damage / Ability / Movement 纯规则是否正确 | 是 | 可选 |
| Godot 节点是否挂载正确 | 否 | 是 |
| Area2D / PhysicsServer2D 是否真实触发 | 否 | 是 |
| 资源路径、场景加载、动画、UI 是否正常 | 否 | 是 |
| BrotatoLike 主场景是否可玩 | 否 | 是 |

## 判断价值

这个测试目录有保留价值：它是框架层最快的回归网，能在 Godot 场景测试之前发现 Runtime 逻辑破坏。它不能替代 BrotatoLike 的 Godot headless smoke，因为它不覆盖真实 Godot 节点生命周期、物理 broadphase、场景资源和游戏侧胶水。

如果未来测试数量继续增长，可以把当前单文件 `Program.cs` 逐步拆成按模块组织的测试文件，例如 `DamageTests.cs`、`AbilityTests.cs`、`MovementTests.cs`。拆分属于维护性改进，不改变它作为 Runtime 回归入口的定位。
