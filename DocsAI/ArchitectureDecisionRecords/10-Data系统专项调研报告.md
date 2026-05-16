# Data 系统专项调研报告

> 日期：2026-05-12
> 任务：用户提出 SlimeAI Runtime Data / DataMeta / DataKey 全面重构。要求：
> 1. 不做兼容；数据/事件是框架根基，必须做好。
> 2. DataDescriptor 装饰字段（DisplayName / Description / IconPath / IsPercentage）保留，但应考虑写进 DataOS 数据库。
> 3. authoring DB 和生成的 snapshot 应统一位置。
> 4. 按 Capability 裁剪：系统删了，对应数据不加载（指游戏启动前的设置阶段）。
> 5. AI-first：数据只为 AI 服务，不为人类工程师方便而保留多余字段。
>
> 输入证据：本地报告 01-09 + 99 综合；SlimeAI 当前 Runtime Data / DataMeta / DataRegistry / Capability DataKeys 源码；DataOS schema / generator / validator / RuntimeDataSnapshot 现状；Unreal GAS UPROPERTY meta 与 Unity Inspector binding 行业事实。
>
> 本报告不写代码，只产出设计事实和可执行方向；基于这份报告开后续 OpenSpec change。

## 0. 2026-05-12 复核结论

本报告的主方向成立：Data 系统需要重构，DataOS 继续走 `SQLite authoring -> validation -> JSON snapshot -> C# runtime loader`，运行时不直接读 SQLite；旧 `DataMeta` 合体模型需要拆分；业务代码不应继续依赖裸 string key。

但报告原先对“方案 B：双源校验”的表述需要修正：

1. **双源的根源不是 C# 强类型。** 根源是 `authoring 数据事实` 和 `runtime 可执行契约` 本来属于两个事实域。弱类型语言不会消灭这个边界，只会把错误推迟到 runtime / validator / 测试。
2. **方案 B 不是两套真相源。** 正确理解应是：C# 持有可执行契约，DB 持有 authoring 事实，二者重叠字段由 validator 强校验。
3. **默认值 owner 原文有矛盾。** 文中既写 `DataKey<T>` 持有 `Default`，又写 C# 默认值删除。后续 OpenSpec 必须先定：runtime default 归 C#，DB `default_value_text` 作为 authoring default / validator mirror；除非显式标记 override，否则二者必须一致。
4. **“typed API = 零 boxing”不能直接成立。** `Get(DataKey<T>)` 只能保证调用侧类型安全；要零 boxing 还必须改存储层为 typed slot、type-partitioned store 或 generated accessor。
5. **AI-first 不是少校验。** AI-first 应减少热路径防御性转换和静默兜底，但增加 DB / generator / snapshot / catalog 的机器校验。

因此，后续设计以新增文档 `深度分析：C# 对 AI-first 框架是限制还是增强.md` 的结论为准：保留 C#，重构 Data 分层，不换 TS / Rust，不削弱 authoring 校验。

## 1. 调研目标

回答四个问题，全部按"AI-first 数据是框架根基"的视角：

1. **Q1 装饰元数据是否应该入库？** 即 `DisplayName / Description / IconPath / IsPercentage / Category / Min / Max / Options` 这些 authoring 装饰字段，是放在 C# 静态 DataMeta 里，还是放进 DataOS 数据库表。
2. **Q2 DataKey 是否应该完全 typed？** 即 `data.Get<float>(DamageKeys.MaxHp)` 能否消成 `data.Get(DamageKeys.MaxHp)` 直接返回 `float`，零 boxing。
3. **Q3 是否应该按 Capability 裁剪 DataKey 加载？** 即"游戏启动前选择启用哪些 Capability，对应 DataKey / Descriptor / 数据记录只加载启用的部分"。
4. **Q4 DataOS 物理布局如何统一？** authoring DB、生成的 snapshot JSON、中间产物、框架默认 seed、游戏 seed 该如何组织。

## 2. 用户两个新点的专项验证

### 2.1 Q1：装饰元数据入库 — 结论 `Adopt Now`

