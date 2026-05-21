# Godot 陷阱知识库

> 面向 AI：记录 SlimeAI 框架下写 Godot C# 代码反复踩过的陷阱和正确模式。
> 不是 Godot 入门教程，不重复官方 API 文档。每个条目必须具备：问题表现 / 错误模式 / 正确模式。
> 更新日期：2026-05-20

## AI 路由

AI 修改任何涉及 Godot 表现层（UI、Camera、Input、Animation、Collision、Node 生命周期）的代码时，先对照本文件逐项检查。

触发词：`Camera2D` `CanvasLayer` `GlobalPosition` `Control` `AnimatedSprite2D` `Area2D` `SetMeta` `QueueFree` `Reparent` `PackedScene` `AnimationPlayer` `Input.IsAction`

---

## 1. 坐标系陷阱

### 1.1 Control 在 CanvasLayer 内的位置

**陷阱**：CanvasLayer 内 Control 的 `GlobalPosition` 不等于世界坐标。用敌人在世界空间的 `GlobalPosition` 直接赋给 Control，血条会偏移。

**根因**：
```
世界坐标系：Node2D.GlobalPosition → 敌人在游戏世界中的位置
画布坐标系：CanvasLayer 内 Control.Position → 父容器内的相对位置
屏幕坐标系：get_viewport().get_mouse_position() → 像素位置
```
CanvasLayer（`FollowViewportEnabled = true`）的 `Layer` 越高，越远离世界坐标系的底层画布变换。

**错误模式**：
```csharp
// HealthBarUI (Control 子类) 中
GlobalPosition = enemy.GlobalPosition + offset; // 坐标系不一致
```

**正确模式**：
```csharp
// Control 在 CanvasLayer 内时，用 Position（父容器内相对坐标）
// CanvasLayer 的 FollowViewportEnabled 会处理好画布变换
Position = worldPosition + offset; // worldPosition 来自调用方传入
```

**调用方注意**：传入的世界位置已经从 `Node2D.GlobalPosition` 获取，`FollowViewportEnabled = true` 的 CanvasLayer 内 Control 可直接使用该值。

### 1.2 Node2D 的 GlobalPosition 正常使用场景

**正确用法**（TargetingIndicator 等 Node2D 子类）：
```csharp
// TargetingIndicatorUI 是 Node2D（不是 Control），用 GlobalPosition 完全正确
GlobalPosition = worldPosition;
```
Node2D 在世界空间中，`GlobalPosition` 直接对应世界坐标，不存在坐标系不一致问题。

### 1.3 判断规则

| 节点类型 | 父容器 | 设置位置用 | 原因 |
|----------|--------|-----------|------|
| Node2D | 世界空间 | `GlobalPosition` | 世界坐标系，GlobalPosition 即世界坐标 |
| Control | CanvasLayer 内 | `Position` | CanvasLayer 已处理画布变换，Position 是层内坐标 |
| Control | 普通 Control 内 | `Position` | 相对于父 Control |

---

## 2. Camera2D 陷阱

### 2.1 Camera2D 必须显式启用

**陷阱**：把 Camera2D 加入场景树后不会自动激活。Godot 场景可以同时存在多个 Camera2D，只有 `Enabled = true`（即 `Current = true`）的那个才实际渲染。

**错误模式**：
```csharp
// 场景文件里加了节点，但没设 Enabled
[node name="Camera2D" type="Camera2D" parent="."]
```

**正确模式**：
```csharp
var camera = new Camera2D
{
    Name = "PlayerCamera",
    Enabled = true  // 必须显式设置，不加就是静态视角
};
```

### 2.2 镜头跟随玩家

**陷阱**：Camera2D 不跟随玩家时，玩家会走出屏幕。

**正确模式**：把 Camera2D 设为玩家实体的子节点。玩家 Node2D 移动，Camera2D 自动跟随。
```csharp
camera.Reparent(playerEntity, false);
camera.Enabled = true;
```

### 2.3 平滑跟随 API（Godot 4.6）

**陷阱**：Godot 4 的 Camera2D 平滑跟随参数是顶层属性，不是嵌套类。用 `new Camera2D.PositionSmoothingParams { Enabled = true }` 会编译失败。

**正确模式**：
```csharp
var camera = new Camera2D
{
    PositionSmoothingEnabled = true,
    PositionSmoothingSpeed = 5f   // 越大越快，5f 是常用值
};
```

---

## 3. Node 生命周期陷阱

### 3.1 访问已销毁节点的 SceneFilePath / 属性

**陷阱**：`entity.DestroyEntity()` 会触发 `QueueFree()`，之后访问 `node.SceneFilePath` 或 `node.GetMeta()` 等属性时抛 `ObjectDisposedException`。

