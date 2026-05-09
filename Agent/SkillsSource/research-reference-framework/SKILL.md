---
name: research-reference-framework
description: 研究 Godot 底层、成熟开源框架、游戏项目或需要外部源码对照的复杂架构 / Debug 问题时使用。
---

# Research Reference 入口

## 必读入口

- `DocsAI/ProjectState.md`
- `GameOS/SkilmeAI.GameOS.DebugGuide.md`
- `/home/slime/Code/Godot/Games/MyGames/brotato-my/DocsAI/Protocols/外部资料与源码研究协议.md`
- `/home/slime/Code/Godot/Games/MyGames/brotato-my/DocsAI/Experience/踩坑与经验索引.md`

## 本地权威源码

- Godot 引擎：`/home/slime/Code/SkilmeAI/Engine/Engine/godot-4.6.2-stable`
- 迁移输入仓库：`/home/slime/Code/Godot/Games/MyGames/brotato-my`
- 框架仓库：`/home/slime/Code/SkilmeAI/SkilmeAI`
- 游戏验证仓库：`/home/slime/Code/SkilmeAI/Games/BrotatoLike`

## 规则

- GitHub 源码分析优先 clone 到本地后用 `rg` / `find` 阅读。
- Godot 底层问题先查本地 `Engine/Engine/godot-4.6.2-stable`。
- 研究结论必须落成可执行决策：修改点、风险、验证命令。
- 不把外部项目结构直接搬进 GameOS；只抽取适配本项目目标的机制。
