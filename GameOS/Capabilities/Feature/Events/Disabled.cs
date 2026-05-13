using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Capabilities.Feature.Events;

/// <summary>Feature 已禁用。FeatureService 是唯一 producer。</summary>
public readonly record struct Disabled(IEntity Owner, IEntity Feature, FeatureDefinition Definition) : IEntityEvent;
