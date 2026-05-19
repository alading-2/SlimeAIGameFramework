using System;

namespace SlimeAI.GameOS.Capabilities.Feature;

/// <summary>
/// Feature 自动触发模式。
/// </summary>
[Flags]
public enum FeatureTriggerMode
{
    /// <summary>不自动触发。</summary>
    None = 0,

    /// <summary>手动触发。</summary>
    Manual = 1 << 0,

    /// <summary>事件触发。</summary>
    OnEvent = 1 << 1,

    /// <summary>周期触发。</summary>
    Periodic = 1 << 2,

    /// <summary>常驻 Feature。</summary>
    Permanent = 1 << 3
}
