using SlimeAI.GameOS.Capabilities.AI;
using SlimeAI.GameOS.Capabilities.Ability;
using SlimeAI.GameOS.Capabilities.Attack;
using SlimeAI.GameOS.Capabilities.Collision;
using SlimeAI.GameOS.Capabilities.Damage;
using SlimeAI.GameOS.Capabilities.Movement;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Timer;
using SlimeAI.GameOS.Runtime.World;
using AttackEvents = SlimeAI.GameOS.Capabilities.Attack.Events;
using static TestAssert;

internal partial class Program
{
    static void TestAIServiceMovesTowardTarget()
    {
        EntityManager.Clear();
        var agent = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-agent") });
        var target = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-target") });
        agent.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
        target.Data.Set(MovementDataKeys.Position, new Vector2Value(3f, 4f));
        agent.Data.Set<EntityId?>(AIDataKeys.TargetEntity, target.EntityId);

        var context = new AIContext { Entity = agent, Delta = 0.1f };
        var action = new MoveToTargetAction(0.75f);
        var result = new AIService().Tick(context, action);

        AssertEqual("ai move success", AIState.Success, result);
        AssertEqual("ai move direction", new Vector2Value(0.6f, 0.8f), agent.Data.Get<Vector2Value>(MovementDataKeys.AIMoveDirection));
        AssertNear("ai move speed multiplier", 0.75f, agent.Data.Get<float>(MovementDataKeys.AIMoveSpeedMultiplier));

        agent.Data.Set(AIDataKeys.IsEnabled, false);
        agent.Data.Set(MovementDataKeys.AIMoveDirection, new Vector2Value(1f, 0f));
        var disabled = new AIService().Tick(context, action);
        AssertEqual("ai disabled fails", AIState.Failure, disabled);
        AssertEqual("ai disabled clears movement", Vector2Value.Zero, agent.Data.Get<Vector2Value>(MovementDataKeys.AIMoveDirection));

        EntityManager.Clear();
    }

    static void TestAIServiceFindsNearestTarget()
    {
        EntityManager.Clear();
        var agent = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-query-agent") });
        agent.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
        agent.Data.Set(CollisionDataKeys.Team, 1);

        var sameTeam = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-query-same-team") });
        sameTeam.Data.Set(MovementDataKeys.Position, new Vector2Value(1f, 0f));
        sameTeam.Data.Set(CollisionDataKeys.Team, 1);

        var farEnemy = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-query-far-enemy") });
        farEnemy.Data.Set(MovementDataKeys.Position, new Vector2Value(8f, 0f));
        farEnemy.Data.Set(CollisionDataKeys.Team, 2);

        var nearEnemy = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-query-near-enemy") });
        nearEnemy.Data.Set(MovementDataKeys.Position, new Vector2Value(3f, 0f));
        nearEnemy.Data.Set(CollisionDataKeys.Team, 2);

        var deadEnemy = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-query-dead-enemy") });
        deadEnemy.Data.Set(MovementDataKeys.Position, new Vector2Value(2f, 0f));
        deadEnemy.Data.Set(CollisionDataKeys.Team, 2);
        deadEnemy.Data.Set(DamageDataKeys.IsDead, true);

        var context = new AIContext { Entity = agent, Delta = 0.1f };
        var root = new SequenceNode("Find And Move")
            .Add(new FindNearestTargetAction(5f))
            .Add(new MoveToTargetAction());

        var state = new AIService().Tick(context, root);

        AssertEqual("ai query success", AIState.Success, state);
        AssertEqual("ai query target", (EntityId?)nearEnemy.EntityId, agent.Data.Get<EntityId?>(AIDataKeys.TargetEntity));
        AssertEqual("ai query target position", new Vector2Value(3f, 0f), agent.Data.Get<Vector2Value>(AIDataKeys.TargetPosition));
        AssertEqual("ai query has target position", true, agent.Data.Get<bool>(AIDataKeys.HasTargetPosition));
        AssertEqual("ai query move direction", new Vector2Value(1f, 0f), agent.Data.Get<Vector2Value>(MovementDataKeys.AIMoveDirection));

        nearEnemy.Data.Set(DamageDataKeys.IsDead, true);
        var failed = new AIService().Tick(context, new FindNearestTargetAction(5f));
        AssertEqual("ai query no target", AIState.Failure, failed);
        AssertEqual("ai query cleared target", false, agent.Data.Has(AIDataKeys.TargetEntity));
        AssertEqual("ai query cleared target position flag", false, agent.Data.Get<bool>(AIDataKeys.HasTargetPosition));

        EntityManager.Clear();
    }

