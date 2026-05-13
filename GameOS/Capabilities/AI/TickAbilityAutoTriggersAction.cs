namespace SlimeAI.GameOS.Capabilities.AI;

/// <summary>
/// 调用 AbilityService 推进自动施法。
/// </summary>
public sealed class TickAbilityAutoTriggersAction : BehaviorNode
{
    /// <summary>
    /// 创建自动施法 Tick 动作。
    /// </summary>
    public TickAbilityAutoTriggersAction() : base("TickAbilityAutoTriggers") { }

    /// <inheritdoc />
    public override AIState Evaluate(AIContext context)
    {
        if (context.AbilityContexts == null || context.AbilityContexts.Count == 0)
        {
            context.LastAbilityReports = null;
            return AIState.Failure;
        }

        var reports = context.AbilityService.TickAutoTriggers(context.AbilityContexts, context.Delta);
        context.LastAbilityReports = reports;
        return reports.Count > 0 ? AIState.Success : AIState.Failure;
    }
}
