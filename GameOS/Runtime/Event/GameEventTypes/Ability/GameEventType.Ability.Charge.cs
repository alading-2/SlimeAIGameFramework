namespace SkilmeAI.GameOS.Runtime.Event;

/// <summary>
/// Ability 充能相关事件定义。
/// </summary>
public static partial class GameEventType
{
    /// <summary>
    /// Ability 运行时事件。
    /// </summary>
    public static partial class Ability
    {
        /// <summary>充能恢复。</summary>
        public const string ChargeRestored = "ability:charge_restored";

        /// <summary>使用技能消耗充能事件。</summary>
        public const string ConsumeCharge = "ability:consume_charge";

        /// <summary>请求增加充能事件。</summary>
        public const string AddCharge = "ability:add_charge";

        /// <summary>充能恢复事件数据。</summary>
        public readonly record struct ChargeRestoredEventData(int CurrentCharges, int MaxCharges);

        /// <summary>消耗充能事件数据。</summary>
        public readonly record struct ConsumeChargeEventData(EventContext Context);

        /// <summary>增加充能事件数据。</summary>
        public readonly record struct AddChargeEventData(int Amount);
    }
}
