-- DataOS migration 002: table-first authoring schema.
-- 业务表是主要编辑面；data_record/data_field 仅保留兼容和投影输出语义。

PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS unit_player (
    id TEXT PRIMARY KEY CHECK (trim(id) <> ''),
    name TEXT NOT NULL CHECK (trim(name) <> ''),
    entity_type TEXT,
    death_type TEXT,
    visual_scene_path TEXT,
    health_bar_height REAL,
    is_show_health_bar INTEGER CHECK (is_show_health_bar IN (0, 1) OR is_show_health_bar IS NULL),
    pickup_range REAL,
    exp_reward INTEGER,
    detection_range REAL,
    collision_team INTEGER,
    collision_layer INTEGER,
    collision_mask INTEGER,
    collision_radius REAL,
    max_hp REAL,
    current_hp REAL,
    armor REAL,
    crit_rate REAL,
    life_steal REAL,
    contact_damage REAL,
    contact_damage_interval REAL,
    move_speed REAL,
    acceleration REAL,
    attack_damage REAL,
    attack_range REAL,
    attack_interval REAL,
    attack_wind_up_time REAL,
    attack_recovery_time REAL,
    ai_is_enabled INTEGER CHECK (ai_is_enabled IN (0, 1) OR ai_is_enabled IS NULL),
    ai_attack_range REAL,
    description TEXT NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS unit_enemy (
    id TEXT PRIMARY KEY CHECK (trim(id) <> ''),
    name TEXT NOT NULL CHECK (trim(name) <> ''),
    entity_type TEXT,
    death_type TEXT,
    visual_scene_path TEXT,
    health_bar_height REAL,
    is_show_health_bar INTEGER CHECK (is_show_health_bar IN (0, 1) OR is_show_health_bar IS NULL),
    pickup_range REAL,
    exp_reward INTEGER,
    detection_range REAL,
    collision_team INTEGER,
    collision_layer INTEGER,
    collision_mask INTEGER,
    collision_radius REAL,
    max_hp REAL,
    current_hp REAL,
    armor REAL,
    crit_rate REAL,
    life_steal REAL,
    contact_damage REAL,
    contact_damage_interval REAL,
    move_speed REAL,
    acceleration REAL,
    attack_damage REAL,
    attack_range REAL,
    attack_interval REAL,
    attack_wind_up_time REAL,
    attack_recovery_time REAL,
    ai_is_enabled INTEGER CHECK (ai_is_enabled IN (0, 1) OR ai_is_enabled IS NULL),
    ai_attack_range REAL,
    spawn_is_enabled INTEGER CHECK (spawn_is_enabled IN (0, 1) OR spawn_is_enabled IS NULL),
    spawn_position_strategy TEXT,
    spawn_min_wave INTEGER,
    spawn_max_wave INTEGER,
    spawn_interval REAL,
    spawn_max_count_per_wave INTEGER,
    spawn_single_count INTEGER,
    spawn_single_variance INTEGER,
    spawn_start_delay REAL,
    spawn_weight INTEGER,
    description TEXT NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS unit_targeting_indicator (
    id TEXT PRIMARY KEY CHECK (trim(id) <> ''),
    name TEXT NOT NULL CHECK (trim(name) <> ''),
    entity_type TEXT,
    visual_scene_path TEXT,
    is_show_health_bar INTEGER CHECK (is_show_health_bar IN (0, 1) OR is_show_health_bar IS NULL),
    collision_team INTEGER,
    collision_layer INTEGER,
    collision_mask INTEGER,
    collision_radius REAL,
    max_hp REAL,
    current_hp REAL,
    is_invulnerable INTEGER CHECK (is_invulnerable IN (0, 1) OR is_invulnerable IS NULL),
    move_speed REAL,
    attack_interval REAL,
    description TEXT NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS ability (
    id TEXT PRIMARY KEY CHECK (trim(id) <> ''),
    name TEXT NOT NULL CHECK (trim(name) <> ''),
    type TEXT NOT NULL CHECK (trim(type) <> ''),
    trigger_mode TEXT NOT NULL CHECK (trim(trigger_mode) <> ''),
    target_selection TEXT,
    feature_group_id TEXT,
    feature_handler_id TEXT NOT NULL CHECK (trim(feature_handler_id) <> ''),
    description TEXT,
    icon_path TEXT,
    cost_type TEXT,
    cost_amount REAL,
    cooldown REAL,
    cast_range REAL,
    auto_target_range REAL,
    auto_target_max_targets INTEGER,
    auto_target_ignore_same_team INTEGER CHECK (auto_target_ignore_same_team IN (0, 1) OR auto_target_ignore_same_team IS NULL),
    auto_target_requires_damageable INTEGER CHECK (auto_target_requires_damageable IN (0, 1) OR auto_target_requires_damageable IS NULL),
    damage REAL,
    damage_interval REAL,
    damage_repeat_count INTEGER,
    apply_immediate_damage INTEGER CHECK (apply_immediate_damage IN (0, 1) OR apply_immediate_damage IS NULL),
    effect_radius REAL,
    chain_count INTEGER,
    chain_range REAL,
    chain_delay REAL,
    chain_damage_decay REAL,
    level INTEGER,
    max_level INTEGER,
    uses_charges INTEGER CHECK (uses_charges IN (0, 1) OR uses_charges IS NULL),
    max_charges INTEGER,
    current_charges INTEGER,
    charge_time REAL
);

CREATE TABLE IF NOT EXISTS ability_effect (
    ability_id TEXT PRIMARY KEY,
    scene_path TEXT NOT NULL CHECK (trim(scene_path) <> ''),
    name TEXT,
    animation_name TEXT,
    duration REAL,
    FOREIGN KEY (ability_id) REFERENCES ability(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS ability_projectile (
    ability_id TEXT PRIMARY KEY,
    scene_path TEXT NOT NULL CHECK (trim(scene_path) <> ''),
    speed REAL,
    max_hit_count INTEGER,
    max_life_time REAL,
    damage REAL,
    FOREIGN KEY (ability_id) REFERENCES ability(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS ability_line_effect (
    ability_id TEXT PRIMARY KEY,
    scene_path TEXT NOT NULL CHECK (trim(scene_path) <> ''),
    FOREIGN KEY (ability_id) REFERENCES ability(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS ability_movement_sine_wave (
    ability_id TEXT PRIMARY KEY,
    wave_amplitude REAL,
    wave_frequency REAL,
    wave_phase REAL,
    max_distance REAL,
    FOREIGN KEY (ability_id) REFERENCES ability(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS ability_movement_orbit (
    ability_id TEXT PRIMARY KEY,
    projectile_count INTEGER,
    orbit_radius REAL,
    orbit_angular_speed REAL,
    orbit_angular_acceleration REAL,
    orbit_total_angle REAL,
    is_orbit_clockwise INTEGER CHECK (is_orbit_clockwise IN (0, 1) OR is_orbit_clockwise IS NULL),
    max_travel_duration REAL,
    FOREIGN KEY (ability_id) REFERENCES ability(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS ability_movement_boomerang (
    ability_id TEXT PRIMARY KEY,
    boomerang_arc_height REAL,
    boomerang_pause_time REAL,
    boomerang_is_clockwise INTEGER CHECK (boomerang_is_clockwise IN (0, 1) OR boomerang_is_clockwise IS NULL),
    boomerang_return_speed_multiplier REAL,
    FOREIGN KEY (ability_id) REFERENCES ability(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS ability_movement_bezier (
    ability_id TEXT PRIMARY KEY,
    projectile_count INTEGER,
    bezier_degree INTEGER,
    bezier_pattern TEXT,
    min_travel_duration REAL,
    max_travel_duration REAL,
    FOREIGN KEY (ability_id) REFERENCES ability(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS ability_movement_circular_arc (
    ability_id TEXT PRIMARY KEY,
    min_travel_duration REAL,
    max_travel_duration REAL,
    circular_arc_radius_scale REAL,
    circular_arc_radius_min_offset REAL,
    circular_arc_clockwise INTEGER CHECK (circular_arc_clockwise IN (0, 1) OR circular_arc_clockwise IS NULL),
    bow_world_up INTEGER CHECK (bow_world_up IN (0, 1) OR bow_world_up IS NULL),
    FOREIGN KEY (ability_id) REFERENCES ability(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS ability_movement_attach_to_host (
    ability_id TEXT PRIMARY KEY,
    projectile_count INTEGER,
    max_distance REAL,
    max_travel_duration REAL,
    FOREIGN KEY (ability_id) REFERENCES ability(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS ability_movement_charge (
    ability_id TEXT PRIMARY KEY,
    move_speed REAL,
    max_distance REAL,
    max_travel_duration REAL,
    FOREIGN KEY (ability_id) REFERENCES ability(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS feature_definition (
    id TEXT PRIMARY KEY CHECK (trim(id) <> ''),
    feature_id TEXT NOT NULL CHECK (trim(feature_id) <> ''),
    name TEXT NOT NULL CHECK (trim(name) <> ''),
    handler_id TEXT,
    description TEXT,
    category TEXT,
    trigger_mode TEXT,
    cooldown REAL,
    trigger_event_type TEXT,
    trigger_chance REAL,
    is_enabled INTEGER CHECK (is_enabled IN (0, 1) OR is_enabled IS NULL)
);

CREATE TABLE IF NOT EXISTS feature_modifier (
    id TEXT PRIMARY KEY CHECK (trim(id) <> ''),
    feature_id TEXT NOT NULL CHECK (trim(feature_id) <> ''),
    name TEXT,
    target_key TEXT NOT NULL CHECK (trim(target_key) <> ''),
    modifier_type TEXT NOT NULL CHECK (trim(modifier_type) <> ''),
    modifier_value REAL NOT NULL,
    priority INTEGER,
    description TEXT,
    FOREIGN KEY (feature_id) REFERENCES feature_definition(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS system_config (
    id TEXT PRIMARY KEY CHECK (trim(id) <> ''),
    mount_group TEXT,
    tags TEXT,
    required INTEGER CHECK (required IN (0, 1) OR required IS NULL),
    auto_load INTEGER CHECK (auto_load IN (0, 1) OR auto_load IS NULL),
    start_enabled INTEGER CHECK (start_enabled IN (0, 1) OR start_enabled IS NULL),
    priority INTEGER,
    dependencies TEXT,
    allowed_flow_states TEXT,
    blocked_overlays TEXT,
    allowed_simulation_states TEXT,
    description TEXT
);

CREATE TABLE IF NOT EXISTS system_preset (
    id TEXT PRIMARY KEY CHECK (trim(id) <> ''),
    preset_name TEXT NOT NULL CHECK (trim(preset_name) <> ''),
    is_active INTEGER CHECK (is_active IN (0, 1) OR is_active IS NULL),
    enabled_tags TEXT,
    enabled_system_ids TEXT,
    disabled_system_ids TEXT,
    description TEXT
);

CREATE TABLE IF NOT EXISTS spawn_config (
    id TEXT PRIMARY KEY CHECK (trim(id) <> ''),
    wave_duration REAL,
    max_waves INTEGER,
    wave_break_time REAL,
    description TEXT
);

DROP VIEW IF EXISTS dataos_runtime_field_stream;
CREATE VIEW dataos_runtime_field_stream AS
SELECT
    'data_field' AS source_table,
    f.table_id || ':' || f.record_id AS source_row_id,
    f.field_key AS source_column,
    f.table_id,
    f.record_id,
    COALESCE(r.display_name, '') AS display_name,
    f.field_key,
    f.value_type,
    f.value_text
FROM data_field f
LEFT JOIN data_record r ON r.table_id = f.table_id AND r.record_id = f.record_id
UNION ALL
SELECT
    'unit_player',
    u.id,
    m.source_column,
    'unit.player',
    u.id,
    u.name,
    m.field_key,
    m.value_type,
    CASE WHEN m.value_type = 'bool' THEN CASE CAST(j.value AS INTEGER) WHEN 1 THEN 'true' ELSE 'false' END ELSE CAST(j.value AS TEXT) END
FROM unit_player u
JOIN json_each(json_object(
    'name', u.name,
    'entity_type', u.entity_type,
    'death_type', u.death_type,
    'visual_scene_path', u.visual_scene_path,
    'health_bar_height', u.health_bar_height,
    'is_show_health_bar', u.is_show_health_bar,
    'pickup_range', u.pickup_range,
    'exp_reward', u.exp_reward,
    'detection_range', u.detection_range,
    'collision_team', u.collision_team,
    'collision_layer', u.collision_layer,
    'collision_mask', u.collision_mask,
    'collision_radius', u.collision_radius,
    'max_hp', u.max_hp,
    'current_hp', u.current_hp,
    'armor', u.armor,
    'crit_rate', u.crit_rate,
    'life_steal', u.life_steal,
    'contact_damage', u.contact_damage,
    'contact_damage_interval', u.contact_damage_interval,
    'move_speed', u.move_speed,
    'acceleration', u.acceleration,
    'attack_damage', u.attack_damage,
    'attack_range', u.attack_range,
    'attack_interval', u.attack_interval,
    'attack_wind_up_time', u.attack_wind_up_time,
    'attack_recovery_time', u.attack_recovery_time,
    'ai_is_enabled', u.ai_is_enabled,
    'ai_attack_range', u.ai_attack_range
)) j
JOIN (
    SELECT 'name' source_column, 'Unit.Name' field_key, 'string' value_type UNION ALL
    SELECT 'entity_type', 'Unit.EntityType', 'string' UNION ALL
    SELECT 'death_type', 'Unit.DeathType', 'string' UNION ALL
    SELECT 'visual_scene_path', 'Unit.VisualScenePath', 'string' UNION ALL
    SELECT 'health_bar_height', 'Unit.HealthBarHeight', 'float' UNION ALL
    SELECT 'is_show_health_bar', 'Unit.IsShowHealthBar', 'bool' UNION ALL
    SELECT 'pickup_range', 'Unit.PickupRange', 'float' UNION ALL
    SELECT 'exp_reward', 'Unit.ExpReward', 'int' UNION ALL
    SELECT 'detection_range', 'Unit.DetectionRange', 'float' UNION ALL
    SELECT 'collision_team', 'Collision.Team', 'int' UNION ALL
    SELECT 'collision_layer', 'Collision.Layer', 'int' UNION ALL
    SELECT 'collision_mask', 'Collision.Mask', 'int' UNION ALL
    SELECT 'collision_radius', 'Collision.Radius', 'float' UNION ALL
    SELECT 'max_hp', 'Damage.MaxHp', 'float' UNION ALL
    SELECT 'current_hp', 'Damage.CurrentHp', 'float' UNION ALL
    SELECT 'armor', 'Damage.Armor', 'float' UNION ALL
    SELECT 'crit_rate', 'Damage.CritRate', 'float' UNION ALL
    SELECT 'life_steal', 'Damage.LifeSteal', 'float' UNION ALL
    SELECT 'contact_damage', 'Damage.ContactDamage', 'float' UNION ALL
    SELECT 'contact_damage_interval', 'Damage.ContactDamageInterval', 'float' UNION ALL
    SELECT 'move_speed', 'Movement.MoveSpeed', 'float' UNION ALL
    SELECT 'acceleration', 'Movement.Acceleration', 'float' UNION ALL
    SELECT 'attack_damage', 'Attack.Damage', 'float' UNION ALL
    SELECT 'attack_range', 'Attack.Range', 'float' UNION ALL
    SELECT 'attack_interval', 'Attack.Interval', 'float' UNION ALL
    SELECT 'attack_wind_up_time', 'Attack.WindUpTime', 'float' UNION ALL
    SELECT 'attack_recovery_time', 'Attack.RecoveryTime', 'float' UNION ALL
    SELECT 'ai_is_enabled', 'AI.IsEnabled', 'bool' UNION ALL
    SELECT 'ai_attack_range', 'AI.AttackRange', 'float'
) m ON m.source_column = j.key
WHERE j.type <> 'null'
UNION ALL
SELECT
    'unit_enemy',
    u.id,
    m.source_column,
    'unit.enemy',
    u.id,
    u.name,
    m.field_key,
    m.value_type,
    CASE WHEN m.value_type = 'bool' THEN CASE CAST(j.value AS INTEGER) WHEN 1 THEN 'true' ELSE 'false' END ELSE CAST(j.value AS TEXT) END
FROM unit_enemy u
JOIN json_each(json_object(
    'name', u.name,
    'entity_type', u.entity_type,
    'death_type', u.death_type,
    'visual_scene_path', u.visual_scene_path,
    'health_bar_height', u.health_bar_height,
    'is_show_health_bar', u.is_show_health_bar,
    'pickup_range', u.pickup_range,
    'exp_reward', u.exp_reward,
    'detection_range', u.detection_range,
    'collision_team', u.collision_team,
    'collision_layer', u.collision_layer,
    'collision_mask', u.collision_mask,
    'collision_radius', u.collision_radius,
    'max_hp', u.max_hp,
    'current_hp', u.current_hp,
    'armor', u.armor,
    'crit_rate', u.crit_rate,
    'life_steal', u.life_steal,
    'contact_damage', u.contact_damage,
    'contact_damage_interval', u.contact_damage_interval,
    'move_speed', u.move_speed,
    'acceleration', u.acceleration,
    'attack_damage', u.attack_damage,
    'attack_range', u.attack_range,
    'attack_interval', u.attack_interval,
    'attack_wind_up_time', u.attack_wind_up_time,
    'attack_recovery_time', u.attack_recovery_time,
    'ai_is_enabled', u.ai_is_enabled,
    'ai_attack_range', u.ai_attack_range,
    'spawn_is_enabled', u.spawn_is_enabled,
    'spawn_position_strategy', u.spawn_position_strategy,
    'spawn_min_wave', u.spawn_min_wave,
    'spawn_max_wave', u.spawn_max_wave,
    'spawn_interval', u.spawn_interval,
    'spawn_max_count_per_wave', u.spawn_max_count_per_wave,
    'spawn_single_count', u.spawn_single_count,
    'spawn_single_variance', u.spawn_single_variance,
    'spawn_start_delay', u.spawn_start_delay,
    'spawn_weight', u.spawn_weight
)) j
JOIN (
    SELECT 'name' source_column, 'Unit.Name' field_key, 'string' value_type UNION ALL
    SELECT 'entity_type', 'Unit.EntityType', 'string' UNION ALL
    SELECT 'death_type', 'Unit.DeathType', 'string' UNION ALL
    SELECT 'visual_scene_path', 'Unit.VisualScenePath', 'string' UNION ALL
    SELECT 'health_bar_height', 'Unit.HealthBarHeight', 'float' UNION ALL
    SELECT 'is_show_health_bar', 'Unit.IsShowHealthBar', 'bool' UNION ALL
    SELECT 'pickup_range', 'Unit.PickupRange', 'float' UNION ALL
    SELECT 'exp_reward', 'Unit.ExpReward', 'int' UNION ALL
    SELECT 'detection_range', 'Unit.DetectionRange', 'float' UNION ALL
    SELECT 'collision_team', 'Collision.Team', 'int' UNION ALL
    SELECT 'collision_layer', 'Collision.Layer', 'int' UNION ALL
    SELECT 'collision_mask', 'Collision.Mask', 'int' UNION ALL
    SELECT 'collision_radius', 'Collision.Radius', 'float' UNION ALL
    SELECT 'max_hp', 'Damage.MaxHp', 'float' UNION ALL
    SELECT 'current_hp', 'Damage.CurrentHp', 'float' UNION ALL
    SELECT 'armor', 'Damage.Armor', 'float' UNION ALL
    SELECT 'crit_rate', 'Damage.CritRate', 'float' UNION ALL
    SELECT 'life_steal', 'Damage.LifeSteal', 'float' UNION ALL
    SELECT 'contact_damage', 'Damage.ContactDamage', 'float' UNION ALL
    SELECT 'contact_damage_interval', 'Damage.ContactDamageInterval', 'float' UNION ALL
    SELECT 'move_speed', 'Movement.MoveSpeed', 'float' UNION ALL
    SELECT 'acceleration', 'Movement.Acceleration', 'float' UNION ALL
    SELECT 'attack_damage', 'Attack.Damage', 'float' UNION ALL
    SELECT 'attack_range', 'Attack.Range', 'float' UNION ALL
    SELECT 'attack_interval', 'Attack.Interval', 'float' UNION ALL
    SELECT 'attack_wind_up_time', 'Attack.WindUpTime', 'float' UNION ALL
    SELECT 'attack_recovery_time', 'Attack.RecoveryTime', 'float' UNION ALL
    SELECT 'ai_is_enabled', 'AI.IsEnabled', 'bool' UNION ALL
    SELECT 'ai_attack_range', 'AI.AttackRange', 'float' UNION ALL
    SELECT 'spawn_is_enabled', 'Spawn.IsEnabled', 'bool' UNION ALL
    SELECT 'spawn_position_strategy', 'Spawn.PositionStrategy', 'string' UNION ALL
    SELECT 'spawn_min_wave', 'Spawn.MinWave', 'int' UNION ALL
    SELECT 'spawn_max_wave', 'Spawn.MaxWave', 'int' UNION ALL
    SELECT 'spawn_interval', 'Spawn.Interval', 'float' UNION ALL
    SELECT 'spawn_max_count_per_wave', 'Spawn.MaxCountPerWave', 'int' UNION ALL
    SELECT 'spawn_single_count', 'Spawn.SingleCount', 'int' UNION ALL
    SELECT 'spawn_single_variance', 'Spawn.SingleVariance', 'int' UNION ALL
    SELECT 'spawn_start_delay', 'Spawn.StartDelay', 'float' UNION ALL
    SELECT 'spawn_weight', 'Spawn.Weight', 'int'
) m ON m.source_column = j.key
WHERE j.type <> 'null'
UNION ALL
SELECT
    'unit_targeting_indicator',
    u.id,
    m.source_column,
    'unit.targeting_indicator',
    u.id,
    u.name,
    m.field_key,
    m.value_type,
    CASE WHEN m.value_type = 'bool' THEN CASE CAST(j.value AS INTEGER) WHEN 1 THEN 'true' ELSE 'false' END ELSE CAST(j.value AS TEXT) END
FROM unit_targeting_indicator u
JOIN json_each(json_object(
    'name', u.name,
    'entity_type', u.entity_type,
    'visual_scene_path', u.visual_scene_path,
    'is_show_health_bar', u.is_show_health_bar,
    'collision_team', u.collision_team,
    'collision_layer', u.collision_layer,
    'collision_mask', u.collision_mask,
    'collision_radius', u.collision_radius,
    'max_hp', u.max_hp,
    'current_hp', u.current_hp,
    'is_invulnerable', u.is_invulnerable,
    'move_speed', u.move_speed,
    'attack_interval', u.attack_interval
)) j
JOIN (
    SELECT 'name' source_column, 'Unit.Name' field_key, 'string' value_type UNION ALL
    SELECT 'entity_type', 'Unit.EntityType', 'string' UNION ALL
    SELECT 'visual_scene_path', 'Unit.VisualScenePath', 'string' UNION ALL
    SELECT 'is_show_health_bar', 'Unit.IsShowHealthBar', 'bool' UNION ALL
    SELECT 'collision_team', 'Collision.Team', 'int' UNION ALL
    SELECT 'collision_layer', 'Collision.Layer', 'int' UNION ALL
    SELECT 'collision_mask', 'Collision.Mask', 'int' UNION ALL
    SELECT 'collision_radius', 'Collision.Radius', 'float' UNION ALL
    SELECT 'max_hp', 'Damage.MaxHp', 'float' UNION ALL
    SELECT 'current_hp', 'Damage.CurrentHp', 'float' UNION ALL
    SELECT 'is_invulnerable', 'Damage.IsInvulnerable', 'bool' UNION ALL
    SELECT 'move_speed', 'Movement.MoveSpeed', 'float' UNION ALL
    SELECT 'attack_interval', 'Attack.Interval', 'float'
) m ON m.source_column = j.key
WHERE j.type <> 'null'
UNION ALL
SELECT
    'ability',
    a.id,
    m.source_column,
    'ability',
    a.id,
    a.name,
    m.field_key,
    m.value_type,
    CASE WHEN m.value_type = 'bool' THEN CASE CAST(j.value AS INTEGER) WHEN 1 THEN 'true' ELSE 'false' END ELSE CAST(j.value AS TEXT) END
FROM ability a
JOIN json_each(json_object(
    'name', a.name,
    'type', a.type,
    'trigger_mode', a.trigger_mode,
    'target_selection', a.target_selection,
    'feature_group_id', a.feature_group_id,
    'feature_handler_id', a.feature_handler_id,
    'description', a.description,
    'icon_path', a.icon_path,
    'cost_type', a.cost_type,
    'cost_amount', a.cost_amount,
    'cooldown', a.cooldown,
    'cast_range', a.cast_range,
    'auto_target_range', a.auto_target_range,
    'auto_target_max_targets', a.auto_target_max_targets,
    'auto_target_ignore_same_team', a.auto_target_ignore_same_team,
    'auto_target_requires_damageable', a.auto_target_requires_damageable,
    'damage', a.damage,
    'damage_interval', a.damage_interval,
    'damage_repeat_count', a.damage_repeat_count,
    'apply_immediate_damage', a.apply_immediate_damage,
    'effect_radius', a.effect_radius,
    'chain_count', a.chain_count,
    'chain_range', a.chain_range,
    'chain_delay', a.chain_delay,
    'chain_damage_decay', a.chain_damage_decay,
    'level', a.level,
    'max_level', a.max_level,
    'uses_charges', a.uses_charges,
    'max_charges', a.max_charges,
    'current_charges', a.current_charges,
    'charge_time', a.charge_time
)) j
JOIN (
    SELECT 'name' source_column, 'Ability.Name' field_key, 'string' value_type UNION ALL
    SELECT 'type', 'Ability.Type', 'string' UNION ALL
    SELECT 'trigger_mode', 'Ability.TriggerMode', 'string' UNION ALL
    SELECT 'target_selection', 'Ability.TargetSelection', 'string' UNION ALL
    SELECT 'feature_group_id', 'Ability.FeatureGroupId', 'string' UNION ALL
    SELECT 'feature_handler_id', 'Ability.FeatureHandlerId', 'string' UNION ALL
    SELECT 'description', 'Ability.Description', 'string' UNION ALL
    SELECT 'icon_path', 'Ability.IconPath', 'string' UNION ALL
    SELECT 'cost_type', 'Ability.CostType', 'string' UNION ALL
    SELECT 'cost_amount', 'Ability.CostAmount', 'float' UNION ALL
    SELECT 'cooldown', 'Ability.Cooldown', 'float' UNION ALL
    SELECT 'cast_range', 'Ability.CastRange', 'float' UNION ALL
    SELECT 'auto_target_range', 'Ability.AutoTargetRange', 'float' UNION ALL
    SELECT 'auto_target_max_targets', 'Ability.AutoTargetMaxTargets', 'int' UNION ALL
    SELECT 'auto_target_ignore_same_team', 'Ability.AutoTargetIgnoreSameTeam', 'bool' UNION ALL
    SELECT 'auto_target_requires_damageable', 'Ability.AutoTargetRequiresDamageable', 'bool' UNION ALL
    SELECT 'damage', 'Ability.Damage', 'float' UNION ALL
    SELECT 'damage_interval', 'Ability.DamageInterval', 'float' UNION ALL
    SELECT 'damage_repeat_count', 'Ability.DamageRepeatCount', 'int' UNION ALL
    SELECT 'apply_immediate_damage', 'Ability.ApplyImmediateDamage', 'bool' UNION ALL
    SELECT 'effect_radius', 'Ability.EffectRadius', 'float' UNION ALL
    SELECT 'chain_count', 'Ability.ChainCount', 'int' UNION ALL
    SELECT 'chain_range', 'Ability.ChainRange', 'float' UNION ALL
    SELECT 'chain_delay', 'Ability.ChainDelay', 'float' UNION ALL
    SELECT 'chain_damage_decay', 'Ability.ChainDamageDecay', 'float' UNION ALL
    SELECT 'level', 'Ability.Level', 'int' UNION ALL
    SELECT 'max_level', 'Ability.MaxLevel', 'int' UNION ALL
    SELECT 'uses_charges', 'Ability.UsesCharges', 'bool' UNION ALL
    SELECT 'max_charges', 'Ability.MaxCharges', 'int' UNION ALL
    SELECT 'current_charges', 'Ability.CurrentCharges', 'int' UNION ALL
    SELECT 'charge_time', 'Ability.ChargeTime', 'float'
) m ON m.source_column = j.key
WHERE j.type <> 'null'
UNION ALL
SELECT
    'ability_effect',
    e.ability_id,
    m.source_column,
    'ability',
    e.ability_id,
    a.name,
    m.field_key,
    m.value_type,
    CAST(j.value AS TEXT)
FROM ability_effect e
JOIN ability a ON a.id = e.ability_id
JOIN json_each(json_object(
    'scene_path', e.scene_path,
    'name', e.name,
    'animation_name', e.animation_name,
    'duration', e.duration
)) j
JOIN (
    SELECT 'scene_path' source_column, 'Effect.ScenePath' field_key, 'string' value_type UNION ALL
    SELECT 'name', 'Effect.Name', 'string' UNION ALL
    SELECT 'animation_name', 'Effect.AnimationName', 'string' UNION ALL
    SELECT 'duration', 'Effect.Duration', 'float'
) m ON m.source_column = j.key
WHERE j.type <> 'null'
UNION ALL
SELECT
    'ability_projectile',
    p.ability_id,
    m.source_column,
    'ability',
    p.ability_id,
    a.name,
    m.field_key,
    m.value_type,
    CAST(j.value AS TEXT)
FROM ability_projectile p
JOIN ability a ON a.id = p.ability_id
JOIN json_each(json_object(
    'scene_path', p.scene_path,
    'speed', p.speed,
    'max_hit_count', p.max_hit_count,
    'max_life_time', p.max_life_time,
    'damage', p.damage
)) j
JOIN (
    SELECT 'scene_path' source_column, 'Projectile.ScenePath' field_key, 'string' value_type UNION ALL
    SELECT 'speed', 'Projectile.Speed', 'float' UNION ALL
    SELECT 'max_hit_count', 'Projectile.MaxHitCount', 'int' UNION ALL
    SELECT 'max_life_time', 'Projectile.MaxLifeTime', 'float' UNION ALL
    SELECT 'damage', 'Projectile.Damage', 'float'
) m ON m.source_column = j.key
WHERE j.type <> 'null'
UNION ALL
SELECT 'ability_line_effect', l.ability_id, 'scene_path', 'ability', l.ability_id, a.name, 'Ability.LineEffectScenePath', 'string', l.scene_path
FROM ability_line_effect l
JOIN ability a ON a.id = l.ability_id
UNION ALL
SELECT 'ability_movement_sine_wave', s.ability_id, 'move_mode', 'ability', s.ability_id, a.name, 'Movement.Handler.MoveMode', 'string', 'SineWave'
FROM ability_movement_sine_wave s
JOIN ability a ON a.id = s.ability_id
UNION ALL
SELECT
    'ability_movement_sine_wave',
    s.ability_id,
    m.source_column,
    'ability',
    s.ability_id,
    a.name,
    m.field_key,
    m.value_type,
    CAST(j.value AS TEXT)
FROM ability_movement_sine_wave s
JOIN ability a ON a.id = s.ability_id
JOIN json_each(json_object(
    'wave_amplitude', s.wave_amplitude,
    'wave_frequency', s.wave_frequency,
    'wave_phase', s.wave_phase,
    'max_distance', s.max_distance
)) j
JOIN (
    SELECT 'wave_amplitude' source_column, 'Movement.WaveAmplitude' field_key, 'float' value_type UNION ALL
    SELECT 'wave_frequency', 'Movement.WaveFrequency', 'float' UNION ALL
    SELECT 'wave_phase', 'Movement.WavePhase', 'float' UNION ALL
    SELECT 'max_distance', 'Movement.Handler.MaxDistance', 'float'
) m ON m.source_column = j.key
WHERE j.type <> 'null'
UNION ALL
SELECT 'ability_movement_orbit', o.ability_id, 'move_mode', 'ability', o.ability_id, a.name, 'Movement.Handler.MoveMode', 'string', 'Orbit'
FROM ability_movement_orbit o
JOIN ability a ON a.id = o.ability_id
UNION ALL
SELECT
    'ability_movement_orbit',
    o.ability_id,
    m.source_column,
    'ability',
    o.ability_id,
    a.name,
    m.field_key,
    m.value_type,
    CASE WHEN m.value_type = 'bool' THEN CASE CAST(j.value AS INTEGER) WHEN 1 THEN 'true' ELSE 'false' END ELSE CAST(j.value AS TEXT) END
FROM ability_movement_orbit o
JOIN ability a ON a.id = o.ability_id
JOIN json_each(json_object(
    'projectile_count', o.projectile_count,
    'orbit_radius', o.orbit_radius,
    'orbit_angular_speed', o.orbit_angular_speed,
    'orbit_angular_acceleration', o.orbit_angular_acceleration,
    'orbit_total_angle', o.orbit_total_angle,
    'is_orbit_clockwise', o.is_orbit_clockwise,
    'max_travel_duration', o.max_travel_duration
)) j
JOIN (
    SELECT 'projectile_count' source_column, 'Movement.Handler.ProjectileCount' field_key, 'int' value_type UNION ALL
    SELECT 'orbit_radius', 'Movement.OrbitRadius', 'float' UNION ALL
    SELECT 'orbit_angular_speed', 'Movement.OrbitAngularSpeed', 'float' UNION ALL
    SELECT 'orbit_angular_acceleration', 'Movement.OrbitAngularAcceleration', 'float' UNION ALL
    SELECT 'orbit_total_angle', 'Movement.OrbitTotalAngle', 'float' UNION ALL
    SELECT 'is_orbit_clockwise', 'Movement.IsOrbitClockwise', 'bool' UNION ALL
    SELECT 'max_travel_duration', 'Movement.Handler.MaxTravelDuration', 'float'
) m ON m.source_column = j.key
WHERE j.type <> 'null'
UNION ALL
SELECT 'ability_movement_boomerang', b.ability_id, 'move_mode', 'ability', b.ability_id, a.name, 'Movement.Handler.MoveMode', 'string', 'Boomerang'
FROM ability_movement_boomerang b
JOIN ability a ON a.id = b.ability_id
UNION ALL
SELECT
    'ability_movement_boomerang',
    b.ability_id,
    m.source_column,
    'ability',
    b.ability_id,
    a.name,
    m.field_key,
    m.value_type,
    CASE WHEN m.value_type = 'bool' THEN CASE CAST(j.value AS INTEGER) WHEN 1 THEN 'true' ELSE 'false' END ELSE CAST(j.value AS TEXT) END
FROM ability_movement_boomerang b
JOIN ability a ON a.id = b.ability_id
JOIN json_each(json_object(
    'boomerang_arc_height', b.boomerang_arc_height,
    'boomerang_pause_time', b.boomerang_pause_time,
    'boomerang_is_clockwise', b.boomerang_is_clockwise,
    'boomerang_return_speed_multiplier', b.boomerang_return_speed_multiplier
)) j
JOIN (
    SELECT 'boomerang_arc_height' source_column, 'Movement.BoomerangArcHeight' field_key, 'float' value_type UNION ALL
    SELECT 'boomerang_pause_time', 'Movement.BoomerangPauseTime', 'float' UNION ALL
    SELECT 'boomerang_is_clockwise', 'Movement.BoomerangIsClockwise', 'bool' UNION ALL
    SELECT 'boomerang_return_speed_multiplier', 'Movement.BoomerangReturnSpeedMultiplier', 'float'
) m ON m.source_column = j.key
WHERE j.type <> 'null'
UNION ALL
SELECT 'ability_movement_bezier', b.ability_id, 'move_mode', 'ability', b.ability_id, a.name, 'Movement.Handler.MoveMode', 'string', 'BezierCurve'
FROM ability_movement_bezier b
JOIN ability a ON a.id = b.ability_id
UNION ALL
SELECT
    'ability_movement_bezier',
    b.ability_id,
    m.source_column,
    'ability',
    b.ability_id,
    a.name,
    m.field_key,
    m.value_type,
    CAST(j.value AS TEXT)
FROM ability_movement_bezier b
JOIN ability a ON a.id = b.ability_id
JOIN json_each(json_object(
    'projectile_count', b.projectile_count,
    'bezier_degree', b.bezier_degree,
    'bezier_pattern', b.bezier_pattern,
    'min_travel_duration', b.min_travel_duration,
    'max_travel_duration', b.max_travel_duration
)) j
JOIN (
    SELECT 'projectile_count' source_column, 'Movement.Handler.ProjectileCount' field_key, 'int' value_type UNION ALL
    SELECT 'bezier_degree', 'Movement.BezierDegree', 'int' UNION ALL
    SELECT 'bezier_pattern', 'Movement.BezierPattern', 'string' UNION ALL
    SELECT 'min_travel_duration', 'Movement.Handler.MinTravelDuration', 'float' UNION ALL
    SELECT 'max_travel_duration', 'Movement.Handler.MaxTravelDuration', 'float'
) m ON m.source_column = j.key
WHERE j.type <> 'null'
UNION ALL
SELECT 'ability_movement_circular_arc', c.ability_id, 'move_mode', 'ability', c.ability_id, a.name, 'Movement.Handler.MoveMode', 'string', 'CircularArc'
FROM ability_movement_circular_arc c
JOIN ability a ON a.id = c.ability_id
UNION ALL
SELECT
    'ability_movement_circular_arc',
    c.ability_id,
    m.source_column,
    'ability',
    c.ability_id,
    a.name,
    m.field_key,
    m.value_type,
    CASE WHEN m.value_type = 'bool' THEN CASE CAST(j.value AS INTEGER) WHEN 1 THEN 'true' ELSE 'false' END ELSE CAST(j.value AS TEXT) END
FROM ability_movement_circular_arc c
JOIN ability a ON a.id = c.ability_id
JOIN json_each(json_object(
    'min_travel_duration', c.min_travel_duration,
    'max_travel_duration', c.max_travel_duration,
    'circular_arc_radius_scale', c.circular_arc_radius_scale,
    'circular_arc_radius_min_offset', c.circular_arc_radius_min_offset,
    'circular_arc_clockwise', c.circular_arc_clockwise,
    'bow_world_up', c.bow_world_up
)) j
JOIN (
    SELECT 'min_travel_duration' source_column, 'Movement.Handler.MinTravelDuration' field_key, 'float' value_type UNION ALL
    SELECT 'max_travel_duration', 'Movement.Handler.MaxTravelDuration', 'float' UNION ALL
    SELECT 'circular_arc_radius_scale', 'Movement.CircularArcRadiusScale', 'float' UNION ALL
    SELECT 'circular_arc_radius_min_offset', 'Movement.CircularArcRadiusMinOffset', 'float' UNION ALL
    SELECT 'circular_arc_clockwise', 'Movement.CircularArcClockwise', 'bool' UNION ALL
    SELECT 'bow_world_up', 'Movement.BowWorldUp', 'bool'
) m ON m.source_column = j.key
WHERE j.type <> 'null'
UNION ALL
SELECT 'ability_movement_attach_to_host', h.ability_id, 'move_mode', 'ability', h.ability_id, a.name, 'Movement.Handler.MoveMode', 'string', 'AttachToHost'
FROM ability_movement_attach_to_host h
JOIN ability a ON a.id = h.ability_id
UNION ALL
SELECT
    'ability_movement_attach_to_host',
    h.ability_id,
    m.source_column,
    'ability',
    h.ability_id,
    a.name,
    m.field_key,
    m.value_type,
    CAST(j.value AS TEXT)
FROM ability_movement_attach_to_host h
JOIN ability a ON a.id = h.ability_id
JOIN json_each(json_object(
    'projectile_count', h.projectile_count,
    'max_distance', h.max_distance,
    'max_travel_duration', h.max_travel_duration
)) j
JOIN (
    SELECT 'projectile_count' source_column, 'Movement.Handler.ProjectileCount' field_key, 'int' value_type UNION ALL
    SELECT 'max_distance', 'Movement.Handler.MaxDistance', 'float' UNION ALL
    SELECT 'max_travel_duration', 'Movement.Handler.MaxTravelDuration', 'float'
) m ON m.source_column = j.key
WHERE j.type <> 'null'
UNION ALL
SELECT 'ability_movement_charge', c.ability_id, 'move_mode', 'ability', c.ability_id, a.name, 'Movement.Handler.MoveMode', 'string', 'Charge'
FROM ability_movement_charge c
JOIN ability a ON a.id = c.ability_id
UNION ALL
SELECT
    'ability_movement_charge',
    c.ability_id,
    m.source_column,
    'ability',
    c.ability_id,
    a.name,
    m.field_key,
    m.value_type,
    CAST(j.value AS TEXT)
FROM ability_movement_charge c
JOIN ability a ON a.id = c.ability_id
JOIN json_each(json_object(
    'move_speed', c.move_speed,
    'max_distance', c.max_distance,
    'max_travel_duration', c.max_travel_duration
)) j
JOIN (
    SELECT 'move_speed' source_column, 'Movement.MoveSpeed' field_key, 'float' value_type UNION ALL
    SELECT 'max_distance', 'Movement.Handler.MaxDistance', 'float' UNION ALL
    SELECT 'max_travel_duration', 'Movement.Handler.MaxTravelDuration', 'float'
) m ON m.source_column = j.key
WHERE j.type <> 'null'
UNION ALL
SELECT
    'feature_definition',
    f.id,
    m.source_column,
    'feature.definition',
    f.id,
    f.name,
    m.field_key,
    m.value_type,
    CASE WHEN m.value_type = 'bool' THEN CASE CAST(j.value AS INTEGER) WHEN 1 THEN 'true' ELSE 'false' END ELSE CAST(j.value AS TEXT) END
FROM feature_definition f
JOIN json_each(json_object(
    'feature_id', f.feature_id,
    'handler_id', f.handler_id,
    'description', f.description,
    'category', f.category,
    'trigger_mode', f.trigger_mode,
    'cooldown', f.cooldown,
    'trigger_event_type', f.trigger_event_type,
    'trigger_chance', f.trigger_chance,
    'is_enabled', f.is_enabled
)) j
JOIN (
    SELECT 'feature_id' source_column, 'Feature.Id' field_key, 'string' value_type UNION ALL
    SELECT 'handler_id', 'Feature.HandlerId', 'string' UNION ALL
    SELECT 'description', 'Feature.Description', 'string' UNION ALL
    SELECT 'category', 'Feature.Category', 'string' UNION ALL
    SELECT 'trigger_mode', 'Feature.TriggerMode', 'string' UNION ALL
    SELECT 'cooldown', 'Feature.Cooldown', 'float' UNION ALL
    SELECT 'trigger_event_type', 'Feature.TriggerEventType', 'string' UNION ALL
    SELECT 'trigger_chance', 'Feature.TriggerChance', 'float' UNION ALL
    SELECT 'is_enabled', 'Feature.IsEnabled', 'bool'
) m ON m.source_column = j.key
WHERE j.type <> 'null'
UNION ALL
SELECT
    'feature_modifier',
    f.id,
    m.source_column,
    'feature.modifier',
    f.id,
    COALESCE(f.name, f.id),
    m.field_key,
    m.value_type,
    CAST(j.value AS TEXT)
FROM feature_modifier f
JOIN json_each(json_object(
    'feature_id', f.feature_id,
    'target_key', f.target_key,
    'modifier_type', f.modifier_type,
    'modifier_value', f.modifier_value,
    'priority', f.priority
)) j
JOIN (
    SELECT 'feature_id' source_column, 'Feature.Id' field_key, 'string' value_type UNION ALL
    SELECT 'target_key', 'Feature.Modifier.TargetKey', 'string' UNION ALL
    SELECT 'modifier_type', 'Feature.Modifier.Type', 'string' UNION ALL
    SELECT 'modifier_value', 'Feature.Modifier.Value', 'float' UNION ALL
    SELECT 'priority', 'Feature.Modifier.Priority', 'int'
) m ON m.source_column = j.key
WHERE j.type <> 'null'
UNION ALL
SELECT
    'system_config',
    s.id,
    m.source_column,
    'system.config',
    s.id,
    s.id,
    m.field_key,
    m.value_type,
    CASE WHEN m.value_type = 'bool' THEN CASE CAST(j.value AS INTEGER) WHEN 1 THEN 'true' ELSE 'false' END ELSE CAST(j.value AS TEXT) END
FROM system_config s
JOIN json_each(json_object(
    'id', s.id,
    'mount_group', s.mount_group,
    'tags', s.tags,
    'required', s.required,
    'auto_load', s.auto_load,
    'start_enabled', s.start_enabled,
    'priority', s.priority,
    'dependencies', s.dependencies,
    'allowed_flow_states', s.allowed_flow_states,
    'blocked_overlays', s.blocked_overlays,
    'allowed_simulation_states', s.allowed_simulation_states,
    'description', s.description
)) j
JOIN (
    SELECT 'id' source_column, 'Schedule.SystemId' field_key, 'string' value_type UNION ALL
    SELECT 'mount_group', 'Schedule.MountGroup', 'string' UNION ALL
    SELECT 'tags', 'Schedule.Tags', 'string' UNION ALL
    SELECT 'required', 'Schedule.Required', 'bool' UNION ALL
    SELECT 'auto_load', 'Schedule.AutoLoad', 'bool' UNION ALL
    SELECT 'start_enabled', 'Schedule.StartEnabled', 'bool' UNION ALL
    SELECT 'priority', 'Schedule.Priority', 'int' UNION ALL
    SELECT 'dependencies', 'Schedule.Dependencies', 'string' UNION ALL
    SELECT 'allowed_flow_states', 'Schedule.AllowedFlowStates', 'string' UNION ALL
    SELECT 'blocked_overlays', 'Schedule.BlockedOverlays', 'string' UNION ALL
    SELECT 'allowed_simulation_states', 'Schedule.AllowedSimulationStates', 'string' UNION ALL
    SELECT 'description', 'Schedule.Description', 'string'
) m ON m.source_column = j.key
WHERE j.type <> 'null'
UNION ALL
SELECT
    'system_preset',
    p.id,
    m.source_column,
    'system.preset',
    p.id,
    p.preset_name,
    m.field_key,
    m.value_type,
    CASE WHEN m.value_type = 'bool' THEN CASE CAST(j.value AS INTEGER) WHEN 1 THEN 'true' ELSE 'false' END ELSE CAST(j.value AS TEXT) END
FROM system_preset p
JOIN json_each(json_object(
    'preset_name', p.preset_name,
    'is_active', p.is_active,
    'enabled_tags', p.enabled_tags,
    'enabled_system_ids', p.enabled_system_ids,
    'disabled_system_ids', p.disabled_system_ids,
    'description', p.description
)) j
JOIN (
    SELECT 'preset_name' source_column, 'Schedule.PresetName' field_key, 'string' value_type UNION ALL
    SELECT 'is_active', 'Schedule.Preset.IsActive', 'bool' UNION ALL
    SELECT 'enabled_tags', 'Schedule.Preset.EnabledTags', 'string' UNION ALL
    SELECT 'enabled_system_ids', 'Schedule.Preset.EnabledSystemIds', 'string' UNION ALL
    SELECT 'disabled_system_ids', 'Schedule.Preset.DisabledSystemIds', 'string' UNION ALL
    SELECT 'description', 'Schedule.Description', 'string'
) m ON m.source_column = j.key
WHERE j.type <> 'null'
UNION ALL
SELECT
    'spawn_config',
    s.id,
    m.source_column,
    'spawn.config',
    s.id,
    COALESCE(s.description, s.id),
    m.field_key,
    m.value_type,
    CAST(j.value AS TEXT)
FROM spawn_config s
JOIN json_each(json_object(
    'wave_duration', s.wave_duration,
    'max_waves', s.max_waves,
    'wave_break_time', s.wave_break_time
)) j
JOIN (
    SELECT 'wave_duration' source_column, 'Schedule.Spawn.WaveDuration' field_key, 'float' value_type UNION ALL
    SELECT 'max_waves', 'Schedule.Spawn.MaxWaves', 'int' UNION ALL
    SELECT 'wave_break_time', 'Schedule.Spawn.WaveBreakTime', 'float'
) m ON m.source_column = j.key
WHERE j.type <> 'null';

DROP VIEW IF EXISTS dataos_content_path_ownership;
CREATE VIEW dataos_content_path_ownership AS
SELECT 'unit_player' AS source_table, id AS source_row_id, 'visual_scene_path' AS source_column, visual_scene_path AS resource_path
FROM unit_player
WHERE visual_scene_path IS NOT NULL AND trim(visual_scene_path) <> ''
UNION ALL
SELECT 'unit_enemy', id, 'visual_scene_path', visual_scene_path
FROM unit_enemy
WHERE visual_scene_path IS NOT NULL AND trim(visual_scene_path) <> ''
UNION ALL
SELECT 'unit_targeting_indicator', id, 'visual_scene_path', visual_scene_path
FROM unit_targeting_indicator
WHERE visual_scene_path IS NOT NULL AND trim(visual_scene_path) <> ''
UNION ALL
SELECT 'ability', id, 'icon_path', icon_path
FROM ability
WHERE icon_path IS NOT NULL AND trim(icon_path) <> ''
UNION ALL
SELECT 'ability_effect', ability_id, 'scene_path', scene_path
FROM ability_effect
WHERE scene_path IS NOT NULL AND trim(scene_path) <> ''
UNION ALL
SELECT 'ability_projectile', ability_id, 'scene_path', scene_path
FROM ability_projectile
WHERE scene_path IS NOT NULL AND trim(scene_path) <> ''
UNION ALL
SELECT 'ability_line_effect', ability_id, 'scene_path', scene_path
FROM ability_line_effect
WHERE scene_path IS NOT NULL AND trim(scene_path) <> '';

INSERT OR IGNORE INTO dataos_schema_version(version, description)
VALUES (2, 'DataOS table-first authoring schema and runtime projection view');
