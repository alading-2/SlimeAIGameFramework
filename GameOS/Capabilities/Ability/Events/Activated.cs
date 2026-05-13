using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Capabilities.Ability.Events;

/// <summary>技能已激活。AbilityService 是唯一 producer。</summary>
public readonly record struct Activated(AbilityCastContext Context) : IBroadcastEvent;
