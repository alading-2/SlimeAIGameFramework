using SlimeAI.GameOS.Capabilities.Ability;
using SlimeAI.GameOS.Capabilities.Attack;
using SlimeAI.GameOS.Capabilities.Collision;
using SlimeAI.GameOS.Capabilities.Damage;
using SlimeAI.GameOS.Capabilities.Effect;
using SlimeAI.GameOS.Capabilities.Feature;
using SlimeAI.GameOS.Capabilities.Movement;
using SlimeAI.GameOS.Capabilities.Projectile;
using SlimeAI.GameOS.Runtime.Data;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;
using SlimeAI.GameOS.Runtime.Resource;
using SlimeAI.GameOS.Runtime.Timer;
using SlimeAI.GameOS.Runtime.World;
using AbilityEvents = SlimeAI.GameOS.Capabilities.Ability.Events;
using EffectEvents = SlimeAI.GameOS.Capabilities.Effect.Events;
using static TestAssert;

internal partial class Program
{
    static void TestAbilityServiceInstantDamageCooldownAndCharge()
    {
        using var world = RuntimeWorld.CreateScoped();
        var caster = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-caster") });
        var ability = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-firebolt") });
        ability.Data.Set(AbilityDataKeys.IsEnabled, true);
        ability.Data.Set(AbilityDataKeys.TargetSelection, AbilityTargetSelection.Entity);
        ability.Data.Set(AbilityDataKeys.Damage, 5f);
        ability.Data.Set(AbilityDataKeys.Cooldown, 1f);
        ability.Data.Set(AbilityDataKeys.UsesCharges, true);
        ability.Data.Set(AbilityDataKeys.CurrentCharges, 2);

        var target = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-target") });
        target.Data.Set(DamageDataKeys.CurrentHp, 20f);

        var executedEvents = 0;
        ability.Events.Subscribe<AbilityEvents.Executed>(data =>
        {
            executedEvents++;
            AssertEqual("ability event targets", 1, data.Result.TargetsHit);
        });

        var service = new AbilityService(new TimerManager("ability-test-timers"));
        var first = service.TryTrigger(new AbilityCastContext
        {
            Caster = caster,
            Ability = ability,
            Targets = new[] { target }
        });

        AssertEqual("ability first success", AbilityTriggerResult.Success, first.Result);
        AssertNear("ability damage hp", 15f, target.Data.Get<float>(DamageDataKeys.CurrentHp));
        AssertEqual("ability charge consumed", 1, ability.Data.Get<int>(AbilityDataKeys.CurrentCharges));
        AssertNear("ability cooldown started", 1f, ability.Data.Get<float>(AbilityDataKeys.CooldownRemaining));
        AssertEqual("ability executed events", 1, executedEvents);

        var blocked = service.TryTrigger(new AbilityCastContext
        {
            Caster = caster,
            Ability = ability,
            Targets = new[] { target }
        });
        AssertEqual("ability cooldown blocked", AbilityTriggerResult.FailCooldown, blocked.Result);

        service.TickCooldowns(new[] { ability }, 1f);
        var second = service.TryTrigger(new AbilityCastContext
        {
            Caster = caster,
            Ability = ability,
            Targets = new[] { target }
        });

        AssertEqual("ability second success", AbilityTriggerResult.Success, second.Result);
        AssertNear("ability second damage hp", 10f, target.Data.Get<float>(DamageDataKeys.CurrentHp));
        AssertEqual("ability second charge consumed", 0, ability.Data.Get<int>(AbilityDataKeys.CurrentCharges));

    }

    static void TestAbilityServiceRequiresTarget()
    {
        using var world = RuntimeWorld.CreateScoped();
        var timerManager = new TimerManager("ability-no-target-test-timers");
        var caster = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-no-target-caster") });
        var ability = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-no-target") });
        ability.Data.Set(AbilityDataKeys.IsEnabled, true);
        ability.Data.Set(AbilityDataKeys.TargetSelection, AbilityTargetSelection.Entity);
        ability.Data.Set(AbilityDataKeys.Damage, 5f);

        var failedEvents = 0;
        ability.Events.Subscribe<AbilityEvents.Failed>(data =>
        {
            failedEvents++;
            AssertEqual("ability fail reason", AbilityTriggerResult.FailNoTarget, data.Result);
        });

        var report = new AbilityService(timerManager).TryTrigger(new AbilityCastContext
        {
            Caster = caster,
            Ability = ability
        });

        AssertEqual("ability no target failed", AbilityTriggerResult.FailNoTarget, report.Result);
        AssertEqual("ability failed events", 1, failedEvents);

        timerManager.Clear();
    }

