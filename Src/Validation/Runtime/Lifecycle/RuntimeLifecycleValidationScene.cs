using System.Collections.Generic;
using System.IO;
using Godot;
using SlimeAI.GameOS.Observation;
using SlimeAI.GameOS.Runtime.Data;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Events.Core;
using SlimeAI.GameOS.Runtime.World;

namespace SlimeAI.SceneTests.Runtime.Lifecycle;

/// <summary>
/// Runtime/Lifecycle 的 Godot headless 验证场景。
/// </summary>
public partial class RuntimeLifecycleValidationScene : Node
{
    private const string ScenePath = "res://SlimeAI/Scenes/Validation/Runtime/Lifecycle/RuntimeLifecycleValidation.tscn";
    private const string ArtifactFileName = "runtime-lifecycle-validation.json";
    private const string LogContext = "RuntimeLifecycleValidation";

    private static readonly DataKey<EntityId?> ValidationOwner = DataKey.Create<EntityId?>("RuntimeLifecycleValidation.Owner");
    private static readonly DataKey<EntityIdList> ValidationOwnedIds = DataKey.Create(
        "RuntimeLifecycleValidation.OwnedIds",
        EntityIdList.Empty);

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
            "Runtime/Lifecycle",
            ArtifactFileName,
            new[]
            {
                "SlimeAI.GameOS.Runtime.Entity",
                "SlimeAI.GameOS.Runtime.World",
                "SlimeAI.GameOS.Runtime.Data",
                "Games/BrotatoLike Godot scene runner"
            },
            new[]
            {
                "This scene validates lifecycle parent-child invariants and typed business reference cleanup.",
                "RuntimeOwnedReferenceRegistry is cleared before and after owner cleanup checks."
            },
            expectedInputs: new[]
            {
                "scoped RuntimeWorld entities for parent, child, grandchild and owned references",
                "LifecycleTree attach/detach requests including invalid self, empty and cycle cases",
                "RuntimeOwnedReferenceRegistry descriptor using RuntimeLifecycleValidation.Owner and RuntimeLifecycleValidation.OwnedIds"
            },
            expectedObservations: new[]
            {
                "single parent is enforced and invalid attach cases are rejected",
                "DestroyRecursively destroys recursive children while Detach keeps detached children alive",
                "EntityIdList remains immutable/value-equal and owner cleanup removes destroyed child ids"
            },
            passCriteria: new[]
            {
                "all lifecycle and owner cleanup checks pass",
                "stdout contains GameOS Runtime Lifecycle validation PASS",
                "failureReasons is empty"
            },
            failCriteria: new[]
            {
                "any lifecycle invariant or owner cleanup check fails",
                "stdout contains GameOS Runtime Lifecycle validation FAIL",
                "failureReasons names the broken lifecycle or cleanup invariant"
            });

        validation.Info("validation start");
        validation.Check("attach_detach_and_single_parent", "LifecycleTree", ValidateAttachDetachAndSingleParent);
        validation.Check("self_cycle_and_empty_rejected", "LifecycleTree", ValidateSelfCycleAndEmptyRejected);
        validation.Check("destroy_policy_recursive_and_detach", "LifecycleTree", ValidateDestroyPolicyRecursiveAndDetach);
        validation.Check("entity_id_list_value_semantics", "EntityIdList", ValidateEntityIdListValueSemantics);
        validation.Check("owned_reference_registry_cleanup", "OwnedReferenceCleanup", ValidateOwnedReferenceRegistryCleanup);

        var success = validation.Success;
        if (success)
        {
            validation.Pass("all checks passed");
        }
        else
        {
            validation.Fail($"{validation.FailureReasons.Count} checks failed");
        }

        RuntimeOwnedReferenceRegistry.Clear();
        validation.WriteArtifact();
        GD.Print(success ? "GameOS Runtime Lifecycle validation PASS" : "GameOS Runtime Lifecycle validation FAIL");
        if (!success)
        {
            GD.Print($"GameOS Runtime Lifecycle validation failures: {string.Join("; ", validation.FailureReasons)}");
        }

        GetTree().Quit(success ? 0 : 1);
    }

    private static CheckResult ValidateAttachDetachAndSingleParent()
    {
        using var world = RuntimeWorld.CreateScoped();
        var attached = 0;
        var detached = 0;
        using var attachSub = world.Events.Subscribe<LifecycleChildAttached>(_ => attached++);
        using var detachSub = world.Events.Subscribe<LifecycleChildDetached>(_ => detached++);

        var parent1 = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("lifecycle-parent-1") });
        var parent2 = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("lifecycle-parent-2") });
        var child = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("lifecycle-child") });

        var attachedFirst = world.Lifecycle.Attach(parent1.EntityId, child.EntityId, ParentDestroyPolicy.Detach, 5);
        var secondParentBlocked = !world.Lifecycle.Attach(parent2.EntityId, child.EntityId);
        var detachedFirst = world.Lifecycle.Detach(parent1.EntityId, child.EntityId);
        var reattachedSecond = world.Lifecycle.Attach(parent2.EntityId, child.EntityId);

        var success = attachedFirst
            && secondParentBlocked
            && detachedFirst
            && reattachedSecond
            && world.Lifecycle.IsAttached(parent2.EntityId, child.EntityId)
            && world.Lifecycle.GetParentEntityId(child.EntityId) == parent2.EntityId
            && attached == 2
            && detached == 1;

        return CheckResult.From(success, success ? "attach/detach and single-parent checks passed" : "attach/detach or single-parent mismatch", new Dictionary<string, object?>
        {
            ["attachedFirst"] = attachedFirst,
            ["secondParentBlocked"] = secondParentBlocked,
            ["detachedFirst"] = detachedFirst,
            ["reattachedSecond"] = reattachedSecond,
            ["attachedEvents"] = attached,
            ["detachedEvents"] = detached
        });
    }

    private static CheckResult ValidateSelfCycleAndEmptyRejected()
    {
        using var world = RuntimeWorld.CreateScoped();
        var parent = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("lifecycle-cycle-parent") });
        var child = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("lifecycle-cycle-child") });
        var grandchild = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("lifecycle-cycle-grandchild") });

        var parentChildAttached = world.Lifecycle.Attach(parent.EntityId, child.EntityId);
        var childGrandchildAttached = world.Lifecycle.Attach(child.EntityId, grandchild.EntityId);
        var selfRejected = !world.Lifecycle.Attach(parent.EntityId, parent.EntityId);
        var emptyParentRejected = !world.Lifecycle.Attach(EntityId.Empty, child.EntityId);
        var emptyChildRejected = !world.Lifecycle.Attach(parent.EntityId, EntityId.Empty);
        var cycleRejected = !world.Lifecycle.Attach(grandchild.EntityId, parent.EntityId);

        var success = parentChildAttached
            && childGrandchildAttached
            && selfRejected
            && emptyParentRejected
            && emptyChildRejected
            && cycleRejected;

        return CheckResult.From(success, success ? "self/cycle/empty rejection passed" : "invalid lifecycle attach was accepted", new Dictionary<string, object?>
        {
            ["selfRejected"] = selfRejected,
            ["emptyParentRejected"] = emptyParentRejected,
            ["emptyChildRejected"] = emptyChildRejected,
            ["cycleRejected"] = cycleRejected
        });
    }

    private static CheckResult ValidateDestroyPolicyRecursiveAndDetach()
    {
        using var world = RuntimeWorld.CreateScoped();
        var parent = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("lifecycle-destroy-parent") });
        var recursiveChild = world.Entities.Spawn(new EntitySpawnConfig
        {
            EntityId = new EntityId("lifecycle-destroy-recursive"),
            ParentEntityId = parent.EntityId,
            ParentDestroyPolicy = ParentDestroyPolicy.DestroyRecursively
        });
        var detachedChild = world.Entities.Spawn(new EntitySpawnConfig
        {
            EntityId = new EntityId("lifecycle-destroy-detach"),
            ParentEntityId = parent.EntityId,
            ParentDestroyPolicy = ParentDestroyPolicy.Detach
        });

        var destroyedParent = world.Entities.Destroy(parent.EntityId);
        var success = destroyedParent
            && world.Entities.Get(parent.EntityId) == null
            && world.Entities.Get(recursiveChild.EntityId) == null
            && world.Entities.Get(detachedChild.EntityId)?.EntityId == detachedChild.EntityId
            && !world.Lifecycle.IsAttached(parent.EntityId, detachedChild.EntityId);

        return CheckResult.From(success, success ? "recursive destroy and detach policy passed" : "parent destroy policy mismatch", new Dictionary<string, object?>
        {
            ["destroyedParent"] = destroyedParent,
            ["recursiveChildAlive"] = world.Entities.Get(recursiveChild.EntityId) != null,
            ["detachedChildAlive"] = world.Entities.Get(detachedChild.EntityId) != null,
            ["detachedChildAttached"] = world.Lifecycle.IsAttached(parent.EntityId, detachedChild.EntityId)
        });
    }

    private static CheckResult ValidateEntityIdListValueSemantics()
    {
        var a = new EntityId("lifecycle-list-a");
        var b = new EntityId("lifecycle-list-b");
        var list = EntityIdList.Empty.Add(a).Add(b).Add(a);
        var same = EntityIdList.Empty.Add(a).Add(b);
        var removed = list.Remove(a);

        var success = list.Count == 2
            && list[0] == a
            && list[1] == b
            && list.Contains(a)
            && list.Equals(same)
            && removed.Count == 1
            && removed[0] == b
            && list.Count == 2;

        return CheckResult.From(success, success ? "EntityIdList value semantics passed" : "EntityIdList value semantics mismatch", new Dictionary<string, object?>
        {
            ["listCount"] = list.Count,
            ["removedCount"] = removed.Count,
            ["containsA"] = list.Contains(a),
            ["valueEquals"] = list.Equals(same)
        });
    }

    private static CheckResult ValidateOwnedReferenceRegistryCleanup()
    {
        RuntimeOwnedReferenceRegistry.Clear();
        using var world = RuntimeWorld.CreateScoped();
        RuntimeOwnedReferenceRegistry.Register(new OwnedReferenceDescriptor(ValidationOwner, ValidationOwnedIds));

        var owner = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("lifecycle-owner") });
        var child1 = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("lifecycle-owned-1") });
        var child2 = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("lifecycle-owned-2") });

        child1.Data.Set(ValidationOwner, owner.EntityId);
        child2.Data.Set(ValidationOwner, owner.EntityId);
        owner.Data.Set(ValidationOwnedIds, EntityIdList.Empty.Add(child1.EntityId).Add(child2.EntityId));

        var beforeContainsBoth = owner.Data.Get(ValidationOwnedIds).Contains(child1.EntityId)
            && owner.Data.Get(ValidationOwnedIds).Contains(child2.EntityId);
        world.Entities.Destroy(child1.EntityId);
        var after = owner.Data.Get(ValidationOwnedIds);
        RuntimeOwnedReferenceRegistry.Clear();

        var success = beforeContainsBoth
            && !after.Contains(child1.EntityId)
            && after.Contains(child2.EntityId);

        return CheckResult.From(success, success ? "owned reference cleanup passed" : "owned reference cleanup mismatch", new Dictionary<string, object?>
        {
            ["beforeContainsBoth"] = beforeContainsBoth,
            ["afterContainsChild1"] = after.Contains(child1.EntityId),
            ["afterContainsChild2"] = after.Contains(child2.EntityId),
            ["afterCount"] = after.Count
        });
    }
}
