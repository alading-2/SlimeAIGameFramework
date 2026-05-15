# 深度分析：AI-first GameOS 语言选型与 GDScript / GDExtension / TypeScript / Rust / C++ 评估

> 日期：2026-05-15（重构版）
> 状态：Architecture Decision Record — 修订版
> 主题：SlimeAI 作为 AI-first GameOS 框架，主语言应继续使用 C#，还是迁移到 GDScript、GDExtension 语言、TypeScript、Rust 或 C++。
> 结论强度：中高。基于 Godot 4.6.2 官方文档、社区项目实际状态、SlimeAI 266 个非生成 C# 文件的本地代码证据。

## 0. 最终决策

**SlimeAI 主框架语言继续使用 C#。**

```text
Core Runtime / Capability / DataOS runtime loader / Validation / Observation
  -> C#，继续作为主语言

Godot 场景胶水 / 一次性验证场景 / 编辑器内轻量交互
  -> 可允许 GDScript，但不作为框架核心契约语言

性能热点 / 第三方 native SDK / 引擎级能力
  -> 可局部引入 C++ GDExtension；Rust 仅在专项 spike 通过后引入

AI 辅助工具 / DataOS authoring 辅助 / codegen / 文档检查 / 可视化工具
  -> TypeScript 可优先考虑

Godot Runtime 主框架改写为 TypeScript
  -> Reject Now
```

但这个决策的前提和代价必须被充分理解：

- **C# Web 导出至今不支持**（Godot 4.6.2 官方文档明确说明）。如果 SlimeAI 未来需要 Web/H5 平台，这是一个结构性限制。
- **C# 在 Godot 中的长期架构可能变化**。Godot 官方路线图已将 "Deprecate .NET Godot builds for a .NET plugin" 列为 5.x 目标，未来 C# 将从内置 Mono/.NET hosting 转为 GDExtension 插件架构。
- **这些风险不能靠换语言自动消除**。换成 GDScript 不会解决 `res://` 跨项目共享问题；换成 TypeScript 目前 Godot 绑定层的稳定性不足以承载 266 个文件的生产框架。

一句话判断：**AI-first 不等于选 AI 最容易写的语法，而是选 AI 最容易被约束、验证、复盘和长期维护的执行环境。在当前 Godot 生态中，C# 仍是这个执行环境中折中风险最小的选项。**

---

## 0.1 核心问题：AI-first 框架选语言，到底在选什么？

最容易想错的地方是：**"AI 写哪个语言轻松"不是唯一标准，甚至不是第一标准。**

SlimeAI 不是一个脚本集合，它是一个让 AI **长期维护**的大框架。大框架最怕的不是"AI 写得慢一点"，而是：

- AI 改错字段，运行时才发现。
- AI 发错事件，场景跑起来才发现。
- AI 绕过 owner service，状态悄悄坏掉。
- AI 不知道某个逻辑归哪个 Capability。
- AI 改完以后没有一个快的命令证明"真的没坏"。
- AI 回来继续工作时，找不到上次改动的 owner 和验证入口。

所以选主语言时，最重要的问题不是：

```text
AI 写这个语言爽不爽？
```

而是：

```text
1. AI 写错时，谁能最快把它拦下来？
2. AI 下次回来时，能不能靠符号、类型、文档和测试快速找回正确入口？
3. 这个语言的生态是否能让 AI 在 Godot 外独立完成大部分验证？
4. 这个语言在 Godot 中的长期稳定性如何？官方是否承诺长期维护？
```

C# 的核心价值：**编译器、类型系统、泛型、record struct、.NET tests、Roslyn analyzer / source generator 潜力，都是 AI 的护栏。**

GDScript 的核心价值：**贴近 Godot，写场景、节点、Resource、Signal、Editor glue 非常快。**

这两个价值不冲突。正确分工是：

```text
C# 负责框架内核和长期契约：
  DataKey / Event / Entity / Schedule / Capability / DataOS loader / tests

GDScript 负责 Godot 贴身胶水：
  scene glue / editor button / validation scene / 小型工具
```

---

## 1. 判断标准

本 ADR 按 AI-first 框架需求排序，不按"人写起来舒服"排序。

| 标准 | 权重 | 为什么重要 |
| --- | --- | --- |
| 编译/类型反馈链长度 | 极高 | AI 最需要短反馈链；强类型错误越早暴露越好。`dotnet build` 失败比 Godot 场景崩溃更容易被 AI 修复。 |
| 离线验证能力 | 极高 | SlimeAI Core 应该在 Godot 编辑器外被 `dotnet test` 验证。 |
| 语言结构对框架契约的表达力 | 极高 | `DataKey<T>`、typed event、Capability owner 边界、命名空间隔离需要语言支持。 |
| Godot 官方支持等级 | 高 | 决定导出、调试、版本升级、编辑器支持和 AI 排错成本。 |
| Godot 集成深度 | 高 | 场景、资源、导出、调试、热重载和 headless 验证都受影响。 |
| 大框架组织能力 | 高 | 命名空间、模块、接口、泛型约束、测试、文档生成。 |
| 运行平台和导出链完整性 | 高 | 框架语言必须跟 Godot export pipeline 一起可靠工作。 |
| AI 路由稳定性 | 高 | 文件结构、符号名、类型系统、错误输出必须容易被 AI 定位。 |
| 生成器/分析器能力 | 中 | AI-first 需要自动生成、静态检查和统一事实源。目前是"潜力"而非"现实"。 |
| 跨项目复用 | 中 | `res://` 和脚本加载约束不因换语言自动消失。 |
| 迁移成本 | 中 | 当前已有约 266 个非生成 C# 文件，迁移会重写 Runtime、Capabilities、GodotBridge 和测试。 |
| AI 写代码轻松度 | 中低 | 是因素，但不是决定性因素。AI 写 TS 确实通常更顺手，但 Godot 绑定层会抵消这一优势。 |
| 长期维护风险 | 高 | 语言在 Godot 生态中的官方承诺和未来架构方向。 |

---

## 2. 本地证据：SlimeAI 不是普通游戏脚本集合

### 2.1 框架定位

[Evidence] `DocsAI/Framework/Overview.md` 已把 SlimeAI 定位为 `AI-first GameOS` 和 `Capability Composition Runtime`，不是传统 Godot 脚本工具包。

[Evidence] `DocsAI/GameOS/Contracts.md` 规定核心契约包括 Runtime Entity、Runtime Data / `DataKey<T>`、typed Event、Relationship、RuntimeSchedule、Capability、Validation 和 Observation。

### 2.2 代码规模与结构

当前源码中非生成 C# 文件约 266 个，覆盖：

- `GameOS/Runtime/`：Entity、Data、Event、Relationship、Schedule、Pool、Timer、Resource。
- `GameOS/Capabilities/`：Movement、Collision、Damage、Ability、Feature、AI、Attack、Projectile、Effect、Unit。
- `GameOS/GodotBridge/`：Godot Node 生命周期、Resource、Physics、Input、可视化实例化 adapter。
- `Tests/SlimeAI.GameOS.Tests/`：Runtime 测试和 EventBus 测试。

### 2.3 核心契约大量使用 C# 编译期特性

[Evidence] `DataKey<T>` 是框架最核心的状态契约机制：

