using Godot;
using SlimeAI.GameOS.Capabilities.Ability;
using SlimeAI.GameOS.Capabilities.Collision;
using SlimeAI.GameOS.Capabilities.Damage;
using SlimeAI.GameOS.Capabilities.Movement;
using SlimeAI.GameOS.Observation;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Timer;

namespace SlimeAI.SceneTests.GameOS.Capabilities.Ability;

public partial class AbilityCapabilityValidationScene : Node
{
    private const string ScenePath = "res://SlimeAI/Src/Validation/GameOS/Capabilities/Ability/AbilityCapabilityValidation.tscn";
    private const string PassMarker = "GameOS Capability Ability validation PASS";
    private const string FailMarker = "GameOS Capability Ability validation FAIL";

    public override void _Ready()
    {
        CapabilityValidationSupport.Run(
            this,
            ScenePath,
            "AbilityCapabilityValidation",
            "GameOS/Capabilities/Ability",
            "ability-capability-validation.json",
            PassMarker,
            FailMarker,
            new[] { "SlimeAI.GameOS.Capabilities.Ability", "SlimeAI.GameOS.Capabilities.Damage", "SlimeAI.GameOS.Capabilities.Feature" },
            new[] { "No game-side Ability handlers or BrotatoLike assets are required." },
            new[]
            {
                "caster, ability, entity target and point target data",
                "cooldown and charge DataKeys",
                "Periodic trigger context",
                "injected IAbilityTargetQuery"
            },
            new[]
            {
                "explicit entity target applies Ability damage through injected service",
                "point target succeeds without requiring entity targets",
                "cooldown and charge gates are enforced",
                "Periodic auto trigger and injected target query are deterministic"
            },
            new[] { $"stdout contains {PassMarker}", "artifact status is pass", "failureReasons is empty" },
            new[] { $"stdout contains {FailMarker}", "any Ability check fails", "artifact standard-answer fields are missing" },
            validation =>
            {
                validation.Check("explicit_entity_target_damage", "AbilityTrigger", ValidateExplicitEntityTargetDamage);
                validation.Check("point_target_success", "AbilityTrigger", ValidatePointTargetSuccess);
                validation.Check("cooldown_charge_gate", "AbilityGate", ValidateCooldownChargeGate);
                validation.Check("periodic_auto_trigger", "AbilityAutoTrigger", ValidatePeriodicAutoTrigger);
                validation.Check("injected_ability_target_query", "AbilitySelector", ValidateInjectedAbilityTargetQuery);
            });
    }

    private static CheckResult ValidateExplicitEntityTargetDamage()
    {
        EntityManager.Clear();
        var timer = new TimerManager("ability-scene-explicit");
        var service = new AbilityService(timer, damageService: new DamageService());
        var caster = CapabilityValidationSupport.Spawn("ability-scene-caster");
        var ability = CapabilityValidationSupport.Spawn("ability-scene-entity");
        var target = CapabilityValidationSupport.Spawn("ability-scene-target");
        ability.Data.Set(AbilityDataKeys.IsEnabled, true);
        ability.Data.Set(AbilityDataKeys.TargetSelection, AbilityTargetSelection.Entity);
        ability.Data.Set(AbilityDataKeys.Damage, 4f);
        target.Data.Set(DamageDataKeys.CurrentHp, 10f);

        var report = service.TryTrigger(new AbilityCastContext { Caster = caster, Ability = ability, Targets = new[] { target } });
        var hp = target.Data.Get<float>(DamageDataKeys.CurrentHp, 0f);
        var success = report.Result == AbilityTriggerResult.Success
            && report.Executed?.TargetsHit == 1
            && CapabilityValidationSupport.Approximately(hp, 6f);

        return CheckResult.From(success, success ? "explicit entity target applied damage" : "explicit entity target mismatch", CapabilityValidationSupport.Details(
            ("result", report.Result.ToString()),
            ("targetsHit", report.Executed?.TargetsHit),
            ("targetHp", hp)));
    }

    private static CheckResult ValidatePointTargetSuccess()
    {
        EntityManager.Clear();
        var service = new AbilityService(new TimerManager("ability-scene-point"), damageService: new DamageService());
        var caster = CapabilityValidationSupport.Spawn("ability-scene-point-caster");
        var ability = CapabilityValidationSupport.Spawn("ability-scene-point");
        ability.Data.Set(AbilityDataKeys.IsEnabled, true);
        ability.Data.Set(AbilityDataKeys.TargetSelection, AbilityTargetSelection.Point);

        var report = service.TryTrigger(new AbilityCastContext
        {
            Caster = caster,
            Ability = ability,
            TargetPosition = new Vector2Value(3f, 4f)
        });
        var success = report.Result == AbilityTriggerResult.Success && report.Executed?.TargetsHit == 0;

        return CheckResult.From(success, success ? "point target trigger succeeded" : "point target trigger mismatch", CapabilityValidationSupport.Details(
            ("result", report.Result.ToString()),
            ("targetsHit", report.Executed?.TargetsHit)));
    }

