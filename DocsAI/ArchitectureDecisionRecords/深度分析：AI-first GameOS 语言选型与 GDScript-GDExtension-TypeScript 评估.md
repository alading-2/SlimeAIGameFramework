# 深度分析：AI-first GameOS 语言选型与 GDScript / GDExtension / TypeScript / Rust / C++ 评估

> 日期：2026-05-15（v5 — 合并版，最终版）
> 状态：Architecture Decision Record — 最终版
> 主题：SlimeAI 作为 AI-first GameOS 框架，主语言应继续使用 C#，还是迁移到 GDScript、GDExtension 语言、TypeScript、Rust 或 C++。本分析合并了 v4 的全面语言对比与 v5 的 GDScript/TypeScript 深度分析。
> 前提：C# 不会降级为 GDExtension（v4 已纠正此过度推断）。本分析在 C# 维持官方脚本语言地位的前提下展开。
> 结论强度：高。基于 Godot 4.6.2 官方文档、GitHub proposals 开放状态、SlimeAI 266 个 C# 文件的本地代码证据、GodotJS 已知稳定性问题。

---

## 0. 核心问题重新定义

之前的分析（v3/v4）已经充分证明了：

- **C# 继续作为 SlimeAI Core 主语言是合理的**，强类型、编译期护栏、.NET 生态对 AI-first 框架是增强而非限制。
- **GDScript 的结构性限制是真实的**：无泛型、无 namespace、无嵌套类型、无 interface。
- **TypeScript 语言优秀但 GodotJS 绑定层不稳定**，当前不适合进入 Godot Runtime。

但用户提出了一个更深的问题：

> **"GDScript 的静态类型系统 + Godot 原生集成，是否足以让 AI 在框架层面写出可靠、可维护的代码？"**

这不是"GDScript 能不能做大项目"（已经确认可以），而是**"AI 在 GDScript 中维护一个需要严格契约边界的框架时，运行时检查 + 约定 + 工具能否等效替代 C# 的编译期保证？"**

以及：

> **"TypeScript 的类型系统 + 工具链优势，如果 GodotJS 稳定后，是否比 GDScript 更适合 SlimeAI Core？"**

本分析不重复 v4 的技术限制清单，而是尝试回答上述两个核心问题，并给出**清晰的决策边界**。

---

## 1. 评估框架：AI 长期维护框架需要什么？

AI 维护大框架与人工维护大框架的需求不同。对 AI 来说，最关键的不是"写起来快不快"，而是：

```text
1. 写错时，谁能最快把它拦下来？
2. 下次回来时，能不能靠符号、类型、文档和测试快速找回正确入口？
3. 修改后，能不能用一个 CLI 命令证明"真的没坏"？
4. 框架契约（DataKey、EventBus、EntityId）被违反时，AI 能不能在最早阶段收到反馈？
```

因此，本分析采用三个核心维度：

| 维度 | 含义 | 为什么对 AI 重要 |
| --- | --- | --- |
| **AI 护栏强度** | 语言/工具能在什么阶段拦截 AI 的错误（编译期、测试期、运行期、场景期） | 错误越晚发现，AI 修复成本指数级上升 |
| **Godot 集成深度** | 语言与 Godot SceneTree、Node、Resource、Signal、Inspector、导出链、调试的集成程度 | 集成越深，AI 与引擎交互的错误越少 |
| **生态与工具链** | 测试框架、分析器、代码生成、schema 处理、CLI 验证、IDE 支持 | 生态越强，AI 越能独立验证和修复 |

---

## 2. 方向一：GDScript 对 AI-first 框架的适合度

### 2.1 不回避 GDScript 的结构性限制

基于 Godot 4.6.2 官方文档和 GitHub proposals 的**原文证据**：

[Evidence] Godot 4.6 官方文档 "Cases where you can't specify types"（`static_typing.md`）：

> "Currently, you cannot specify the types of individual elements within an array or the keys and values within a dictionary during initialization. Furthermore, **nested types, such as an array containing other arrays of a specific class, are not supported** in the current version of the engine."

[Evidence] Godot 4.6 官方文档 `gdscript_basics.md` 关于 Typed Array：

> "An array's type applies to `for` loop variables, as well as some operators like `[]`, `[...] =` (assignment), and `+`. **Array methods (such as `push_back`) and other operators (such as `==`) are still untyped.** ... **Nested array types (like `Array[Array[int]`) are not supported.**"

[Evidence] Godot 4.6 官方文档关于 Typed Dictionary：

> "A dictionary's value type applies to for loop variables, as well as some operators like `[]` and `[...] =` (assignment). **Dictionary methods that return values and other operators (such as `==`) are still untyped.** ... **Nested typed collections (like `Dictionary[String, Dictionary[String, int]]`) are not supported.**"

[Evidence] GitHub godot-proposals #13800 "GdScript Generics for classes and functions"（Open，未实现）：

> 用户请求："I want moar type safety in compile time" — 提案提出 `@generic(ResultType)` 语法，但**官方尚未实现**。

[Evidence] GitHub godot-proposals #1566 "Implement namespaces in GDScript"（Open，未实现）。

[Evidence] GitHub godot-proposals #12224 "Add support for nested typed arrays and dictionaries"（Open，未实现）。

**关键限制清单（对 SlimeAI Core 最致命的）**：

| 限制 | 状态 | 对 SlimeAI Core 的影响 |
| --- | --- | --- |
| **无用户自定义泛型** | 官方未实现（#13800 Open） | `DataKey<T>`、`EventBus<T>` 无法在编译期保证类型安全 |
| **无 namespace** | 官方未实现（#1566 Open） | 266 个文件的全局命名空间竞争；Capability 边界依赖约定 |
| **无嵌套类型** | 官方明确不支持 | `Dictionary<EntityId, Dictionary<DataKey, float>>` 等 DataOS 结构只能退化为 untyped |
| **无 interface 多实现** | 不支持 | Capability 正交组合只能通过单继承 + 约定实现 |
| **Array/Dictionary 方法 untyped** | 官方确认 | `push_back`、`get` 返回值是 `Variant`，AI 需手动转换 |
| **enum 本质上是 int** | 官方确认 | 不保证值在枚举集合内，AI 可赋任意 int |
| **无 readonly / 值类型** | 不支持 | `EntityId` 无法在语言层面保证不可变性 |
| **无运算符重载** | 不支持 | `EntityId` 无法 `==` 直接比较，需手动实现方法 |

这些限制是**结构性**的，不是"GDScript 不够好"，而是**语言设计选择**。Godot 团队选择了轻量、快速迭代、与引擎深度集成的脚本语言，牺牲了 C#/TypeScript/Rust 级别的类型系统表达力。

---

### 2.2 SlimeAI Core 契约在 GDScript 中的最佳近似实现

下面用 GDScript 写出 SlimeAI 三个最核心契约的**最佳近似实现**，并标注与 C# 版本的差距。

