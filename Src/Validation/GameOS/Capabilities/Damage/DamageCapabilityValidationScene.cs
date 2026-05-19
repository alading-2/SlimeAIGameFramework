using Godot;
using SlimeAI.GameOS.Capabilities.Damage;
using SlimeAI.GameOS.Capabilities.Damage.Events;
using SlimeAI.GameOS.Observation;

namespace SlimeAI.SceneTests.GameOS.Capabilities.Damage;

public partial class DamageCapabilityValidationScene : Node
{
    private const string ScenePath = "res://SlimeAI/Src/Validation/GameOS/Capabilities/Damage/DamageCapabilityValidation.tscn";
    private const string PassMarker = "GameOS Capability Damage validation PASS";
    private const string FailMarker = "GameOS Capability Damage validation FAIL";

    public override void _Ready()
    {
        CapabilityValidationSupport.Run(
            this,
            ScenePath,
            "DamageCapabilityValidation",
            "GameOS/Capabilities/Damage",
            "damage-capability-validation.json",
            PassMarker,
            FailMarker,
            new[] { "SlimeAI.GameOS.Capabilities.Damage" },
            new[] { "All HP mutation in this scene goes through DamageService or HealService." },
            new[] { "attacker and victim entities", "damage processors", "armor, shield, crit and heal DataKeys", "damage event subscriptions" },
            new[]
            {
                "processors execute by priority",
                "crit, shield and armor affect final damage",
                "heal and killed events are emitted by Damage services",
                "Encounter* statistics are updated and simulation does not write HP"
            },
            new[] { $"stdout contains {PassMarker}", "artifact status is pass", "failureReasons is empty" },
            new[] { $"stdout contains {FailMarker}", "any Damage check fails", "artifact standard-answer fields are missing" },
            validation =>
            {
                validation.Check("processor_order", "DamagePipeline", ValidateProcessorOrder);
                validation.Check("crit_armor_shield_pipeline", "DamagePipeline", ValidateCritArmorShieldPipeline);
                validation.Check("heal_and_killed_events", "DamageEvents", ValidateHealAndKilledEvents);
                validation.Check("encounter_stats_and_simulation_boundary", "DamageStats", ValidateEncounterStatsAndSimulationBoundary);
            });
    }

    private static CheckResult ValidateProcessorOrder()
    {
        var service = new DamageService();
        service.ClearProcessors();
        var order = new System.Collections.Generic.List<string>();
        service.RegisterProcessor(new OrderedDamageProcessor("late", 20, order));
        service.RegisterProcessor(new OrderedDamageProcessor("early", 10, order));
        var victim = CapabilityValidationSupport.Spawn("damage-scene-order-victim");
        victim.Data.Set(DamageDataKeys.CurrentHp, 10f);
        service.Process(new DamageInfo { Victim = victim, Damage = 1f });

        var success = order.Count == 2 && order[0] == "early" && order[1] == "late";
        return CheckResult.From(success, success ? "processors ran by priority" : "processor order mismatch", CapabilityValidationSupport.Details(
            ("order", string.Join(",", order))));
    }

    private static CheckResult ValidateCritArmorShieldPipeline()
    {
        var service = new DamageService();
        var attacker = CapabilityValidationSupport.Spawn("damage-scene-pipeline-attacker");
        var victim = CapabilityValidationSupport.Spawn("damage-scene-pipeline-victim");
        attacker.Data.Set(DamageDataKeys.CritRate, 100f);
        attacker.Data.Set(DamageDataKeys.CritDamage, 200f);
        victim.Data.Set(DamageDataKeys.CurrentHp, 50f);
        victim.Data.Set(DamageDataKeys.Shield, 5f);
        victim.Data.Set(DamageDataKeys.Armor, 10f);

        var result = service.Process(new DamageInfo { Attacker = attacker, Victim = victim, Damage = 10f });
        var hp = victim.Data.Get<float>(DamageDataKeys.CurrentHp, 0f);
        var shield = victim.Data.Get<float>(DamageDataKeys.Shield, -1f);
        var success = result.Applied
            && result.Info.IsCritical
            && CapabilityValidationSupport.Approximately(shield, 0f)
            && result.Info.ShieldDamageAbsorbed > 0f
            && result.Info.ArmorMultiplier < 1f
            && hp < 50f;

        return CheckResult.From(success, success ? "crit armor shield pipeline applied" : "crit/armor/shield mismatch", CapabilityValidationSupport.Details(
            ("applied", result.Applied),
            ("isCritical", result.Info.IsCritical),
            ("shield", shield),
            ("armorMultiplier", result.Info.ArmorMultiplier),
            ("targetHp", hp)));
    }

