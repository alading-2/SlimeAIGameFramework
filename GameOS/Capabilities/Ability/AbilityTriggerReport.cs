namespace SlimeAI.GameOS.Capabilities.Ability;

/// <summary>
/// 技能触发报告。
/// </summary>
public readonly record struct AbilityTriggerReport(
    AbilityTriggerResult Result,
    AbilityExecutedResult? Executed,
    string Message)
{
    /// <summary>是否触发成功。</summary>
    public bool Success => Result == AbilityTriggerResult.Success;
}
