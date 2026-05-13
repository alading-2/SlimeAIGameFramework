namespace SlimeAI.GameOS.GodotBridge;

/// <summary>
/// Godot AI bridge 内置行为树类型。
/// </summary>
public enum GodotAIBehaviorTreeKind
{
    /// <summary>攻击优先、追逐其次、巡逻兜底的标准近战树。</summary>
    Melee = 0,

    /// <summary>自动施法优先的近战树。</summary>
    AbilityMelee = 1,

    /// <summary>只巡逻。</summary>
    PatrolOnly = 2,

    /// <summary>只追逐，找不到目标时巡逻。</summary>
    Chaser = 3
}
