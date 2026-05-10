using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.Runtime.Event;

/// <summary>
/// Effect Capability 事件名和 payload。
/// </summary>
public static partial class GameEventType
{
    /// <summary>
    /// Effect 运行时事件。
    /// </summary>
    public static class Effect
    {
        /// <summary>效果已生成。</summary>
        public const string Spawned = "effect:spawned";

        /// <summary>效果生成 payload。</summary>
        public readonly record struct SpawnedEventData(
            IEntity Effect,
            IEntity Source,
            IEntity? Ability,
            IEntity? Target);
    }
}