#### 2.2.1 DataKey —— 运行时类型字符串 + 约定

C# 版本（编译期类型安全）：

```csharp
// GameOS/Runtime/Data/DataKey.cs
public sealed class DataKey<T> : IDataKey
{
    public string StableKey { get; }
    public T DefaultValue { get; }
    public Type ValueType => typeof(T);
}

public static DataKey<T> Create<T>(string stableKey, T defaultValue = default!)

public bool Set<T>(DataKey<T> key, T value)
public T Get<T>(DataKey<T> key)
```

GDScript 最佳近似：

```gdscript
# res://slime_ai/game_os/runtime/data/data_key.gd
class_name DataKey
extends RefCounted

var stable_key: String
var value_type: String  # "int", "float", "Vector2", "bool" — 运行时字符串标识
var default_value: Variant

func _init(p_stable_key: String, p_value_type: String, p_default_value: Variant):
    stable_key = p_stable_key
    value_type = p_value_type
    default_value = p_default_value

# 运行时类型检查辅助
func _is_valid_type(value: Variant) -> bool:
    match value_type:
        "int": return value is int
        "float": return value is float
        "bool": return value is bool
        "String": return value is String
        "Vector2": return value is Vector2
        "EntityId": return value is EntityId
        _: return false
```

```gdscript
# res://slime_ai/game_os/runtime/data/data_container.gd
class_name DataContainer
extends RefCounted

var slots: Dictionary[String, Variant] = {}
var keys: Dictionary[String, DataKey] = {}

func register(key: DataKey) -> void:
    keys[key.stable_key] = key
    if not slots.has(key.stable_key):
        slots[key.stable_key] = key.default_value

func set_value(key: DataKey, value: Variant) -> bool:
    # 运行时类型检查 —— 错误在这里被发现，而不是编译期
    if not key._is_valid_type(value):
        push_error("Type mismatch for key '%s': expected %s, got %s" % [
            key.stable_key, key.value_type, typeof(value)])
        return false
    slots[key.stable_key] = value
    return true

func get_value(key: DataKey) -> Variant:
    return slots.get(key.stable_key, key.default_value)

# AI 使用时：
# var health_key = DataKey.new("Health", "float", 100.0)
# data.set_value(health_key, "wrong")  # 运行时错误，不是编译错误
# var hp = data.get_value(health_key) as float  # as 失败静默为 null！
```

**与 C# 的差距**：

| 差距点 | C# | GDScript |
| --- | --- | --- |
| 类型安全阶段 | **编译期** — `Set(DamageDataKeys.CurrentHp, "wrong")` 编译失败 | **运行时** — `set_value(health_key, "wrong")` 运行到 `_is_valid_type` 才发现 |
| 类型标识 | `typeof(T)` — 编译器知道 | `value_type: String` — AI 可能写错 `"folat"` |
| 返回值 | `T` — 编译器保证类型 | `Variant` — AI 需手动 `as` 转换，`as` 失败静默为 `null` |
| 默认值 | `T` — 与类型绑定 | `Variant` — 不保证与 `value_type` 一致 |
| API 发现 | IDE 自动完成 `DataKey<float>` 相关方法 | IDE 只看到 `DataKey`，看不到具体类型 |

**关键风险**：AI 写 `data.get_value(health_key) as float` 时，如果 `health_key` 的 `value_type` 其实是 `"int"`，`as` 不会报错，只是**静默返回 `null`**。这是 GDScript `as` 的行为（失败时转为 null）。这种静默失败对 AI 极其危险，因为 AI 无法从编译错误中意识到问题。

#### 2.2.2 EventBus —— Signal + Dictionary payload 或自定义分发

C# 版本（编译期泛型约束 + 运行时 scope 检查）：

```csharp
// GameOS/Runtime/Event/IEventBus.cs
void Publish<T>(in T @event) where T : struct, IEvent;
IDisposable Subscribe<T>(Action<T> handler) where T : struct, IEvent;

// 运行时额外检查：
if (!typeof(IEntityEvent).IsAssignableFrom(eventType))
{
    Log.Error("EntityEventBus rejected: payload MUST implement IEntityEvent");
}
```

GDScript 最佳近似（方案 A：自定义 EventBus）：

```gdscript
# res://slime_ai/game_os/runtime/event/event_bus.gd
class_name EventBus
extends RefCounted

var _handlers: Dictionary[StringName, Array] = {}  # Array 元素 untyped
var _valid_events: Dictionary[StringName, String] = {}  # event_name -> scope ("entity"/"global"/"broadcast")

func register_event(event_name: StringName, scope: String) -> void:
    _valid_events[event_name] = scope

func publish(event_name: StringName, payload: Dictionary) -> void:
    # 运行时 scope 检查
    if not _valid_events.has(event_name):
        push_error("Unknown event: " + event_name)
        return

    var expected_scope = _valid_events[event_name]
    # 没有编译期约束，payload 结构全靠约定

    if _handlers.has(event_name):
        var handlers = _handlers[event_name]
        for handler in handlers:
            if handler.is_valid():
                handler.call(payload)

func subscribe(event_name: StringName, handler: Callable) -> void:
    if not _handlers.has(event_name):
        _handlers[event_name] = []
    _handlers[event_name].append(handler)
```

GDScript 最佳近似（方案 B：Signal）：

```gdscript
# 更贴近 Godot 原生，但类型约束更弱
signal entity_event(event_name: StringName, payload: Dictionary)

func publish(event_name: StringName, payload: Dictionary) -> void:
    entity_event.emit(event_name, payload)

# 订阅方：
# entity_bus.entity_event.connect(_on_entity_event)
# func _on_entity_event(event_name: StringName, payload: Dictionary) -> void:
#     if event_name == "DamageTaken":
#         var amount = payload.get("amount", 0)  # 无类型，无编译期检查
```

**与 C# 的差距**：

| 差距点 | C# | GDScript |
| --- | --- | --- |
| 事件类型约束 | **编译期** — `where T : struct, IEvent` | **无** — payload 是 `Dictionary`，结构靠约定 |
| 事件 scope | **编译期 + 运行时** — `IEntityEvent` / `IGlobalEvent` / `IBroadcastEvent` | **运行时字符串** — `_valid_events[event_name]` |
| payload 类型 | **struct** — 值类型，不可变（设计上） | **Dictionary** — 引用类型，可被修改 |
| 订阅退订 | `IDisposable` — 语言级资源管理 | `Callable` + 手动 `disconnect` — 容易内存泄漏 |
| 重入保护 | 运行时 `HashSet<Type>` 检查 | 需手动实现 |
| 异常隔离 | `try/catch` 隔离单个 handler | `try/catch` 可用但非 Signal 原生 |

**关键风险**：AI 写事件时，C# 的错误是"`DamageEvent` 没有实现 `IEntityEvent`"（编译失败）。GDScript 的错误可能是"`payload` 里没有 `amount` 键"（运行时，可能场景跑起来才出现），或者是"事件名拼写错误"（Signal 连接失败，无报错）。