    static void TestAbilityServiceAcceptsPointTarget()
    {
        using var world = RuntimeWorld.CreateScoped();
        var timerManager = new TimerManager("ability-point-test-timers");
        var caster = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-point-caster") });
        var ability = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-point") });
        ability.Data.Set(AbilityDataKeys.IsEnabled, true);
        ability.Data.Set(AbilityDataKeys.TargetSelection, AbilityTargetSelection.Point);

        var missingPoint = new AbilityService(timerManager).TryTrigger(new AbilityCastContext
        {
            Caster = caster,
            Ability = ability
        });
        AssertEqual("ability point missing failed", AbilityTriggerResult.FailNoTarget, missingPoint.Result);

        var report = new AbilityService(timerManager).TryTrigger(new AbilityCastContext
        {
            Caster = caster,
            Ability = ability,
            TargetPosition = new Vector2Value(3f, 4f)
        });
        AssertEqual("ability point success", AbilityTriggerResult.Success, report.Result);

        timerManager.Clear();
    }

    static void TestAbilityServiceAcceptsEntityOrPointTarget()
    {
        using var world = RuntimeWorld.CreateScoped();
        var timerManager = new TimerManager("ability-either-test-timers");
        var caster = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-either-caster") });
        var ability = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-either") });
        ability.Data.Set(AbilityDataKeys.IsEnabled, true);
        ability.Data.Set(AbilityDataKeys.TargetSelection, AbilityTargetSelection.EntityOrPoint);

        var missing = new AbilityService(timerManager).TryTrigger(new AbilityCastContext
        {
            Caster = caster,
            Ability = ability
        });
        AssertEqual("ability either missing failed", AbilityTriggerResult.FailNoTarget, missing.Result);

        var pointReport = new AbilityService(timerManager).TryTrigger(new AbilityCastContext
        {
            Caster = caster,
            Ability = ability,
            TargetPosition = new Vector2Value(5f, 0f)
        });
        AssertEqual("ability either point success", AbilityTriggerResult.Success, pointReport.Result);

        var target = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-either-target") });
        var entityReport = new AbilityService(timerManager).TryTrigger(new AbilityCastContext
        {
            Caster = caster,
            Ability = ability,
            Targets = new[] { target }
        });
        AssertEqual("ability either entity success", AbilityTriggerResult.Success, entityReport.Result);

        timerManager.Clear();
    }

    static void TestAbilityServicePeriodicDamage()
    {
        using var world = RuntimeWorld.CreateScoped();
        var timerManager = new TimerManager("ability-periodic-test-timers");
        var service = new AbilityService(timerManager);
        var caster = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-periodic-caster") });
        var ability = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-periodic") });
        ability.Data.Set(AbilityDataKeys.IsEnabled, true);
        ability.Data.Set(AbilityDataKeys.TargetSelection, AbilityTargetSelection.Entity);
        ability.Data.Set(AbilityDataKeys.Damage, 2f);
        ability.Data.Set(AbilityDataKeys.DamageInterval, 0.5f);
        ability.Data.Set(AbilityDataKeys.DamageRepeatCount, 3);
        ability.Data.Set(AbilityDataKeys.ApplyImmediateDamage, true);

        var target = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-periodic-target") });
        target.Data.Set(DamageDataKeys.CurrentHp, 20f);

        var report = service.TryTrigger(new AbilityCastContext
        {
            Caster = caster,
            Ability = ability,
            Targets = new[] { target },
            DamageType = DamageType.Magical,
            DamageTags = DamageTags.Persistent
        });

        AssertEqual("ability periodic success", AbilityTriggerResult.Success, report.Result);
        AssertEqual("ability periodic has timer", true, report.Executed?.DamageResult?.Timer != null);
        AssertNear("ability periodic immediate", 18f, target.Data.Get<float>(DamageDataKeys.CurrentHp));

        timerManager.Tick(0.5f);
        timerManager.Tick(0.5f);

        AssertNear("ability periodic final hp", 14f, target.Data.Get<float>(DamageDataKeys.CurrentHp));
        timerManager.Clear();
    }

