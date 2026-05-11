using SkilmeAI.GameOS.Runtime.Entity;
using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.Capabilities.Unit.Events;

/// <summary>请求表现层停止动画。producer 与 PlayAnimationRequested 对称。</summary>
public readonly record struct StopAnimationRequested(IEntity Entity) : IEntityEvent;
