# Feature Capability Contract

## 1. 解决什么问题

提供通用能力生命周期框架：

- FeatureDefinition 注册和启用
- Modifier 授予/回滚（Additive/Multiplicative/Override）
- Handler 生命周期管理（Granted / Removed / Enabled / Disabled / Activated / Executed / Ended）
- `IFeatureAction` 原子动作列表和 `FeatureService.ExecuteActions`
- Periodic / OnEvent 自动触发注册
- 激活计数
- Ability 触发后的具体执行委托

Feature 是"做什么"，Ability 是"何时做"。

## 2. 不是本能力职责的内容

- **技能何时触发** → Ability Capability
- **Feature 自身 Periodic / OnEvent 自动触发注册** → `FeatureAutoTriggerService`
- **伤害计算细节** → Damage Capability
- **移动执行** → Movement Capability
- **数据持久化** → DataOS / Authoring

## 3. 需要的 Runtime 能力

- Entity
- Data
- Event
- Schedule

事件目录：`SlimeAI/GameOS/Capabilities/Feature/Events/`

## 4. 需要哪些其他 Capability

无。Feature 是基础框架 Capability，被 Ability 依赖。

## 5. 写入的 DataKey

| DataKey | 类型 | 说明 |
|---------|------|------|
| Feature.IsActive | bool | 是否激活中 |
| Feature.ActivationCount | int | 激活次数 |
| Feature.TriggerMode | FeatureTriggerMode | Feature 自身触发模式 |
| Feature.Cooldown | float | Periodic 触发间隔 / 冷却 |
| Feature.TriggerEventType | string | OnEvent 触发事件 stable key，第一版由调用方显式注册泛型事件 |
| Feature.TriggerChance | float | OnEvent 触发概率，0-100 |

Modifier 通过 Data.Modifier API 写入目标 DataKey。

## 6. 读取的 DataKey

| DataKey | 说明 |
|---------|------|
| Feature.FeatureId | 能力 ID |
| Feature.HandlerId | 处理器 ID |
| Feature.IsEnabled | 是否启用 |
| Feature.TriggerMode | Feature 自身触发模式 |
| Feature.Cooldown | Periodic 触发间隔 |
| Feature.TriggerEventType | OnEvent 事件 stable key |
| Feature.TriggerChance | OnEvent 触发概率 |
| Feature.ModifierTargetKey | Modifier 目标 DataKey |
| Feature.ModifierType | Modifier 类型 |
| Feature.ModifierValue | Modifier 数值 |

## 7. 运行时入口 / Adapter

- `FeatureService` — Feature 生命周期服务；`Default` 是进程级默认入口，测试和局部运行域优先显式构造。
- `FeatureHandlerRegistry` — Handler 注册表
- `IFeatureHandler` — Handler 接口
- `IFeatureAction` — 纯逻辑原子动作接口
- `FeatureAutoTriggerService` — Feature Periodic / OnEvent 自动触发注册服务，注册返回 `IDisposable`

## 8. Runtime Process / Strategy / Handler

- `FeatureService` — service-driven lifecycle，不隐式挂入 RuntimeSchedule。
- `FeatureService.ExecuteActions` — 批量执行 `IFeatureAction`，`Grant` 时会执行 `FeatureDefinition.Actions`。
- `FeatureHandlerRegistry` — Handler 注册和查找。
- `FeatureAutoTriggerService.RegisterPeriodic` — 使用调用方提供的 `TimerManager` 按 `Feature.Cooldown` 激活，Dispose 后停止。
- `FeatureAutoTriggerService.RegisterOnEvent<TEvent>` — 订阅调用方提供的 `IEventBus`，通过 `Feature.TriggerChance` gating，Dispose 后退订。

游戏侧 handler 可以作为验证证据存在于 `Games/BrotatoLike`，但不属于框架默认契约。

## 9. 如何启用和关闭

启用：调用 `FeatureDataKeys.RegisterAll()`，注册 Handler，通过 `FeatureService.Grant()` 授予并按需要调用 `Activate()` 或注册 `FeatureAutoTriggerService`。

关闭：调用 `FeatureService.Disable()`、`FeatureService.End()`，或 Dispose 自动触发注册句柄。

## 10. 如何测试

```bash
cd /home/slime/Code/SlimeAI/SlimeAI
Tools/run-tests.sh
```

覆盖 Modifier 授予/回滚、handler 生命周期、ActivationCount、Feature action 执行、Periodic / OnEvent 自动触发和 typed Ability-to-Feature result flow。

## 11. 常见错误日志

| 日志/症状 | 根因 |
|-----------|------|
| "Handler not found: X" | `FeatureHandlerId` 未注册到 `FeatureHandlerRegistry` |
| Modifier 不生效 | `ModifierTargetKey` 错误或目标 DataKey 不支持 Modifier |
| Feature 重复激活 | 未检查 `IsActive` 状态 |

## 12. AI 修改边界

### 可以修改

- 新增 `IFeatureHandler` 实现
- 新增 FeatureDefinition
- 调整 Modifier 类型和值

### 禁止修改

- 新 framework handler 必须使用 `FeatureContext.ActivationPayload` / `TryGetActivation<T>()` 和 `ExecutionResult` / `TryGetExecutionResult<T>()`。
- `ActivationData`、`ExecuteResult`、`SourceEventData`、`ExtraData` 仅为 `[Obsolete]` 兼容迁移入口，不作为新契约。
- Modifier 回滚必须和授予成对出现
- 不直接修改被 Modifier 影响的 DataKey（通过 Modifier API）

## 13. Runtime CommandBuffer / Guard

- Feature handler 可能由 AbilityService 或 event handler 间接触发；若处于 guard 内，handler 中的结构变更自动进入 Runtime CommandBuffer。
- Handler 生成 Projectile / Effect 后可以继续写返回 entity handle 的 Data；不要在同一 guard 内通过 registry 立即 `Get` 新实体。
- 本 Capability 不提供独立 deferred queue，也不提供 `EnqueueAndWait`；确需同步等待时必须开独立 OpenSpec change。