#### 2.2.3 EntityId —— 最接近的近似，但仍是引用类型

C# 版本（值类型 + readonly）：

```csharp
// GameOS/Runtime/Entity/EntityId.cs
public readonly record struct EntityId(string Value)
{
    public static EntityId Empty { get; } = new EntityId(string.Empty);
    public static EntityId From(string? value) =>
        string.IsNullOrEmpty(value) ? Empty : new EntityId(value);
    public bool IsEmpty => string.IsNullOrEmpty(Value);
    public override string ToString() => Value ?? string.Empty;
    public bool Equals(EntityId other) =>
        string.Equals(Value ?? string.Empty, other.Value ?? string.Empty, StringComparison.Ordinal);
}
```

GDScript 最佳近似：

```gdscript
# res://slime_ai/game_os/runtime/entity/entity_id.gd
class_name EntityId
extends RefCounted  # 注意：是引用类型，不是值类型

var _value: String = ""

func _init(p_value: String = ""):
    _value = p_value if p_value != null else ""

func is_empty() -> bool:
    return _value.is_empty()

func to_string() -> String:
    return _value

# 没有运算符重载，不能 entity_id1 == entity_id2
# 需要手动：
func equals(other: EntityId) -> bool:
    if other == null:
        return is_empty()
    return _value == other._value

static func empty() -> EntityId:
    return EntityId.new("")

static func from(value: String) -> EntityId:
    if value == null or value.is_empty():
        return empty()
    return EntityId.new(value)
```

**与 C# 的差距**：

| 差距点 | C# | GDScript |
| --- | --- | --- |
| 值语义 | **值类型（`struct`）** — 复制时无堆分配 | **引用类型（`RefCounted`）** — 每次 `new` 都堆分配 |
| 不可变性 | **readonly** — 构造后不可改 | 无 `readonly` — AI 可意外 `entity_id._value = "bad"` |
| 相等比较 | `==` 运算符自动使用 `Equals` | 无运算符重载 — 需调用 `.equals()`，AI 易误用 `==`（比较引用） |
| null 安全 | `string?` + `From(string?)` 统一处理 | `String` 无 null 安全，需手动检查 |
| 默认语义 | `default(EntityId) == Empty` | 无 `default`，需显式调用 `EntityId.empty()` |

**关键风险**：AI 写 `if entity_id == EntityId.empty()` 时，GDScript 的 `==` 比较的是**引用地址**，不是值内容。因为 `EntityId.empty()` 每次返回的是**新对象**（虽然值相同，但引用不同），这个判断永远为 `false`。AI 必须记住用 `.equals()`，这是一个对 AI 不友好的陷阱。

---

### 2.3 运行时检查 + 约定 + 工具能否等效替代编译期保证？

这是本分析的核心问题。答案是：**可以部分替代，但等效性取决于三个条件，缺一不可。**

#### 条件 1：运行时检查必须覆盖所有契约边界

GDScript 可以写运行时检查：

```gdscript
func set_value(key: DataKey, value: Variant) -> bool:
    if not key._is_valid_type(value):
        push_error("Type mismatch")
        return false
    ...
```

**但运行时检查的问题**：

1. **覆盖率依赖执行路径**：如果某条代码路径从未被测试触发，类型错误可能潜伏很久。
2. **Silent failures**：GDScript 的 `as` 转换失败返回 `null`，`push_error` 不会中断执行。AI 可能继续用 `null` 做计算，导致更远的逻辑漂移。
3. **性能成本**：每次 `set_value` 都要做 `match typeof(value)`，高频调用时有开销。

C# 的编译期检查**零运行时成本**，且**覆盖率 100%**（所有代码路径都被编译器检查）。

#### 条件 2：约定必须有自动化工具验证

GDScript 可以靠约定：

```gdscript
# 约定：所有 DataKey 的 value_type 字符串必须与 DataOS schema 一致
# 约定：所有 event_name 必须在 EventRegistry 中注册
# 约定：所有 EntityId 比较必须用 .equals()
```

**但约定对 AI 的问题**：

1. **AI 不一定读文档**。AI 靠上下文推断，不一定会去查"约定文档"。
2. **约定漂移**：随着框架演进，约定可能更新，旧代码可能与新约定不一致。
3. **无工具验证**：GDScript 没有 Roslyn analyzer 或 eslint 级别的静态检查工具。虽然有 GDScript linter（gdlint），但生态远弱于 C# / TS。

C# 的约定可以变成**编译期规则**：

```csharp
// analyzer：禁止 Capability 直接 new Data()
// analyzer：DataKey stable_key 必须与 DB schema 匹配
// 这些可以自动验证，AI 违反时编译失败
```

#### 条件 3：测试覆盖率必须极高

运行时检查 + 约定要想等效于编译期检查，前提是**所有代码路径都被测试覆盖**。

C# 的 `dotnet test` 可以在 Godot 外运行大量单元测试。GDScript 的测试需要 Godot 环境（GUT 等），且测试生态不如 .NET 成熟。

**实际难度**：SlimeAI 有 266 个文件，Capability 之间交互复杂。要达到"运行时检查 + 测试等效于编译期检查"，测试覆盖率需要接近 100%。这对 AI 维护来说成本极高。

#### 结论：不等效，但"足够好"的阈值在哪里？

```text
编译期保证 = 100% 覆盖率，零成本，零遗漏
运行时检查 + 约定 + 工具 = 覆盖率依赖执行路径，有性能成本，有 silent failure 风险
```

**不等效**，但差距的大小取决于场景：

| 场景 | 差距是否致命 | 说明 |
| --- | --- | --- |
| 独立小游戏，框架契约简单 | **不致命** | 运行时检查 + 少量测试足够 |
| 中型项目，10-20 个 Capability | **中等** | 需要较完善的测试 + 工具链 |
| SlimeAI 级框架，266 个文件，严格 DataOS/EventBus/Entity 契约 | **较致命** | AI 错误容易在运行时漂移，定位成本高 |

**关键洞察**：GDScript 的运行时检查 + 约定 + 工具，**对"人类维护的小型团队"可能足够**，但对"AI 维护的大型框架"不够。因为 AI 的错误模式不是"偶尔写错"，而是"在 266 个文件的修改中，某处违反了某个约定"。人类可以通过代码审查发现，AI 依赖编译器和测试。

---

### 2.4 GDScript "一等公民"优势的量化

GDScript 的优势是真实的，但需要量化到 AI-first 框架的具体场景：