**用户判断成立**。装饰元数据本质上就是表格数据，没有理由用 C# 静态代码硬编码。行业头部引擎全部支持这条路：

| 引擎 | 装饰元数据存放位置 | 运行时读取方式 |
|---|---|---|
| Unreal Engine | `UPROPERTY(meta=(DisplayName="生命上限", ClampMin="0", Tooltip="..."))` 写在 C++ 字段上 | reflection 读取 `FProperty::GetMetaData("DisplayName")`；编辑器 Inspector 自动可见 |
| Unity | `[Tooltip("...")]`、`[Range(0, 100)]`、`[Header("...")]` C# attribute | Editor `SerializedProperty` 通过 reflection 拿 attribute；Runtime 完全不读 |
| Unity DOTS Baker | authoring `MonoBehaviour` 字段持有装饰；Baker 转成 `IComponentData`（runtime 字段无装饰） | runtime 只见 typed value，**装饰在 baking 阶段被抛弃** |
| Bevy | Rust attribute `#[reflect(Default, PartialEq)]` + `bevy_inspector_egui` | reflection + editor crate；runtime 不读 |
| QFramework | 没有装饰元数据系统，UI 绑定靠 `BindableProperty<T>` 直接读值 | 不参考 |

**关键事实**：装饰元数据**只服务 authoring time + 编辑器 / UI binding**。三家引擎都把它附在字段上（reflection metadata 或 attribute），但**没有任何一家把它放进运行时业务热路径**。

SlimeAI 当前的 `DataMeta` 把装饰字段和 runtime 行为字段（Type / DefaultValue / SupportModifiers / Compute）揉在一起，违反 authoring/runtime 边界。本地报告 `06-UnityCsReference` 3.5 节、`07-Unity-Entities-Samples` 3.2 节都明确指出这条边界。

**改进方向**：

```text
authoring 层（DataOS DB 里的一张表）
  data_key_descriptor 表：
    key TEXT PK
    capability_id TEXT FK → capability_manifest（Q3 用）
    value_type TEXT NOT NULL            -- "float" / "int" / "bool" / "string" / "Vector2"
    default_value_text TEXT NOT NULL    -- 反序列化时 parse 成 typed value
    support_modifiers INTEGER NOT NULL DEFAULT 0
    has_compute INTEGER NOT NULL DEFAULT 0   -- 是否有 computed 函数（runtime C# 注册）
    display_name TEXT NOT NULL DEFAULT ''
    description TEXT NOT NULL DEFAULT ''
    icon_path TEXT NOT NULL DEFAULT ''
    is_percentage INTEGER NOT NULL DEFAULT 0  -- 用户确认保留：内部 0-100，公式 *0.01
    category TEXT NOT NULL DEFAULT ''
    min_value REAL
    max_value REAL
    options_json TEXT
    owner_skill TEXT NOT NULL DEFAULT ''

runtime 层（C# 静态注册）
  DataKey<T> 只持有：Id, StableKey, Default, SupportModifiers, Compute
  没有 DisplayName / Description / IconPath 等装饰字段

加载流程
  1. snapshot 加载阶段：从 snapshot.descriptors[] 反序列化进 DataDescriptorRegistry
  2. UI / HUD binder：通过 stableKey 查 DataDescriptorRegistry
  3. Capability tick：完全不访问 DataDescriptorRegistry
```

**好处**：

- 改 DisplayName 不需要重新编译 C#。
- 多语言可以直接加列 `display_name_zh / display_name_en`。
- AI 修改装饰只需要改 DB 行，不碰 C# 代码，路径更窄。
- runtime 热路径无装饰字段，cache locality 更好。

**潜在风险**：

- C# 里 DataKey 注册集合 vs DB 里 descriptor 集合可能漂移。
- 解决：DataOS validator 比对两边，缺失或多余都报错。

### 2.2 Q3：按 Capability 裁剪 — 结论 `Adopt Now`

**用户判断成立**。本地多份报告对这一方向有强证据：

