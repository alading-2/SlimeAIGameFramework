using System.Collections.Generic;
using System.IO;
using Godot;
using SlimeAI.GameOS.Capabilities.AI;
using SlimeAI.GameOS.Capabilities.Ability;
using SlimeAI.GameOS.Capabilities.Collision;
using SlimeAI.GameOS.Capabilities.Damage;
using SlimeAI.GameOS.Capabilities.Feature;
using SlimeAI.GameOS.Capabilities.Movement;
using SlimeAI.GameOS.Observation;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;
using SlimeAI.GameOS.Runtime.Timer;
using SlimeAI.GameOS.Runtime.World;

namespace SlimeAI.SceneTests.GameOS.Capabilities;

/// <summary>
/// GameOS Capability 收敛改动的 Godot headless 验证场景。
/// </summary>
public partial class CapabilityConvergenceValidationScene : Node
{
    private const string ScenePath = "res://SlimeAI/Src/Validation/GameOS/Capabilities/Convergence/CapabilityConvergenceValidation.tscn";
    private const string ArtifactFileName = "capability-convergence-validation.json";
    private const string LogContext = "CapabilityConvergenceValidation";

    /// <inheritdoc />
    public override void _Ready()
    {
        using var observation = GameOSObservationSession.FromEnvironment(
            ScenePath,
            "validation",
            Path.Combine(Directory.GetCurrentDirectory(), ".ai-temp", "scene-tests", "manual", "artifacts"));
        using var validation = new SceneValidationSession(
            observation,
            LogContext,
            "GameOS/Capabilities",
            ArtifactFileName,
            new[]
            {
                "SlimeAI.GameOS.Capabilities.Feature",
                "SlimeAI.GameOS.Capabilities.Ability",
                "SlimeAI.GameOS.Capabilities.Damage",
                "SlimeAI.GameOS.Capabilities.AI"
            },
            new[]
            {
                "This scene validates framework Capability contracts only.",
                "BrotatoLike is used only as the Godot host project."
            },
            expectedInputs: new[]
            {
                "scoped RuntimeWorld with isolated runtime entities",
                "Feature action, auto-trigger and typed handler definitions",
                "Ability, Damage, AI and Ability selector test entities"
            },
            expectedObservations: new[]
            {
                "Feature action and auto-trigger paths invoke handlers deterministically",
                "Ability-to-Feature typed flow returns AbilityExecutedResult without fallback damage",
                "injected DamageService and capability-owned selectors are used instead of process globals"
            },
            passCriteria: new[]
            {
                "all Capability convergence checks pass",
                "stdout contains GameOS Capability convergence validation PASS",
                "failureReasons is empty"
            },
            failCriteria: new[]
            {
                "any Capability convergence check fails",
                "stdout contains GameOS Capability convergence validation FAIL",
                "artifact is missing standard-answer fields or failureReasons is non-empty"
            });

        validation.Info("validation start");

        validation.Check("feature_actions_and_auto_trigger", "Feature", ValidateFeatureActionsAndAutoTrigger);
        validation.Check("typed_ability_feature_flow", "AbilityFeature", ValidateTypedAbilityFeatureFlow);
        validation.Check("injected_damage_service", "ScopedServices", ValidateInjectedDamageService);
        validation.Check("damage_encounter_stats", "Damage", ValidateDamageEncounterStats);
        validation.Check("capability_owned_selectors", "Selectors", ValidateCapabilityOwnedSelectors);

        var success = validation.Success;
        if (success)
        {
            validation.Pass("all checks passed");
        }
        else
        {
            validation.Fail($"{validation.FailureReasons.Count} checks failed");
        }

        validation.WriteArtifact();

        GD.Print(success ? "GameOS Capability convergence validation PASS" : "GameOS Capability convergence validation FAIL");
        if (!success)
        {
            GD.Print($"GameOS Capability convergence validation failures: {string.Join("; ", validation.FailureReasons)}");
        }

        GetTree().Quit(success ? 0 : 1);
    }

