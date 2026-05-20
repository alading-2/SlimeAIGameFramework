#!/usr/bin/env bash
set -euo pipefail

if [ "$#" -ne 1 ]; then
    echo "usage: $0 <authoring.db>" >&2
    exit 2
fi

db_path="$1"
report_path="${DATAOS_REPORT_PATH:-}"

if [ ! -f "$db_path" ]; then
    echo "DataOS db not found: $db_path" >&2
    exit 1
fi

source_path="${db_path//\'/\'\'}"

issue_sql="
PRAGMA foreign_keys = ON;

WITH checks AS (
    SELECT 'foreign_key' AS check_id, 'error' AS severity, '$source_path' AS source_path, '' AS stable_key,
           parent AS expected, \"table\" || ':' || rowid AS actual,
           'foreign key violation' AS summary
    FROM pragma_foreign_key_check
    UNION ALL
    SELECT 'data_table.empty_id', 'error', '$source_path', '', 'non-empty table_id', table_id,
           'data_table.table_id is empty'
    FROM data_table
    WHERE trim(table_id) = ''
    UNION ALL
    SELECT 'data_record.empty_id', 'error', '$source_path', '', 'non-empty record_id', table_id || ':' || record_id,
           'data_record.record_id is empty'
    FROM data_record
    WHERE trim(record_id) = ''
    UNION ALL
    SELECT 'data_field.empty_key', 'error', '$source_path', field_key, 'non-empty field_key', table_id || ':' || record_id,
           'data_field.field_key is empty'
    FROM data_field
    WHERE trim(field_key) = ''
    UNION ALL
    SELECT 'data_field.bool_value', 'error', '$source_path', field_key, 'true|false', value_text,
           'bool field has invalid value'
    FROM data_field
    WHERE value_type = 'bool'
      AND lower(value_text) NOT IN ('true', 'false')
    UNION ALL
    SELECT 'data_field.missing_descriptor', 'error', '$source_path', f.field_key, 'data_key_descriptor row', f.table_id || ':' || f.record_id,
           'field has no active descriptor'
    FROM data_field f
    LEFT JOIN data_key_descriptor d ON d.stable_key = f.field_key
    WHERE d.stable_key IS NULL
    UNION ALL
    SELECT 'data_field.type_mismatch', 'error', '$source_path', f.field_key, d.value_type, f.value_type,
           'field value_type differs from descriptor'
    FROM data_field f
    JOIN data_key_descriptor d ON d.stable_key = f.field_key
    WHERE f.value_type <> d.value_type
    UNION ALL
    SELECT 'descriptor.owner_skill_missing', 'error', '$source_path', stable_key, 'owner_skill', owner_skill,
           'descriptor owner_skill is required'
    FROM data_key_descriptor
    WHERE trim(owner_skill) = ''
    UNION ALL
    SELECT 'descriptor.default_bool', 'error', '$source_path', stable_key, 'true|false', default_value_text,
           'bool descriptor default is invalid'
    FROM data_key_descriptor
    WHERE value_type = 'bool'
      AND lower(default_value_text) NOT IN ('true', 'false')
    UNION ALL
    SELECT 'descriptor.range', 'error', '$source_path', stable_key, 'min <= max', COALESCE(min_value, '') || '>' || COALESCE(max_value, ''),
           'descriptor min_value is greater than max_value'
    FROM data_key_descriptor
    WHERE min_value IS NOT NULL
      AND max_value IS NOT NULL
      AND min_value > max_value
    UNION ALL
    SELECT 'descriptor.options_json', 'error', '$source_path', stable_key, 'valid json array', options_json,
           'descriptor options_json is invalid'
    FROM data_key_descriptor
    WHERE json_valid(options_json) = 0
       OR json_type(options_json) <> 'array'
    UNION ALL
    SELECT 'modifier.target_type', 'error', '$source_path', stable_key, 'numeric descriptor type', value_type,
           'supports_modifiers requires numeric descriptor type'
    FROM data_key_descriptor
    WHERE supports_modifiers = 1
      AND value_type NOT IN ('int', 'float', 'double')
    UNION ALL
    SELECT 'capability.disabled_field', 'error', '$source_path', f.field_key, 'enabled capability', d.owner_capability,
           'field belongs to disabled capability'
    FROM data_field f
    JOIN data_key_descriptor d ON d.stable_key = f.field_key
    JOIN capability_manifest c ON c.capability_id = d.owner_capability
    WHERE c.enabled = 0
    UNION ALL
    SELECT 'capability.disabled_descriptor', 'error', '$source_path', d.stable_key, 'enabled capability', d.owner_capability,
           'active descriptor belongs to disabled capability'
    FROM data_key_descriptor d
    JOIN capability_manifest c ON c.capability_id = d.owner_capability
    WHERE c.enabled = 0
      AND EXISTS (SELECT 1 FROM data_field f WHERE f.field_key = d.stable_key)
    UNION ALL
    SELECT 'capability.disabled_dependency', 'error', '$source_path', '', 'enabled dependency', c.capability_id || '->' || dep.capability_id,
           'enabled capability depends on disabled capability'
    FROM capability_manifest c
    JOIN capability_manifest dep
      ON instr(',' || replace(c.dependencies, ' ', '') || ',', ',' || dep.capability_id || ',') > 0
    WHERE c.enabled = 1
      AND dep.enabled = 0
    UNION ALL
    SELECT 'resource.category', 'error', '$source_path', resource_key, 'known ResourceCategory', category,
           'resource category is invalid'
    FROM resource_entry
    WHERE category NOT IN ('Entity', 'Component', 'System', 'Tools', 'UI', 'Asset', 'Data', 'Config', 'Test', 'Other')
    UNION ALL
    SELECT 'resource.path', 'error', '$source_path', resource_key, 'res:// path', resource_path,
           'resource path must use res://'
    FROM resource_entry
    WHERE resource_path NOT LIKE 'res://%'
    UNION ALL
    SELECT 'resource.disabled_owner', 'error', '$source_path', resource_key, 'enabled capability or shared', owner_capability,
           'resource belongs to disabled capability'
    FROM resource_entry r
    JOIN capability_manifest c ON c.capability_id = r.owner_capability
    WHERE r.owner_capability <> 'shared'
      AND c.enabled = 0
    UNION ALL
    SELECT 'resource.legacy', 'warning', '$source_path', resource_key, 'active resource', legacy_status,
           'resource is marked legacy'
    FROM resource_entry
    WHERE legacy_status = 'legacy'
    UNION ALL
    SELECT 'resource.intentionally_dropped', 'warning', '$source_path', resource_key, 'deleted from seed', legacy_status,
           'intentionally-dropped resource should be deleted from seed SQL, not kept as a zombie row'
    FROM resource_entry
    WHERE legacy_status = 'intentionally-dropped'
    UNION ALL
    SELECT 'resource.missing', 'warning', '$source_path', resource_key, 'deleted or fixed in seed', legacy_status,
           'missing resource should be deleted or fixed in seed SQL, not kept as a zombie row'
    FROM resource_entry
    WHERE legacy_status = 'missing'
)
SELECT json_object(
    'checks', COALESCE(json_group_array(json_object(
        'checkId', check_id,
        'severity', severity,
        'sourcePath', source_path,
        'stableKey', stable_key,
        'expected', expected,
        'actual', actual,
        'summary', summary
    )), json('[]')),
    'summary', json_object(
        'errorCount', COALESCE(SUM(CASE WHEN severity = 'error' THEN 1 ELSE 0 END), 0),
        'warningCount', COALESCE(SUM(CASE WHEN severity = 'warning' THEN 1 ELSE 0 END), 0)
    )
) FROM checks;
"

report="$(sqlite3 "$db_path" "$issue_sql")"
if [ -n "$report_path" ]; then
    mkdir -p "$(dirname "$report_path")"
    printf '%s\n' "$report" > "$report_path"
fi

error_count="$(printf '%s' "$report" | jq -r '.summary.errorCount')"

if [ "$error_count" != "0" ]; then
    echo "$report" >&2
    exit 1
fi

echo "DataOS validation PASS: $db_path"
