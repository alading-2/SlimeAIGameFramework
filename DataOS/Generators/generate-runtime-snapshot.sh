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

if [ ! -f "$db_path" ]; then
    echo "DataOS db not found: $db_path" >&2
    exit 1
fi

"$repo_root/DataOS/Validation/validate-dataos.sh" "$db_path"
mkdir -p "$(dirname "$output_path")"

sqlite3 "$db_path" > "$output_path" <<SQL
PRAGMA foreign_keys = ON;

WITH
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
                FROM data_field f
                WHERE f.table_id = r.table_id
                  AND f.record_id = r.record_id
                ORDER BY f.field_key
            ), json('{}'))
        ) AS doc
    FROM data_record r
    ORDER BY r.table_id, r.record_id
),
resource_docs AS (
    SELECT json_object(
        'category', category,
        'key', resource_key,
        'path', resource_path
    ) AS doc
    FROM resource_entry
    ORDER BY category, resource_key
)
SELECT json_object(
    'schemaVersion', 1,
    'generatedAtUtc', '$generated_at_utc',
    'records', COALESCE((SELECT json_group_array(json(doc)) FROM record_docs), json('[]')),
    'resources', COALESCE((SELECT json_group_array(json(doc)) FROM resource_docs), json('[]'))
);
SQL
