using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.Runtime.Event;

/// <summary>
/// Unit 表现层事件名和 payload。
/// </summary>
public static partial class GameEventType
{
    /// <summary>
    /// Unit 表现层事件。
    /// </summary>
    public static class Unit
    {
        /// <summary>请求播放动画。</summary>
        public const string PlayAnimationRequested = "unit:play_animation_requested";

        /// <summary>请求停止当前动画并回到待机。</summary>
        public const string StopAnimationRequested = "unit:stop_animation_requested";

        /// <summary>动画播放完成。</summary>
        public const string AnimationFinished = "unit:animation_finished";

        /// <summary>请求播放动画 payload。</summary>
        public readonly record struct PlayAnimationRequestedEventData(
            IEntity Entity,
            string AnimationName,
            bool ForceRestart = false,
            float Duration = -1f);

        /// <summary>请求停止动画 payload。</summary>
        public readonly record struct StopAnimationRequestedEventData(IEntity Entity);

        /// <summary>动画播放完成 payload。</summary>
        public readonly record struct AnimationFinishedEventData(IEntity Entity, string AnimationName);
    }
}
