using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Capabilities.Movement.Events;

/// <summary>实体移动已开始。MovementSystem 是唯一 producer。</summary>
public readonly record struct Started(IEntity Entity, MovementParams Params) : IEntityEvent;
