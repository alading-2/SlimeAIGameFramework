# SkilmeAI Framework Overview

## 定位

SkilmeAI 是 AI-first GameOS 框架，不是通用游戏引擎。它面向 Godot 4.6 C# 项目，提供可由 AI 稳定路由和验证的运行时内核、可选能力、数据快照、观察日志、Agent 协议和游戏适配边界。

核心结构：

```text
Small Runtime Kernel
  + Optional Capabilities
  + Genre Profiles
  + Game Adapters
  + DataOS
  + Validation / Observation
  + Agent Protocol
```

## 框架职责

- Runtime Kernel：Entity、Data、Event、Relationship、Schedule、Timer、Pool、Resource。
- Optional Capabilities：Movement、Collision、Damage、Unit、Attack、AI、Ability、Feature、Projectile、Effect。
- Genre Profiles：定义某类游戏可复用能力组合，例如 Survivor2D。
- Game Adapters：把框架能力接入具体游戏，不把游戏专属行为上提为框架默认。
- DataOS：Authoring schema、migration、snapshot、validator 和 Runtime snapshot loader。
- Validation / Observation：构建、测试、Godot headless 场景、日志、JSONL、artifact。
- Agent Protocol：AI 修改前的路由、边界、完成汇报和多轮失败处理。

## 当前事实源

- 长期文档：`SkilmeAI/DocsAI/`
- 实施计划：`openspec/changes/<change>/`
- Skill 源头：`.codex/skills/`
- 游戏侧文档：`Games/BrotatoLike/DocsAI/`

## 非目标

- 不替代 Godot 引擎。
- 不复制 ECS/GAS/DOTS/QFramework 公共 API。
- 不让 runtime hot path 查询 DataOS authoring SQLite。
- 不把 BrotatoLike 技能、波次、UI、资产路径当成框架契约。
