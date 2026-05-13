using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Capabilities.Attack.Events;

/// <summary>攻击已开始。AttackService 是唯一 producer。</summary>
public readonly record struct Started(IEntity Attacker, IEntity? Target) : IEntityEvent;
