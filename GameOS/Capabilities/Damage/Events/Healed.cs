using SkilmeAI.GameOS.Runtime.Entity;
using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.Capabilities.Damage.Events;

/// <summary>实体获得治疗。Damage Capability 是唯一 producer。</summary>
public readonly record struct Healed(IEntity Entity, float Amount, HealInfo Info) : IBroadcastEvent;
