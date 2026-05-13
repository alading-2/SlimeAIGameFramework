using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Capabilities.Projectile.Events;

/// <summary>投射物已生成。ProjectileTool 是唯一 producer。</summary>
public readonly record struct Spawned(
    IEntity Projectile,
    IEntity Source,
    IEntity? Ability,
    IEntity? Target) : IBroadcastEvent;
