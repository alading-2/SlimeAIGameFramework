using SkilmeAI.GameOS.Runtime.Data;
using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.Runtime.Entity;

/// <summary>
/// 纯 C# Entity，用于 GameOS 测试、工具和非场景运行时代码。
/// </summary>
public sealed class RuntimeEntity : IEntity
{
    /// <summary>
    /// 创建带局部 Data 和 EventBus 的实体。
    /// </summary>
    /// <param name="entityId">稳定运行时实体 Id。</param>
    /// <param name="catalog">可选 DataCatalog；为空时使用框架默认 catalog。</param>
    public RuntimeEntity(string entityId, DataCatalog? catalog = null)
    {
        EntityId = entityId;
        Events = new EntityEventBus($"entity:{entityId}", WorldEvents.World);
        Data = catalog == null
            ? new Data.Data(new EventDataChangeSink(Events))
            : new Data.Data(catalog, new EventDataChangeSink(Events));
    }

    /// <inheritdoc />
    public string EntityId { get; }

    /// <inheritdoc />
    public Data.Data Data { get; }

    /// <inheritdoc />
    public IEventBus Events { get; }
}