| 优势 | 对 AI 维护 SlimeAI Core 的价值 | 量化评估 |
| --- | --- | --- |
| **编辑器原生集成** | 写 scene glue 时确实更快 | 但 SlimeAI Core 不依赖 scene glue |
| **调试原生体验** | 断点、Inspector、Signal 可视化 | Core Runtime 不应依赖 Godot 调试 |
| **热重载** | 保存即运行 | 对玩法试验有价值，对框架契约验证价值有限 |
| **导出链完整性** | 支持所有平台（含 Web） | C# 缺 Web 是真实限制，但 Core 逻辑不因此改变 |
| **社区生态** | 84% 开发者使用 GDScript | Asset Store 插件多，但 SlimeAI 是框架不是游戏 |
| **AI 写代码轻松度** | 无 namespace、无泛型约束 | 写单文件快，但维护多文件框架时命名冲突风险上升 |
| **长期维护** | Godot 官方承诺不放弃 | 真实优势，但 C# 也是官方支持语言 |

**关键区分**：

```text
GDScript 的优势集中在"与 Godot 引擎交互"的层：
  - Node/SceneTree/Resource/Signal/Inspector/Editor

SlimeAI Core 的定位是"与 Godot 引擎隔离"的层：
  - DataKey/EventBus/Entity/Schedule/Capability
  - 这些应该能在 Godot 外被 dotnet test 验证
```

GDScript 的"一等公民"优势在 Core 层**被削弱了**，因为 Core 层的设计目标就是**减少与 Godot 的直接耦合**。

---

### 2.5 从"AI 长期维护框架"角度的综合评估

对 AI 来说，GDScript vs C# 的核心差异不是"语法谁更好写"，而是**"错误在哪一层被发现"**：

```text
C# 的错误发现链：
  写代码 -> 编译失败（10 秒内）-> AI 立刻修
  写代码 -> dotnet test 失败（30 秒内）-> AI 看断言修
  写代码 -> Godot 场景失败（分钟级）-> AI 查日志修

GDScript 的错误发现链：
  写代码 -> 保存通过（无编译）-> 运行到某处 -> push_error（可能被忽略）
  写代码 -> 保存通过 -> Godot 场景跑起来 -> 行为不对（状态漂移）
  写代码 -> 保存通过 -> 运行一段时间 -> 某处 null 引用（as 转换失败）
```

GDScript 把更多错误从"编译期"推到了"运行时"和"场景期"。对 AI 来说，这意味着：

1. **修复循环变长**：AI 无法从编译错误中快速定位问题，需要依赖运行日志和测试。
2. **Silent failures 更危险**：`as` 转 null、`Dictionary` 键不存在、`Variant` 类型不匹配 不会立即崩溃，而是导致状态悄悄错误。
3. **符号路由更弱**：无 namespace 意味着 AI 搜索 `MovementService` 时可能找到多个同名类，需要依赖目录结构约定。

**但 GDScript 也有对 AI 有利的方面**：

1. **写场景胶水确实更快**：AI 写 GodotBridge、场景适配、输入处理时，GDScript 比 C# 更直接。
2. **保存即运行减少了"编译焦虑"**：C# 的 `dotnet build` 失败有时是因为 Godot API 绑定更新，而非逻辑错误。
3. **与 Godot 概念一致**：AI 不需要在 `Vector2` / `Vector2`（C# Godot 命名空间）之间转换。

---

### 2.6 GDScript 决策边界

**Clear Adopt（立即采用）**：

- Godot 场景胶水层（GodotBridge adapter、输入处理、可视化实例化）。
- 编辑器内轻量工具、验证场景、临时 inspector 辅助。
- 游戏侧 gameplay 脚本（不进入框架 Core）。
- 需要 Web 导出且 C# 不支持时的 fallback。

**Conditional Adopt（有条件采用）**：

- 新 Capability 的 prototype：如果该 Capability 逻辑简单、契约松散、不需要严格 DataOS 集成，可用 GDScript 快速验证。
- DataOS 可视化编辑器 UI 层：Godot EditorPlugin 用 GDScript 写，调用 C# Runtime API。

**Clear Reject（明确不采用）**：

- 用 GDScript dynamic Dictionary / Variant 取代 `DataKey<T>` 作为主状态契约。
- 用 GDScript Signal / string event 取代 typed EventBus 作为事件总线。
- 把 `GameOS/Runtime`、`GameOS/Capabilities` 全量改写为 GDScript。
- 在 AI 需要严格类型护栏的框架契约层放弃 C# 编译期保证。

**决策边界公式**：

```text
如果模块满足以下全部条件，可考虑 GDScript：
  1. 不需要 DataKey<T> 级别的类型安全。
  2. 不需要跨 Capability 的严格事件契约。
  3. 逻辑可在 Godot 场景内完整验证（不需要 dotnet test）。
  4. 命名冲突风险低（模块小、文件少）。
  5. 与 Godot Node/Scene/Resource 深度集成。

如果任一条件不满足，C# 更优。
```

---

## 3. 方向二：TypeScript 对 AI-first 框架的适合度

### 3.1 TypeScript 类型系统对 AI 写代码的真实体验

TypeScript 的类型系统确实非常适合 AI：

| 特性 | 对 AI 的价值 |
| --- | --- |
| **泛型** | `DataKey<T>`、`EventBus<T>` 可以精确表达 |
| **Interface + 多实现** | Capability 正交组合无压力 |
| **Namespace/Module** | 大项目组织清晰 |
| **类型推断** | AI 写代码时不需要频繁显式标注 |
| **Union / Literal / Mapped types** | DataOS schema 表达力极强 |
| **条件类型** | 高级契约可以类型级编程 |
| **Zod / io-ts 运行时验证** | 编译期 + 运行时双重护栏 |

**AI 写 TS 的体验确实通常优于 C#**：

- 语法更轻量，没有 `public sealed class` 等 ceremony。
- 类型推断更强，AI 不需要写那么多显式类型标注。
- 错误信息通常更友好（没有 C# 的 `where T : struct, IEvent` 泛型约束错误）。
- 工具链（npm、eslint、prettier、vitest）对 AI 更友好。

**但有一个常被忽略的问题**：

```text
TypeScript 的类型系统再强，也只是编译时的。
一旦进入 Godot Runtime，类型被擦除。
```

这和 C# 有本质区别：C# 的泛型在运行时**保留**（`typeof(T)`、`DataKey<float>` 和 `DataKey<int>` 是不同的类型），而 TypeScript 的泛型在运行时**完全擦除**。

对 SlimeAI EventBus 来说：

```typescript
// TypeScript：编译期类型安全
function publish<T extends IEvent>(event: T): void

// 运行时：event 的类型信息丢失
// 无法做 typeof(T) 检查，除非手动维护类型注册表
```

这意味着 TS 的 EventBus 也需要和 GDScript 类似的运行时类型注册表，**无法等效替代 C# 的反射能力**。

---

### 3.2 GodotJS / godot-ts 的当前真实状态

[Evidence] GodotJS GitHub README（截至 2026）：

> "The core functionality is implemented and essentially usable but **still under testing**..."

[Evidence] GodotJS 需要 `npm install && npx tsc` 才能运行示例项目，说明有 transpile 步骤。

