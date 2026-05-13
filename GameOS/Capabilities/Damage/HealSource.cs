namespace SlimeAI.GameOS.Capabilities.Damage;

/// <summary>
/// 治疗来源。
/// </summary>
public enum HealSource
{
    /// <summary>直接治疗。</summary>
    Direct = 0,

    /// <summary>吸血治疗。</summary>
    Lifesteal = 1,

    /// <summary>恢复效果。</summary>
    Regeneration = 2
}
