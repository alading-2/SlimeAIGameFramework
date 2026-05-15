using System.Collections.Generic;
using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.Runtime.World;

/// <summary>
/// RuntimeWorld 内部的 lifecycle 树句柄。
/// </summary>
public interface ILifecycleTree
{
    /// <summary>挂接 lifecycle parent-child 关系。</summary>
    bool Attach(
        EntityId parentId,
        EntityId childId,
        ParentDestroyPolicy destroyPolicy = ParentDestroyPolicy.DestroyRecursively,
        int priority = 0);

    /// <summary>断开 lifecycle parent-child 关系。</summary>
    bool Detach(EntityId parentId, EntityId childId);

    /// <summary>判断 parent-child 是否已挂接。</summary>
    bool IsAttached(EntityId parentId, EntityId childId);

    /// <summary>获取 child 的 parent；无 parent 返回 EntityId.Empty。</summary>
    EntityId GetParentEntityId(EntityId childId);

    /// <summary>获取 parent 下的 child link 快照。</summary>
    IReadOnlyList<LifecycleLink> GetChildren(EntityId parentId);

    /// <summary>获取 parent 下的 child EntityId 快照。</summary>
    IReadOnlyList<EntityId> GetChildEntityIds(EntityId parentId);

    /// <summary>解除指定 entity 参与的所有 lifecycle link。</summary>
    void DetachAll(EntityId entityId);

    /// <summary>清空全部 lifecycle link。</summary>
    void Clear();
}
