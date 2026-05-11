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
- 游戏锁定版本：不更新 submodule 指针即可。
- 游戏升级框架：`cd SkilmeAI && git pull origin main && cd .. && git add SkilmeAI && git commit -m "bump SkilmeAI to <commit>"`。
- SkilmeAI 仓可用 tag 标记里程碑版本（如 `v0.3.0`）。

## 工作流

### 日常开发

```bash
# 在 SkilmeAI 仓开发框架
cd /home/slime/Code/SkilmeAI/SkilmeAI
# 改代码、改场景...
git add . && git commit -m "..." && git push SlimeAIGameFramework main

# 在游戏仓拉取更新
cd /home/slime/Code/SkilmeAI/Games/BrotatoLike
cd SkilmeAI && git pull && cd ..
git add SkilmeAI && git commit -m "bump SkilmeAI"
git push BrotatoLike main
```

### 新游戏接入

```bash
mkdir Games/Game2 && cd Games/Game2
# 初始化 Godot 项目（project.godot + Game2.csproj）
git init && git remote add origin <url>
git submodule add https://github.com/alading-2/SlimeAIGameFramework.git SkilmeAI
# Game2.csproj 默认 **/*.cs glob 自动吃 SkilmeAI 源码
# 场景里可直接引用 res://SkilmeAI/... 的脚本和场景
```

### 克隆游戏仓

```bash
git clone --recurse-submodules https://github.com/alading-2/BrotatoLike.git
# 或
git clone https://github.com/alading-2/BrotatoLike.git
cd BrotatoLike && git submodule update --init
```

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

游戏 csproj 当前使用 `<ProjectReference>`（过渡期）：

```xml
<ProjectReference Include="SkilmeAI/GameOS/SkilmeAI.GameOS.csproj" />
```

最终形态可改为纯 glob（删除 ProjectReference，依赖默认 `**/*.cs`）。两种都能工作，区别在于 ProjectReference 保留了 assembly 边界（IDE 智能提示更精确），glob 更简单但所有代码混入同一 assembly。
