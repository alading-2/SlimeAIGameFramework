using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Capabilities.Damage.Events;

/// <summary>实体获得治疗。Damage Capability 是唯一 producer。</summary>
public readonly record struct Healed(IEntity Entity, float Amount, HealInfo Info) : IBroadcastEvent;
