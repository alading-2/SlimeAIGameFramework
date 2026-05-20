-- DataOS core schema reference.
-- Runtime hot path consumes generated snapshots, not arbitrary SQL queries.

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

CREATE TABLE IF NOT EXISTS capability_manifest (
    capability_id TEXT PRIMARY KEY,
    owner_skill TEXT NOT NULL DEFAULT '',
    enabled INTEGER NOT NULL DEFAULT 1 CHECK (enabled IN (0, 1)),
    version TEXT NOT NULL DEFAULT '1',
    dependencies TEXT NOT NULL DEFAULT '',
    profile TEXT NOT NULL DEFAULT 'framework',
    trim_policy TEXT NOT NULL DEFAULT 'fail' CHECK (trim_policy IN ('fail', 'trim')),
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

CREATE TABLE IF NOT EXISTS data_key_descriptor (
    stable_key TEXT PRIMARY KEY,
    owner_capability TEXT NOT NULL,
    owner_skill TEXT NOT NULL DEFAULT '',
    value_type TEXT NOT NULL CHECK (value_type IN ('string', 'int', 'float', 'double', 'bool')),
    default_value_text TEXT NOT NULL DEFAULT '',
    display_name TEXT NOT NULL DEFAULT '',
    description TEXT NOT NULL DEFAULT '',
    icon_path TEXT NOT NULL DEFAULT '',
    category TEXT NOT NULL DEFAULT '',
    min_value REAL,
    max_value REAL,
    options_json TEXT NOT NULL DEFAULT '[]',
    is_percentage INTEGER NOT NULL DEFAULT 0 CHECK (is_percentage IN (0, 1)),
    supports_modifiers INTEGER NOT NULL DEFAULT 0 CHECK (supports_modifiers IN (0, 1)),
    is_computed INTEGER NOT NULL DEFAULT 0 CHECK (is_computed IN (0, 1)),
    FOREIGN KEY (owner_capability) REFERENCES capability_manifest(capability_id) ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS resource_entry (
    category TEXT NOT NULL,
    resource_key TEXT NOT NULL,
    resource_path TEXT NOT NULL,
    owner_capability TEXT NOT NULL DEFAULT 'shared',
    legacy_status TEXT NOT NULL DEFAULT 'active' CHECK (legacy_status IN ('active', 'legacy', 'legacy-input', 'missing', 'intentionally-dropped')),
    description TEXT NOT NULL DEFAULT '',
    PRIMARY KEY (category, resource_key)
);
