---
name: attack-system
description: 修改 SkilmeAI.GameOS AttackService、AttackDataKeys、攻击请求事件、前摇后摇冷却或 GodotAttackComponent 时使用。
---

# Attack Capability 入口

## 必读入口

- `GameOS/SkilmeAI.GameOS.Contracts.md`
- `GameOS/SkilmeAI.GameOS.ApiIndex.md`
- `DocsAI/ProjectState.md`

## 源码位置

- `GameOS/Capabilities/Attack/`
- `GameOS/Capabilities/Damage/`
- `GameOS/GodotBridge/GodotAttackComponent.cs`
- `GameOS/GodotBridge/AttackComponent.cs`
- `GameOS/GodotBridge/GodotUnitAnimationComponent.cs`
- `Tests/SkilmeAI.GameOS.Tests/`

## 规则

- AI / 输入层发 `GameEventType.Attack.Requested` 或调用 AttackService，不直接扣血。
- 攻击伤害统一走 `DamageService`，标签使用 `DamageTags.Attack`。
- 前摇 / 后摇 / 冷却统一用 `TimerManager`。
- 旧场景类名兼容放 `AttackComponent` 包装，不污染新 API。

## 验证

```bash
Tools/run-build.sh
Tools/run-tests.sh
cd /home/slime/Code/SkilmeAI/Games/BrotatoLike
Tools/run-godot-scene.sh run-main-smoke --log-dir .ai-temp/scene-tests/runs
```
