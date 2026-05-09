# Damage Capability Contract

## 1. 解决什么问题

提供完整的伤害计算管线，从原始伤害输入到最终生命值变更的全流程处理：

- 多级伤害处理器（暴击、闪避、护甲、吸血、护盾、承伤倍率）
- 治疗系统
- 伤害统计（累计/波次/最高单次）
- 接触伤害（ContactDamage）
- 实体死亡判定

所有伤害通过 `DamageService.Instance.Process()` 统一入口，保证处理顺序和可扩展性。

## 2. 不是本能力职责的内容

- **移动/位移** → Movement Capability
- **碰撞检测** → Collision Capability（ContactDamage 使用 Collision 事件，但伤害计算在 Damage）
- **技能触发逻辑** → Ability Capability
- **视觉表现** → Effect / GodotBridge
- **AI 决策** → AI Capability

## 3. 需要的 Runtime 能力

- Entity
- Data
- Event

## 4. 需要哪些其他 Capability

无。Damage 是基础 Capability，被 Attack/Ability/Projectile 依赖。

## 5. 写入的 DataKey

| DataKey | 类型 | 说明 |
|---------|------|------|
| Damage.CurrentHp | float | 当前生命值 |
| Damage.IsDead | bool | 是否死亡 |
| Damage.TotalDamageDealt | float | 累计造成伤害 |
| Damage.TotalDamageTaken | float | 累计承受伤害 |
| Damage.TotalKills | int | 累计击杀 |
| Damage.TotalHits | int | 累计命中 |
| ... | ... | 完整列表见 DamageDataKeys |

## 6. 读取的 DataKey

| DataKey | 类型 | 说明 |
|---------|------|------|
| Damage.MaxHp | float | 最大生命值（支持 Modifier） |
| Damage.Armor | float | 护甲（支持 Modifier） |
| Damage.DodgeChance | float | 闪避率 0-100（支持 Modifier） |
| Damage.CritRate | float | 暴击率 0-100（支持 Modifier） |
| Damage.CritDamage | float | 暴击伤害百分比（支持 Modifier） |
| Damage.DamageTakenMultiplier | float | 承伤倍率（支持 Modifier） |
| Damage.LifeSteal | float | 吸血率 0-100（支持 Modifier） |
| Damage.Shield | float | 护盾值（支持 Modifier） |
| Damage.IsInvulnerable | bool | 是否无敌 |

## 7. 发布的事件

- `Damage.Applied` — 伤害结算完成
- `Damage.Death` — 实体死亡
- `Damage.HealApplied` — 治疗完成

## 8. 订阅的事件

无直接订阅。通过 `DamageService.Process()` 入口被动调用。

## 9. 挂载的 Component

- `DamageService` — 全局伤害处理服务
- `DamageTool` — 便捷伤害调用工具
- `HealService` — 治疗服务
- `GodotContactDamageComponent`（GodotBridge）— 接触伤害触发器

## 10. 注册的 System / Strategy / Handler

### DamageProcessor（按优先级排序）

1. `DodgeProcessor` — 闪避判定
2. `CriticalDamageProcessor` — 暴击倍率
3. `ArmorDamageProcessor` — 护甲减免
4. `DamageTakenAmplificationProcessor` — 承伤倍率
5. `LifestealProcessor` — 吸血恢复
6. `ShieldDamageProcessor` — 护盾吸收
7. `HealthExecutionProcessor` — 最终生命值扣减
8. `DamageStatisticsProcessor` — 统计数据更新

## 11. 如何启用和关闭

启用：调用 `DamageDataKeys.RegisterAll()` 和 `DamageService.InitializeProcessors()`。

关闭：移除 `DamageService` 处理器或停止处理调用。

## 12. 如何测试

```bash
cd /home/slime/Code/SkilmeAI/SkilmeAI
Tools/run-tests.sh
```

Runtime tests 覆盖暴击、闪避、护甲、吸血、护盾等全处理器链路。

## 13. 常见错误日志

| 日志/症状 | 根因 |
|-----------|------|
| "No damage processors registered" | DamageService 未初始化处理器 |
| 伤害始终为 0 | Armor 过高或 DamageTakenMultiplier 为 0 |
| 暴击不触发 | CritRate 为 0 或随机种子固定 |
| 吸血不生效 | LifeSteal 为 0 或伤害被闪避 |
| 实体死亡后还能受伤 | 未检查 `IsDead` 或 `IsInvulnerable` |

## 14. AI 修改边界

### 可以修改

- 新增 `IDamageProcessor` 实现并注册
- 新增伤害类型或标签
- 调整处理器优先级
- 新增统计 DataKey

### 禁止修改

- 不直接修改 `CurrentHp`（必须用 `DamageService.Process()`）
- 不删除 `HealthExecutionProcessor`
- 概率值必须保持 0-100 语义
- 不在热路径分配对象
