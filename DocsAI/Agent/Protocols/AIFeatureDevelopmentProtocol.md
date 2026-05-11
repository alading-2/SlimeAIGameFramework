# AI 新功能开发协议

> 更新日期：2026-05-11  
> 状态：生效候选  
> 适用范围：创建新功能、扩展现有能力、迁移旧逻辑、实现 OpenSpec task、重构框架行为、添加 DataOS 字段、增加 GodotBridge 组件或接入游戏验证切片。

## 什么时候用

每次 AI 准备写新功能或改变现有行为前都使用本协议，并先调用 `.codex/skills/ai-feature-development`。本协议在 owner skill 之前使用；owner skill 负责具体领域，本协议负责 AI-first 路由、边界和收尾。

## 基本原则

- AI 便利优先：入口少、事实源少、验证命令固定、artifact 可检查。
- 不为旧框架兼容保留新入口。旧实现只作为迁移输入；如果旧写法妨碍 AI 路由、验证或观察，按当前 GameOS 边界重构。
- 小内核，可选能力：Runtime kernel 保持少入口；功能进入 Capability、DataOS、GodotBridge 或游戏 adapter。
- 纯 Runtime / tooling 优先使用 C# 标准库和框架 API。JSON、普通文件、集合处理、时间、随机数等非引擎职责不要用 Godot helper。
- GodotBridge 只做引擎适配：`Node`、`SceneTree`、Physics、Input、Resource、可视化实例化和场景生命周期。

## 执行顺序

1. 读 `DocsAI/INDEX.md`、`DocsAI/ProjectState.md` 和相关 domain 文档。
2. 判断是否需要 OpenSpec。大型功能、架构变更、跨模块重构、长期设计决策和迁移账本必须进入 `openspec/changes/<change>/`。
3. 选择 owner skill：Capability 用对应 `*-system`，DataOS 用 `data-authoring`，GodotBridge 组件用 `ecs-component`，测试用 `test-system` 或 `godot-scene-test`。
4. 读 owner Contract、Debug、ApiIndex、tests、CapabilityIndex 或游戏侧状态文档。
5. 改文件前在真实仓库目录运行 `git status --short`，不要覆盖用户已有改动。
6. 实现最小闭环：代码、测试、验证 artifact、DocsAI 更新一起完成。
7. 结束前按 `AITaskCompletionContract.md` 汇报修改文件、验证结果、未验证原因和剩余风险。

## 设计检查

新功能设计必须能回答：

- AI 下次从哪个文档和哪个 skill 进入？
- public API 是否写入 Contract / ApiIndex？
- 新 DataKey / Event / selector / service 的 owner 是谁？
- Runtime 是否脱离 Godot 可测？
- DataOS 是否只让 Runtime 读取 generated snapshot？
- Observation 是否能导出足够的 dump / trace / blocked reason？
- 失败时 AI 应读哪个 Debug 文档，跑哪个命令？

## 外部资料使用

优先读本地 `Engine/Docs/FrameworkAnalysis/Reports/`。外部框架只筛选可服务 AI 的机制：

- Bevy / Flecs / Unity Entities / Unreal GAS 的能力组织、阶段、deferred command、authoring-to-runtime 和验证产物可参考。
- 不复制外部 ECS 存储、通用 query DSL、动态插件生命周期或复杂 tag/attribute 栈。

## 文档同步

完成后按影响面同步：

- 新 AI 路由：`.codex/skills/*`、`DocsAI/INDEX.md`、`DocsAI/Agent/Overview.md`。
- Runtime / Capability API：`DocsAI/GameOS/Contracts.md`、`DocsAI/GameOS/ApiIndex.md`、对应 Contract / Debug / `CapabilityIndex.md`。
- DataOS：`DocsAI/DataOS/Overview.md`、snapshot manifest、schema / validator 文档。
- 测试和验证：`DocsAI/Tests/*`、Godot scene artifact 说明。
- 当前阶段变化：`DocsAI/ProjectState.md`。
