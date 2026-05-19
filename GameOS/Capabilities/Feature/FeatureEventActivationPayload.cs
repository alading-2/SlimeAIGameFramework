using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Capabilities.Feature;

/// <summary>
/// OnEvent 自动触发时传给 Feature handler 的类型化事件载荷。
/// </summary>
/// <typeparam name="TEvent">事件 payload 类型。</typeparam>
public sealed record FeatureEventActivationPayload<TEvent>(TEvent Event) : IFeatureActivationPayload
    where TEvent : struct, IEntityEvent;