```csharp
// GameOS/Runtime/Data/DataKey.cs:61
public sealed class DataKey<T> : IDataKey
{
    public string StableKey { get; }
    public int Id { get; }
    public T DefaultValue { get; }
    public Type ValueType => typeof(T);
    public Func<Data, T>? Compute { get; }
    // ... min/max clamping, option validation, numeric detection
}

// GameOS/Runtime/Data/DataKey.cs:195
public static DataKey<T> Create<T>(
    string stableKey,
    T defaultValue = default!,
    Enum? category = null,
    float? minValue = null,
    float? maxValue = null,
    bool isPercentage = false,
    bool supportsModifiers = false,
    // ...
)
```

[Evidence] `IData` 接口提供编译期类型安全的状态读写：

```csharp
// GameOS/Runtime/Data/Data.cs:39
public bool Set<T>(DataKey<T> key, T value)
public T Get<T>(DataKey<T> key)
public bool TryGet<T>(DataKey<T> key, out T value)
public T GetBase<T>(DataKey<T> key)
public bool Has<T>(DataKey<T> key)
```

**AI 的价值**：如果 AI 想修改 AI 目标实体，它写的是：

```csharp
entity.Data.Set(AIDataKeys.TargetEntity, targetId);
```

如果 AI 拼写错误为 `AIDataKeys.TaregtEntity`，编译器立刻失败。这是 AI 修复循环中成本最低的一类错误。

[Evidence] EventBus 使用泛型约束确保事件 scope 正确：

```csharp
// GameOS/Runtime/Event/IEventBus.cs:20
void Publish<T>(in T @event) where T : struct, IEvent;
IDisposable Subscribe<T>(Action<T> handler) where T : struct, IEvent;

// GameOS/Runtime/Event/EntityEventBus.cs:39
public void Publish<T>(in T @event) where T : struct, IEvent
{
    var eventType = typeof(T);
    if (!typeof(IEntityEvent).IsAssignableFrom(eventType))
    {
        Log.Error($"EntityEventBus rejected {eventType.Name}: " +
                  $"payload MUST implement IEntityEvent or IBroadcastEvent.");
        return;
    }
    // ... reentry guard, dispatch, broadcast routing
}
```

**AI 的价值**：事件类型错误在编译期被泛型约束拦截；事件 scope 错误（把 `IEntityEvent` 发到 WorldBus 或反之）在运行时通过 `IsAssignableFrom` 检查并记录到 observation。AI 可以从日志中精确定位问题。

[Evidence] `EntityId` 是值类型，避免 string 混用：

```csharp
public readonly record struct EntityId(int Value)
{
    public static readonly EntityId Empty = new(0);
    public bool IsEmpty => Value == 0;
}
```

**AI 的价值**：AI 不会把 `EntityId` 和 `string` 或 `int` 混用。编译器会拦住类型不匹配。

[Inference] SlimeAI 的核心价值已经不是"能在 Godot 中写玩法"，而是"让 AI 能稳定定位 owner、修改 typed contract、运行 CLI 验证并拿到 artifact"。语言选择必须服务这个闭环。

### 2.4 C# 优先不是历史惯性

[Evidence] `DocsAI/Framework/Principles.md` 已写明"C# 优先"：纯 Runtime、DataOS tooling、验证脚本和普通数据处理优先使用 C# 标准库与 SlimeAI API；只有 Node、SceneTree、Physics、Input、Resource、可视化实例化和场景生命周期进入 GodotBridge。

[Evidence] `DocsAI/Framework/GodotCSharpConstraints.md` 进一步说明：纯逻辑越少依赖 Godot，越容易用 `dotnet build` 和 Runtime tests 验证，也越不容易把 Godot 场景树误当玩法状态真相源。

[Inference] 对 AI-first 来说，C# 的价值不是"更像传统企业语言"，而是它把 Runtime 从 Godot SceneTree 中分离出来，让大部分逻辑能在 Godot 外通过 CLI 构建和测试。

---

## 3. Godot 官方语言边界与平台现实

### 3.1 官方路线

[Evidence] Godot 官方 FAQ 明确列出支持的语言：

```text
- GDScript：内置原生语言，官方主力。
- C#/.NET：官方支持，但不是内置语言（需要 .NET 运行时）。
- C++：通过 GDExtension 或 engine module。
- 其他语言：通过社区绑定 / GDExtension 接入。
```

[Evidence] Godot 4.6.2 官方文档（2026-03-31 发布）关于 C# 的说明：

> "C# projects **cannot currently be exported to the web platform**. For web platform support with C#, consider using Godot 3."

这是**截至 Godot 4.6.2 仍未解决**的限制。C# Web 导出在 Godot 4.0 发布后已缺失超过 3 年。

[Evidence] Godot 官方 2024 年文章说明 C# 移动端状态：

- Android：实验性支持（.NET 7+，linux-bionic Mono runtime，arm64/x64  only）
- iOS：实验性支持（NativeAOT，仅 macOS 导出，Xcode 构建）
- Web：**不支持**
- Desktop：完全支持

[Evidence] 2025 年 3 月 GodotCon Boston 展示了 C# Web 导出原型（PR #106125），但官方文章明确称之为：

> "still seems **very brittle** to me and has some limitations... The C# project must match the WASM features supported by the Godot template... We cannot commit to a specific timeline yet."

[Evidence] Godot 官方 2025 年文章确认 C# 包迁移到 .NET 8，并计划跟随 .NET 10 LTS（2025 年 11 月发布）。

### 3.2 C# 未来架构风险

[Evidence] Godot 官方 Priorities 页面列出长期目标：

> "Deprecate .NET Godot builds for a .NET plugin."

这意味着在 Godot 5.x 中，C# 可能从当前的 Mono/.NET hosting 架构迁移到 GDExtension 插件架构。这个迁移对 C# 在 Godot 中的集成深度、调试体验和导出稳定性可能有显著影响。

[Inference] 这不是"C# 会被移除"的风险，而是"C# 的存在形式可能变化"的风险。对 SlimeAI 这种大框架来说，这意味着：

1. 当前 `dotnet build` + `Godot.NET.Sdk` 的工作流可能变化。
2. C# 与 GDScript 的互调方式可能变化。
3. 导出链和调试链需要重新验证。

但这也可能是一个**正面变化**：如果 C# 通过 GDExtension 实现，理论上可以解决 Web 导出等当前限制。只是这个正面变化的时间表未知。

### 3.3 GDExtension 不是"任意语言无成本进入 Godot"

[Evidence] GDExtension 是 native extension 边界，不是 Godot 的第二套官方脚本系统。C++ 是最直接路径；Rust、JS/TS、Python 等一般依赖社区 binding 项目。

[Inference] GDExtension 适合"少量稳定 native 能力"，不适合把整个 AI-first GameOS Core 放进去。因为 Core 需要大量 DataKey、Event、Service、Tool、Validation、Observation 和测试协作；一旦主框架跨 GDExtension 边界，AI 要同时理解 Godot API、绑定层、native ABI、语言 runtime、导出配置和跨语言错误。

---

## 4. GDScript 深度分析：能承担大项目，但结构性不适合 SlimeAI Core

### 4.1 结论：不是"玩具语言"，但有结构性天花板

GDScript 不应被低估，也不应被高估。

