using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Capabilities.Feature.Events;

/// <summary>Feature 已授予。FeatureService 是唯一 producer。</summary>
public readonly record struct Granted(IEntity Owner, IEntity Feature, FeatureDefinition Definition) : IBroadcastEvent;