    private static CheckResult ValidateHealAndKilledEvents()
    {
        var healService = new HealService();
        var damageService = new DamageService(healService);
        var healer = CapabilityValidationSupport.Spawn("damage-scene-healer");
        var victim = CapabilityValidationSupport.Spawn("damage-scene-heal-kill-victim");
        victim.Data.Set(DamageDataKeys.MaxHp, 20f);
        victim.Data.Set(DamageDataKeys.CurrentHp, 5f);
        var healed = 0;
        var killed = 0;
        using var healedToken = victim.Events.Subscribe<Healed>(_ => healed++);
        using var killedToken = victim.Events.Subscribe<Killed>(_ => killed++);

        var heal = healService.Process(new HealInfo { Healer = healer, Target = victim, Amount = 5f });
        var damage = damageService.Process(new DamageInfo { Attacker = healer, Victim = victim, Damage = 20f });
        var success = heal.Applied
            && damage.Applied
            && healed == 1
            && killed == 1
            && victim.Data.Get<bool>(DamageDataKeys.IsDead, false);

        return CheckResult.From(success, success ? "heal and killed events emitted" : "heal/killed event mismatch", CapabilityValidationSupport.Details(
            ("healApplied", heal.Applied),
            ("damageApplied", damage.Applied),
            ("healed", healed),
            ("killed", killed),
            ("isDead", victim.Data.Get<bool>(DamageDataKeys.IsDead, false))));
    }

    private static CheckResult ValidateEncounterStatsAndSimulationBoundary()
    {
        var service = new DamageService();
        var attacker = CapabilityValidationSupport.Spawn("damage-scene-stats-attacker");
        var victim = CapabilityValidationSupport.Spawn("damage-scene-stats-victim");
        attacker.Data.Set(DamageDataKeys.CritRate, 100f);
        victim.Data.Set(DamageDataKeys.CurrentHp, 30f);
        var simulated = service.Process(new DamageInfo { Attacker = attacker, Victim = victim, Damage = 4f, IsSimulation = true });
        var hpAfterSimulation = victim.Data.Get<float>(DamageDataKeys.CurrentHp, 0f);
        var applied = service.Process(new DamageInfo { Attacker = attacker, Victim = victim, Damage = 4f });
        var success = simulated.Applied
            && applied.Applied
            && CapabilityValidationSupport.Approximately(hpAfterSimulation, 30f)
            && attacker.Data.Get<float>(DamageDataKeys.EncounterDamageDealt, 0f) > 0f
            && victim.Data.Get<float>(DamageDataKeys.EncounterDamageTaken, 0f) > 0f
            && attacker.Data.Get<int>(DamageDataKeys.EncounterCriticalHits, 0) >= 1;

        return CheckResult.From(success, success ? "encounter stats and simulation boundary passed" : "encounter stats or simulation boundary mismatch", CapabilityValidationSupport.Details(
            ("hpAfterSimulation", hpAfterSimulation),
            ("encounterDamageDealt", attacker.Data.Get<float>(DamageDataKeys.EncounterDamageDealt, 0f)),
            ("encounterDamageTaken", victim.Data.Get<float>(DamageDataKeys.EncounterDamageTaken, 0f)),
            ("encounterCriticalHits", attacker.Data.Get<int>(DamageDataKeys.EncounterCriticalHits, 0))));
    }

    private sealed class OrderedDamageProcessor : IDamageProcessor
    {
        private readonly string name;
        private readonly System.Collections.Generic.List<string> order;

        public OrderedDamageProcessor(string name, int priority, System.Collections.Generic.List<string> order)
        {
            this.name = name;
            Priority = priority;
            this.order = order;
        }

        public int Priority { get; }

        public void Process(DamageInfo info)
        {
            order.Add(name);
        }
    }
}
