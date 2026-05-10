using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.Runtime.Event;

/// <summary>
/// Input 运行时事件名和 payload。
/// </summary>
public static partial class GameEventType
{
    /// <summary>
    /// 输入运行时事件。
    /// </summary>
    public static class Input
    {
        /// <summary>释放当前技能。</summary>
        public const string UseSkill = "input:use_skill";

        /// <summary>切换到上一个技能。</summary>
        public const string PreviousSkill = "input:previous_skill";

        /// <summary>切换到下一个技能。</summary>
        public const string NextSkill = "input:next_skill";

        /// <summary>释放当前技能 payload。</summary>
        public readonly record struct UseSkillEventData(IEntity Entity);

        /// <summary>切换到上一个技能 payload。</summary>
        public readonly record struct PreviousSkillEventData(IEntity Entity);

        /// <summary>切换到下一个技能 payload。</summary>
        public readonly record struct NextSkillEventData(IEntity Entity);
    }
}
