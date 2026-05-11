using SkilmeAI.GameOS.Runtime.Entity;
using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.Runtime.Events.Core;

/// <summary>Entity 已从 EntityManager 销毁并注销。Entity 生命周期唯一 producer 是 EntityManager。</summary>
public readonly record struct EntityDestroyed(IEntity Entity) : IBroadcastEvent;