    static void TestAIServicePatrolAction()
    {
        EntityManager.Clear();
        var agent = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-patrol-agent") });
        agent.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
        agent.Data.Set(AIDataKeys.PatrolCenter, Vector2Value.Zero);
        agent.Data.Set(AIDataKeys.PatrolRadius, 10f);
        agent.Data.Set(AIDataKeys.PatrolWaitTime, 0.5f);

        var context = new AIContext { Entity = agent, Delta = 0.1f };
        var action = new PatrolAction(reachDistance: 0.25f, speedMultiplier: 0.5f);
        var first = new AIService().Tick(context, action);

        AssertEqual("ai patrol running", AIState.Running, first);
        AssertEqual("ai patrol target right", new Vector2Value(10f, 0f), agent.Data.Get<Vector2Value>(AIDataKeys.PatrolTargetPosition));
        AssertEqual("ai patrol has target", true, agent.Data.Get<bool>(AIDataKeys.HasPatrolTargetPosition));
        AssertEqual("ai patrol move right", new Vector2Value(1f, 0f), agent.Data.Get<Vector2Value>(MovementDataKeys.AIMoveDirection));
        AssertNear("ai patrol speed", 0.5f, agent.Data.Get<float>(MovementDataKeys.AIMoveSpeedMultiplier));

        agent.Data.Set(MovementDataKeys.Position, new Vector2Value(10f, 0f));
        var arrived = new AIService().Tick(context, action);
        AssertEqual("ai patrol arrived running", AIState.Running, arrived);
        AssertEqual("ai patrol stops at target", Vector2Value.Zero, agent.Data.Get<Vector2Value>(MovementDataKeys.AIMoveDirection));
        AssertNear("ai patrol wait set", 0.5f, agent.Data.Get<float>(AIDataKeys.PatrolWaitRemaining));
        AssertEqual("ai patrol target cleared", false, agent.Data.Get<bool>(AIDataKeys.HasPatrolTargetPosition));

        context.Delta = 0.25f;
        var waiting = new AIService().Tick(context, action);
        AssertEqual("ai patrol waiting", AIState.Running, waiting);
        AssertNear("ai patrol wait tick", 0.25f, agent.Data.Get<float>(AIDataKeys.PatrolWaitRemaining));
        AssertEqual("ai patrol waiting stopped", Vector2Value.Zero, agent.Data.Get<Vector2Value>(MovementDataKeys.AIMoveDirection));

        context.Delta = 0.25f;
        var next = new AIService().Tick(context, action);
        AssertEqual("ai patrol next running", AIState.Running, next);
        AssertNear("ai patrol wait complete", 0f, agent.Data.Get<float>(AIDataKeys.PatrolWaitRemaining));
        AssertEqual("ai patrol target left", new Vector2Value(-10f, 0f), agent.Data.Get<Vector2Value>(AIDataKeys.PatrolTargetPosition));
        AssertEqual("ai patrol move left", new Vector2Value(-1f, 0f), agent.Data.Get<Vector2Value>(MovementDataKeys.AIMoveDirection));
        AssertNear("ai patrol speed restored", 0.5f, agent.Data.Get<float>(MovementDataKeys.AIMoveSpeedMultiplier));

        EntityManager.Clear();
    }

