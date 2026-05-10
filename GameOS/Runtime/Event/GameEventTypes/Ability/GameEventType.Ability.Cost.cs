namespace SkilmeAI.GameOS.Runtime.Event;

/// <summary>
/// Ability 消耗相关事件定义。
/// </summary>
public static partial class GameEventType
{
    /// <summary>
    /// Ability 运行时事件。
    /// </summary>
    public static partial class Ability
    {
        /// <summary>请求消耗成本。</summary>
        public const string ConsumeCost = "ability:consume_cost";

        /// <summary>成本消耗完成事件。</summary>
        public const string CostConsumed = "ability:cost_consumed";

        /// <summary>消耗成本请求事件数据。</summary>
        public readonly record struct ConsumeCostEventData(EventContext Context);

        /// <summary>成本消耗完成事件数据。</summary>
        public readonly record struct CostConsumedEventData(string CostType, float Amount);
    }
}
