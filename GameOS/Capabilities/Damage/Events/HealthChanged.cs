using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Capabilities.Damage.Events;

/// <summary>Victim 的 HP 发生变化。Damage Capability 是唯一 producer。</summary>
public readonly record struct HealthChanged(IEntity Victim, float OldHp, float NewHp) : IBroadcastEvent;