    static void TestAIBehaviorTreeBuilderMeleeAttackPriority()
    {
        EntityManager.Clear();
        var agent = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-builder-agent") });
        var target = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-builder-target") });
        agent.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
        agent.Data.Set(CollisionDataKeys.Team, 1);
        agent.Data.Set(AIDataKeys.AttackRange, 5f);
        target.Data.Set(MovementDataKeys.Position, new Vector2Value(3f, 4f));
        target.Data.Set(CollisionDataKeys.Team, 2);

        var requested = 0;
        agent.Events.Subscribe<AttackEvents.Requested>(_ => requested++);

        var context = new AIContext { Entity = agent, Delta = 0.1f };
        var root = EnemyBehaviorTreeBuilder.BuildMeleeEnemyTree(targetSearchRange: 20f, defaultAttackRange: 1f);
        var state = new AIService().Tick(context, root);

        AssertEqual("ai builder attack running", AIState.Running, state);
        AssertEqual("ai builder target selected", (EntityId?)target.EntityId, agent.Data.Get<EntityId?>(AIDataKeys.TargetEntity));
        AssertEqual("ai builder attack requested", 1, requested);
        AssertEqual("ai builder attack flag", true, agent.Data.Get<bool>(AIDataKeys.IsAttackRequested));
        AssertEqual("ai builder attack face", new Vector2Value(0.6f, 0.8f), agent.Data.Get<Vector2Value>(MovementDataKeys.AIMoveDirection));
        AssertNear("ai builder attack stopped", 0f, agent.Data.Get<float>(MovementDataKeys.AIMoveSpeedMultiplier));

        EntityManager.Clear();
    }

    static void TestAIBehaviorTreeBuilderPatrolFallback()
    {
        EntityManager.Clear();
        var agent = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-builder-patrol-agent") });
        agent.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
        agent.Data.Set(AIDataKeys.PatrolCenter, Vector2Value.Zero);
        agent.Data.Set(AIDataKeys.PatrolRadius, 6f);
        agent.Data.Set(AIDataKeys.PatrolWaitTime, 0.1f);

        var context = new AIContext { Entity = agent, Delta = 0.1f };
        var root = EnemyBehaviorTreeBuilder.BuildMeleeEnemyTree(
            targetSearchRange: 4f,
            defaultAttackRange: 2f,
            patrolReachDistance: 0.25f,
            patrolSpeedMultiplier: 0.4f);
        var state = new AIService().Tick(context, root);

        AssertEqual("ai builder patrol running", AIState.Running, state);
        AssertEqual("ai builder no attack flag", false, agent.Data.Get<bool>(AIDataKeys.IsAttackRequested));
        AssertEqual("ai builder patrol target", new Vector2Value(6f, 0f), agent.Data.Get<Vector2Value>(AIDataKeys.PatrolTargetPosition));
        AssertEqual("ai builder patrol direction", new Vector2Value(1f, 0f), agent.Data.Get<Vector2Value>(MovementDataKeys.AIMoveDirection));
        AssertNear("ai builder patrol speed", 0.4f, agent.Data.Get<float>(MovementDataKeys.AIMoveSpeedMultiplier));

        EntityManager.Clear();
    }

    static void TestAIServiceRequestsAttackInRange()
    {
        EntityManager.Clear();
        var agent = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-attack-agent") });
        var target = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-attack-target") });
        agent.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
        agent.Data.Set(AIDataKeys.AttackRange, 5f);
        target.Data.Set(MovementDataKeys.Position, new Vector2Value(3f, 4f));
        agent.Data.Set<EntityId?>(AIDataKeys.TargetEntity, target.EntityId);

        var requested = 0;
        AttackEvents.Requested? payload = null;
        agent.Events.Subscribe<AttackEvents.Requested>(data =>
        {
            requested++;
            payload = data;
        });

        var context = new AIContext { Entity = agent, Delta = 0.1f };
        var attackBranch = new SequenceNode("AI Attack")
            .Add(new IsTargetInRangeCondition(AIDataKeys.AttackRange, 0f))
            .Add(new RequestAttackAction());

        var running = new AIService().Tick(context, attackBranch);

        AssertEqual("ai attack running", AIState.Running, running);
        AssertEqual("ai attack event count", 1, requested);
        AssertEqual("ai attack payload attacker", agent, payload?.Attacker);
        AssertEqual("ai attack payload target", target, payload?.Target);
        AssertEqual("ai attack payload position", new Vector2Value(3f, 4f), payload?.TargetPosition);
        AssertEqual("ai attack requested flag", true, agent.Data.Get<bool>(AIDataKeys.IsAttackRequested));
        AssertEqual("ai attack face direction", new Vector2Value(0.6f, 0.8f), agent.Data.Get<Vector2Value>(MovementDataKeys.AIMoveDirection));
        AssertNear("ai attack stops movement", 0f, agent.Data.Get<float>(MovementDataKeys.AIMoveSpeedMultiplier));

        target.Data.Set(MovementDataKeys.Position, new Vector2Value(10f, 0f));
        var outOfRange = new AIService().Tick(context, attackBranch);
        AssertEqual("ai attack out of range fails", AIState.Failure, outOfRange);
        AssertEqual("ai attack out of range no extra event", 1, requested);
        AssertEqual("ai attack out of range clears flag", false, agent.Data.Get<bool>(AIDataKeys.IsAttackRequested));

        EntityManager.Clear();
    }

