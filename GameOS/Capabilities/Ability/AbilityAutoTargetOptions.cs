namespace SlimeAI.GameOS.Capabilities.Ability;

/// <summary>
/// Ability 自动索敌参数。
/// </summary>
public sealed class AbilityAutoTargetOptions
{
    /// <summary>查询半径，-1 表示不限制距离。</summary>
    public float Range { get; init; } = -1f;

    /// <summary>最大目标数，-1 表示不限制数量。</summary>
    public int MaxTargets { get; init; } = 1;

    /// <summary>是否过滤同队目标。</summary>
    public bool IgnoreSameTeam { get; init; } = true;

    /// <summary>是否只选择具备 HP Data 的实体。</summary>
    public bool RequiresDamageable { get; init; } = true;

    /// <summary>是否允许选择已死亡目标。</summary>
    public bool IncludeDeadTargets { get; init; }
}
