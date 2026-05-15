using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Runtime.Events.Core;

/// <summary>生命周期父子链已挂接。唯一 producer 是 <see cref="LifecycleTree"/>。</summary>
public readonly record struct LifecycleChildAttached(
    EntityId ParentEntityId,
    EntityId ChildEntityId,
    ParentDestroyPolicy DestroyPolicy) : IGlobalEvent;