| 引擎 | 模块化裁剪机制 | 关键证据 |
|---|---|---|
| Bevy | `DefaultPlugins` vs `MinimalPlugins`；Cargo feature 编译期裁剪；`PluginGroup` 运行期启用 | 01 报告 3.1 |
| Flecs | module / import；ChildOf 等内建 trait；examples 中 scene / inventory / facts 分离 | 02 报告 3 |
| Unreal Modular Game Features | 每个 GameFeature 是独立 plugin + PrimaryDataAsset；启用时执行 actions（注册 abilities、加载 data） | 09 报告 3.6 |
| SlimeAI GenreProfile | 每个 GenreProfile 声明默认能力、候选能力、DataOS preset | 99 综合 3.3 |

**实现路径**：

```text
authoring DB
├─ capability_manifest 表
│    capability_id PK
│    owner_skill TEXT
│    status TEXT          -- Draft / Experimental / Supported / Deprecated
│    enabled_by_default INTEGER
│    depends_on TEXT      -- JSON array of capability_id
├─ data_key_descriptor   （带 capability_id FK）
├─ data_record           （带 capability_id FK 或通过 data_table.capability_id 间接归属）
└─ resource_entry        （带 capability_id FK）

snapshot generator 入参：
  --enabled-capabilities movement,damage,ai,collision,...
  → 只输出启用 capability 的 records / resources / descriptors

C# 侧：
  *DataKeys.cs 仍按 Capability 组织（代码层面解耦）
  Capability 启动时调用自己的 RegisterAll(IDataRegistry registry)
  Runtime DataRegistry 在 snapshot 加载时只接受 snapshot 里出现的 key
  未启用 Capability：C# 代码可以保留，但 DataKey 不进入 runtime registry
```

**风险与对策**：

- 跨 Capability 硬引用：Ability 引用 `Damage.MaxHp`，禁用 Damage 会破坏 Ability。
  → 解决：每个 Capability manifest 写 `depends_on`，validator 检查"启用 Capability 的依赖必须也启用"。
- 测试覆盖：profile 组合容易爆炸。
  → 解决：先只验证 `MinimalProfile` / `Survivor2DProfile` 两套；其他组合通过 validator 静态检查，不跑 smoke。

## 3. Q2 与 Q4 调研结论

### 3.1 Q2：DataKey 完全 typed — 结论 `Adopt Now`

**当前问题**：

```csharp
data.Get<float>(DamageDataKeys.MaxHp);   // <float> 是冗余信息
data.Set<float>(DamageDataKeys.MaxHp, 100f);
```

`DataKey` 已经是 typed handle（`DataKey<float>`），重复写 `<T>` 既冗余又容易写错（写成 `<int>` 编译过、运行炸）。

**目标 API**：

```csharp
float hp = data.Get(DamageKeys.MaxHp);       // 自动返回 float，零 boxing
data.Set(DamageKeys.MaxHp, 100f);             // 编译期类型校验
data.TryGet(DamageKeys.MaxHp, out float v);
```

**实现方式**：

```csharp
public readonly struct DataKey<T>
{
    public int Id { get; }
    public string StableKey { get; }
    public T Default { get; }
    public bool SupportModifiers { get; }
    public Func<IEntity, T> Compute { get; }   // 可空
}

public interface IData
{
    T Get<T>(DataKey<T> key);
    bool TryGet<T>(DataKey<T> key, out T value);
    void Set<T>(DataKey<T> key, T value);
    bool Has<T>(DataKey<T> key);
    // 修饰器同理
}
```

存储层用 `Dictionary<int, IDataSlot>`（slot 是 typed wrapper）；或者按 type 拆多个字典（`Dictionary<int, float>`、`Dictionary<int, int>`、…）避免 boxing。

**Adopt Now 理由**：用户明确说不要兼容，全部走 typed。

### 3.2 Q4：DataOS 物理布局统一 — 结论 `Adopt Now`

**用户判断成立**。当前 DataOS 在 `SlimeAI/DataOS/`，snapshot 在某个生成产物路径；多份调研都建议两者紧邻：

