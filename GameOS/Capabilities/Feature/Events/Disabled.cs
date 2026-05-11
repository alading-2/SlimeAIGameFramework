using SkilmeAI.GameOS.Runtime.Entity;
using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.Capabilities.Feature.Events;

/// <summary>Feature 已禁用。FeatureService 是唯一 producer。</summary>
public readonly record struct Disabled(IEntity Owner, IEntity Feature, FeatureDefinition Definition) : IEntityEvent;
