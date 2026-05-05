#!/usr/bin/env bash
set -euo pipefail

if [ "$#" -ne 1 ]; then
    echo "usage: $0 <authoring.db>" >&2
    exit 2
fi

db_path="$1"

if [ ! -f "$db_path" ]; then
    echo "DataOS db not found: $db_path" >&2
    exit 1
fi

check_sql="
PRAGMA foreign_keys = ON;

WITH issues AS (
    SELECT 'foreign_key:' || \"table\" || ':' || rowid || ':' || parent AS issue
    FROM pragma_foreign_key_check
    UNION ALL
    SELECT 'empty table_id'
    FROM data_table
    WHERE trim(table_id) = ''
    UNION ALL
    SELECT 'empty record_id:' || table_id
    FROM data_record
    WHERE trim(record_id) = ''
    UNION ALL
    SELECT 'empty field_key:' || table_id || ':' || record_id
    FROM data_field
    WHERE trim(field_key) = ''
    UNION ALL
    SELECT 'invalid bool:' || table_id || ':' || record_id || ':' || field_key
    FROM data_field
    WHERE value_type = 'bool'
      AND lower(value_text) NOT IN ('true', 'false')
    UNION ALL
    SELECT 'invalid resource category:' || category || ':' || resource_key
    FROM resource_entry
    WHERE category NOT IN ('Entity', 'Component', 'System', 'Tools', 'UI', 'Asset', 'Data', 'Config', 'Test', 'Other')
    UNION ALL
    SELECT 'invalid resource path:' || category || ':' || resource_key
    FROM resource_entry
    WHERE resource_path NOT LIKE 'res://%'
)
SELECT issue FROM issues;
"

issues="$(sqlite3 "$db_path" "$check_sql")"
if [ -n "$issues" ]; then
    echo "$issues" >&2
    exit 1
fi

echo "DataOS validation PASS: $db_path"
