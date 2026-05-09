# DataOS Snapshot Manifest 与 Validation Report

> 更新日期：2026-05-09  
> 状态：contract draft  
> 适用范围：DataOS generated snapshot、source trace、structured validation report、Observation sidecar。

## 定位

`runtime_snapshot.json` 仍是 Runtime 消费的主要数据输入。Manifest 和 validation report 是追溯、验证和 Observation sidecar，用于回答：

- snapshot 从哪些 authoring source 生成。
- 使用哪个 schema / migration / generator version。
- 生成时有哪些 warning / error。
- 资源路径是 migrated、legacy、missing 还是 intentionally dropped。
- DataOS 字段类型是否与 Runtime DataKey/DataMeta 一致。

Manifest 和 validation report 不进入 gameplay hot path，不要求 Runtime 查询 authoring SQLite。

## 文件约定

推荐输出：

```text
runtime_snapshot.json
runtime_snapshot.manifest.json
dataos_validation_report.json
```

`runtime_snapshot.json` 可继续保持当前结构。`runtime_snapshot.manifest.json` 和 `dataos_validation_report.json` 可以由 generator / validator 后续分阶段实现。

## Snapshot Manifest 字段

Manifest 最低字段：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `snapshotVersion` | string | Snapshot contract 版本。 |
| `schemaVersion` | int | DataOS schema version。 |
| `migrationVersion` | int | 最新 migration version。 |
| `generatorVersion` | string | generator 脚本或工具版本。 |
| `generatedAtUtc` | string | UTC ISO-8601 生成时间；默认可保持稳定时间以避免反复改动。 |
| `sourcePaths` | string[] | 参与生成的 schema、migration、seed、override 路径。 |
| `sourceHashes` | object | `path -> sha256` 映射。 |
| `profilePresetVersion` | string | 使用的 GenreProfile preset 版本；没有则为空字符串。 |
| `gameOverrideVersion` | string | 游戏侧 override / seed 版本；没有则为空字符串。 |
| `validationReportId` | string | 对应 structured validation report id。 |
| `resourceDependencies` | array | 资源依赖摘要。 |
| `snapshotHash` | string | `runtime_snapshot.json` 的 sha256。 |
| `recordCounts` | object | 按 table 统计记录数量。 |
| `warningCount` | int | validation warning 数量。 |
| `errorCount` | int | validation error 数量。 |

示例形状：

```json
{
  "snapshotVersion": "1",
  "schemaVersion": 1,
  "migrationVersion": 1,
  "generatorVersion": "generate-runtime-snapshot.sh@1",
  "generatedAtUtc": "1970-01-01T00:00:00Z",
  "sourcePaths": [
    "DataOS/Migrations/001_initial.sql",
    "Games/BrotatoLike/DataOS/Authoring/BrotatoLike.seed.sql"
  ],
  "sourceHashes": {
    "DataOS/Migrations/001_initial.sql": "sha256:..."
  },
  "profilePresetVersion": "",
  "gameOverrideVersion": "BrotatoLike.seed.sql@1",
  "validationReportId": "dataos-validation-19700101T000000Z",
  "resourceDependencies": [],
  "snapshotHash": "sha256:...",
  "recordCounts": {
    "unit.enemy": 2,
    "ability": 10
  },
  "warningCount": 0,
  "errorCount": 0
}
```

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
| `dataKeyTypeChecks` | array | DataKey/DataMeta 类型一致性检查结果。 |
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

资源路径状态：

| 状态 | 含义 |
| --- | --- |
| `migrated` | 路径指向新仓库可用资源，并由验证报告或迁移账本确认。 |
| `legacy` | 路径仍指向旧结构或旧语义，例如 `res://Src/...`。 |
| `missing` | 路径应该存在，但当前仓库找不到。 |
| `intentionally dropped` | 旧资源明确不迁移，且有账本或设计原因。 |

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

`migrated` 只表示资源路径状态，不表示玩法行为完成。旧行为是否完成必须由 migration ledger、Observation artifact 和可玩切片 PASS/FAIL evidence 证明。

## DataKey / DataMeta Type Consistency

类型一致性检查目标：

- DataOS `data_field.field_key` 必须能映射到已知 Runtime DataKey，或被 report 标记为 unknown。
- DataOS `data_field.value_type` 必须与 Runtime `DataMeta` 期望类型一致。
- 枚举、flags、资源路径等 string 字段可以先作为 string 检查，但 report 必须保留进一步校验入口。
- 未注册 DataMeta 的字段不应自动失败；第一阶段可标记 warning，直到对应 DataKey owner 明确迁移状态。

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

- 如果新路径已存在并已写入 authoring data，旧路径标记为 `migrated` 或从 seed 中删除。
- 如果旧路径仍保留但后续需要迁，标记为 `legacy`。
- 如果旧路径应该迁但当前仓库没有对应资源，标记为 `missing`。
- 如果旧资源明确不再需要，标记为 `intentionally dropped`，并引用迁移账本原因。

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

- generator 输出 `runtime_snapshot.manifest.json`。
- validator 输出 `dataos_validation_report.json`。
- 设计 DataKey/DataMeta manifest 或导出机制，用于 validator 做类型一致性检查。
- 把 validation report 接入 Observation artifact。