[Evidence] GodotJS 支持多引擎（V8、QuickJS、quickjs-ng、JavaScriptCore、Browser JS），但多引擎支持意味着**更多故障源**。

[Evidence] GodotJS issue #76（Codegen doesn't work）：用户报告 V8、QuickJS、quickjs-ng 都有编译问题。

[Evidence] GodotJS issue #122（Issues related with quickjs）仍在 Open。

**关键判断**：

```text
GodotJS 不是"beta 可用"，而是"core functionality essentially usable but still under testing"。
这意味着：
  1. 基本功能可以跑通。
  2. 但已知有严重构建问题。
  3. 多引擎支持增加了稳定性风险。
  4. 没有生产级项目公开使用 GodotJS 承载大型框架的证据。
```

[Evidence] GodotJS 文档说明 `godot-ts build` 和 `godot-ts watch` 编译 `.ts` 文件，这意味着：

1. **有编译步骤**：不是"保存即运行"，需要 `npx tsc`。
2. **类型擦除**：TS 类型不会进入 Godot runtime。
3. **调试链更长**：TS -> JS -> GodotJS binding -> GDExtension -> Godot。

---

### 3.3 SlimeAI Core 契约在 TypeScript（GodotJS）中的近似实现

TypeScript 版本的 DataKey（编译期类型安全，运行时擦除）：

```typescript
// slime_ai/game_os/runtime/data/data_key.ts
// 编译期类型安全 ✅
class DataKey<T> {
    stableKey: string;
    defaultValue: T;

    constructor(stableKey: string, defaultValue: T) {
        this.stableKey = stableKey;
        this.defaultValue = defaultValue;
    }
}

// AI 写错会编译失败：
// const healthKey = new DataKey<number>("Health", 100);
// data.setValue(healthKey, "wrong");  // ❌ 编译错误！

class DataContainer {
    private slots = new Map<string, any>();

    setValue<T>(key: DataKey<T>, value: T): boolean {
        this.slots.set(key.stableKey, value);
        return true;
    }

    getValue<T>(key: DataKey<T>): T {
        return this.slots.get(key.stableKey) ?? key.defaultValue;
    }
}
```

**TypeScript 的问题**：

1. **运行时无类型信息**：`DataKey<T>` 在编译后变成 `DataKey`，`T` 被擦除。无法做运行时类型检查。
2. **GodotJS 绑定**：`DataContainer` 如果要与 Godot C++ API 交互，需要 GodotJS 的 binding 支持 `Map` 类型转换。这引入了额外的复杂度。
3. **泛型约束在运行时不可见**：`publish<T extends IEvent>` 在运行时无法检查 `T` 是否真的是 `IEvent`。

对比：

| 维度 | C# | TypeScript（GodotJS） | GDScript |
| --- | --- | --- | --- |
| 编译期类型安全 | ✅ 强 | ✅ 强 | ⚠️ 弱（可选 hint） |
| 运行时类型保留 | ✅ `typeof(T)` 可用 | ❌ 完全擦除 | ✅ `typeof()` / `is` 可用 |
| 泛型约束 | ✅ `where T : struct, IEvent` | ✅ `extends` | ❌ 无 |
| namespace | ✅ | ✅ | ❌ |
| interface 多实现 | ✅ | ✅ | ❌ |
| 反射能力 | ✅ `System.Reflection` 完整 | ⚠️ 有限（需 JS runtime） | ⚠️ `get_method_list()` 等 |
| Godot 集成 | ✅ 官方内置 | ❌ GodotJS 不稳定 | ✅ 原生 |
| 调试体验 | ✅ 原生 | ⚠️ 多层（TS->JS->binding） | ✅ 原生 |
| 导出链 | ⚠️ 缺 Web | ❌ 未知 | ✅ 完整 |
| AI 写错拦截 | 编译期 + 测试 | 编译期（但不进 Godot） | 运行时 |

TypeScript 在**编译期**和 C# 一样强，但**运行时**比 C# 弱（类型擦除），**Godot 集成**比 GDScript 弱（GodotJS 不稳定）。

---

### 3.4 TypeScript 工具链优势对 AI-first 框架的价值

TypeScript 的工具链优势是真实的，但价值集中在**工具层**，不是 **Runtime 层**：

| 工具 | 对 SlimeAI 的价值 | 适用层 |
| --- | --- | --- |
| ts-morph / AST 操作 | DataOS -> C# DataKey 代码生成 | **工具链** |
| Zod / json-schema | DataOS schema 运行时验证 | **工具链 + Runtime 辅助** |
| vitest / jest | 快速单元测试 | **工具链** |
| eslint / prettier | 代码风格一致性 | **工具链** |
| npm 生态 | JSON/SQLite/Markdown/CLI 处理 | **工具链** |
| TypeDoc | 自动生成 API 文档 | **工具链** |

**关键区分**：这些工具链优势**不依赖 TypeScript 进入 Godot Runtime**。SlimeAI 可以：

```text
用 TypeScript 写：
  - DataOS schema validator
  - C# DataKey Source Generator 的生成逻辑
  - DocsAI 一致性检查器
  - Snapshot 可视化工具

生成到 C# Runtime：
  - DataKey<T> 自动生成的 C# 代码
  - 保持 C# 的编译期类型安全
```

这是"TS 工具链 + C# Runtime"的最佳分工，而不是"用 TS 取代 C# Runtime"。

---

### 3.5 TypeScript 决策边界

**Clear Adopt（立即采用）**：

- DataOS authoring 辅助工具。
- schema -> doc / code generator。
- DocsAI 一致性检查、链接检查。
- Snapshot / observation 可视化工具。
- 外部 Web UI（不进入 Godot）。

**Spike（验证后采用）**：

- 用 TS 生成 DataOS -> C# DataKey 的自动生成逻辑（无需进入 Godot Runtime）。
- TS + Zod 做 DataOS schema validator（CLI 工具，不进入 Godot）。

**Clear Reject（明确不采用）**：

- 在 GodotJS 当前状态下进入 Godot Runtime。
- 用 TS 取代 C# 的 `GameOS/Runtime` Core。
- 任何需要生产级稳定性的 Godot 集成模块。

**未来条件（GodotJS 稳定后重新评估）**：

```text
如果以下全部满足，可重新评估 TS 作为 Core 语言：
  1. GodotJS 无已知严重构建问题（issue #76 级别修复）。
  2. 有公开的大型项目（>100 文件）使用 GodotJS 生产运行。
  3. 导出链（Desktop + Mobile + Web）经过验证。
  4. 调试体验（断点、堆栈、异常）达到 C# 同等水平。
  5. 类型声明与 Godot 4.x API 同步（无版本漂移）。
```

**当前状态**：5 个条件中 0 个满足。

---

## 4. 综合决策：三维模型与清晰边界

### 4.1 三维评估模型

用三个维度评估每种语言对 SlimeAI Core 的适合度：

