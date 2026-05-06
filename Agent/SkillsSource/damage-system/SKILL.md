---
name: damage-system
description: 修改 SkilmeAI.GameOS DamageService、DamageInfo、处理器管线、HealService、DamageTool 或接触伤害桥时使用。
---

# Damage Capability 入口

## 必读入口

- `GameOS/SkilmeAI.GameOS.Contracts.md`
- `GameOS/SkilmeAI.GameOS.ApiIndex.md`
- `DocsAI/ProjectState.md`

## 源码位置

- `GameOS/Capabilities/Damage/`
- `GameOS/GodotBridge/GodotContactDamageComponent.cs`
- `Tests/SkilmeAI.GameOS.Tests/`

## 规则

- 伤害和治疗入口分别是 `DamageService` / `HealService`。
- 概率值统一 0-100，计算时再除以 100。
- 新伤害修正写成 `IDamageProcessor` 并明确优先级。
- 接触、攻击、技能、投射物只组装 `DamageInfo`，不要绕过 DamageService 直接写 `CurrentHp`。

## 验证

```bash
Tools/run-build.sh
Tools/run-tests.sh
cd /home/slime/Code/SkilmeAI/Games/BrotatoLike
Tools/run-godot-scene.sh run-main-smoke --log-dir .ai-temp/scene-tests/runs
```
