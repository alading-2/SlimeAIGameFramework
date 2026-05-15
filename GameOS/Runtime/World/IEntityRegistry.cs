using System.Collections.Generic;
using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.Runtime.World;

/// <summary>
/// RuntimeWorld 内部的实体注册表句柄。
/// </summary>
public interface IEntityRegistry
{
    /// <summary>生成并注册一个纯 Runtime Entity。</summary>
    RuntimeEntity Spawn(EntitySpawnConfig config = default);

    /// <summary>注册已有实体。</summary>
    bool Register(IEntity entity);

    /// <summary>按 Id 销毁实体。</summary>
    bool Destroy(EntityId entityId);

    /// <summary>销毁实体实例。</summary>
    bool Destroy(IEntity entity);

    /// <summary>按 Id 查找实体。</summary>
    IEntity? Get(EntityId entityId);

    /// <summary>返回全部实体快照。</summary>
    IReadOnlyList<IEntity> GetAll();

    /// <summary>返回 EntityId 到实体的字典快照。</summary>
    IReadOnlyDictionary<EntityId, IEntity> Snapshot();

    /// <summary>清空实体状态。</summary>
    void Clear();

    /// <summary>为已存在实体绑定 lifecycle parent。</summary>
    bool AttachLifecycleParent(
        EntityId childEntityId,
        EntityId parentEntityId,
        ParentDestroyPolicy parentDestroyPolicy = ParentDestroyPolicy.DestroyRecursively);
}
