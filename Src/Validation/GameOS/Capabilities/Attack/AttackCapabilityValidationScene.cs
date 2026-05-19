using Godot;
using SlimeAI.GameOS.Capabilities.Attack;
using SlimeAI.GameOS.Capabilities.Attack.Events;
using SlimeAI.GameOS.Capabilities.Damage;
using SlimeAI.GameOS.Capabilities.Movement;
using SlimeAI.GameOS.Observation;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Timer;

namespace SlimeAI.SceneTests.GameOS.Capabilities.Attack;

public partial class AttackCapabilityValidationScene : Node
{
    private const string ScenePath = "res://SlimeAI/Src/Validation/GameOS/Capabilities/Attack/AttackCapabilityValidation.tscn";
    private const string PassMarker = "GameOS Capability Attack validation PASS";
    private const string FailMarker = "GameOS Capability Attack validation FAIL";

    public override void _Ready()
    {
        CapabilityValidationSupport.Run(
            this,
            ScenePath,
            "AttackCapabilityValidation",
            "GameOS/Capabilities/Attack",
            "attack-capability-validation.json",
            PassMarker,
            FailMarker,
            new[] { "SlimeAI.GameOS.Capabilities.Attack", "SlimeAI.GameOS.Capabilities.Damage", "SlimeAI.GameOS.Runtime.Timer" },
            new[] { "No Godot animation node or game-side unit visual is required." },
            new[] { "attacker and target entities", "attack request event", "TimerManager", "injected DamageService" },
            new[]
            {
                "AttackService consumes Requested events after Register",
                "range gate cancels out-of-range requests",
                "windup, recovery and cooldown mutate Attack state deterministically",
                "injected DamageService controls the damage path"
            },
            new[] { $"stdout contains {PassMarker}", "artifact status is pass", "failureReasons is empty" },
            new[] { $"stdout contains {FailMarker}", "any Attack check fails", "artifact standard-answer fields are missing" },
            validation =>
            {
                validation.Check("request_consumption_and_damage", "AttackRequest", ValidateRequestConsumptionAndDamage);
                validation.Check("range_gate_cancels_request", "AttackGate", ValidateRangeGateCancelsRequest);
                validation.Check("windup_recovery_cooldown", "AttackTiming", ValidateWindupRecoveryCooldown);
                validation.Check("injected_damage_path", "AttackDamage", ValidateInjectedDamagePath);
            });
    }

    private static CheckResult ValidateRequestConsumptionAndDamage()
    {
        EntityManager.Clear();
        var timer = new TimerManager("attack-scene-request");
        var service = new AttackService(timer, new DamageService());
        var attacker = CapabilityValidationSupport.Spawn("attack-scene-attacker");
        var target = CapabilityValidationSupport.Spawn("attack-scene-target");
        attacker.Data.Set(AttackDataKeys.Damage, 5f);
        attacker.Data.Set(AttackDataKeys.Range, 20f);
        attacker.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
        target.Data.Set(MovementDataKeys.Position, new Vector2Value(5f, 0f));
        target.Data.Set(DamageDataKeys.CurrentHp, 12f);
        var started = 0;
        var finished = 0;
        using var startedToken = attacker.Events.Subscribe<Started>(_ => started++);
        using var finishedToken = attacker.Events.Subscribe<Finished>(data => { if (data.DidHit) finished++; });

        service.Register(attacker);
        attacker.Events.Publish(new Requested(attacker, target, new Vector2Value(5f, 0f)));
        var success = started == 1
            && finished == 1
            && CapabilityValidationSupport.Approximately(target.Data.Get<float>(DamageDataKeys.CurrentHp, 0f), 7f);

        service.Clear();
        timer.Clear();
        return CheckResult.From(success, success ? "attack request consumed and damaged target" : "attack request consumption mismatch", CapabilityValidationSupport.Details(
            ("started", started),
            ("finished", finished),
            ("targetHp", target.Data.Get<float>(DamageDataKeys.CurrentHp, 0f))));
    }

    private static CheckResult ValidateRangeGateCancelsRequest()
    {
        EntityManager.Clear();
        var service = new AttackService(new TimerManager("attack-scene-range"), new DamageService());
        var attacker = CapabilityValidationSupport.Spawn("attack-scene-range-attacker");
        var target = CapabilityValidationSupport.Spawn("attack-scene-range-target");
        attacker.Data.Set(AttackDataKeys.Range, 3f);
        attacker.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
        target.Data.Set(MovementDataKeys.Position, new Vector2Value(10f, 0f));
        target.Data.Set(DamageDataKeys.CurrentHp, 8f);
        AttackCancelReason? reason = null;
        using var token = attacker.Events.Subscribe<Cancelled>(data => reason = data.Reason);

        var report = service.TryRequest(new Requested(attacker, target, new Vector2Value(10f, 0f)));
        var success = report.Result == AttackTriggerResult.FailOutOfRange
            && reason == AttackCancelReason.TargetOutOfRange
            && CapabilityValidationSupport.Approximately(target.Data.Get<float>(DamageDataKeys.CurrentHp, 0f), 8f);

        return CheckResult.From(success, success ? "range gate cancelled out-of-range request" : "range gate mismatch", CapabilityValidationSupport.Details(
            ("result", report.Result.ToString()),
            ("reason", reason?.ToString()),
            ("targetHp", target.Data.Get<float>(DamageDataKeys.CurrentHp, 0f))));
    }