**目标结构**：

```text
SlimeAI/
└─ DataOS/
   ├─ Schemas/                  # 静态 schema 与迁移
   │   ├─ 001_initial.sql
   │   ├─ 002_descriptor.sql
   │   └─ ...
   ├─ Authoring/                # SQLite 文件（仓库内只放 framework 默认）
   │   └─ framework-default.sqlite
   ├─ Snapshots/                # 生成产物（gitignore 或专门保留）
   │   ├─ framework-default.json
   │   └─ framework-default.manifest.json
   ├─ Generators/               # 生成器脚本
   ├─ Validators/               # 校验脚本
   └─ README.md

Games/BrotatoLike/
└─ DataOS/
   ├─ Authoring/
   │   └─ game-seed.sqlite       # 游戏 seed，引用 framework 默认
   └─ Snapshots/
       ├─ brotato.json
       └─ brotato.manifest.json
```

**关键原则**：
- authoring DB 文件和 snapshot 同目录祖先（`DataOS/`），AI 路由稳定。
- 生成 manifest 同步落地：snapshot version、source hash、enabled capabilities、validation report id。
- 框架 default 和游戏 seed 分仓：框架默认在 SlimeAI 仓，游戏覆盖在游戏仓。99 综合 6.3 已要求 `framework default → genre preset → game override → game seed` 链。

## 4. 不重构原则与 AI-first 取舍

用户明确：AI-first，不为人类工程师方便保留多余字段。这意味着：

| 取舍 | 选择 | 理由 |
|---|---|---|
| 装饰字段仍保留 `DisplayName / Description` | **保留** | UI / HUD 不可避免要展示；AI 也可能基于 description 推断字段语义 |
| `IconPath` | **保留** | UI 必需；AI 不读取，但放 DB 不增加 runtime 成本 |
| `IsPercentage` | **保留** | 内部 0-100、公式 *0.01 是确定的语义边界，AI 必须知道 |
| C# 注释 `<summary>` 注释 DataKey | **不强制** | description 已在 DB 里 |
| 静态 `*DataKeys.cs` 文件本身 | **保留** | C# 编译期类型安全的入口，AI 通过文件路径找 DataKey 比通过 DB 行更稳定 |
| C# 里写默认值 | **删除** | 默认值进 DB descriptor.default_value_text，C# 不再 hardcode |
| C# 里写 SupportModifiers | **可选保留** | 严格说能从 DB 推；但 C# 编译期决定 IDataSlot 实现，留在代码端更直观。最终建议：**留在 C#**，validator 比对两边一致 |

**最终原则**：C# 持有运行时行为契约（type、modifier 支持、computed 函数），DB 持有 authoring 装饰（display / description / category / icon / range）。两者通过 `stableKey` 关联，validator 强校验。

## 5. 候选实现方案

提出三种方案，留给用户挑选：

### 方案 A：彻底 DB 化

**摘要**：所有 DataKey 元数据全部在 DB；C# 启动时从 DataDescriptorRegistry 反向构建 `DataKey<T>` 实例。

- 优点：单一来源（DB），AI 路径最简单。
- 缺点：C# 编译期无 `DamageKeys.MaxHp` 静态引用，IDE 自动补全靠 codegen；codegen 失败时一切瘫痪。
- 评价：**不推荐**。runtime 失去类型安全。

### 方案 B：双源校验（推荐）

**摘要**：C# `*DataKeys.cs` 保留 typed handle（Id / StableKey / Default / Modifier / Compute）；DB `data_key_descriptor` 持有装饰；validator 比对两边一致。

- 优点：C# 编译期类型安全 + DB 灵活装饰。
- 缺点：两套数据需要同步；validator 必须强制运行。
- 评价：**推荐方案，但需按复核结论修正语义**。它不是“同一事实写两遍”，而是“C# 可执行契约 + DB authoring 事实 + 重叠字段强校验”。后续 OpenSpec 必须显式定义哪些字段由 C# owning，哪些字段由 DB owning，哪些字段只是 mirror。