**正确模式**：在销毁之前收集需要的值，或用 `GodotObject.IsInstanceValid()` 防护。
```csharp
// 先收集数据，再销毁
var scenePath = node.SceneFilePath;
var meta = node.GetMeta("Level");
entity.DestroyEntity();
// 然后使用 scenePath 和 meta

// 或者防护性检查
if (GodotObject.IsInstanceValid(node))
{
    var path = node.SceneFilePath; // 安全
}
```

### 3.2 遍历时删除子节点

**陷阱**：在 `for` 循环中 `QueueFree()` 后继续访问子节点属性。

**正确模式**：
```csharp
// 先收集待删除节点，再批量删除
var toRemove = new List<Node>();
for (int i = 0; i < children.Count; i++)
{
    if (ShouldRemove(children[i]))
        toRemove.Add(children[i]);
}
foreach (var node in toRemove)
    node.QueueFree();

// 或者倒序遍历 + IsQueuedForDeletion 跳过
for (int i = children.Count - 1; i >= 0; i--)
{
    if (children[i].IsQueuedForDeletion())
        continue;
    // 安全操作
}
```

### 3.3 Reparent 保留全局变换

**陷阱**：`Reparent(newParent)` 默认保持局部变换，节点在世界空间中的位置会跳变。

**正确模式**：第二个参数 `keepGlobalTransform = false` 保持局部变换不变（即节点在父容器中的相对位置不变），通常用于 UI 重组；如果需要保持世界位置，传 `true`。
```csharp
camera.Reparent(playerEntity, false); // 保持局部坐标不变（camera 原本就在原点，效果相同）
```

---

## 4. 输入陷阱

### 4.1 死亡后仍在响应输入

**陷阱**：输入组件只检查业务门控（`CanMoveInput`），不检查生命状态（`IsDead`）。玩家死亡后仍可移动和释放技能。

**错误模式**：
```csharp
public void TickInput()
{
    if (!entity.Data.Get<bool>(MovementDataKeys.CanMoveInput, true))
        return; // 只检查 CanMoveInput，不防死亡
    // 读取输入...
}
```

**正确模式**：双门控——`CanMoveInput`（业务开关）+ `IsDead`（生命状态）。
```csharp
public void TickInput()
{
    if (!entity.Data.Get<bool>(MovementDataKeys.CanMoveInput, true))
        return;
    if (entity.Data.Get<bool>(DamageDataKeys.IsDead, false))
        return; // 死亡后禁止一切输入
    // 读取输入...
}
```

### 4.2 运行时死亡后必须同步设 CanMoveInput

**陷阱**：只检查 `IsDead` 不够——`InputDrivenMovement` 只看 `CanMoveInput`。死亡时两个都要设。

**正确模式**：
```csharp
if (isDead)
{
    entity.Data.Set(MovementDataKeys.CanMoveInput, false); // 停止 Movement 处理
    // TickInput 中再设 IsDead 门控作为第二层
}
```

---

## 5. 场景与代码创建

### 5.1 SceneFilePath 判空

**陷阱**：`new Control()` / `new Label()` 创建的节点 `SceneFilePath` 为空字符串；`PackedScene.Instantiate()` 创建的节点 `SceneFilePath` 非空。误判会导致验证证据失效。

**正确模式**：
```csharp
bool IsSceneBacked(Node node) =>
    node != null
    && GodotObject.IsInstanceValid(node)  // 先检查未销毁
    && !string.IsNullOrEmpty(node.SceneFilePath);
```

### 5.2 CanvasLayer 的 Layer 层级

**陷阱**：多个 CanvasLayer 时，Layer 值越大越在上层。Layer=20 的 HUD 会遮挡 Layer=10 的 UI。设 Layer 时避开 Godot 内置层级（-1 到 128）。

**常用值**：HUD=20, PauseMenu=100, Debug=128。

---

## 6. Animation / AnimatedSprite2D 陷阱

### 6.1 动画名必须精确匹配

**陷阱**：`AnimatedSprite2D.Play("idle")` 的动画名必须与 `SpriteFrames` 中配置的动画名完全一致（区分大小写）。Name 不匹配不会抛异常，只是不播放。

**正确模式**：先在 `SpriteFrames` 资源中确认可用动画列表，再从列表中选取。
```csharp
var availableAnimations = sprite.SpriteFrames.GetAnimationNames();
if (availableAnimations.Contains("attack_01"))
    sprite.Play("attack_01");
else if (availableAnimations.Length > 0)
    sprite.Play(availableAnimations[0]); // 回退到第一个可用动画
```

### 6.2 一次性动画完成检测

**陷阱**：`AnimatedSprite2D` 非循环动画播完后不会自动通知。需要在 `_Process` 中轮询 `Frame >= SpriteFrames.GetFrameCount(anim) - 1` 或连接 `animation_finished` 信号。