    static void TestAIServiceAbilityAutoTriggerAction()
    {
        EntityManager.Clear();
        var abilityTimer = new TimerManager("ai-ability-auto-trigger-test-timers");
        var abilityService = new AbilityService(abilityTimer);
        var agent = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-ability-agent") });
        var ability = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-ability") });
        ability.Data.Set(AbilityDataKeys.IsEnabled, true);
        ability.Data.Set(AbilityDataKeys.TriggerMode, AbilityTriggerMode.Periodic);
        ability.Data.Set(AbilityDataKeys.TargetSelection, AbilityTargetSelection.Entity);
        ability.Data.Set(AbilityDataKeys.Cooldown, 1f);
        ability.Data.Set(AbilityDataKeys.Damage, 4f);

        var target = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-ability-target") });
        target.Data.Set(DamageDataKeys.CurrentHp, 20f);

        var context = new AIContext
        {
            Entity = agent,
            Delta = 0f,
            AbilityService = abilityService,
            AbilityContexts =
            [
                new AbilityCastContext
                {
                    Caster = agent,
                    Ability = ability,
                    Targets = new[] { target }
                }
            ]
        };
        var root = new SequenceNode("AI Auto Cast").Add(new TickAbilityAutoTriggersAction());

        var first = new AIService().Tick(context, root);
        AssertEqual("ai ability action success", AIState.Success, first);
        AssertEqual("ai ability reports", 1, context.LastAbilityReports?.Count);
        AssertNear("ai ability damage", 16f, target.Data.Get<float>(DamageDataKeys.CurrentHp));

        context.Delta = 0.5f;
        var blocked = new AIService().Tick(context, root);
        AssertEqual("ai ability action blocked", AIState.Failure, blocked);
        AssertEqual("ai ability blocked reports", 0, context.LastAbilityReports?.Count);
        AssertNear("ai ability cooldown tick", 0.5f, ability.Data.Get<float>(AbilityDataKeys.CooldownRemaining));

        abilityTimer.Clear();
        EntityManager.Clear();
    }

