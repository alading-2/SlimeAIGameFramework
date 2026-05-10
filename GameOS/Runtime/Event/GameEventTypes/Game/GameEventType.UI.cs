namespace SkilmeAI.GameOS.Runtime.Event;

/// <summary>
/// UI 相关事件定义。
/// </summary>
public static partial class GameEventType
{
    /// <summary>
    /// UI 运行时事件。
    /// </summary>
    public static class UI
    {
        /// <summary>主动技能选中切换。</summary>
        public const string ActiveSkillSelected = "ui:active_skill_selected";

        /// <summary>主动技能选中切换事件数据。</summary>
        public readonly record struct ActiveSkillSelectedEventData(int SlotIndex, string AbilityName);
    }
}
