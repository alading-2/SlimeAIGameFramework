using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Capabilities.Damage.Events;

/// <summary>实体闪避了一次伤害。Damage Capability 是唯一 producer。</summary>
public readonly record struct Dodged(IEntity Victim, IEntity? Attacker, DamageInfo Info) : IBroadcastEvent;
