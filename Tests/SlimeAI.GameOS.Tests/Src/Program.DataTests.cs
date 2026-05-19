using SlimeAI.GameOS.Capabilities.Ability;
using SlimeAI.GameOS.Capabilities.Attack;
using SlimeAI.GameOS.Capabilities.Damage;
using SlimeAI.GameOS.Capabilities.Movement;
using SlimeAI.GameOS.Runtime.Data;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Resource;
using SlimeAI.GameOS.Runtime.World;
using static TestAssert;

internal partial class Program
{
    static void TestRuntimeTypedDataApi()
    {
        var health = DataKey.Create<float>(
            "Test.Typed.Health",
            defaultValue: 10f,
            category: RuntimeDataTestCategory.Runtime,
            minValue: 0f,
            maxValue: 100f,
            supportsModifiers: true);
        var name = DataKey.Create<string>(
            "Test.Typed.Name",
            defaultValue: "unset",
            category: RuntimeDataTestCategory.Config);
        var option = DataKey.Create<int>(
            "Test.Typed.Option",
            defaultValue: 0,
            options: ["A", "B"]);
        var catalog = DataCatalog.CreateBuilder("test-runtime-data")
            .Add(health)
            .Add(name)
            .Add(option)
            .Build();
        var sink = new RecordingDataChangeSink();
        var data = new Data(catalog, sink);

        AssertEqual("typed get default", 10f, data.Get(health));
        AssertEqual("typed get caller fallback", 99f, data.Get(health, 99f));
        AssertEqual("typed tryget unset", false, data.TryGet(health, out var healthValue));
        AssertEqual("typed tryget default out", 10f, healthValue);
        AssertEqual("typed has unset", false, data.Has(health));

        AssertEqual("typed set", true, data.Set(health, 25f));
        AssertEqual("typed get set", 25f, data.Get(health));
        AssertEqual("typed has set", true, data.Has(health));
        AssertEqual("typed tryget set", true, data.TryGet(health, out healthValue));
        AssertEqual("typed tryget set out", 25f, healthValue);

        data.Add(health, 5f);
        data.Multiply(health, 2f);
        AssertEqual("typed add multiply", 60f, data.GetBase(health));
        AssertEqual("typed clamp", true, data.Set(health, 200f));
        AssertEqual("typed clamped value", 100f, data.Get(health));

        AssertEqual("typed option invalid", false, data.Set(option, 2));
        AssertEqual("typed option valid", true, data.Set(option, 1));

        data.Set(name, "unit");
        var all = data.GetAll();
        AssertEqual("typed get all count", 3, all.Count);
        AssertEqual("typed get all stable key", "unit", all[name.StableKey]);
        AssertEqual("typed change sink key", name.StableKey, sink.LastChange?.StableKey);

        data.ResetByCategory(RuntimeDataTestCategory.Runtime);
        AssertEqual("typed reset category", 10f, data.Get(health));

        AssertEqual("typed remove", true, data.Remove(health));
        AssertEqual("typed has removed", false, data.Has(health));
        AssertEqual("typed get removed default", 10f, data.Get(health));
    }

    static void TestRuntimeTypedDataModifierConstraints()
    {
        var modifiable = DataKey.Create<float>(
            "Test.Modifier.Modifiable",
            defaultValue: 10f,
            supportsModifiers: true);
        var nonmodifiable = DataKey.Create<float>(
            "Test.Modifier.Nonmodifiable",
            defaultValue: 10f);
        var text = DataKey.Create<string>(
            "Test.Modifier.Text",
            defaultValue: "base",
            supportsModifiers: true);
        var catalog = DataCatalog.CreateBuilder("test-runtime-modifier")
            .Add(modifiable)
            .Add(nonmodifiable)
            .Add(text)
            .Build();
        var data = new Data(catalog);

        data.Set(modifiable, 10f);
        AssertEqual("modifier nonmodifiable rejected", false, data.AddModifier(nonmodifiable, new DataModifier(ModifierType.Additive, 1f, id: "bad-no-support")));
        AssertEqual("modifier nonnumeric rejected", false, data.AddModifier(text, new DataModifier(ModifierType.Additive, 1f, id: "bad-text")));

        AssertEqual("modifier add", true, data.AddModifier(modifiable, new DataModifier(ModifierType.Additive, 5f, priority: 10, id: "add")));
        AssertEqual("modifier multiply", true, data.AddModifier(modifiable, new DataModifier(ModifierType.Multiplicative, 2f, priority: 20, id: "mul")));
        AssertEqual("modifier final", true, data.AddModifier(modifiable, new DataModifier(ModifierType.FinalAdditive, 1f, priority: 30, id: "final")));
        AssertEqual("modifier priority result", 31f, data.Get(modifiable));
        AssertEqual("modifier duplicate rejected", false, data.AddModifier(modifiable, new DataModifier(ModifierType.Additive, 1f, id: "add")));
        AssertEqual("modifier remove", true, data.RemoveModifier(modifiable, "mul"));
        AssertEqual("modifier remove result", 16f, data.Get(modifiable));
    }

