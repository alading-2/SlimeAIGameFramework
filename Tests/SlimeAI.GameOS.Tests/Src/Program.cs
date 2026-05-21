using SlimeAI.GameOS.Capabilities.Movement;
using SlimeAI.GameOS.Capabilities.Collision;
using SlimeAI.GameOS.Capabilities.Damage;
using SlimeAI.GameOS.Capabilities.Effect;
using SlimeAI.GameOS.Capabilities.Ability;
using SlimeAI.GameOS.Capabilities.Feature;
using SlimeAI.GameOS.Capabilities.AI;
using SlimeAI.GameOS.Capabilities.Attack;
using SlimeAI.GameOS.Capabilities.Projectile;
using SlimeAI.GameOS.Observation;
using SlimeAI.GameOS.Runtime.Data;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;
using SlimeAI.GameOS.Runtime.Events.Core;
using SlimeAI.GameOS.Runtime.Pool;
using SlimeAI.GameOS.Runtime.Resource;
using SlimeAI.GameOS.Runtime.Schedule;
using SlimeAI.GameOS.Runtime.Timer;
using SlimeAI.GameOS.Runtime.World;
using AbilityEvents = SlimeAI.GameOS.Capabilities.Ability.Events;
using AttackEvents = SlimeAI.GameOS.Capabilities.Attack.Events;
using CollisionEvents = SlimeAI.GameOS.Capabilities.Collision.Events;
using DamageEvents = SlimeAI.GameOS.Capabilities.Damage.Events;
using EffectEvents = SlimeAI.GameOS.Capabilities.Effect.Events;
using MovementEvents = SlimeAI.GameOS.Capabilities.Movement.Events;
using ProjectileEvents = SlimeAI.GameOS.Capabilities.Projectile.Events;

