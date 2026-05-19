using System.Collections.Generic;
using Godot;
using SlimeAI.GameOS.Capabilities.Ability;
using SlimeAI.GameOS.Capabilities.AI;
using SlimeAI.GameOS.Capabilities.Attack.Events;
using SlimeAI.GameOS.Capabilities.Collision;
using SlimeAI.GameOS.Capabilities.Damage;
using SlimeAI.GameOS.Capabilities.Movement;
using SlimeAI.GameOS.Observation;
using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.SceneTests.GameOS.Capabilities.AI;

public partial class AICapabilityValidationScene : Node
{
    private const string ScenePath = "res://SlimeAI/Src/Validation/GameOS/Capabilities/AI/AICapabilityValidation.tscn";
    private const string PassMarker = "GameOS Capability AI validation PASS";
    private const string FailMarker = "GameOS Capability AI validation FAIL";

    public override void _Ready()
    {
        CapabilityValidationSupport.Run(
            this,
            ScenePath,
            "AICapabilityValidation",
            "GameOS/Capabilities/AI",
            "ai-capability-validation.json",
            PassMarker,
            FailMarker,
            new[] { "SlimeAI.GameOS.Capabilities.AI", "SlimeAI.GameOS.Capabilities.Ability", "SlimeAI.GameOS.Capabilities.Attack" },
            new[] { "BrotatoLike is only the Godot host project." },
            new[]
            {
                "isolated Runtime default entity set",
                "injected IAITargetQuery candidates",
                "AI entity with patrol, target, attack and auto-target DataKeys"
            },
            new[]
            {
                "FindNearestTargetAction selects nearest valid enemy through injected query",
                "PatrolAction writes deterministic AI movement intent",
                "RequestAttackAction publishes attack request and stops movement speed",
                "PrepareAbilityAutoTargetContextsAction builds AbilityCastContext for auto-target ability"
            },
            new[]
            {
                "all AI capability checks pass",
                $"stdout contains {PassMarker}",
                "artifact status is pass and failureReasons is empty"
            },
            new[]
            {
                $"stdout contains {FailMarker}",
                "any AI capability check fails",
                "artifact is missing standard-answer fields or failureReasons is non-empty"
            },
            validation =>
            {
                validation.Check("injected_target_query_nearest_target", "AISelector", ValidateInjectedTargetQueryNearestTarget);
                validation.Check("deterministic_patrol_writes_movement_intent", "AIPatrol", ValidateDeterministicPatrol);
                validation.Check("attack_request_event_payload", "AIAttack", ValidateAttackRequest);
                validation.Check("ability_auto_target_context_preparation", "AIAbility", ValidateAbilityAutoTargetContext);
            });
    }

    private static CheckResult ValidateInjectedTargetQueryNearestTarget()
    {
        EntityManager.Clear();
        var self = CapabilityValidationSupport.Spawn("ai-scene-self");
        var near = CapabilityValidationSupport.Spawn("ai-scene-near");
        var far = CapabilityValidationSupport.Spawn("ai-scene-far");
        var friend = CapabilityValidationSupport.Spawn("ai-scene-friend");
        self.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
        self.Data.Set(CollisionDataKeys.Team, 1);
        near.Data.Set(MovementDataKeys.Position, new Vector2Value(10f, 0f));
        near.Data.Set(CollisionDataKeys.Team, 2);
        far.Data.Set(MovementDataKeys.Position, new Vector2Value(40f, 0f));
        far.Data.Set(CollisionDataKeys.Team, 2);
        friend.Data.Set(MovementDataKeys.Position, new Vector2Value(1f, 0f));
        friend.Data.Set(CollisionDataKeys.Team, 1);

        var query = new FixedAITargetQuery(new IEntity[] { far, friend, near });
        var state = new FindNearestTargetAction(range: 30f, targetQuery: query).Evaluate(new AIContext { Entity = self, Delta = 0.1f });
        var selected = self.Data.Get<EntityId?>(AIDataKeys.TargetEntity, null);
        var success = state == AIState.Success
            && query.Calls == 1
            && selected == near.EntityId
            && self.Data.Get<bool>(AIDataKeys.HasTargetPosition, false);

        return CheckResult.From(success, success ? "injected query selected nearest enemy" : "AI nearest target selection mismatch", CapabilityValidationSupport.Details(
            ("queryCalls", query.Calls),
            ("selected", selected?.Value),
            ("expected", near.EntityId.Value),
            ("state", state.ToString())));
    }

