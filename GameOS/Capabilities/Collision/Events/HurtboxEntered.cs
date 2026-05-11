using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.Capabilities.Collision.Events;

/// <summary>Hurtbox 重叠开始。GodotCollisionBridge 是 Godot 侧 producer。</summary>
public readonly record struct HurtboxEntered(CollisionContact Contact) : IEntityEvent;
