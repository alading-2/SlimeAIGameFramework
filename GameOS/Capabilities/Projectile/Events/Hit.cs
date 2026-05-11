using SkilmeAI.GameOS.Capabilities.Damage;
using SkilmeAI.GameOS.Capabilities.Movement;
using SkilmeAI.GameOS.Runtime.Entity;
using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.Capabilities.Projectile.Events;

/// <summary>投射物命中目标。ProjectileTool 是唯一 producer。</summary>
public readonly record struct Hit(
    IEntity Projectile,
    IEntity Source,
    IEntity Target,
    MovementCollisionContext Collision,
    DamageResult Damage) : IBroadcastEvent;
