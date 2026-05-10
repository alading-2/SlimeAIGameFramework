using SkilmeAI.GameOS.Capabilities.Feature;
using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.Runtime.Event;

/// <summary>
/// Feature Capability 事件名和 payload。
/// </summary>
public static partial class GameEventType
{
    /// <summary>
    /// Feature 运行时事件。
    /// </summary>
    public static class Feature
    {
        /// <summary>Feature 已授予。</summary>
        public const string Granted = "feature:granted";

        /// <summary>Feature 已移除。</summary>
        public const string Removed = "feature:removed";

        /// <summary>Feature 已启用。</summary>
        public const string Enabled = "feature:enabled";

        /// <summary>Feature 已禁用。</summary>
        public const string Disabled = "feature:disabled";

        /// <summary>Feature 已激活。</summary>
        public const string Activated = "feature:activated";

        /// <summary>Feature 已执行。</summary>
        public const string Executed = "feature:executed";

        /// <summary>Feature 已结束。</summary>
        public const string Ended = "feature:ended";

        /// <summary>Feature 变更 payload。</summary>
        public readonly record struct ChangedEventData(IEntity Owner, IEntity Feature, FeatureDefinition Definition);

        /// <summary>Feature 上下文 payload。</summary>
        public readonly record struct ContextEventData(FeatureContext Context);

        /// <summary>Feature 结束 payload。</summary>
        public readonly record struct EndedEventData(FeatureContext Context, FeatureEndReason Reason);
    }
}
