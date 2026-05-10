using SkilmeAI.GameOS.Capabilities.Ability;

namespace SkilmeAI.GameOS.Runtime.Event;

/// <summary>
/// Ability Capability 事件名和 payload。
/// </summary>
public static partial class GameEventType
{
    /// <summary>
    /// Ability 运行时事件。
    /// </summary>
    public static partial class Ability
    {
        /// <summary>技能已激活。</summary>
        public const string Activated = "ability:activated";

        /// <summary>技能已执行。</summary>
        public const string Executed = "ability:executed";

        /// <summary>技能触发失败。</summary>
        public const string Failed = "ability:failed";

        /// <summary>技能激活 payload。</summary>
        public readonly record struct ActivatedEventData(AbilityCastContext Context);

        /// <summary>技能执行 payload。</summary>
        public readonly record struct ExecutedEventData(AbilityCastContext Context, AbilityExecutedResult Result);

        /// <summary>技能失败 payload。</summary>
        public readonly record struct FailedEventData(AbilityCastContext Context, AbilityTriggerResult Result, string Message);
    }
}
