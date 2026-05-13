using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Capabilities.Feature.Events;

/// <summary>Feature 已启用。FeatureService 是唯一 producer。</summary>
public readonly record struct Enabled(IEntity Owner, IEntity Feature, FeatureDefinition Definition) : IEntityEvent;