**能做什么**：

[Evidence] GDScript 是 Godot 官方原生语言，深度集成 SceneTree、Node、Resource、Signal、Editor、Inspector 和即时运行流程。

[Evidence] Godot 官方文档提供 GDScript 静态类型系统，支持类型标注、`@abstract` 类、类继承、信号系统和编辑器集成。

[Evidence] Godot 社区有大量使用 GDScript 开发的中大型项目（Steam 上 2025 年新增 1200+ Godot 游戏）。

[Inference] "GDScript 只是入门级语言，做不了大框架"这个判断**不成立**。它可以组织大型游戏代码。

**不能做什么（对 SlimeAI Core 而言）**：

GDScript 缺乏以下对 AI-first 框架至关重要的语言结构：

#### 4.1.1 没有命名空间（Namespace）

[Evidence] Godot 官方提案 #1566 "Implement namespaces in GDScript" 是开放状态，尚未实现。

[Evidence] GDScript 使用 `class_name` 将类注册为全局名称，这意味着：

- 所有 `class_name` 名称在全局命名空间中竞争。
- 大型项目中容易出现命名冲突。
- 没有目录结构到命名空间的自动映射。

对 SlimeAI 的影响：当前 C# 代码使用 `SlimeAI.GameOS.Capabilities.AI`、`SlimeAI.GameOS.Capabilities.Movement` 等命名空间隔离 Capability。如果迁移到 GDScript，这些边界退化为目录约定，AI 更容易在全局名称中迷失。

#### 4.1.2 没有泛型（Generics）

[Evidence] Godot 官方 issue #30180 "Parameterized types / generics for GDScript" 是开放状态。

[Evidence] GDScript 的类型系统只有：

- 基本类型标注：`var health: int`、`func foo() -> String`
- Typed Array：`Array[int]`、`Array[Enemy]`
- Typed Dictionary：Godot 4.4+ 新增 `Dictionary[String, int]`
- 嵌套 typed collection：**不支持**（提案 #12224）

**没有的是**：

- 用户定义泛型类型：`DataKey<T>`、 `EventBus<T>`
- 泛型约束：`where T : struct, IEvent`
- 泛型方法：`Publish<T>(in T event)`

对 SlimeAI 的影响：`DataKey<T>` 是框架最核心、使用最频繁的类型。在 GDScript 中，这必须退化为：

```gdscript
# GDScript 模拟 DataKey 的方式
var key = "Movement.Position"  # string key
var value = entity.data.get(key)  # 返回 Variant，需要手动转换
```

或者使用 Typed Dictionary：

```gdscript
var data: Dictionary[DataKey, Variant]  # 丢失了 T 的类型信息
```

无论哪种方式，AI 失去了编译期对 "key 和 value 类型是否匹配" 的验证。这是 AI-first 框架的核心损失。

#### 4.1.3 接口系统弱于 C#

[Evidence] GDScript 支持 `class X extends Y` 继承和 `@abstract` 抽象方法，但没有接口（interface）的等价物。

GDScript 的抽象类和 C# 接口的区别：

- GDScript `@abstract` 要求继承；C# `interface` 允许多实现。
- SlimeAI 的 `IEventBus`、`IEntity`、`IDataChangeSink` 等契约在 GDScript 中只能变成抽象类或鸭子类型约定。
- 多实现缺失意味着 Capability 之间的正交组合更难表达。

#### 4.1.4 反射能力有限

[Evidence] GDScript 提供 `get_method_list()`、`has_method()`、`call()` 等反射 API，但远弱于 C# 的 `System.Reflection`。

SlimeAI EventBus 使用 C# `typeof(T)` 做事件类型路由：

```csharp
var eventType = typeof(T);
if (!typeof(IEntityEvent).IsAssignableFrom(eventType))
{
    // 拒绝错误 scope 的事件
}
```

在 GDScript 中，事件分发通常依赖 `StringName` 或 Signal 名称匹配，失去了类型级别的 scope 检查。

#### 4.1.5 循环依赖和加载问题

[Evidence] GDScript `preload()` 在编辑器中可能触发循环依赖死锁（GitHub issue #73983）。

[Evidence] GDScript 社区长期存在 `class_name` 全局类冲突和循环依赖问题（Reddit 多帖讨论）。

对 SlimeAI 的影响：266 个文件的框架中，Capability 之间的引用关系复杂。C# 的命名空间 + 项目引用系统可以优雅处理这些依赖。GDScript 的 `preload()`/`load()`/`class_name` 系统在大规模下更容易出问题。

### 4.2 GDScript 的优势（对 AI 的价值）

| 优势 | 对 AI 的价值 |
| --- | --- |
| 无需编译，保存即可运行 | 反馈短，适合场景验证和玩法试验。 |
| Godot 原生概念一致 | AI 不需要在 C# 命名约定和 Godot API 之间转换。 |
| 编辑器集成强 | 导出变量、signal、resource、scene glue 很轻。 |
| 脚本类和静态类型可用 | 比完全动态语言更可维护。 |
| 运行时加载轻 | 小功能、小验证场景和游戏侧 glue 成本低。 |

### 4.3 GDScript 的限制（对 SlimeAI 的具体影响）

| 限制 | 对 SlimeAI 的影响 |
| --- | --- |
| **无 namespace** | 266 个文件的全局命名空间竞争；Capability 边界依赖目录约定而非语言强制。 |
| **无泛型** | `DataKey<T>`、typed `Publish<T>`、`EntityId` 值类型等核心契约无法在编译期保证类型安全。 |
| **无接口多实现** | Capability 之间的正交组合和契约隔离变弱。 |
| **类型系统弱于 C#** | AI 写错跨模块契约时，更多错误会延迟到运行时或场景测试。 |
| **缺少 .NET 级 Source Generator / analyzer** | DataOS -> typed contract 自动生成和静态规则检查会更弱。 |
| **大型离线测试和工具链生态弱于 .NET** | Runtime tests、CLI validation、schema codegen、artifact 分析不如 C# 顺手。 |
| **循环依赖风险** | `preload()`/`class_name` 在大项目中更容易触发编辑器死锁。 |
| **跨项目复用仍受 `res://` 限制** | 换成 GDScript 不会自动解决 Godot 场景/脚本共享边界。 |

### 4.4 真正的问题：GDScript 的错误反馈太靠后

GDScript 最大的优势是"快写、快跑"。这对人类调玩法很好，对 AI 写小功能也很好。但 SlimeAI Core 的问题不是"能不能跑起来"，而是"AI 写错以后，错误在哪一层被发现"。

对 AI 来说，错误越晚发现，成本越高：

```text
最理想：
  写代码 -> 编译失败 -> AI 立刻修

可接受：
  写代码 -> 单元测试失败 -> AI 看断言修

成本高：
  写代码 -> Godot headless 场景失败 -> AI 查日志修

最差：
  写代码 -> 游戏运行一段时间状态错了 -> AI 很难定位是谁改坏的
```

C# 更容易把 SlimeAI Core 的错误放在前两层。GDScript 更容易把很多框架契约错误推到后两层。

具体到 SlimeAI：

