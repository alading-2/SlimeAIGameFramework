-- Framework DataOS validation seed.
-- 只覆盖框架级 descriptor/manifest 最小闭环；游戏专属 seed 留在 Games/<Game>。

PRAGMA foreign_keys = ON;

INSERT OR IGNORE INTO capability_manifest(capability_id, owner_skill, enabled, version, dependencies, profile, trim_policy, description)
VALUES
    ('Damage', 'damage-system', 1, '1', '', 'framework', 'fail', 'Damage runtime data keys.'),
    ('Movement', 'movement-system', 1, '1', '', 'framework', 'fail', 'Movement runtime data keys.'),
    ('Attack', 'attack-system', 1, '1', 'Damage,Movement', 'framework', 'fail', 'Attack runtime data keys.'),
    ('Ability', 'ability-system', 1, '1', 'Damage', 'framework', 'fail', 'Ability runtime data keys.'),
    ('DisabledProbe', 'test-system', 0, '1', '', 'framework', 'trim', 'Disabled fixture capability.');

INSERT OR IGNORE INTO data_table(table_id, domain, description)
VALUES ('framework.probe', 'framework', 'Framework DataOS descriptor validation probe.');

INSERT OR IGNORE INTO data_record(table_id, record_id, display_name, description)
VALUES ('framework.probe', 'default', 'Framework Probe', 'Framework DataOS typed snapshot probe.');

INSERT OR IGNORE INTO data_key_descriptor(
    stable_key,
    owner_capability,
    owner_skill,
    value_type,
    default_value_text,
    display_name,
    description,
    category,
    min_value,
    max_value,
    is_percentage,
    supports_modifiers)
VALUES
    ('Damage.MaxHp', 'Damage', 'damage-system', 'float', '0', 'Max HP', 'Maximum health.', 'Damage.Config', 0, NULL, 0, 1),
    ('Damage.CurrentHp', 'Damage', 'damage-system', 'float', '0', 'Current HP', 'Current health.', 'Damage.Runtime', 0, NULL, 0, 0),
    ('Movement.MoveSpeed', 'Movement', 'movement-system', 'float', '0', 'Move Speed', 'Movement speed.', 'Movement.Config', 0, NULL, 0, 1),
    ('Attack.Damage', 'Attack', 'attack-system', 'float', '0', 'Attack Damage', 'Attack damage.', 'Attack.Basic', 0, NULL, 0, 1),
    ('Ability.TriggerMode', 'Ability', 'ability-system', 'string', 'None', 'Trigger Mode', 'Ability trigger mode enum name.', 'Ability.Basic', NULL, NULL, 0, 0),
    ('DisabledProbe.Value', 'DisabledProbe', 'test-system', 'float', '0', 'Disabled Value', 'Must not enter active snapshot.', 'Test', NULL, NULL, 0, 0);

INSERT OR IGNORE INTO data_field(table_id, record_id, field_key, value_type, value_text)
VALUES
    ('framework.probe', 'default', 'Damage.MaxHp', 'float', '100'),
    ('framework.probe', 'default', 'Damage.CurrentHp', 'float', '100'),
    ('framework.probe', 'default', 'Movement.MoveSpeed', 'float', '120'),
    ('framework.probe', 'default', 'Attack.Damage', 'float', '8'),
    ('framework.probe', 'default', 'Ability.TriggerMode', 'string', 'Manual');

INSERT OR IGNORE INTO resource_entry(category, resource_key, resource_path, owner_capability, legacy_status, description)
VALUES ('Test', 'FrameworkProbe', 'res://DataOS/FrameworkProbe.tres', 'shared', 'active', 'Framework validation probe resource.');
