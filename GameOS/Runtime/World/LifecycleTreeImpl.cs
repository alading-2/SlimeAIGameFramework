using System;
using System.Collections.Generic;
using SlimeAI.GameOS.Runtime.CommandBuffer;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Events.Core;
using SlimeAI.GameOS.Runtime.Schedule;

namespace SlimeAI.GameOS.Runtime.World;

/// <summary>
/// RuntimeWorld 持有的 lifecycle parent-child 树。
/// </summary>
public sealed class LifecycleTreeImpl : ILifecycleTree
{
    private readonly IWorldEventBus events;
    private readonly RuntimeCommandBuffer commands;
    private readonly Dictionary<EntityId, LifecycleLink> childToLink = new();
    private readonly Dictionary<EntityId, List<LifecycleLink>> parentToLinks = new();
    private readonly Dictionary<EntityId, EntityId> childToParent = new();

    /// <summary>
    /// 创建 lifecycle 树实例。
    /// </summary>
    public LifecycleTreeImpl(IWorldEventBus events, RuntimeCommandBuffer commands)
    {
        this.events = events ?? throw new ArgumentNullException(nameof(events));
        this.commands = commands ?? throw new ArgumentNullException(nameof(commands));
    }

    /// <inheritdoc />
    public bool Attach(
        EntityId parentId,
        EntityId childId,
        ParentDestroyPolicy destroyPolicy = ParentDestroyPolicy.DestroyRecursively,
        int priority = 0)
    {
        if (commands.IsGuarded && !commands.IsPlayingBack)
        {
            if (parentId.IsEmpty || childId.IsEmpty || parentId.Equals(childId))
            {
                return false;
            }

            commands.Enqueue(DeferredRuntimeCommand.ForAttach(parentId, childId, destroyPolicy, priority, SchedulePhase.EndOfFrame));
            return true;
        }

        return AttachImmediate(parentId, childId, destroyPolicy, priority);
    }

    internal bool AttachImmediate(
        EntityId parentId,
        EntityId childId,
        ParentDestroyPolicy destroyPolicy = ParentDestroyPolicy.DestroyRecursively,
        int priority = 0)
    {
        if (parentId.IsEmpty || childId.IsEmpty)
        {
            return false;
        }

        if (parentId.Equals(childId))
        {
            return false;
        }

        if (childToParent.ContainsKey(childId))
        {
            return false;
        }

        if (WouldCreateCycle(parentId, childId))
        {
            return false;
        }

        var link = new LifecycleLink(parentId, childId, destroyPolicy, priority);
        childToLink[childId] = link;
        childToParent[childId] = parentId;
        if (!parentToLinks.TryGetValue(parentId, out var siblings))
        {
            siblings = new List<LifecycleLink>();
            parentToLinks[parentId] = siblings;
        }

        siblings.Add(link);
        using (commands.EnterGuard("lifecycle-callback"))
        {
            events.Publish(new LifecycleChildAttached(parentId, childId, destroyPolicy));
        }

        return true;
    }

    /// <inheritdoc />
    public bool Detach(EntityId parentId, EntityId childId)
    {
        if (commands.IsGuarded && !commands.IsPlayingBack)
        {
            if (parentId.IsEmpty || childId.IsEmpty)
            {
                return false;
            }

            commands.Enqueue(DeferredRuntimeCommand.ForDetach(parentId, childId, SchedulePhase.EndOfFrame));
            return true;
        }

        return DetachImmediate(parentId, childId);
    }

    internal bool DetachImmediate(EntityId parentId, EntityId childId)
    {
        if (parentId.IsEmpty || childId.IsEmpty)
        {
            return false;
        }

        if (!childToLink.TryGetValue(childId, out var link) || !link.ParentEntityId.Equals(parentId))
        {
            return false;
        }

        childToLink.Remove(childId);
        childToParent.Remove(childId);
        if (parentToLinks.TryGetValue(parentId, out var siblings))
        {
            for (var i = siblings.Count - 1; i >= 0; i--)
            {
                if (siblings[i].ChildEntityId.Equals(childId))
                {
                    siblings.RemoveAt(i);
                }
            }

            if (siblings.Count == 0)
            {
                parentToLinks.Remove(parentId);
            }
        }

        using (commands.EnterGuard("lifecycle-callback"))
        {
            events.Publish(new LifecycleChildDetached(parentId, childId, link.DestroyPolicy));
        }

        return true;
    }

    /// <inheritdoc />
    public bool IsAttached(EntityId parentId, EntityId childId)
    {
        if (parentId.IsEmpty || childId.IsEmpty)
        {
            return false;
        }

        return childToParent.TryGetValue(childId, out var existing) && existing.Equals(parentId);
    }

    /// <inheritdoc />
    public EntityId GetParentEntityId(EntityId childId)
    {
        if (childId.IsEmpty)
        {
            return EntityId.Empty;
        }

        return childToParent.TryGetValue(childId, out var parent) ? parent : EntityId.Empty;
    }

    /// <inheritdoc />
    public IReadOnlyList<LifecycleLink> GetChildren(EntityId parentId)
    {
        if (parentId.IsEmpty || !parentToLinks.TryGetValue(parentId, out var siblings))
        {
            return Array.Empty<LifecycleLink>();
        }

        return new List<LifecycleLink>(siblings);
    }

    /// <inheritdoc />
    public IReadOnlyList<EntityId> GetChildEntityIds(EntityId parentId)
    {
        if (parentId.IsEmpty || !parentToLinks.TryGetValue(parentId, out var siblings) || siblings.Count == 0)
        {
            return Array.Empty<EntityId>();
        }

        var result = new EntityId[siblings.Count];
        for (var i = 0; i < siblings.Count; i++)
        {
            result[i] = siblings[i].ChildEntityId;
        }

        return result;
    }

    /// <inheritdoc />
    public void DetachAll(EntityId entityId)
    {
        if (entityId.IsEmpty)
        {
            return;
        }

        if (childToParent.TryGetValue(entityId, out var parentId))
        {
            Detach(parentId, entityId);
        }

        if (parentToLinks.TryGetValue(entityId, out var children) && children.Count > 0)
        {
            var snapshot = new EntityId[children.Count];
            for (var i = 0; i < children.Count; i++)
            {
                snapshot[i] = children[i].ChildEntityId;
            }

            for (var i = 0; i < snapshot.Length; i++)
            {
                Detach(entityId, snapshot[i]);
            }
        }
    }

    /// <inheritdoc />
    public void Clear()
    {
        while (childToLink.Count > 0)
        {
            using var enumerator = childToLink.Values.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                break;
            }

            var link = enumerator.Current;
            Detach(link.ParentEntityId, link.ChildEntityId);
        }
    }

    private bool WouldCreateCycle(EntityId parentId, EntityId childId)
    {
        // 沿 parent 现有 lifecycle 链向上回查；若回到 childId 即形成环。
        var current = parentId;
        for (var depth = 0; depth < 128; depth++)
        {
            if (!childToParent.TryGetValue(current, out var next))
            {
                return false;
            }

            if (next.Equals(childId))
            {
                return true;
            }

            current = next;
        }

        // 深度超限按 cycle 处理，避免误连。
        return true;
    }
}
