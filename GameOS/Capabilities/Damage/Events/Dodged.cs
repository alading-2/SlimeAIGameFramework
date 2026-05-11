using SkilmeAI.GameOS.Runtime.Entity;
using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.Capabilities.Damage.Events;

/// <summary>实体闪避了一次伤害。Damage Capability 是唯一 producer。</summary>
public readonly record struct Dodged(IEntity Victim, IEntity? Attacker, DamageInfo Info) : IBroadcastEvent;
