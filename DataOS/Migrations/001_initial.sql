-- DataOS migration 001: core authoring schema.

PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS dataos_schema_version (
    version INTEGER PRIMARY KEY,
    applied_at_utc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%SZ', 'now')),
    description TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS data_table (
    table_id TEXT PRIMARY KEY,
    domain TEXT NOT NULL,
    description TEXT NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS data_record (
    table_id TEXT NOT NULL,
    record_id TEXT NOT NULL,
    display_name TEXT NOT NULL DEFAULT '',
    description TEXT NOT NULL DEFAULT '',
    PRIMARY KEY (table_id, record_id),
    FOREIGN KEY (table_id) REFERENCES data_table(table_id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS data_field (
    table_id TEXT NOT NULL,
    record_id TEXT NOT NULL,
    field_key TEXT NOT NULL,
    value_type TEXT NOT NULL CHECK (value_type IN ('string', 'int', 'float', 'double', 'bool')),
    value_text TEXT NOT NULL,
    PRIMARY KEY (table_id, record_id, field_key),
    FOREIGN KEY (table_id, record_id) REFERENCES data_record(table_id, record_id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS resource_entry (
    category TEXT NOT NULL,
    resource_key TEXT NOT NULL,
    resource_path TEXT NOT NULL,
    description TEXT NOT NULL DEFAULT '',
    PRIMARY KEY (category, resource_key)
);

INSERT OR IGNORE INTO dataos_schema_version(version, description)
VALUES (1, 'DataOS core authoring schema');
