using System.Collections.Generic;
using SlimeAI.GameOS.Runtime.World;

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
        return RuntimeWorld.Default.Lifecycle.Attach(parentId, childId, destroyPolicy, priority);
    }

    /// <summary>
    /// 断开 <paramref name="parentId"/> 与 <paramref name="childId"/> 之间的 lifecycle link。
    /// </summary>
    public static bool Detach(EntityId parentId, EntityId childId)
    {
        return RuntimeWorld.Default.Lifecycle.Detach(parentId, childId);
    }

    /// <summary>
    /// 判断 <paramref name="parentId"/> 与 <paramref name="childId"/> 之间是否存在 lifecycle link。
    /// </summary>
    public static bool IsAttached(EntityId parentId, EntityId childId)
    {
        return RuntimeWorld.Default.Lifecycle.IsAttached(parentId, childId);
    }

    /// <summary>
    /// 获取 <paramref name="childId"/> 的 lifecycle parent；无 parent 返回 <see cref="EntityId.Empty"/>。
    /// </summary>
    public static EntityId GetParentEntityId(EntityId childId)
    {
        return RuntimeWorld.Default.Lifecycle.GetParentEntityId(childId);
    }

    /// <summary>
    /// 获取 <paramref name="parentId"/> 下的全部 lifecycle child link 快照。
    /// </summary>
    public static IReadOnlyList<LifecycleLink> GetChildren(EntityId parentId)
    {
        return RuntimeWorld.Default.Lifecycle.GetChildren(parentId);
    }

    /// <summary>
    /// 获取 <paramref name="parentId"/> 下的全部 lifecycle child EntityId 快照。
    /// </summary>
    public static IReadOnlyList<EntityId> GetChildEntityIds(EntityId parentId)
    {
        return RuntimeWorld.Default.Lifecycle.GetChildEntityIds(parentId);
    }

    /// <summary>
    /// 解除 <paramref name="entityId"/> 参与的所有 lifecycle link：
    /// 作为 child 与 parent 的 link 都会被移除（每条仍会触发 Detached 事件）。
    /// </summary>
    public static void DetachAll(EntityId entityId)
    {
        RuntimeWorld.Default.Lifecycle.DetachAll(entityId);
    }

    /// <summary>
    /// 清空全部 lifecycle link，不触发事件。
    /// </summary>
    public static void Clear()
    {
        RuntimeWorld.Default.Lifecycle.Clear();
    }
}
