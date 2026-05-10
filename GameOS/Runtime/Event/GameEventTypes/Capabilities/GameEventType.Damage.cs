using SkilmeAI.GameOS.Capabilities.Damage;
using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.Runtime.Event;

/// <summary>
/// Damage Capability 事件名和 payload。
/// </summary>
public static partial class GameEventType
{
    /// <summary>
    /// Damage 运行时事件。
    /// </summary>
    public static class Damage
    {
        /// <summary>生命值发生变化。</summary>
        public const string HealthChanged = "damage:health_changed";

        /// <summary>实体受到伤害。</summary>
        public const string Damaged = "damage:damaged";

        /// <summary>实体闪避伤害。</summary>
        public const string Dodged = "damage:dodged";

        /// <summary>实体获得治疗。</summary>
        public const string Healed = "damage:healed";

        /// <summary>实体被击杀。</summary>
        public const string Killed = "damage:killed";

        /// <summary>生命值变化 payload。</summary>
        public readonly record struct HealthChangedEventData(IEntity Entity, float OldHp, float NewHp);

        /// <summary>受伤 payload。</summary>
        public readonly record struct DamagedEventData(DamageInfo Info);

        /// <summary>闪避 payload。</summary>
        public readonly record struct DodgedEventData(IEntity Victim, IEntity? Attacker, DamageInfo Info);

        /// <summary>治疗 payload。</summary>
        public readonly record struct HealedEventData(IEntity Entity, float Amount, HealInfo Info);

        /// <summary>击杀 payload。</summary>
        public readonly record struct KilledEventData(IEntity Victim, IEntity? Killer, DamageInfo Info);
    }
}
