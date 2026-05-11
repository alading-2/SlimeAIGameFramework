# Godot C# 场景/脚本加载约束

本文档记录 Godot 4.6.2 引擎中 C# 脚本与场景加载的硬约束，基于引擎源码分析。这些约束决定了框架不能通过 NuGet 或 ProjectReference 共享 `.tscn` 场景。

## 核心链路

```
csproj 编译 → assembly 产出 → [ScriptPathAttribute] 登记 → res:// 路径查找
```

Godot 加载 C# 脚本的完整路径：

1. 游戏 csproj 编译产出主 assembly（由 `project.godot` 的 `dotnet/project/assembly_name` 指定）。
2. Godot 启动时 `GodotPlugins/Main.cs:154` 调用 `ScriptManagerBridge.LookupScriptsInAssembly(projectAssembly)`，**只扫描这一个 assembly**，不递归 ProjectReference。
3. `ScriptManagerBridge.cs:300-344` 遍历 assembly 中所有类型的 `[ScriptPathAttribute]`，建立 `res://` 路径 → Type 的双向映射（`_pathTypeBiMap`）。
4. 场景加载时 `csharp_script.cpp:2612` 调用 `AddScriptBridge`，后者在 `ScriptManagerBridge.cs:443` 查 `_pathTypeBiMap`。找不到 → 脚本无效。

## [ScriptPathAttribute] 路径生成

`ScriptPathAttributeGenerator.cs:97-110`（Godot.NET.Sdk 内置 source generator）：

- 对每个 `partial class : GodotObject`，计算 `.cs` 文件相对于 `GodotProjectDir` 的路径。
- `GodotProjectDir` 默认 = `$(MSBuildProjectDirectory)`（`Sdk.props:15`），即 csproj 所在目录。
- 生成 `[ScriptPathAttribute("res://相对路径")]`。

关键推论：**被引用 csproj 的 `.cs` 文件生成的 `res://` 路径是相对于自己 csproj 目录的**，不是相对于主游戏项目的。

## res:// 根定义

`core/config/project_settings.cpp:675,820`：`resource_path` = `project.godot` 所在目录。`editor/editor_file_system.cpp:246,402` 从此目录扫描文件系统。

`res://` 空间**只包含** `project.godot` 所在目录及其子目录。外部路径不可达。

## 默认 Compile glob

`Godot.NET.Sdk/Sdk.props:43` 设置 `EnableDefaultNoneItems=false`，但未禁用 `EnableDefaultCompileItems`。因此 Microsoft.NET.Sdk 的默认 `**/*.cs` glob 生效，会吞入项目目录下所有 `.cs`（包括 `addons/` 子目录）。

## 约束总结

### NuGet 包的 .tscn 不可见

- NuGet 包解压到 `~/.nuget/packages/<pkg>/<ver>/`，完全在项目目录外。
- `Godot.NET.Sdk/Sdk.props` 和 `Sdk.targets` 无任何将 NuGet `contentFiles` 复制到项目目录的 target。
- 结论：**NuGet 永远无法共享 `.tscn` 场景**。

### ProjectReference 场景脚本解析失败

- 被引用 class library 的 `GodotProjectDir` 是其自己的 csproj 目录（如 `../../SkilmeAI/GameOS/`）。
- source generator 生成的路径如 `res://Capabilities/...`，这个 `res://` 不在主游戏项目的 `res://` 空间里。
- `Main.cs:154` 只扫描主 assembly，被引用 assembly 的 `[ScriptPathAttribute]` 根本不会被登记。
- 结论：**ProjectReference 的 `.cs` 不能作为 Godot 脚本资源附加到场景节点**。

### 子场景同样受限

即使场景 A 引用场景 B（PackedScene），场景 B 的脚本仍走 `AddScriptBridge`（`csharp_script.cpp:2612`）同一条路径。不存在"子场景可以绕过"的例外。

