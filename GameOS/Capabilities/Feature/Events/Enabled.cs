using SkilmeAI.GameOS.Runtime.Entity;
using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.Capabilities.Feature.Events;

/// <summary>Feature 已启用。FeatureService 是唯一 producer。</summary>
public readonly record struct Enabled(IEntity Owner, IEntity Feature, FeatureDefinition Definition) : IEntityEvent;
