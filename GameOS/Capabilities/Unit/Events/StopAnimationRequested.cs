using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Capabilities.Unit.Events;

/// <summary>请求表现层停止动画。producer 与 PlayAnimationRequested 对称。</summary>
public readonly record struct StopAnimationRequested(IEntity Entity) : IEntityEvent;
