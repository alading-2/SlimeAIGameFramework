using SkilmeAI.GameOS.Runtime.Entity;
using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.Capabilities.Feature.Events;

/// <summary>Feature 已授予。FeatureService 是唯一 producer。</summary>
public readonly record struct Granted(IEntity Owner, IEntity Feature, FeatureDefinition Definition) : IBroadcastEvent;
