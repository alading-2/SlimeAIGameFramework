using SkilmeAI.GameOS.Capabilities.Collision;

namespace SkilmeAI.GameOS.Runtime.Event;

/// <summary>
/// Collision Capability 事件名和 payload。
/// </summary>
public static partial class GameEventType
{
    /// <summary>
    /// Collision 运行时事件。
    /// </summary>
    public static class Collision
    {
        /// <summary>碰撞进入事件。</summary>
        public const string Entered = "collision:entered";

        /// <summary>碰撞离开事件。</summary>
        public const string Exited = "collision:exited";

        /// <summary>受击区进入事件。</summary>
        public const string HurtboxEntered = "collision:hurtbox_entered";

        /// <summary>受击区离开事件。</summary>
        public const string HurtboxExited = "collision:hurtbox_exited";

        /// <summary>碰撞进入 payload。</summary>
        public readonly record struct EnteredEventData(CollisionContact Contact);

        /// <summary>碰撞离开 payload。</summary>
        public readonly record struct ExitedEventData(CollisionContact Contact);

        /// <summary>受击区进入 payload。</summary>
        public readonly record struct HurtboxEnteredEventData(CollisionContact Contact);

        /// <summary>受击区离开 payload。</summary>
        public readonly record struct HurtboxExitedEventData(CollisionContact Contact);
    }
}