| SlimeAI 核心契约 | C# 的错误发现位置 | GDScript 常见错误发现位置 |
| --- | --- | --- |
| `DataKey<T>` 类型写错 | 编译期 / Runtime test | 运行时 Variant 转换失败 / 场景日志 |
| 事件 payload 参数错 | 编译期（泛型约束） | Signal emit / handler 运行时 |
| `EntityId` 和 string 混用 | 编译期 | 运行时逻辑漂移 |
| Capability owner 调错 | 可用 analyzer / build / tests 收敛 | 更多依赖约定和人工检查 |
| DataOS snapshot 类型漂移 | validator + typed loader | validator 可以做，但运行时类型入口弱 |

### 4.5 GDScript 适合写哪一层

GDScript 很适合贴着 Godot 的层：

```text
Godot Node / Scene / Resource / Signal / Editor
```

这层的特点是：本来就需要 Godot 才能验证；代码和场景、Inspector、Resource 强绑定；逻辑通常比较薄；错了也容易通过场景测试看到。

SlimeAI Core 不属于这层。Core 的特点是：大量逻辑不应该依赖 Godot；应该可以 `dotnet test` 验证；状态契约要跨 Capability 复用；事件、DataKey、Entity、Schedule 要长期稳定；AI 每次修改都要知道 owner 和验证命令。

### 4.6 GDScript 决策边界

**Adopt Now**：

- Godot 验证场景里的最小脚本。
- 游戏侧 scene glue。
- 编辑器内调试按钮、临时 inspector 辅助。
- 不进入 Core Runtime 的轻量工具。

**Adopt Later**：

- DataOS 可视化编辑器的 Godot EditorPlugin 层。
- 用 GDScript 做薄 UI 层，调用 C# Runtime API。

**Reject**：

- 把 `GameOS/Runtime`、`GameOS/Capabilities`、`DataOS runtime loader` 全量改写为 GDScript。
- 用 GDScript dynamic Dictionary / Variant 取代 `DataKey<T>` 作为主状态契约。
- 用 Godot Signal / string event 取代 typed EventBus。

---

## 5. C# 继续作为主语言的理由与真实风险

### 5.1 C# 对 AI 的关键优势不是语法，而是护栏

| 能力 | C# 表现 | AI-first 价值 |
| --- | --- | --- |
| 强类型泛型 | `DataKey<T>`、typed `Publish<T>`、record struct payload | 错误早暴露，AI 修复链路短。 |
| 编译器错误 | `dotnet build` 输出明确 | CLI 可重复，适合 agent loop。 |
| 测试生态 | 普通 .NET 测试无需启动 Godot | 大量 Runtime 能快速验证。 |
| Namespace / 接口 / 抽象类 | 成熟的多层组织机制 | Capability owner 边界清晰，AI 路由稳定。 |
| 反射 | `System.Reflection` 完整 | EventBus 类型路由、Observation 数据导出依赖此能力。 |
| Analyzer / Source Generator | Roslyn 成熟 | **潜力**：可把 DataOS 规则前移到编译期（尚未实现）。 |
| 标准库 | JSON、文件、集合、反射、时间、随机、CLI 都成熟 | DataOS 和验证工具不依赖 Godot。 |
| Godot 官方支持 | 官方 .NET 工作流 | 低于 GDScript 的集成深度，但高于所有社区语言。 |
| AI 训练数据 | C# 代码和错误模式丰富 | AI 能稳定修复常见编译错误。 |

[Inference] 对 SlimeAI，强类型不是人类洁癖，而是 AI 的执行护栏。AI 写错 `DamageDataKeys.CurrentHp`、事件 payload 或 `EntityId` 类型时，C# 编译器能在场景运行前拦住一批错误。

### 5.2 六个硬理由

#### 理由 1：C# 把"框架规则"变成编译器能检查的东西

SlimeAI 很多规则不是建议，而是不能破坏的契约：

```text
Damage.CurrentHp 必须是 float。
AI.TargetEntity 必须是 EntityId?。
事件必须声明 IEntityEvent / IGlobalEvent / IBroadcastEvent scope。
Runtime Entity Id 不能随手拿 string 混用。
Capability 应该通过 service/tool/handler 修改状态。
```

在 C# 里，这些规则可以变成类型、泛型、record struct、接口和 analyzer。

在 GDScript 里，也可以写类型标注和工具检查，但很多规则更容易退化成"约定"。约定对人还可以，对 AI 不稳定。AI 会遵守文档，但 AI 更需要编译器帮它记住文档。

#### 理由 2：C# 让 AI 可以脱离 Godot 验证大部分逻辑

SlimeAI Core 里很多东西本来不需要 Godot：

- Data slot。
- EventBus。
- Ability 冷却。
- Damage 结算。
- Movement 数学。
- AI 行为树。
- Projectile 命中生命周期。
- DataOS snapshot loader。

这些如果用 C#，AI 可以跑：

```bash
Tools/run-build.sh
Tools/run-tests.sh
```

不需要打开 Godot，不需要等场景，不需要看一大堆引擎日志。

如果 Core 全用 GDScript，大量验证会变成 Godot headless scene。headless scene 很重要，但它比普通 Runtime test 重，失败日志也更难定位。

所以 C# 的价值是：**让框架内核变成普通软件工程，可以被普通 CLI 快速验证。**

#### 理由 3：C# 的符号更适合 AI 搜索和路由

AI 修改代码前，第一件事通常是搜索。

C# 符号很稳定：

```text
DamageDataKeys.CurrentHp
AbilityService.TryTrigger
ProjectileTool.Spawn
IEventBus.Publish<T>
RuntimeSchedule.Register
```

AI 可以直接 `rg "TryTrigger"`、`rg "CurrentHp"` 找到 owner。

GDScript 也能搜索，但大型项目里更容易出现：

```text
"current_hp"
"CurrentHp"
&"current_hp"
Dictionary key
signal name
NodePath
Resource path
```

这些不是 GDScript 写不了，而是 AI 更容易搜漏、搜错、改错 owner。

#### 理由 4：C# 更适合 DataOS 的"单源生成 + 强校验"

SlimeAI 的长期方向不是手写一堆字段，而是：

```text
DataOS DB / schema
  -> generator
  -> C# DataKey<T>
  -> validator
  -> runtime snapshot
```

C# 有成熟的 Roslyn Source Generator 和 analyzer，可以把"AI 不能乱写 DataKey""DB 和 C# 不能漂移"变成工具链规则。

**重要声明**：SlimeAI 目前**尚未实现任何 Source Generator**。这是"潜力"而不是"已验证的优势"。选 C# 不是因为它已经有 generator，而是因为 generator 生态成熟，实现路径清晰。如果 SlimeAI 已经实现了 generator，这个理由会更强。

GDScript 可以做生成代码，但生态没有 C# 这条链强。TS 也很适合做生成器，所以 TS 应该进入工具层；但生成出来的 Runtime typed contract 仍然建议落到 C#。

#### 理由 5：C# 更容易隔离 Godot

SlimeAI 的原则是：

```text
Godot 是宿主和表现层，不是玩法状态真相源。
```

C# 当前做法是：

```text
Core Runtime:
  不依赖 Node，不依赖 SceneTree，不直接用 Godot 物理。

GodotBridge:
  专门负责 Node / Resource / Physics / Input / Visual。
```

