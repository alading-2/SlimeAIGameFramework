using SkilmeAI.GameOS.Runtime.Data;

namespace SkilmeAI.GameOS.Runtime.Event;

/// <summary>
/// Data 运行时事件名和 payload。
/// </summary>
public static partial class GameEventType
{
    /// <summary>
    /// Data 运行时事件。
    /// </summary>
    public static class Data
    {
        /// <summary>任意运行时 Data 值发生变化。</summary>
        public const string PropertyChanged = "data:property_changed";

        /// <summary>运行时 Data 已重置。</summary>
        public const string Reset = "data:reset";

        /// <summary>运行时生命值已变化，兼容旧事件入口。</summary>
        public const string HealthChanged = "unit:health_changed";

        /// <summary>
        /// Data 变更 payload。
        /// </summary>
        public readonly record struct PropertyChangedEventData(DataChangedEventData Change);

        /// <summary>
        /// Data 重置 payload。
        /// </summary>
        public readonly record struct ResetEventData();
    }
}
