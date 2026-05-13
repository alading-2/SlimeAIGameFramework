# Capability 修改协议

> 更新日期：2026-05-13  
> 状态：生效候选  
> 适用范围：修改 `SlimeAI/GameOS/Capabilities/`、相关 DataKeys、Events、GodotBridge adapter、Capability tests 或 Capability 文档。

## 什么时候用

任务涉及以下内容时使用本协议：

- Movement、Collision、Damage、Unit、Attack、AI、Ability、Feature、Projectile、Effect。
- Capability service、tool、handler、DataKeys、Events、Contract、Debug、tests。
- Capability 相关 GodotBridge Adapter / legacy `Godot*Component`。
- Capability status、owner skill、validation evidence。

## 必须读什么

每次 Capability 修改前必须读：

- `SlimeAI/DocsAI/GameOS/Capabilities/CapabilityIndex.md`
- 对应 `.codex/skills/<owner>/SKILL.md`
- 对应 Capability 的 `capability.json`
- 对应 Capability 的 `Contract.md`
- 对应 Capability 的 `Debug.md`
- `SlimeAI/DocsAI/GameOS/Contracts.md`
- `SlimeAI/DocsAI/GameOS/ApiIndex.md`
- 相关 tests：通常在 `SlimeAI/Tests/SlimeAI.GameOS.Tests/`
- 当前 OpenSpec change artifacts

跨边界时追加：

- DataOS 改动读 `SlimeAI/DocsAI/DataOS/Overview.md` 和 validator/generator。
- GodotBridge 改动读 `ecs-component` skill 和相关 bridge component。
- 游戏 adapter 改动读 `Games/BrotatoLike/DocsAI/GameProjectState.md`。

## 修改规则

- 新 DataKey 必须通过对应 `*DataKeys.cs` 暴露为 `DataKey<T>` handle，并通过 `FrameworkDataKeys.RegisterAll()` / profile `DataCatalog` 进入 active catalog；authoring metadata 写入 DataOS `data_key_descriptor`，不新增散落字符串访问。
- 新 Event 按 Capability 放入 `SlimeAI/GameOS/Capabilities/<Cap>/Events/<Name>.cs`，每个事件为 `readonly record struct` 并实现 `IEntityEvent / IGlobalEvent / IBroadcastEvent` 之一；跨 Capability 的 Runtime 级事件放 `Runtime/Events/{Core,Global}/`。
- Capability service 只处理本能力职责，不直接吞掉其他 Capability 的状态真相源。
- Gameplay 行为必须路由到 Capability service、tool、handler、DataKey、Event、selector 或必要的 Runtime Process；不要新增泛型 Component 或裸 System 来拥有玩法行为。
- 纯 Runtime Capability 不依赖 Godot `Node`、`Vector2` 或资源加载。
- GodotBridge Adapter 只做 Node / SceneTree / Physics / Input / Resource 适配，不把 Godot Node tree 当玩法状态真相源。`IGodotComponent` / `Godot*Component` 是 legacy compatibility name。
- 游戏侧 handler 可以消费 Capability API，但不把游戏专属规则写回框架。
- 修改 owner、status、DataKeys、Events、GodotBridge boundary 或 validation evidence 时，同步更新 `CapabilityIndex.md`。

## 禁止什么

- 禁止绕过 DamageService 直接扣 HP。
- 禁止绕过 AttackService / AbilityService 私自触发完整攻击或施法流程。
- 禁止 AI 节点直接移动 Godot 节点或直接造成伤害；AI 只写意图和请求。
- 禁止 Capability 内直接调用 `GD.Load`；资源加载放在 ResourceManagement / GodotBridge / 游戏侧。
- 禁止把 BrotatoLike 专属技能行为写进框架默认 Capability。
- 禁止把 legacy `stable` 自动当作 `Supported`。
- 禁止把 SlimeAI 任务按传统 ECS world / component storage / query system 心智模型改造；除非单独 OpenSpec change 已明确论证并列出验证。

## 验证什么

最低验证：

```bash
cd /home/slime/Code/SlimeAI
openspec validate <change-id> --strict
```

核心框架代码或 Capability 文档变更：

```bash
cd /home/slime/Code/SlimeAI/SlimeAI
Tools/run-build.sh
Tools/run-tests.sh
```

DataOS 相关：

```bash
cd /home/slime/Code/SlimeAI/SlimeAI
Tools/run-dataos-validate.sh
```

GodotBridge 或游戏 adapter 相关：

```bash
cd /home/slime/Code/SlimeAI/Games/BrotatoLike
Tools/run-build.sh
Tools/run-godot-scene.sh run-main-smoke --log-dir .ai-temp/scene-tests/runs
Tools/analyze-godot-scene-logs.sh
```

## 完成时怎么汇报

必须汇报：

- Capability 名称和 owner skill。
- primary category：通常是 `Capability`，并列出 secondary categories。
- 修改文件列表。
- 是否更新 `CapabilityIndex.md`、Contract、Debug、ApiIndex 或 tests。
- 验证命令和结果。
- 未验证原因。
- status 是否变化；如果涉及 `Supported`，列出 Contract / Debug / Tests / profile or game-slice evidence。
- 需要人工重点 review 的行为边界。

## 多轮失败时如何复盘

同一 Capability 修改连续失败两次时，记录：

- 失败命令和关键错误。
- 失败发生在 Runtime、DataOS、GodotBridge、tests 还是 docs。
- 是否违反 owner skill 或边界协议。
- 是否需要先补 Contract / Debug / tests / CapabilityIndex。
- 是否应该拆出更小 OpenSpec child change。

复盘后再继续，不重复盲跑同一命令。
