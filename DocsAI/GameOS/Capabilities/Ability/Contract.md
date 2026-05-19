# Ability Capability Contract

## 1. 解决什么问题

提供技能的完整生命周期管理：

- 触发模式：Manual / Periodic / Permanent / None
- 目标选择：None / Entity / Point / EntityOrPoint
- 自动索敌：基于范围、同队过滤、死亡过滤
- 冷却和充能管理
- 通过 typed `AbilityCastContext` 委托给 Feature handler 执行具体逻辑

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

- Damage（死亡判定、伤害门禁；`AbilityService` 可注入 `DamageService`）
- Feature（handler 委托执行；`AbilityService` 可注入 `FeatureService`）
- Timer（冷却和周期伤害；`AbilityService` 构造必须显式传入 `TimerManager`）

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

## 7. 运行时入口 / Adapter

- `AbilityService` — 技能服务；构造注入 `TimerManager`，并可注入 `FeatureService` / `DamageService`；`Default` 是进程级默认入口。
- `AbilityTargetingTool` — 自动索敌工具
- `IAbilityTargetQuery` / `RuntimeAbilityTargetQuery` — Ability-owned selector。
- 暂无框架通用 `GodotAbilityComponent`；BrotatoLike 输入和 UI 属于游戏侧 adapter。

## 8. Runtime Process / Strategy / Handler

- `AbilityService` — service-driven trigger / cooldown / charge / periodic tick，不隐式挂入 RuntimeSchedule。
- `AbilityTargetingTool` — 目标查询和筛选
- `AbilityService` 调用 Feature handler 时把 `AbilityCastContext` 放入 `FeatureContext.ActivationPayload`，并通过 typed `AbilityExecutedResult` 读取结果；不再依赖 raw object bag。

### Target Query 接口

- `IAbilityTargetQuery` — Ability 目标候选查询接口（Capability-owned selector）
- `RuntimeAbilityTargetQuery` — 默认实现，返回 EntityManager 快照（纯 Runtime 全量扫描回退）
- Godot 项目可注入 physics-aware 实现替换

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

## 13. Runtime CommandBuffer / Guard

- Ability event handler 或 GodotBridge component callback 内触发技能时，handler 中的 `Spawn / Destroy / Attach / Detach` 会跟随 Runtime CommandBuffer guard 语义。
- Ability handler 不应在 guarded spawn 后立刻通过 `EntityManager.Get` 要求 registry 可见；使用返回的 entity handle 写 Data，等待 `RunPhase` playback 后再查询 registry。
- `QueuedEvent` deferred replay 只支持 framework-known `IGlobalEvent`，不支持游戏侧输入或技能动态事件重放。
