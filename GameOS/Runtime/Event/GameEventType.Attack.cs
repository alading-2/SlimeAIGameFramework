using SkilmeAI.GameOS.Capabilities.Attack;
using SkilmeAI.GameOS.Capabilities.Movement;
using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.Runtime.Event;

/// <summary>
/// Attack Runtime 事件名和 payload。
/// </summary>
public static partial class GameEventType
{
    /// <summary>
    /// 攻击运行时事件。
    /// </summary>
    public static class Attack
    {
        /// <summary>请求发动攻击。</summary>
        public const string Requested = "attack:requested";

        /// <summary>请求中断攻击。</summary>
        public const string CancelRequested = "attack:cancel_requested";

        /// <summary>攻击开始。</summary>
        public const string Started = "attack:started";

        /// <summary>攻击完成。</summary>
        public const string Finished = "attack:finished";

        /// <summary>攻击被取消。</summary>
        public const string Cancelled = "attack:cancelled";

        /// <summary>请求发动攻击 payload。</summary>
        public readonly record struct RequestedEventData(
            IEntity Attacker,
            IEntity? Target,
            Vector2Value TargetPosition);

        /// <summary>请求中断攻击 payload。</summary>
        public readonly record struct CancelRequestedEventData(
            IEntity Attacker,
            AttackCancelReason Reason);

        /// <summary>攻击开始 payload。</summary>
        public readonly record struct StartedEventData(IEntity Attacker, IEntity? Target);

        /// <summary>攻击完成 payload。</summary>
        public readonly record struct FinishedEventData(IEntity Attacker, IEntity? Target, bool DidHit);

        /// <summary>攻击被取消 payload。</summary>
        public readonly record struct CancelledEventData(IEntity Attacker, AttackCancelReason Reason);
    }
}
