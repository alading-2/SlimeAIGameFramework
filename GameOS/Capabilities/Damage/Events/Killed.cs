using SkilmeAI.GameOS.Runtime.Entity;
using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.Capabilities.Damage.Events;

/// <summary>实体被击杀。Damage Capability 是唯一 producer。</summary>
public readonly record struct Killed(IEntity Victim, IEntity? Killer, DamageInfo Info) : IBroadcastEvent;
