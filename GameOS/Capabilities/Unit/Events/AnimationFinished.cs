using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Capabilities.Unit.Events;

/// <summary>动画播放完成。GodotUnitAnimationComponent 是唯一 producer。</summary>
public readonly record struct AnimationFinished(IEntity Entity, string AnimationName) : IEntityEvent;