    private static CheckResult ValidateFeatureActionsAndAutoTrigger()
    {
        using var world = RuntimeWorld.CreateScoped();
        FeatureHandlerRegistry.Clear();
        var timerManager = new TimerManager("capability-feature-scene-timers");
        var featureService = new FeatureService();
        var autoTrigger = new FeatureAutoTriggerService(featureService, timerManager);
        var periodicHandler = new CountingFeatureHandler("scene.feature.periodic");
        var eventHandler = new EventFeatureHandler("scene.feature.event");
        FeatureHandlerRegistry.Register(periodicHandler);
        FeatureHandlerRegistry.Register(eventHandler);

        try
        {
            var owner = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("scene-feature-owner") });
            var actionFeature = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("scene-feature-action") });
            var executeActionA = new CountingFeatureAction();
            var executeActionB = new CountingFeatureAction();
            var grantAction = new CountingFeatureAction();
            var actionContext = new FeatureContext { Owner = owner, Feature = actionFeature };

            featureService.ExecuteActions(new IFeatureAction[] { executeActionA, executeActionB }, actionContext);
            featureService.Grant(owner, actionFeature, new FeatureDefinition
            {
                FeatureId = "scene.feature.action",
                Actions = new[] { grantAction }
            });

            var periodicFeature = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("scene-feature-periodic") });
            periodicFeature.Data.Set(FeatureDataKeys.IsEnabled, true);
            periodicFeature.Data.Set(FeatureDataKeys.Cooldown, 0.25f);
            using var periodicRegistration = autoTrigger.RegisterPeriodic(owner, periodicFeature, new FeatureDefinition
            {
                FeatureId = "scene.feature.periodic",
                HandlerId = periodicHandler.FeatureId
            });

            timerManager.Tick(0.24f);
            var beforePeriodic = periodicHandler.Activated;
            timerManager.Tick(0.01f);
            var afterPeriodic = periodicHandler.Activated;
            periodicRegistration.Dispose();
            timerManager.Tick(0.25f);
            var afterDispose = periodicHandler.Activated;

            var eventFeature = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("scene-feature-event") });
            eventFeature.Data.Set(FeatureDataKeys.IsEnabled, true);
            eventFeature.Data.Set(FeatureDataKeys.TriggerChance, 100f);
            using var eventRegistration = autoTrigger.RegisterOnEvent<SceneFeatureEvent>(owner, eventFeature, new FeatureDefinition
            {
                FeatureId = "scene.feature.event",
                HandlerId = eventHandler.FeatureId
            }, owner.Events);

            owner.Events.Publish(new SceneFeatureEvent(7));
            eventFeature.Data.Set(FeatureDataKeys.TriggerChance, 0f);
            owner.Events.Publish(new SceneFeatureEvent(9));

            var success = executeActionA.Count == 1
                && executeActionB.Count == 1
                && grantAction.Count == 1
                && beforePeriodic == 0
                && afterPeriodic == 1
                && afterDispose == 1
                && eventHandler.Activated == 1
                && eventHandler.LastValue == 7;

            return CheckResult.From(success, success ? "feature action and auto-trigger contract passed" : "feature action or auto-trigger contract mismatch", new Dictionary<string, object?>
            {
                ["executeActionA"] = executeActionA.Count,
                ["executeActionB"] = executeActionB.Count,
                ["grantAction"] = grantAction.Count,
                ["beforePeriodic"] = beforePeriodic,
                ["afterPeriodic"] = afterPeriodic,
                ["afterDispose"] = afterDispose,
                ["eventActivations"] = eventHandler.Activated,
                ["eventLastValue"] = eventHandler.LastValue
            });
        }
        finally
        {
            timerManager.Clear();
            FeatureHandlerRegistry.Clear();
        }
    }

    private static CheckResult ValidateTypedAbilityFeatureFlow()
    {
        using var world = RuntimeWorld.CreateScoped();
        FeatureHandlerRegistry.Clear();
        var timerManager = new TimerManager("capability-ability-feature-scene-timers");
        var featureService = new FeatureService();
        var damageService = new DamageService();
        var handler = new AbilityFeatureProbeHandler();
        FeatureHandlerRegistry.Register(handler);

        try
        {
            var caster = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("scene-ability-caster") });
            var ability = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("scene-ability-feature") });
            ability.Data.Set(AbilityDataKeys.IsEnabled, true);
            ability.Data.Set(AbilityDataKeys.FeatureHandlerId, handler.FeatureId);
            ability.Data.Set(AbilityDataKeys.Damage, 99f);
            var target = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("scene-ability-target") });
            target.Data.Set(DamageDataKeys.CurrentHp, 20f);

            var abilityService = new AbilityService(timerManager, featureService, damageService);
            var report = abilityService.TryTrigger(new AbilityCastContext
            {
                Caster = caster,
                Ability = ability,
                Targets = new[] { target }
            });

            var success = ReferenceEquals(featureService, abilityService.FeatureService)
                && ReferenceEquals(damageService, abilityService.DamageService)
                && report.Result == AbilityTriggerResult.Success
                && handler.Executed == 1
                && handler.TypedActivationSeen
                && report.Executed?.TargetsHit == 1
                && target.Data.Get<float>(DamageDataKeys.CurrentHp) == 20f
                && !ability.Data.Get<bool>(FeatureDataKeys.IsActive);

            return CheckResult.From(success, success ? "typed Ability-to-Feature flow passed" : "typed Ability-to-Feature flow mismatch", new Dictionary<string, object?>
            {
                ["featureServiceInjected"] = ReferenceEquals(featureService, abilityService.FeatureService),
                ["damageServiceInjected"] = ReferenceEquals(damageService, abilityService.DamageService),
                ["triggerResult"] = report.Result.ToString(),
                ["handlerExecuted"] = handler.Executed,
                ["typedActivationSeen"] = handler.TypedActivationSeen,
                ["targetsHit"] = report.Executed?.TargetsHit,
                ["targetHp"] = target.Data.Get<float>(DamageDataKeys.CurrentHp)
            });
        }
        finally
        {
            timerManager.Clear();
            FeatureHandlerRegistry.Clear();
        }
    }

    private static CheckResult ValidateInjectedDamageService()
    {
        using var world = RuntimeWorld.CreateScoped();
        var timerManager = new TimerManager("capability-injected-damage-scene-timers");
        var damageService = new DamageService();
        damageService.RegisterProcessor(new BlockingDamageProcessor());
        var abilityService = new AbilityService(timerManager, new FeatureService(), damageService);

        var caster = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("scene-injected-damage-caster") });
        var ability = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("scene-injected-damage-ability") });
        ability.Data.Set(AbilityDataKeys.IsEnabled, true);
        ability.Data.Set(AbilityDataKeys.TargetSelection, AbilityTargetSelection.Entity);
        ability.Data.Set(AbilityDataKeys.Damage, 10f);
        var target = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("scene-injected-damage-target") });
        target.Data.Set(DamageDataKeys.CurrentHp, 20f);

        var report = abilityService.TryTrigger(new AbilityCastContext
        {
            Caster = caster,
            Ability = ability,
            Targets = new[] { target }
        });
        timerManager.Clear();

        var success = report.Result == AbilityTriggerResult.Success
            && ReferenceEquals(damageService, abilityService.DamageService)
            && target.Data.Get<float>(DamageDataKeys.CurrentHp) == 20f;

        return CheckResult.From(success, success ? "injected DamageService path passed" : "injected DamageService path mismatch", new Dictionary<string, object?>
        {
            ["triggerResult"] = report.Result.ToString(),
            ["damageServiceInjected"] = ReferenceEquals(damageService, abilityService.DamageService),
            ["targetHp"] = target.Data.Get<float>(DamageDataKeys.CurrentHp)
        });
    }

    private static CheckResult ValidateDamageEncounterStats()
    {
        using var world = RuntimeWorld.CreateScoped();
        var attacker = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("scene-damage-attacker") });
        attacker.Data.Set(DamageDataKeys.CritRate, 100f);
        attacker.Data.Set(DamageDataKeys.CritDamage, 200f);
        var victim = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("scene-damage-victim") });
        victim.Data.Set(DamageDataKeys.CurrentHp, 100f);
        victim.Data.Set(DamageDataKeys.Armor, 15f);

        var result = new DamageService().Process(new DamageInfo
        {
            Attacker = attacker,
            Victim = victim,
            Damage = 20f,
            Type = DamageType.Physical
        });

        var success = result.Applied
            && result.Info.IsCritical
            && result.Info.FinalDamage == 20f
            && victim.Data.Get<float>(DamageDataKeys.CurrentHp) == 80f
            && attacker.Data.Get<int>(DamageDataKeys.EncounterCriticalHits) == 1
            && attacker.Data.Get<int>(DamageDataKeys.TotalHits) == 1
            && attacker.Data.Get<float>(DamageDataKeys.EncounterDamageDealt) == 20f
            && victim.Data.Get<float>(DamageDataKeys.EncounterDamageTaken) == 20f;

        return CheckResult.From(success, success ? "Damage Encounter stats passed" : "Damage Encounter stats mismatch", new Dictionary<string, object?>
        {
            ["applied"] = result.Applied,
            ["isCritical"] = result.Info.IsCritical,
            ["finalDamage"] = result.Info.FinalDamage,
            ["victimHp"] = victim.Data.Get<float>(DamageDataKeys.CurrentHp),
            ["encounterCriticalHits"] = attacker.Data.Get<int>(DamageDataKeys.EncounterCriticalHits),
            ["encounterDamageDealt"] = attacker.Data.Get<float>(DamageDataKeys.EncounterDamageDealt),
            ["encounterDamageTaken"] = victim.Data.Get<float>(DamageDataKeys.EncounterDamageTaken)
        });
    }

    private static CheckResult ValidateCapabilityOwnedSelectors()
    {
        using var world = RuntimeWorld.CreateScoped();
        var caster = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("scene-selector-caster") });
        caster.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
        caster.Data.Set(CollisionDataKeys.Team, 1);
        var ability = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("scene-selector-ability") });
        ability.Data.Set(AbilityDataKeys.TargetSelection, AbilityTargetSelection.Entity);
        ability.Data.Set(AbilityDataKeys.AutoTargetRange, 10f);
        ability.Data.Set(AbilityDataKeys.AutoTargetMaxTargets, 1);

        var injectedNear = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("scene-selector-near") });
        injectedNear.Data.Set(MovementDataKeys.Position, new Vector2Value(2f, 0f));
        injectedNear.Data.Set(CollisionDataKeys.Team, 2);
        injectedNear.Data.Set(DamageDataKeys.CurrentHp, 20f);
        var injectedFar = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("scene-selector-far") });
        injectedFar.Data.Set(MovementDataKeys.Position, new Vector2Value(6f, 0f));
        injectedFar.Data.Set(CollisionDataKeys.Team, 2);
        injectedFar.Data.Set(DamageDataKeys.CurrentHp, 20f);
        var abilityQuery = new StaticAbilityTargetQuery(new[] { injectedFar, injectedNear });

        var abilityBuilt = AbilityTargetingTool.TryBuildContext(caster, ability, out var abilityContext, targetQuery: abilityQuery);

        var aiAgent = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("scene-ai-agent") });
        aiAgent.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
        aiAgent.Data.Set(CollisionDataKeys.Team, 1);
        var aiTarget = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("scene-ai-target") });
        aiTarget.Data.Set(MovementDataKeys.Position, new Vector2Value(3f, 0f));
        aiTarget.Data.Set(CollisionDataKeys.Team, 2);
        var ignoredTarget = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("scene-ai-ignored") });
        ignoredTarget.Data.Set(MovementDataKeys.Position, new Vector2Value(1f, 0f));
        ignoredTarget.Data.Set(CollisionDataKeys.Team, 2);

        var aiQuery = new StaticAITargetQuery(new[] { aiTarget });
        var aiAbilityService = new AbilityService(
            new TimerManager("capability-selector-ai-ability-timers"),
            new FeatureService(),
            new DamageService());
        var aiState = new AIService().Tick(new AIContext
        {
            Entity = aiAgent,
            Delta = 0.1f,
            AbilityService = aiAbilityService
        }, new FindNearestTargetAction(10f, targetQuery: aiQuery));

        var success = abilityBuilt
            && abilityContext?.Targets?.Count == 1
            && abilityContext.Targets[0].EntityId == injectedNear.EntityId
            && abilityQuery.Calls == 1
            && aiState == AIState.Success
            && aiAgent.Data.Get<EntityId?>(AIDataKeys.TargetEntity) == aiTarget.EntityId
            && aiQuery.Calls == 1
            && ignoredTarget.EntityId != aiAgent.Data.Get<EntityId?>(AIDataKeys.TargetEntity);

        return CheckResult.From(success, success ? "capability-owned selector injection passed" : "capability-owned selector injection mismatch", new Dictionary<string, object?>
        {
            ["abilityBuilt"] = abilityBuilt,
            ["abilityTarget"] = abilityContext?.Targets?[0].EntityId.Value,
            ["abilityQueryCalls"] = abilityQuery.Calls,
            ["aiState"] = aiState.ToString(),
            ["aiTarget"] = aiAgent.Data.Get<EntityId?>(AIDataKeys.TargetEntity)?.Value,
            ["aiQueryCalls"] = aiQuery.Calls
        });
    }

    private sealed class CountingFeatureAction : IFeatureAction
    {
        public int Count { get; private set; }

        public void Execute(FeatureContext context)
        {
            Count++;
        }
    }

    private sealed class CountingFeatureHandler : IFeatureHandler
    {
        public CountingFeatureHandler(string featureId)
        {
            FeatureId = featureId;
        }

        public string FeatureId { get; }
        public int Activated { get; private set; }
        public int Executed { get; private set; }

        public void OnActivated(FeatureContext context) => Activated++;

        public IFeatureExecutionResult? OnExecute(FeatureContext context)
        {
            Executed++;
            return null;
        }
    }

    private sealed class EventFeatureHandler : IFeatureHandler
    {
        public EventFeatureHandler(string featureId)
        {
            FeatureId = featureId;
        }

        public string FeatureId { get; }
        public int Activated { get; private set; }
        public int LastValue { get; private set; }

        public void OnActivated(FeatureContext context)
        {
            Activated++;
            if (context.SourceEventPayload is FeatureEventActivationPayload<SceneFeatureEvent> payload)
            {
                LastValue = payload.Event.Value;
            }
        }
    }

    private sealed class AbilityFeatureProbeHandler : IFeatureHandler
    {
        public string FeatureId => "scene.ability.probe";
        public int Executed { get; private set; }
        public bool TypedActivationSeen { get; private set; }

        public IFeatureExecutionResult? OnExecute(FeatureContext context)
        {
            Executed++;
            TypedActivationSeen = context.TryGetActivation<AbilityCastContext>(out var cast);
            return new AbilityExecutedResult
            {
                TargetsHit = cast?.Targets?.Count ?? 0,
                TotalDamage = 0f
            };
        }
    }

    private sealed class BlockingDamageProcessor : IDamageProcessor
    {
        public int Priority => DamageProcessorPriority.HealthExecution - 1;

        public void Process(DamageInfo info)
        {
            info.FinalDamage = 0f;
            info.IsBlocked = true;
        }
    }

    private sealed class StaticAbilityTargetQuery : IAbilityTargetQuery
    {
        private readonly IReadOnlyList<IEntity> candidates;

        public StaticAbilityTargetQuery(IReadOnlyList<IEntity> candidates)
        {
            this.candidates = candidates;
        }

        public int Calls { get; private set; }

        public IReadOnlyList<IEntity> GetCandidates(IEntity caster)
        {
            Calls++;
            return candidates;
        }
    }

    private sealed class StaticAITargetQuery : IAITargetQuery
    {
        private readonly IReadOnlyList<IEntity> candidates;

        public StaticAITargetQuery(IReadOnlyList<IEntity> candidates)
        {
            this.candidates = candidates;
        }

        public int Calls { get; private set; }

        public IReadOnlyList<IEntity> GetCandidates(IEntity self)
        {
            Calls++;
            return candidates;
        }
    }

    private readonly record struct SceneFeatureEvent(int Value) : IEntityEvent;
}
