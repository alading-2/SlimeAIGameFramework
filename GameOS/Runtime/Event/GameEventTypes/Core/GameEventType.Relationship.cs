namespace SkilmeAI.GameOS.Runtime.Event;

/// <summary>
/// Relationship 运行时事件名和 payload。
/// </summary>
public static partial class GameEventType
{
    /// <summary>
    /// Relationship 运行时事件。
    /// </summary>
    public static class Relationship
    {
        /// <summary>关系已添加。</summary>
        public const string Added = "relationship:added";

        /// <summary>关系已移除。</summary>
        public const string Removed = "relationship:removed";

        /// <summary>
        /// 关系变更 payload。
        /// </summary>
        public readonly record struct ChangedEventData(
            string ParentEntityId,
            string ChildEntityId,
            string RelationType);
    }
}
