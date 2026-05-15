using System;
using System.Collections.Generic;
using SlimeAI.GameOS.Runtime.Event;
using SlimeAI.GameOS.Runtime.Events.Core;

namespace SlimeAI.GameOS.Runtime.Entity;

/// <summary>
/// 生命周期父子树 façade，唯一公开的 Runtime Relationship 入口。
/// </summary>
/// <remarks>
/// <para>
/// <see cref="LifecycleTree"/> 假设每个 entity 至多一个 lifecycle parent；
/// 业务多对多引用 MUST 用 typed <c>DataKey&lt;EntityId?&gt;</c> 或
/// <c>DataKey&lt;EntityIdList&gt;</c>，不通过 <see cref="LifecycleTree"/>。
/// </para>
/// <para>
/// 所有 API 接受 <see cref="EntityId"/>；<see cref="EntityId.Empty"/> 不允许作为参数。
/// </para>
/// </remarks>
public static class LifecycleTree
{
    private static readonly Dictionary<EntityId, LifecycleLink> ChildToLink = new();
    private static readonly Dictionary<EntityId, List<LifecycleLink>> ParentToLinks = new();
    private static readonly Dictionary<EntityId, EntityId> ChildToParent = new();

    /// <summary>
    /// 将 <paramref name="childId"/> 作为 <paramref name="parentId"/> 的 lifecycle child 挂接。
    /// </summary>
    /// <param name="parentId">父实体 Id；不能为 <see cref="EntityId.Empty"/>。</param>
    /// <param name="childId">子实体 Id；不能为 <see cref="EntityId.Empty"/>，且不能已有 lifecycle parent。</param>
    /// <param name="destroyPolicy">父销毁策略。</param>
    /// <param name="priority">优先级，数值越小越靠前。</param>
    /// <returns>成功 true；参数非法、自挂、cycle、child 已有 parent 时返回 false。</returns>
    public static bool Attach(
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

        if (ChildToParent.ContainsKey(childId))
        {
            return false;
        }

        if (WouldCreateCycle(parentId, childId))
        {
            return false;
        }

        var link = new LifecycleLink(parentId, childId, destroyPolicy, priority);
        ChildToLink[childId] = link;
        ChildToParent[childId] = parentId;
        if (!ParentToLinks.TryGetValue(parentId, out var siblings))
        {
            siblings = new List<LifecycleLink>();
            ParentToLinks[parentId] = siblings;
        }

        siblings.Add(link);
        WorldEvents.World.Publish(new LifecycleChildAttached(parentId, childId, destroyPolicy));
        return true;
    }

    /// <summary>
    /// 断开 <paramref name="parentId"/> 与 <paramref name="childId"/> 之间的 lifecycle link。
    /// </summary>
    public static bool Detach(EntityId parentId, EntityId childId)
    {
        if (parentId.IsEmpty || childId.IsEmpty)
        {
            return false;
        }

        if (!ChildToLink.TryGetValue(childId, out var link) || !link.ParentEntityId.Equals(parentId))
        {
            return false;
        }

        ChildToLink.Remove(childId);
        ChildToParent.Remove(childId);
        if (ParentToLinks.TryGetValue(parentId, out var siblings))
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
                ParentToLinks.Remove(parentId);
            }
        }

        WorldEvents.World.Publish(new LifecycleChildDetached(parentId, childId, link.DestroyPolicy));
        return true;
    }

    /// <summary>
    /// 判断 <paramref name="parentId"/> 与 <paramref name="childId"/> 之间是否存在 lifecycle link。
    /// </summary>
    public static bool IsAttached(EntityId parentId, EntityId childId)
    {
        if (parentId.IsEmpty || childId.IsEmpty)
        {
            return false;
        }

        return ChildToParent.TryGetValue(childId, out var existing) && existing.Equals(parentId);
    }

    /// <summary>
    /// 获取 <paramref name="childId"/> 的 lifecycle parent；无 parent 返回 <see cref="EntityId.Empty"/>。
    /// </summary>
    public static EntityId GetParentEntityId(EntityId childId)
    {
        if (childId.IsEmpty)
        {
            return EntityId.Empty;
        }

        return ChildToParent.TryGetValue(childId, out var parent) ? parent : EntityId.Empty;
    }

    /// <summary>
    /// 获取 <paramref name="parentId"/> 下的全部 lifecycle child link 快照。
    /// </summary>
    public static IReadOnlyList<LifecycleLink> GetChildren(EntityId parentId)
    {
        if (parentId.IsEmpty || !ParentToLinks.TryGetValue(parentId, out var siblings))
        {
            return Array.Empty<LifecycleLink>();
        }

        return new List<LifecycleLink>(siblings);
    }

    /// <summary>
    /// 获取 <paramref name="parentId"/> 下的全部 lifecycle child EntityId 快照。
    /// </summary>
    public static IReadOnlyList<EntityId> GetChildEntityIds(EntityId parentId)
    {
        if (parentId.IsEmpty || !ParentToLinks.TryGetValue(parentId, out var siblings) || siblings.Count == 0)
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

    /// <summary>
    /// 解除 <paramref name="entityId"/> 参与的所有 lifecycle link：
    /// 作为 child 与 parent 的 link 都会被移除（每条仍会触发 Detached 事件）。
    /// </summary>
    public static void DetachAll(EntityId entityId)
    {
        if (entityId.IsEmpty)
        {
            return;
        }

        // 1) 作为 child：从其 lifecycle parent detach。
        if (ChildToParent.TryGetValue(entityId, out var parentId))
        {
            Detach(parentId, entityId);
        }

        // 2) 作为 parent：遍历所有 child 并 detach。
        if (ParentToLinks.TryGetValue(entityId, out var children) && children.Count > 0)
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

    /// <summary>
    /// 清空全部 lifecycle link，不触发事件。
    /// </summary>
    public static void Clear()
    {
        ChildToLink.Clear();
        ParentToLinks.Clear();
        ChildToParent.Clear();
    }

    private static bool WouldCreateCycle(EntityId parentId, EntityId childId)
    {
        // 沿 parent 现有 lifecycle 链向上回查；若回到 childId 即形成环。
        var current = parentId;
        for (var depth = 0; depth < 128; depth++)
        {
            if (!ChildToParent.TryGetValue(current, out var next))
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