    static void TestAIServicePreparesAbilityAutoTargetContexts()
    {
        EntityManager.Clear();
        var abilityTimer = new TimerManager("ai-ability-auto-target-test-timers");
        var abilityService = new AbilityService(abilityTimer);
        var agent = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-ability-auto-target-agent") });
        agent.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
        agent.Data.Set(CollisionDataKeys.Team, 1);

        var ability = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-ability-auto-target-ability") });
        ability.Data.Set(AbilityDataKeys.IsEnabled, true);
        ability.Data.Set(AbilityDataKeys.TriggerMode, AbilityTriggerMode.Periodic);
        ability.Data.Set(AbilityDataKeys.TargetSelection, AbilityTargetSelection.Entity);
        ability.Data.Set(AbilityDataKeys.AutoTargetRange, 8f);
        ability.Data.Set(AbilityDataKeys.Damage, 4f);

        var sameTeam = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-ability-auto-target-same-team") });
        sameTeam.Data.Set(MovementDataKeys.Position, new Vector2Value(1f, 0f));
        sameTeam.Data.Set(CollisionDataKeys.Team, 1);
        sameTeam.Data.Set(DamageDataKeys.CurrentHp, 20f);

        var target = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ai-ability-auto-target-enemy") });
        target.Data.Set(MovementDataKeys.Position, new Vector2Value(4f, 0f));
        target.Data.Set(CollisionDataKeys.Team, 2);
        target.Data.Set(DamageDataKeys.CurrentHp, 20f);

        var context = new AIContext
        {
            Entity = agent,
            Delta = 0f,
            AbilityService = abilityService,
            AutoTargetAbilities = [ability]
        };
        var root = new SequenceNode("AI Auto Target Cast")
            .Add(new PrepareAbilityAutoTargetContextsAction())
            .Add(new TickAbilityAutoTriggersAction());

        var first = new AIService().Tick(context, root);
        AssertEqual("ai ability auto target success", AIState.Success, first);
        AssertEqual("ai ability auto target contexts", 1, context.AbilityContexts?.Count);
        AssertEqual("ai ability auto target selected", target, context.AbilityContexts?[0].Targets?[0]);
        AssertEqual("ai ability auto target reports", 1, context.LastAbilityReports?.Count);
        AssertNear("ai ability auto target damage", 16f, target.Data.Get<float>(DamageDataKeys.CurrentHp));
        AssertNear("ai ability auto target same team hp", 20f, sameTeam.Data.Get<float>(DamageDataKeys.CurrentHp));

        target.Data.Set(MovementDataKeys.Position, new Vector2Value(20f, 0f));
        var missing = new AIService().Tick(context, root);
        AssertEqual("ai ability auto target missing fails", AIState.Failure, missing);
        AssertEqual("ai ability auto target missing contexts", 0, context.AbilityContexts?.Count);

        abilityTimer.Clear();
        EntityManager.Clear();
    }

