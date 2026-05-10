# Attack Capability Contract

## 1. 解决什么问题

提供攻击请求与执行的完整流程：

- 攻击请求事件处理
- 前摇（WindUp）/ 后摇（Recovery）计时
- 攻击冷却管理
- 距离和死亡门禁
- 通过 DamageService 结算伤害
- 攻击状态机（Idle/WindUp/Active/Recovery/Cooldown）

## 2. 不是本能力职责的内容

- **伤害计算细节** → Damage Capability
- **移动/追击** → Movement / AI Capability
- **动画播放** → GodotBridge / 游戏侧
- **目标选择** → Ability / AI Capability

## 3. 需要的 Runtime 能力

- Entity
- Data
- Event
- Schedule
- Timer

## 4. 需要哪些其他 Capability

- Damage（伤害结算）

## 5. 写入的 DataKey

| DataKey | 类型 | 说明 |
|---------|------|------|
| Attack.IsAttacking | bool | 是否攻击中 |
| Attack.State | AttackState | 攻击状态 |
| Attack.CooldownRemaining | float | 剩余冷却 |

## 6. 读取的 DataKey

| DataKey | 说明 |
|---------|------|
| Attack.Damage | 攻击伤害 |
| Attack.Range | 攻击范围 |
| Attack.Interval | 攻击间隔 |
| Attack.WindUpTime | 前摇时间 |
| Attack.RecoveryTime | 后摇时间 |
| Attack.CanAttack | 是否可攻击 |

## 7. 发布的事件

- `Attack.Started` — 攻击开始（前摇结束）
- `Attack.Completed` — 攻击完成
- `Attack.Cancelled` — 攻击取消

## 8. 订阅的事件

- `Attack.Requested` — 攻击请求
- `Attack.CancelRequested` — 取消请求

## 9. 挂载的 Component

- `AttackService` — 全局攻击服务
- `GodotAttackComponent`（GodotBridge）— 参数导出和动画事件转发

## 10. 注册的 System / Strategy / Handler

- `AttackService` — Schedule Update 阶段 Tick

## 11. 如何启用和关闭

启用：调用 `AttackDataKeys.RegisterAll()`，订阅 `Attack.Requested` 事件。

关闭：设置 `Attack.CanAttack = false` 或取消事件订阅。

## 12. 如何测试

```bash
cd /home/slime/Code/SkilmeAI/SkilmeAI
Tools/run-tests.sh
```

覆盖攻击请求、前摇/后摇、冷却、距离门禁、伤害结算。

## 13. 常见错误日志

| 日志/症状 | 根因 |
|-----------|------|
| "FailNoTarget" | 无目标或目标死亡 |
| "FailSelfDead" | 攻击者死亡 |
| 攻击无伤害 | `Attack.Damage` 为 0 或 DamageService 未初始化 |
| 攻击状态卡住 | Timer 未正确启动或取消 |

## 14. AI 修改边界

### 可以修改

- 新增攻击状态
- 调整前摇/后摇逻辑
- 新增攻击触发条件

### 禁止修改

- 不绕过 `AttackService` 直接调用 `DamageService`
- 攻击状态转换必须完整（WindUp -> Active -> Recovery -> Cooldown）