    static void TestAbilityServicePeriodicAutoTrigger()
    {
        using var world = RuntimeWorld.CreateScoped();
        var service = new AbilityService(new TimerManager("ability-auto-trigger-test-timers"));
        var caster = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-auto-caster") });
        var ability = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-auto") });
        ability.Data.Set(AbilityDataKeys.IsEnabled, true);
        ability.Data.Set(AbilityDataKeys.TriggerMode, AbilityTriggerMode.Periodic);
        ability.Data.Set(AbilityDataKeys.TargetSelection, AbilityTargetSelection.Entity);
        ability.Data.Set(AbilityDataKeys.Cooldown, 1f);
        ability.Data.Set(AbilityDataKeys.Damage, 3f);

        var manualAbility = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-auto-manual") });
        manualAbility.Data.Set(AbilityDataKeys.IsEnabled, true);
        manualAbility.Data.Set(AbilityDataKeys.TriggerMode, AbilityTriggerMode.Manual);
        manualAbility.Data.Set(AbilityDataKeys.TargetSelection, AbilityTargetSelection.Entity);
        manualAbility.Data.Set(AbilityDataKeys.Damage, 100f);

        var target = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-auto-target") });
        target.Data.Set(DamageDataKeys.CurrentHp, 20f);

        var context = new AbilityCastContext
        {
            Caster = caster,
            Ability = ability,
            Targets = new[] { target }
        };
        var manualContext = new AbilityCastContext
        {
            Caster = caster,
            Ability = manualAbility,
            Targets = new[] { target }
        };

        var first = service.TickAutoTriggers(new[] { context, manualContext }, 0f);
        AssertEqual("ability auto first count", 1, first.Count);
        AssertEqual("ability auto first success", AbilityTriggerResult.Success, first[0].Result);
        AssertNear("ability auto first damage", 17f, target.Data.Get<float>(DamageDataKeys.CurrentHp));
        AssertNear("ability auto cooldown started", 1f, ability.Data.Get<float>(AbilityDataKeys.CooldownRemaining));

        var blocked = service.TickAutoTriggers(new[] { context }, 0.5f);
        AssertEqual("ability auto blocked count", 0, blocked.Count);
        AssertNear("ability auto cooldown ticked", 0.5f, ability.Data.Get<float>(AbilityDataKeys.CooldownRemaining));
        AssertNear("ability auto blocked damage", 17f, target.Data.Get<float>(DamageDataKeys.CurrentHp));

        var second = service.TickAutoTriggers(new[] { context }, 0.5f);
        AssertEqual("ability auto second count", 1, second.Count);
        AssertNear("ability auto second damage", 14f, target.Data.Get<float>(DamageDataKeys.CurrentHp));

    }

    static void TestAbilityTargetingToolFindsNearestEntity()
    {
        EntityManager.Clear();
        var caster = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-targeting-caster") });
        caster.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
        caster.Data.Set(CollisionDataKeys.Team, 1);

        var ability = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-targeting-ability") });
        ability.Data.Set(AbilityDataKeys.TargetSelection, AbilityTargetSelection.Entity);
        ability.Data.Set(AbilityDataKeys.AutoTargetRange, 5f);
        ability.Data.Set(AbilityDataKeys.AutoTargetMaxTargets, 2);

        var sameTeam = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-targeting-same-team") });
        sameTeam.Data.Set(MovementDataKeys.Position, new Vector2Value(1f, 0f));
        sameTeam.Data.Set(CollisionDataKeys.Team, 1);
        sameTeam.Data.Set(DamageDataKeys.CurrentHp, 20f);

        var deadEnemy = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-targeting-dead-enemy") });
        deadEnemy.Data.Set(MovementDataKeys.Position, new Vector2Value(2f, 0f));
        deadEnemy.Data.Set(CollisionDataKeys.Team, 2);
        deadEnemy.Data.Set(DamageDataKeys.CurrentHp, 20f);
        deadEnemy.Data.Set(DamageDataKeys.IsDead, true);

        var nearEnemy = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-targeting-near-enemy") });
        nearEnemy.Data.Set(MovementDataKeys.Position, new Vector2Value(3f, 0f));
        nearEnemy.Data.Set(CollisionDataKeys.Team, 2);
        nearEnemy.Data.Set(DamageDataKeys.CurrentHp, 20f);

        var farEnemy = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-targeting-far-enemy") });
        farEnemy.Data.Set(MovementDataKeys.Position, new Vector2Value(4f, 0f));
        farEnemy.Data.Set(CollisionDataKeys.Team, 2);
        farEnemy.Data.Set(DamageDataKeys.CurrentHp, 20f);

        var outOfRangeEnemy = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-targeting-out-of-range") });
        outOfRangeEnemy.Data.Set(MovementDataKeys.Position, new Vector2Value(8f, 0f));
        outOfRangeEnemy.Data.Set(CollisionDataKeys.Team, 2);
        outOfRangeEnemy.Data.Set(DamageDataKeys.CurrentHp, 20f);

        var built = AbilityTargetingTool.TryBuildContext(caster, ability, out var context);
        AssertEqual("ability targeting built", true, built);
        AssertEqual("ability targeting target count", 2, context?.Targets?.Count);
        AssertEqual("ability targeting nearest", nearEnemy, context?.Targets?[0]);
        AssertEqual("ability targeting second", farEnemy, context?.Targets?[1]);
        AssertEqual("ability targeting target position", new Vector2Value(3f, 0f), context?.TargetPosition);

        ability.Data.Set(AbilityDataKeys.AutoTargetRange, 2f);
        var failed = AbilityTargetingTool.TryBuildContext(caster, ability, out var missing);
        AssertEqual("ability targeting no target", false, failed);
        AssertEqual("ability targeting missing context", null, missing);

        EntityManager.Clear();
    }

