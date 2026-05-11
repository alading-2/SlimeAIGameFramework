using SkilmeAI.GameOS.Runtime.Entity;
using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.Capabilities.Projectile.Events;

/// <summary>投射物已生成。ProjectileTool 是唯一 producer。</summary>
public readonly record struct Spawned(
    IEntity Projectile,
    IEntity Source,
    IEntity? Ability,
    IEntity? Target) : IBroadcastEvent;
