using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Capabilities.Effect.Events;

/// <summary>效果实体已生成。EffectTool 是唯一 producer。</summary>
public readonly record struct Spawned(
    IEntity Effect,
    IEntity Source,
    IEntity? Ability,
    IEntity? Target) : IBroadcastEvent;