    private static CheckResult ValidateCooldownChargeGate()
    {
        EntityManager.Clear();
        var service = new AbilityService(new TimerManager("ability-scene-gate"), damageService: new DamageService());
        var caster = CapabilityValidationSupport.Spawn("ability-scene-gate-caster");
        var ability = CapabilityValidationSupport.Spawn("ability-scene-gate");
        ability.Data.Set(AbilityDataKeys.IsEnabled, true);
        ability.Data.Set(AbilityDataKeys.UsesCharges, true);
        ability.Data.Set(AbilityDataKeys.CurrentCharges, 1);
        ability.Data.Set(AbilityDataKeys.Cooldown, 1f);

        var first = service.TryTrigger(new AbilityCastContext { Caster = caster, Ability = ability });
        var second = service.TryTrigger(new AbilityCastContext { Caster = caster, Ability = ability });
        service.TickCooldowns(new[] { ability }, 1f);
        var third = service.TryTrigger(new AbilityCastContext { Caster = caster, Ability = ability });

        var success = first.Result == AbilityTriggerResult.Success
            && second.Result == AbilityTriggerResult.FailCooldown
            && third.Result == AbilityTriggerResult.FailNoCharge
            && ability.Data.Get<int>(AbilityDataKeys.CurrentCharges, -1) == 0;

        return CheckResult.From(success, success ? "cooldown and charge gates enforced" : "cooldown/charge gate mismatch", CapabilityValidationSupport.Details(
            ("first", first.Result.ToString()),
            ("second", second.Result.ToString()),
            ("third", third.Result.ToString()),
            ("charges", ability.Data.Get<int>(AbilityDataKeys.CurrentCharges, -1))));
    }

    private static CheckResult ValidatePeriodicAutoTrigger()
    {
        EntityManager.Clear();
        var service = new AbilityService(new TimerManager("ability-scene-periodic"), damageService: new DamageService());
        var caster = CapabilityValidationSupport.Spawn("ability-scene-periodic-caster");
        var ability = CapabilityValidationSupport.Spawn("ability-scene-periodic");
        var target = CapabilityValidationSupport.Spawn("ability-scene-periodic-target");
        ability.Data.Set(AbilityDataKeys.IsEnabled, true);
        ability.Data.Set(AbilityDataKeys.TriggerMode, AbilityTriggerMode.Periodic);
        ability.Data.Set(AbilityDataKeys.TargetSelection, AbilityTargetSelection.Entity);
        ability.Data.Set(AbilityDataKeys.Damage, 3f);
        target.Data.Set(DamageDataKeys.CurrentHp, 9f);

        var reports = service.TickAutoTriggers(new[]
        {
            new AbilityCastContext { Caster = caster, Ability = ability, Targets = new[] { target } }
        }, 0.1f);
        var success = reports.Count == 1
            && reports[0].Result == AbilityTriggerResult.Success
            && CapabilityValidationSupport.Approximately(target.Data.Get<float>(DamageDataKeys.CurrentHp, 0f), 6f);

        return CheckResult.From(success, success ? "periodic auto trigger executed" : "periodic auto trigger mismatch", CapabilityValidationSupport.Details(
            ("reportCount", reports.Count),
            ("targetHp", target.Data.Get<float>(DamageDataKeys.CurrentHp, 0f))));
    }

    private static CheckResult ValidateInjectedAbilityTargetQuery()
    {
        EntityManager.Clear();
        var caster = CapabilityValidationSupport.Spawn("ability-scene-query-caster");
        var ability = CapabilityValidationSupport.Spawn("ability-scene-query-ability");
        var near = CapabilityValidationSupport.Spawn("ability-scene-query-near");
        var far = CapabilityValidationSupport.Spawn("ability-scene-query-far");
        caster.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
        caster.Data.Set(CollisionDataKeys.Team, 1);
        ability.Data.Set(AbilityDataKeys.AutoTargetRange, 100f);
        ability.Data.Set(AbilityDataKeys.AutoTargetMaxTargets, 1);
        near.Data.Set(MovementDataKeys.Position, new Vector2Value(5f, 0f));
        near.Data.Set(CollisionDataKeys.Team, 2);
        near.Data.Set(DamageDataKeys.CurrentHp, 10f);
        far.Data.Set(MovementDataKeys.Position, new Vector2Value(30f, 0f));
        far.Data.Set(CollisionDataKeys.Team, 2);
        far.Data.Set(DamageDataKeys.CurrentHp, 10f);

        var query = new FixedAbilityTargetQuery(new IEntity[] { far, near });
        var targets = AbilityTargetingTool.ResolveEntityTargets(caster, ability, targetQuery: query);
        var success = query.Calls == 1 && targets.Count == 1 && targets[0].EntityId == near.EntityId;

        return CheckResult.From(success, success ? "injected Ability target query selected nearest target" : "Ability target query mismatch", CapabilityValidationSupport.Details(
            ("queryCalls", query.Calls),
            ("targetCount", targets.Count),
            ("selected", targets.Count > 0 ? targets[0].EntityId.Value : null)));
    }
}
