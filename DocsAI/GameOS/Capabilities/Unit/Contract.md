# Unit Capability Contract

## 1. 解决什么问题

提供游戏单位的通用数据定义：

- 单位名称、类型（Player/Enemy/Neutral）、死亡类型
- 视觉场景路径（`VisualScenePath`）
- 血条高度和显示开关
- 拾取范围、经验奖励、AI 检测范围
- 可用动画列表

Unit 是"单位长什么样、有什么基础属性"，不是"单位能做什么"。

## 2. 不是本能力职责的内容

- **移动** → Movement Capability
- **生命值** → Damage Capability
- **攻击** → Attack Capability
- **AI 决策** → AI Capability
- **生成规则** → SpawnSystem（游戏侧）

## 3. 需要的 Runtime 能力

- Entity
- Data

事件目录：无（Unit 是纯数据 Capability，不发布事件）

## 4. 需要哪些其他 Capability

- Damage（生命值相关）
- Movement（移动速度相关）

## 5. 写入的 DataKey

Unit DataKey 主要是配置型，在 DataOS seed 中定义，Runtime 读取。

| DataKey | 类型 | 说明 |
|---------|------|------|
| Unit.Name | string | 显示名称 |
| Unit.EntityType | string | 实体类型 |
| Unit.DeathType | string | 死亡类型 |
| Unit.VisualScenePath | string | 视觉场景路径 |
| Unit.HealthBarHeight | float | 血条高度 |
| Unit.IsShowHealthBar | bool | 是否显示血条 |
| Unit.PickupRange | float | 拾取范围 |
| Unit.ExpReward | int | 经验奖励 |
| Unit.DetectionRange | float | AI 检测范围 |
| Unit.AvailableAnimations | string | 可用动画列表 |

## 6. 读取的 DataKey

同写入。Unit DataKey 主要被其他 Capability 和 GodotBridge 读取。

## 7. 挂载的 Component

无独立系统。Unit DataKey 在 `BrotatoLikeDataOSBootstrap` 中从 snapshot 写入 Entity Data。

## 8. 注册的 System / Strategy / Handler

无。

## 9. 如何启用和关闭

启用：调用 `UnitDataKeys.RegisterAll()`，在 DataOS seed 中定义单位数据。

关闭：无需显式关闭。

## 10. 如何测试

```bash
cd /home/slime/Code/SlimeAI/SlimeAI
dotnet build
```

Unit DataKey 的验证通过 build 和 DataOS snapshot 生成完成。

## 11. 常见错误日志

| 日志/症状 | 根因 |
|-----------|------|
| 视觉场景加载失败 | `VisualScenePath` 错误或资源不存在 |
| 血条不显示 | `IsShowHealthBar` 为 false 或 `HealthBarHeight` 为 0 |
| 单位名称不正确 | DataOS seed 中 `Unit.Name` 错误 |

## 12. AI 修改边界

### 可以修改

- 新增 Unit DataKey
- 调整单位配置

### 禁止修改

- 不删除现有 DataKey（其他 Capability 可能依赖）
- `EntityType` 和 `DeathType` 的枚举值需同步更新使用处