    static void TestRuntimeTypedDataComputedCache()
    {
        var baseKey = DataKey.Create<float>(
            "Test.Computed.Base",
            defaultValue: 2f);
        var calls = 0;
        var computedKey = DataKey.Create<float>(
            "Test.Computed.Double",
            defaultValue: 0f,
            dependencies: [baseKey],
            compute: data =>
            {
                calls++;
                return data.Get(baseKey) * 2f;
            });
        var catalog = DataCatalog.CreateBuilder("test-runtime-computed")
            .Add(baseKey)
            .Add(computedKey)
            .Build();
        var data = new Data(catalog);

        AssertEqual("computed first read", 4f, data.Get(computedKey));
        AssertEqual("computed first calls", 1, calls);
        AssertEqual("computed cached read", 4f, data.Get(computedKey));
        AssertEqual("computed cached calls", 1, calls);

        data.Set(baseKey, 3f);
        AssertEqual("computed dirty read", 6f, data.Get(computedKey));
        AssertEqual("computed dirty calls", 2, calls);

        data.Remove(baseKey);
        AssertEqual("computed remove clears cache", 4f, data.Get(computedKey));
        AssertEqual("computed remove calls", 3, calls);
        AssertEqual("computed has", true, data.Has(computedKey));
    }

    static void TestRuntimeDataSnapshotAppliesDataAndResources()
    {
        const string json = """
        {
          "schemaVersion": 2,
          "generatedAtUtc": "2026-05-05T00:00:00Z",
          "manifest": {
            "schemaVersion": 2,
            "generatedAtUtc": "2026-05-05T00:00:00Z",
            "profile": "test",
            "catalogId": "framework",
            "enabledCapabilities": ["Damage", "Movement", "Attack", "Ability"],
            "descriptorCount": 5,
            "recordCount": 1,
            "resourceCount": 1,
            "validation": { "warningCount": 0, "errorCount": 0 }
          },
          "descriptors": [
            { "stableKey": "Damage.MaxHp", "ownerCapability": "Damage", "ownerSkill": "damage-system", "valueType": "float", "defaultValue": "0", "displayName": "Max HP", "category": "Damage.Config", "supportsModifiers": true },
            { "stableKey": "Damage.CurrentHp", "ownerCapability": "Damage", "ownerSkill": "damage-system", "valueType": "float", "defaultValue": "0", "displayName": "Current HP", "category": "Damage.Runtime" },
            { "stableKey": "Movement.MoveSpeed", "ownerCapability": "Movement", "ownerSkill": "movement-system", "valueType": "float", "defaultValue": "0", "displayName": "Move Speed", "category": "Movement.Config", "supportsModifiers": true },
            { "stableKey": "Attack.Damage", "ownerCapability": "Attack", "ownerSkill": "attack-system", "valueType": "float", "defaultValue": "0", "displayName": "Attack Damage", "category": "Attack.Basic", "supportsModifiers": true },
            { "stableKey": "Ability.TriggerMode", "ownerCapability": "Ability", "ownerSkill": "ability-system", "valueType": "string", "defaultValue": "None", "displayName": "Trigger Mode", "category": "Ability.Basic" }
          ],
          "records": [
            {
              "table": "unit.enemy",
              "id": "yuren",
              "name": "鱼人",
              "fields": {
                "Damage.MaxHp": { "type": "float", "value": 150 },
                "Damage.CurrentHp": { "type": "float", "value": 150 },
                "Movement.MoveSpeed": { "type": "float", "value": 150 },
                "Attack.Damage": { "type": "float", "value": 6 },
                "Ability.TriggerMode": { "type": "string", "value": "Manual" }
              }
            }
          ],
          "resources": [
            { "category": "Entity", "key": "EnemyEntity", "path": "res://Scenes/Enemy.tscn" }
          ]
        }
        """;

        ResourceCatalog.Clear();
        var snapshot = RuntimeDataSnapshot.FromJson(json);
        AssertEqual("snapshot schema", 2, snapshot.SchemaVersion);
        AssertEqual("snapshot descriptor count", 5, snapshot.Descriptors.Count);
        AssertEqual("snapshot find record", true, snapshot.TryFindRecord("unit.enemy", "鱼人", out var record));

        var data = new Data();
        AssertEqual("snapshot applied count", 5, snapshot.ApplyRecord(data, record));
        AssertEqual("snapshot max hp", 150f, data.Get<float>(DamageDataKeys.MaxHp));
        AssertEqual("snapshot move speed", 150f, data.Get<float>(MovementDataKeys.MoveSpeed));
        AssertEqual("snapshot attack damage", 6f, data.Get<float>(AttackDataKeys.Damage));
        AssertEqual("snapshot enum string", AbilityTriggerMode.Manual, data.Get<AbilityTriggerMode>(AbilityDataKeys.TriggerMode));

        AssertEqual("snapshot resources", 1, snapshot.RegisterResources());
        AssertEqual("snapshot resource path", "res://Scenes/Enemy.tscn", ResourceManagement.GetPath("EnemyEntity", ResourceCategory.Entity));
        ResourceCatalog.Clear();
    }

