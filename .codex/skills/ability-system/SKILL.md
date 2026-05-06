---
name: ability-system
description: 修改 SkilmeAI.GameOS AbilityService、AbilityDataKeys、目标选择、冷却充能、自动索敌或技能触发执行流程时使用。
---

# Ability Capability 入口

## 必读入口

- `GameOS/SkilmeAI.GameOS.Contracts.md`
- `GameOS/SkilmeAI.GameOS.ApiIndex.md`
- `DocsAI/ProjectState.md`
- `DataOS/README.md`

## 源码位置

- `GameOS/Capabilities/Ability/`
- `GameOS/Capabilities/Feature/`
- `GameOS/Capabilities/Damage/`
- `GameOS/Capabilities/Projectile/`
- `/home/slime/Code/SkilmeAI/Games/BrotatoLike/Src/Game/BrotatoLikeAbilityHandlers.cs`

## 参考资料

- 旧 Ability handler 参数语义：`references/ability-logic-parameters.md`

## 规则

- `AbilityService` 负责门禁、冷却、充能、触发和调度，不隐式做输入层点选。
- 自动索敌通过 `AbilityTargetingTool` 显式准备 `AbilityCastContext`。
- 具体技能效果优先实现为 Feature handler 或游戏侧 handler，不把 BrotatoLike 逻辑写进框架通用 Ability。
- 技能造成伤害统一走 `DamageTool` / `DamageService`；投射物走 `ProjectileTool`。

## 验证

```bash
Tools/run-build.sh
Tools/run-tests.sh
cd /home/slime/Code/SkilmeAI/Games/BrotatoLike
Tools/run-godot-scene.sh run-main-smoke --log-dir .ai-temp/scene-tests/runs
```
