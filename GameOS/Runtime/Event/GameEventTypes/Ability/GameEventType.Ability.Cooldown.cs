namespace SkilmeAI.GameOS.Runtime.Event;

/// <summary>
/// Ability 冷却相关事件定义。
/// </summary>
public static partial class GameEventType
{
    /// <summary>
    /// Ability 运行时事件。
    /// </summary>
    public static partial class Ability
    {
        /// <summary>技能冷却完成。</summary>
        public const string Ready = "ability:ready";

        /// <summary>请求启动冷却。</summary>
        public const string StartCooldown = "ability:start_cooldown";

        /// <summary>请求重置冷却。</summary>
        public const string ResetCooldown = "ability:reset_cooldown";

        /// <summary>技能冷却完成事件数据。</summary>
        public readonly record struct ReadyEventData();

        /// <summary>启动冷却事件数据。</summary>
        public readonly record struct StartCooldownEventData();

        /// <summary>重置冷却事件数据。</summary>
        public readonly record struct ResetCooldownEventData();
    }
}
