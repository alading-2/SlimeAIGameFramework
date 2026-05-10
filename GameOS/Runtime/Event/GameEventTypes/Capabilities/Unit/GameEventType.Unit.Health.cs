using SkilmeAI.GameOS.Capabilities.Damage;
using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.Runtime.Event;

/// <summary>
/// Unit 生命和伤害兼容事件定义。
/// </summary>
public static partial class GameEventType
{
    /// <summary>
    /// Unit 表现层事件。
    /// </summary>
    public static partial class Unit
    {
        /// <summary>单位受到伤害。</summary>
        public const string Damaged = "unit:damaged";

        /// <summary>单位闪避成功。</summary>
        public const string Dodged = "unit:dodged";

        /// <summary>请求治疗。</summary>
        public const string HealRequest = "unit:heal_request";

        /// <summary>治疗已应用。</summary>
        public const string HealApplied = "unit:heal_applied";

        /// <summary>单位被击杀。</summary>
        public const string Killed = "unit:killed";

        /// <summary>单位死亡类型。</summary>
        public enum UnitDeathType
        {
            /// <summary>普通死亡。</summary>
            Normal = 0,

            /// <summary>超时死亡。</summary>
            Timeout = 1,

            /// <summary>主动移除。</summary>
            Despawn = 2
        }

        /// <summary>单位受到伤害事件数据。</summary>
        public readonly record struct DamagedEventData(
            IEntity Victim,
            float Amount,
            IEntity? Attacker = null,
            DamageType Type = DamageType.True,
            bool IsCritical = false);

        /// <summary>单位闪避事件数据。</summary>
        public readonly record struct DodgedEventData(IEntity Victim, IEntity? Attacker = null);

        /// <summary>请求治疗事件数据。</summary>
        public readonly record struct HealRequestEventData(float Amount, HealSource Source = HealSource.Direct);

        /// <summary>治疗已应用事件数据。</summary>
        public readonly record struct HealAppliedEventData(
            IEntity Victim,
            float RequestedAmount,
            float ActualAmount,
            HealSource Source);

        /// <summary>单位被击杀事件数据。</summary>
        public readonly record struct KilledEventData(
            IEntity? Victim,
            IEntity? Killer,
            UnitDeathType DeathType = UnitDeathType.Normal,
            DamageType DamageType = DamageType.True);
    }
}
