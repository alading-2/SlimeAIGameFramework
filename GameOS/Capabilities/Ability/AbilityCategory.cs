namespace SkilmeAI.GameOS.Capabilities.Ability;

/// <summary>
/// Ability Capability DataKey 分类。
/// </summary>
public enum AbilityCategory
{
    /// <summary>基础配置。</summary>
    Basic = 0,

    /// <summary>运行时状态。</summary>
    Runtime = 1,

    /// <summary>冷却。</summary>
    Cooldown = 2,

    /// <summary>充能。</summary>
    Charge = 3,

    /// <summary>效果。</summary>
    Effect = 4,

    /// <summary>目标。</summary>
    Target = 5,

    /// <summary>资源与表现。</summary>
    Visual = 6,

    /// <summary>链式效果。</summary>
    Chain = 7
}
