# DataOS Snapshot Manifest 与 Validation Report

> 更新日期：2026-05-13  
> 状态：contract draft  
> 适用范围：DataOS generated snapshot、source trace、structured validation report、Observation sidecar。

## 定位

`runtime_snapshot.json` 仍是 Runtime 消费的主要数据输入。Manifest 和 validation report 是追溯、验证和 Observation sidecar，用于回答：

- snapshot 从哪些 authoring source 生成。
- 使用哪个 schema / migration / generator version。
- 生成时有哪些 warning / error。
- 资源路径是 migrated、legacy、missing 还是 intentionally dropped。
- DataOS 字段类型是否与 Runtime `DataKey<T>` / active `DataCatalog` 一致。

Manifest 和 validation report 不进入 gameplay hot path，不要求 Runtime 查询 authoring SQLite。

## 文件约定

推荐输出：

```text
runtime_snapshot.json
runtime_snapshot.manifest.json
dataos_validation_report.json
```

当前 `runtime_snapshot.json` 已内嵌 `manifest / descriptors / records / resources` 四段。独立 `runtime_snapshot.manifest.json` 和 `dataos_validation_report.json` 可作为后续 sidecar 输出，但 runtime loader 的正式输入以 generated snapshot 内嵌 manifest/descriptors 为准。

## Snapshot Manifest 字段

Manifest 最低字段：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `schemaVersion` | int | DataOS schema version。 |
| `generatedAtUtc` | string | UTC ISO-8601 生成时间；默认可保持稳定时间以避免反复改动。 |
| `profile` | string | active profile，例如 `framework` 或 `brotatolike`。 |
| `catalogId` | string | active `DataCatalog` id。 |
| `enabledCapabilities` | string[] | snapshot 中启用的 capability id。 |
| `descriptorCount` | int | `descriptors[]` 数量。 |
| `recordCount` | int | `records[]` 数量。 |
| `resourceCount` | int | `resources[]` 数量。 |
| `validation.warningCount` | int | validation warning 数量。 |
| `validation.errorCount` | int | validation error 数量。 |

示例形状：

```json
{
  "schemaVersion": 2,
  "generatedAtUtc": "1970-01-01T00:00:00Z",
  "profile": "brotatolike",
  "catalogId": "brotatolike",
  "enabledCapabilities": ["Ability", "Movement", "Projectile"],
  "descriptorCount": 124,
  "recordCount": 37,
  "resourceCount": 39,
  "validation": {
    "warningCount": 0,
    "errorCount": 0
  },
}
```

## Snapshot Descriptors

`descriptors[]` 是 authoring metadata mirror，不进入 gameplay hot path。字段包括：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `stableKey` | string | Runtime `DataKey<T>.StableKey`。 |
| `ownerCapability` | string | 拥有该 key 的 capability 或 profile。 |
| `ownerSkill` | string | AI owner skill。 |
| `valueType` | string | DataOS canonical type：`bool / int / float / double / string`。 |
| `defaultValue` | string | C# runtime default 的 authoring mirror。 |
| `displayName / description / iconPath / category` | string | 工具和文档 metadata。 |
| `minValue / maxValue / options / isPercentage` | mixed | authoring validation metadata。 |
| `supportsModifiers / isComputed` | bool | 与 runtime key contract 对齐的校验标记。 |

loader 必须先通过 active `DataCatalog` resolve `stableKey`，再比较 descriptor `valueType/defaultValue` 与 `DataKey<T>` runtime contract。drift 是错误，不允许把字段当作缺省值处理。

## Validation Report 字段

Structured validation report 最低字段：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `reportId` | string | 稳定 report id。 |
| `generatedAtUtc` | string | UTC ISO-8601 生成时间。 |
| `dbPath` | string | 被校验的 authoring DB 路径或逻辑名。 |
| `schemaVersion` | int | schema version。 |
| `migrationVersion` | int | migration version。 |
| `checks` | array | 每项检查的名称、状态、数量和 message。 |
| `resources` | array | 资源路径检查结果。 |
| `dataKeyTypeChecks` | array | DataKey descriptor / runtime catalog 类型一致性检查结果。 |
| `warnings` | array | warning 明细。 |
| `errors` | array | error 明细。 |
| `summary` | object | 总计和 PASS/FAIL。 |

示例形状：

