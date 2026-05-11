using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.Capabilities.Feature.Events;

/// <summary>Feature 已结束。FeatureService 是唯一 producer。</summary>
public readonly record struct Ended(FeatureContext Context, FeatureEndReason Reason) : IBroadcastEvent;
