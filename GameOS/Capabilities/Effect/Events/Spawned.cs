using SkilmeAI.GameOS.Runtime.Entity;
using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.Capabilities.Effect.Events;

/// <summary>效果实体已生成。EffectTool 是唯一 producer。</summary>
public readonly record struct Spawned(
    IEntity Effect,
    IEntity Source,
    IEntity? Ability,
    IEntity? Target) : IBroadcastEvent;
