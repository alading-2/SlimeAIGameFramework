---
name: feature-system
description: 修改 SkilmeAI.GameOS FeatureService、FeatureDefinition、FeatureModifierEntry、IFeatureHandler 或 Feature 与 Ability 接入时使用。
---

# Feature Capability 入口

## 必读入口

- `GameOS/SkilmeAI.GameOS.Contracts.md`
- `GameOS/SkilmeAI.GameOS.ApiIndex.md`
- `DocsAI/ProjectState.md`
- `DataOS/README.md`

## 源码位置

- `GameOS/Capabilities/Feature/`
- `GameOS/Capabilities/Ability/`
- `/home/slime/Code/SkilmeAI/Games/BrotatoLike/Src/Game/BrotatoLikeAbilityHandlers.cs`
- `Tests/SkilmeAI.GameOS.Tests/`

## 规则

- Feature 核心不引用游戏专有类型。
- 子系统上下文通过 `FeatureContext.ActivationData` 传入，执行结果通过 `FeatureContext.ExecuteResult` 返回。
- Modifier 授予 / 回滚由 `FeatureService` 管理，避免 handler 私自写长期状态。
- 游戏具体 Feature actions 可以先放游戏仓库 handler，抽象稳定后再上迁框架。

## 验证

```bash
Tools/run-build.sh
Tools/run-tests.sh
cd /home/slime/Code/SkilmeAI/Games/BrotatoLike
Tools/run-godot-scene.sh run-main-smoke --log-dir .ai-temp/scene-tests/runs
```
