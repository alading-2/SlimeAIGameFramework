namespace SlimeAI.GameOS.Runtime.Entity;

using SlimeAI.GameOS.Runtime.Data;

/// <summary>
/// 纯 Runtime Entity 生成参数。
/// </summary>
public readonly record struct EntitySpawnConfig
{
    /// <summary>
    /// 调用方传入的实体 Id；为 <see cref="Entity.EntityId.Empty"/> 时自动生成 GUID 字符串。
    /// </summary>
    public EntityId EntityId { get; init; }

    /// <summary>
    /// 可选 DataCatalog；为空时使用框架默认 catalog。
    /// </summary>
    public DataCatalog? DataCatalog { get; init; }

    /// <summary>
    /// 可选 lifecycle parent 实体 Id；为 <see cref="Entity.EntityId.Empty"/> 时不挂 lifecycle parent。
    /// </summary>
    public EntityId ParentEntityId { get; init; }

    /// <summary>
    /// 父实体销毁时对子实体的处理策略；只有 <see cref="ParentEntityId"/> 非空时生效。
    /// </summary>
    public ParentDestroyPolicy ParentDestroyPolicy { get; init; }
}
