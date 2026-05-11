using SkilmeAI.GameOS.Runtime.Entity;
using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.Runtime.Events.Core;

/// <summary>Entity 已注册到 EntityManager。Entity 生命周期唯一 producer 是 EntityManager。</summary>
public readonly record struct EntitySpawned(IEntity Entity) : IBroadcastEvent;
