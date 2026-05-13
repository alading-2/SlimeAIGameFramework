using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Capabilities.Feature.Events;

/// <summary>Feature 已执行一次。FeatureService 是唯一 producer。</summary>
public readonly record struct Executed(FeatureContext Context) : IBroadcastEvent;
