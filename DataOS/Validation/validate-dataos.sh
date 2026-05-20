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

WITH
business_bool_values AS (
    SELECT 'unit_player' AS source_table, u.id AS row_id, j.key AS source_column, CAST(j.value AS TEXT) AS actual
    FROM unit_player u
    JOIN json_each(json_object('is_show_health_bar', is_show_health_bar, 'ai_is_enabled', ai_is_enabled)) j
    WHERE j.type <> 'null'
    UNION ALL
    SELECT 'unit_enemy', u.id, j.key, CAST(j.value AS TEXT)
    FROM unit_enemy u
    JOIN json_each(json_object('is_show_health_bar', is_show_health_bar, 'ai_is_enabled', ai_is_enabled, 'spawn_is_enabled', spawn_is_enabled)) j
    WHERE j.type <> 'null'
    UNION ALL
    SELECT 'unit_targeting_indicator', u.id, j.key, CAST(j.value AS TEXT)
    FROM unit_targeting_indicator u
    JOIN json_each(json_object('is_show_health_bar', is_show_health_bar, 'is_invulnerable', is_invulnerable)) j
    WHERE j.type <> 'null'
    UNION ALL
    SELECT 'ability', a.id, j.key, CAST(j.value AS TEXT)
    FROM ability a
    JOIN json_each(json_object(
        'auto_target_ignore_same_team', auto_target_ignore_same_team,
        'auto_target_requires_damageable', auto_target_requires_damageable,
        'apply_immediate_damage', apply_immediate_damage,
        'uses_charges', uses_charges
    )) j
    WHERE j.type <> 'null'
    UNION ALL
    SELECT 'feature_definition', f.id, 'is_enabled', CAST(is_enabled AS TEXT)
    FROM feature_definition f
    WHERE is_enabled IS NOT NULL
    UNION ALL
    SELECT 'system_config', s.id, j.key, CAST(j.value AS TEXT)
    FROM system_config s
    JOIN json_each(json_object('required', required, 'auto_load', auto_load, 'start_enabled', start_enabled)) j
    WHERE j.type <> 'null'
    UNION ALL
    SELECT 'system_preset', p.id, 'is_active', CAST(is_active AS TEXT)
    FROM system_preset p
    WHERE is_active IS NOT NULL
),
movement_handler_rows AS (
    SELECT ability_id, 'ability_movement_sine_wave' AS handler_table FROM ability_movement_sine_wave
    UNION ALL SELECT ability_id, 'ability_movement_orbit' FROM ability_movement_orbit
    UNION ALL SELECT ability_id, 'ability_movement_boomerang' FROM ability_movement_boomerang
    UNION ALL SELECT ability_id, 'ability_movement_bezier' FROM ability_movement_bezier
    UNION ALL SELECT ability_id, 'ability_movement_circular_arc' FROM ability_movement_circular_arc
    UNION ALL SELECT ability_id, 'ability_movement_attach_to_host' FROM ability_movement_attach_to_host
    UNION ALL SELECT ability_id, 'ability_movement_charge' FROM ability_movement_charge
),
checks AS (
    SELECT 'foreign_key' AS check_id, 'error' AS severity, '$source_path' AS source_path, '' AS stable_key,
           parent AS expected, \"table\" || ':' || rowid AS actual,
           'foreign key violation' AS summary,
           \"table\" AS source_table, CAST(rowid AS TEXT) AS row_id, '' AS source_column, '' AS expected_type, '' AS actual_type
    FROM pragma_foreign_key_check
    UNION ALL
    SELECT 'data_table.empty_id', 'error', '$source_path', '', 'non-empty table_id', table_id,
           'data_table.table_id is empty', 'data_table', table_id, 'table_id', '', ''
    FROM data_table
    WHERE trim(table_id) = ''
    UNION ALL
    SELECT 'data_record.empty_id', 'error', '$source_path', '', 'non-empty record_id', table_id || ':' || record_id,
           'data_record.record_id is empty', 'data_record', table_id || ':' || record_id, 'record_id', '', ''
    FROM data_record
    WHERE trim(record_id) = ''
    UNION ALL
    SELECT 'data_field.empty_key', 'error', '$source_path', field_key, 'non-empty field_key', table_id || ':' || record_id,
           'data_field.field_key is empty', 'data_field', table_id || ':' || record_id, 'field_key', '', ''
    FROM data_field
    WHERE trim(field_key) = ''
    UNION ALL
    SELECT 'data_field.bool_value', 'error', '$source_path', field_key, 'true|false', value_text,
           'bool field has invalid value', 'data_field', table_id || ':' || record_id, 'value_text', 'bool', value_type
    FROM data_field
    WHERE value_type = 'bool'
      AND lower(value_text) NOT IN ('true', 'false')
    UNION ALL
    SELECT 'business.required_path', 'error', '$source_path', '', 'non-empty visual_scene_path', COALESCE(visual_scene_path, ''),
           'unit_player visual_scene_path is required', 'unit_player', id, 'visual_scene_path', '', ''
    FROM unit_player
    WHERE trim(COALESCE(visual_scene_path, '')) = ''
    UNION ALL
    SELECT 'business.required_path', 'error', '$source_path', '', 'non-empty visual_scene_path', COALESCE(visual_scene_path, ''),
           'unit_enemy visual_scene_path is required', 'unit_enemy', id, 'visual_scene_path', '', ''
    FROM unit_enemy
    WHERE trim(COALESCE(visual_scene_path, '')) = ''
    UNION ALL
    SELECT 'business.required_path', 'error', '$source_path', '', 'non-empty scene_path', COALESCE(scene_path, ''),
           'ability_effect scene_path is required', 'ability_effect', ability_id, 'scene_path', '', ''
    FROM ability_effect
    WHERE trim(COALESCE(scene_path, '')) = ''
    UNION ALL
    SELECT 'business.required_path', 'error', '$source_path', '', 'non-empty scene_path', COALESCE(scene_path, ''),
           'ability_projectile scene_path is required', 'ability_projectile', ability_id, 'scene_path', '', ''
    FROM ability_projectile
    WHERE trim(COALESCE(scene_path, '')) = ''
    UNION ALL
    SELECT 'business.required_ability', 'error', '$source_path', '', 'name/type/trigger_mode/feature_handler_id', a.id,
           'ability common required fields are missing', 'ability', a.id, 'name|type|trigger_mode|feature_handler_id', '', ''
    FROM ability a
    WHERE trim(COALESCE(name, '')) = ''
       OR trim(COALESCE(type, '')) = ''
       OR trim(COALESCE(trigger_mode, '')) = ''
       OR trim(COALESCE(feature_handler_id, '')) = ''
    UNION ALL
    SELECT 'business.bool_integer', 'error', '$source_path', '', '0|1', actual,
           'business table bool column must use canonical integer', source_table, row_id, source_column, 'bool', typeof(actual)
    FROM business_bool_values
    WHERE actual NOT IN ('0', '1')
    UNION ALL
    SELECT 'business.path', 'error', '$source_path', '', 'res:// path', resource_path,
           'business content path must use res://', source_table, source_row_id, source_column, '', ''
    FROM dataos_content_path_ownership
    WHERE resource_path NOT LIKE 'res://%'
    UNION ALL
    SELECT 'business.duplicate_name', 'error', '$source_path', '', 'unique name', name,
           'unit_player name must be unique', 'unit_player', group_concat(id), 'name', '', ''
    FROM unit_player
    WHERE trim(name) <> ''
    GROUP BY name
    HAVING COUNT(*) > 1
    UNION ALL
    SELECT 'business.duplicate_name', 'error', '$source_path', '', 'unique name', name,
           'unit_enemy name must be unique', 'unit_enemy', group_concat(id), 'name', '', ''
    FROM unit_enemy
    WHERE trim(name) <> ''
    GROUP BY name
    HAVING COUNT(*) > 1
    UNION ALL
    SELECT 'business.duplicate_name', 'error', '$source_path', '', 'unique name', name,
           'ability name must be unique', 'ability', group_concat(id), 'name', '', ''
    FROM ability
    WHERE trim(name) <> ''
    GROUP BY name
    HAVING COUNT(*) > 1
    UNION ALL
    SELECT 'movement.handler_combination', 'error', '$source_path', '', 'at most one handler table per ability', group_concat(handler_table),
           'ability has multiple movement handler tables', 'ability_movement_*', ability_id, 'ability_id', '', ''
    FROM movement_handler_rows
    GROUP BY ability_id
    HAVING COUNT(*) > 1
    UNION ALL
    SELECT 'projection.duplicate_field', 'error', '$source_path', field_key, 'one projected source', group_concat(source_table || '.' || source_column),
           'multiple authoring sources project to the same runtime field', 'dataos_runtime_field_stream', table_id || ':' || record_id, field_key, '', ''
    FROM dataos_runtime_field_stream
    GROUP BY table_id, record_id, field_key
    HAVING COUNT(*) > 1
    UNION ALL
    SELECT 'projection.missing_descriptor', 'error', '$source_path', f.field_key, 'data_key_descriptor row', f.table_id || ':' || f.record_id,
           'projected field has no descriptor', f.source_table, f.source_row_id, f.source_column, '', f.value_type
    FROM dataos_runtime_field_stream f
    LEFT JOIN data_key_descriptor d ON d.stable_key = f.field_key
    WHERE d.stable_key IS NULL
    UNION ALL
    SELECT 'projection.type_mismatch', 'error', '$source_path', f.field_key, d.value_type, f.value_type,
           'projected field value_type differs from descriptor', f.source_table, f.source_row_id, f.source_column, d.value_type, f.value_type
    FROM dataos_runtime_field_stream f
    JOIN data_key_descriptor d ON d.stable_key = f.field_key
    WHERE f.value_type <> d.value_type
    UNION ALL
    SELECT 'descriptor.owner_skill_missing', 'error', '$source_path', stable_key, 'owner_skill', owner_skill,
           'descriptor owner_skill is required', 'data_key_descriptor', stable_key, 'owner_skill', '', ''
    FROM data_key_descriptor
    WHERE trim(owner_skill) = ''
    UNION ALL
    SELECT 'descriptor.default_bool', 'error', '$source_path', stable_key, 'true|false', default_value_text,
           'bool descriptor default is invalid', 'data_key_descriptor', stable_key, 'default_value_text', 'bool', value_type
    FROM data_key_descriptor
    WHERE value_type = 'bool'
      AND lower(default_value_text) NOT IN ('true', 'false')
    UNION ALL
    SELECT 'descriptor.range', 'error', '$source_path', stable_key, 'min <= max', COALESCE(min_value, '') || '>' || COALESCE(max_value, ''),
           'descriptor min_value is greater than max_value', 'data_key_descriptor', stable_key, 'min_value|max_value', '', ''
    FROM data_key_descriptor
    WHERE min_value IS NOT NULL
      AND max_value IS NOT NULL
      AND min_value > max_value
    UNION ALL
    SELECT 'descriptor.options_json', 'error', '$source_path', stable_key, 'valid json array', options_json,
           'descriptor options_json is invalid', 'data_key_descriptor', stable_key, 'options_json', '', ''
    FROM data_key_descriptor
    WHERE json_valid(options_json) = 0
       OR json_type(options_json) <> 'array'
    UNION ALL
    SELECT 'modifier.target_type', 'error', '$source_path', stable_key, 'numeric descriptor type', value_type,
           'supports_modifiers requires numeric descriptor type', 'data_key_descriptor', stable_key, 'supports_modifiers', 'numeric', value_type
    FROM data_key_descriptor
    WHERE supports_modifiers = 1
      AND value_type NOT IN ('int', 'float', 'double')
    UNION ALL
    SELECT 'capability.disabled_field', 'error', '$source_path', f.field_key, 'enabled capability', d.owner_capability,
           'projected field belongs to disabled capability', f.source_table, f.source_row_id, f.source_column, '', f.value_type
    FROM dataos_runtime_field_stream f
    JOIN data_key_descriptor d ON d.stable_key = f.field_key
    JOIN capability_manifest c ON c.capability_id = d.owner_capability
    WHERE c.enabled = 0
    UNION ALL
    SELECT 'capability.disabled_descriptor', 'error', '$source_path', d.stable_key, 'enabled capability', d.owner_capability,
           'active descriptor belongs to disabled capability', 'data_key_descriptor', d.stable_key, 'owner_capability', '', d.value_type
    FROM data_key_descriptor d
    JOIN capability_manifest c ON c.capability_id = d.owner_capability
    WHERE c.enabled = 0
      AND EXISTS (SELECT 1 FROM dataos_runtime_field_stream f WHERE f.field_key = d.stable_key)
    UNION ALL
    SELECT 'capability.disabled_dependency', 'error', '$source_path', '', 'enabled dependency', c.capability_id || '->' || dep.capability_id,
           'enabled capability depends on disabled capability', 'capability_manifest', c.capability_id, 'dependencies', '', ''
    FROM capability_manifest c
    JOIN capability_manifest dep
      ON instr(',' || replace(c.dependencies, ' ', '') || ',', ',' || dep.capability_id || ',') > 0
    WHERE c.enabled = 1
      AND dep.enabled = 0
    UNION ALL
    SELECT 'resource.category', 'error', '$source_path', resource_key, 'known ResourceCategory', category,
           'resource category is invalid', 'resource_entry', resource_key, 'category', '', ''
    FROM resource_entry
    WHERE category NOT IN ('Entity', 'Component', 'System', 'Tools', 'UI', 'Asset', 'Data', 'Config', 'Test', 'Other')
    UNION ALL
    SELECT 'resource.path', 'error', '$source_path', resource_key, 'res:// path', resource_path,
           'resource path must use res://', 'resource_entry', resource_key, 'resource_path', '', ''
    FROM resource_entry
    WHERE resource_path NOT LIKE 'res://%'
    UNION ALL
    SELECT 'resource.duplicate_content_owner', 'error', '$source_path', r.resource_key, 'single content path owner or justified lookup/legacy classification', r.resource_path,
           'resource_entry duplicates a content-owned business path without lookup/legacy justification',
           p.source_table, p.source_row_id, p.source_column, '', ''
    FROM dataos_content_path_ownership p
    JOIN resource_entry r ON r.resource_path = p.resource_path
    WHERE r.legacy_status NOT IN ('intentionally-dropped', 'missing')
      AND r.legacy_status NOT LIKE 'legacy%'
      AND lower(r.description) NOT LIKE '%legacy%'
      AND lower(r.description) NOT LIKE '%lookup%'
    UNION ALL
    SELECT 'resource.disabled_owner', 'error', '$source_path', resource_key, 'enabled capability or shared', owner_capability,
           'resource belongs to disabled capability', 'resource_entry', resource_key, 'owner_capability', '', ''
    FROM resource_entry r
    JOIN capability_manifest c ON c.capability_id = r.owner_capability
    WHERE r.owner_capability <> 'shared'
      AND c.enabled = 0
    UNION ALL
    SELECT 'resource.legacy', 'warning', '$source_path', resource_key, 'active resource', legacy_status,
           'resource is marked legacy', 'resource_entry', resource_key, 'legacy_status', '', ''
    FROM resource_entry
    WHERE legacy_status = 'legacy'
    UNION ALL
    SELECT 'resource.intentionally_dropped', 'warning', '$source_path', resource_key, 'deleted from seed', legacy_status,
           'intentionally-dropped resource should be deleted from seed SQL, not kept as a zombie row', 'resource_entry', resource_key, 'legacy_status', '', ''
    FROM resource_entry
    WHERE legacy_status = 'intentionally-dropped'
    UNION ALL
    SELECT 'resource.missing', 'warning', '$source_path', resource_key, 'deleted or fixed in seed', legacy_status,
           'missing resource should be deleted or fixed in seed SQL, not kept as a zombie row', 'resource_entry', resource_key, 'legacy_status', '', ''
    FROM resource_entry
    WHERE legacy_status = 'missing'
)
SELECT json_object(
    'checks', COALESCE(json_group_array(json_object(
        'checkId', check_id,
        'severity', severity,
        'sourcePath', source_path,
        'sourceTable', source_table,
        'rowId', row_id,
        'sourceColumn', source_column,
        'stableKey', stable_key,
        'expected', expected,
        'actual', actual,
        'expectedType', expected_type,
        'actualType', actual_type,
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
