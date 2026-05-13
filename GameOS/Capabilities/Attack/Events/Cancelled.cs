using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Capabilities.Attack.Events;

/// <summary>攻击已被取消。AttackService 是唯一 producer。</summary>
public readonly record struct Cancelled(IEntity Attacker, AttackCancelReason Reason) : IEntityEvent;