internal partial class Program
{
    static int Main(string[] args)
    {
        var tests = new (string Name, Action Run)[]
        {
            ("EventBus subscribe publish and dispose", TestEventBus),
            ("EventBus reentry same type is blocked", EventBusTests.ReentrySameTypeIsBlocked),
            ("EventBus different type cascade allowed", EventBusTests.DifferentTypeCascadeAllowed),
            ("EventBus different bus same type allowed", EventBusTests.DifferentBusSameTypeAllowed),
            ("EventBus handler exception is isolated", EventBusTests.HandlerExceptionIsIsolated),
            ("EventBus dispose unsubscribes", EventBusTests.DisposeUnsubscribes),
            ("EventBus registration order preserved", EventBusTests.RegistrationOrderPreserved),
            ("EventBus broadcast routes to both buses", EventBusTests.BroadcastRoutesToBothBuses),
            ("EventBus world publish logs event and handler count", EventBusTests.WorldPublishLogsEventAndHandlerCount),
            ("EventBus entity publish logs event and handler count", EventBusTests.EntityPublishLogsEventAndHandlerCount),
            ("EventBus entity event on world bus rejected", EventBusTests.EntityEventOnWorldBusRejected),
            ("EventBus global event on entity bus rejected", EventBusTests.GlobalEventOnEntityBusRejected),
            ("RuntimeWorld default singleton", RuntimeWorldTests.DefaultSingleton),
            ("RuntimeWorld default cannot dispose", RuntimeWorldTests.DefaultCannotDispose),
            ("RuntimeWorld scoped creation logs", RuntimeWorldTests.ScopedCreationLogsWorldCreated),
            ("RuntimeWorld scoped subsystem isolation", RuntimeWorldTests.CreateScopedIsolatesSubsystems),
            ("RuntimeWorld dispose order", RuntimeWorldTests.DisposeOrderIsDeterministic),
            ("RuntimeWorld dispose publishes teardown events", RuntimeWorldTests.DisposePublishesTeardownEventsBeforeEventsClear),
            ("RuntimeWorld access after dispose throws", RuntimeWorldTests.DisposeAfterAccessThrows),
            ("RuntimeWorld subsystem handles present", RuntimeWorldTests.SubsystemHandlesArePresent),
            ("GodotBridge context registries are isolated", GodotBridgeContextTests.ContextRegistriesAreIsolated),
            ("GodotBridge component registration logs binding", GodotBridgeContextTests.ComponentRegistrationLogsBinding),
            ("RuntimeCommandBuffer enqueue snapshot and clear", RuntimeCommandBufferTests.EnqueueSnapshotAndClear),
            ("RuntimeCommandBuffer plays all command kinds", RuntimeCommandBufferTests.PlaysAllCommandKinds),
            ("RuntimeCommandBuffer reports typical failures", RuntimeCommandBufferTests.ReportsTypicalFailuresForAllCommandKinds),
            ("RuntimeCommandBuffer payload invariant and sequence ordering", RuntimeCommandBufferTests.PayloadInvariantAndSequenceOrdering),
            ("RuntimeCommandBuffer dispose skips pending commands", RuntimeCommandBufferTests.DisposeSkipsPendingCommands),
            ("SchedulePhase run phase covers fixed phases", SchedulePhaseTests.RunPhaseCoversFixedPhases),
            ("SchedulePhase only plays matching phase", SchedulePhaseTests.RunPhaseOnlyPlaysMatchingPhase),
            ("SchedulePhase manual is not played by frame phases", SchedulePhaseTests.ManualPhaseIsNotPlayedByFramePhases),
            ("SchedulePhase run after clear throws", SchedulePhaseTests.RunPhaseAfterScheduleClearThrows),
            ("StructuralChangeGuard nested scopes", StructuralChangeGuardTests.EnterGuardTracksNestedScopes),
            ("StructuralChangeGuard guarded spawn queues reserved entity", StructuralChangeGuardTests.GuardedSpawnQueuesReservedEntity),
            ("StructuralChangeGuard guarded destroy and lifecycle queue", StructuralChangeGuardTests.GuardedDestroyAndLifecycleMutationsQueue),
            ("StructuralChangeGuard disposes after exception", StructuralChangeGuardTests.GuardDisposesAfterException),
            ("StructuralChangeGuard command limit throws", StructuralChangeGuardTests.GuardScopeCommandLimitThrows),
            ("Entity Data event bridge", TestEntityDataBridge),
            ("EntityId value equality", EntityIdTests.ValueEqualityWorksAcrossSameKey),
            ("EntityId empty/default/null all equal", EntityIdTests.EmptyDefaultAndFromNullAllEqual),
            ("EntityId IsEmpty for null/empty/whitespace", EntityIdTests.IsEmptyTreatsNullAndEmptyAsEmpty),
            ("EntityId Value preserves underlying string", EntityIdTests.ValuePreservesUnderlyingString),
            ("Entity lifecycle", TestEntityLifecycle),
            ("Entity registry spawn destroy logs", TestEntityRegistrySpawnDestroyLogs),
            ("LifecycleTree attach detach single-parent", TestLifecycleTree),
            ("EntityIdList immutable value-equality", TestEntityIdList),
            ("RuntimeOwnedReferenceRegistry cleanup descriptor", TestRuntimeOwnedReferenceRegistry),
            ("Entity parent destroy policy", TestEntityParentDestroyPolicy),
            ("SystemRunCondition gates", TestSystemRunCondition),
            ("RuntimeSchedule lifecycle and command", TestRuntimeSchedule),
            ("RuntimeSchedule logging and status", TestRuntimeScheduleLoggingAndStatus),
            ("ObjectPool reuse", TestObjectPool),
            ("TimerManager tick", TestTimerManager),
            ("ResourceCatalog mapping", TestResourceCatalog),
            ("Runtime typed Data API", TestRuntimeTypedDataApi),
            ("Runtime typed Data modifier constraints", TestRuntimeTypedDataModifierConstraints),
            ("Runtime typed Data computed cache", TestRuntimeTypedDataComputedCache),
            ("Runtime DataOS snapshot applies data and resources", TestRuntimeDataSnapshotAppliesDataAndResources),
            ("Runtime DataOS snapshot rejects invalid fields", TestRuntimeDataSnapshotRejectsInvalidFields),
            ("Runtime DataOS snapshot trims disabled resources", TestRuntimeDataSnapshotRejectsDisabledResources),
            ("Runtime DataOS snapshot rejects dropped and missing resources", TestRuntimeDataSnapshotRejectsDroppedAndMissingResources),
            ("Collision layer mask event", TestCollisionLayerMaskEvent),
            ("Damage service applies health", TestDamageServiceAppliesHealth),
            ("Damage service killed event", TestDamageServiceKilledEvent),
            ("Damage pipeline dodge", TestDamagePipelineDodge),
            ("Damage pipeline critical armor and stats", TestDamagePipelineCriticalArmorAndStats),
            ("Damage pipeline true damage bypasses dodge and armor", TestDamagePipelineTrueDamageBypassesDodgeAndArmor),
            ("Damage pipeline lifesteal", TestDamagePipelineLifesteal),
            ("Damage pipeline shield", TestDamagePipelineShield),
            ("Heal service applies and clamps", TestHealServiceAppliesAndClamps),
            ("DamageTool multi target and periodic", TestDamageToolMultiTargetAndPeriodic),
            ("Ability service instant damage cooldown and charge", TestAbilityServiceInstantDamageCooldownAndCharge),
            ("Ability service requires target", TestAbilityServiceRequiresTarget),
            ("Ability service accepts point target", TestAbilityServiceAcceptsPointTarget),
            ("Ability service accepts entity or point target", TestAbilityServiceAcceptsEntityOrPointTarget),
            ("Ability service periodic damage", TestAbilityServicePeriodicDamage),
            ("Ability service periodic auto trigger", TestAbilityServicePeriodicAutoTrigger),
            ("Ability targeting tool finds nearest entity", TestAbilityTargetingToolFindsNearestEntity),
            ("Projectile tool spawns runtime entity", TestProjectileToolSpawnsRuntimeEntity),
            ("Projectile movement hit damages and destroys", TestProjectileMovementHitDamagesAndDestroys),
            ("Projectile movement pierces and destroys after max hits", TestProjectileMovementPiercesAndDestroysAfterMaxHits),
            ("Projectile movement lifetime destroys", TestProjectileMovementLifetimeDestroys),
            ("Effect tool spawns runtime entity", TestEffectToolSpawnsRuntimeEntity),
            ("Feature service grants modifiers and removes", TestFeatureServiceGrantsModifiersAndRemoves),
            ("Feature service lifecycle handler", TestFeatureServiceLifecycleHandler),
            ("Feature actions execute and grant", TestFeatureActionsExecuteAndGrant),
            ("Feature auto trigger periodic", TestFeatureAutoTriggerPeriodic),
            ("Feature auto trigger on event", TestFeatureAutoTriggerOnEvent),
            ("Ability service invokes feature handler", TestAbilityServiceInvokesFeatureHandler),
            ("Ability service injected damage service", TestAbilityServiceUsesInjectedDamageService),
            ("AI service moves toward target", TestAIServiceMovesTowardTarget),
            ("AI service finds nearest target", TestAIServiceFindsNearestTarget),
            ("AI service patrol action", TestAIServicePatrolAction),
            ("AI behavior tree builder melee attack priority", TestAIBehaviorTreeBuilderMeleeAttackPriority),
            ("AI behavior tree builder patrol fallback", TestAIBehaviorTreeBuilderPatrolFallback),
            ("AI service requests attack in range", TestAIServiceRequestsAttackInRange),
            ("AI service ability auto trigger action", TestAIServiceAbilityAutoTriggerAction),
            ("AI service prepares ability auto target contexts", TestAIServicePreparesAbilityAutoTargetContexts),
            ("Attack service consumes request and damages", TestAttackServiceConsumesRequestAndDamages),
            ("Attack service gates range and cooldown", TestAttackServiceGatesRangeAndCooldown),
            ("Attack service windup and recovery timers", TestAttackServiceWindupAndRecoveryTimers),
            ("Movement charge tick", TestMovementChargeTick),
            ("Movement target stop event", TestMovementTargetStopEvent),
            ("Movement orbit tick", TestMovementOrbitTick),
            ("Movement sine wave tick", TestMovementSineWaveTick),
            ("Movement bezier curve tick", TestMovementBezierCurveTick),
            ("Movement boomerang tick", TestMovementBoomerangTick),
            ("Movement attach to host tick", TestMovementAttachToHostTick),
            ("Movement player input tick", TestMovementPlayerInputTick),
            ("Movement AI controlled tick", TestMovementAIControlledTick),
            ("Movement parabola tick", TestMovementParabolaTick),
            ("Movement circular arc tick", TestMovementCircularArcTick),
            ("Movement orientation params defaults", TestMovementOrientationParamsDefaults),
            ("Movement collision stop tick", TestMovementCollisionStopTick),
            ("Movement collision target query injection", TestMovementCollisionTargetQueryInjection),
            ("Movement collision notify without stop", TestMovementCollisionNotifyWithoutStop),
            ("Observation log level filter and formatting", TestObservationLogLevelFilterAndFormatting),
            ("Observation context filter overrides minimum level", TestObservationContextFilterOverridesMinimumLevel),
            ("Observation JSONL serialization", TestObservationJsonlSerialization),
            ("Scene validation session aggregates failures", TestSceneValidationSessionAggregatesFailures),
            ("Scene validation session writes standard answer fields", TestSceneValidationSessionWritesStandardAnswerFields)
        };

        var failed = 0;
        for (var i = 0; i < tests.Length; i++)
        {
            try
            {
                tests[i].Run();
                Console.WriteLine($"PASS {tests[i].Name}");
            }
            catch (Exception ex)
            {
                failed++;
                Console.Error.WriteLine($"FAIL {tests[i].Name}: {ex.Message}");
            }
        }

        return failed;
    }
}