这种隔离对 AI 很重要。AI 不会每次改 Damage、Ability、AI 时都掉进 Godot 节点树。GDScript 太贴近 Godot，这是优势，也是风险：AI 更容易把 runtime 状态和 node 状态混在一起。

#### 理由 6：C# 的坏处可以被工程手段修，GDScript 的核心短板更难补

C# 的主要坏处：

- 编译慢一点。
- 写法比 GDScript 重。
- Editor 贴合度不如 GDScript。
- Web 导出不支持。

前三个可以通过分层、测试、watch、generator、GodotBridge、少量 GDScript glue 缓解。Web 导出是一个真实限制，但：

1. 它对当前 SlimeAI（桌面/移动端优先）不是 immediate blocker。
2. Godot 官方正在积极解决（PR #106125）。
3. 即使 web 导出一直不支持，SlimeAI Core 的纯 C# 逻辑仍然可以被 Godot 3 或其他宿主复用。

GDScript 作为 Core 的主要坏处：

- 类型护栏弱一些。
- 无 namespace，无泛型。
- 大量框架规则依赖约定。
- 离线测试和 analyzer 生态弱。
- AI 错误更容易推迟到场景运行时。

这些更难补，因为它们正好打在 AI-first 框架最需要的地方。

### 5.3 C# 的真实风险（必须被承认）

| 风险 | 严重度 | 说明 |
| --- | --- | --- |
| **Web 导出缺失** | 高 | Godot 4.x 中 C# 项目无法导出 Web。Godot 官方正在解决但无时间表。 |
| **未来架构迁移（GDExtension plugin）** | 中 | Godot 5.x 可能将 C# 从内置转为 GDExtension。影响集成深度和工作流，但不是"C# 被移除"。 |
| **移动端实验性支持** | 中 | Android/iOS 为实验性，NativeAOT/Mono 有已知限制。 |
| **社区使用率低** | 低 | 2025 Godot 社区调查仅 ~16% 开发者使用 C#。意味着 C# 专属教程、插件、工具比 GDScript 少。 |
| **编译反馈慢于 GDScript** | 低 | 可通过增量 build、watch、分层测试缓解。 |
| **Godot C# 脚本共享有 `res://`/assembly 约束** | 低 | 已采用源码级 submodule/物理嵌入；换语言不自动解决。 |

### 5.4 C# 劣势的处理方式

| 劣势 | 处理方式 |
| --- | --- |
| 编译反馈慢于 GDScript | 优化 `dotnet build`、增量测试、watch、专项 test wrapper。 |
| Godot C# 脚本共享有 `res://`/assembly 约束 | 已采用源码级 submodule/物理嵌入；换语言不自动解决。 |
| C# 与 Godot API 命名差异 | GodotBridge 封装引擎边界，Runtime 不直接依赖 Godot 类型。 |
| DataKey 与 DB descriptor 可能双源漂移 | DataOS validator + 后续 Source Generator。 |
| 编辑器侧体验弱于 GDScript | 只在编辑器插件/UI glue 层引入 GDScript 或 TS 工具。 |
| Web 导出缺失 | 监控 PR #106125；如 web 是硬需求，评估 Godot 3 降级或 WASM 专项方案。 |

---

## 6. TypeScript 重点分析：语言优势真实，Godot 绑定成本也真实

### 6.1 结论：TS 是优秀工具语言，不是当前 Godot Runtime 主语言

用户直觉成立：AI 写 TypeScript 通常很轻松。TS 的类型系统、npm 生态、AST tooling、代码生成、CLI 开发、JSON/schema 处理都非常适合 AI。

但 Godot Runtime 的主语言决策不是普通 Node.js 项目选型。关键问题是：

```text
TypeScript 语言优势
  vs
Godot 非官方绑定 + JS runtime + GDExtension 边界 + 导出/调试/版本兼容风险
```

在 SlimeAI 当前阶段，后者权重大于前者。

### 6.2 TS 的真实优势

| 优势 | 说明 |
| --- | --- |
| AI 熟悉度极高 | AI 生成、重构、解释 TS 的稳定性通常很好。 |
| 工具链强 | npm、ts-morph、zod、json-schema、eslint、prettier、vitest 很适合 DataOS 和文档工具。 |
| 类型系统表达力强 | union、literal type、mapped type、schema inference 很适合 authoring schema。 |
| 反馈快 | `tsc --noEmit`、lint、unit test 反馈通常快。 |
| 生态适合 codegen | 对 SQLite/JSON/Markdown/CLI/网页 UI 非常顺手。 |
| 运行时 schema 验证 | Zod / io-ts 等库可以在运行时保持类型安全，与 DataOS 校验需求天然契合。 |

这些优势应被利用，但位置应是：

- DataOS authoring assistant。
- schema -> doc / code generator。
- DocsAI consistency checker。
- 可视化编辑器的外部 Web UI。
- Godot 项目外的 AI 操作台。

### 6.3 TS 进入 Godot Runtime 的风险

| 风险 | 影响 |
| --- | --- |
| **非官方主线** | Godot 官方文档没有把 TypeScript 列为官方脚本语言。 |
| **绑定层复杂且不稳定** | 需要 JS runtime、Godot API binding、GDExtension glue、类型声明同步。 |
| **GodotJS 已知有严重构建问题** | GitHub issue #76："Codegen doesn't work"。用户报告"V8、QuickJS、quickjs-ng 都有编译问题"。 |
| **导出链不确定** | 每个平台都要验证 JS runtime 和 native extension export。 |
| **调试链更长** | 错误可能来自 TS、transpile、JS runtime、binding、GDExtension、Godot。 |
| **类型运行时擦除** | TS 类型不能直接保证 Godot runtime 边界类型安全。但 C# 在 Godot marshal 边界也有类似问题。 |
| **与 Godot scene/script 资源关系要验证** | `.tscn` 挂脚本、资源引用、热重载、editor reload 等都需专项验证。 |
| **现有 C# 迁移成本极高** | 需要重写 Runtime、Capabilities、GodotBridge、测试和 DataOS runtime loader。 |

### 6.4 GodotJS / godot-ts 的真实状态

[Evidence] GodotJS GitHub README 说明：

> "The core functionality is implemented and essentially usable but **still under testing**... Support for multiple javascript engines (v8, quickjs, quickjs-ng, JavaScriptCore, the host Browser JS)"

[Evidence] GodotJS issue #76（标题"Codegen doesn't work"）：

> "I have issues compiling with V8, with quickjs, and with quickjs-ng. I have made several PRs trying to make things work but it feels like whenever I try using this again there are always issues compiling or using it."

[Evidence] GodotJS issue #122："Issues related with quickjs"（Open，2025-10-13）。

[Evidence] QuickJS 性能远低于 V8（20-100x slower）。

[Inference] **GodotJS 不是"需要 spike 验证"，而是"已知有严重构建和稳定性问题"**。任何声称"TS 可以进入 Godot Runtime"的评估，如果不承认这些已知问题，是不负责任的。

### 6.5 TypeScript 决策

**Adopt Now**：

- DataOS / DocsAI / codegen / validation 辅助工具优先考虑 TS。
- 可用 TS 做外部 authoring Web UI、schema checker、文档链接检查。

**Spike（仅限工具层，不进入 Runtime）**：

