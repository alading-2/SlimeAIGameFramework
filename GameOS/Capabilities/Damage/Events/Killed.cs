using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Capabilities.Damage.Events;

/// <summary>实体被击杀。Damage Capability 是唯一 producer。</summary>
public readonly record struct Killed(IEntity Victim, IEntity? Killer, DamageInfo Info) : IBroadcastEvent;
