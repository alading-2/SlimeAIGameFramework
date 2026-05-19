using Godot;
using SlimeAI.GameOS.Capabilities.Collision;
using SlimeAI.GameOS.Capabilities.Collision.Events;
using SlimeAI.GameOS.Capabilities.Damage;
using SlimeAI.GameOS.Observation;

namespace SlimeAI.SceneTests.GameOS.Capabilities.Collision;

public partial class CollisionCapabilityValidationScene : Node
{
    private const string ScenePath = "res://SlimeAI/Src/Validation/GameOS/Capabilities/Collision/CollisionCapabilityValidation.tscn";
    private const string PassMarker = "GameOS Capability Collision validation PASS";
    private const string FailMarker = "GameOS Capability Collision validation FAIL";

    public override void _Ready()
    {
        CapabilityValidationSupport.Run(
            this,
            ScenePath,
            "CollisionCapabilityValidation",
            "GameOS/Capabilities/Collision",
            "collision-capability-validation.json",
            PassMarker,
            FailMarker,
            new[] { "SlimeAI.GameOS.Capabilities.Collision", "SlimeAI.GameOS.Capabilities.Damage" },
            new[] { "Contact damage is validated only as a boundary; HP mutation belongs to Damage Capability." },
            new[] { "source and target collision layers", "collision masks", "team ids", "collision event subscribers" },
            new[]
            {
                "layer/mask and team filters decide whether collision can emit",
                "entered and exited events carry CollisionContact payloads",
                "CollisionSystem does not directly mutate Damage.CurrentHp"
            },
            new[] { $"stdout contains {PassMarker}", "artifact status is pass", "failureReasons is empty" },
            new[] { $"stdout contains {FailMarker}", "any Collision check fails", "artifact standard-answer fields are missing" },
            validation =>
            {
                validation.Check("layer_mask_team_filtering", "CollisionFilter", ValidateLayerMaskTeamFiltering);
                validation.Check("entered_exited_payloads", "CollisionEvents", ValidateEnteredExitedPayloads);
                validation.Check("contact_damage_boundary_no_hp_write", "CollisionDamageBoundary", ValidateContactDamageBoundary);
            });
    }

    private static CheckResult ValidateLayerMaskTeamFiltering()
    {
        var system = new CollisionSystem();
        var source = CapabilityValidationSupport.Spawn("collision-scene-source");
        var target = CapabilityValidationSupport.Spawn("collision-scene-target");
        source.Data.Set(CollisionDataKeys.CollisionMask, 0b0010u);
        source.Data.Set(CollisionDataKeys.CollisionLayer, 0b0001u);
        source.Data.Set(CollisionDataKeys.Team, 1);
        target.Data.Set(CollisionDataKeys.CollisionLayer, 0b0010u);
        target.Data.Set(CollisionDataKeys.Team, 2);

        var policy = new CollisionFilterPolicy(RequireLayerMaskMatch: true);
        var layerHit = system.CanCollide(source, target, policy);
        target.Data.Set(CollisionDataKeys.CollisionLayer, 0b0100u);
        var layerMiss = system.CanCollide(source, target, policy);
        target.Data.Set(CollisionDataKeys.CollisionLayer, 0b0010u);
        target.Data.Set(CollisionDataKeys.Team, 1);
        var sameTeamIgnored = system.CanCollide(source, target, new CollisionFilterPolicy(IgnoreSameTeam: true));
        var success = layerHit && !layerMiss && !sameTeamIgnored;

        return CheckResult.From(success, success ? "layer/mask/team filters passed" : "collision filter mismatch", CapabilityValidationSupport.Details(
            ("layerHit", layerHit),
            ("layerMiss", layerMiss),
            ("sameTeamIgnored", sameTeamIgnored)));
    }

    private static CheckResult ValidateEnteredExitedPayloads()
    {
        var system = new CollisionSystem();
        var source = CapabilityValidationSupport.Spawn("collision-scene-events-source");
        var target = CapabilityValidationSupport.Spawn("collision-scene-events-target");
        source.Data.Set(CollisionDataKeys.CollisionMask, 0b0010u);
        source.Data.Set(CollisionDataKeys.CollisionLayer, 0b0001u);
        target.Data.Set(CollisionDataKeys.CollisionLayer, 0b0010u);
        var entered = 0;
        var exited = 0;
        uint enteredTargetLayer = 0;
        using var enteredToken = source.Events.Subscribe<Entered>(data =>
        {
            entered++;
            enteredTargetLayer = data.Contact.TargetLayer;
        });
        using var exitedToken = source.Events.Subscribe<Exited>(data =>
        {
            if (data.Contact.Source.EntityId == source.EntityId && data.Contact.Target.EntityId == target.EntityId)
            {
                exited++;
            }
        });

        var emitEntered = system.EmitEntered(source, target);
        var emitExited = system.EmitExited(source, target);
        var success = emitEntered && emitExited && entered == 1 && exited == 1 && enteredTargetLayer == 0b0010u;

        return CheckResult.From(success, success ? "entered/exited payloads emitted" : "collision event payload mismatch", CapabilityValidationSupport.Details(
            ("emitEntered", emitEntered),
            ("emitExited", emitExited),
            ("entered", entered),
            ("exited", exited),
            ("enteredTargetLayer", enteredTargetLayer)));
    }

    private static CheckResult ValidateContactDamageBoundary()
    {
        var system = new CollisionSystem();
        var source = CapabilityValidationSupport.Spawn("collision-scene-damage-source");
        var target = CapabilityValidationSupport.Spawn("collision-scene-damage-target");
        source.Data.Set(CollisionDataKeys.CollisionMask, 0b0010u);
        target.Data.Set(CollisionDataKeys.CollisionLayer, 0b0010u);
        source.Data.Set(DamageDataKeys.ContactDamage, 99f);
        target.Data.Set(DamageDataKeys.CurrentHp, 20f);

        var emitted = system.EmitEntered(source, target);
        var hp = target.Data.Get<float>(DamageDataKeys.CurrentHp, 0f);
        var success = emitted && CapabilityValidationSupport.Approximately(hp, 20f);

        return CheckResult.From(success, success ? "collision did not directly mutate HP" : "collision contact damage boundary mismatch", CapabilityValidationSupport.Details(
            ("emitted", emitted),
            ("targetHp", hp)));
    }
}