- 验证 TS 生成 DataOS docs、schema diff、DocsAI 链接检查、snapshot 可视化。
- 验收：`npm test` 或项目选定 CLI test。

**Reject Now**：

- 用 TS 重写 `GameOS/Runtime`。
- 用 TS 作为 Capability 主语言。
- 用 TS 替换 C# typed DataKey/Event contract。
- 在 GodotJS 当前状态下，任何生产级别的 Runtime 集成。

---

## 7. Rust / C++ / GDExtension 多语言分析

### 7.1 C++

C++ 是 GDExtension 最直接、最官方语境的 native 选择。

**适合**：

- 性能热点。
- 引擎 API 深度扩展。
- 第三方 C/C++ 库绑定。
- 需要最少中间层的 native capability。

**不适合**：

- AI-first 主框架逻辑。
- DataOS authoring/runtime contract。
- 高频变更的 Capability gameplay 规则。

[Inference] C++ 的 AI 可写性、内存安全、构建速度和错误诊断都不如 C# / TS。除非有明确性能或 native SDK 需求，否则不要把主框架放进 C++。

### 7.2 Rust

Rust 的类型系统和内存安全非常强，`godot-rust/gdext` 是 Godot 4 GDExtension 生态的重要项目。

[Evidence] Reddit 讨论（2026）确认："The bindings are definitely ready for production, we have been using it at work for a while now and we rarely come across bugs that are related to the bindings themselves."

[Evidence] godot-rust 2026 年 3 月 dev update 显示项目活跃，API 持续改进。

**适合**：

- 高性能算法。
- 复杂数据结构。
- 原生库复用。
- 对内存安全要求高的 native 扩展。

**风险**：

- Godot 绑定是社区项目，不是 Godot 官方主语言。
- 编译和生命周期/trait 错误对 AI 反馈较重。
- 与 Godot 对象生命周期和 GDExtension ABI 的交互需要长期维护。
- AI 能写 Rust，但复杂 Rust + Godot binding 的修复成本高。

**决策**：

- Rust 可作为专项性能扩展候选（成熟度高于 GodotJS）。
- 不作为 SlimeAI Core Runtime 主语言。

### 7.3 其他 GDExtension 语言（简要）

| 语言 | 绑定项目 | 成熟度 | SlimeAI 适用性 |
| --- | --- | --- | --- |
| Python | godot-python | 社区维护，活跃度一般 | 不适合 Core。Python 的动态类型与 SlimeAI 的 typed contract 目标冲突。 |
| Lua | 多个社区绑定 | 碎片化 | 不适合 Core。Lua 极简语义不利于表达复杂框架契约。 |
| TS/JS | GodotJS | **已知不稳定** | 见 6.4 节。 |

### 7.4 GDExtension 总体边界

**Adopt**：

- `NativeAcceleration`：路径搜索、空间查询、批量数值计算、外部 SDK。
- `NativeBridge`：框架需要绑定不可避免的 native 库时。

**Reject**：

- 把 Capability owner 逻辑迁到 GDExtension。
- 把 DataOS runtime loader 迁到 GDExtension。
- 为了"支持更多语言"或"AI 写起来轻松"而引入 GDExtension。

GDExtension 应该是能力扩展点，不是主框架语言逃生舱。

---

## 8. 修正后的对比矩阵

| 维度 | C# | GDScript | TS via GodotJS | Rust GDExtension | C++ GDExtension |
| --- | --- | --- | --- | --- | --- |
| Godot 官方支持 | **中高** | **最高** | 低/社区且不稳定 | 中/社区 | 高/native |
| 编译/类型护栏 | **高** | 中 | 中高但运行时擦除 + 绑定层风险 | **最高** | 高但手工成本高 |
| CLI 离线构建/测试 | **高** | 中低 | 高但 Godot 绑定另算 | 中 | 中 |
| 大框架组织能力 | **高** | 中（无 namespace、无泛型） | 高 | 高 | 高 |
| AI 写代码轻松度 | 高 | 中 | **最高** | 中 | 中低 |
| Godot 编辑器集成 | 中高 | **最高** | 低/不稳定 | 中 | 中 |
| DataOS/codegen 适配 | **高** | 中低 | **最高**（工具层） | 中 | 中低 |
| 运行平台完整性 | **中**（缺 Web） | **最高** | 低 | 中 | 高 |
| 迁移成本 | 0 | 极高 | 极高 | 极高 | 极高 |
| 运行性能 | 高 | 中 | Unknown（QuickJS 慢 V8 20-100x） | 高 | **最高** |
| 长期维护风险 | **中**（5.x 架构迁移） | **低** | **高**（绑定层不稳定） | 中高 | 中高 |
| SlimeAI Core 适合度 | **最优** | 不推荐 | 当前不推荐 | 不推荐 | 不推荐 |
| SlimeAI 工具层适合度 | 高 | 中 | **最优** | 低 | 低 |

**关键修正说明**（相比原 ADR）：

1. **C# Godot 官方支持从"高"改为"中高"**：承认 C# 在 Godot 中的官方支持低于 GDScript，Web 导出缺失是结构性限制。
2. **C# 运行平台完整性从"高"改为"中"**：明确标注 Web 不支持。
3. **C# 长期维护风险从"中"保留为"中"**：不低估 Godot 5.x 架构迁移风险。
4. **GDScript 大框架组织能力从"中"细化说明**：明确标注"无 namespace、无泛型"是结构性限制。
5. **TS via GodotJS 运行性能标注**：QuickJS 慢 V8 20-100x，性能不可预测。
6. **TS via GodotJS 维护风险升级为"高"**：基于已知的严重构建问题。

---

## 9. 语言分层建议

```text
SlimeAI Language Policy

1. Framework Core
   C#
   - Runtime Entity / Data / Event / Relationship / Schedule
   - Capability service / tool / handler
   - DataOS runtime snapshot loader
   - Observation / Validation contracts
   - Runtime tests

2. GodotBridge
   C# first
   - Node / SceneTree / Resource / Physics / Input adapter
   - 允许极少量 GDScript 做场景验证或 editor glue

3. Authoring / Tooling
   C# or TypeScript
   - DataOS generator / validator 可继续 C#
   - 外部 UI、schema/doc/codegen 可优先 TS
   - 文档检查、链接验证、snapshot 可视化优先 TS

4. Native Extensions
   C++ first, Rust by spike
   - 只做性能热点和第三方 native 库
   - 不持有 gameplay owner

5. Experiments
   TypeScript / Rust / GDScript
   - 任何进入主线前必须有 OpenSpec + headless scene artifact + export 验证
   - GodotJS 在已知构建问题解决前，禁止进入任何主线
```

---

## 10. 对 DataOS 和单源事实的影响

语言不应改变 DataOS 的核心方向。

当前正确方向仍是：

```text
Authoring DB / seed / migration
  -> validator
  -> generated snapshot / generated typed code
  -> C# Runtime DataKey<T> / DataCatalog
  -> Runtime tests + Godot scene artifacts
```

TS 可以增强这个链路：

- 生成 schema docs。
- 生成 DataOS 编辑 UI。
- 辅助检查 DocsAI 与 schema 漂移。
- 做 authoring assistant。

但 TS 不应该绕过 Runtime typed contract：

```text
Reject:
TS object / JSON dictionary
  -> Godot runtime directly reads and mutates gameplay state
```

