# SkilmeAI 多游戏架构

本文档记录 SkilmeAI 大框架 + 多游戏的仓库架构决策和工作流。

## 架构模型

```
SkilmeAI 仓（唯一可写，remote: alading-2/SlimeAIGameFramework）
   │
   │  git submodule（只读镜像）
   ▼
游戏仓 A（remote: alading-2/BrotatoLike）
游戏仓 B（未来）
游戏仓 C（未来）
```

- **SkilmeAI 仓**：独立 git 仓库，是框架的唯一事实源。所有框架代码、通用场景、验证场景都在此维护。
- **每个游戏仓**：独立 git 仓库，有自己的 `project.godot`。通过 git submodule 持有一份 SkilmeAI 的只读镜像。
- **单向数据流**：框架改动只在 SkilmeAI 仓提交 → push；游戏仓通过 `git submodule update` 拉取新版本。

## 为什么能工作

游戏仓结构：

```
Games/BrotatoLike/          (游戏仓根 = project.godot 所在 = res:// 根)
├── project.godot
├── BrotatoLike.csproj      (默认 **/*.cs glob)
├── SkilmeAI/               (git submodule)
│   ├── GameOS/
│   ├── DataOS/
│   └── Scenes/Validation/
├── Scenes/Main.tscn
└── Src/Game/
```

机制链路（详见 `GodotCSharpConstraints.md`）：

1. `BrotatoLike.csproj` 的默认 `**/*.cs` glob 把 `SkilmeAI/**/*.cs` 编译进游戏主 assembly。
2. `ScriptPathAttributeGenerator` 基于游戏 csproj 的 `GodotProjectDir`（= 游戏仓根）生成路径：`[ScriptPathAttribute("res://SkilmeAI/GameOS/GodotBridge/GodotEntity2D.cs")]`。
3. `ScriptManagerBridge.LookupScriptsInAssembly` 扫描主 assembly，登记所有 `[ScriptPathAttribute]`。
4. `.tscn` 里 `ExtResource("Script", path="res://SkilmeAI/...")` 能正确解析。

结论：SkilmeAI 的代码和场景都在游戏的 `res://` 空间内，脚本加载链路完全成立。

## 版本管理

- submodule commit hash = 框架版本号，天然写在游戏仓的 git 记录里。
- SkilmeAI 仓可用 tag 标记里程碑版本（如 `v0.3.0`）。
- 游戏升级框架、克隆、故障排查等操作详见工作区文档 `Workspace/DocsAI/GitSubmoduleWorkflow.md`。

## 禁区

- **游戏仓禁止对 `SkilmeAI/` 目录做业务改动**。只允许"submodule 指针前进"（commit hash 变化）。
- **SkilmeAI 仓禁止依赖任何游戏专属资源**（禁止出现 BrotatoLike 特有美术路径、特有输入键名）。
- **场景路径必须用 `res://` 绝对路径**，不用相对路径。相对路径在跨目录实例化时会失效。

## 场景归属

| 场景类型 | 放哪里 | 原因 |
|----------|--------|------|
| 框架验证场景（Observation、Event 等） | SkilmeAI 仓 `Scenes/Validation/` | 通用，所有游戏复用 |
| 框架 prefab（可选） | SkilmeAI 仓 `Scenes/Prefabs/` | 通用组件 |
| 游戏主场景（Main.tscn） | 游戏仓 `Scenes/` | 游戏专属入口 |
| 游戏玩法场景 | 游戏仓 `Scenes/Game/` | 游戏专属 |
| 游戏美术资产场景 | 游戏仓 `assets/` | 游戏专属 |

## 当前实例

| 仓库 | Remote | 角色 |
|------|--------|------|
| `/home/slime/Code/SkilmeAI/SkilmeAI` | `alading-2/SlimeAIGameFramework` | 框架仓（唯一可写） |
| `/home/slime/Code/SkilmeAI/Games/BrotatoLike` | `alading-2/BrotatoLike` | 第一个游戏仓 |
| `Games/BrotatoLike/SkilmeAI/` | submodule → SlimeAIGameFramework | 只读镜像 |

## csproj 配置

**过渡期**：游戏 csproj 通过 `ProjectReference` 引用独立工作树的框架项目：

```xml
<ProjectReference Include="../../SkilmeAI/GameOS/SkilmeAI.GameOS.csproj" />
```

- `../../SkilmeAI/` 指向工作区根下的框架独立工作树（`/home/slime/Code/SkilmeAI/SkilmeAI`），不是 git submodule 目录。
- `ScriptPathAttributeGenerator` 基于游戏 csproj 的 `GodotProjectDir`（= 游戏仓根）生成路径，如 `res://SkilmeAI/GameOS/GodotBridge/GodotEntity2D.cs`。
- 游戏 csproj 默认 `**/*.cs` glob 会自动包含 `SkilmeAI/` submodule 目录下的源码；需用 `<Compile Remove="SkilmeAI/Tests/**/*.cs" />` 排除测试文件。

**最终形态**：改为 `SkilmeAI/GameOS/SkilmeAI.GameOS.csproj`（引用 submodule 内项目）或纯 glob（删除 ProjectReference，依赖默认 `**/*.cs`）。两者区别在于 ProjectReference 保留了 assembly 边界（IDE 智能提示更精确），glob 更简单但所有代码混入同一 assembly。
