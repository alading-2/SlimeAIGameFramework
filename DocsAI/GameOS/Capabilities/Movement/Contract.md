# Movement Capability Contract

## 1. 解决什么问题

为实体提供可配置、可扩展的移动策略系统。支持从简单的直线移动到复杂的曲线弹道，包括：

- 玩家输入驱动移动（PlayerInput）
- AI 驱动移动（AIControlled）
- 技能/投射物弹道（Charge, SineWave, Orbit, Boomerang, BezierCurve, CircularArc, Parabola）
- 宿主跟随（AttachToHost）

MovementSystem 每帧根据实体的 `MovementParams` 和当前策略计算位移，通过 `GodotMovementDriver` 同步到 Godot `Node2D.Position`。

## 2. 不是本能力职责的内容

- **碰撞检测** → Collision Capability
- **伤害计算** → Damage Capability
- **动画播放** → GodotBridge / 游戏侧 AnimationComponent
- **寻路/网格** → 当前未实现，AI 使用简单方向向量
- **物理模拟** → 纯运动学位移，不依赖 Godot Physics 推动

## 3. 需要的 Runtime 能力

- Entity（实体生命周期）
- Data（DataKey 容器和 Modifier 支持）
- Event（事件发布/订阅）
- Schedule（系统 Tick 调度）

事件目录：`SkilmeAI/GameOS/Capabilities/Movement/Events/`

## 4. 需要哪些其他 Capability

无。Movement 是最底层 Capability 之一，被 Attack/AI/Projectile/Ability 依赖。

## 5. 写入的 DataKey

| DataKey | 类型 | 说明 |
|---------|------|------|
| Movement.Position | Vector2Value | 运行时位置（Strategy 输出） |
| Movement.Velocity | Vector2Value | 本帧速度 |
| Movement.FacingDirection | Vector2Value | 面向方向 |
| Movement.IsMoving | bool | 是否处于活动移动状态 |
| Movement.LastMoveDirection | Vector2Value | 最后一次非零主动方向 |

## 6. 读取的 DataKey

| DataKey | 类型 | 说明 |
|---------|------|------|
| Movement.MoveSpeed | float | 默认移动速度（支持 Modifier） |
| Movement.InputDirection | Vector2Value | 玩家输入方向 |
| Movement.AIMoveDirection | Vector2Value | AI 移动方向 |
| Movement.AIMoveSpeedMultiplier | float | AI 速度倍率 |
| Movement.CanMoveInput | bool | 输入移动门控 |
| Movement.Handler.* | 多种 | Ability/Feature handler 传入的参数 |

完整列表见 `MovementDataKeys.cs`。

## 7. 挂载的 Component

- `MovementSystem` — 全局移动更新系统
- `MovementStrategyRegistry` — 策略注册表
- `GodotMovementDriver`（GodotBridge）— Node2D 位置同步
- `GodotOrientationComponent`（GodotBridge）— RotationDegrees 同步

## 8. 注册的 System / Strategy / Handler

### Strategy（IMovementStrategy 实现）

- `ChargeMovementStrategy`
- `SineWaveMovementStrategy`
- `OrbitMovementStrategy`
- `BoomerangMovementStrategy`
- `BezierCurveMovementStrategy`
- `CircularArcMovementStrategy`
- `ParabolaMovementStrategy`
- `AttachToHostMovementStrategy`
- `PlayerInputMovementStrategy`
- `AIControlledMovementStrategy`

### System

- `MovementSystem` — Schedule Update 阶段执行

## 9. 如何启用和关闭

启用：在 `GameBootstrap` 或 `GameRuntime` 初始化时调用 `MovementDataKeys.RegisterAll()` 和 `MovementStrategyRegistry.RegisterDefaults()`。

关闭：停止 `MovementSystem` Tick 或移除实体上的 Movement 相关 Data。

## 10. 如何测试

### 最小测试

```bash
cd /home/slime/Code/SkilmeAI/SkilmeAI
Tools/run-tests.sh
```

Runtime tests 覆盖所有 MoveMode 的纯 C# 位移计算。

### Godot 场景测试

```bash
cd /home/slime/Code/SkilmeAI/Games/BrotatoLike
Tools/run-godot-smoke.sh
```

Godot smoke 验证所有 MoveMode 的 `Node2D.Position` 同步。

## 11. 常见错误日志

| 日志/症状 | 根因 |
|-----------|------|
| "Unknown MoveMode: X" | MoveMode enum 值未注册对应 Strategy |
| 实体位置不更新 | GodotMovementDriver 未挂载或未在 `_Process` 中 Tick |
| SineWave/Orbit 方向异常 | 角度单位混淆（应为度，不是弧度） |
| Boomerang 不返回 | `BoomerangReturnSpeedMultiplier` 为 0 或 PauseTime 过长 |
| 移动速度为 0 | `MoveSpeed` Data 未设置或被 Modifier 清零 |

## 12. AI 修改边界

### 可以修改

- 新增 `IMovementStrategy` 实现并注册到 `MovementStrategyRegistry`
- 新增 `MovementDataKeys`（遵循 `static readonly DataMeta` + `DataRegistry.Register`）
- 调整 `MovementParams` 默认值
- 在 GodotBridge 中新增驱动方式

### 禁止修改

- 不改 `Vector2Value` 为 Godot `Vector2`（Runtime 保持纯 C#）
- 不在 `_Process` 中 `new` 对象或 LINQ
- 不直接修改 `Node2D.Position` 绕过 `GodotMovementDriver`
- 不删除现有 Strategy 的默认注册