| 语言 | AI 护栏强度 | Godot 集成深度 | 生态工具链 | SlimeAI Core 适合度 |
| --- | --- | --- | --- | --- |
| **C#** | ✅ 高（编译期泛型、namespace、analyzer 潜力） | ✅ 中-高（官方脚本语言，但弱于 GDScript） | ✅ 高（.NET test、Roslyn） | **首选** |
| **GDScript** | ⚠️ 中（运行时检查、约定） | ✅ **最高**（原生） | ⚠️ 中（Godot 测试生态发展中） | **场景胶水层** |
| **TypeScript** | ✅ 高（编译期类型强，但运行时擦除） | ❌ 低（GodotJS 不稳定） | ✅ **最高**（npm 生态） | **工具链层** |
| **Rust** | ✅ **最高**（所有权 + 泛型） | ⚠️ 中（GDExtension，社区绑定） | ⚠️ 中（cargo 强但 Godot 集成复杂） | **性能热点** |

### 4.2 决策边界：什么条件下选择什么语言

**C# 更适合的条件**：

```text
✅ 需要编译期类型安全（DataKey<T>、EventBus<T>、EntityId struct）。
✅ 需要 namespace 隔离大框架（266 个文件）。
✅ 需要离线验证（dotnet test 不启动 Godot）。
✅ 需要 AI 在编译期收到错误反馈。
✅ 需要长期稳定的框架契约（Capability Owner 边界）。
✅ 需要 Source Generator / analyzer 潜力（DataOS 单源生成）。
```

**GDScript 更适合的条件**：

```text
✅ 紧贴 Godot 的代码（Node、SceneTree、Resource、Signal、Inspector）。
✅ 需要 Web 导出且 C# 不支持。
✅ 逻辑简单、文件少、命名冲突风险低。
✅ 不需要严格 DataOS/EventBus 契约。
✅ 验证场景、编辑器工具、一次性脚本。
✅ 人类快速迭代玩法（AI 维护 Core 时不需要修改）。
```

**TypeScript 更适合的条件**：

```text
✅ 工具链开发（codegen、validator、schema 处理）。
✅ 不进入 Godot Runtime 的辅助工具。
✅ 需要快速 CLI 验证和 npm 生态。
✅ 外部 Web UI 或文档检查。
❌ 不适用于 GodotJS 当前状态下的任何 Runtime 模块。
```

---

## 5. 最终结论与行动建议

### 5.1 结论

**一句话**：

> C# 继续作为 SlimeAI Core 主语言；GDScript 用于 Godot 场景胶水层；TypeScript 用于工具链层；GodotJS 稳定前不进入 Runtime。

**详细分层**：

```text
Core Runtime / Capability / DataOS runtime loader / Validation / Observation
  -> C#（编译期护栏对 AI 维护大框架不可替代）

Godot 场景胶水 / Editor 插件 / 验证场景 / 游戏侧轻量逻辑
  -> GDScript（Godot 原生集成的不可替代优势）

AI 辅助工具 / DataOS authoring / codegen / schema validator / 可视化
  -> TypeScript（工具链生态的不可替代优势）

性能热点 / 复杂 native 算法
  -> Rust GDExtension（必要时 spike）

Godot Runtime 主框架改写为 GDScript 或 TypeScript
  -> Reject Now（结构性限制未解决）
```

### 5.2 为什么不是"继续用 C#"的简单结论

本分析承认用户的直觉是**部分正确**的：

1. **GDScript 确实是 Godot 生态的未来主语言**（84% 开发者、官方承诺、Asset Store 生态）。
2. **GDScript 的"一等公民"优势是真实的**，在编辑器/调试/热重载/导出链上是不可逾越的。
3. **TypeScript 的工具链优势是真实的**，在 AI 辅助开发和 codegen 上优于 C#。

但**这些优势所在的层与 SlimeAI Core 所在的层不同**：

```text
GDScript 优势层 = Godot 引擎交互层
TypeScript 优势层 = 工具链开发层
C# 优势层 = 框架内核契约层
```

**SlimeAI 的长期竞争力不在于"选哪个语言"，而在于"每个语言用在正确的层"**。

### 5.3 关键行动

1. **维持 C# Core，不因为 GDScript 生态好而迁移 Core**：
   - `DataKey<T>`、`EventBus<T>`、`EntityId` 等契约继续 C#。
   - 新 Capability 默认 C#，除非明确满足 GDScript 更适合的条件。

2. **扩大 GDScript 在正确层的使用**：
   - GodotBridge adapter 层更多使用 GDScript（减少 C#-Godot 命名转换）。
   - 编辑器插件、验证场景、游戏侧 gameplay 逻辑优先 GDScript。

3. **引入 TypeScript 工具链**：
   - 用 TS 写 DataOS schema validator（CLI 工具，不入 Godot）。
   - 用 TS + Zod 做 snapshot 一致性检查。
   - 评估 TS 生成 C# DataKey 的 codegen 工具。

4. **监控 GodotJS 但不等待**：
   - 跟踪 GodotJS issue #76 / #122 等关键 bug。
   - 不将任何 Core 逻辑绑定到 GodotJS 的稳定性上。
   - 每季度重新评估 GodotJS 状态（但当前不满足任何采用条件）。

5. **监控 GDScript 类型系统演进**：
   - 跟踪 proposals #13800（泛型）、#1566（namespace）、#12224（嵌套类型）。
   - 如果 Godot 官方实现其中任意一个，重新评估 GDScript 对 Core 的适合度。
   - **当前状态**：三个提案全部 Open，无实现时间表。

### 5.4 最重要的认知

对 AI-first 框架来说，**语言的"生态好"不等于"框架适合"**。

GDScript 生态再好，它的类型系统缺失（泛型、namespace、嵌套类型、interface）是**结构性**的，不是"暂时不够完善"。这意味着：

```text
即使 GDScript 未来增加了泛型（#13800），也需要多年生态成熟。
即使 GodotJS 未来稳定了，TypeScript 的类型运行时擦除也是语言设计级别的。
C# 的编译期护栏是当下就能用的，不需要等待任何提案。
```

**选语言不是投票选最受欢迎的，而是选最能约束 AI 不出错的。** 在当前 Godot 生态中，C# 仍然是最能约束 AI 在框架层面写出可靠代码的语言。

---

## 6. 参考资料

### 本地资料

- `DocsAI/ArchitectureDecisionRecords/深度分析：AI-first GameOS 语言选型与 GDScript-GDExtension-TypeScript 评估.md`（v4）
- `DocsAI/ArchitectureDecisionRecords/深度分析：C# 对 AI-first 框架是限制还是增强.md`
- `GameOS/Runtime/Data/DataKey.cs`
- `GameOS/Runtime/Event/IEventBus.cs`
- `GameOS/Runtime/Event/EntityEventBus.cs`
- `GameOS/Runtime/Entity/EntityId.cs`

### 外部资料

