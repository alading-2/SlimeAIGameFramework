using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Capabilities.Collision.Events;

/// <summary>Hurtbox 重叠结束。GodotCollisionBridge 是 Godot 侧 producer。</summary>
public readonly record struct HurtboxExited(CollisionContact Contact) : IEntityEvent;
