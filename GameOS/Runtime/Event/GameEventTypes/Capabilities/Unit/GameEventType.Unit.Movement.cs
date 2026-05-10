using SkilmeAI.GameOS.Capabilities.Movement;
using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.Runtime.Event;

/// <summary>
/// Unit 运动兼容事件定义。
/// </summary>
public static partial class GameEventType
{
    /// <summary>
    /// Unit 表现层事件。
    /// </summary>
    public static partial class Unit
    {
        /// <summary>运动开始或切换。</summary>
        public const string MovementStarted = "unit:movement:started";

        /// <summary>运动完成。</summary>
        public const string MovementCompleted = "unit:movement:completed";

        /// <summary>运动中有效碰撞。</summary>
        public const string MovementCollision = "unit:movement:collision";

        /// <summary>移动停止请求。</summary>
        public const string MovementStopRequested = "unit:movement:stop_requested";

        /// <summary>朝向控制启动。</summary>
        public const string OrientationStarted = "unit:orientation:started";

        /// <summary>朝向控制停止。</summary>
        public const string OrientationStopped = "unit:orientation:stopped";

        /// <summary>运动开始或切换事件数据。</summary>
        public readonly record struct MovementStartedEventData(MoveMode Mode, MovementParams Params);

        /// <summary>运动完成事件数据。</summary>
        public readonly record struct MovementCompletedEventData(
            MoveMode Mode,
            float ElapsedTime,
            float TraveledDistance,
            MovementStopReason Reason,
            IEntity? CollisionTarget = null);

        /// <summary>运动中有效碰撞事件数据。</summary>
        public readonly record struct MovementCollisionEventData(
            MoveMode Mode,
            IEntity? Target = null,
            int CollisionCount = 0,
            bool WillStop = false);

        /// <summary>朝向控制启动事件数据。</summary>
        public readonly record struct OrientationStartedEventData(
            OrientationParams Params,
            bool StopWithMovement = false);

        /// <summary>朝向控制停止事件数据。</summary>
        public readonly record struct OrientationStoppedEventData(MovementStopReason Reason = MovementStopReason.External);

        /// <summary>移动停止请求事件数据。</summary>
        public readonly record struct MovementStopRequestedEventData(
            MovementStopReason Reason = MovementStopReason.External,
            bool EmitCompletedEvent = true,
            MoveMode NextMode = MoveMode.None,
            IEntity? CollisionTarget = null,
            bool DestroyEntity = false);
    }
}
