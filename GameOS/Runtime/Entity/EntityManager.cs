using System.Collections.Generic;
using SlimeAI.GameOS.Runtime.World;

namespace SlimeAI.GameOS.Runtime.Entity;

/// <summary>
/// Entity 注册表和生命周期门面。
/// </summary>
public static class EntityManager
{
    /// <summary>
    /// 生成并注册一个纯 Runtime Entity。
    /// </summary>
    /// <param name="config">生成参数。</param>
    public static RuntimeEntity Spawn(EntitySpawnConfig config = default)
    {
        return RuntimeWorld.Default.Entities.Spawn(config);
    }

    /// <summary>
    /// 注册已有实体。
    /// </summary>
    /// <param name="entity">运行时实体实例。</param>
    public static bool Register(IEntity entity)
    {
        return RuntimeWorld.Default.Entities.Register(entity);
    }

    /// <summary>
    /// 按 Id 销毁实体。
    /// </summary>
    /// <param name="entityId">稳定运行时实体 Id。</param>
    public static bool Destroy(EntityId entityId)
    {
        return RuntimeWorld.Default.Entities.Destroy(entityId);
    }

    /// <summary>
    /// 销毁实体实例。
    /// </summary>
    /// <param name="entity">运行时实体实例。</param>
    public static bool Destroy(IEntity entity)
    {
        return RuntimeWorld.Default.Entities.Destroy(entity);
    }

    /// <summary>
    /// 按 Id 查找实体。
    /// </summary>
    /// <param name="entityId">稳定运行时实体 Id。</param>
    public static IEntity? Get(EntityId entityId)
    {
        return RuntimeWorld.Default.Entities.Get(entityId);
    }

    /// <summary>
    /// 返回所有已注册实体的稳定快照。
    /// </summary>
    public static IReadOnlyList<IEntity> GetAll()
    {
        return RuntimeWorld.Default.Entities.GetAll();
    }

    /// <summary>
    /// 清空实体、lifecycle 树和 world bus 状态。
    /// </summary>
    public static void Clear()
    {
        RuntimeWorld.Default.Entities.Clear();
        RuntimeWorld.Default.Lifecycle.Clear();
        RuntimeWorld.Default.Events.Clear();
    }

    /// <summary>
    /// 为已存在实体绑定 lifecycle parent；薄包装做实体存在性校验，然后委托 <see cref="LifecycleTree.Attach"/>。
    /// </summary>
    /// <param name="childEntityId">子实体 Id。</param>
    /// <param name="parentEntityId">父实体 Id。</param>
    /// <param name="parentDestroyPolicy">父实体销毁策略。</param>
    public static bool AttachLifecycleParent(
        EntityId childEntityId,
        EntityId parentEntityId,
        ParentDestroyPolicy parentDestroyPolicy = ParentDestroyPolicy.DestroyRecursively)
    {
        return RuntimeWorld.Default.Entities.AttachLifecycleParent(childEntityId, parentEntityId, parentDestroyPolicy);
    }
}