### 方案 C：DB → C# 单向 codegen

**摘要**：DB 是唯一来源；通过 generator 生成 `*DataKeys.cs` 静态类（含 `DataKey<float> MaxHp = new(...)`）。Compute 函数仍手写。

- 优点：DB 单源；C# 自动同步。
- 缺点：生成的代码进仓库（CI 友好）vs 不进仓库（IDE 失去补全）；生成时机和提交流程复杂。
- 评价：**Adopt Later 候选**。如果方案 B 的两边漂移成为长期负担，再升级到方案 C。

## 6. 边界硬约束

无论选哪个方案，下面这些边界**必须遵守**：

1. **DataOS authoring DB 不进 runtime 热路径**（06 / 07 / 99 反复要求）。
2. **snapshot 必须带 manifest**（99 综合 6.2）：sourceHash / enabledCapabilities / schemaVersion / validationReportId。
3. **不引入第三方 ECS**：不复制 Unity DOTS、Bevy、Flecs、Arch、DefaultEcs、EnTT、QFramework 的 runtime 模型（99 综合 10）。
4. **Capability 依赖必须显式声明**：禁用 Capability 时 validator 报告所有依赖错误。
5. **DataKey 重构必须配套 Validation Tests**：`DataKey.TryGet/Get/Set` 类型安全测试、跨 Capability 裁剪 smoke、Descriptor 缺失/多余告警测试。

## 7. 风险

| 风险 | 影响 | 对策 |
|---|---|---|
| 用户要求"不兼容"，旧 brotato-my 数据可能失效 | 游戏侧 Capability 全量改写 | 用户已确认接受；按 Capability 一个一个 OpenSpec 迁移 |
| C# 与 DB 描述漂移 | runtime 拿不到 Descriptor 或拿到错误 type | DataOS validator 强校验，CI 必跑；snapshot 加载时再校验一次 |
| 按 Capability 裁剪导致 profile 组合爆炸 | 测试不可能覆盖全部 | 只验证 Minimal / Survivor2D 两套；其余靠 validator 静态校验 |
| C# `Get<T>` 改成 typed `Get(DataKey<T>)` 全仓库改动巨大 | 工作量大 | 用户确认不要兼容，一次性改完；OpenSpec change 拆按 Capability 推进 |
| Computed 函数怎么按 Capability 裁剪 | C# 代码层面解耦 | 每个 Capability 自己注册 Compute 到 DataRegistry；未启用 Capability 不注册 |
| 把 AI-first 误解成少做校验 | 数据错误被推迟到运行时场景 | 校验前移到 SQLite constraint、DataOS validator、snapshot manifest 和 C# catalog 对账；runtime hot path 只做轻量 typed access |
| typed API 被误认为自动零 boxing | 性能和契约目标落空 | 第一阶段只承诺调用侧类型安全；零 boxing 需要 typed slot / type-partitioned store / generated accessor 另行设计 |
| static global DataRegistry 阻碍 Capability 裁剪 | 禁用 Capability 仍可能因静态初始化注册 key | 后续目标改为 profile/world scoped frozen DataCatalog；static API 仅作为过渡 |

## 8. 后续行动

建议按这个顺序开 OpenSpec change：

1. **`redesign-runtime-data-and-descriptor`**（最高优先级）
   - 重写 `IData / DataKey<T> / DataRegistry` API，全 typed，零 boxing。
   - 把 DataMeta 拆成 runtime `DataKey<T>` + DB `data_key_descriptor`。
   - 验证：每个 Capability 单元测试通过、跨 Capability smoke 通过。

2. **`define-dataos-physical-layout`**
   - 固化 `SlimeAI/DataOS/{Schemas,Authoring,Snapshots,Generators,Validators}` 布局。
   - 框架 default DB 进仓库；snapshot 输出位置统一。

3. **`define-capability-manifest-and-trimming`**
   - 新增 `capability_manifest` 表 + DB 列 `capability_id`。
   - generator 支持 `--enabled-capabilities` 入参，按裁剪输出 snapshot。
   - validator 检查依赖一致性。

