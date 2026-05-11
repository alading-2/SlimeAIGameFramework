using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.Capabilities.Collision.Events;

/// <summary>两个 CollisionComponent 重叠区间开始。CollisionSystem 是唯一 producer。</summary>
public readonly record struct Entered(CollisionContact Contact) : IEntityEvent;
