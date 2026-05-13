using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Capabilities.Damage.Events;

/// <summary>实体受到伤害。Damage Capability 是唯一 producer。</summary>
public readonly record struct Damaged(DamageInfo Info) : IBroadcastEvent;
