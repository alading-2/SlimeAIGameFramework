using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Capabilities.Attack.Events;

/// <summary>请求中断攻击。AI 或玩家输入层是 producer。</summary>
public readonly record struct CancelRequested(
    IEntity Attacker,
    AttackCancelReason Reason) : IEntityEvent;
