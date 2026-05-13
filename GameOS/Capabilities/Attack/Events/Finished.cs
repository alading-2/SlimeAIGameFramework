using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Capabilities.Attack.Events;

/// <summary>攻击已完成。AttackService 是唯一 producer。</summary>
public readonly record struct Finished(IEntity Attacker, IEntity? Target, bool DidHit) : IEntityEvent;
