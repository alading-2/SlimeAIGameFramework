using SlimeAI.GameOS.Runtime.Data;
using SlimeAI.GameOS.Runtime.Event;
using SlimeAI.GameOS.Runtime.World;

namespace SlimeAI.GameOS.Runtime.Entity;

/// <summary>
/// 纯 C# Entity，用于 GameOS 测试、工具和非场景运行时代码。
/// </summary>
public sealed class RuntimeEntity : IEntity
{
    /// <summary>
    /// 创建带局部 Data 和 EventBus 的实体。
    /// </summary>
    /// <param name="entityId">稳定运行时实体 Id；不允许 <see cref="Entity.EntityId.Empty"/>。</param>
    /// <param name="catalog">可选 DataCatalog；为空时使用框架默认 catalog。</param>
    public RuntimeEntity(EntityId entityId, DataCatalog? catalog = null)
        : this(entityId, catalog, WorldEvents.World)
    {
    }

    /// <summary>
    /// 创建带指定 world bus 路由的实体。
    /// </summary>
    /// <param name="entityId">稳定运行时实体 Id；不允许 <see cref="Entity.EntityId.Empty"/>。</param>
    /// <param name="catalog">可选 DataCatalog；为空时使用框架默认 catalog。</param>
    /// <param name="worldRouter">Broadcast 事件转发到所属 world bus 的路由器。</param>
    public RuntimeEntity(EntityId entityId, DataCatalog? catalog, IWorldEventBusRouter? worldRouter)
    {
        EntityId = entityId;
        Events = new EntityEventBus($"entity:{entityId.Value}", worldRouter);
        Data = catalog == null
            ? new Data.Data(new EventDataChangeSink(Events))
            : new Data.Data(catalog, new EventDataChangeSink(Events));
    }

    /// <inheritdoc />
    public EntityId EntityId { get; }

    /// <inheritdoc />
    public Data.Data Data { get; }

    /// <inheritdoc />
    public IEventBus Events { get; }
}
