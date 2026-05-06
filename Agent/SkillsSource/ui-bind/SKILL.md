---
name: ui-bind
description: 迁移或设计 SkilmeAI / BrotatoLike UI、HUD、响应式绑定、Entity 数据显示或游戏侧 UI 场景时使用。
---

# UI / HUD 入口

## 当前边界

- 通用 UI 绑定协议尚未作为 GameOS 稳定 Capability 完成。
- BrotatoLike 真实 UI / 输入 / 场景内容后续从 `MigrationInput/` 迁入游戏仓库。
- 框架层只沉淀可复用协议；游戏视觉、布局和具体 HUD 放 `Games/BrotatoLike`。

## 必读入口

- `DocsAI/ProjectState.md`
- `GameOS/SkilmeAI.GameOS.Contracts.md`
- `/home/slime/Code/SkilmeAI/Games/BrotatoLike/DocsAI/GameProjectState.md`
- `/home/slime/Code/SkilmeAI/Games/BrotatoLike/MigrationInput/`

## 规则

- UI 读取 Entity 状态通过 Runtime Data / Entity.Events，不直接改业务状态。
- UI 不绕过 Damage / Ability / Feature / Schedule 服务修改核心逻辑。
- 游戏特定 UI 先放 BrotatoLike；复用稳定后再抽到框架。

## 验证

```bash
cd /home/slime/Code/SkilmeAI/Games/BrotatoLike
Tools/run-build.sh
Tools/run-godot-scene.sh run-main-smoke --log-dir .ai-temp/scene-tests/runs
```
