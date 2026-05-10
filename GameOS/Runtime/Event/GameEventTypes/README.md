# GameEventTypes 目录规则

这里专门放 `GameEventType.*` 事件名和 payload 类型。

- `Core/`：Runtime 基础事件，例如 `Data`、`Entity`、`Relationship`、`Input`。
- `Global/`：全局低频广播事件，例如游戏状态、波次、全局实体、鼠标选择。
- `Ability/`：Ability 专属事件，包括执行、冷却、充能、消耗和旧兼容事件。
- `Capabilities/`：已迁入框架 Capability 的事件，例如 Attack、Collision、Damage、Movement、Projectile、Effect、Feature、Unit。
- `Game/`：暂存仍偏游戏侧语义但需要框架兼容的事件，例如 Player、UI、Targeting、TestSystem。

新增事件优先放到最贴近语义的子目录，不要直接放回 `Runtime/Event/` 根目录。根目录只保留 `EventBus`、`GlobalEventBus`、`EventContext`、`EventPriority` 和 Data-to-Event bridge。
