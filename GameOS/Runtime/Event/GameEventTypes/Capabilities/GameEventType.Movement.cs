using SkilmeAI.GameOS.Capabilities.Movement;
using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.Runtime.Event;

/// <summary>
/// Movement Capability 事件名和 payload。
/// </summary>
public static partial class GameEventType
{
    /// <summary>
    /// Movement 运行时事件。
    /// </summary>
    public static class Movement
    {
        /// <summary>实体移动已开始。</summary>
        public const string Started = "movement:started";

        /// <summary>实体移动已停止。</summary>
        public const string Stopped = "movement:stopped";

        /// <summary>实体移动发生有效碰撞。</summary>
        public const string Collision = "movement:collision";

        /// <summary>
        /// 移动开始 payload。
        /// </summary>
        public readonly record struct StartedEventData(IEntity Entity, MovementParams Params);

        /// <summary>
        /// 移动停止 payload。
        /// </summary>
        public readonly record struct StoppedEventData(MovementStopContext Context);

        /// <summary>
        /// 移动碰撞 payload。
        /// </summary>
        public readonly record struct CollisionEventData(MovementCollisionContext Context);
    }
}
