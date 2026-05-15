# Effect Capability Contract

## 1. 解决什么问题

提供特效的生成、播放和生命周期管理：

- 按场景路径实例化特效节点
- 自动播放 `AnimatedSprite2D` 动画
- 绑定到源实体/目标实体/位置
- 按 Duration 自动销毁
- 视觉节点清理和回池

## 2. 不是本能力职责的内容

- **伤害/治疗** → Damage Capability
- **移动** → Movement Capability
- **技能触发** → Ability Capability
- **音效** → 当前未单独抽象，在游戏侧处理

## 3. 需要的 Runtime 能力

- Entity
- Data
- Event
- Pool

事件目录：`SlimeAI/GameOS/Capabilities/Effect/Events/`

## 4. 需要哪些其他 Capability

无。Effect 是基础表现 Capability，被 Ability/Projectile 依赖。

## 5. 写入的 DataKey

| DataKey | 类型 | 说明 |
|---------|------|------|
| Effect.Position | Vector2Value | 特效位置 |

## 6. 读取的 DataKey

| DataKey | 说明 |
|---------|------|
| Effect.ScenePath | 特效场景路径 |
| Effect.Name | 特效名称 |
| Effect.AnimationName | 动画名称（空表示自动播放） |
| Effect.SourceEntity | DataKey<EntityId?>；来源实体 Id |
| Effect.AbilityEntity | DataKey<EntityId?>；关联技能实体 Id |
| Effect.TargetEntity | DataKey<EntityId?>；目标实体 Id |
| Effect.Duration | 持续时间（-1 表示不自动销毁） |

## 7. 挂载的 Component

- `EffectTool` — 特效生成工具
- `GodotProjectileEffectSpawner`（GodotBridge，共享）— 场景实例化

## 8. 注册的 System / Strategy / Handler

- `EffectTool` — 生成和生命周期管理

## 9. 如何启用和关闭

启用：调用 `EffectDataKeys.RegisterAll()`。

关闭：停止生成或取消已生成的特效。

## 10. 如何测试

```bash
cd /home/slime/Code/SlimeAI/SlimeAI
Tools/run-tests.sh
```

覆盖生成、动画播放、Duration 销毁、节点清理。

## 11. 常见错误日志

| 日志/症状 | 根因 |
|-----------|------|
| 特效不显示 | `ScenePath` 错误或资源未导入 |
| 动画不播放 | `AnimatedSprite2D` 不存在或动画名错误 |
| 特效不销毁 | `Duration` 为 -1 且未手动清理 |
| 节点残留 | 回池流程异常 |

## 12. AI 修改边界

### 可以修改

- 新增 Effect DataKey
- 调整播放逻辑
- 新增销毁条件

### 禁止修改

- 不直接 `QueueFree` 节点（必须通过对象池）
- 不修改 `EffectTool` 接口签名

## 13. Runtime CommandBuffer / Guard

- `EffectTool.Spawn` 可能在 event handler、lifecycle callback 或 GodotBridge component callback guard 内被调用；guarded spawn 返回 reserved `RuntimeEntity`，本工具后续 `Data.Set` 会保留到 phase playback。
- `EffectTool.Spawn` 不应在同一 guard 内依赖 `EntityManager.Get(effect.EntityId)`；需要 registry 可见时由承载节点在合适 phase 调 `RuntimeWorld.Default.Schedule.RunPhase(...)`。
- `GodotProjectileEffectSpawner` 消费 `Spawned` event 时使用 event 内 entity handle，不要求 registry 已注册。
