using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Capabilities.Ability.Events;

/// <summary>技能已执行完毕。AbilityService 是唯一 producer。</summary>
public readonly record struct Executed(AbilityCastContext Context, AbilityExecutedResult Result) : IBroadcastEvent;