    private static CheckResult ValidateWindupRecoveryCooldown()
    {
        EntityManager.Clear();
        var timer = new TimerManager("attack-scene-timing");
        var service = new AttackService(timer, new DamageService());
        var attacker = CapabilityValidationSupport.Spawn("attack-scene-timing-attacker");
        var target = CapabilityValidationSupport.Spawn("attack-scene-timing-target");
        attacker.Data.Set(AttackDataKeys.Damage, 2f);
        attacker.Data.Set(AttackDataKeys.Range, 20f);
        attacker.Data.Set(AttackDataKeys.Interval, 0.3f);
        attacker.Data.Set(AttackDataKeys.WindUpTime, 0.1f);
        attacker.Data.Set(AttackDataKeys.RecoveryTime, 0.1f);
        attacker.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
        target.Data.Set(MovementDataKeys.Position, new Vector2Value(2f, 0f));
        target.Data.Set(DamageDataKeys.CurrentHp, 10f);

        var report = service.TryRequest(new Requested(attacker, target, new Vector2Value(2f, 0f)));
        var stateAfterRequest = attacker.Data.Get<AttackState>(AttackDataKeys.State, AttackState.Idle);
        timer.Tick(0.1f);
        var stateAfterWindup = attacker.Data.Get<AttackState>(AttackDataKeys.State, AttackState.Idle);
        timer.Tick(0.1f);
        var stateAfterRecovery = attacker.Data.Get<AttackState>(AttackDataKeys.State, AttackState.WindUp);
        timer.Tick(0.2f);
        var cooldown = attacker.Data.Get<float>(AttackDataKeys.CooldownRemaining, -1f);
        var success = report.Result == AttackTriggerResult.Success
            && stateAfterRequest == AttackState.WindUp
            && stateAfterWindup == AttackState.Recovery
            && stateAfterRecovery == AttackState.Idle
            && CapabilityValidationSupport.Approximately(cooldown, 0f);

        timer.Clear();
        return CheckResult.From(success, success ? "windup recovery and cooldown advanced" : "attack timing mismatch", CapabilityValidationSupport.Details(
            ("result", report.Result.ToString()),
            ("stateAfterRequest", stateAfterRequest.ToString()),
            ("stateAfterWindup", stateAfterWindup.ToString()),
            ("stateAfterRecovery", stateAfterRecovery.ToString()),
            ("cooldown", cooldown)));
    }

    private static CheckResult ValidateInjectedDamagePath()
    {
        EntityManager.Clear();
        var damageService = new DamageService();
        damageService.RegisterProcessor(new BlockingAttackDamageProcessor());
        var service = new AttackService(new TimerManager("attack-scene-injected"), damageService);
        var attacker = CapabilityValidationSupport.Spawn("attack-scene-injected-attacker");
        var target = CapabilityValidationSupport.Spawn("attack-scene-injected-target");
        attacker.Data.Set(AttackDataKeys.Damage, 9f);
        attacker.Data.Set(AttackDataKeys.Range, 20f);
        attacker.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
        target.Data.Set(MovementDataKeys.Position, new Vector2Value(2f, 0f));
        target.Data.Set(DamageDataKeys.CurrentHp, 10f);

        var report = service.TryRequest(new Requested(attacker, target, new Vector2Value(2f, 0f)));
        var success = report.Result == AttackTriggerResult.Success
            && CapabilityValidationSupport.Approximately(target.Data.Get<float>(DamageDataKeys.CurrentHp, 0f), 10f);

        return CheckResult.From(success, success ? "injected DamageService blocked damage path" : "injected damage path mismatch", CapabilityValidationSupport.Details(
            ("result", report.Result.ToString()),
            ("targetHp", target.Data.Get<float>(DamageDataKeys.CurrentHp, 0f))));
    }

    private sealed class BlockingAttackDamageProcessor : IDamageProcessor
    {
        public int Priority => DamageProcessorPriority.Base - 10;

        public void Process(DamageInfo info)
        {
            info.IsBlocked = true;
            info.IsEnd = true;
            info.AddLog("scene blocked injected attack damage");
        }
    }
}
