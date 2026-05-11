using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.Capabilities.Feature.Events;

/// <summary>Feature 已激活进入执行流程。FeatureService 是唯一 producer。</summary>
public readonly record struct Activated(FeatureContext Context) : IBroadcastEvent;
