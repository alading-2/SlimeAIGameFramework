using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Capabilities.Feature.Events;

/// <summary>Feature 已激活进入执行流程。FeatureService 是唯一 producer。</summary>
public readonly record struct Activated(FeatureContext Context) : IBroadcastEvent;
