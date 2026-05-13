using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Capabilities.Collision.Events;

/// <summary>两个 CollisionComponent 重叠结束。CollisionSystem 是唯一 producer。</summary>
public readonly record struct Exited(CollisionContact Contact) : IEntityEvent;
