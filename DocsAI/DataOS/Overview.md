# DataOS

`DataOS` 是 AI 原生数据层。

## 目标

- 使用 SQLite 作为 authoring 真相源。
- 业务数据先用清晰业务表表达，再投影成 Runtime 机器友好的 snapshot。
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
- `Migrations/001_initial.sql`：核心兼容 schema，保留 `data_record / data_field / data_key_descriptor / resource_entry`。
- `Migrations/002_table_first_authoring.sql`：table-first authoring schema，定义 `unit_player / unit_enemy / ability / ability_effect / ability_projectile / ability_movement_* / feature_definition / feature_modifier / system_config / system_preset / spawn_config` 等业务表，并通过 `dataos_runtime_field_stream` 投影到 runtime fields。
- `capability_manifest`：声明 profile 中 capability 的 owner skill、enabled 状态、依赖和 trim policy。
- `data_key_descriptor`：显式声明 stable DataKey 的 authoring metadata、owner capability、value type、display/default mirror、range/options 和 modifier/computed 标记；descriptor 不能从数据行反推。
- `data_record / data_field`：兼容层和 projection 输出形状，不再是 Unit / Ability / Feature / System / Spawn 等业务内容的首选手写入口。
- `resource_entry`：资源索引、全局 lookup 和 legacy 分类面；单位 visual、技能 projectile/effect 等内容归属路径优先写在业务表列中。
- `Generators/generate-runtime-snapshot.sh`：把 authoring DB 的业务表投影为 typed Runtime JSON snapshot，输出 `manifest / descriptors / records / resources`。
- `Validation/validate-dataos.sh`：校验外键、空键、业务表必填字段、canonical bool、descriptor 覆盖、projection type/default drift、disabled capability trimming、资源分类和 `res://` 路径，并输出结构化 report。
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

## AI 数据操作协议

DataOS 是 AI-first 框架的事实源。AI 操作数据库时必须遵循以下规则：

### AI-first 不是万能 key-value

- AI-first DataOS 的目标是让人和 AI 少猜、少扫描、少靠上下文脑补；不是把业务数据库退化成万能 key-value / EAV 主入口。
- 新增或迁移业务内容时，优先写清晰业务表：单位写 `unit_player / unit_enemy / unit_targeting_indicator`，技能写 `ability` 与 `ability_effect / ability_projectile / ability_movement_*`，Feature 写 `feature_definition / feature_modifier`，调度和生成写 `system_config / system_preset / spawn_config`。
- Generator 负责把业务列投影成 `DataKey<T>` stable key；Runtime snapshot shape 仍保持 `manifest / descriptors / records / resources`。
- `data_field` 只作为兼容输入或投影结果语义存在。除非迁移兼容或框架测试 fixture 明确需要，不要把新的游戏业务字段手写成 `table_id / record_id / field_key / value_text` 行。
- `data_key_descriptor` 必须显式维护。Validator 会拒绝缺 descriptor 或 value type 不匹配的投影字段，不能因为某条数据行存在就自动合法化 descriptor。
- `resource_entry` 不再是所有 Asset 的内容主表。只有全局 `ResourceCatalog` lookup、legacy/migration 分类或无法自然归属单一业务行的资源才写入 `resource_entry`；业务拥有的 path 写在业务表列中。

### seed SQL 是活数据，不是审计日志

- seed SQL 只保留当前有效数据。已废弃、已确认不迁移的资源应直接 `DELETE`，不要留在表里标状态。
- 迁移审计靠 `git log` 和 OpenSpec change 历史，不靠数据库里的死行。
- AI 查询 `SELECT * FROM resource_entry` 时应看到全部活资源，不需要额外 `WHERE` 过滤死行。

### legacy_status 操作语义

| 值 | 含义 | 进 snapshot | 保留在 seed SQL |
| --- | --- | --- | --- |
| `active` | 当前使用中 | ✅ | ✅ |
| `legacy-input` | 旧路径但仍在用，迁移中 | ✅ | ✅ |
| `legacy` | 兼容保留，暂不删除 | ✅（validator 报 warning） | ✅ |
| `intentionally-dropped` | 已废弃，确认不迁移 | ❌ | ❌ — 应 DELETE |
| `missing` | 引用丢失 | ❌ | ❌ — 应 DELETE 或修复路径 |

- `intentionally-dropped` 和 `missing` 是**迁移过程中的临时标注**，确认后必须从 seed SQL 中删除。
- Generator 不输出 `intentionally-dropped` / `missing` 资源到 snapshot JSON。
- Runtime `RegisterResourcesWithReport()` 跳过 `intentionally-dropped` / `missing` 资源（防御性检查）。
- Validator 对 `intentionally-dropped` / `missing` 行报 warning，提示应清理。

### AI 增删改规则

- **新增资源**：`INSERT OR REPLACE INTO resource_entry`，`legacy_status` 默认 `'active'`。
- **迁移中资源**：旧路径标 `'legacy-input'`，新路径标 `'active'`；迁移完成后 DELETE 旧行。
- **废弃资源**：先标 `'intentionally-dropped'` 确认意图，确认后 DELETE 该行。不要长期保留 `'intentionally-dropped'` 行。
- **修改资源**：直接 `INSERT OR REPLACE` 覆盖。不需要保留旧值行。

## 功能开关：DataOS 层

DataOS 是功能开关的第一层（最底层）。在 seed SQL 中通过 `capability_manifest.enabled` 控制：

```sql
-- 关闭某个 Capability：其 DataKey 不进 DataCatalog，运行时无法访问
INSERT OR IGNORE INTO capability_manifest(capability_id, enabled, ...)
VALUES ('AI', 0, ...);  -- AI 系统不会进入 snapshot

-- 框架自测用的 DisabledProbe：enabled=0, trim_policy='trim'
-- Generator 会自动裁剪，不进游戏 snapshot
```

与运行期开关的区别：

| 属性 | DataOS 开关 | RuntimeSchedule 开关 | ProjectState 开关 |
| --- | --- | --- | --- |
| 生效时机 | snapshot 生成时 | 运行时即时 | 运行时即时 |
| 持久性 | 永久（直到下次改 seed 重新生成） | 当前会话 | 当前会话 |
| 回滚方式 | 改 seed SQL + 重新生成 snapshot | `SetSystemEnabled(id, true)` | `ProjectState.ClosePauseMenu()` 等 |
| 适用场景 | 该游戏根本不需要某个 Capability | 局内临时关闭刷怪/AI | 暂停/菜单/过场 |

完整开关模型见 `DocsAI/GameOS/Overview.md#功能开关总览`。

## 验证

```bash
Tools/run-dataos-validate.sh
Tools/run-tests.sh
```
