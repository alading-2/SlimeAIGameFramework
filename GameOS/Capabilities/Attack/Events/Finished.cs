using SkilmeAI.GameOS.Runtime.Entity;
using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.Capabilities.Attack.Events;

/// <summary>攻击已完成。AttackService 是唯一 producer。</summary>
public readonly record struct Finished(IEntity Attacker, IEntity? Target, bool DidHit) : IEntityEvent;
