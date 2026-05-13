using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Capabilities.Feature.Events;

/// <summary>Feature 已移除（含 definition 便于 AI 观察）。FeatureService 是唯一 producer。</summary>
public readonly record struct Removed(IEntity Owner, IEntity Feature, FeatureDefinition Definition) : IBroadcastEvent;
