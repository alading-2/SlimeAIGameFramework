# DataOS

`DataOS` 是 AI 原生数据层。

## 目标

- 使用 SQLite 作为 authoring 真相源。
- 通过 schema 和迁移表达约束。
- 生成 C# / JSON snapshot 供 Runtime 使用。
- 运行时热路径不直接依赖任意 SQL 查询。

## 子域

- `Schema/`
- `Migrations/`
- `Generators/`
- `Snapshots/`
- `Analytics/`

## 当前最小闭环

- `Schema/core.sql`：DataOS core schema 参考。
- `Migrations/001_initial.sql`：可执行 SQLite migration。
- `capability_manifest`：声明 profile 中 capability 的 owner skill、enabled 状态、依赖和 trim policy。
- `data_key_descriptor`：声明 stable DataKey 的 authoring metadata、owner capability、value type、display/default mirror、range/options 和 modifier/computed 标记。
- `Generators/generate-runtime-snapshot.sh`：把 authoring DB 生成 typed Runtime JSON snapshot，输出 `manifest / descriptors / records / resources`。
- `Validation/validate-dataos.sh`：校验外键、空键、bool 值、descriptor 覆盖、type/default drift、disabled capability trimming、资源分类和 `res://` 路径，并输出结构化 report。
- `Tools/run-dataos-validate.sh`：框架侧最小 DataOS 验证入口，已接入 `Tools/run-tests.sh`。
- BrotatoLike 已用同一 schema 扩展 seed，覆盖 Unit / TargetingIndicator / Ability / ChainAbility / Feature / System / Spawn / ResourcePaths 第一批，并通过游戏侧 active catalog + typed snapshot loader 消费 snapshot。

## Snapshot 契约

生成的 JSON 由 `SlimeAI.GameOS.Runtime.Data.RuntimeDataSnapshot` 消费：

- `manifest` 记录 `schemaVersion / generatedAtUtc / profile / catalogId / enabledCapabilities / descriptorCount / recordCount / resourceCount / validation`。
- `descriptors[]` 是 active profile 的 DataKey descriptor mirror；每个 active snapshot field 必须有且只有一个 descriptor。
- `records[].table` 是表 Id，例如 `unit.enemy`。
- `records[].id` 是稳定记录 Id。
- `records[].name` 是显示名或旧 DataNew `Name`。
- `records[].fields` 的键必须能通过 active `DataCatalog` resolve 到 `DataKey<T>`，值包含 `type` 和 `value`。
- `resources[]` 会注册到 `ResourceCatalog`，`category` 使用 `ResourceCategory` 枚举名。
- 游戏正式入口应读取 generated snapshot，例如 BrotatoLike 的 `BrotatoLikeDataOSBootstrap`，不得在 Runtime 热路径查询 SQLite。
- loader 对 unknown key、wrong type、descriptor missing/extra、descriptor type/default drift 和 disabled capability resource 报错；不能把坏数据静默替换为 runtime default。

生成器默认写入稳定 `generatedAtUtc = 1970-01-01T00:00:00Z`，避免构建反复改动 snapshot；需要真实时间时传入 `DATAOS_GENERATED_AT_UTC`。

## 验证

```bash
Tools/run-dataos-validate.sh
Tools/run-tests.sh
```
