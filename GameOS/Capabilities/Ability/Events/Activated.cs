using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.Capabilities.Ability.Events;

/// <summary>技能已激活。AbilityService 是唯一 producer。</summary>
public readonly record struct Activated(AbilityCastContext Context) : IBroadcastEvent;
