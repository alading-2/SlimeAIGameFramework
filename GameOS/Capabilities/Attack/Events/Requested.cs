using SkilmeAI.GameOS.Capabilities.Movement;
using SkilmeAI.GameOS.Runtime.Entity;
using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.Capabilities.Attack.Events;

/// <summary>请求发动攻击。AI 决策层是 producer。</summary>
public readonly record struct Requested(
    IEntity Attacker,
    IEntity? Target,
    Vector2Value TargetPosition) : IEntityEvent;
