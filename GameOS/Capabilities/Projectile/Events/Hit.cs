using SlimeAI.GameOS.Capabilities.Damage;
using SlimeAI.GameOS.Capabilities.Movement;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Capabilities.Projectile.Events;

/// <summary>投射物命中目标。ProjectileTool 是唯一 producer。</summary>
public readonly record struct Hit(
    IEntity Projectile,
    IEntity Source,
    IEntity Target,
    MovementCollisionContext Collision,
    DamageResult Damage) : IBroadcastEvent;