    static void TestRuntimeDataSnapshotRejectsInvalidFields()
    {
        var wrongType = RuntimeDataSnapshot.FromJson(BuildSingleFieldSnapshot(
            "Damage.MaxHp",
            "float",
            "\"not-a-number\"",
            "Damage.MaxHp",
            "float",
            "0"));
        AssertEqual(
            "snapshot wrong type code",
            "snapshot.wrong_type",
            wrongType.ApplyRecordWithReport(new Data(), wrongType.Records[0]).Errors[0].Code);

        var unknownKey = RuntimeDataSnapshot.FromJson(BuildSingleFieldSnapshot(
            "Unknown.Key",
            "float",
            "1",
            "Unknown.Key",
            "float",
            "0"));
        AssertEqual(
            "snapshot unknown key code",
            "snapshot.unknown_key",
            unknownKey.ApplyRecordWithReport(new Data(), unknownKey.Records[0]).Errors[0].Code);

        var defaultDrift = RuntimeDataSnapshot.FromJson(BuildSingleFieldSnapshot(
            "Damage.MaxHp",
            "float",
            "1",
            "Damage.MaxHp",
            "float",
            "1"));
        AssertEqual(
            "snapshot default drift code",
            "snapshot.default_drift",
            defaultDrift.ApplyRecordWithReport(new Data(), defaultDrift.Records[0]).Errors[0].Code);

        var missingDescriptor = RuntimeDataSnapshot.FromJson(BuildMissingDescriptorSnapshot());
        AssertEqual(
            "snapshot missing descriptor code",
            "snapshot.missing_descriptor",
            missingDescriptor.ApplyRecordWithReport(new Data(), missingDescriptor.Records[0]).Errors[0].Code);
    }

    static void TestRuntimeDataSnapshotRejectsDisabledResources()
    {
        const string json = """
        {
          "schemaVersion": 2,
          "generatedAtUtc": "2026-05-05T00:00:00Z",
          "manifest": {
            "schemaVersion": 2,
            "generatedAtUtc": "2026-05-05T00:00:00Z",
            "profile": "test",
            "catalogId": "framework",
            "enabledCapabilities": ["Damage"],
            "descriptorCount": 0,
            "recordCount": 0,
            "resourceCount": 1,
            "validation": { "warningCount": 0, "errorCount": 0 }
          },
          "descriptors": [],
          "records": [],
          "resources": [
            { "category": "Entity", "key": "DisabledProjectile", "path": "res://Projectiles/Disabled.tscn", "ownerCapability": "Projectile", "legacyStatus": "active" }
          ]
        }
        """;

        var snapshot = RuntimeDataSnapshot.FromJson(json);
        AssertEqual(
            "snapshot disabled resource code",
            "snapshot.resource_disabled_capability",
            snapshot.RegisterResourcesWithReport().Errors[0].Code);
    }

    static string BuildSingleFieldSnapshot(
        string fieldKey,
        string fieldType,
        string fieldValueJson,
        string descriptorKey,
        string descriptorType,
        string descriptorDefault)
    {
        return $$"""
        {
          "schemaVersion": 2,
          "generatedAtUtc": "2026-05-05T00:00:00Z",
          "manifest": {
            "schemaVersion": 2,
            "generatedAtUtc": "2026-05-05T00:00:00Z",
            "profile": "test",
            "catalogId": "framework",
            "enabledCapabilities": ["Damage"],
            "descriptorCount": 1,
            "recordCount": 1,
            "resourceCount": 0,
            "validation": { "warningCount": 0, "errorCount": 0 }
          },
          "descriptors": [
            { "stableKey": "{{descriptorKey}}", "ownerCapability": "Damage", "ownerSkill": "damage-system", "valueType": "{{descriptorType}}", "defaultValue": "{{descriptorDefault}}", "displayName": "{{descriptorKey}}", "category": "Damage.Config" }
          ],
          "records": [
            {
              "table": "unit.enemy",
              "id": "probe",
              "name": "Probe",
              "fields": {
                "{{fieldKey}}": { "type": "{{fieldType}}", "value": {{fieldValueJson}} }
              }
            }
          ],
          "resources": []
        }
        """;
    }

    static string BuildMissingDescriptorSnapshot()
    {
        return """
        {
          "schemaVersion": 2,
          "generatedAtUtc": "2026-05-05T00:00:00Z",
          "manifest": {
            "schemaVersion": 2,
            "generatedAtUtc": "2026-05-05T00:00:00Z",
            "profile": "test",
            "catalogId": "framework",
            "enabledCapabilities": ["Damage"],
            "descriptorCount": 0,
            "recordCount": 1,
            "resourceCount": 0,
            "validation": { "warningCount": 0, "errorCount": 0 }
          },
          "descriptors": [],
          "records": [
            {
              "table": "unit.enemy",
              "id": "probe",
              "name": "Probe",
              "fields": {
                "Damage.MaxHp": { "type": "float", "value": 1 }
              }
            }
          ],
          "resources": []
        }
        """;
    }
}