- Godot 4.6 官方文档 — GDScript Static Typing（`static_typing.md`）：<https://docs.godotengine.org/en/stable/tutorials/scripting/gdscript/static_typing.html>
- Godot 4.6 官方文档 — GDScript Basics（Typed Array / Typed Dictionary）：<https://docs.godotengine.org/en/stable/tutorials/scripting/gdscript/gdscript_basics.html>
- GitHub godot-proposals #13800 "GdScript Generics for classes and functions"（Open）
- GitHub godot-proposals #1566 "Implement namespaces in GDScript"（Open）
- GitHub godot-proposals #12224 "Add support for nested typed arrays and dictionaries"（Open）
- GodotJS GitHub：<https://github.com/godotjs/GodotJS>
- GodotJS issue #76 "Codegen doesn't work"
- GodotJS issue #122 "Issues related with quickjs"
- GodotJS 文档 — godot-ts：<https://godotjs.github.io/documentation/typescript/godot-ts/>
- Godot 官方 C# 文档：<https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/index.html>
- Godot 官方 — Scripting languages（"Which language should I use?"）：<https://docs.godotengine.org/en/stable/getting_started/step_by_step/scripting_languages.html>

---

## 附录 A：Rust / C++ / GDExtension 多语言分析

### A.1 C++

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

C++ 的 AI 可写性、内存安全、构建速度和错误诊断都不如 C# / TS。除非有明确性能或 native SDK 需求，否则不要把主框架放进 C++。

### A.2 Rust

Rust 的类型系统和内存安全非常强，`godot-rust/gdext` 是 Godot 4 GDExtension 生态的重要项目。

[Evidence] Reddit 讨论（2026）确认："The bindings are definitely ready for production, we have been using it at work for a while now and we rarely come across bugs that are related to the bindings themselves."

[Evidence] godot-rust 2025 年回顾："type-safe signals"等关键功能已落地。

[Evidence] godot-rust 官方文档确认支持多个稳定 Godot 版本。

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

### A.3 其他 GDExtension 语言（简要）

| 语言 | 绑定项目 | 成熟度 | SlimeAI 适用性 |
| --- | --- | --- | --- |
| Python | godot-python | 社区维护，活跃度一般 | 不适合 Core。Python 的动态类型与 SlimeAI 的 typed contract 目标冲突。 |
| Lua | 多个社区绑定 | 碎片化 | 不适合 Core。Lua 极简语义不利于表达复杂框架契约。 |
| TS/JS | GodotJS | **已知不稳定** | 见 §3.2。 |

### A.4 GDExtension 总体边界

**Adopt**：

- `NativeAcceleration`：路径搜索、空间查询、批量数值计算、外部 SDK。
- `NativeBridge`：框架需要绑定不可避免的 native 库时。

**Reject**：

- 把 Capability owner 逻辑迁到 GDExtension。
- 把 DataOS runtime loader 迁到 GDExtension。
- 为了"支持更多语言"或"AI 写起来轻松"而引入 GDExtension。

GDExtension 应该是能力扩展点，不是主框架语言逃生舱。

---

## 附录 B：修正后的对比矩阵

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
| 长期维护风险 | **低** | **最低** | **高**（绑定层不稳定） | 中高 | 中高 |
| SlimeAI Core 适合度 | **最优** | 不推荐 | 当前不推荐 | 不推荐 | 不推荐 |
| SlimeAI 工具层适合度 | 高 | 中 | **最优** | 低 | 低 |

**关键修正说明**（相比 v3）：

1. **C# 长期维护风险从"中高"修正为"低"**：官方计划统一构建是正面变化，不是风险。
2. **C# Godot 官方支持保留"中高"**：C# 仍是官方支持语言，统一构建不改变这一事实。
3. **删除"C# GDExtension"伪概念**：C# 没有变成 GDExtension，官方说的是"plugin/module"。
4. **GDScript 的 SlimeAI Core 适合度保留"不推荐"**：无泛型和无 namespace 的结构性限制不因 C# 路线变化而改变。
5. **C# 的 SlimeAI Core 适合度恢复"最优"**：类型系统、泛型、namespace、.NET 测试生态仍然是 Core 最需要的。

---

## 附录 C：语言分层建议

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

## 附录 D：对 DataOS 和单源事实的影响

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

## 附录 E：后续 OpenSpec 候选

### E.1 `language-policy-for-slimeai`

目标：

- 把本 ADR 的语言分层写入 `DocsAI/Framework/Principles.md`。
- 在 `Workspace/DocsAI/AgentWorkflow/Protocols/AIFeatureDevelopmentProtocol.md` 增加"默认 C#，例外需说明"的门禁。
- 在 owner skills 中加入 GDScript / TS / GDExtension 例外条件。

验收：

- `rg -n "Language Policy|TypeScript|GDScript|GDExtension" DocsAI .ai-config/skills`
- `Tools/run-build.sh`
- `Tools/run-tests.sh`

### E.2 `typescript-tooling-spike`

目标：

- 只做工具层，不进 Godot Runtime。
- 评估 TS 生成 DataOS docs、schema diff、DocsAI 链接检查、snapshot 可视化。

验收：

- `npm test` 或项目选定 CLI test。
- 生成 artifact 放入 `.ai-temp/`，不污染长期事实源。

### E.3 `godotjs-runtime-evaluation`

目标：

- 重新评估 GodotJS 在 Godot 4.6.2 下的稳定性（关注 issue #76 等关键 bug 是否已修复）。
- 如已修复，建立 `experiments/godot-ts-runtime-spike`，验证：Godot 4.6.2 启动、脚本挂载、headless scene、export、与 C# bridge 互调。
- 如未修复，维持 Reject。

硬门槛：

- 不通过 export/headless 验证，不进入主框架。
- 不替换现有 C# Runtime。

### E.4 `gdscript-editor-glue-policy`

目标：

- 允许 GDScript 进入 EditorPlugin / validation scene glue。
- 禁止 GDScript 持有 Core Runtime state truth。

验收：

- 新增 GDScript 文件必须能被 `rg` 路由到 owner 文档。
- 每个 GDScript glue 都要说明它调用哪个 C# Runtime API。

### E.5 `csharp-source-generator-roadmap`

目标：

- 把"Source Generator 潜力"变成"Source Generator 现实"。
- 设计第一个 Roslyn Source Generator：从 DataOS schema 生成 `DataKey<T>` 和 validator 规则。

验收：

- 实现一个最小可用 generator。
- 集成到 `Tools/run-build.sh`。
- AI 可以通过修改 schema 文件，让 generator 自动更新 C# 代码。

---

## 附录 F：Reject List

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
| **基于"C# 会变成 GDExtension"推断改变语言策略** | **Godot 官方明确说 C# 脚本语言实现会保留；统一构建是正面变化，不是降级。** |

---

## 附录 G：Unknown / 待验证

