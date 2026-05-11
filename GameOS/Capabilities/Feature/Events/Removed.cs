using SkilmeAI.GameOS.Runtime.Entity;
using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.Capabilities.Feature.Events;

/// <summary>Feature 已移除（含 definition 便于 AI 观察）。FeatureService 是唯一 producer。</summary>
public readonly record struct Removed(IEntity Owner, IEntity Feature, FeatureDefinition Definition) : IBroadcastEvent;
