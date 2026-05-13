using SlimeAI.GameOS.Capabilities.Movement;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Capabilities.Attack.Events;

/// <summary>请求发动攻击。AI 决策层是 producer。</summary>
public readonly record struct Requested(
    IEntity Attacker,
    IEntity? Target,
    Vector2Value TargetPosition) : IEntityEvent;