    static void TestEffectToolSpawnsRuntimeEntity()
    {
        EntityManager.Clear();
        WorldEvents.World.Clear();
        var source = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("effect-source") });
        var ability = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("effect-ability") });
        var target = EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId("effect-target") });
        target.Data.Set(MovementDataKeys.Position, new Vector2Value(2f, 3f));

        var spawnedEvents = 0;
        var spawnSub = WorldEvents.World.Subscribe<EffectEvents.Spawned>(data =>
        {
            spawnedEvents++;
            AssertEqual("effect event source", source.EntityId, data.Source.EntityId);
            AssertEqual("effect event target", target.EntityId, data.Target?.EntityId);
        });

        var result = EffectTool.Spawn(new EffectSpawnOptions
        {
            Source = source,
            Ability = ability,
            Target = target,
            EntityId = new EntityId("effect-runtime"),
            ScenePath = "res://Effects/Impact.tscn",
            Name = "Impact",
            AnimationName = "Effect",
            Position = Vector2Value.Zero,
            Duration = 0.75f
        });

        AssertEqual("effect created", true, result.Created);
        AssertEqual("effect registered", result.Effect.EntityId, EntityManager.Get(new EntityId("effect-runtime"))?.EntityId);
        AssertEqual("effect source typed", source.EntityId, result.Effect.Data.Get<EntityId?>(EffectDataKeys.SourceEntity, null) ?? EntityId.Empty);
        AssertEqual("effect target typed", target.EntityId, result.Effect.Data.Get<EntityId?>(EffectDataKeys.TargetEntity, null) ?? EntityId.Empty);
        AssertEqual("spawner spawned-effect-id present", true, source.Data.Get(EffectDataKeys.SpawnedEffectIds).Contains(result.Effect.EntityId));
        AssertEqual("effect lifecycle parent attached", true, LifecycleTree.IsAttached(source.EntityId, result.Effect.EntityId));
        AssertEqual("effect scene path", "res://Effects/Impact.tscn", result.Effect.Data.Get<string>(EffectDataKeys.ScenePath));
        AssertEqual("effect name", "Impact", result.Effect.Data.Get<string>(EffectDataKeys.Name));
        AssertEqual("effect animation name", "Effect", result.Effect.Data.Get<string>(EffectDataKeys.AnimationName));
        AssertEqual("effect target position", new Vector2Value(2f, 3f), result.Effect.Data.Get<Vector2Value>(EffectDataKeys.Position));
        AssertNear("effect duration", 0.75f, result.Effect.Data.Get<float>(EffectDataKeys.Duration));
        AssertEqual("effect events", 1, spawnedEvents);

        spawnSub.Dispose();
        WorldEvents.World.Clear();
        EntityManager.Clear();
    }

    static void TestFeatureServiceGrantsModifiersAndRemoves()
    {
        using var world = RuntimeWorld.CreateScoped();
        var owner = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("feature-owner") });
        var feature = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("feature-damage-bonus") });
        owner.Data.Set(DamageDataKeys.ContactDamage, 5f);

        var service = new FeatureService();
        var definition = new FeatureDefinition
        {
            FeatureId = "feature.damage_bonus",
            Modifiers =
            [
                new FeatureModifierEntry(
                    DamageDataKeys.ContactDamage,
                    ModifierType.Additive,
                    3f)
            ]
        };
        service.Grant(owner, feature, definition);

        AssertNear("feature modifier applied", 8f, owner.Data.Get<float>(DamageDataKeys.ContactDamage));

        service.Remove(owner, feature);
        AssertNear("feature modifier removed", 5f, owner.Data.Get<float>(DamageDataKeys.ContactDamage));

    }

    static void TestFeatureServiceLifecycleHandler()
    {
        using var world = RuntimeWorld.CreateScoped();
        FeatureHandlerRegistry.Clear();
        var handler = new CountingFeatureHandler("feature.lifecycle");
        FeatureHandlerRegistry.Register(handler);

        var owner = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("feature-lifecycle-owner") });
        var feature = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("feature-lifecycle") });
        var service = new FeatureService();
        var definition = new FeatureDefinition
        {
            FeatureId = "feature.lifecycle",
            HandlerId = "feature.lifecycle"
        };

        service.Grant(owner, feature, definition);
        service.Disable(owner, feature);
        service.Enable(owner, feature);
        var context = new FeatureContext { Owner = owner, Feature = feature, Definition = definition, ActivationData = "payload" };
        service.Activate(context);
        service.End(context);
        service.Remove(owner, feature);

        AssertEqual("feature granted", 1, handler.Granted);
        AssertEqual("feature disabled", 1, handler.Disabled);
        AssertEqual("feature enabled", 1, handler.Enabled);
        AssertEqual("feature activated", 1, handler.Activated);
        AssertEqual("feature executed", 1, handler.Executed);
        AssertEqual("feature ended", 1, handler.Ended);
        AssertEqual("feature removed", 1, handler.Removed);
        AssertEqual("feature execute result", "executed:payload", context.ExecuteResult);
        AssertEqual("feature active reset", false, feature.Data.Get<bool>(FeatureDataKeys.IsActive));
        AssertEqual("feature activation count", 1, feature.Data.Get<int>(FeatureDataKeys.ActivationCount));

        FeatureHandlerRegistry.Clear();
    }

    static void TestAbilityServiceInvokesFeatureHandler()
    {
        using var world = RuntimeWorld.CreateScoped();
        var timerManager = new TimerManager("ability-feature-test-timers");
        FeatureHandlerRegistry.Clear();
        var handler = new AbilityFeatureProbeHandler();
        FeatureHandlerRegistry.Register(handler);

        var caster = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-feature-caster") });
        var ability = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-feature") });
        ability.Data.Set(AbilityDataKeys.IsEnabled, true);
        ability.Data.Set(AbilityDataKeys.FeatureHandlerId, handler.FeatureId);
        ability.Data.Set(AbilityDataKeys.Damage, 99f);

        var target = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("ability-feature-target") });
        target.Data.Set(DamageDataKeys.CurrentHp, 20f);

        var report = new AbilityService(timerManager).TryTrigger(new AbilityCastContext
        {
            Caster = caster,
            Ability = ability,
            Targets = new[] { target }
        });

        AssertEqual("ability feature success", AbilityTriggerResult.Success, report.Result);
        AssertEqual("ability feature executed", 1, handler.Executed);
        AssertEqual("ability feature targets", 1, report.Executed?.TargetsHit);
        AssertNear("ability feature avoids fallback damage", 20f, target.Data.Get<float>(DamageDataKeys.CurrentHp));
        AssertEqual("ability feature active reset", false, ability.Data.Get<bool>(FeatureDataKeys.IsActive));

        timerManager.Clear();
        FeatureHandlerRegistry.Clear();
    }

    sealed class CountingFeatureHandler : IFeatureHandler
    {
        public CountingFeatureHandler(string featureId)
        {
            FeatureId = featureId;
        }

        public string FeatureId { get; }
        public int Granted { get; private set; }
        public int Removed { get; private set; }
        public int Enabled { get; private set; }
        public int Disabled { get; private set; }
        public int Activated { get; private set; }
        public int Executed { get; private set; }
        public int Ended { get; private set; }

        public void OnGranted(FeatureContext context) => Granted++;
        public void OnRemoved(FeatureContext context) => Removed++;
        public void OnEnabled(FeatureContext context) => Enabled++;
        public void OnDisabled(FeatureContext context) => Disabled++;
        public void OnActivated(FeatureContext context) => Activated++;

        public object? OnExecute(FeatureContext context)
        {
            Executed++;
            return $"executed:{context.ActivationData}";
        }

        public void OnEnded(FeatureContext context, FeatureEndReason reason) => Ended++;
    }

    sealed class AbilityFeatureProbeHandler : IFeatureHandler
    {
        public string FeatureId => "ability.probe";
        public int Executed { get; private set; }

        public object? OnExecute(FeatureContext context)
        {
            Executed++;
            var cast = context.ActivationData as AbilityCastContext;
            return new AbilityExecutedResult
            {
                TargetsHit = cast?.Targets?.Count ?? 0,
                TotalDamage = 0f
            };
        }
    }
}
