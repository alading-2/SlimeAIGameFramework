using Godot;
using SkilmeAI.GameOS.Capabilities.Ability;

namespace SkilmeAI.GameOS.Runtime.Event;

/// <summary>
/// 瞄准系统相关事件定义。
/// </summary>
public static partial class GameEventType
{
    /// <summary>
    /// Targeting 运行时事件。
    /// </summary>
    public static class Targeting
    {
        /// <summary>开始瞄准。</summary>
        public const string StartTargeting = "targeting:start";

        /// <summary>瞄准确认。</summary>
        public const string TargetConfirmed = "targeting:confirmed";

        /// <summary>瞄准取消。</summary>
        public const string TargetCancelled = "targeting:cancelled";

        /// <summary>瞄准结束。</summary>
        public const string TargetingEnded = "targeting:ended";

        /// <summary>开始瞄准事件数据。</summary>
        public readonly record struct StartTargetingEventData(AbilityCastContext Context);

        /// <summary>瞄准确认事件数据。</summary>
        public readonly record struct TargetConfirmedEventData(Vector2 TargetPosition);

        /// <summary>瞄准取消事件数据。</summary>
        public readonly record struct TargetCancelledEventData();

        /// <summary>瞄准结束事件数据。</summary>
        public readonly record struct TargetingEndedEventData(bool WasConfirmed);
    }
}
