namespace SkilmeAI.GameOS.Runtime.Event;

/// <summary>
/// Ability 执行流程兼容事件定义。
/// </summary>
public static partial class GameEventType
{
    /// <summary>
    /// Ability 运行时事件。
    /// </summary>
    public static partial class Ability
    {
        /// <summary>技能被取消。</summary>
        public const string Cancelled = "ability:cancelled";

        /// <summary>技能取消事件数据。</summary>
        public readonly record struct CancelledEventData(string Reason);
    }
}