| Unknown | 降低不确定性的验证 |
| --- | --- |
| Godot PR #106125（C# Web 导出）何时合并 | 持续监控；每季度 review。 |
| C# 统一构建的具体实现和时间表 | 关注 Godot 官方博客和 release notes；没有版本号推断。 |
| GodotJS issue #76 等关键 bug 何时修复 | 每季度检查 GodotJS 仓库状态。 |
| C# Source Generator 在 SlimeAI 中的 ROI | 通过 `csharp-source-generator-roadmap` spike 验证。 |
| GDScript 静态类型在 SlimeAI 风格 DataKey/Event 上的表达力上限 | 写一个 GDScript-only mini runtime，对比 C# tests 和错误输出。 |
| Rust GDExtension 与当前 GodotBridge 生命周期冲突 | 只在性能热点 spike 中验证，不进入 Core。 |
| C# 编译反馈能否压到 AI 可接受速度 | 增量 build/test、watch、analyzer、分层测试计划。 |

---

## 附录 H：最终摘要

| 问题 | 结论 |
| --- | --- |
| AI-first 是否应该选择 AI 最会写的语言？ | **不完全**。应选择 AI 最容易被约束、验证和复盘的语言/工具链。 |
| 是否继续 C#？ | **是**，Core Runtime 继续 C#。官方统一构建计划不改变 C# 的地位。 |
| C# 是否即将变成 GDExtension？ | **不是**。官方计划是"统一构建 + C# plugin"，C# 脚本语言实现会保留。GDExtension 只是 C# 未来新增的能力之一。 |
| GDScript 能否承担大框架？ | **能承担大项目/框架**，但 SlimeAI Core 需要的 namespace、泛型、接口多实现、纯 runtime tests 在 GDScript 中要么缺失要么退化为约定。 |
| GDScript 是否只是入门语言？ | **不是**。这个判断应废弃。但它有结构性天花板（无 namespace、无泛型）。 |
| TypeScript 是否值得重视？ | **非常值得**，优先用于工具链和 authoring；Runtime 需 spike 后再谈。 |
| GodotJS 当前是否可用？ | **已知有严重构建问题**（issue #76），不是"需要 spike"，而是"需要等 bug 修复"。 |
| GDExtension 是否能正常运作？ | **能**，但它是 native extension 边界，不是主框架语言替代品。 |
| Rust / C++ 是否考虑？ | C++ 用于 native/perf；Rust 用于专项 spike；都不做 Core 主语言。 |
| 语言切换能否解决 Godot 跨项目场景共享？ | **不能**，`res://`/资源系统约束仍在。 |
| 下一步最有价值的改进 | 1. C# Source Generator 从"潜力"到"现实"；2. DataOS 单源生成；3. TS 工具链 spike；4. 持续监控 GodotJS 稳定性。 |
| GDScript 一等公民优势是否足以抵消类型缺失？ | **在 Core 层不足以抵消**。优势集中在 Godot 引擎交互层，Core 层的设计目标是减少与 Godot 的直接耦合。 |
| 运行时检查+约定+工具能否替代编译期保证？ | **不等效**。对 AI 维护 266 个文件的框架，编译期护栏的不可替代性更高。 |

---

## 附录 I：参考资料（补充）

### 本地资料（补充）

- `DocsAI/Framework/Overview.md`
- `DocsAI/Framework/Principles.md`
- `DocsAI/Framework/GodotCSharpConstraints.md`
- `DocsAI/GameOS/Contracts.md`
- `DocsAI/GameOS/Overview.md`
- `DocsAI/DataOS/Overview.md`
- `DocsAI/ArchitectureDecisionRecords/10-Data系统专项调研报告.md`
- `DocsAI/ArchitectureDecisionRecords/深度分析：C# 对 AI-first 框架是限制还是增强.md`
- `DocsAI/ArchitectureDecisionRecords/深度分析：AI-firstGameOS与ECS概念边界.md`
- `GameOS/Runtime/Data/Data.cs`
- `GameOS/Runtime/Data/DataCatalog.cs`
- `GameOS/Runtime/Entity/RuntimeEntity.cs`
- `GameOS/Capabilities/AI/AIDataKeys.cs`
- `GameOS/Capabilities/Movement/MovementDataKeys.cs`

### 外部资料（补充）

- Godot 官方 4.6 FAQ — 支持语言：<https://docs.godotengine.org/en/4.6/getting_started/introduction/index.html>
- Godot 官方 4.6 C# 平台支持（明确说明 Web 不支持）：<https://docs.godotengine.org/en/4.6/tutorials/scripting/c_sharp/index.html>
- Godot 官方 4.6 C# Exports：<https://docs.godotengine.org/en/4.6/tutorials/scripting/c_sharp/c_sharp_exports.html>
- Godot 官方 4.2 C# 平台支持文章：<https://godotengine.org/article/platform-state-in-csharp-for-godot-4-2/>
- Godot 官方 4.6.2 维护版本（2026-03-31）：<https://godotengine.org/article/maintenance-release-godot-4-6-2/>
- GodotCon Boston 2025 C# Web 原型：<https://godotengine.org/article/live-from-godotcon-boston-web-dotnet-prototype/>
- Godot 官方 Priorities（C# 统一构建计划）：<https://godotengine.org/priorities/>
- Godot 4.0 C# 文章（Future 部分，明确"keeping the scripting language implementation"）：<https://godotengine.org/article/whats-new-in-csharp-for-godot-4-0/>
- GitHub proposals #2333 "From embedding Mono to Godot as a library"：<https://github.com/godotengine/godot-proposals/issues/2333>
- Godot 官方 GDScript 静态类型：<https://docs.godotengine.org/en/4.6/tutorials/scripting/gdscript/static_typing.html>
- Godot 官方 GDExtension：<https://docs.godotengine.org/en/4.6/tutorials/scripting/gdextension/what_is_gdextension.html>
- GDScript namespace 提案：GitHub godot-proposals #1566
- GDScript generics 提案：GitHub godot-proposals #1207 / godot issues #30180
- GodotJS：<https://github.com/godotjs/GodotJS>（issue #76 等已知问题）
- godot-ts：<https://github.com/godotjs/godot-ts>
- godot-rust / gdext：<https://github.com/godot-rust/gdext>
- godot-rust 2025 回顾：<https://godot-rust.github.io/dev/godot-rust-2025-review/>
- Chickensoft GDScript vs C# 对比：<https://chickensoft.games/blog/gdscript-vs-csharp>
- GDScript large project 组织：StackOverflow 78057743
- GDScript cyclic dependency：GitHub godot issues #73983

MCP：

- 本次通过 Context7 MCP 查询 Godot 4.6 官方文档，获取 C# 平台支持、GDScript 类型系统、语言支持状态的官方说明。
- 本次通过 web 搜索和 read_url_content 直接访问 Godot 官方 Priorities 页面、Godot 4.0 C# 文章，获取 C# 未来路线的原文。