### addons/ 源码级复用可行

如果把框架源码物理放到游戏项目的 `addons/SkilmeAI/` 或 `SkilmeAI/` 子目录下：
- 默认 `**/*.cs` glob 把这些 `.cs` 编译进**主 assembly**。
- source generator 基于主 csproj 的 `GodotProjectDir` 生成路径，如 `res://SkilmeAI/GameOS/GodotBridge/GodotEntity2D.cs`。
- `ScriptManagerBridge` 扫描主 assembly 时登记这些路径。
- `.tscn` 里 `ExtResource("Script", path="res://SkilmeAI/...")` 能正确解析。
- 结论：**源码级复用（submodule / subtree / 物理复制）是唯一可行的场景共享方式**。

## 改引擎评估

如果要让 Godot 支持"递归扫描 ProjectReference assembly + 统一 res:// 命名空间"：

| 文件 | 改动 |
|------|------|
| `ScriptManagerBridge.cs:300` | `LookupScriptsInAssembly` 改为递归扫描 `assembly.GetReferencedAssemblies()` |
| `ScriptPathAttributeGenerator.cs:97` | 路径计算改用"主项目的 GodotProjectDir"而非当前 csproj 的 |
| `Sdk.props` | 新增 MSBuild 属性传播 `MainGodotProjectDir` |
| `Main.cs:154` | 调用改为递归方法 |
| NuGet 打包 | 重新打包 `Godot.NET.Sdk` + `GodotSharp` |

真实成本不在 patch 本身，在于：
- 维护 fork：每次 Godot 升级都要 rebase modules/mono、source generator、Sdk.props。
- 跨父目录 `res://` 合法性：`RelativeToDir`（第 188-201 行）用 `Uri.MakeRelativeUri`，跨父目录时 `res://../../...` 在 `project_settings` 层会被截断，还要改资源系统。
- 所有用框架的游戏都要换成 fork runtime，NuGet 路线完全破产。

结论：**不改引擎**。

## 与 Unity UPM 的本质差异

| 维度 | Unity Packages/UPM | Godot 当前机制 |
|------|-------------------|---------------|
| 虚拟根 | `Assets/` + `Packages/` 统一虚拟根 | 每个项目独立 `res://` 根 |
| 场景共享 | Package 内 `.prefab` 自动进入 Assets 空间 | 不支持跨项目场景引用 |
| 代码共享 | `.asmdef` + UPM | `.csproj` + NuGet / ProjectReference |
| 框架做法 | UPM 包管理器（代码 + 场景双共享） | git submodule → 源码级（代码 + 场景双共享） |

Unity 之所以"做好了"，是因为从架构层就把 `Packages/` 纳入 Assets 虚拟根。Godot 的 `res://` 没有虚拟根概念，改引擎 = 把 UPM 搬进 Godot，是巨型 PR。

## 源码引用索引

| 功能 | 文件 | 行号 |
|------|------|------|
| 主 assembly 扫描 | `modules/mono/glue/GodotPlugins/Main.cs` | 154 |
| ScriptPath 生成 | `modules/mono/editor/Godot.NET.Sdk/Godot.SourceGenerators/ScriptPathAttributeGenerator.cs` | 97, 108-110, 188-201 |
| ScriptPath 登记 | `modules/mono/glue/GodotSharp/GodotSharp/Core/Bridge/ScriptManagerBridge.cs` | 300-344 |
| 查找失败点 | `modules/mono/csharp_script.cpp` | 2612 |
| `_pathTypeBiMap` 查询 | `ScriptManagerBridge.cs` | 443 |
| `res://` 根 | `core/config/project_settings.cpp` | 675, 820 |
| EditorFileSystem 扫描 | `editor/editor_file_system.cpp` | 246, 402 |
| 默认 GodotProjectDir | `modules/mono/editor/Godot.NET.Sdk/Godot.NET.Sdk/Sdk.props` | 15, 43 |
