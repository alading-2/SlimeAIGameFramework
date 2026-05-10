using SkilmeAI.GameOS.Capabilities.Ability;
using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.Runtime.Event;

/// <summary>
/// Ability 系统/通用事件定义。
/// </summary>
public static partial class GameEventType
{
    /// <summary>
    /// Ability 运行时事件。
    /// </summary>
    public static partial class Ability
    {
        /// <summary>技能被添加到单位。</summary>
        public const string Added = "ability:added";

        /// <summary>技能被移除。</summary>
        public const string Removed = "ability:removed";

        /// <summary>技能升级。</summary>
        public const string LevelUp = "ability:level_up";

        /// <summary>请求检查技能是否可用。</summary>
        public const string CheckCanUse = "ability:check_can_use";

        /// <summary>尝试激活技能。</summary>
        public const string TryTrigger = "ability:try_trigger";

        /// <summary>技能被添加事件数据。</summary>
        public readonly record struct AddedEventData(IEntity Ability, IEntity Owner);

        /// <summary>技能被移除事件数据。</summary>
        public readonly record struct RemovedEventData(string AbilityName, string AbilityId, IEntity Owner);

        /// <summary>技能升级事件数据。</summary>
        public readonly record struct LevelUpEventData(IEntity? Ability, int OldLevel, int NewLevel);

        /// <summary>检查可用性事件数据。</summary>
        public readonly record struct CheckCanUseEventData(EventContext Context);

        /// <summary>尝试激活事件数据。</summary>
        public readonly record struct TryTriggerEventData(AbilityCastContext Context);
    }
}
