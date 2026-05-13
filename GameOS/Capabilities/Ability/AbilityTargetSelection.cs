namespace SlimeAI.GameOS.Capabilities.Ability;

/// <summary>
/// 技能目标输入类型。
/// </summary>
public enum AbilityTargetSelection
{
    /// <summary>无目标。</summary>
    None = 0,

    /// <summary>实体目标。</summary>
    Entity = 1,

    /// <summary>点位目标。</summary>
    Point = 2,

    /// <summary>实体或点位目标。</summary>
    EntityOrPoint = 3
}
