# Ability Capability Contract

## 1. 解决什么问题

提供技能的完整生命周期管理：

- 触发模式：Manual / Periodic / Permanent / None
- 目标选择：None / Entity / Point / Direction
- 自动索敌：基于范围、同队过滤、死亡过滤
- 冷却和充能管理
- 委托给 FeatureHandler 执行具体逻辑

Ability 是"何时触发 + 选什么目标"，Feature 是"触发后做什么"。

## 2. 不是本能力职责的内容

- **具体技能效果** → Feature Capability（handler 实现）
- **伤害计算** → Damage Capability
- **投射物生成** → Projectile Capability
- **特效播放** → Effect Capability
- **移动执行** → Movement Capability
- **UI 显示** → 游戏侧 UI 组件

## 3. 需要的 Runtime 能力

- Entity
- Data
- Event
- Schedule

事件目录：`SlimeAI/GameOS/Capabilities/Ability/Events/`

## 4. 需要哪些其他 Capability

- Damage（死亡判定、伤害门禁）
- Feature（handler 委托执行）

## 5. 写入的 DataKey

| DataKey | 类型 | 说明 |
|---------|------|------|
| Ability.IsActive | bool | 是否执行中 |
| Ability.CooldownRemaining | float | 剩余冷却 |
| Ability.CurrentCharges | int | 当前充能 |

## 6. 读取的 DataKey

完整列表见 `AbilityDataKeys.cs`。核心读取项：

| DataKey | 说明 |
|---------|------|
| Ability.TriggerMode | 触发模式 |
| Ability.TargetSelection | 目标选择 |
| Ability.Cooldown | 冷却时间 |
| Ability.CastRange | 施法距离 |
| Ability.FeatureHandlerId | 委托的 handler |
| Ability.IsEnabled | 是否启用 |
| Ability.Damage | 技能伤害 |

## 7. 挂载的 Component

- `AbilityService` — 全局技能服务
- `AbilityTargetingTool` — 自动索敌工具
- `GodotAbilityComponent`（GodotBridge）— UI 参数导出

## 8. 注册的 System / Strategy / Handler

- `AbilityService` — Schedule Update 阶段 Tick
- `AbilityTargetingTool` — 目标查询和筛选

## 9. 如何启用和关闭

启用：调用 `AbilityDataKeys.RegisterAll()`，在 Entity 上写入 Ability 相关 Data。

关闭：设置 `Ability.IsEnabled = false` 或移除 Ability Data。

## 10. 如何测试

```bash
cd /home/slime/Code/SlimeAI/SlimeAI
Tools/run-tests.sh
```

覆盖点选目标、Periodic 自动触发、自动索敌过滤。

## 11. 常见错误日志

| 日志/症状 | 根因 |
|-----------|------|
| "FailCooldown" | 冷却中 |
| "FailNoTarget" | 目标选择要求未满足 |
| "FailCasterDead" | 施法者死亡 |
| 自动索敌不工作 | `AutoTargetRange` 为 -1 或范围内无有效目标 |
| Permanent 技能不触发 | Permanent 在 Feature 层处理，Ability 只负责启用 |

## 12. AI 修改边界

### 可以修改

- 新增 `AbilityTriggerMode`
- 新增 `AbilityTargetSelection`
- 调整 `AbilityTargetingTool` 过滤逻辑
- 新增 Ability 相关 DataKey

### 禁止修改

- 不绕过 `AbilityService.TryTrigger()` 直接执行 handler
- 不改触发模式语义
- 冷却/充能逻辑保持统一