4. **`migrate-capability-X-to-new-data`**（每个 Capability 一个）
   - 按 Capability 把 `*DataKeys.cs` 改造、DB 入条目、测试覆盖。
   - 顺序建议：Damage → Movement → Collision → Ability → Feature → AI → Attack → Projectile → Effect → Resource → Event。

5. **`validate-and-archive-snapshot-manifest`**
   - snapshot manifest 字段固化；validator 产出 PASS/FAIL artifact。

## 9. 不采纳

| 不采纳项 | 原因 |
|---|---|
| 用 Unreal `UPROPERTY meta` 那种 string-keyed metadata 设计 | C# 无原生 reflection metadata；改用 typed column 表更直接 |
| 用 ScriptableObject / .asset 文件存装饰 | SlimeAI 已用 SQLite + JSON snapshot，不引入 Godot Resource 编辑器依赖 |
| 用 codegen 生成 `*DataKeys.cs`（方案 C） | 优先方案 B（双源校验），降低生成器复杂度 |
| 把 Description 翻译成 i18n key 系统 | 第一阶段只保留 `display_name`、`description` 单列；翻译后再加列 |
| `IsPercentage` 改成 `Unit` 枚举（Percentage / Time / Distance / Currency） | YAGNI；当前只确认 Percentage 一种特殊处理；未来按需扩列 |

## 10. SlimeAI 影响矩阵

| SlimeAI 域 | 影响 | 等级 | 后续文件 |
|---|---|---|---|
| GameOS Runtime | `IData / DataKey<T>` API 全 typed 重写；`Get/Set/TryGet` 不再要 `<T>` 参数 | `Adopt Now` | `SlimeAI/GameOS/Runtime/Data/*` |
| GameOS Runtime | DataMeta 拆成 runtime `DataKey<T>` + authoring `Descriptor`；DataRegistry 接受 snapshot.descriptors | `Adopt Now` | `SlimeAI/GameOS/Runtime/Data/DataMeta.cs / DataRegistry.cs / DataDescriptor.cs (新)` |
| Capability | 每个 `*DataKeys.cs` 改写：去掉装饰字段，只留 typed handle；Capability 启动注册 Compute | `Adopt Now` | `SlimeAI/GameOS/Capabilities/*/*DataKeys.cs` |
| DataOS | 新增 `data_key_descriptor` 表 + `capability_manifest` 表；schema 迁移 | `Adopt Now` | `SlimeAI/DataOS/Schemas/*.sql` |
| DataOS | 物理布局统一：`DataOS/{Schemas,Authoring,Snapshots,Generators,Validators}` | `Adopt Now` | `SlimeAI/DataOS/README.md` |
| DataOS | snapshot generator 支持 `--enabled-capabilities`；输出 manifest 字段固化 | `Adopt Now` | `SlimeAI/DataOS/Generators/*` |
| DataOS | validator 校验：DataKey vs Descriptor 一致、Capability 依赖完整、profile 内引用闭合 | `Adopt Now` | `SlimeAI/DataOS/Validators/*` |
| GodotBridge | UI / HUD binder 通过 DataDescriptorRegistry 查装饰（不通过 Data） | `Adopt Now` | `SlimeAI/GameOS/GodotBridge/HUD/*` |
| Validation | 新增 typed Data API smoke、Capability 裁剪 smoke、Descriptor 一致性测试 | `Adopt Now` | `SlimeAI/Tests/SlimeAI.GameOS.Tests/*` |
| Observation | DataDescriptorRegistry dump、Capability 启用列表 dump、snapshot manifest dump | `Adopt Later` | `SlimeAI/GameOS/Observation/Contract.md` |
| Agent Protocol | 新增禁止事项："不要在 C# 中 hardcode DisplayName / Description"；改 DataKey 装饰只能改 DB | `Adopt Now` | `Workspace/DocsAI/AgentWorkflow/Protocols/*` |