```json
{
  "reportId": "dataos-validation-19700101T000000Z",
  "generatedAtUtc": "1970-01-01T00:00:00Z",
  "dbPath": "authoring.db",
  "schemaVersion": 1,
  "migrationVersion": 1,
  "checks": [
    {
      "name": "foreign_key_check",
      "status": "pass",
      "warningCount": 0,
      "errorCount": 0
    }
  ],
  "resources": [],
  "dataKeyTypeChecks": [],
  "warnings": [],
  "errors": [],
  "summary": {
    "status": "pass",
    "warningCount": 0,
    "errorCount": 0
  }
}
```

## Resource Status

资源路径状态与操作语义（与 `DataOS/Overview.md#AI 数据操作协议` 对齐）：

| 状态 | 含义 | 进 snapshot | 保留在 seed SQL |
| --- | --- | --- | --- |
| `active` | 当前使用中。 | ✅ | ✅ |
| `legacy-input` | 旧路径但仍在用，迁移中。 | ✅ | ✅ |
| `legacy` | 兼容保留，暂不删除。validator 报 warning。 | ✅ | ✅ |
| `intentionally-dropped` | 已废弃，确认不迁移。 | ❌ | ❌ — 确认后应 DELETE |
| `missing` | 引用丢失。 | ❌ | ❌ — 应 DELETE 或修复路径 |

操作规则：

- `intentionally-dropped` 和 `missing` 是迁移过程中的临时标注，确认后必须从 seed SQL 中 DELETE。不要长期保留死行。
- Generator 不输出 `intentionally-dropped` / `missing` 资源到 snapshot JSON。
- Runtime `RegisterResourcesWithReport()` 跳过 `legacyStatus` 为 `intentionally-dropped` / `missing` 的资源。
- Validator 对 `intentionally-dropped` / `missing` 行报 warning，提示应清理。
- 迁移审计靠 `git log` 和 OpenSpec change 历史，不靠数据库里的死行。

资源检查项示例：

```json
{
  "category": "Asset",
  "key": "old-main-scene",
  "path": "res://Src/Main/Main.tscn",
  "status": "legacy",
  "reason": "old Src path remains in authoring data",
  "ledgerRef": "create-brotatolike-migration-ledger"
}
```

`active` 只表示资源路径状态，不表示玩法行为完成。旧行为是否完成必须由 migration ledger、Observation artifact 和可玩切片 PASS/FAIL evidence 证明。

## DataKey Type Consistency

类型一致性检查目标：

- DataOS `data_field.field_key` 必须能映射到 active catalog 中的 Runtime `DataKey<T>`，否则 snapshot loader 报 `snapshot.unknown_key`。
- DataOS `data_field.value_type`、descriptor `valueType` 和 Runtime `DataKey<T>` 类型必须一致。
- descriptor `defaultValue` 必须 mirror `DataKey<T>.DefaultValue`；不一致时报 `snapshot.default_drift`。
- 枚举和 flags 在 DataOS 中按 canonical string 编码，loader 通过 typed key 转换到 enum 类型。
- 未注册或被 disabled capability trim 掉的字段、descriptor、resource 不能进入 active snapshot。

检查项示例：

```json
{
  "table": "unit.player",
  "recordId": "deluyi",
  "fieldKey": "Movement.MoveSpeed",
  "authoringType": "float",
  "runtimeType": "float",
  "status": "pass"
}
```

## BrotatoLike Legacy Path 分类

遇到 `res://Src/...` 时：

- 如果新路径已存在并已写入 authoring data，旧路径标记为 `legacy-input` 或从 seed 中 DELETE。
- 如果旧路径仍保留但后续需要迁，标记为 `legacy-input`。
- 如果旧路径应该迁但当前仓库没有对应资源，标记为 `missing`；确认后 DELETE 该行或修复路径。
- 如果旧资源明确不再需要，标记为 `intentionally-dropped`；确认后 DELETE 该行。不要长期保留。

DataOS validator 或 migration ledger 必须至少有一个地方记录该分类，避免把旧路径存在误判为已迁移。

## Runtime 边界

Runtime 允许：

- 读取 `runtime_snapshot.json`。
- 在 debug / Observation 场景读取 manifest 或 validation report。
- 把 snapshot records 写入 `Data`，把 resources 注册到 `ResourceCatalog`。

Runtime 禁止：

- 在 gameplay hot path 查询 authoring SQLite。
- 根据 authoring DB 动态决定玩法逻辑。
- 把 DataOS record 存在当成玩法迁移完成证据。

## 后续实现任务

- 如需要外部审计 artifact，再输出独立 `runtime_snapshot.manifest.json`。
- 如需要 CI 长期留档，再输出独立 `dataos_validation_report.json`。
- 把 validation report 接入 Observation artifact。
