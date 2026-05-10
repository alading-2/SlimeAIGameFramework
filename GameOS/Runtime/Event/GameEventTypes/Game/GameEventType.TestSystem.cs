using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.Runtime.Event;

/// <summary>
/// TestSystem 局部事件定义。
/// </summary>
public static partial class GameEventType
{
    /// <summary>
    /// TestSystem 运行时事件。
    /// </summary>
    public static class TestSystem
    {
        /// <summary>TestSystem 当前选中实体变化。</summary>
        public const string SelectionChanged = "test_system:selection_changed";

        /// <summary>TestSystem 当前选中实体变化事件数据。</summary>
        public readonly record struct SelectionChangedEventData(IEntity? Entity);
    }
}