原因是 AI 会失去 C# 编译器和 Runtime tests 的第一层护栏。

---

## 11. 对 GDScript 的公正判断

需要明确纠正两个极端判断。

### 11.1 "GDScript 是玩具语言"是错的

Godot 官方长期把 GDScript 作为主力脚本语言。它有静态类型（可选）、`@abstract`、文档注释、编辑器集成、Resource/Signal/Scene 深度支持。一个组织良好的 Godot 项目完全可以用 GDScript 写出大量可维护 gameplay 逻辑。

### 11.2 "GDScript 原生所以最适合 SlimeAI Core"也是错的

SlimeAI Core 不是普通 gameplay 脚本层。它需要：

- typed `DataKey<T>` 作为状态契约。
- typed event payload + scope 约束（`IEntityEvent` / `IGlobalEvent` / `IBroadcastEvent`）。
- pure runtime tests（无需 Godot）。
- DataOS validator。
- source generator / analyzer 潜力。
- AI 路由到 Capability owner（依赖 namespace + 接口）。
- GodotBridge 与 Runtime 分离。

这些目标 C# 更强，不是因为 C# 语法更好，而是因为 C# 的类型系统、泛型、命名空间、反射和 .NET 测试生态**在语言层面支持**这些需求。GDScript 不是做不到，是**在语言层面缺少表达这些契约的工具**，导致更多依赖运行时检查和约定。

最终判断：

| 问题 | 答案 |
| --- | --- |
| GDScript 能不能做大项目？ | 能。 |
| GDScript 能不能做框架？ | 能，但要非常严格地组织目录、类型、资源和测试，且缺少 namespace 和泛型会带来结构性成本。 |
| GDScript 是否适合 SlimeAI Core？ | 不如 C#。不是因为 GDScript 弱，而是因为 SlimeAI Core 的需求在 C# 中有更好的语言支持。 |
| GDScript 在 SlimeAI 是否有位置？ | 有，适合 scene glue、验证场景、editor plugin UI 薄层。 |

---

## 12. 关键风险与缓解

### 12.1 C# Web 导出缺失

**风险**：如果 SlimeAI 框架或基于 SlimeAI 的游戏需要 Web/H5 平台，C# 当前无法导出。

**缓解**：

1. 监控 Godot PR #106125（C# Web 导出原型）。
2. 如 web 是硬需求，评估 Godot 3 降级方案（Godot 3 支持 C# Web 导出）。
3. SlimeAI Core 的纯 C# 逻辑不依赖 Godot，理论上可被其他宿主（如自定义 WASM runtime）复用。
4. 在 ADR 中明确记录此风险，不假装它不存在。

### 12.2 C# 未来架构迁移（Godot 5.x GDExtension plugin）

**风险**：Godot 5.x 可能将 C# 从内置 Mono 架构迁移到 GDExtension 插件架构，改变工作流和集成深度。

**缓解**：

1. 这是一个"变化"风险，不是"移除"风险。C# 不会被 Godot 移除。
2. 保持 Core Runtime 与 Godot 的隔离（当前已做到），减少架构迁移的影响面。
3. 持续关注 Godot 官方路线图和 C# GDExtension 讨论（GitHub proposals #7895）。

### 12.3 TypeScript 绑定层不成熟

**风险**：如果过早将 TS 引入 Runtime，GodotJS 的不稳定性可能破坏框架可靠性。

**缓解**：

1. TS 严格限制在工具链层。
2. 任何 GodotJS spike 必须通过 headless scene + export 验证才能进入主线。
3. 在 GodotJS 已知构建问题解决前（issue #76 等），禁止生产使用。

### 12.4 GDScript 类型系统不足以表达 Core 契约

**风险**：如果未来有人提议"把 Core 改写为 GDScript"，ADR 的证据需要能被快速引用。

**缓解**：

1. 本 ADR 第 4.1 节详细列出了 GDScript 的结构性缺失（无 namespace、无泛型、无接口多实现）。
2. 在 Framework Principles 中增加"Core Runtime 语言变更需通过 Architecture Review"。
3. 定期（每季度）review 本 ADR，补充新的语言生态证据。

---

## 13. 后续 OpenSpec 候选

### 13.1 `language-policy-for-slimeai`

目标：

- 把本 ADR 的语言分层写入 `DocsAI/Framework/Principles.md`。
- 在 `DocsAI/Agent/Protocols/AIFeatureDevelopmentProtocol.md` 增加"默认 C#，例外需说明"的门禁。
- 在 owner skills 中加入 GDScript / TS / GDExtension 例外条件。

验收：

- `rg -n "Language Policy|TypeScript|GDScript|GDExtension" DocsAI .ai-config/skills`
- `Tools/run-build.sh`
- `Tools/run-tests.sh`

### 13.2 `typescript-tooling-spike`

目标：

- 只做工具层，不进 Godot Runtime。
- 评估 TS 生成 DataOS docs、schema diff、DocsAI 链接检查、snapshot 可视化。

验收：

- `npm test` 或项目选定 CLI test。
- 生成 artifact 放入 `.ai-temp/`，不污染长期事实源。

### 13.3 `godotjs-runtime-evaluation`（注意：不是"spike"，是"评估是否已可用"）

目标：

- 重新评估 GodotJS 在 Godot 4.6.2 下的稳定性（关注 issue #76 等关键 bug 是否已修复）。
- 如已修复，建立 `experiments/godot-ts-runtime-spike`，验证：Godot 4.6.2 启动、脚本挂载、headless scene、export、与 C# bridge 互调。
- 如未修复，维持 Reject。

硬门槛：

- 不通过 export/headless 验证，不进入主框架。
- 不替换现有 C# Runtime。

### 13.4 `gdscript-editor-glue-policy`

目标：

- 允许 GDScript 进入 EditorPlugin / validation scene glue。
- 禁止 GDScript 持有 Core Runtime state truth。

验收：

- 新增 GDScript 文件必须能被 `rg` 路由到 owner 文档。
- 每个 GDScript glue 都要说明它调用哪个 C# Runtime API。

### 13.5 `csharp-source-generator-roadmap`（新）

目标：

- 把"Source Generator 潜力"变成"Source Generator 现实"。
- 设计第一个 Roslyn Source Generator：从 DataOS schema 生成 `DataKey<T>` 和 validator 规则。

验收：

- 实现一个最小可用 generator。
- 集成到 `Tools/run-build.sh`。
- AI 可以通过修改 schema 文件，让 generator 自动更新 C# 代码。

---

## 14. Reject List