    static void TestAttackServiceConsumesRequestAndDamages()
    {
        using var world = RuntimeWorld.CreateScoped();
        var timer = new TimerManager("attack-service-instant-test-timers");
        var service = new AttackService(timer, new DamageService());
        var attacker = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("attack-instant-attacker") });
        var target = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("attack-instant-target") });
        attacker.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
        attacker.Data.Set(AttackDataKeys.Damage, 6f);
        attacker.Data.Set(AttackDataKeys.Range, 20f);
        attacker.Data.Set(AttackDataKeys.Interval, 1f);
        target.Data.Set(MovementDataKeys.Position, new Vector2Value(5f, 0f));
        target.Data.Set(DamageDataKeys.CurrentHp, 20f);

        var started = 0;
        var finished = 0;
        var didHit = false;
        attacker.Events.Subscribe<AttackEvents.Started>(_ => started++);
        attacker.Events.Subscribe<AttackEvents.Finished>(data =>
        {
            finished++;
            didHit = data.DidHit;
        });
        service.Register(attacker);

        attacker.Events.Publish(new AttackEvents.Requested(
            attacker,
            target,
            target.Data.Get<Vector2Value>(MovementDataKeys.Position)));

        AssertEqual("attack service started", 1, started);
        AssertEqual("attack service finished", 1, finished);
        AssertEqual("attack service hit", true, didHit);
        AssertNear("attack service damage", 14f, target.Data.Get<float>(DamageDataKeys.CurrentHp));
        AssertEqual("attack service attack tag stats", 1, attacker.Data.Get<int>(DamageDataKeys.TotalHits));
        AssertEqual("attack service state idle", AttackState.Idle, attacker.Data.Get<AttackState>(AttackDataKeys.State));
        AssertNear("attack service cooldown set", 1f, attacker.Data.Get<float>(AttackDataKeys.CooldownRemaining));

        service.Clear();
        timer.Clear();
    }

    static void TestAttackServiceGatesRangeAndCooldown()
    {
        using var world = RuntimeWorld.CreateScoped();
        var timer = new TimerManager("attack-service-gates-test-timers");
        var service = new AttackService(timer, new DamageService());
        var attacker = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("attack-gate-attacker") });
        var target = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("attack-gate-target") });
        attacker.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
        attacker.Data.Set(AttackDataKeys.Damage, 6f);
        attacker.Data.Set(AttackDataKeys.Range, 4f);
        attacker.Data.Set(AttackDataKeys.Interval, 0.5f);
        target.Data.Set(MovementDataKeys.Position, new Vector2Value(5f, 0f));
        target.Data.Set(DamageDataKeys.CurrentHp, 20f);

        var cancelled = 0;
        AttackCancelReason? reason = null;
        attacker.Events.Subscribe<AttackEvents.Cancelled>(data =>
        {
            cancelled++;
            reason = data.Reason;
        });

        var request = new AttackEvents.Requested(attacker, target, target.Data.Get<Vector2Value>(MovementDataKeys.Position));
        var outOfRange = service.TryRequest(request);
        AssertEqual("attack out of range result", AttackTriggerResult.FailOutOfRange, outOfRange.Result);
        AssertEqual("attack out of range cancel event", 1, cancelled);
        AssertEqual("attack out of range reason", AttackCancelReason.TargetOutOfRange, reason);
        AssertNear("attack out of range hp unchanged", 20f, target.Data.Get<float>(DamageDataKeys.CurrentHp));

        attacker.Data.Set(AttackDataKeys.Range, 10f);
        var accepted = service.TryRequest(request);
        AssertEqual("attack accepted", AttackTriggerResult.Success, accepted.Result);
        AssertNear("attack accepted damage", 14f, target.Data.Get<float>(DamageDataKeys.CurrentHp));

        var cooldown = service.TryRequest(request);
        AssertEqual("attack cooldown blocked", AttackTriggerResult.FailCooldown, cooldown.Result);
        timer.Tick(0.5f);
        var afterCooldown = service.TryRequest(request);
        AssertEqual("attack after cooldown accepted", AttackTriggerResult.Success, afterCooldown.Result);

        timer.Clear();
    }

    static void TestAttackServiceWindupAndRecoveryTimers()
    {
        using var world = RuntimeWorld.CreateScoped();
        var timer = new TimerManager("attack-service-windup-test-timers");
        var service = new AttackService(timer, new DamageService());
        var attacker = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("attack-windup-attacker") });
        var target = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("attack-windup-target") });
        attacker.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
        attacker.Data.Set(AttackDataKeys.Damage, 5f);
        attacker.Data.Set(AttackDataKeys.Range, 10f);
        attacker.Data.Set(AttackDataKeys.Interval, 1f);
        attacker.Data.Set(AttackDataKeys.WindUpTime, 0.25f);
        attacker.Data.Set(AttackDataKeys.RecoveryTime, 0.25f);
        target.Data.Set(MovementDataKeys.Position, new Vector2Value(3f, 0f));
        target.Data.Set(DamageDataKeys.CurrentHp, 20f);

        var finished = 0;
        attacker.Events.Subscribe<AttackEvents.Finished>(_ => finished++);
        var request = new AttackEvents.Requested(attacker, target, target.Data.Get<Vector2Value>(MovementDataKeys.Position));
        var accepted = service.TryRequest(request);

        AssertEqual("attack windup accepted", AttackTriggerResult.Success, accepted.Result);
        AssertEqual("attack windup state", AttackState.WindUp, attacker.Data.Get<AttackState>(AttackDataKeys.State));
        AssertEqual("attack windup active", true, attacker.Data.Get<bool>(AttackDataKeys.IsAttacking));
        AssertNear("attack windup before damage", 20f, target.Data.Get<float>(DamageDataKeys.CurrentHp));

        timer.Tick(0.25f);
        AssertEqual("attack windup finished count", 1, finished);
        AssertNear("attack windup damage", 15f, target.Data.Get<float>(DamageDataKeys.CurrentHp));
        AssertEqual("attack recovery state", AttackState.Recovery, attacker.Data.Get<AttackState>(AttackDataKeys.State));

        timer.Tick(0.25f);
        AssertEqual("attack recovery idle", AttackState.Idle, attacker.Data.Get<AttackState>(AttackDataKeys.State));
        AssertEqual("attack recovery inactive", false, attacker.Data.Get<bool>(AttackDataKeys.IsAttacking));
        AssertEqual("attack cooldown still blocks", AttackTriggerResult.FailCooldown, service.TryRequest(request).Result);

        timer.Tick(0.5f);
        AssertEqual("attack cooldown complete", AttackTriggerResult.Success, service.TryRequest(request).Result);

        timer.Clear();
    }
}
