using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Capabilities.Unit.Events;

/// <summary>请求表现层播放动画。AttackService / Ability 等是 producer；GodotUnitAnimationComponent 是 consumer。</summary>
public readonly record struct PlayAnimationRequested(
    IEntity Entity,
    string AnimationName,
    bool ForceRestart = false,
    float Duration = -1f) : IEntityEvent;
