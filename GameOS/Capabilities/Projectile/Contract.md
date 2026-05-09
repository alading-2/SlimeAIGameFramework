# Projectile Capability Contract

## 1. 解决什么问题

提供投射物的生成与完整生命周期管理：

- 从 Ability/Feature 生成投射物 Entity
- 绑定 Movement 策略
- 命中检测（穿透、最大命中次数）
- 最大存活时间（MaxLifeTime）
- 命中后通过 DamageService 造成伤害
- 命中/超时后销毁并清理视觉节点

## 2. 不是本能力职责的内容

- **弹道计算** → Movement Capability（Projectile 只启动 Movement）
- **伤害计算** → Damage Capability
- **特效播放** → Effect Capability
- **碰撞检测** → Collision Capability

## 3. 需要的 Runtime 能力

- Entity
- Data
- Event
- Schedule
- Pool（对象池复用）

## 4. 需要哪些其他 Capability

- Movement（投射物移动）
- Damage（命中伤害）
- Effect（命中特效）

## 5. 写入的 DataKey

| DataKey | 类型 | 说明 |
|---------|------|------|
| Projectile.HitCount | int | 当前命中次数 |
| Projectile.SpawnPosition | Vector2Value | 生成位置 |
| Projectile.TargetPosition | Vector2Value | 目标位置 |
| Projectile.Direction | Vector2Value | 方向 |

## 6. 读取的 DataKey

| DataKey | 说明 |
|---------|------|
| Projectile.ScenePath | 视觉场景路径 |
| Projectile.SourceEntity | 来源实体 |
| Projectile.AbilityEntity | 关联技能实体 |
| Projectile.TargetEntity | 目标实体 |
| Projectile.Speed | 速度 |
| Projectile.MaxHitCount | 最大命中次数（-1 无限） |
| Projectile.MaxLifeTime | 最大存活时间（-1 无限） |
| Projectile.Damage | 伤害值 |
| Projectile.DamageType | 伤害类型 |
| Projectile.DamageTags | 伤害标签 |

## 7. 发布的事件

- `Projectile.Spawned` — 投射物生成
- `Projectile.Hit` — 命中目标
- `Projectile.LifeTimeExpired` — 存活时间到期
- `Projectile.Destroyed` — 销毁完成

## 8. 订阅的事件

无直接订阅。通过 `ProjectileTool.StartMovement()` 和 MovementCollision 回调驱动。

## 9. 挂载的 Component

- `ProjectileTool` — 投射物生成和管理
- `GodotProjectileEffectSpawner`（GodotBridge）— 场景实例化和视觉管理

## 10. 注册的 System / Strategy / Handler

- `ProjectileTool` — 生成、命中处理、生命周期管理

## 11. 如何启用和关闭

启用：调用 `ProjectileDataKeys.RegisterAll()`。

关闭：停止生成或设置 `MaxLifeTime = 0`。

## 12. 如何测试

```bash
cd /home/slime/Code/SkilmeAI/SkilmeAI
Tools/run-tests.sh
```

覆盖生成、命中、穿透多目标、MaxLifeTime 销毁、视觉节点清理。

## 13. 常见错误日志

| 日志/症状 | 根因 |
|-----------|------|
| 投射物不移动 | Movement Handler 未绑定或 `Speed` 为 0 |
| 命中不触发伤害 | `MaxHitCount` 已达上限或目标已死亡 |
| 视觉节点残留 | `GodotProjectileEffectSpawner` 未正确清理 |
| 穿透失效 | `MaxHitCount` 为 1 或 DamageService 未初始化 |

## 14. AI 修改边界

### 可以修改

- 新增投射物属性 DataKey
- 调整命中逻辑
- 新增销毁条件

### 禁止修改

- 不直接 `QueueFree` 视觉节点（必须通过对象池）
- 不在 `_Process` 中分配集合
- 命中后必须正确更新 `HitCount`
