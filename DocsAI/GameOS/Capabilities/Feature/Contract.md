# Feature Capability Contract

## 1. 解决什么问题

提供通用能力生命周期框架：

- FeatureDefinition 注册和启用
- Modifier 授予/回滚（Additive/Multiplicative/Override）
- Handler 生命周期管理（OnActivate/OnDeactivate/OnTick）
- 激活计数
- Ability 触发后的具体执行委托

Feature 是"做什么"，Ability 是"何时做"。

## 2. 不是本能力职责的内容

- **何时触发技能** → Ability Capability
- **伤害计算细节** → Damage Capability
- **移动执行** → Movement Capability
- **数据持久化** → DataOS / Authoring

## 3. 需要的 Runtime 能力

- Entity
- Data
- Event
- Schedule

事件目录：`SkilmeAI/GameOS/Capabilities/Feature/Events/`

## 4. 需要哪些其他 Capability

无。Feature 是基础框架 Capability，被 Ability 依赖。

## 5. 写入的 DataKey

| DataKey | 类型 | 说明 |
|---------|------|------|
| Feature.IsActive | bool | 是否激活中 |
| Feature.ActivationCount | int | 激活次数 |

Modifier 通过 Data.Modifier API 写入目标 DataKey。

## 6. 读取的 DataKey

| DataKey | 说明 |
|---------|------|
| Feature.FeatureId | 能力 ID |
| Feature.HandlerId | 处理器 ID |
| Feature.IsEnabled | 是否启用 |
| Feature.ModifierTargetKey | Modifier 目标 DataKey |
| Feature.ModifierType | Modifier 类型 |
| Feature.ModifierValue | Modifier 数值 |

## 7. 挂载的 Component

- `FeatureService` — 全局 Feature 服务
- `FeatureHandlerRegistry` — Handler 注册表
- `IFeatureHandler` — Handler 接口

## 8. 注册的 System / Strategy / Handler

- `FeatureService` — Schedule Update 阶段 Tick
- `FeatureHandlerRegistry` — Handler 注册和查找

游戏侧 Handler 实现（示例）：
- `技能.投射物.正弦波射击`
- `技能.主动.猛击`
- `技能.被动.环绕技能`
- 等等（见 `BrotatoLikeAbilityHandlers`）

## 9. 如何启用和关闭

启用：调用 `FeatureDataKeys.RegisterAll()`，注册 Handler，通过 `FeatureService.Activate()` 激活。

关闭：调用 `FeatureService.Deactivate()` 或设置 `IsEnabled = false`。

## 10. 如何测试

```bash
cd /home/slime/Code/SkilmeAI/SkilmeAI
Tools/run-tests.sh
```

覆盖 Modifier 授予/回滚、handler 生命周期、ActivationCount。

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

- 不改 `IFeatureHandler` 接口签名
- Modifier 回滚必须和授予成对出现
- 不直接修改被 Modifier 影响的 DataKey（通过 Modifier API）
