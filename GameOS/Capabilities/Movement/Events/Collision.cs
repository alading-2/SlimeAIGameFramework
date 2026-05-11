using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.Capabilities.Movement.Events;

/// <summary>实体移动过程中发生有效碰撞。MovementSystem 是唯一 producer。</summary>
public readonly record struct Collision(MovementCollisionContext Context) : IEntityEvent;
