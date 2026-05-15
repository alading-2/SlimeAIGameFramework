# Collision Capability Contract

## 1. 解决什么问题

提供碰撞层管理和碰撞事件分发：

- CollisionLayer / CollisionMask 位掩码管理
- Team（队伍）过滤
- 碰撞半径配置
- 碰撞进入/离开事件
- ContactDamage 接触伤害触发

## 2. 不是本能力职责的内容

- **伤害计算** → Damage Capability（ContactDamage 触发 DamageService）
- **物理模拟** → Godot Physics（本项目使用 Area2D 的监控机制）
- **移动策略** → Movement Capability

## 3. 需要的 Runtime 能力

- Entity
- Data
- Event

事件目录：`SlimeAI/GameOS/Capabilities/Collision/Events/`

## 4. 需要哪些其他 Capability

无。Collision 是基础 Capability，被 Damage（ContactDamage）依赖。

## 5. 写入的 DataKey

| DataKey | 类型 | 说明 |
|---------|------|------|
| Collision.CollisionLayer | int | 碰撞层 |
| Collision.CollisionMask | int | 碰撞掩码 |
| Collision.Team | int | 队伍 ID |
| Collision.CollisionRadius | float | 碰撞半径 |

## 6. 读取的 DataKey

同写入。碰撞配置通常是双向读取（两个实体互相检查）。

## 7. 挂载的 Component

- `CollisionSystem` — 碰撞事件管理
- `GodotCollisionComponent`（GodotBridge）— Area2D 碰撞体
- `GodotHurtboxComponent`（GodotBridge）— 受击区域
- `GodotContactDamageComponent`（GodotBridge）— 接触伤害触发器

## 8. 注册的 System / Strategy / Handler

- `CollisionSystem` — 碰撞过滤和事件分发
- `GodotCollisionIsolation` — 对象池碰撞隔离工具

## 9. 如何启用和关闭

启用：在 Entity 上写入 Collision DataKey，挂载 Godot 碰撞组件。

关闭：禁用 Area2D 监控或移除碰撞组件。

## 10. 如何测试

```bash
cd /home/slime/Code/SlimeAI/SlimeAI
Tools/run-tests.sh
```

Godot smoke 验证 Area2D 进入/离开事件。

## 11. 常见错误日志

| 日志/症状 | 根因 |
|-----------|------|
| 碰撞不触发 | Layer/Mask 不匹配或 Team 相同被过滤 |
| 对象池旧碰撞残留 | 回池时未正确脱树/禁用（见 GodotCollisionIsolation） |
| ContactDamage 不生效 | `ContactDamageInterval` 未过或目标无敌 |

## 12. AI 修改边界

### 可以修改

- 新增 CollisionLayer 枚举值
- 调整 Team 过滤逻辑
- 新增碰撞形状支持

### 禁止修改

- 不直接操作 Godot PhysicsServer2D RID
- 碰撞隔离逻辑必须保留（对象池安全）

## 13. Runtime CommandBuffer / Guard

- Collision / Hurtbox / ContactDamage 事件 handler 内若调用结构变更 API，会处于 event dispatch guard 并进入 Runtime CommandBuffer。
- CollisionSystem 自身不持有 deferred queue；只发布事件和写 Data。
- Godot collision bridge component callback 处于 `godot-bridge-callback` guard 内；callback 中不要依赖新 spawn 立即进入 registry。
