#!/usr/bin/env bash
set -euo pipefail

if [ "$#" -ne 2 ]; then
    echo "usage: $0 <authoring.db> <output.json>" >&2
    exit 2
fi

db_path="$1"
output_path="$2"
repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
generated_at_utc="${DATAOS_GENERATED_AT_UTC:-1970-01-01T00:00:00Z}"
validation_report_path="${DATAOS_VALIDATION_REPORT_PATH:-}"
profile="${DATAOS_PROFILE:-framework}"
catalog_id="${DATAOS_CATALOG_ID:-$profile}"

if [ ! -f "$db_path" ]; then
    echo "DataOS db not found: $db_path" >&2
    exit 1
fi

if [ -n "$validation_report_path" ]; then
    DATAOS_REPORT_PATH="$validation_report_path" "$repo_root/DataOS/Validation/validate-dataos.sh" "$db_path"
else
    "$repo_root/DataOS/Validation/validate-dataos.sh" "$db_path"
fi
mkdir -p "$(dirname "$output_path")"

sqlite3 "$db_path" > "$output_path" <<SQL
PRAGMA foreign_keys = ON;

WITH
enabled_capabilities AS (
    SELECT capability_id
    FROM capability_manifest
    WHERE enabled = 1
    ORDER BY capability_id
),
active_fields AS (
    SELECT f.table_id, f.record_id, f.field_key, f.value_type, f.value_text, d.owner_capability
    FROM data_field f
    JOIN data_key_descriptor d ON d.stable_key = f.field_key
    JOIN capability_manifest c ON c.capability_id = d.owner_capability
    WHERE c.enabled = 1
),
record_docs AS (
    SELECT
        r.table_id,
        r.record_id,
        json_object(
            'table', r.table_id,
            'id', r.record_id,
            'name', r.display_name,
            'fields', COALESCE((
                SELECT json_group_object(f.field_key, json(
                    CASE
                        WHEN f.value_type = 'int' THEN json_object('type', f.value_type, 'value', CAST(f.value_text AS INTEGER))
                        WHEN f.value_type = 'float' THEN json_object('type', f.value_type, 'value', CAST(f.value_text AS REAL))
                        WHEN f.value_type = 'double' THEN json_object('type', f.value_type, 'value', CAST(f.value_text AS REAL))
                        WHEN f.value_type = 'bool' THEN json_object('type', f.value_type, 'value', json(CASE lower(f.value_text) WHEN 'true' THEN 'true' ELSE 'false' END))
                        ELSE json_object('type', f.value_type, 'value', f.value_text)
                    END
                ))
                FROM active_fields f
                WHERE f.table_id = r.table_id
                  AND f.record_id = r.record_id
                ORDER BY f.field_key
            ), json('{}'))
        ) AS doc
    FROM data_record r
    WHERE EXISTS (
        SELECT 1
        FROM active_fields f
        WHERE f.table_id = r.table_id
          AND f.record_id = r.record_id
    )
    ORDER BY r.table_id, r.record_id
),
descriptor_docs AS (
    SELECT json_object(
        'stableKey', d.stable_key,
        'ownerCapability', d.owner_capability,
        'ownerSkill', d.owner_skill,
        'valueType', d.value_type,
        'defaultValue', d.default_value_text,
        'displayName', d.display_name,
        'description', d.description,
        'iconPath', d.icon_path,
        'category', d.category,
        'minValue', d.min_value,
        'maxValue', d.max_value,
        'options', json(d.options_json),
        'isPercentage', json(CASE d.is_percentage WHEN 1 THEN 'true' ELSE 'false' END),
        'supportsModifiers', json(CASE d.supports_modifiers WHEN 1 THEN 'true' ELSE 'false' END),
        'isComputed', json(CASE d.is_computed WHEN 1 THEN 'true' ELSE 'false' END)
    ) AS doc
    FROM data_key_descriptor d
    JOIN capability_manifest c ON c.capability_id = d.owner_capability
    WHERE c.enabled = 1
      AND EXISTS (SELECT 1 FROM active_fields f WHERE f.field_key = d.stable_key)
    ORDER BY d.stable_key
),
resource_docs AS (
    SELECT json_object(
        'category', r.category,
        'key', r.resource_key,
        'path', r.resource_path,
        'ownerCapability', r.owner_capability,
        'legacyStatus', r.legacy_status
    ) AS doc
    FROM resource_entry r
    LEFT JOIN capability_manifest c ON c.capability_id = r.owner_capability
    WHERE r.owner_capability = 'shared'
       OR c.enabled = 1
    ORDER BY r.category, r.resource_key
),
counts AS (
    SELECT
        (SELECT COUNT(*) FROM descriptor_docs) AS descriptor_count,
        (SELECT COUNT(*) FROM record_docs) AS record_count,
        (SELECT COUNT(*) FROM resource_docs) AS resource_count
)
SELECT json_object(
    'schemaVersion', 2,
    'generatedAtUtc', '$generated_at_utc',
    'manifest', json_object(
        'schemaVersion', 2,
        'generatedAtUtc', '$generated_at_utc',
        'profile', '$profile',
        'catalogId', '$catalog_id',
        'enabledCapabilities', COALESCE((SELECT json_group_array(capability_id) FROM enabled_capabilities), json('[]')),
        'descriptorCount', (SELECT descriptor_count FROM counts),
        'recordCount', (SELECT record_count FROM counts),
        'resourceCount', (SELECT resource_count FROM counts),
        'validation', json_object('warningCount', 0, 'errorCount', 0)
    ),
    'descriptors', COALESCE((SELECT json_group_array(json(doc)) FROM descriptor_docs), json('[]')),
    'records', COALESCE((SELECT json_group_array(json(doc)) FROM record_docs), json('[]')),
    'resources', COALESCE((SELECT json_group_array(json(doc)) FROM resource_docs), json('[]'))
);
SQL
