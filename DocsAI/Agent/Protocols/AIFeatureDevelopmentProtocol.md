# AI 新功能开发协议

> 更新日期：2026-05-11  
> 状态：生效候选  
> 适用范围：创建新功能、扩展现有能力、迁移旧逻辑、实现 OpenSpec task、重构框架行为、添加 DataOS 字段、增加 GodotBridge Adapter 或接入游戏验证切片。

## 什么时候用

每次 AI 准备写新功能或改变现有行为前都使用本协议，并先调用 `.codex/skills/ai-feature-development`。本协议在 owner skill 之前使用；owner skill 负责具体领域，本协议负责 AI-first 路由、边界和收尾。

## 基本原则

- AI 便利优先：入口少、事实源少、验证命令固定、artifact 可检查。
- 不为旧框架兼容保留新入口。旧实现只作为迁移输入；如果旧写法妨碍 AI 路由、验证或观察，按当前 GameOS 边界重构。
- 小内核，可选能力：Runtime kernel 保持少入口；功能进入 Capability、DataOS、GodotBridge 或游戏 adapter。
- 纯 Runtime / tooling 优先使用 C# 标准库和框架 API。JSON、普通文件、集合处理、时间、随机数等非引擎职责不要用 Godot helper。
- GodotBridge 只做引擎适配：`Node`、`SceneTree`、Physics、Input、Resource、可视化实例化和场景生命周期。
- 术语优先：SlimeAI 是 `AI-first GameOS / Capability Composition Runtime`，不是传统 ECS 框架。`Entity / Component / System` 命名敏感任务必须先读 `DocsAI/GameOS/Overview.md#术语表` 和 ECS 边界 ADR。

## 执行顺序

1. 读 `DocsAI/INDEX.md`、`DocsAI/ProjectState.md` 和相关 domain 文档。
2. 判断是否需要 OpenSpec。大型功能、架构变更、跨模块重构、长期设计决策和迁移账本必须进入 `openspec/changes/<change>/`。
3. 选择 owner skill：Capability 用对应 `*-system`，DataOS 用 `data-authoring`，GodotBridge Adapter / legacy `IGodotComponent` 用 `ecs-component`，测试用 `test-system` 或 `godot-scene-test`。
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

## 功能收尾闸门

新功能不能只以“代码能编译”结束。结束前必须检查：

- 是否有专项测试入口。纯 Runtime / Capability 行为优先补 `Tests/SlimeAI.GameOS.Tests/`；涉及 GodotBridge、Godot Node 生命周期、Physics、Input、Resource、UI、动画或游戏侧胶水时，必须在 BrotatoLike 新增独立 Godot 验证场景，遵守 `DocsAI/Tests/GodotSceneValidation.md` 的目录、README、PASS/FAIL marker 和 artifact 规则。
- 框架级 Godot 验证场景写在框架仓 `Scenes/Validation/...` / `Src/SceneTests/...`；跑 Godot 前必须选定承载游戏。当前初始开发阶段默认 BrotatoLike，并直接把新增/修改的场景和脚本同步到 `Games/BrotatoLike/SlimeAI/` submodule 工作树；后续多游戏 / 成品阶段不默认同步所有游戏，改按各游戏版本策略更新 submodule 指针，并确认承载游戏 wrapper 扫描 `SlimeAI/Scenes,SlimeAI/Src`。
- `run-main-smoke`、普通 `Scenes/Main.tscn` 或 playable acceptance 只能作为回归补充，不能替代新功能专项场景。
- DataOS 新字段必须更新 schema / migration / seed 或 snapshot generator / validator / Runtime loader 中对应的断言；descriptor mirror 必须匹配 Runtime `DataKey<T>` 类型和默认值。
- Runtime Data 新代码必须使用 `DataKey<T>` + active `DataCatalog`，不新增旧 string/object/DataMeta/DataRegistry 业务入口。
- 不用传统 ECS 心智模型设计新功能：不得默认新增 global world query DSL、archetype/chunk/sparse-set API、纯 ECS component storage 或绕过 Capability owner 的 query system。若确有必要，必须先创建单独 OpenSpec proposal，说明 Capability-owned selector、DataKey、Runtime Process 或 GodotBridge Adapter 为什么不足。
- 改了 public API、DataKey、Event、selector、service、验证 artifact 或 AI 路由时，必须同步所有相关 DocsAI 和 owner skill。

## 外部资料使用

优先读本地 `Resources/Engine/Docs/FrameworkAnalysis/Reports/`。外部框架只筛选可服务 AI 的机制：

- Bevy / Flecs / Unity Entities / Unreal GAS 的能力组织、阶段、deferred command、authoring-to-runtime 和验证产物可参考。
- 不复制外部 ECS 存储、通用 query DSL、动态插件生命周期或复杂 tag/attribute 栈。

## 文档同步

完成后按影响面同步：

- 新 AI 路由：`.ai-config/skills/*` 统一源、`DocsAI/INDEX.md`、`DocsAI/Agent/Overview.md`。
- Runtime / Capability API：`DocsAI/GameOS/Contracts.md`、`DocsAI/GameOS/ApiIndex.md`、对应 Contract / Debug / `CapabilityIndex.md`。
- DataOS：`DocsAI/DataOS/Overview.md`、snapshot manifest、schema / validator 文档。
- 测试和验证：`DocsAI/Tests/*`、Godot scene artifact 说明。
- 当前阶段变化：`DocsAI/ProjectState.md`。

Skill 的唯一维护源是工作区 `.ai-config/skills/<category>/<name>/SKILL.md`；修改后运行 `bash Workspace/Tools/ai-config-sync/sync-ai-config.sh` 生成 `.codex/.claude/.windsurf` 副本，不直接手改副本。