    private static CheckResult ValidateDeterministicPatrol()
    {
        EntityManager.Clear();
        var self = CapabilityValidationSupport.Spawn("ai-scene-patrol");
        self.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
        self.Data.Set(AIDataKeys.PatrolCenter, Vector2Value.Zero);
        self.Data.Set(AIDataKeys.PatrolRadius, 5f);
        self.Data.Set(AIDataKeys.PatrolDirectionSign, 1);

        var state = new PatrolAction(reachDistance: 0.1f, speedMultiplier: 0.75f).Evaluate(new AIContext { Entity = self, Delta = 0.1f });
        var direction = self.Data.Get<Vector2Value>(MovementDataKeys.AIMoveDirection, Vector2Value.Zero);
        var speed = self.Data.Get<float>(MovementDataKeys.AIMoveSpeedMultiplier, 0f);
        var target = self.Data.Get<Vector2Value>(AIDataKeys.PatrolTargetPosition, Vector2Value.Zero);
        var success = state == AIState.Running
            && CapabilityValidationSupport.Approximately(direction.X, 1f)
            && CapabilityValidationSupport.Approximately(speed, 0.75f)
            && CapabilityValidationSupport.Approximately(target.X, 5f);

        return CheckResult.From(success, success ? "patrol wrote deterministic movement intent" : "AI patrol intent mismatch", CapabilityValidationSupport.Details(
            ("directionX", direction.X),
            ("speedMultiplier", speed),
            ("targetX", target.X),
            ("state", state.ToString())));
    }

    private static CheckResult ValidateAttackRequest()
    {
        EntityManager.Clear();
        var self = CapabilityValidationSupport.Spawn("ai-scene-attacker");
        var target = CapabilityValidationSupport.Spawn("ai-scene-attack-target");
        self.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
        self.Data.Set<EntityId?>(AIDataKeys.TargetEntity, target.EntityId);
        target.Data.Set(MovementDataKeys.Position, new Vector2Value(6f, 0f));
        var requestCount = 0;
        EntityId? requestedTarget = null;
        using var token = self.Events.Subscribe<Requested>(request =>
        {
            requestCount++;
            requestedTarget = request.Target?.EntityId;
        });

        var state = new RequestAttackAction().Evaluate(new AIContext { Entity = self, Delta = 0.1f });
        var success = state == AIState.Running
            && requestCount == 1
            && requestedTarget == target.EntityId
            && self.Data.Get<bool>(AIDataKeys.IsAttackRequested, false)
            && CapabilityValidationSupport.Approximately(self.Data.Get<float>(MovementDataKeys.AIMoveSpeedMultiplier, -1f), 0f);

        return CheckResult.From(success, success ? "attack request payload published" : "AI attack request mismatch", CapabilityValidationSupport.Details(
            ("requestCount", requestCount),
            ("requestedTarget", requestedTarget?.Value),
            ("speedMultiplier", self.Data.Get<float>(MovementDataKeys.AIMoveSpeedMultiplier, -1f)),
            ("state", state.ToString())));
    }

    private static CheckResult ValidateAbilityAutoTargetContext()
    {
        EntityManager.Clear();
        var caster = CapabilityValidationSupport.Spawn("ai-scene-ability-caster");
        var ability = CapabilityValidationSupport.Spawn("ai-scene-ability");
        var target = CapabilityValidationSupport.Spawn("ai-scene-ability-target");
        caster.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
        caster.Data.Set(CollisionDataKeys.Team, 1);
        ability.Data.Set(AbilityDataKeys.TargetSelection, AbilityTargetSelection.Entity);
        ability.Data.Set(AbilityDataKeys.AutoTargetRange, 50f);
        ability.Data.Set(AbilityDataKeys.AutoTargetMaxTargets, 1);
        target.Data.Set(MovementDataKeys.Position, new Vector2Value(8f, 0f));
        target.Data.Set(CollisionDataKeys.Team, 2);
        target.Data.Set(DamageDataKeys.CurrentHp, 10f);

        var context = new AIContext
        {
            Entity = caster,
            Delta = 0.1f,
            AutoTargetAbilities = new[] { ability }
        };
        var state = new PrepareAbilityAutoTargetContextsAction().Evaluate(context);
        var selected = context.AbilityContexts?[0].Targets?[0].EntityId;
        var success = state == AIState.Success
            && context.AbilityContexts?.Count == 1
            && selected == target.EntityId;

        return CheckResult.From(success, success ? "AI prepared ability auto-target context" : "AI ability context mismatch", CapabilityValidationSupport.Details(
            ("contextCount", context.AbilityContexts?.Count),
            ("selectedTarget", selected?.Value),
            ("state", state.ToString())));
    }
}