**正确模式**：
```csharp
// 优先用信号
sprite.AnimationFinished += OnAnimationFinished;

// 或轮询 + 帧检测
if (!sprite.IsPlaying() || sprite.Animation != expectedAnim)
    return; // 还没开始或已切换
if (sprite.Frame >= sprite.SpriteFrames.GetFrameCount(expectedAnim) - 1)
    OnAnimationComplete();
```

---

## 7. Area2D 碰撞信号

### 7.1 C# 信号必须手动连接

**陷阱**：在 `.tscn` 编辑器中连接 Area2D 的 `body_entered` 信号到 C# 脚本，运行时可能不触发。Godot 的 C# 信号连接和 GDScript 不完全等价。

**正确模式**：在 `_Ready()` 中手动连接。
```csharp
public override void _Ready()
{
    var area = GetNode<Area2D>("Area2D");
    area.BodyEntered += OnBodyEntered;
    area.AreaEntered += OnAreaEntered;
}
```

### 7.2 CollisionObject2D 从场景树移除后碰撞失效

**陷阱**：对象池中节点脱树后，`CollisionObject2D` 不再参与物理。重新挂树时需要恢复 layer/mask 和碰撞形状。

**正确模式**：出池后显式恢复（框架 `GodotNodePool` 已内置）：先挂回 ActiveParent，再 `Activate()` 恢复碰撞和可见性。

---

## 8. 常见 Godot 4 API 差异

### 8.1 Camera2D 平滑

| Godot 3 | Godot 4 |
|---------|---------|
| `smoothing_enabled = true` | `PositionSmoothingEnabled = true` |
| `smoothing_speed = 5` | `PositionSmoothingSpeed = 5f` |
| `drag_margin_*` | 用 `LimitSmoothed` + `AnchorMode` 替代 |

### 8.2 AnimatedSprite2D 名称变更

| Godot 3 | Godot 4 |
|---------|---------|
| `AnimatedSprite` | `AnimatedSprite2D` |
| `playing` | 无直接属性，用 `IsPlaying()` 方法 |
| `animation` | `Animation`（属性名首字母大写） |

### 8.3 Control 布局模式

Godot 4 新增 `layout_mode`：`0` = 位置/大小手动（offset_*），`1` = 锚点自动，`2` = 容器自动。scene 文件中 `layout_mode = 3` 是 `anchors` 模式。代码创建的 Control 默认手动模式，用 `Position` 和 `Size` 直接控制。

---

## 9. 验证相关陷阱

### 9.1 Control 节点的 SceneFilePath

**陷阱**：`new Control()` / `new Label()` 的 `SceneFilePath` 是空字符串 `""`，但 `string.IsNullOrEmpty("")` 返回 `true`。静态扫描脚本用 `grep` 匹配 `new (Label|Control|...)` 来检测代码创建的 UI。

**豁免注释格式**：代码创建但属于豁免的节点必须在上方一行写注释。
```csharp
// scene-first exception: 简单容器层，非复合 UI
var layer = new Control { Name = "HeadHealthBarLayer" };
```

### 9.2 SetMeta / GetMeta 仅用于调试

**陷阱**：`SetMeta` / `GetMeta` 是 Godot 提供的临时元数据机制，不参与序列化。不要在 meta 中存储核心游戏状态——用 `DataKey<T>`（Entity.Data）存储。

**允许使用 meta 的场景**：调试信息、验证证据、临时标记。

---

## 索引

| 陷阱编号 | 类别 | 典型症状 |
|----------|------|---------|
| 1.1 | 坐标系 | 血条漂移、UI 不跟随实体 |
| 1.2-1.3 | 坐标系 | 判断用 Position 还是 GlobalPosition |
| 2.1 | Camera2D | 镜头不激活、黑屏或静止 |
| 2.2 | Camera2D | 玩家走出屏幕 |
| 2.3 | Camera2D | 编译错误：PositionSmoothing 类型不存在 |
| 3.1 | 生命周期 | ObjectDisposedException |
| 3.2 | 生命周期 | 遍历删除时崩溃 |
| 3.3 | 生命周期 | Reparent 后位置跳变 |
| 4.1 | 输入 | 死亡后可移动 |
| 4.2 | 输入 | Movement 在死亡后继续推进 |
| 5.1 | 场景 | SceneFilePath 空字符串 vs null |
| 5.2 | 场景 | CanvasLayer 层级遮挡 |
| 6.1 | 动画 | Play() 无效果 |
| 6.2 | 动画 | 不知道动画何时播完 |
| 7.1 | 碰撞 | Area2D 信号不触发 |
| 7.2 | 碰撞 | 出池后碰撞不工作 |
| 8.1-8.3 | API 差异 | 编译错误：API 不存在 |
| 9.1-9.2 | 验证 | 静态扫描误报 / meta 滥用 |
