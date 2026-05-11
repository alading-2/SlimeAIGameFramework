using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.Capabilities.Ability.Events;

/// <summary>技能触发失败。AbilityService 是唯一 producer。</summary>
public readonly record struct Failed(
    AbilityCastContext Context,
    AbilityTriggerResult Result,
    string Message) : IBroadcastEvent;
