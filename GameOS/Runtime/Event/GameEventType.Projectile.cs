using SkilmeAI.GameOS.Runtime.Entity;
using SkilmeAI.GameOS.Capabilities.Damage;
using SkilmeAI.GameOS.Capabilities.Movement;

namespace SkilmeAI.GameOS.Runtime.Event;

/// <summary>
/// Projectile Capability 事件名和 payload。
/// </summary>
public static partial class GameEventType
{
    /// <summary>
    /// Projectile 运行时事件。
    /// </summary>
    public static class Projectile
    {
        /// <summary>投射物已生成。</summary>
        public const string Spawned = "projectile:spawned";

        /// <summary>投射物命中目标。</summary>
        public const string Hit = "projectile:hit";

        /// <summary>投射物生成 payload。</summary>
        public readonly record struct SpawnedEventData(
            IEntity Projectile,
            IEntity Source,
            IEntity? Ability,
            IEntity? Target);

        /// <summary>投射物命中 payload。</summary>
        public readonly record struct HitEventData(
            IEntity Projectile,
            IEntity Source,
            IEntity Target,
            MovementCollisionContext Collision,
            DamageResult Damage);
    }
}
