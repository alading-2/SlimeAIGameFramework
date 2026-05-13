using System;

namespace SlimeAI.GameOS.Capabilities.Ability;

/// <summary>
/// 技能触发模式。
/// </summary>
[Flags]
public enum AbilityTriggerMode
{
    /// <summary>无触发。</summary>
    None = 0,

    /// <summary>手动触发。</summary>
    Manual = 1 << 0,

    /// <summary>事件触发。</summary>
    OnEvent = 1 << 1,

    /// <summary>周期触发。</summary>
    Periodic = 1 << 2,

    /// <summary>永久生效。</summary>
    Permanent = 1 << 3
}
