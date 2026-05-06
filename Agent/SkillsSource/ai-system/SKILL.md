---
name: ai-system
description: 修改 SkilmeAI.GameOS AIService、行为树节点、AIDataKeys、目标查询、巡逻、攻击请求或 Godot AI bridge 时使用。
---

# AI Capability 入口

## 必读入口

- `GameOS/SkilmeAI.GameOS.Contracts.md`
- `GameOS/SkilmeAI.GameOS.ApiIndex.md`
- `DocsAI/ProjectState.md`

## 源码位置

- `GameOS/Capabilities/AI/`
- `GameOS/Capabilities/Movement/`
- `GameOS/Capabilities/Ability/`
- `GameOS/GodotBridge/GodotAIComponent.cs`
- `Tests/SkilmeAI.GameOS.Tests/`

## 规则

- AI 节点只写意图、目标和请求事件，不直接移动节点、不直接造成伤害。
- 移动意图写入 `MovementDataKeys.AIMoveDirection / AIMoveSpeedMultiplier`。
- 攻击通过 `GameEventType.Attack.Requested` 和 AttackService 消费。
- 自动施法上下文通过 AbilityTargetingTool / BehaviorNode 显式准备。

## 验证

```bash
Tools/run-build.sh
Tools/run-tests.sh
cd /home/slime/Code/SkilmeAI/Games/BrotatoLike
Tools/run-godot-scene.sh run-main-smoke --log-dir .ai-temp/scene-tests/runs
```
