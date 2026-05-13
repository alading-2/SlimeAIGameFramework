# AI Capability Contract

## 1. 解决什么问题

提供基于行为树的 AI 决策系统：

- 最近目标查询和追踪
- 行为树节点：Selector / Sequence / Action / Condition
- 预制行为块：攻击优先、追逐、巡逻回退
- 范围内自动发出攻击请求
- Ability 自动索敌上下文准备
- 巡逻和等待倒计时

## 2. 不是本能力职责的内容

- **移动执行** → Movement Capability（AI 只写 `AIMoveDirection` DataKey）
- **攻击执行** → Attack Capability（AI 只发出 `Attack.Requested` 事件）
- **伤害计算** → Damage Capability
- **寻路/网格** → 当前使用简单方向向量，未实现 A*

## 3. 需要的 Runtime 能力

- Entity
- Data
- Event
- Schedule
- Relationship（目标查询）

事件目录：`SlimeAI/GameOS/Capabilities/AI/Events/`（本 Capability 不发布独立事件，直接写入 AI DataKey 并向目标 Entity 发布 `Attack.Requested`；目录可缺省）

## 4. 需要哪些其他 Capability

- Movement（AI 写入移动意图）
- Attack（AI 发出攻击请求）
- Ability（AI 准备自动索敌上下文）

## 5. 写入的 DataKey

| DataKey | 类型 | 说明 |
|---------|------|------|
| AI.TargetEntity | Entity | 当前目标实体 |
| AI.TargetPosition | Vector2Value | 目标位置 |
| AI.HasTargetPosition | bool | 是否有目标位置 |
| AI.IsAttackRequested | bool | 是否请求攻击 |
| AI.PatrolTargetPosition | Vector2Value | 巡逻目标位置 |
| AI.PatrolWaitRemaining | float | 巡逻等待剩余时间 |
| AI.PatrolDirectionSign | int | 巡逻方向 |
| Movement.AIMoveDirection | Vector2Value | 移动意图（写入 Movement） |
| Movement.AIMoveSpeedMultiplier | float | 速度倍率 |

## 6. 读取的 DataKey

| DataKey | 说明 |
|---------|------|
| AI.IsEnabled | AI 是否启用 |
| AI.AttackRange | 攻击范围 |
| AI.PatrolCenter | 巡逻中心 |
| AI.PatrolRadius | 巡逻半径 |
| AI.PatrolWaitTime | 巡逻等待时间 |

## 7. 挂载的 Component

- `AIService` — 全局 AI 服务
- `EnemyBehaviorTreeBuilder` — 敌人行为树构建器
- `GodotAIComponent`（GodotBridge）— 参数导出

## 8. 注册的 System / Strategy / Handler

### BehaviorNode 类型

- `SelectorNode` — 选择第一个成功的子节点
- `SequenceNode` — 顺序执行所有子节点
- `FindNearestTargetAction` — 查找最近目标
- `MoveToTargetAction` — 向目标移动
- `RequestAttackAction` — 请求攻击
- `PatrolAction` — 巡逻
- `PrepareAbilityAutoTargetContextsAction` — 准备 Ability 上下文
- `TickAbilityAutoTriggersAction` — Tick Ability 自动触发
- `IsTargetInRangeCondition` — 目标是否在范围内

## 9. 如何启用和关闭

启用：在 Entity 上写入 AI DataKey，调用 `AIService.Register(entity, behaviorTree)`。

关闭：设置 `AI.IsEnabled = false` 或取消注册。

## 10. 如何测试

```bash
cd /home/slime/Code/SlimeAI/SlimeAI
Tools/run-tests.sh
```

覆盖最近目标查询、巡逻、攻击请求、Ability 自动触发。

## 11. 常见错误日志

| 日志/症状 | 根因 |
|-----------|------|
| AI 不移动 | `AI.IsEnabled` 为 false 或 `AIMoveDirection` 未写入 |
| AI 不攻击 | 目标不在 `AttackRange` 内或攻击冷却中 |
| 巡逻范围异常 | `PatrolRadius` 为 0 或负值 |
| 目标闪烁切换 | 多个目标距离相近，每帧切换 |

## 12. AI 修改边界

### 可以修改

- 新增 `BehaviorNode` 子类
- 调整 `EnemyBehaviorTreeBuilder` 的行为块组合
- 新增 AI DataKey
- 修改目标查询过滤逻辑

### 禁止修改

- 不直接调用 `MovementSystem` 或 `AttackService`
- 行为树节点必须保持纯函数式（不写外部状态）
- 不在 `_Process` 中分配对象