| Reject | 原因 |
| --- | --- |
| 现在把 SlimeAI Core 从 C# 改为 GDScript | 迁移成本极高；丢失泛型 `DataKey<T>`、namespace、.NET typed/testing/codegen 优势；GDScript 无 namespace 和泛型是结构性限制。 |
| 现在把 SlimeAI Core 改为 TypeScript | Godot TS（GodotJS）不是官方主线，已知有严重构建问题（issue #76）；绑定和导出风险未验证。 |
| 现在把 SlimeAI Core 放入 Rust/C++ GDExtension | AI 修改难度、构建复杂度和 native ABI 风险过高。 |
| 用 dynamic Dictionary / JSON object 替代 `DataKey<T>` | 破坏 AI-first typed contract。 |
| 用 Godot Signal/string event 替代 typed EventBus | 事件 owner、payload、scope 和 reentry guard 会变弱。 |
| 只因为"AI 写 TS 更快"就迁移 Runtime | 忽略 Godot host 和验证链路，是局部优化。 |
| 只因为"GDScript 无需编译"就迁移 Runtime | 无编译不是无验证；大型框架更需要静态护栏。GDScript 的无编译优势在 SlimeAI Core 层是劣势。 |
| 为了语言多样性引入 GDExtension | GDExtension 只应解决具体 native 问题。 |
| 假装 C# Web 导出不是问题 | 必须承认此限制，监控 PR #106125，在需要 Web 时评估替代方案。 |
| 在 GodotJS 已知构建问题解决前引入 TS Runtime | 当前 GodotJS issue #76 等关键问题未解决，生产使用不负责任。 |

---

## 15. Unknown / 待验证

| Unknown | 降低不确定性的验证 |
| --- | --- |
| Godot PR #106125（C# Web 导出）何时合并 | 持续监控；每季度 review。 |
| Godot 5.x C# GDExtension plugin 的具体影响 | 等待官方路线图更新；关注 proposals #7895。 |
| GodotJS issue #76 等关键 bug 何时修复 | 每季度检查 GodotJS 仓库状态。 |
| C# Source Generator 在 SlimeAI 中的 ROI | 通过 `csharp-source-generator-roadmap` spike 验证。 |
| GDScript 静态类型在 SlimeAI 风格 DataKey/Event 上的表达力上限 | 写一个 GDScript-only mini runtime，对比 C# tests 和错误输出。 |
| Rust GDExtension 与当前 GodotBridge 生命周期冲突 | 只在性能热点 spike 中验证，不进入 Core。 |
| C# 编译反馈能否压到 AI 可接受速度 | 增量 build/test、watch、analyzer、分层测试计划。 |

---

## 16. 最终摘要

| 问题 | 结论 |
| --- | --- |
| AI-first 是否应该选择 AI 最会写的语言？ | **不完全**。应选择 AI 最容易被约束、验证和复盘的语言/工具链。在当前 Godot 生态中，这个语言仍是 C#。 |
| 是否继续 C#？ | **是**，Core Runtime 继续 C#。但 Web 导出缺失和未来架构迁移是必须承认的风险。 |
| GDScript 能否承担大框架？ | **能承担大项目/框架**，但 SlimeAI Core 需要的 namespace、泛型、接口多实现、纯 runtime tests 在 GDScript 中要么缺失要么退化为约定。 |
| GDScript 是否只是入门语言？ | **不是**。这个判断应废弃。但它有结构性天花板（无 namespace、无泛型）。 |
| TypeScript 是否值得重视？ | **非常值得**，优先用于工具链和 authoring；Runtime 需等待 GodotJS 稳定性成熟。 |
| GodotJS 当前是否可用？ | **已知有严重构建问题**（issue #76），不是"需要 spike"，而是"需要等 bug 修复"。 |
| GDExtension 是否能正常运作？ | **能**，但它是 native extension 边界，不是主框架语言替代品。Rust 绑定成熟度高于 TS 绑定。 |
| Rust / C++ 是否考虑？ | C++ 用于 native/perf；Rust 用于专项 spike；都不做 Core 主语言。 |
| 语言切换能否解决 Godot 跨项目场景共享？ | **不能**，`res://`/资源系统约束仍在。 |
| 下一步最有价值的改进 | 1. C# Source Generator 从"潜力"到"现实"；2. DataOS 单源生成；3. TS 工具链 spike；4. 持续监控 GodotJS / C# Web 导出 / C# GDExtension 化进展。 |

---

## 17. 参考资料

本地资料：

- `DocsAI/Framework/Overview.md`
- `DocsAI/Framework/Principles.md`
- `DocsAI/Framework/GodotCSharpConstraints.md`
- `DocsAI/GameOS/Contracts.md`
- `DocsAI/GameOS/Overview.md`
- `DocsAI/DataOS/Overview.md`
- `DocsAI/ArchitectureDecisionRecords/10-Data系统专项调研报告.md`
- `DocsAI/ArchitectureDecisionRecords/深度分析：C# 对 AI-first 框架是限制还是增强.md`
- `DocsAI/ArchitectureDecisionRecords/深度分析：AI-firstGameOS与ECS概念边界.md`
- `GameOS/Runtime/Data/DataKey.cs`
- `GameOS/Runtime/Data/Data.cs`
- `GameOS/Runtime/Data/DataCatalog.cs`
- `GameOS/Runtime/Event/IEventBus.cs`
- `GameOS/Runtime/Event/EntityEventBus.cs`
- `GameOS/Runtime/Entity/RuntimeEntity.cs`
- `GameOS/Runtime/Entity/EntityId.cs`
- `GameOS/Capabilities/AI/AIDataKeys.cs`
- `GameOS/Capabilities/Movement/MovementDataKeys.cs`

外部资料：

- Godot 官方 4.6 FAQ — 支持语言：<https://docs.godotengine.org/en/4.6/getting_started/introduction/index.html>
- Godot 官方 4.6 C# 平台支持（明确说明 Web 不支持）：<https://docs.godotengine.org/en/4.6/tutorials/scripting/c_sharp/index.html>
- Godot 官方 4.6 C# Exports：<https://docs.godotengine.org/en/4.6/tutorials/scripting/c_sharp/c_sharp_exports.html>
- Godot 官方 4.2 C# 平台支持文章：<https://godotengine.org/article/platform-state-in-csharp-for-godot-4-2/>
- Godot 官方 4.6.2 维护版本（2026-03-31）：<https://godotengine.org/article/maintenance-release-godot-4-6-2/>
- GodotCon Boston 2025 C# Web 原型：<https://godotengine.org/article/live-from-godotcon-boston-web-dotnet-prototype/>
- Godot 官方 Priorities（C# GDExtension plugin 路线图）：<https://godotengine.org/priorities/>
- Godot C# GDExtension Roadmap Discussion：GitHub proposals #7895
- Godot 官方 GDScript 静态类型：<https://docs.godotengine.org/en/4.6/tutorials/scripting/gdscript/static_typing.html>
- Godot 官方 GDExtension：<https://docs.godotengine.org/en/4.6/tutorials/scripting/gdextension/what_is_gdextension.html>
- GDScript namespace 提案：GitHub godot-proposals #1566
- GDScript generics 提案：GitHub godot-proposals #1207 / godot issues #30180
- GodotJS：<https://github.com/godotjs/GodotJS>（issue #76 等已知问题）
- godot-ts：<https://github.com/godotjs/godot-ts>
- godot-rust / gdext：<https://github.com/godot-rust/gdext>
- godot-rust 2026 dev update：<https://godot-rust.github.io/dev/march-2026-update/>
- Chickensoft GDScript vs C# 对比：<https://chickensoft.games/blog/gdscript-vs-csharp>
- GDScript large project 组织：StackOverflow 78057743
- GDScript cyclic dependency：GitHub godot issues #73983

MCP：

- 本次通过 Context7 MCP 查询 Godot 4.6 官方文档，获取 C# 平台支持、GDScript 类型系统、语言支持状态的官方说明。
