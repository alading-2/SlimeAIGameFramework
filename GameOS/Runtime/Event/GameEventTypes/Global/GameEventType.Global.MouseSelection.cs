using System.Collections.Generic;
using Godot;
using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.Runtime.Event;

/// <summary>
/// Global 鼠标选择相关事件定义。
/// </summary>
public static partial class GameEventType
{
    /// <summary>
    /// 全局运行时事件。
    /// </summary>
    public static partial class Global
    {
        /// <summary>鼠标选择交互类型。</summary>
        public enum MouseSelectionInteractionKind
        {
            /// <summary>单点点击选择。</summary>
            Click = 0,

            /// <summary>框选区域选择。</summary>
            Box = 1
        }

        /// <summary>鼠标选择命中来源。</summary>
        public enum MouseSelectionHitKind
        {
            /// <summary>没有命中。</summary>
            None = 0,

            /// <summary>物理点选命中。</summary>
            PhysicsPoint = 1,

            /// <summary>距离回退命中。</summary>
            DistanceFallback = 2,

            /// <summary>框选矩形命中。</summary>
            BoxRect = 3
        }

        /// <summary>鼠标选择完成。</summary>
        public const string MouseSelectionCompleted = "global:mouse_selection:completed";

        /// <summary>鼠标框选预览更新。</summary>
        public const string MouseSelectionPreviewUpdated = "global:mouse_selection:preview_updated";

        /// <summary>鼠标选择未命中。</summary>
        public const string MouseSelectionMissed = "global:mouse_selection:missed";

        /// <summary>鼠标选择完成事件数据。</summary>
        public readonly record struct MouseSelectionCompletedEventData(
            IReadOnlyList<IEntity> Entities,
            IEntity? PrimaryEntity,
            Vector2 ScreenPosition,
            Vector2 WorldPosition,
            Rect2 ScreenRect,
            MouseSelectionHitKind HitKind,
            MouseSelectionInteractionKind InteractionKind);

        /// <summary>鼠标框选预览更新事件数据。</summary>
        public readonly record struct MouseSelectionPreviewUpdatedEventData(
            Vector2 StartScreenPosition,
            Vector2 CurrentScreenPosition,
            Rect2 ScreenRect);

        /// <summary>鼠标选择未命中事件数据。</summary>
        public readonly record struct MouseSelectionMissedEventData(
            Vector2 ScreenPosition,
            Vector2 WorldPosition,
            Rect2 ScreenRect,
            MouseSelectionInteractionKind InteractionKind);
    }
}
